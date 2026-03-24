import numpy as np
from typing import List, Dict, Union, Tuple, cast


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

    def calculate(
        self, theta: Union[float, np.ndarray]
    ) -> Union[
        Tuple[float, float, float, float],
        Tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray],
    ]:
        """
        Calculate displacement (s), velocity (v), acceleration (a), and jerk (j)
        at a specific master position theta. Supports scalar and numpy array inputs.
        """
        # Ensure input is array-like for uniform handling
        is_scalar = np.isscalar(theta)
        # Use atleast_1d to ensure we can index it, but keep track if we need to return scalar
        theta_arr = np.atleast_1d(theta)

        beta = self.master_end - self.master_start

        # Avoid division by zero
        if beta == 0:
            zeros = np.zeros_like(theta_arr, dtype=np.float64)
            if is_scalar:
                return 0.0, 0.0, 0.0, 0.0
            return zeros, zeros, zeros, zeros

        # Normalize theta to tau
        # tau = (theta - start) / beta
        tau = (theta_arr - self.master_start) / beta
        h = self.slave_end - self.slave_start

        # Initialize result arrays with zeros (handles v, a, j outside range automatically)
        s = np.zeros_like(theta_arr, dtype=np.float64)
        v = np.zeros_like(theta_arr, dtype=np.float64)
        a = np.zeros_like(theta_arr, dtype=np.float64)
        j = np.zeros_like(theta_arr, dtype=np.float64)

        # Create masks
        mask_before = theta_arr < self.master_start
        mask_after = theta_arr > self.master_end
        # Valid range: start <= theta <= end
        mask_valid = ~(mask_before | mask_after)

        # Apply Boundary Conditions for Displacement
        if np.any(mask_before):
            s[mask_before] = self.slave_start
        if np.any(mask_after):
            s[mask_after] = self.slave_end

        # Valid points processing
        if np.any(mask_valid):
            t = tau[mask_valid]

            # Precompute powers for efficiency (Vectorized)
            t2 = t * t
            t3 = t2 * t
            t4 = t3 * t
            t5 = t4 * t

            # Displacement (s)
            s_norm = 10.0 * t3 - 15.0 * t4 + 6.0 * t5
            s[mask_valid] = self.slave_start + h * s_norm

            # Velocity (v)
            v_norm = 30.0 * t2 - 60.0 * t3 + 30.0 * t4
            v[mask_valid] = (h / beta) * v_norm

            # Acceleration (a)
            a_norm = 60.0 * t - 180.0 * t2 + 120.0 * t3
            a[mask_valid] = (h / (beta**2)) * a_norm

            # Jerk (j)
            j_norm = 60.0 - 360.0 * t + 360.0 * t2
            j[mask_valid] = (h / (beta**3)) * j_norm

        if is_scalar:
            return float(s[0]), float(v[0]), float(a[0]), float(j[0])

        return s, v, a, j

    def generate_table(self, resolution: int = 100) -> List[Dict[str, float]]:
        """
        Generate a table of discrete points for the entire range using vectorized operations.
        """
        if resolution < 2:
            return []

        # Generate all theta values at once
        thetas = np.linspace(self.master_start, self.master_end, resolution)

        # Calculate all profiles in one go (vectorized)
        result = self.calculate(thetas)

        # Explicitly handle the tuple logic for type checking
        if isinstance(
            result[0], float
        ):  # Should not happen with array input, but safe guard
            s, v, a, j = result  # type: ignore
            # Wrap in list if scalar (unlikely path)
            return [
                {
                    "theta": float(thetas[0]),
                    "s": float(s),
                    "v": float(v),
                    "a": float(a),
                    "j": float(j),
                }
            ]

        # Cast to array for type checker
        s_arr = cast(np.ndarray, result[0])
        v_arr = cast(np.ndarray, result[1])
        a_arr = cast(np.ndarray, result[2])
        j_arr = cast(np.ndarray, result[3])

        # Efficiently construct list of dicts
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
