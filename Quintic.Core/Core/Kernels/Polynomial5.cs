using Quintic.Wpf.Core.Interfaces;
using Quintic.Wpf.Core.Kernels.Base;
using Quintic.Wpf.Core.Models;
using System;
using System.Collections.Generic;

namespace Quintic.Wpf.Core.Kernels
{
    public class Polynomial5 : BaseMotionKernel
    {
        private readonly double _slaveStart;
        private readonly double _slaveEnd;
        
        // Boundary Conditions
        private readonly double _vStart;
        private readonly double _vEnd;
        private readonly double _aStart;
        private readonly double _aEnd;

        // Coefficients
        private double C0, C1, C2, C3, C4, C5;

        public Polynomial5(double masterStart, double masterEnd, double slaveStart, double slaveEnd,
                           double vStart = 0, double vEnd = 0, double aStart = 0, double aEnd = 0)
            : base(masterStart, masterEnd)
        {
            _slaveStart = slaveStart;
            _slaveEnd = slaveEnd;

            _vStart = vStart;
            _vEnd = vEnd;
            _aStart = aStart;
            _aEnd = aEnd;

            CalculateCoefficients();
        }

        private void CalculateCoefficients()
        {
            double beta = _masterEnd - _masterStart;
            double h = _slaveEnd - _slaveStart;

            if (Math.Abs(beta) < 1e-9)
            {
                C0 = _slaveStart;
                return;
            }

            // Normalize derivatives to tau domain (0..1)
            // s'(tau) = v * beta
            // s''(tau) = a * beta^2
            double s0 = 0; // Relative to start
            double s1 = h;
            double v0 = _vStart * beta;
            double v1 = _vEnd * beta;
            double a0 = _aStart * beta * beta;
            double a1 = _aEnd * beta * beta;

            // General 5th Order Polynomial Coefficients
            // s(tau) = C0 + C1*t + C2*t^2 + C3*t^3 + C4*t^4 + C5*t^5
            
            C0 = s0;
            C1 = v0;
            C2 = 0.5 * a0;

            // Solve for C3, C4, C5 using boundary conditions at tau=1
            // System of equations derived from s(1), v(1), a(1)
            C3 = 10 * (s1 - s0) - 6 * v0 - 1.5 * a0 - 4 * v1 + 0.5 * a1;
            C4 = -15 * (s1 - s0) + 8 * v0 + 1.5 * a0 + 7 * v1 - a1;
            C5 = 6 * (s1 - s0) - 3 * v0 - 0.5 * a0 - 3 * v1 + 0.5 * a1;
        }

        public override CamPoint Calculate(double theta)
        {
            double beta = _masterEnd - _masterStart;
            if (Math.Abs(beta) < 1e-9) return new CamPoint(theta, _slaveStart, 0, 0, 0);

            // Normalize theta to tau (0..1)
            double t = (theta - _masterStart) / beta;

            // Handle out of bounds
            if (t < 0) t = 0;
            if (t > 1) t = 1;

            double t2 = t * t;
            double t3 = t2 * t;
            double t4 = t3 * t;
            double t5 = t4 * t;

            // Displacement (s) relative to start
            double s_rel = C0 + C1 * t + C2 * t2 + C3 * t3 + C4 * t4 + C5 * t5;
            double s = _slaveStart + s_rel;

            // Velocity (v)
            // ds/dt = (ds/dtau) * (dtau/dt) = P'(tau) * (1/beta)
            double v_tau = C1 + 2 * C2 * t + 3 * C3 * t2 + 4 * C4 * t3 + 5 * C5 * t4;
            double v = v_tau / beta;

            // Acceleration (a)
            // d2s/dt2 = P''(tau) * (1/beta^2)
            double a_tau = 2 * C2 + 6 * C3 * t + 12 * C4 * t2 + 20 * C5 * t3;
            double a = a_tau / (beta * beta);

            // Jerk (j)
            // d3s/dt3 = P'''(tau) * (1/beta^3)
            double j_tau = 6 * C3 + 24 * C4 * t + 60 * C5 * t2;
            double j = j_tau / (beta * beta * beta);

            return new CamPoint(theta, s, v, a, j);
        }

    }
}
