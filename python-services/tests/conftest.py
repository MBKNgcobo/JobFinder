"""Shared pytest fixtures for the CI-blocking suite.

No live network: the LLM boundary is stubbed with a fake
HTTP transport. T5+ expands this with the full suite.
"""

import json
import sys
from pathlib import Path

import httpx
import pytest

# Allow running pytest from the repo root or python-services/.
sys.path.insert(
    0, str(Path(__file__).resolve().parent.parent)
)


def _fake_llm_payload(result: dict) -> dict:
    return {
        "choices": [
            {
                "message": {
                    "content": json.dumps(result),
                },
            },
        ],
    }


@pytest.fixture
def fake_llm_transport(monkeypatch):
    """Serve canned LLM responses through the HTTP seam,
    so generate_json exercises its real request/response
    path without touching OpenRouter."""

    monkeypatch.setenv(
        "LLM_API_KEY", "test-key"
    )
    monkeypatch.setenv("LLM_MODEL", "test-model")

    def handler(request: httpx.Request) -> httpx.Response:
        return httpx.Response(
            200,
            json=_fake_llm_payload(
                {"ok": True},
            ),
        )

    transport = httpx.MockTransport(handler)

    real_client_class = httpx.AsyncClient

    def fake_client_class(*args, **kwargs):
        return real_client_class(
            *args,
            transport=transport,
            **kwargs,
        )

    monkeypatch.setattr(
        httpx,
        "AsyncClient",
        fake_client_class,
    )

    return transport
