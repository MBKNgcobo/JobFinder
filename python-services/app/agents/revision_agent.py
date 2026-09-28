from typing import Any

from app.agents.base_agent import BaseAgent
from app.llm.llm_service import LLMService
from app.llm.prompts import REVISION_SYSTEM_PROMPT


class RevisionAgent(BaseAgent):
    name = "revision_agent"

    def __init__(self, llm_service: LLMService):
        self.llm_service = llm_service

    async def run(
        self,
        input_data: dict[str, Any],
    ) -> dict[str, Any]:

        user_prompt = f"""
Revise this application package based on the critic's findings.

CANDIDATE:
{input_data.get("candidate", {})}

JOB:
{input_data.get("job", {})}

MATCHING:
{input_data.get("matching", {})}

CURRENT PROJECTS:
{input_data.get("projects", {})}

CURRENT CV CHANGES:
{input_data.get("cv", {})}

CURRENT COVER LETTER:
{input_data.get("cover_letter", {})}

CRITIC:
{input_data.get("critic", {})}
"""

        return await self.llm_service.generate_json(
            system_prompt=REVISION_SYSTEM_PROMPT,
            user_prompt=user_prompt,
        )