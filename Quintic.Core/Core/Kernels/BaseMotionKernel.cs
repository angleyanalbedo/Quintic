using Quintic.Wpf.Core.Models;
using System.Collections.Generic;

namespace Quintic.Wpf.Core.Kernels
{
    public abstract class BaseMotionKernel : IMotionKernel
    {
        protected readonly double _masterStart;
        protected readonly double _masterEnd;

        protected BaseMotionKernel(double masterStart, double masterEnd)
        {
            _masterStart = masterStart;
            _masterEnd = masterEnd;
        }

        public abstract CamPoint Calculate(double theta);

        public List<CamPoint> GenerateTable(int resolution)
        {
            var points = new List<CamPoint>(resolution);
            if (resolution < 2) return points;

            double step = (_masterEnd - _masterStart) / (resolution - 1);

            for (int i = 0; i < resolution; i++)
            {
                double theta = _masterStart + i * step;
                // Ensure last point is exactly end
                if (i == resolution - 1) theta = _masterEnd;

                points.Add(Calculate(theta));
            }

            return points;
        }
    }
}
