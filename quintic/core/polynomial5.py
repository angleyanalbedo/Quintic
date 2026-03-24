import math


class Polynomial5:
    """
    Implements a 5th-order polynomial motion law (Rest-in-Rest) as per VDI 2143.
    This provides a smooth transition with zero velocity and acceleration at the start and end.

    Formula: s(tau) = 10*tau^3 - 15*tau^4 + 6*tau^5
    where tau is the normalized time/position (0 to 1).
    """

    def __init__(
        self,
        master_start: float,
        master_end: float,
        slave_start: float,
        slave_end: float,
    ):
        self.master_start = master_start
        self.master_end = master_end
        self.slave_start = slave_start
        self.slave_end = slave_end

    def calculate(self, theta: float):
        """
        Calculate displacement (s), velocity (v), acceleration (a), and jerk (j)
        at a specific master position theta.
        """
        # 1. Normalize theta to tau [0, 1]
        beta = self.master_end - self.master_start

        # Avoid division by zero
        if beta == 0:
            return 0.0, 0.0, 0.0, 0.0

        # Handle out of bounds (clamping) for R-R
        if theta < self.master_start:
            return self.slave_start, 0.0, 0.0, 0.0
        if theta > self.master_end:
            return self.slave_end, 0.0, 0.0, 0.0

        tau = (theta - self.master_start) / beta
        h = self.slave_end - self.slave_start

        # 2. 5th Order Polynomial Coefficients (Rest-in-Rest)
        # s_norm(tau) = 10*tau^3 - 15*tau^4 + 6*tau^5

        # Displacement (s)
        # s = start + h * s_norm
        s_norm = 10 * (tau**3) - 15 * (tau**4) + 6 * (tau**5)
        s = self.slave_start + h * s_norm

        # Velocity (v) - First Derivative
        # v_norm = 30*tau^2 - 60*tau^3 + 30*tau^4
        # v = (h / beta) * v_norm
        v_norm = 30 * (tau**2) - 60 * (tau**3) + 30 * (tau**4)
        v = (h / beta) * v_norm

        # Acceleration (a) - Second Derivative
        # a_norm = 60*tau - 180*tau^2 + 120*tau^3
        # a = (h / beta^2) * a_norm
        a_norm = 60 * tau - 180 * (tau**2) + 120 * (tau**3)
        a = (h / (beta**2)) * a_norm

        # Jerk (j) - Third Derivative
        # j_norm = 60 - 360*tau + 360*tau^2
        # j = (h / beta^3) * j_norm
        j_norm = 60 - 360 * tau + 360 * (tau**2)
        j = (h / (beta**3)) * j_norm

        return s, v, a, j

    def generate_table(self, resolution: int = 100):
        """
        Generate a table of discrete points for the entire range.
        """
        data = []
        if resolution < 2:
            return data

        step = (self.master_end - self.master_start) / (resolution - 1)

        for i in range(resolution):
            theta = self.master_start + i * step
            s, v, a, j = self.calculate(theta)
            data.append({"theta": theta, "s": s, "v": v, "a": a, "j": j})
        return data
