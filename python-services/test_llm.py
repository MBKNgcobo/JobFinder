import asyncio
import json

from app.agents.candidate_analysis_agent import (
    CandidateAnalysisAgent,
)
from app.agents.job_analysis_agent import (
    JobAnalysisAgent,
)
from app.llm.llm_service import LLMService


async def main():

    llm = LLMService()

    job_agent = JobAnalysisAgent(llm)
    candidate_agent = CandidateAnalysisAgent(llm)

    job = {
        "title": "Software Developer",
        "company": "Network IT",
        "description": """
        Develop and maintain software applications.
        GraphQL experience is required.
        SQL and Python experience are advantageous.
        Work with APIs and backend systems.
        """,
        "required_skills": [
            "GraphQL"
        ],
        "preferred_skills": [
            "SQL",
            "Python"
        ],
    }

    candidate = {
        "first_name": "Mcebo",
        "last_name": "Test",
        "summary": "Final-year IT student building software projects.",
        "years_of_experience": 0,
        "skills": [
            "Python",
            "SQL",
            "Kubernetes",
        ],
        "projects": [
            {
                "name": "JobFinder",
                "skills": [
                    "Python",
                    "SQL",
                    "Docker",
                    "FastAPI",
                ],
            }
        ],
    }

    job_result = await job_agent.run(job)

    candidate_result = await candidate_agent.run(
        candidate
    )

    print("\n==============================")
    print("JOB ANALYSIS")
    print("==============================")

    print(
        json.dumps(
            job_result,
            indent=2,
        )
    )

    print("\n==============================")
    print("CANDIDATE ANALYSIS")
    print("==============================")

    print(
        json.dumps(
            candidate_result,
            indent=2,
        )
    )


if __name__ == "__main__":
    asyncio.run(main())