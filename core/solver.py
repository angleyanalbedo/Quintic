import numpy as np

class CamKernel:
    def solve_quintic(self, s0, s1, v0, v1, a0, a1, T):
        """
        Solve for the coefficients of a quintic polynomial:
        s(t) = c0 + c1*t + c2*t^2 + c3*t^3 + c4*t^4 + c5*t^5
        
        Satisfying boundary conditions at t=0 and t=T.
        """
        # A * x = B
        # x = [c0, c1, c2, c3, c4, c5]
        
        # Rows correspond to:
        # 1. s(0) = s0
        # 2. v(0) = v0
        # 3. a(0) = a0
        # 4. s(T) = s1
        # 5. v(T) = v1
        # 6. a(T) = a1
        
        A = np.array([
            [1, 0, 0, 0, 0, 0],
            [0, 1, 0, 0, 0, 0],
            [0, 0, 2, 0, 0, 0],
            [1, T, T**2, T**3, T**4, T**5],
            [0, 1, 2*T, 3*T**2, 4*T**3, 5*T**4],
            [0, 0, 2, 6*T, 12*T**2, 20*T**3]
        ])
        
        B = np.array([s0, v0, a0, s1, v1, a1])
        
        coeffs = np.linalg.solve(A, B)
        return coeffs

    def evaluate(self, coeffs, t, T):
        """
        Evaluate position, velocity, and acceleration at time t.
        """
        c0, c1, c2, c3, c4, c5 = coeffs
        
        s = c0 + c1*t + c2*t**2 + c3*t**3 + c4*t**4 + c5*t**5
        v = c1 + 2*c2*t + 3*c3*t**2 + 4*c4*t**3 + 5*c5*t**4
        a = 2*c2 + 6*c3*t + 12*c4*t**2 + 20*c5*t**3
        
        return s, v, a
