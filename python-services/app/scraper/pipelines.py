
from itemadapter import ItemAdapter

from app.database.repositories.job_repository import JobRepository


class PostgreSQLJobPipeline:
    """
    Saves scraped jobs and their skills into PostgreSQL.
    """

    def open_spider(self, spider):
        self.repository = JobRepository()

        spider.logger.info(
            "PostgreSQL job pipeline started."
        )

    def process_item(self, item, spider):

        adapter = ItemAdapter(item)

        source_url = adapter.get("source_url")

        # Prevent duplicate jobs.
        if self.repository.job_exists(source_url):
            spider.logger.info(
                "Skipping duplicate job: %s",
                adapter.get("title"),
            )

            return item

        company_name = (
            adapter.get("company")
            or "Unknown Company"
        )

        location = adapter.get("location")

        # 1. Company
        company_id = self.repository.get_or_create_company(
            name=company_name,
            location=location,
        )

        # 2. Posted date
        posted_date = self.repository.convert_timestamp(
            adapter.get("created_at")
        )

        # 3. Employment type
        job_types = adapter.get("job_types") or []

        employment_type = (
            job_types[0]
            if job_types
            else None
        )

        # 4. Job
        job_id = self.repository.create_job(
            title=adapter.get("title") or "",
            description=adapter.get("description") or "",
            location=location,
            employment_type=employment_type,
            posted_date=posted_date,
            source="arbeitnow.com",
            source_url=source_url or "",
            company_id=company_id,
        )

        analysis = adapter.get("analysis") or {}

        required_skills = (
            analysis.get("required_skills") or []
        )

        preferred_skills = (
            analysis.get("preferred_skills") or []
        )

        # 5. Required skills
        for skill_name in required_skills:

            skill_id = self.repository.get_or_create_skill(
                skill_name
            )

            self.repository.create_job_skill(
                job_id=job_id,
                skill_id=skill_id,
                is_required=True,
            )

        # 6. Preferred skills
        for skill_name in preferred_skills:

            skill_id = self.repository.get_or_create_skill(
                skill_name
            )

            self.repository.create_job_skill(
                job_id=job_id,
                skill_id=skill_id,
                is_required=False,
            )

        spider.logger.info(
            "Saved job '%s' with ID %s",
            adapter.get("title"),
            job_id,
        )

        return item

