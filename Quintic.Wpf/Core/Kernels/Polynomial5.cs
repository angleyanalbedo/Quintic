using Quintic.Wpf.Core.Models;
using System;
using System.Collections.Generic;

namespace Quintic.Wpf.Core.Kernels
{
    public class Polynomial5 : IMotionKernel
    {
        private readonly double _masterStart;
        private readonly double _masterEnd;
        private readonly double _slaveStart;
        private readonly double _slaveEnd;

        public Polynomial5(double masterStart, double masterEnd, double slaveStart, double slaveEnd)
        {
            _masterStart = masterStart;
            _masterEnd = masterEnd;
            _slaveStart = slaveStart;
            _slaveEnd = slaveEnd;
        }

        public CamPoint Calculate(double theta)
        {
            double beta = _masterEnd - _masterStart;
            if (Math.Abs(beta) < 1e-9) return new CamPoint(theta, 0, 0, 0, 0);

            // Normalize theta to tau (0..1)
            double tau = (theta - _masterStart) / beta;

            // Handle out of bounds (though usually caller handles this)
            if (tau < 0) tau = 0;
            if (tau > 1) tau = 1;

            double h = _slaveEnd - _slaveStart;

            double t = tau;
            double t2 = t * t;
            double t3 = t2 * t;
            double t4 = t3 * t;
            double t5 = t4 * t;

            // Displacement (s)
            // s_norm = 10*t^3 - 15*t^4 + 6*t^5
            double s_norm = 10.0 * t3 - 15.0 * t4 + 6.0 * t5;
            double s = _slaveStart + h * s_norm;

            // Velocity (v)
            // v_norm = 30*t^2 - 60*t^3 + 30*t^4
            double v_norm = 30.0 * t2 - 60.0 * t3 + 30.0 * t4;
            double v = (h / beta) * v_norm;

            // Acceleration (a)
            // a_norm = 60*t - 180*t^2 + 120*t^3
            double a_norm = 60.0 * t - 180.0 * t2 + 120.0 * t3;
            double a = (h / (beta * beta)) * a_norm;

            // Jerk (j)
            // j_norm = 60 - 360*t + 360*t^2
            double j_norm = 60.0 - 360.0 * t + 360.0 * t2;
            double j = (h / (beta * beta * beta)) * j_norm;

            return new CamPoint(theta, s, v, a, j);
        }

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
