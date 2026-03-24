import pytest
import numpy as np
from core.solver import CamKernel

def test_solve_quintic():
    kernel = CamKernel()
    
    # Parameters
    T = 100.0
    s0, s1 = 0.0, 50.0
    v0, v1 = 0.0, 0.0
    a0, a1 = 0.0, 0.0
    
    coeffs = kernel.solve_quintic(s0, s1, v0, v1, a0, a1, T)
    
    # Assert 6 coefficients
    assert len(coeffs) == 6
    
    # Evaluate at end point
    s_end, v_end, a_end = kernel.evaluate(coeffs, T, T)
    
    # Check position at end (allow for small floating point errors)
    assert np.isclose(s_end, s1)
    
    # Check boundary conditions match
    assert np.isclose(v_end, v1)
    assert np.isclose(a_end, a1)
