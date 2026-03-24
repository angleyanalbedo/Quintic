import numpy as np
from typing import Tuple, Union, cast


class ModifiedTrapezoid:
    """
    Implements a simplified Modified Trapezoid (or Modified Sine) using
    Polynomial 7 (4-5-6-7) approximation for VDI 2143.
    This provides finite jerk, continuous acceleration, and S-Curve behavior.

    Formula: s(tau) = 35*tau^4 - 84*tau^5 + 70*tau^6 - 20*tau^7
    A_max = 5.25 (approx Mod Trap which is 4.89)
    J_max = 52.5 (approx Mod Trap which is 61.4)
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

    def generate_table(self, resolution: int = 100) -> list[dict]:
        if resolution < 2:
            return []
        thetas = np.linspace(self.master_start, self.master_end, resolution)
        s, v, a, j = self.calculate(thetas)  # type: ignore

        s_arr = cast(np.ndarray, s)
        v_arr = cast(np.ndarray, v)
        a_arr = cast(np.ndarray, a)
        j_arr = cast(np.ndarray, j)

        return [
            {
                "theta": float(t),
                "s": float(si),
                "v": float(vi),
                "a": float(ai),
                "j": float(ji),
            }
            for t, si, vi, ai, ji in zip(thetas, s_arr, v_arr, a_arr, j_arr)
        ]

    def calculate(
        self, theta: Union[float, np.ndarray]
    ) -> Union[
        Tuple[float, float, float, float],
        Tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray],
    ]:
        theta_arr = np.atleast_1d(theta)
        beta = self.master_end - self.master_start
        if beta == 0:
            zeros = np.zeros_like(theta_arr, dtype=np.float64)
            return zeros, zeros, zeros, zeros

        tau = (theta_arr - self.master_start) / beta
        h = self.slave_end - self.slave_start

        # Valid range mask
        mask_valid = (tau >= 0) & (tau <= 1)

        s = np.zeros_like(theta_arr, dtype=np.float64)
        v = np.zeros_like(theta_arr, dtype=np.float64)
        a = np.zeros_like(theta_arr, dtype=np.float64)
        j = np.zeros_like(theta_arr, dtype=np.float64)

        # Boundary filling
        s[tau > 1] = self.slave_end
        s[tau < 0] = self.slave_start

        if np.any(mask_valid):
            t = tau[mask_valid]
            t2 = t * t
            t3 = t2 * t
            t4 = t3 * t
            t5 = t4 * t
            t6 = t5 * t
            t7 = t6 * t

            # Polynomial 4-5-6-7 (Rest-in-Rest, Zero Jerk at Ends)
            # s = 35t^4 - 84t^5 + 70t^6 - 20t^7
            s_norm = 35 * t4 - 84 * t5 + 70 * t6 - 20 * t7
            s[mask_valid] = self.slave_start + h * s_norm

            # v = 140t^3 - 420t^4 + 420t^5 - 140t^6
            v_norm = 140 * t3 - 420 * t4 + 420 * t5 - 140 * t6
            v[mask_valid] = (h / beta) * v_norm

            # a = 420t^2 - 1680t^3 + 2100t^4 - 840t^5
            a_norm = 420 * t2 - 1680 * t3 + 2100 * t4 - 840 * t5
            a[mask_valid] = (h / (beta**2)) * a_norm

            # j = 840t - 5040t^2 + 8400t^3 - 4200t^4
            j_norm = 840 * t - 5040 * t2 + 8400 * t3 - 4200 * t4
            j[mask_valid] = (h / (beta**3)) * j_norm

        if np.isscalar(theta):
            return float(s[0]), float(v[0]), float(a[0]), float(j[0])
        return s, v, a, j
