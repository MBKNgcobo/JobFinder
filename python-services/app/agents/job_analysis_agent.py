import asyncio
from typing import Any

from app.agents.base_agent import BaseAgent
from app.llm.llm_service import LLMService
from app.llm.prompts import JOB_ANALYSIS_SYSTEM_PROMPT


class JobAnalysisAgent(BaseAgent):
    name = "job_analysis_agent"

    def __init__(self, llm_service: LLMService):
        self.llm_service = llm_service

    async def run(
        self,
        input_data: dict[str, Any],
    ) -> dict[str, Any]:

        user_prompt = f"""
Analyse this job posting.

Title: {input_data.get("title", "")}
Company: {input_data.get("company", "")}

Description:
{input_data.get("description", "")}

Required skills:
{input_data.get("required_skills", [])}

Preferred skills:
{input_data.get("preferred_skills", [])}
"""

        try:
            result = await asyncio.wait_for(
                self.llm_service.generate_json(
                    system_prompt=JOB_ANALYSIS_SYSTEM_PROMPT,
                    user_prompt=user_prompt,
                ),
                timeout=55,
            )

            return result

        except Exception as exception:
            print(
                f"[JOB ANALYSIS FALLBACK] "
                f"LLM unavailable/slow: "
                f"{type(exception).__name__}: {exception!r}"
            )

            # The database already contains structured job data.
            # Use it rather than blocking the whole application.
            description = input_data.get("description", "")

            return {
                "title": input_data.get("title", ""),
                "company": input_data.get("company", ""),
                "summary": description[:500],
                "required_skills": input_data.get(
                    "required_skills",
                    [],
                ),
                "preferred_skills": input_data.get(
                    "preferred_skills",
                    [],
                ),
                "responsibilities": [],
                "experience_requirements": [],
                "education_requirements": [],
                "keywords": (
                    input_data.get("required_skills", [])
                    + input_data.get("preferred_skills", [])
                ),
            }