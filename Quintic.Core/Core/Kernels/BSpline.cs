using Quintic.Wpf.Core.Kernels.Base;
using Quintic.Wpf.Core.Models;
using System;
using System.Collections.Generic;

namespace Quintic.Wpf.Core.Kernels
{
    /// <summary>
    /// Implements a Quintic Hermite Spline (Polynomial 5th Order equivalent for splines).
    /// Provides C2 continuity (Velocity and Acceleration).
    /// </summary>
    public class BSpline : BaseMotionKernel
    {
        private readonly double _slaveStart;
        private readonly double _slaveEnd;
        private readonly double _vStart;
        private readonly double _vEnd;
        private readonly double _aStart;
        private readonly double _aEnd;
        private readonly List<CamPoint> _controlPoints;

        public BSpline(double masterStart, double masterEnd, double slaveStart, double slaveEnd, 
                       double vStart = 0, double vEnd = 0, double aStart = 0, double aEnd = 0, List<CamPoint> controlPoints = null)
            : base(masterStart, masterEnd)
        {
            _slaveStart = slaveStart;
            _slaveEnd = slaveEnd;
            _vStart = vStart;
            _vEnd = vEnd;
            _aStart = aStart;
            _aEnd = aEnd;
            _controlPoints = controlPoints ?? new List<CamPoint>();
        }

        public override CamPoint Calculate(double theta)
        {
            double beta = _masterEnd - _masterStart;
            if (Math.Abs(beta) < 1e-9) return new CamPoint(theta, _slaveStart, 0, 0, 0);

            double tau = (theta - _masterStart) / beta;
            if (tau < 0) tau = 0;
            if (tau > 1) tau = 1;

            double t = tau;
            double t2 = t * t;
            double t3 = t2 * t;
            double t4 = t3 * t;
            double t5 = t4 * t;

            // Quintic Hermite Basis Functions
            double h00 = 1 - 10 * t3 + 15 * t4 - 6 * t5;
            double h10 = t - 6 * t3 + 8 * t4 - 3 * t5;
            double h20 = 0.5 * t2 - 1.5 * t3 + 1.5 * t4 - 0.5 * t5;
            
            double h01 = 10 * t3 - 15 * t4 + 6 * t5;
            double h11 = -4 * t3 + 7 * t4 - 3 * t5;
            double h21 = 0.5 * t3 - t4 + 0.5 * t5;

            // Scaled boundary conditions
            double m0 = _vStart * beta;
            double m1 = _vEnd * beta;
            double n0 = _aStart * beta * beta;
            double n1 = _aEnd * beta * beta;

            // Position
            double s = h00 * _slaveStart + h10 * m0 + h20 * n0 + h01 * _slaveEnd + h11 * m1 + h21 * n1;

            // First Derivatives of Basis Functions
            double h00_d = -30 * t2 + 60 * t3 - 30 * t4;
            double h10_d = 1 - 18 * t2 + 32 * t3 - 15 * t4;
            double h20_d = t - 4.5 * t2 + 6 * t3 - 2.5 * t4;
            
            double h01_d = 30 * t2 - 60 * t3 + 30 * t4;
            double h11_d = -12 * t2 + 28 * t3 - 15 * t4;
            double h21_d = 1.5 * t2 - 4 * t3 + 2.5 * t4;

            // Velocity
            double v_tau = h00_d * _slaveStart + h10_d * m0 + h20_d * n0 + h01_d * _slaveEnd + h11_d * m1 + h21_d * n1;
            double v = v_tau / beta;

            // Second Derivatives of Basis Functions
            double h00_dd = -60 * t + 180 * t2 - 120 * t3;
            double h10_dd = -36 * t + 96 * t2 - 60 * t3;
            double h20_dd = 1 - 9 * t + 18 * t2 - 10 * t3;
            
            double h01_dd = 60 * t - 180 * t2 + 120 * t3;
            double h11_dd = -24 * t + 84 * t2 - 60 * t3;
            double h21_dd = 3 - 12 * t + 10 * t2;

            // Acceleration
            double a_tau = h00_dd * _slaveStart + h10_dd * m0 + h20_dd * n0 + h01_dd * _slaveEnd + h11_dd * m1 + h21_dd * n1;
            double a = a_tau / (beta * beta);

            // Third Derivatives of Basis Functions
            double h00_ddd = -60 + 360 * t - 360 * t2;
            double h10_ddd = -36 + 192 * t - 180 * t2;
            double h20_ddd = -9 + 36 * t - 30 * t2;
            
            double h01_ddd = 60 - 360 * t + 360 * t2;
            double h11_ddd = -24 + 168 * t - 180 * t2;
            double h21_ddd = -12 + 20 * t;

            // Jerk
            double j_tau = h00_ddd * _slaveStart + h10_ddd * m0 + h20_ddd * n0 + h01_ddd * _slaveEnd + h11_ddd * m1 + h21_ddd * n1;
            double j = j_tau / (beta * beta * beta);

            return new CamPoint(theta, s, v, a, j);
        }
    }
}
