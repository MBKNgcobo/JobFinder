from fastapi import APIRouter

from app.schemas.job import (
    JobAnalysisRequest,
    JobAnalysisResponse
)

router = APIRouter()


@router.get("/health")
async def jobs_health():
    return {
        "status": "ok",
        "service": "job-service"
    }


@router.post(
    "/analyze",
    response_model=JobAnalysisResponse
)
async def analyze_job(
    request: JobAnalysisRequest
):

    # Temporary implementation.
    # Later this will call your Job Analysis Agent.

    return JobAnalysisResponse(
        title=request.title,
        required_skills=request.skills,
        preferred_skills=[],
        experience_required=None,
        education_required=None
    )