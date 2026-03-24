from schemas.cam import (
    Project,
    CamPoint,
    MotionLawType,
    CoordinateMode,
    ReferenceType,
    Segment,
)
from kernels.polynomial5 import Polynomial5
from typing import List


class CamCalculator:
    @staticmethod
    def _resolve_coordinates(project: Project) -> List[Segment]:
        """
        Compiler Step: Convert relative/time-based inputs into absolute Master/Slave coordinates.
        This is the "Logic Core" of the application.
        """
        resolved_segments = []
        current_master = 0.0
        current_slave = 0.0

        # Determine global speed for Time -> Master conversion
        # RPM * 360 / 60 = deg/s
        if project.config.master_velocity == 0:
            speed_deg_per_sec = 0.0
        else:
            speed_deg_per_sec = (project.config.master_velocity * 360.0) / 60.0

        for seg in project.segments:
            # 1. Resolve Master End Position
            master_start = current_master
            master_end = 0.0

            # --- Reference Logic ---
            if seg.reference_type == ReferenceType.TIME:
                # Duration (seconds) -> Delta Master
                duration_sec = seg.master_val
                delta_master = duration_sec * speed_deg_per_sec
                master_end = master_start + delta_master

            elif seg.coordinate_mode == CoordinateMode.RELATIVE:
                # Delta Master (degrees/units)
                # For master relative, we treat master_val as delta
                master_end = master_start + seg.master_val

            else:  # ABSOLUTE MASTER
                # For master absolute, master_val is the target end position
                master_end = seg.master_val

            # 2. Resolve Slave End Position
            slave_start = current_slave
            slave_end = 0.0

            if seg.coordinate_mode == CoordinateMode.RELATIVE:
                slave_end = slave_start + seg.slave_val
            else:  # ABSOLUTE SLAVE
                slave_end = seg.slave_val

            # 3. Create Compiled Segment
            # We use model_copy() to create a new instance based on the original
            resolved_seg = seg.model_copy()
            # Inject computed values
            resolved_seg.computed_master_start = master_start
            resolved_seg.computed_master_end = master_end
            resolved_seg.computed_slave_start = slave_start
            resolved_seg.computed_slave_end = slave_end

            resolved_segments.append(resolved_seg)

            # Update state for next iteration
            current_master = master_end
            current_slave = slave_end

        return resolved_segments

    @staticmethod
    def calculate_project(project: Project) -> List[CamPoint]:
        # Step 1: Compile / Resolve Coordinates
        compiled_segments = CamCalculator._resolve_coordinates(project)

        full_profile = []
        if not compiled_segments:
            return []

        # Total Master Duration (Computed)
        # Use simple coalescing to avoid NoneType errors (though logic guarantees values)
        start_master = compiled_segments[0].computed_master_start or 0.0
        end_master = compiled_segments[-1].computed_master_end or 0.0
        total_master = end_master - start_master

        resolution = project.config.resolution

        # Step 2: Compute Curves
        for i, segment in enumerate(compiled_segments):
            # Safe unwrapping (they are set in _resolve_coordinates)
            m_start = segment.computed_master_start or 0.0
            m_end = segment.computed_master_end or 0.0
            s_start = segment.computed_slave_start or 0.0
            s_end = segment.computed_slave_end or 0.0

            # 2.1 Local Resolution Allocation
            segment_master = m_end - m_start
            if total_master <= 0:
                seg_res = 2
            else:
                seg_res = max(2, int(resolution * (segment_master / total_master)))

            # 2.2 Kernel Execution
            # Currently MVP only supports Poly5
            poly = Polynomial5(
                master_start=m_start,
                master_end=m_end,
                slave_start=s_start,
                slave_end=s_end,
            )
            segment_data = poly.generate_table(resolution=seg_res)

            # 2.3 Stitching (Remove last point of current segment)
            if i < len(compiled_segments) - 1 and len(segment_data) > 0:
                segment_data.pop()

            full_profile.extend(segment_data)

        return full_profile
