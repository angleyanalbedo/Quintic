from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List
from core.polynomial5 import Polynomial5

app = FastAPI(
    title="Quintic API", description="VDI 2143 Compliant Cam Profile Generator"
)

# Configure CORS
origins = [
    "http://localhost:5173",
    "http://127.0.0.1:5173",
]

app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


class CamPoint(BaseModel):
    theta: float
    s: float
    v: float
    a: float
    j: float


class Point(BaseModel):
    master: float
    slave: float


class SequenceRequest(BaseModel):
    points: List[Point]
    resolution: int = 100


@app.post("/calculate_sequence", response_model=List[CamPoint])
async def calculate_sequence(request: SequenceRequest):
    points = request.points

    # Validation
    if len(points) < 2:
        raise HTTPException(status_code=400, detail="At least 2 points are required")

    # Sort points by master position just in case
    points.sort(key=lambda p: p.master)

    # Validate strictly increasing master position
    for i in range(len(points) - 1):
        if points[i].master >= points[i + 1].master:
            raise HTTPException(
                status_code=400,
                detail=f"Master positions must be strictly increasing. Error at point {i + 1}",
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
        segment_resolution = max(
            2, int(request.resolution * (segment_master / total_master))
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

        if i < len(points) - 2:
            segment_data.pop()

        full_profile.extend(segment_data)

    return full_profile


@app.get("/")
async def root():
    return {"message": "Quintic API is running"}
