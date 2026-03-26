using Quintic.Wpf.Core.Kernels.Base;
using Quintic.Wpf.Core.Models;
using System;
using System.Collections.Generic;

namespace Quintic.Wpf.Core.Kernels
{
    /// <summary>
    /// Implements a Cubic Spline (Polynomial 3rd Order).
    /// Provides C1 continuity (Velocity).
    /// s(t) = a*t^3 + b*t^2 + c*t + d
    /// </summary>
    public class BSpline : BaseMotionKernel
    {
        private readonly double _slaveStart;
        private readonly double _slaveEnd;
        private readonly double _vStart;
        private readonly double _vEnd;
        private readonly List<CamPoint> _controlPoints;

        public BSpline(double masterStart, double masterEnd, double slaveStart, double slaveEnd, 
                       double vStart = 0, double vEnd = 0, List<CamPoint> controlPoints = null)
            : base(masterStart, masterEnd)
        {
            _slaveStart = slaveStart;
            _slaveEnd = slaveEnd;
            _vStart = vStart;
            _vEnd = vEnd;
            _controlPoints = controlPoints ?? new List<CamPoint>();
        }

        public override CamPoint Calculate(double theta)
        {
            double beta = _masterEnd - _masterStart;
            if (Math.Abs(beta) < 1e-9) return new CamPoint(theta, _slaveStart, 0, 0, 0);

            double tau = (theta - _masterStart) / beta;
            if (tau < 0) tau = 0;
            if (tau > 1) tau = 1;

            // Cubic Hermite Spline
            // P(t) = (2t^3 - 3t^2 + 1)P0 + (t^3 - 2t^2 + t)m0 + (-2t^3 + 3t^2)P1 + (t^3 - t^2)m1
            // m0 = vStart * beta
            // m1 = vEnd * beta
            
            double t = tau;
            double t2 = t * t;
            double t3 = t2 * t;

            double h00 = 2 * t3 - 3 * t2 + 1;
            double h10 = t3 - 2 * t2 + t;
            double h01 = -2 * t3 + 3 * t2;
            double h11 = t3 - t2;

            double m0 = _vStart * beta;
            double m1 = _vEnd * beta;

            double s = h00 * _slaveStart + h10 * m0 + h01 * _slaveEnd + h11 * m1;

            // Velocity (ds/dtau / beta)
            // h00' = 6t^2 - 6t
            // h10' = 3t^2 - 4t + 1
            // h01' = -6t^2 + 6t
            // h11' = 3t^2 - 2t
            
            double h00_d = 6 * t2 - 6 * t;
            double h10_d = 3 * t2 - 4 * t + 1;
            double h01_d = -6 * t2 + 6 * t;
            double h11_d = 3 * t2 - 2 * t;

            double v_tau = h00_d * _slaveStart + h10_d * m0 + h01_d * _slaveEnd + h11_d * m1;
            double v = v_tau / beta;

            // Acceleration
            // h00'' = 12t - 6
            // h10'' = 6t - 4
            // h01'' = -12t + 6
            // h11'' = 6t - 2
            
            double h00_dd = 12 * t - 6;
            double h10_dd = 6 * t - 4;
            double h01_dd = -12 * t + 6;
            double h11_dd = 6 * t - 2;

            double a_tau = h00_dd * _slaveStart + h10_dd * m0 + h01_dd * _slaveEnd + h11_dd * m1;
            double a = a_tau / (beta * beta);

            // Jerk
            // h00''' = 12
            // h10''' = 6
            // h01''' = -12
            // h11''' = 6
            
            double h00_ddd = 12;
            double h10_ddd = 6;
            double h01_ddd = -12;
            double h11_ddd = 6;

            double j_tau = h00_ddd * _slaveStart + h10_ddd * m0 + h01_ddd * _slaveEnd + h11_ddd * m1;
            double j = j_tau / (beta * beta * beta);

            return new CamPoint(theta, s, v, a, j);
        }
    }
}
