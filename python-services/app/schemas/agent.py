from pydantic import BaseModel


class JobMatchRequest(BaseModel):
    user_id: int
    job_id: int
    user_skills: list[str]
    required_skills: list[str]
    preferred_skills: list[str] = []

class JobMatchResponse(BaseModel):
    user_id: int
    job_id: int
    match_score: float
    matched_skills: list[str]
    missing_skills: list[str]