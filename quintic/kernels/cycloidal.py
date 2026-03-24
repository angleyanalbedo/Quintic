import numpy as np
from typing import Tuple, Union, cast


class Cycloidal:
    """
    Implements Cycloidal Motion (VDI 2143: Bestehorn / Sinusoid).
    Excellent for high speed, low vibration.
    A_max = 2*pi (~6.28), J_max = 4*pi^2 (~39.48).

    Formula: s(tau) = tau - (1/2pi)*sin(2pi*tau)
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

        # Explicit casting for type checking
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
            TwoPi = 2 * np.pi

            # Displacement: s = h * (tau - 1/2pi * sin(2pi*tau))
            s[mask_valid] = self.slave_start + h * (t - (1 / TwoPi) * np.sin(TwoPi * t))

            # Velocity: v = (h/beta) * (1 - cos(2pi*tau))
            v[mask_valid] = (h / beta) * (1 - np.cos(TwoPi * t))

            # Acceleration: a = (h/beta^2) * 2pi * sin(2pi*tau)
            a[mask_valid] = (h / (beta**2)) * TwoPi * np.sin(TwoPi * t)

            # Jerk: j = (h/beta^3) * 4pi^2 * cos(2pi*tau)
            j[mask_valid] = (h / (beta**3)) * (TwoPi**2) * np.cos(TwoPi * t)

        if np.isscalar(theta):
            return float(s[0]), float(v[0]), float(a[0]), float(j[0])
        return s, v, a, j
