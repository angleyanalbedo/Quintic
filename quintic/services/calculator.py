from typing import List, Dict, Union
import numpy as np
from schemas.cam import Project, Segment, CamPoint, MotionLawType
from kernels.polynomial5 import Polynomial5


class CamCalculator:
    @staticmethod
    def calculate_project(project: Project) -> List[CamPoint]:
        full_profile = []

        # Sort segments by master_start (just in case)
        project.segments.sort(key=lambda s: s.master_start)

        # Validation: Check for gaps or overlaps in master position
        for i in range(len(project.segments) - 1):
            curr = project.segments[i]
            next_seg = project.segments[i + 1]
            if curr.master_end != next_seg.master_start:
                raise ValueError(
                    f"Gap detected between segment {curr.id} (end {curr.master_end}) and {next_seg.id} (start {next_seg.master_start}). Segments must be continuous."
                )

        total_master = (
            project.segments[-1].master_end - project.segments[0].master_start
        )
        resolution = project.config.resolution

        for i, segment in enumerate(project.segments):
            # 1. Determine local resolution
            segment_master = segment.master_end - segment.master_start
            if total_master == 0:
                seg_res = 2
            else:
                seg_res = max(2, int(resolution * (segment_master / total_master)))

            # 2. Select Kernel based on Motion Law
            # Currently only Poly5 (R-R) is supported.
            # Future expansion: Switch/If based on segment.motion_law

            if segment.motion_law == MotionLawType.POLY5:
                # TODO: Pass boundary velocities/accelerations once implemented in kernel
                poly = Polynomial5(
                    master_start=segment.master_start,
                    master_end=segment.master_end,
                    slave_start=segment.slave_start,
                    slave_end=segment.slave_end,
                )
                segment_data = poly.generate_table(resolution=seg_res)
            else:
                # Fallback or Error for unsupported types
                raise ValueError(
                    f"Motion Law {segment.motion_law} not yet implemented."
                )

            # 3. Stitching
            # Remove last point of current segment to avoid duplicate with start of next
            if i < len(project.segments) - 1 and len(segment_data) > 0:
                segment_data.pop()

            full_profile.extend(segment_data)

        return full_profile
