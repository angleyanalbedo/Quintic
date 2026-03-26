using Quintic.Wpf.Core.Kernels.Base;
using Quintic.Wpf.Core.Models;
using System;

namespace Quintic.Wpf.Core.Kernels
{
    public class SimpleSine : BaseMotionKernel
    {
        private readonly double _slaveStart;
        private readonly double _slaveEnd;

        public SimpleSine(double masterStart, double masterEnd, double slaveStart, double slaveEnd)
            : base(masterStart, masterEnd)
        {
            _slaveStart = slaveStart;
            _slaveEnd = slaveEnd;
        }

        public override CamPoint Calculate(double theta)
        {
            double beta = _masterEnd - _masterStart;
            if (Math.Abs(beta) < 1e-9) return new CamPoint(theta, _slaveStart, 0, 0, 0);

            // Normalize theta to tau (0..1)
            double tau = (theta - _masterStart) / beta;

            if (tau < 0) tau = 0;
            if (tau > 1) tau = 1;

            double h = _slaveEnd - _slaveStart;
            
            // Simple Sine (Harmonic)
            // s = h * (1 - cos(pi * tau)) / 2
            double piTau = Math.PI * tau;
            double cosPiTau = Math.Cos(piTau);
            
            double s_norm = (1.0 - cosPiTau) / 2.0;
            double s = _slaveStart + h * s_norm;

            // v = h * pi/2 * sin(pi * tau) / beta
            double v_norm = (Math.PI / 2.0) * Math.Sin(piTau);
            double v = (h / beta) * v_norm;

            // a = h * pi^2/2 * cos(pi * tau) / beta^2
            double a_norm = (Math.PI * Math.PI / 2.0) * cosPiTau;
            double a = (h / (beta * beta)) * a_norm;

            // j = -h * pi^3/2 * sin(pi * tau) / beta^3
            double j_norm = -(Math.Pow(Math.PI, 3) / 2.0) * Math.Sin(piTau);
            double j = (h / Math.Pow(beta, 3)) * j_norm;

            return new CamPoint(theta, s, v, a, j);
        }
    }
}
