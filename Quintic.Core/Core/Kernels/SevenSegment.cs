using Quintic.Wpf.Core.Kernels.Base;
using Quintic.Wpf.Core.Models;
using System;

namespace Quintic.Wpf.Core.Kernels
{
    public class SevenSegment : BaseMotionKernel
    {
        private readonly double _slaveStart;
        private readonly double _slaveEnd;

        public SevenSegment(double masterStart, double masterEnd, double slaveStart, double slaveEnd)
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

            // 7-Segment (Trapezoidal Acceleration)
            // We use a standard symmetric profile:
            // t1 (Jerk+) = 1/8
            // t2 (Const Acc) = 2/8
            // t3 (Jerk-) = 1/8
            // ... symmetric for deceleration
            
            double s_norm = 0, v_norm = 0, a_norm = 0, j_norm = 0;

            // Constants derived for 1/8 - 2/8 - 1/8 split
            // Max Accel (Ca) = 8 (relative to h/beta^2 if normalized to T=1? No, let's calculate)
            // Let's use normalized time t in 0..1
            // v_max = 2
            // a_max = 8
            // j_max = 64
            // Wait, let's derive precisely for 1/8 split.
            // Area of Accel trapezoid must equal v_max/2? No, integral of a is v.
            // v(0.5) = 1.0 (normalized, if h=1).
            // Accel profile is symmetric. Area of half accel triangle/trapezoid = 1.
            // Half duration = 0.5.
            // Trapezoid: rise=0.125, flat=0.25, fall=0.125.
            // Area = a_max * (0.25 + 0.125) = a_max * 0.375.
            // Area must be 1.0? No, v(0.5) is peak velocity.
            // Average velocity is 1. Peak velocity for this profile is 2.
            // So Area = 2.
            // a_max * 0.375 = 2 => a_max = 5.333... (16/3)
            // j_max = a_max / 0.125 = 5.333 / 0.125 = 42.666... (128/3)

            double jMax = 128.0 / 3.0;
            double aMax = 16.0 / 3.0;
            
            // Phase 1: 0 - 0.125 (Jerk +)
            if (tau <= 0.125)
            {
                double t = tau;
                j_norm = jMax;
                a_norm = jMax * t;
                v_norm = 0.5 * jMax * t * t;
                s_norm = (1.0 / 6.0) * jMax * t * t * t;
            }
            // Phase 2: 0.125 - 0.375 (Const Accel)
            else if (tau <= 0.375)
            {
                double t = tau - 0.125;
                // State at end of P1
                double a0 = aMax;
                double v0 = 0.5 * jMax * 0.015625; // 0.125^2 = 0.015625
                double s0 = (1.0 / 6.0) * jMax * 0.001953125; // 0.125^3

                j_norm = 0;
                a_norm = aMax;
                v_norm = v0 + aMax * t;
                s_norm = s0 + v0 * t + 0.5 * aMax * t * t;
            }
            // Phase 3: 0.375 - 0.5 (Jerk -)
            else if (tau <= 0.5)
            {
                double t = tau - 0.375;
                // State at end of P2
                // v(0.375) = v(0.125) + aMax * 0.25
                double v0 = (0.5 * jMax * 0.015625) + (aMax * 0.25);
                double s0 = 0.5; // By symmetry, s at 0.5 is 0.5. Let's calculate s at 0.375 properly if needed, but let's integrate.
                // Actually, let's use symmetry for the second half to avoid error accumulation.
                
                // Re-calculate s0 at 0.375
                double t_p2 = 0.25;
                double v_p2_start = 0.5 * jMax * 0.015625;
                double s_p2_start = (1.0 / 6.0) * jMax * 0.001953125;
                double s_at_375 = s_p2_start + v_p2_start * t_p2 + 0.5 * aMax * t_p2 * t_p2;

                j_norm = -jMax;
                a_norm = aMax - jMax * t;
                v_norm = v0 + aMax * t - 0.5 * jMax * t * t;
                s_norm = s_at_375 + v0 * t + 0.5 * aMax * t * t - (1.0 / 6.0) * jMax * t * t * t;
            }
            // Phase 4: 0.5 - 0.625 (Jerk -)
            else if (tau <= 0.625)
            {
                // Use symmetry: s(tau) = 1 - s(1-tau)
                // v(tau) = v(1-tau)
                // a(tau) = -a(1-tau)
                // j(tau) = j(1-tau) -> Wait, jerk is anti-symmetric in sign?
                // Profile: J+ J0 J- J- J0 J+
                // J(0.1) = +, J(0.9) = +
                // A(0.1) = +, A(0.9) = -
                // V(0.1) = +, V(0.9) = +
                
                double t_mirror = 1.0 - tau;
                // Calculate for t_mirror (which is in Phase 3: 0.375 - 0.5)
                // ... Recursion is messy here. Let's just implement the logic.
                
                double t = tau - 0.5;
                // At 0.5: s=0.5, v=2.0, a=0
                double s0 = 0.5;
                double v0 = 2.0;
                
                j_norm = -jMax;
                a_norm = -jMax * t;
                v_norm = v0 - 0.5 * jMax * t * t;
                s_norm = s0 + v0 * t - (1.0 / 6.0) * jMax * t * t * t;
            }
            // Phase 5: 0.625 - 0.875 (Const Decel)
            else if (tau <= 0.875)
            {
                double t = tau - 0.625;
                // At 0.625
                double v0 = 2.0 - 0.5 * jMax * 0.015625; // v at 0.5 minus drop
                double s_at_625 = 0.5 + 2.0 * 0.125 - (1.0 / 6.0) * jMax * 0.001953125;

                j_norm = 0;
                a_norm = -aMax;
                v_norm = v0 - aMax * t;
                s_norm = s_at_625 + v0 * t - 0.5 * aMax * t * t;
            }
            // Phase 6: 0.875 - 1.0 (Jerk +)
            else
            {
                double t = tau - 0.875;
                // At 0.875
                // v should be small
                double v0 = 0.5 * jMax * 0.015625; // Symmetric to start
                // s should be close to 1
                
                // Let's calculate backwards from 1.0 for precision
                double t_end = 1.0 - tau;
                // s(tau) = 1 - s(t_end)
                // s(t_end) is Phase 1 calculation
                double s_end = (1.0 / 6.0) * jMax * t_end * t_end * t_end;
                
                s_norm = 1.0 - s_end;
                v_norm = 0.5 * jMax * t_end * t_end;
                a_norm = -jMax * t_end;
                j_norm = jMax;
            }

            double s = _slaveStart + h * s_norm;
            double v = (h / beta) * v_norm;
            double a = (h / (beta * beta)) * a_norm;
            double j = (h / Math.Pow(beta, 3)) * j_norm;

            return new CamPoint(theta, s, v, a, j);
        }
    }
}
