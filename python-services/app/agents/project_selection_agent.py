from typing import Any

from app.agents.base_agent import BaseAgent
from app.llm.llm_service import LLMService
from app.llm.prompts import PROJECT_SELECTION_SYSTEM_PROMPT


class ProjectSelectionAgent(BaseAgent):
    name = "project_selection_agent"

    def __init__(self, llm_service: LLMService):
        self.llm_service = llm_service

    async def run(self, input_data: dict[str, Any]) -> dict[str, Any]:
        user_prompt = f"""
Select the candidate's most relevant projects for this job.

CANDIDATE:
{input_data.get("candidate", {})}

JOB:
{input_data.get("job", {})}

MATCHING:
{input_data.get("matching", {})}
"""

        return await self.llm_service.generate_json(
            system_prompt=PROJECT_SELECTION_SYSTEM_PROMPT,
            user_prompt=user_prompt,
        )