from pydantic import BaseModel
from typing import Optional


class JobAnalysisRequest(BaseModel):
    title: str
    description: str
    skills: list[str]


class JobAnalysisResponse(BaseModel):
    title: str
    required_skills: list[str]
    preferred_skills: list[str]
    experience_required: Optional[str] = None
    education_required: Optional[str] = None