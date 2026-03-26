using Quintic.Wpf.Core.Models;
using System.Collections.Generic;

namespace Quintic.Wpf.Core.Kernels
{
    public class Dwell : IMotionKernel
    {
        private readonly double _masterStart;
        private readonly double _masterEnd;
        private readonly double _slaveStart;
        
        public Dwell(double masterStart, double masterEnd, double slaveStart)
        {
            _masterStart = masterStart;
            _masterEnd = masterEnd;
            _slaveStart = slaveStart;
        }

        public CamPoint Calculate(double theta)
        {
            // Dwell means s is constant, v=0, a=0, j=0
            return new CamPoint(theta, _slaveStart, 0, 0, 0);
        }

        public List<CamPoint> GenerateTable(int resolution)
        {
            var points = new List<CamPoint>(resolution);
            if (resolution < 2) return points;

            double step = (_masterEnd - _masterStart) / (resolution - 1);
            
            for (int i = 0; i < resolution; i++)
            {
                double theta = _masterStart + i * step;
                if (i == resolution - 1) theta = _masterEnd;
                points.Add(Calculate(theta));
            }

            return points;
        }
    }
}
