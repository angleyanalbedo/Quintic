from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from api.v1.endpoints import router as v1_router


def create_app() -> FastAPI:
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

    app.include_router(v1_router, prefix="/api/v1")
    # For backward compatibility (MVP), we can also mount it at root or redirect,
    # but let's encourage using the new prefix or just mount it directly for now to not break frontend.
    app.include_router(v1_router)

    @app.get("/")
    async def root():
        return {"message": "Quintic API is running"}

    return app


app = create_app()

if __name__ == "__main__":
    import uvicorn

    uvicorn.run("main:app", host="127.0.0.1", port=8000, reload=True)
