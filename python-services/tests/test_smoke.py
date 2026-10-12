"""Placeholder proving the red-green gate end to end:
generate_json exercises its real HTTP request/response
path through the LLM boundary against a fake transport.
No live network. T5+ replaces this with the full suite."""

import asyncio

from app.llm.llm_service import LLMService


def test_generate_json_returns_stubbed_result(
    fake_llm_transport,
):
    service = LLMService()

    result = asyncio.run(
        service.generate_json(
            system_prompt="system",
            user_prompt="user",
        )
    )

    assert result == {"ok": True}
