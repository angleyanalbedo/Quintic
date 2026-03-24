from enum import Enum
from pydantic import BaseModel, Field
from typing import List, Optional, Union

# --- Enums ---


class MotionLawType(str, Enum):
    """
    Standard VDI 2143 motion laws.
    """

    POLY5 = "Polynomial5"
    MODIFIED_SINE = "ModifiedSine"  # Placeholder
    CYCLOIDAL = "Cycloidal"  # Placeholder
    CONSTANT_VELOCITY = "ConstantVelocity"  # Placeholder
    # Future: Modified Trapezoid, S-Curve, Spline


class CoordinateMode(str, Enum):
    """
    Defines how the segment coordinates are interpreted.
    """

    ABSOLUTE = "Absolute"  # Master/Slave values are absolute positions
    RELATIVE = "Relative"  # Master/Slave values are deltas relative to previous end


class ReferenceType(str, Enum):
    """
    Defines the reference basis for the segment duration.
    """

    MASTER = "Master"  # Duration is defined by Master Axis degrees/units
    TIME = "Time"  # Duration is defined by Time (seconds/ms)


class ExecutionMode(str, Enum):
    """
    Defines how the cam profile executes.
    """

    ONE_SHOT = "OneShot"  # Runs once and stops (Velocity 0 at end)
    CYCLIC = "Cyclic"  # Repeats continuously (Start/End must match)
    # Future: CYCLIC_APPENDING (e.g., Feeding material)


class BoundaryType(str, Enum):
    """
    Standard boundary conditions (VDI 2143):
    R: Rest (v=0, a=0)
    U: Umkehr (Reversal, v=0, a!=0)
    G: Gleichlauf (Constant Velocity, a=0)
    B: Bewegt (General Motion, v!=0, a!=0)
    """

    R = "R"
    U = "U"
    G = "G"
    B = "B"
    # Note: These map to velocity/acceleration constraints in the segment definition


# --- Core Objects ---


class PhysicalLimits(BaseModel):
    """
    Hardware constraints for validation.
    """

    max_velocity: Optional[float] = None
    max_acceleration: Optional[float] = None
    max_jerk: Optional[float] = None


class CamEvent(BaseModel):
    """
    Triggers based on cam execution.
    """

    id: str
    trigger_master_pos: float  # Absolute master position to trigger
    action: str  # e.g., "SetBit:IO_1", "Log:Cut"
    # Future: trigger_type (Time/Master), active_high/low


class Segment(BaseModel):
    """
    Defines a single motion segment with high flexibility.
    """

    id: str  # Unique ID for frontend tracking
    motion_law: MotionLawType = MotionLawType.POLY5

    # Mode Settings
    coordinate_mode: CoordinateMode = CoordinateMode.ABSOLUTE
    reference_type: ReferenceType = ReferenceType.MASTER

    # Geometry (Can be Absolute or Relative based on mode)
    # If ReferenceType=Time, master_end/delta is calculated from duration + velocity
    master_val: float  # Absolute End Position OR Delta Master OR Duration (seconds)
    slave_val: float  # Absolute End Position OR Delta Slave

    # Boundary Conditions (Start/End)
    # These override the automatic calculation if provided
    start_velocity: Optional[float] = 0.0
    end_velocity: Optional[float] = 0.0
    start_acceleration: Optional[float] = 0.0
    end_acceleration: Optional[float] = 0.0

    # Advanced Parameters
    lambda_param: float = 0.5  # For inflection point shifting (0.0 - 1.0)

    # Derived/Computed Fields (Filled by Compiler, not User)
    computed_master_start: Optional[float] = None
    computed_master_end: Optional[float] = None
    computed_slave_start: Optional[float] = None
    computed_slave_end: Optional[float] = None


class ProjectConfig(BaseModel):
    """
    Global settings for the entire cam project.
    """

    master_velocity: float = (
        60.0  # RPM (Reference velocity for Time <-> Master conversion)
    )
    resolution: int = 360  # Number of calculation points
    execution_mode: ExecutionMode = ExecutionMode.ONE_SHOT
    limits: PhysicalLimits = Field(default_factory=PhysicalLimits)
    units_master: str = "deg"  # Display only
    units_slave: str = "mm"  # Display only


class Project(BaseModel):
    """
    The root object representing the entire cam design.
    """

    config: ProjectConfig
    segments: List[Segment]
    events: List[CamEvent] = []  # Global event list


class CamPoint(BaseModel):
    theta: float
    s: float
    v: float
    a: float
    j: float


class CalculationResponse(BaseModel):
    points: List[CamPoint]
    events: List[CamEvent]  # Return events with resolved trigger positions
    max_velocity: float
    max_acceleration: float
    max_jerk: float
