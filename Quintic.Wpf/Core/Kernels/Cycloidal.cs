using Quintic.Wpf.Core.Models;
using System;
using System.Collections.Generic;

namespace Quintic.Wpf.Core.Kernels
{
    public class Cycloidal : IMotionKernel
    {
        private readonly double _masterStart;
        private readonly double _masterEnd;
        private readonly double _slaveStart;
        private readonly double _slaveEnd;

        public Cycloidal(double masterStart, double masterEnd, double slaveStart, double slaveEnd)
        {
            _masterStart = masterStart;
            _masterEnd = masterEnd;
            _slaveStart = slaveStart;
            _slaveEnd = slaveEnd;
        }

        public CamPoint Calculate(double theta)
        {
            double beta = _masterEnd - _masterStart;
            if (Math.Abs(beta) < 1e-9) return new CamPoint(theta, _slaveStart, 0, 0, 0);

            // Normalize theta to tau (0..1)
            double tau = (theta - _masterStart) / beta;

            // Handle out of bounds
            if (tau < 0) tau = 0;
            if (tau > 1) tau = 1;

            double h = _slaveEnd - _slaveStart;
            double TwoPi = 2 * Math.PI;

            // Displacement: s = h * (tau - 1/2pi * sin(2pi*tau))
            double s_norm = tau - (1.0 / TwoPi) * Math.Sin(TwoPi * tau);
            double s = _slaveStart + h * s_norm;

            // Velocity: v = (h/beta) * (1 - cos(2pi*tau))
            double v_norm = 1.0 - Math.Cos(TwoPi * tau);
            double v = (h / beta) * v_norm;

            // Acceleration: a = (h/beta^2) * 2pi * sin(2pi*tau)
            double a_norm = TwoPi * Math.Sin(TwoPi * tau);
            double a = (h / (beta * beta)) * a_norm;

            // Jerk: j = (h/beta^3) * 4pi^2 * cos(2pi*tau)
            double j_norm = (TwoPi * TwoPi) * Math.Cos(TwoPi * tau);
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
                if (i == resolution - 1) theta = _masterEnd;
                points.Add(Calculate(theta));
            }

            return points;
        }
    }
}
