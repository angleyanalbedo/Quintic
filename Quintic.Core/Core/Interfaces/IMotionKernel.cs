using Quintic.Wpf.Core.Models;
using System.Collections.Generic;

namespace Quintic.Core.Core.Interfaces
{
    public interface IMotionKernel
    {
        CamPoint Calculate(double theta);
        List<CamPoint> GenerateTable(int resolution);
    }
}
