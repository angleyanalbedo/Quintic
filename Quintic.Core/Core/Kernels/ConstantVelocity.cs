using Quintic.Core.Kernels.Base;
using Quintic.Wpf.Core.Models;
using System.Collections.Generic;

namespace Quintic.Wpf.Core.Kernels
{
    public class ConstantVelocity : BaseMotionKernel
    {
        private readonly double _slaveStart;
        private readonly double _slaveEnd;

        public ConstantVelocity(double masterStart, double masterEnd, double slaveStart, double slaveEnd)
            : base(masterStart, masterEnd)
        {
            _slaveStart = slaveStart;
            _slaveEnd = slaveEnd;
        }

        public override CamPoint Calculate(double theta)
        {
            double beta = _masterEnd - _masterStart;
            if (System.Math.Abs(beta) < 1e-9) return new CamPoint(theta, _slaveStart, 0, 0, 0);

            // Normalize theta to tau (0..1)
            double tau = (theta - _masterStart) / beta;

            if (tau < 0) tau = 0;
            if (tau > 1) tau = 1;

            double h = _slaveEnd - _slaveStart;

            // s = h * tau
            double s = _slaveStart + h * tau;

            // v = h / beta
            double v = h / beta;

            // a = 0
            double a = 0;

            // j = 0
            double j = 0;

            return new CamPoint(theta, s, v, a, j);
        }

    }
}
