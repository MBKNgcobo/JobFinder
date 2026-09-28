from app.database.repositories.job_repository import JobRepository


class PNetJobService:

    def __init__(self):
        self.repository = JobRepository()

    def save_job(self, job: dict) -> int | None:

        source_url = job.get("source_url")

        # Prevent duplicates
        if self.repository.job_exists(source_url):
            return None

        company_name = (
            job.get("company")
            or "Unknown Company"
        )

        location = job.get("location")

        company_id = (
            self.repository.get_or_create_company(
                name=company_name,
                location=location,
            )
        )

        analysis = (
            job.get("analysis")
            or {}
        )

        required_skills = (
            analysis.get("required_skills")
            or []
        )

        preferred_skills = (
            analysis.get("preferred_skills")
            or []
        )

        job_id = self.repository.create_job(
            title=job.get("title") or "",
            description=job.get("description") or "",
            location=location,
            employment_type=None,
            posted_date=None,
            source="pnet.co.za",
            source_url=source_url or "",
            company_id=company_id,
        )

        # Required skills
        for skill_name in required_skills:

            skill_id = (
                self.repository.get_or_create_skill(
                    skill_name
                )
            )

            self.repository.create_job_skill(
                job_id=job_id,
                skill_id=skill_id,
                is_required=True,
            )

        # Preferred skills
        for skill_name in preferred_skills:

            skill_id = (
                self.repository.get_or_create_skill(
                    skill_name
                )
            )

            self.repository.create_job_skill(
                job_id=job_id,
                skill_id=skill_id,
                is_required=False,
            )

        return job_id