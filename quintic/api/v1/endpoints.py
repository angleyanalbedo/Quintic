from fastapi import APIRouter, HTTPException
from typing import List
from schemas.cam import Project, CamPoint
from services.calculator import CamCalculator

router = APIRouter()


@router.post("/calculate_project", response_model=List[CamPoint])
async def calculate_project(project: Project):
    try:
        return CamCalculator.calculate_project(project)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
