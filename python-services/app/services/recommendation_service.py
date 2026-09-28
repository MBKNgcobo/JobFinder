from app.agents.matching_agent import MatchingAgent
from app.database.repositories.matching_repository import MatchingRepository


class RecommendationService:

    def __init__(self):
        self.repository = MatchingRepository()
        self.agent = MatchingAgent()

    def generate_recommendations(
        self,
        user_id: int,
        minimum_score: float = 0.0,
        limit: int = 20,
        preferred_location: str | None = None,
    ) -> list[dict]:

        print(
            f"Generating recommendations "
            f"for user {user_id}..."
        )

        user_skills = self.repository.get_user_skills(
            user_id
        )

        print(
            f"User has {len(user_skills)} skills."
        )

        jobs = self.repository.get_jobs_with_skills(
            preferred_location
        )

        print(
            f"Loaded {len(jobs)} jobs with one database query."
        )

        recommendations = []

        for index, job in enumerate(jobs, start=1):

            print(
                f"Matching job "
                f"{index}/{len(jobs)}..."
            )

            result = self.agent.match(
                user_skills=user_skills,
                required_skills=job["required_skills"],
                preferred_skills=job["preferred_skills"],
            )

            score = result["match_score"]

            if score < minimum_score:
                continue

            recommendations.append({
                "job_id": job["id"],
                "title": job["title"],
                "company": job["company"],
                "location": job["location"],
                "employment_type": job["employment_type"],
                "salary_min": job["salary_min"],
                "salary_max": job["salary_max"],
                "posted_date": job["posted_date"],
                "source": job["source"],
                "source_url": job["source_url"],

                "match_score": score,

                "matched_required_skills":
                    result["matched_required_skills"],

                "missing_required_skills":
                    result["missing_required_skills"],

                "matched_preferred_skills":
                    result["matched_preferred_skills"],

                "missing_preferred_skills":
                    result["missing_preferred_skills"],
            })

        recommendations.sort(
            key=lambda job: job["match_score"],
            reverse=True,
        )

        recommendations = recommendations[:limit]

        print(
            f"Saving {len(recommendations)} "
            f"recommendations..."
        )

        for recommendation in recommendations:

            analysis = {
                "user_skills": user_skills,

                "matched_required_skills":
                    recommendation[
                        "matched_required_skills"
                    ],

                "missing_required_skills":
                    recommendation[
                        "missing_required_skills"
                    ],

                "matched_preferred_skills":
                    recommendation[
                        "matched_preferred_skills"
                    ],

                "missing_preferred_skills":
                    recommendation[
                        "missing_preferred_skills"
                    ],
            }

            match_id = self.repository.save_job_match(
                user_id=user_id,
                job_id=recommendation["job_id"],
                match_score=recommendation["match_score"],
                analysis=analysis,
            )

            recommendation["match_id"] = match_id

        print(
            f"Generated "
            f"{len(recommendations)} recommendations."
        )

        return recommendations