import asyncio
import json
import os
import re
from typing import Any

import httpx
from dotenv import load_dotenv

load_dotenv()


class LLMService:
    """
    Centralized LLM service for JobFinder.

    Uses OpenRouter's OpenAI-compatible API.

    The service:
    - supports a primary model
    - supports fallback models
    - handles rate limiting
    - handles provider/server errors
    - cleans markdown JSON
    - validates JSON responses
    - provides useful diagnostics
    """

    def __init__(self):

        self.base_url = os.getenv(
            "LLM_BASE_URL",
            "https://openrouter.ai/api/v1",
        ).rstrip("/")

        self.api_key = os.getenv(
            "LLM_API_KEY"
        )

        primary_model = os.getenv(
            "LLM_MODEL",
            "openrouter/free",
        ).strip()

        fallback_models = [
            model.strip()
            for model in os.getenv(
                "LLM_FALLBACK_MODELS",
                "",
            ).split(",")
            if model.strip()
        ]

        self.models = []

        if primary_model:
            self.models.append(
                primary_model
            )

        for model in fallback_models:

            if model not in self.models:
                self.models.append(
                    model
                )

        if not self.api_key:
            raise RuntimeError(
                "LLM_API_KEY is not configured."
            )

        if not self.models:
            raise RuntimeError(
                "No LLM models are configured."
            )

        print(
            "[LLM] Configured models:",
            ", ".join(self.models),
            flush=True,
        )

    async def generate_json(
        self,
        system_prompt: str,
        user_prompt: str,
    ) -> dict[str, Any]:

        if not self.api_key:
            raise RuntimeError(
                "LLM_API_KEY is not configured."
            )

        if not self.models:
            raise RuntimeError(
                "No LLM models are configured."
            )
        headers = {
            "Authorization": (
                f"Bearer {self.api_key}"
            ),
            "Content-Type": (
                "application/json"
            ),
            "HTTP-Referer": (
                "http://localhost:7176"
            ),
            "X-Title": "JobFinder",
        }

        errors: list[str] = []

        async with httpx.AsyncClient(
            timeout=60
        ) as client:

            for model_index, model in enumerate(
                self.models
            ):

                payload = {
                    "model": model,
                    "messages": [
                        {
                            "role": "system",
                            "content": system_prompt,
                        },
                        {
                            "role": "user",
                            "content": user_prompt,
                        },
                    ],
                }

                # JSON response format is supported by
                # models such as Gemma.
                #
                # openrouter/free may route to models
                # with different structured-output
                # capabilities, so we intentionally
                # do not force response_format there.
                if (
                    "gemma" in model.lower()
                    and ":free" in model.lower()
                ):
                    payload[
                        "response_format"
                    ] = {
                        "type": "json_object"
                    }

                success = False

                # Only retry a rate limit once.
                # We do not want a 25-job scraping cycle
                # to burn through the free API quota.
                for attempt in range(2):

                    try:

                        response = await client.post(
                            (
                                f"{self.base_url}"
                                "/chat/completions"
                            ),
                            headers=headers,
                            json=payload,
                        )

                        status = (
                            response.status_code
                        )

                        # ---------------------------------
                        # RATE LIMIT
                        # ---------------------------------

                        if status == 429:

                            message = (
                                f"{model}: "
                                "429 rate limited"
                            )

                            if attempt == 0:

                                retry_after = (
                                    response.headers.get(
                                        "Retry-After"
                                    )
                                )

                                try:
                                    wait_time = (
                                        float(
                                            retry_after
                                        )
                                        if retry_after
                                        else 3
                                    )

                                except (
                                    ValueError,
                                    TypeError,
                                ):

                                    wait_time = 3

                                wait_time = min(
                                    wait_time,
                                    10,
                                )

                                print(
                                    f"[LLM] {model} "
                                    f"rate limited. "
                                    f"Retrying in "
                                    f"{wait_time}s...",
                                    flush=True,
                                )

                                await asyncio.sleep(
                                    wait_time
                                )

                                continue

                            errors.append(
                                message
                            )

                            print(
                                f"[LLM] {message}",
                                flush=True,
                            )

                            break

                        # ---------------------------------
                        # SERVER / PROVIDER ERROR
                        # ---------------------------------

                        if status >= 500:

                            message = (
                                f"{model}: "
                                f"{status} "
                                "server/provider error"
                            )

                            errors.append(
                                message
                            )

                            print(
                                f"[LLM] {message}: "
                                f"{response.text[:500]}",
                                flush=True,
                            )

                            break

                        # ---------------------------------
                        # OTHER HTTP ERROR
                        # ---------------------------------

                        if not response.is_success:

                            body = (
                                response.text[:1000]
                            )

                            message = (
                                f"{model}: "
                                f"{status} "
                                f"{body}"
                            )

                            errors.append(
                                message
                            )

                            print(
                                "[LLM] Request failed: "
                                f"{message}",
                                flush=True,
                            )

                            break

                        # ---------------------------------
                        # PARSE HTTP JSON
                        # ---------------------------------

                        try:

                            data = response.json()

                        except Exception as exception:

                            message = (
                                f"{model}: invalid "
                                f"HTTP JSON: "
                                f"{exception}"
                            )

                            errors.append(
                                message
                            )

                            print(
                                f"[LLM] {message}",
                                flush=True,
                            )

                            break

                        # ---------------------------------
                        # PROVIDER ERROR OBJECT
                        # ---------------------------------

                        if "error" in data:

                            error = data[
                                "error"
                            ]

                            if isinstance(
                                error,
                                dict,
                            ):

                                message_text = (
                                    error.get(
                                        "message",
                                        str(error),
                                    )
                                )

                            else:

                                message_text = (
                                    str(error)
                                )

                            message = (
                                f"{model}: "
                                f"{message_text}"
                            )

                            errors.append(
                                message
                            )

                            print(
                                f"[LLM] {message}",
                                flush=True,
                            )

                            break

                        # ---------------------------------
                        # CHOICES
                        # ---------------------------------

                        choices = data.get(
                            "choices"
                        )

                        if not choices:

                            message = (
                                f"{model}: "
                                "response contained "
                                "no choices"
                            )

                            errors.append(
                                message
                            )

                            print(
                                f"[LLM] {message}",
                                flush=True,
                            )

                            print(
                                json.dumps(
                                    data,
                                    indent=2,
                                )[:3000],
                                flush=True,
                            )

                            break

                        # ---------------------------------
                        # MESSAGE
                        # ---------------------------------

                        message_data = (
                            choices[0].get(
                                "message",
                                {},
                            )
                        )

                        content = (
                            message_data.get(
                                "content"
                            )
                        )

                        # Some reasoning models can return
                        # content in unusual formats.
                        if isinstance(
                            content,
                            list,
                        ):

                            text_parts = []

                            for part in content:

                                if isinstance(
                                    part,
                                    dict,
                                ):

                                    text_value = (
                                        part.get(
                                            "text"
                                        )
                                    )

                                    if text_value:
                                        text_parts.append(
                                            text_value
                                        )

                            content = "\n".join(
                                text_parts
                            )

                        if not content:

                            message = (
                                f"{model}: "
                                "empty content"
                            )

                            errors.append(
                                message
                            )

                            print(
                                f"[LLM] {message}",
                                flush=True,
                            )

                            break

                        # ---------------------------------
                        # CLEAN JSON
                        # ---------------------------------

                        content = (
                            self._clean_json_content(
                                str(content)
                            )
                        )

                        # ---------------------------------
                        # PARSE MODEL JSON
                        # ---------------------------------

                        try:

                            result = json.loads(
                                content
                            )

                        except json.JSONDecodeError as exception:

                            # Sometimes a model produces
                            # explanatory text before/after
                            # the JSON object.
                            extracted = (
                                self._extract_json_object(
                                    content
                                )
                            )

                            if extracted:

                                try:

                                    result = json.loads(
                                        extracted
                                    )

                                except (
                                    json.JSONDecodeError
                                ):

                                    result = None

                            else:

                                result = None

                            if result is None:

                                message = (
                                    f"{model}: "
                                    "invalid JSON response: "
                                    f"{exception}"
                                )

                                errors.append(
                                    message
                                )

                                print(
                                    f"[LLM] {message}",
                                    flush=True,
                                )

                                print(
                                    "Raw response:",
                                    content[:2000],
                                    flush=True,
                                )

                                break

                        # ---------------------------------
                        # VALIDATE OBJECT
                        # ---------------------------------

                        if not isinstance(
                            result,
                            dict,
                        ):

                            message = (
                                f"{model}: "
                                "JSON response was "
                                "not an object"
                            )

                            errors.append(
                                message
                            )

                            print(
                                f"[LLM] {message}",
                                flush=True,
                            )

                            break

                        print(
                            "[LLM] Request succeeded "
                            f"using model: {model}",
                            flush=True,
                        )

                        success = True

                        return result

                    # -------------------------------------
                    # TIMEOUT
                    # -------------------------------------

                    except httpx.TimeoutException:

                        message = (
                            f"{model}: "
                            "request timed out"
                        )

                        errors.append(
                            message
                        )

                        print(
                            f"[LLM] {message}",
                            flush=True,
                        )

                        break

                    # -------------------------------------
                    # CONNECTION ERROR
                    # -------------------------------------

                    except httpx.RequestError as exception:

                        message = (
                            f"{model}: "
                            f"request error: "
                            f"{exception}"
                        )

                        errors.append(
                            message
                        )

                        print(
                            f"[LLM] {message}",
                            flush=True,
                        )

                        break

                if success:
                    break

                # Small delay before trying the next model.
                if model_index < len(
                    self.models
                ) - 1:

                    await asyncio.sleep(
                        0.5
                    )

        error_summary = "\n".join(
            f"  - {error}"
            for error in errors
        )

        raise RuntimeError(
            "All configured LLM models failed "
            "to return a valid response."
            + (
                f"\n{error_summary}"
                if error_summary
                else ""
            )
        )

    @staticmethod
    def _clean_json_content(
        content: str,
    ) -> str:

        content = content.strip()

        # Remove ```json
        content = re.sub(
            r"^```(?:json)?\s*",
            "",
            content,
            flags=re.IGNORECASE,
        )

        # Remove closing ```
        content = re.sub(
            r"\s*```$",
            "",
            content,
        )

        return content.strip()

    @staticmethod
    def _extract_json_object(
        content: str,
    ) -> str | None:

        """
        Attempts to extract the first complete
        JSON object from a response containing
        surrounding text.
        """

        start = content.find("{")

        if start == -1:
            return None

        depth = 0
        in_string = False
        escaped = False

        for index in range(
            start,
            len(content),
        ):

            character = content[index]

            if escaped:

                escaped = False
                continue

            if character == "\\" and in_string:

                escaped = True
                continue

            if character == '"':

                in_string = not in_string
                continue

            if in_string:
                continue

            if character == "{":

                depth += 1

            elif character == "}":

                depth -= 1

                if depth == 0:

                    return content[
                        start:index + 1
                    ]

        return None