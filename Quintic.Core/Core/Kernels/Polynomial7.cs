using Quintic.Wpf.Core.Kernels.Base;
using Quintic.Wpf.Core.Models;
using System;

namespace Quintic.Wpf.Core.Kernels
{
    public class Polynomial7 : BaseMotionKernel
    {
        private readonly double _slaveStart;
        private readonly double _slaveEnd;
        private readonly double _beta;

        // Polynomial coefficients
        private readonly double _c0, _c1, _c2, _c3, _c4, _c5, _c6, _c7;

        public Polynomial7(double masterStart, double masterEnd, double slaveStart, double slaveEnd,
                           double v0 = 0, double v1 = 0, double a0 = 0, double a1 = 0, double j0 = 0, double j1 = 0)
            : base(masterStart, masterEnd)
        {
            _slaveStart = slaveStart;
            _slaveEnd = slaveEnd;
            _beta = masterEnd - masterStart;

            if (Math.Abs(_beta) < 1e-9)
            {
                _c0 = slaveStart;
                return;
            }

            // Calculate coefficients for normalized time tau (0 to 1)
            // S(tau) = c0 + c1*tau + c2*tau^2 + c3*tau^3 + c4*tau^4 + c5*tau^5 + c6*tau^6 + c7*tau^7
            
            _c0 = slaveStart;
            _c1 = v0 * _beta;
            _c2 = 0.5 * a0 * _beta * _beta;
            _c3 = (1.0 / 6.0) * j0 * Math.Pow(_beta, 3);

            // Deltas for the remaining system of equations
            double deltaS = slaveEnd - (_c0 + _c1 + _c2 + _c3);
            double deltaV = v1 * _beta - (_c1 + 2 * _c2 + 3 * _c3);
            double deltaA = a1 * _beta * _beta - (2 * _c2 + 6 * _c3);
            double deltaJ = j1 * Math.Pow(_beta, 3) - 6 * _c3;

            // Solve the 4x4 matrix for c4, c5, c6, c7
            _c4 = 35 * deltaS - 15 * deltaV + 2.5 * deltaA - (1.0 / 6.0) * deltaJ;
            _c5 = -84 * deltaS + 39 * deltaV - 7 * deltaA + 0.5 * deltaJ;
            _c6 = 70 * deltaS - 34 * deltaV + 6.5 * deltaA - 0.5 * deltaJ;
            _c7 = -20 * deltaS + 10 * deltaV - 2 * deltaA + (1.0 / 6.0) * deltaJ;
        }

        public override CamPoint Calculate(double theta)
        {
            if (Math.Abs(_beta) < 1e-9) return new CamPoint(theta, _slaveStart, 0, 0, 0);

            // Normalize theta to tau (0..1)
            double tau = (theta - _masterStart) / _beta;

            if (tau < 0) tau = 0;
            if (tau > 1) tau = 1;

            double t = tau;
            double t2 = t * t;
            double t3 = t2 * t;
            double t4 = t3 * t;
            double t5 = t4 * t;
            double t6 = t5 * t;
            double t7 = t6 * t;

            // Position
            double s = _c0 + _c1 * t + _c2 * t2 + _c3 * t3 + _c4 * t4 + _c5 * t5 + _c6 * t6 + _c7 * t7;

            // Velocity (ds/dtheta = ds/dtau * dtau/dtheta = ds/dtau / beta)
            double ds_dtau = _c1 + 2 * _c2 * t + 3 * _c3 * t2 + 4 * _c4 * t3 + 5 * _c5 * t4 + 6 * _c6 * t5 + 7 * _c7 * t6;
            double v = ds_dtau / _beta;

            // Acceleration
            double d2s_dtau2 = 2 * _c2 + 6 * _c3 * t + 12 * _c4 * t2 + 20 * _c5 * t3 + 30 * _c6 * t4 + 42 * _c7 * t5;
            double a = d2s_dtau2 / (_beta * _beta);

            // Jerk
            double d3s_dtau3 = 6 * _c3 + 24 * _c4 * t + 60 * _c5 * t2 + 120 * _c6 * t3 + 210 * _c7 * t4;
            double j = d3s_dtau3 / Math.Pow(_beta, 3);

            return new CamPoint(theta, s, v, a, j);
        }
    }
}
