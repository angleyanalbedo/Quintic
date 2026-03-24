from fastapi import FastAPI

app = FastAPI(title="Quintic Kernel")

@app.get("/health")
def health_check():
    return {"status": "Quintic Kernel is running"}
