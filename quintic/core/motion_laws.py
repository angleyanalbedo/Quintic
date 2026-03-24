from enum import Enum

class MotionLaw(Enum):
    QUINTIC_POLYNOMIAL = "Quintic Polynomial"
    CYCLOIDAL = "Cycloidal"
    MODIFIED_SINE = "Modified Sine"
    DWELL = "Dwell"
