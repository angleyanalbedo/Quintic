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


class CalculationRequest(BaseModel):
    masterStart: float
    masterEnd: float
    slaveStart: float
    slaveEnd: float
    resolution: int = 100


class CamPoint(BaseModel):
    theta: float
    s: float
    v: float
    a: float
    j: float


@app.post("/calculate", response_model=List[CamPoint])
async def calculate_profile(request: CalculationRequest):
    if request.masterEnd <= request.masterStart:
        raise HTTPException(
            status_code=400, detail="Master End must be greater than Master Start"
        )

    poly = Polynomial5(
        master_start=request.masterStart,
        master_end=request.masterEnd,
        slave_start=request.slaveStart,
        slave_end=request.slaveEnd,
    )

    # Generate points
    # The generate_table method returns a list of dictionaries that match CamPoint structure
    data = poly.generate_table(resolution=request.resolution)

    return data


@app.get("/")
async def root():
    return {"message": "Quintic API is running"}
