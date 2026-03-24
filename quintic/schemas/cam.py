from enum import Enum
from pydantic import BaseModel, Field
from typing import List, Optional


class MotionLawType(str, Enum):
    """
    Standard VDI 2143 motion laws.
    """

    POLY5 = "Polynomial5"
    MODIFIED_SINE = "ModifiedSine"  # Placeholder
    CYCLOIDAL = "Cycloidal"  # Placeholder
    CONSTANT_VELOCITY = "ConstantVelocity"  # Placeholder
    # Add more as needed: Modified Trapezoid, etc.


class BoundaryType(str, Enum):
    """
    Standard boundary conditions:
    R: Rest (v=0, a=0)
    U: Umkehr (Reversal, v=0, a!=0)
    G: Gleichlauf (Constant Velocity, a=0)
    B: Bewegt (General Motion, v!=0, a!=0)
    """

    R = "R"
    U = "U"
    G = "G"
    B = "B"


class Segment(BaseModel):
    """
    Defines a single motion segment (e.g., Rise, Dwell, Return).
    """

    id: str  # Unique ID for frontend tracking
    motion_law: MotionLawType = MotionLawType.POLY5

    # Geometry (Absolute Coordinates)
    master_start: float
    master_end: float
    slave_start: float
    slave_end: float

    # Boundary Conditions (Start/End)
    # Defaults to 0 (Rest-in-Rest) for MVP, but expandable later
    start_velocity: float = 0.0
    end_velocity: float = 0.0
    start_acceleration: float = 0.0
    end_acceleration: float = 0.0

    # Optional: Jerk limits, lambda, etc.


class ProjectConfig(BaseModel):
    """
    Global settings for the entire cam project.
    """

    master_velocity: float = 60.0  # RPM (Revolutions Per Minute)
    resolution: int = 360  # Number of calculation points


class Project(BaseModel):
    """
    The root object representing the entire cam design.
    """

    config: ProjectConfig
    segments: List[Segment]


class CamPoint(BaseModel):
    theta: float
    s: float
    v: float
    a: float
    j: float


class CalculationResponse(BaseModel):
    points: List[CamPoint]
    max_velocity: float
    max_acceleration: float
    max_jerk: float
