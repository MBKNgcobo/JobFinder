
from app.agents.matching_agent import MatchingAgent
from app.database.repositories.matching_repository import (
    MatchingRepository,
)


class MatchingService:
    """
    Coordinates database data with the MatchingAgent.
    """

    def __init__(self):
        self.repository = MatchingRepository()
        self.agent = MatchingAgent()

    def match_user_to_job(
        self,
        user_id: int,
        job_id: int,
    ) -> dict:

        # Get the user's actual skills.
        user_skills = self.repository.get_user_skills(
            user_id
        )

        # Get the job's actual requirements.
        job_skills = self.repository.get_job_skills(
            job_id
        )

        required_skills = job_skills[
            "required_skills"
        ]

        preferred_skills = job_skills[
            "preferred_skills"
        ]

        # Run the actual MatchingAgent.
        result = self.agent.match(
            user_skills=user_skills,
            required_skills=required_skills,
            preferred_skills=preferred_skills,
        )

        # Save the result to JobMatches.
        analysis = {
            "user_skills": user_skills,
            "required_skills": required_skills,
            "preferred_skills": preferred_skills,
            "matched_required_skills": result[
                "matched_required_skills"
            ],
            "missing_required_skills": result[
                "missing_required_skills"
            ],
            "matched_preferred_skills": result[
                "matched_preferred_skills"
            ],
            "missing_preferred_skills": result[
                "missing_preferred_skills"
            ],
        }

        match_id = self.repository.save_job_match(
            user_id=user_id,
            job_id=job_id,
            match_score=result["match_score"],
            analysis=analysis,
        )

        return {
            "match_id": match_id,
            "user_id": user_id,
            "job_id": job_id,
            "match_score": result["match_score"],
            **result,
        }

