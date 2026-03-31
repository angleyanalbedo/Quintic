using Quintic.Wpf.Core.Interfaces;
using Quintic.Wpf.Core.Kernels;
using Quintic.Wpf.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quintic.Wpf.Core.Services
{
    public class CamCalculator
    {
        public static List<Segment> ResolveCoordinates(List<Segment> segments, ProjectConfig config)
        {
            var resolvedSegments = new List<Segment>();
            if (segments == null || segments.Count == 0) return resolvedSegments;

            double speedDegPerSec = (config.MasterVelocity * 360.0) / 60.0;
            if (Math.Abs(config.MasterVelocity) < 1e-9) speedDegPerSec = 0.0;

            // Pass 1: Resolve Absolute Coordinates (Master & Slave)
            double currentMaster = 0.0;
            double currentSlave = 0.0;

            foreach (var seg in segments)
            {
                double masterStart = currentMaster;
                double masterEnd = 0.0;

                if (seg.ReferenceType == ReferenceType.Time)
                {
                    double durationSec = seg.MasterVal;
                    masterEnd = masterStart + (durationSec * speedDegPerSec);
                }
                else if (seg.CoordinateMode == CoordinateMode.Relative)
                {
                    masterEnd = masterStart + seg.MasterVal;
                }
                else
                {
                    masterEnd = seg.MasterVal;
                }

                double slaveStart = currentSlave;
                double slaveEnd = 0.0;

                if (seg.MotionLaw == MotionLawType.Dwell)
                {
                    slaveEnd = slaveStart;
                }
                else if (seg.CoordinateMode == CoordinateMode.Relative)
                {
                    slaveEnd = slaveStart + seg.SlaveVal;
                }
                else
                {
                    slaveEnd = seg.SlaveVal;
                }

                var resolvedSeg = seg.Clone();
                resolvedSeg.ComputedMasterStart = masterStart;
                resolvedSeg.ComputedMasterEnd = masterEnd;
                resolvedSeg.ComputedSlaveStart = slaveStart;
                resolvedSeg.ComputedSlaveEnd = slaveEnd;

                resolvedSegments.Add(resolvedSeg);

                currentMaster = masterEnd;
                currentSlave = slaveEnd;
            }

            // Pass 2: Global Boundary Value Solver (Forward & Backward Sweep for C3 Continuity)
            // Initialize boundaries
            for (int i = 0; i < resolvedSegments.Count; i++)
            {
                var seg = resolvedSegments[i];
                double duration = (seg.ComputedMasterEnd ?? 0) - (seg.ComputedMasterStart ?? 0);
                double height = (seg.ComputedSlaveEnd ?? 0) - (seg.ComputedSlaveStart ?? 0);

                // Pre-calculate rigid boundary conditions (Dwell, Constant Velocity)
                if (seg.MotionLaw == MotionLawType.Dwell)
                {
                    seg.StartVelocity = 0; seg.EndVelocity = 0;
                    seg.StartAcceleration = 0; seg.EndAcceleration = 0;
                    seg.StartJerk = 0; seg.EndJerk = 0;
                }
                else if (seg.MotionLaw == MotionLawType.ConstantVelocity)
                {
                    double v = Math.Abs(duration) > 1e-9 ? height / duration : 0;
                    seg.StartVelocity = v; seg.EndVelocity = v;
                    seg.StartAcceleration = 0; seg.EndAcceleration = 0;
                    seg.StartJerk = 0; seg.EndJerk = 0;
                }
            }

            // Forward Sweep: Propagate V, A, and J
            double currentV = 0.0;
            double currentA = 0.0;
            double currentJ = 0.0;
            for (int i = 0; i < resolvedSegments.Count; i++)
            {
                var seg = resolvedSegments[i];
                
                // Inherit from previous unless it's a rigid segment that already defined its start
                if (seg.MotionLaw != MotionLawType.Dwell && seg.MotionLaw != MotionLawType.ConstantVelocity)
                {
                    seg.StartVelocity = currentV;
                    seg.StartAcceleration = currentA;
                    seg.StartJerk = currentJ;
                }
                else
                {
                    currentV = seg.StartVelocity;
                    currentA = seg.StartAcceleration;
                    currentJ = seg.StartJerk;
                }

                // Estimate End Conditions for flexible laws
                if (seg.MotionLaw == MotionLawType.Polynomial5 || seg.MotionLaw == MotionLawType.Polynomial7 || seg.MotionLaw == MotionLawType.BSpline)
                {
                    // Use user defined or keep current
                    currentV = seg.EndVelocity;
                    currentA = seg.EndAcceleration;
                    currentJ = seg.EndJerk;
                }
                else if (seg.MotionLaw != MotionLawType.Dwell && seg.MotionLaw != MotionLawType.ConstantVelocity)
                {
                    // For other laws, we aim for C3 continuity by leaving them open for the backward sweep,
                    // but default to 0 if it's the last segment.
                    currentV = 0;
                    currentA = 0;
                    currentJ = 0;
                    seg.EndVelocity = currentV;
                    seg.EndAcceleration = currentA;
                    seg.EndJerk = currentJ;
                }
            }

            // Backward Sweep: Smooth out transitions (Simplified BVP)
            for (int i = resolvedSegments.Count - 2; i >= 0; i--)
            {
                var currentSeg = resolvedSegments[i];
                var nextSeg = resolvedSegments[i + 1];

                // Force end conditions of current segment to match start conditions of next segment
                currentSeg.EndVelocity = nextSeg.StartVelocity;
                currentSeg.EndAcceleration = nextSeg.StartAcceleration;
                currentSeg.EndJerk = nextSeg.StartJerk;
            }

            return resolvedSegments;
        }

        public static CalculationResponse CalculateProject(List<Segment> segments, ProjectConfig config)
        {
            var compiledSegments = ResolveCoordinates(segments, config);
            var fullProfile = new List<CamPoint>();
            
            if (compiledSegments.Count == 0)
            {
                return new CalculationResponse();
            }

            double startMaster = compiledSegments[0].ComputedMasterStart ?? 0.0;
            double endMaster = compiledSegments[compiledSegments.Count - 1].ComputedMasterEnd ?? 0.0;
            double totalMaster = endMaster - startMaster;

            int baseResolution = config.Resolution;

            for (int i = 0; i < compiledSegments.Count; i++)
            {
                var segment = compiledSegments[i];
                
                double mStart = segment.ComputedMasterStart ?? 0.0;
                double mEnd = segment.ComputedMasterEnd ?? 0.0;
                double sStart = segment.ComputedSlaveStart ?? 0.0;
                double sEnd = segment.ComputedSlaveEnd ?? 0.0;

                IMotionKernel kernel = null;
                switch (segment.MotionLaw)
                {
                    case MotionLawType.Cycloidal:
                        kernel = new Cycloidal(mStart, mEnd, sStart, sEnd);
                        break;
                    case MotionLawType.SimpleSine:
                        kernel = new SimpleSine(mStart, mEnd, sStart, sEnd);
                        break;
                    case MotionLawType.SevenSegment:
                        kernel = new SevenSegment(mStart, mEnd, sStart, sEnd);
                        break;
                    case MotionLawType.Gutman:
                        kernel = new Gutman(mStart, mEnd, sStart, sEnd);
                        break;
                    case MotionLawType.BSpline:
                        kernel = new BSpline(mStart, mEnd, sStart, sEnd, 
                                             segment.StartVelocity, segment.EndVelocity, 
                                             segment.StartAcceleration, segment.EndAcceleration, 
                                             segment.ControlPoints);
                        break;
                    case MotionLawType.ModifiedSine:
                    case MotionLawType.ModifiedTrapezoid:
                        kernel = new ModifiedTrapezoid(mStart, mEnd, sStart, sEnd);
                        break;
                    case MotionLawType.Polynomial7:
                        kernel = new Polynomial7(mStart, mEnd, sStart, sEnd, 
                                                 segment.StartVelocity, segment.EndVelocity, 
                                                 segment.StartAcceleration, segment.EndAcceleration,
                                                 segment.StartJerk, segment.EndJerk);
                        break;
                    case MotionLawType.ConstantVelocity:
                        kernel = new ConstantVelocity(mStart, mEnd, sStart, sEnd);
                        break;
                    case MotionLawType.Dwell:
                        kernel = new Dwell(mStart, mEnd, sStart, sEnd);
                        break;
                    case MotionLawType.Polynomial5:
                    default:
                        kernel = new Polynomial5(mStart, mEnd, sStart, sEnd, 
                                                 segment.StartVelocity, segment.EndVelocity,
                                                 segment.StartAcceleration, segment.EndAcceleration);
                        break;
                }

                // Adaptive Sampling: Generate points dynamically based on curvature
                double segmentMaster = mEnd - mStart;
                int minPoints = Math.Max(10, (int)(baseResolution * (segmentMaster / totalMaster)));
                var segmentData = GenerateAdaptiveTable(kernel, mStart, mEnd, minPoints);

                if (i < compiledSegments.Count - 1 && segmentData.Count > 0)
                {
                    segmentData.RemoveAt(segmentData.Count - 1);
                }

                fullProfile.AddRange(segmentData);
            }

            double maxV = 0.0;
            double maxA = 0.0;
            double maxJ = 0.0;

            if (fullProfile.Count > 0)
            {
                maxV = fullProfile.Max(p => Math.Abs(p.V));
                maxA = fullProfile.Max(p => Math.Abs(p.A));
                maxJ = fullProfile.Max(p => Math.Abs(p.J));
            }

            return new CalculationResponse
            {
                Points = fullProfile,
                MaxVelocity = maxV,
                MaxAcceleration = maxA,
                MaxJerk = maxJ
            };
        }

        /// <summary>
        /// Adaptive Sampling Engine: Dynamically subdivides intervals where acceleration changes rapidly.
        /// </summary>
        private static List<CamPoint> GenerateAdaptiveTable(IMotionKernel kernel, double mStart, double mEnd, int minPoints)
        {
            var points = new List<CamPoint>();
            if (Math.Abs(mEnd - mStart) < 1e-9) return points;

            double step = (mEnd - mStart) / (minPoints - 1);
            double currentTheta = mStart;

            CamPoint prevPoint = kernel.Calculate(currentTheta);
            points.Add(prevPoint);

            for (int i = 1; i < minPoints; i++)
            {
                double nextTheta = mStart + i * step;
                if (i == minPoints - 1) nextTheta = mEnd;

                CamPoint nextPoint = kernel.Calculate(nextTheta);

                // Check curvature (difference in Acceleration)
                double aDiff = Math.Abs(nextPoint.A - prevPoint.A);
                
                // If acceleration changes too much, inject a midpoint (Adaptive Subdivision)
                if (aDiff > 5.0) // Threshold can be tuned or made configurable
                {
                    double midTheta = (currentTheta + nextTheta) / 2.0;
                    points.Add(kernel.Calculate(midTheta));
                }

                points.Add(nextPoint);
                prevPoint = nextPoint;
                currentTheta = nextTheta;
            }

            return points;
        }
    }
}
