using Quintic.Wpf.Core.Interfaces;
using Quintic.Wpf.Core.Kernels.Base;
using Quintic.Wpf.Core.Models;
using System.Collections.Generic;

namespace Quintic.Wpf.Core.Kernels
{
    public class Dwell : BaseMotionKernel
    {
        private readonly double _slaveStart;
        
        public Dwell(double masterStart, double masterEnd, double slaveStart)
            : base(masterStart, masterEnd)
        {
            _slaveStart = slaveStart;
        }

        public override CamPoint Calculate(double theta)
        {
            // Dwell means s is constant, v=0, a=0, j=0
            return new CamPoint(theta, _slaveStart, 0, 0, 0);
        }

    }
}
