from fastapi import APIRouter, HTTPException
from pydantic import BaseModel, Field

from app.agents.orchestrator import AgentOrchestrator
from app.services.matching_service import MatchingService
from app.services.recommendation_service import (
    RecommendationService,
)


router = APIRouter()


orchestrator = AgentOrchestrator()
matching_service = MatchingService()
recommendation_service = RecommendationService()


# =========================================================
# MATCHING
# =========================================================

class JobMatchRequest(BaseModel):
    user_id: int
    job_id: int


class JobMatchResponse(BaseModel):
    match_id: int
    user_id: int
    job_id: int
    match_score: float

    matched_required_skills: list[str]
    missing_required_skills: list[str]

    matched_preferred_skills: list[str]
    missing_preferred_skills: list[str]


# =========================================================
# APPLICATION AGENT REQUEST MODELS
# =========================================================

from pydantic import BaseModel, Field


class ProjectRequest(BaseModel):
    name: str
    skills: list[str] = Field(default_factory=list)


class UserProfileRequest(BaseModel):
    first_name: str
    last_name: str
    summary: str | None = None
    years_of_experience: int = 0
    skills: list[str] = Field(default_factory=list)
    projects: list[ProjectRequest] = Field(default_factory=list)


class JobApplicationRequest(BaseModel):
    title: str
    company: str
    description: str = ""
    required_skills: list[str] = Field(default_factory=list)
    preferred_skills: list[str] = Field(default_factory=list)


class RecommendedProjectResponse(BaseModel):
    name: str
    reason: str


class ApplicationAgentResponse(BaseModel):
    job_title: str
    company: str
    matched_skills: list[str] = Field(default_factory=list)
    missing_skills: list[str] = Field(default_factory=list)
    recommended_projects: list[
        RecommendedProjectResponse
    ] = Field(default_factory=list)
    relevant_experience: list[str] = Field(default_factory=list)
    cover_letter: str = ""
    cv_changes: list[str] = Field(default_factory=list)


class ApplicationAgentRequest(BaseModel):
    user_profile: UserProfileRequest
    job: JobApplicationRequest
@router.post(
    "/prepare-application",
    response_model=ApplicationAgentResponse,
)
async def prepare_application(
    request: ApplicationAgentRequest,
):
    try:
        result = await orchestrator.prepare_application(
            user_profile=request.user_profile.model_dump(),
            job=request.job.model_dump(),
        )

        return ApplicationAgentResponse(
            **result
        )

    except Exception as exception:
        raise HTTPException(
            status_code=500,
            detail=str(exception),
        )
# =========================================================
# RECOMMENDATIONS
# =========================================================

@router.get("/recommendations/{user_id}")
async def get_recommendations(
    user_id: int,
    minimum_score: float = 0.0,
    preferred_location: str | None = None,
):
    try:

        recommendations = (
            recommendation_service
            .generate_recommendations(
                user_id=user_id,
                minimum_score=minimum_score,
                preferred_location=preferred_location,
            )
        )

        return {
            "user_id": user_id,
            "count": len(recommendations),
            "recommendations": recommendations,
        }

    except Exception as exception:

        raise HTTPException(
            status_code=500,
            detail=str(exception),
        )


# =========================================================
# MATCH JOB
# =========================================================

@router.post(
    "/match-job",
    response_model=JobMatchResponse,
)
async def match_job(
    request: JobMatchRequest,
):

    try:

        result = (
            matching_service
            .match_user_to_job(
                user_id=request.user_id,
                job_id=request.job_id,
            )
        )

        return JobMatchResponse(
            match_id=result["match_id"],
            user_id=result["user_id"],
            job_id=result["job_id"],
            match_score=result["match_score"],

            matched_required_skills=result[
                "matched_required_skills"
            ],

            missing_required_skills=result[
                "missing_required_skills"
            ],

            matched_preferred_skills=result[
                "matched_preferred_skills"
            ],

            missing_preferred_skills=result[
                "missing_preferred_skills"
            ],
        )

    except Exception as exception:

        raise HTTPException(
            status_code=500,
            detail=str(exception),
        )


# =========================================================
# APPLICATION AGENT
# =========================================================
@router.post("/prepare-application")
async def prepare_application(
    request: ApplicationAgentRequest,
):
    try:
        result = await orchestrator.prepare_application(
            user_profile=request.user_profile.model_dump(),
            job=request.job.model_dump(),
        )

        return result

    except Exception as exception:
        raise HTTPException(
            status_code=500,
            detail=str(exception),
        )