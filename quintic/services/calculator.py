from schemas.cam import Point, CamPoint
from kernels.polynomial5 import Polynomial5


class CamCalculator:
    @staticmethod
    def calculate_sequence(
        points: list[Point], resolution: int = 100
    ) -> list[CamPoint]:
        # Sort points by master position just in case
        points.sort(key=lambda p: p.master)

        # Validate strictly increasing master position
        for i in range(len(points) - 1):
            if points[i].master >= points[i + 1].master:
                raise ValueError(
                    f"Master positions must be strictly increasing. Error at point {i + 1}"
                )

        full_profile = []

        # Iterate through segments
        for i in range(len(points) - 1):
            p_start = points[i]
            p_end = points[i + 1]

            # Determine resolution for this segment based on its duration proportion
            total_master = points[-1].master - points[0].master
            segment_master = p_end.master - p_start.master

            # Allocate points proportionally, but ensure at least 2 points per segment
            # Avoid division by zero if total_master is 0 (handled by validation above)
            if total_master == 0:
                segment_resolution = 2
            else:
                segment_resolution = max(
                    2, int(resolution * (segment_master / total_master))
                )

            poly = Polynomial5(
                master_start=p_start.master,
                master_end=p_end.master,
                slave_start=p_start.slave,
                slave_end=p_end.slave,
            )

            # Generate segment data
            # For all segments except the last one, remove the last point to avoid duplication
            # (Start of next segment = End of current segment)
            segment_data = poly.generate_table(resolution=segment_resolution)

            if i < len(points) - 2 and len(segment_data) > 0:
                segment_data.pop()

            full_profile.extend(segment_data)

        return full_profile
