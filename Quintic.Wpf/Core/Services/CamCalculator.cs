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

                if (seg.CoordinateMode == CoordinateMode.Relative)
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

                resolvedSegments.Add(resolvedSeg);

                // Update state for next iteration
                currentMaster = masterEnd;
                currentSlave = slaveEnd;
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
                        // kernel = new Cycloidal(mStart, mEnd, sStart, sEnd);
                        kernel = new Polynomial5(mStart, mEnd, sStart, sEnd); // Fallback
                        break;
                    case MotionLawType.ModifiedSine:
                         // kernel = new ModifiedTrapezoid(mStart, mEnd, sStart, sEnd);
                         kernel = new Polynomial5(mStart, mEnd, sStart, sEnd); // Fallback
                        break;
                    default:
                        // Default to Poly5
                        kernel = new Polynomial5(mStart, mEnd, sStart, sEnd);
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
