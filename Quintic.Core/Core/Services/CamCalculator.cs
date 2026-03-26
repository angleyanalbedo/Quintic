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
            double currentMaster = 0.0;
            double currentSlave = 0.0;
            double currentV = 0.0; // Geometric Velocity (ds/dtheta)
            double currentA = 0.0; // Geometric Acceleration (d2s/dtheta2)

            // Determine global speed for Time -> Master conversion
            // RPM * 360 / 60 = deg/s
            double speedDegPerSec = (config.MasterVelocity * 360.0) / 60.0;
            if (Math.Abs(config.MasterVelocity) < 1e-9) speedDegPerSec = 0.0;

            foreach (var seg in segments)
            {
                // 1. Resolve Master End Position
                double masterStart = currentMaster;
                double masterEnd = 0.0;

                // --- Reference Logic ---
                if (seg.ReferenceType == ReferenceType.Time)
                {
                    // Duration (seconds) -> Delta Master
                    double durationSec = seg.MasterVal;
                    double deltaMaster = durationSec * speedDegPerSec;
                    masterEnd = masterStart + deltaMaster;
                }
                else if (seg.CoordinateMode == CoordinateMode.Relative)
                {
                    // Delta Master (degrees/units)
                    masterEnd = masterStart + seg.MasterVal;
                }
                else // ABSOLUTE MASTER
                {
                    masterEnd = seg.MasterVal;
                }

                // 2. Resolve Slave End Position
                double slaveStart = currentSlave;
                double slaveEnd = 0.0;

                if (seg.MotionLaw == MotionLawType.Dwell)
                {
                    // Force Dwell to have zero displacement to ensure continuity
                    slaveEnd = slaveStart;
                }
                else if (seg.CoordinateMode == CoordinateMode.Relative)
                {
                    slaveEnd = slaveStart + seg.SlaveVal;
                }
                else // ABSOLUTE SLAVE
                {
                    slaveEnd = seg.SlaveVal;
                }

                // 3. Create Compiled Segment
                var resolvedSeg = seg.Clone();
                
                // Inject computed values
                resolvedSeg.ComputedMasterStart = masterStart;
                resolvedSeg.ComputedMasterEnd = masterEnd;
                resolvedSeg.ComputedSlaveStart = slaveStart;
                resolvedSeg.ComputedSlaveEnd = slaveEnd;

                // --- Continuity Logic ---
                // Inherit start conditions from previous segment
                resolvedSeg.StartVelocity = currentV;
                resolvedSeg.StartAcceleration = currentA;

                // Calculate End Conditions for the next segment
                double duration = masterEnd - masterStart;
                double height = slaveEnd - slaveStart;
                double nextV = 0.0;
                double nextA = 0.0;

                switch (seg.MotionLaw)
                {
                    case MotionLawType.ConstantVelocity:
                        if (Math.Abs(duration) > 1e-9)
                            nextV = height / duration;
                        else
                            nextV = 0;
                        nextA = 0;
                        break;

                    case MotionLawType.Polynomial5:
                        // For Poly5, End V/A are user inputs (or 0 if not set)
                        nextV = seg.EndVelocity;
                        nextA = seg.EndAcceleration;
                        break;

                    case MotionLawType.Polynomial7:
                    case MotionLawType.Cycloidal:
                    case MotionLawType.ModifiedTrapezoid:
                    case MotionLawType.ModifiedSine:
                    case MotionLawType.SevenSegment:
                    case MotionLawType.Gutman:
                    case MotionLawType.Dwell:
                    default:
                        // Standard laws typically end at rest (V=0, A=0)
                        nextV = 0;
                        nextA = 0;
                        break;

                    case MotionLawType.SimpleSine:
                        nextV = 0;
                        // Simple Sine ends with non-zero acceleration: a_end = -h * pi^2 / (2 * beta^2)
                        if (Math.Abs(duration) > 1e-9)
                        {
                            double beta = duration;
                            nextA = -height * Math.Pow(Math.PI, 2) / (2 * beta * beta);
                        }
                        else
                        {
                            nextA = 0;
                        }
                        break;

                    case MotionLawType.BSpline:
                        // BSpline (Cubic) ends with specific V/A
                        // V is user defined (EndVelocity)
                        nextV = seg.EndVelocity;
                        // A is calculated from Cubic formula at t=1
                        // a_tau(1) = 6*s0 + 2*m0 - 6*s1 + 4*m1
                        // where s0=0, s1=height, m0=vStart*beta, m1=vEnd*beta
                        {
                            double beta = duration;
                            if (Math.Abs(beta) > 1e-9)
                            {
                                double m0 = seg.StartVelocity * beta;
                                double m1 = seg.EndVelocity * beta;
                                double s0 = 0; 
                                double s1 = height;
                                double a_tau_end = 6 * s0 + 2 * m0 - 6 * s1 + 4 * m1;
                                nextA = a_tau_end / (beta * beta);
                            }
                            else
                            {
                                nextA = 0;
                            }
                        }
                        break;
                }

                // Store calculated end conditions back into the resolved segment
                // (Important for Poly5 if we want to use them, and for debugging)
                resolvedSeg.EndVelocity = nextV;
                resolvedSeg.EndAcceleration = nextA;

                resolvedSegments.Add(resolvedSeg);

                // Update state for next iteration
                currentMaster = masterEnd;
                currentSlave = slaveEnd;
                currentV = nextV;
                currentA = nextA;
            }

            return resolvedSegments;
        }

        public static CalculationResponse CalculateProject(List<Segment> segments, ProjectConfig config)
        {
            // Step 1: Compile / Resolve Coordinates
            var compiledSegments = ResolveCoordinates(segments, config);

            var fullProfile = new List<CamPoint>();
            
            // Return empty response if no segments
            if (compiledSegments.Count == 0)
            {
                return new CalculationResponse();
            }

            // Total Master Duration (Computed)
            double startMaster = compiledSegments[0].ComputedMasterStart ?? 0.0;
            double endMaster = compiledSegments[compiledSegments.Count - 1].ComputedMasterEnd ?? 0.0;
            double totalMaster = endMaster - startMaster;

            int resolution = config.Resolution;

            // Step 2: Compute Curves
            for (int i = 0; i < compiledSegments.Count; i++)
            {
                var segment = compiledSegments[i];
                
                double mStart = segment.ComputedMasterStart ?? 0.0;
                double mEnd = segment.ComputedMasterEnd ?? 0.0;
                double sStart = segment.ComputedSlaveStart ?? 0.0;
                double sEnd = segment.ComputedSlaveEnd ?? 0.0;

                // 2.1 Local Resolution Allocation
                double segmentMaster = mEnd - mStart;
                int segRes = 2;
                if (totalMaster > 0)
                {
                    segRes = Math.Max(2, (int)(resolution * (segmentMaster / totalMaster)));
                }

                // 2.2 Kernel Execution (Factory Pattern)
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
                        kernel = new BSpline(mStart, mEnd, sStart, sEnd, segment.StartVelocity, segment.EndVelocity, segment.ControlPoints);
                        break;
                    case MotionLawType.ModifiedSine:
                    case MotionLawType.ModifiedTrapezoid:
                        kernel = new ModifiedTrapezoid(mStart, mEnd, sStart, sEnd);
                        break;
                    case MotionLawType.Polynomial7:
                        kernel = new Polynomial7(mStart, mEnd, sStart, sEnd);
                        break;
                    case MotionLawType.ConstantVelocity:
                        kernel = new ConstantVelocity(mStart, mEnd, sStart, sEnd);
                        break;
                    case MotionLawType.Dwell:
                        kernel = new Dwell(mStart, mEnd, sStart, sEnd);
                        break;
                    case MotionLawType.Polynomial5:
                    default:
                        // Default to Poly5 with boundary conditions
                        kernel = new Polynomial5(mStart, mEnd, sStart, sEnd, 
                                                 segment.StartVelocity, segment.EndVelocity,
                                                 segment.StartAcceleration, segment.EndAcceleration);
                        break;
                }

                var segmentData = kernel.GenerateTable(segRes);

                // 2.3 Stitching (Remove last point of current segment to avoid duplicate points at boundaries)
                if (i < compiledSegments.Count - 1 && segmentData.Count > 0)
                {
                    segmentData.RemoveAt(segmentData.Count - 1);
                }

                fullProfile.AddRange(segmentData);
            }

            // Step 3: Analyze Characteristics
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
    }
}
