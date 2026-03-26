using Quintic.Wpf.Core.Kernels.Base;
using Quintic.Wpf.Core.Models;
using System;

namespace Quintic.Wpf.Core.Kernels
{
    public class Gutman : BaseMotionKernel
    {
        private readonly double _slaveStart;
        private readonly double _slaveEnd;

        public Gutman(double masterStart, double masterEnd, double slaveStart, double slaveEnd)
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

            // Gutman (Freydenstein) 1-3
            // s(tau) = tau - (15/32pi)*sin(2pi*tau) - (1/96pi)*sin(6pi*tau)
            
            double pi = Math.PI;
            double twoPiTau = 2 * pi * tau;
            double sixPiTau = 6 * pi * tau;

            double c1 = 15.0 / (32.0 * pi);
            double c2 = 1.0 / (96.0 * pi);

            // Displacement
            double s_norm = tau - c1 * Math.Sin(twoPiTau) - c2 * Math.Sin(sixPiTau);
            double s = _slaveStart + h * s_norm;

            // Velocity (ds/dtau)
            // v_norm = 1 - (15/16)*cos(2pi*tau) - (1/16)*cos(6pi*tau)
            double v_norm = 1.0 - (15.0 / 16.0) * Math.Cos(twoPiTau) - (1.0 / 16.0) * Math.Cos(sixPiTau);
            double v = (h / beta) * v_norm;

            // Acceleration (d2s/dtau2)
            // a_norm = (15pi/8)*sin(2pi*tau) + (3pi/8)*sin(6pi*tau)
            double a_norm = (15.0 * pi / 8.0) * Math.Sin(twoPiTau) + (3.0 * pi / 8.0) * Math.Sin(sixPiTau);
            double a = (h / (beta * beta)) * a_norm;

            // Jerk (d3s/dtau3)
            // j_norm = (15pi^2/4)*cos(2pi*tau) + (9pi^2/4)*cos(6pi*tau)
            double j_norm = (15.0 * pi * pi / 4.0) * Math.Cos(twoPiTau) + (9.0 * pi * pi / 4.0) * Math.Cos(sixPiTau);
            double j = (h / Math.Pow(beta, 3)) * j_norm;

            return new CamPoint(theta, s, v, a, j);
        }
    }
}
