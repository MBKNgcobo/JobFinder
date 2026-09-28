from typing import Any

from app.agents.base_agent import BaseAgent
from app.llm.llm_service import LLMService
from app.llm.prompts import CRITIC_SYSTEM_PROMPT


class CriticAgent(BaseAgent):
    name = "critic_agent"

    def __init__(self, llm_service: LLMService):
        self.llm_service = llm_service

    async def run(
        self,
        input_data: dict[str, Any],
    ) -> dict[str, Any]:

        user_prompt = f"""
Review this generated job application package.

CANDIDATE:
{input_data.get("candidate", {})}

JOB:
{input_data.get("job", {})}

MATCHING:
{input_data.get("matching", {})}

SELECTED PROJECTS:
{input_data.get("projects", {})}

CV CHANGES:
{input_data.get("cv", {})}

COVER LETTER:
{input_data.get("cover_letter", {})}
"""

        return await self.llm_service.generate_json(
            system_prompt=CRITIC_SYSTEM_PROMPT,
            user_prompt=user_prompt,
        )