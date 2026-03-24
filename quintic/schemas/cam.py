from pydantic import BaseModel
from typing import List


class Point(BaseModel):
    master: float
    slave: float


class CamPoint(BaseModel):
    theta: float
    s: float
    v: float
    a: float
    j: float


class SequenceRequest(BaseModel):
    points: List[Point]
    resolution: int = 100
