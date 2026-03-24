from fastapi import APIRouter, HTTPException
from typing import List
from schemas.cam import SequenceRequest, CamPoint
from services.calculator import CamCalculator

router = APIRouter()


@router.post("/calculate_sequence", response_model=List[CamPoint])
async def calculate_sequence(request: SequenceRequest):
    if len(request.points) < 2:
        raise HTTPException(status_code=400, detail="At least 2 points are required")

    try:
        return CamCalculator.calculate_sequence(request.points, request.resolution)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
