using Quintic.Core.Kernels.Base;
using Quintic.Wpf.Core.Models;
using System;
using System.Collections.Generic;

namespace Quintic.Wpf.Core.Kernels
{
    public class ModifiedTrapezoid : BaseMotionKernel
    {
        private readonly double _slaveStart;
        private readonly double _slaveEnd;

        public ModifiedTrapezoid(double masterStart, double masterEnd, double slaveStart, double slaveEnd)
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

            // Handle out of bounds
            if (tau < 0) tau = 0;
            if (tau > 1) tau = 1;

            double h = _slaveEnd - _slaveStart;

            double t = tau;
            double t2 = t * t;
            double t3 = t2 * t;
            double t4 = t3 * t;
            double t5 = t4 * t;
            double t6 = t5 * t;
            double t7 = t6 * t;

            // Polynomial 4-5-6-7 (Rest-in-Rest, Zero Jerk at Ends)
            // s = 35t^4 - 84t^5 + 70t^6 - 20t^7
            double s_norm = 35.0 * t4 - 84.0 * t5 + 70.0 * t6 - 20.0 * t7;
            double s = _slaveStart + h * s_norm;

            // v = 140t^3 - 420t^4 + 420t^5 - 140t^6
            double v_norm = 140.0 * t3 - 420.0 * t4 + 420.0 * t5 - 140.0 * t6;
            double v = (h / beta) * v_norm;

            // a = 420t^2 - 1680t^3 + 2100t^4 - 840t^5
            double a_norm = 420.0 * t2 - 1680.0 * t3 + 2100.0 * t4 - 840.0 * t5;
            double a = (h / (beta * beta)) * a_norm;

            // j = 840t - 5040t^2 + 8400t^3 - 4200t^4
            double j_norm = 840.0 * t - 5040.0 * t2 + 8400.0 * t3 - 4200.0 * t4;
            double j = (h / (beta * beta * beta)) * j_norm;

            return new CamPoint(theta, s, v, a, j);
        }

    }
}
