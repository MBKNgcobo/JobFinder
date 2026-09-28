import asyncio

from app.llm.llm_service import LLMService


async def main():
    llm = LLMService()

    result = await llm.generate_json(
        system_prompt="""
Return valid JSON only.

Return exactly:

{
    "message": ""
}
""",
        user_prompt="""
Say hello to the JobFinder system.
""",
    )

    print(result)


asyncio.run(main())