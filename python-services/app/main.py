from fastapi import FastAPI

from app.routers import jobs
from app.routers import agents


app = FastAPI(
    title="JobFinder AI Services",
    description="Python services for the JobFinder application",
    version="1.0.0"
)

app.include_router(
    jobs.router,
    prefix="/api/jobs",
    tags=["Jobs"]
)

app.include_router(
    agents.router,
    prefix="/api/agents",
    tags=["Agents"]
    
)


@app.get("/health")
async def health_check():
    return {
        "status": "healthy",
        "service": "JobFinder Python Services"
    }

@app.get("/health")
async def health():
    return {
        "status": "ok",
        "service": "jobfinder-api",
    }