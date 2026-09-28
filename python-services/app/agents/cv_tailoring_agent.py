from typing import Any

from app.agents.base_agent import BaseAgent
from app.llm.llm_service import LLMService
from app.llm.prompts import CV_TAILORING_SYSTEM_PROMPT


class CvTailoringAgent(BaseAgent):
    name = "cv_tailoring_agent"

    def __init__(self, llm_service: LLMService):
        self.llm_service = llm_service

    async def run(self, input_data: dict[str, Any]) -> dict[str, Any]:
        user_prompt = f"""
Determine how the candidate's CV should be tailored for this job.

CANDIDATE:
{input_data.get("candidate", {})}

JOB:
{input_data.get("job", {})}

MATCHING:
{input_data.get("matching", {})}

SELECTED PROJECTS:
{input_data.get("projects", {})}
"""

        return await self.llm_service.generate_json(
            system_prompt=CV_TAILORING_SYSTEM_PROMPT,
            user_prompt=user_prompt,
        )