import json

from app.database.connection import get_connection
from app.database.repositories.job_repository import JobRepository


PNET_SOURCE = "pnet.co.za"


def clean_skill_names(values) -> list[str]:
    """
    Normalize a list of skill names while removing duplicates.
    """

    if not values:
        return []

    if isinstance(values, str):
        values = [values]

    result = []

    for value in values:

        if not value:
            continue

        value = str(value).strip()

        if not value:
            continue

        # Case-insensitive duplicate protection.
        if any(
            existing.lower() == value.lower()
            for existing in result
        ):
            continue

        result.append(value)

    return result


def get_existing_job_id(
    source_url: str,
) -> int | None:
    """
    Find an existing job by its source URL.
    """

    if not source_url:
        return None

    connection = get_connection()

    try:
        with connection.cursor() as cursor:

            cursor.execute(
                """
                SELECT "Id"
                FROM "Jobs"
                WHERE "SourceUrl" = %s
                LIMIT 1;
                """,
                (source_url,),
            )

            row = cursor.fetchone()

            if row:
                return row[0]

            return None

    finally:
        connection.close()


def save_job_skills(
    repository: JobRepository,
    job_id: int,
    analysis: dict,
) -> int:
    """
    Persist required, preferred and detected keyword skills.

    Required skills are inserted first so that if the same skill
    appears in another list, it retains required status.
    """

    required_skills = clean_skill_names(
        analysis.get(
            "required_skills",
            [],
        )
    )

    preferred_skills = clean_skill_names(
        analysis.get(
            "preferred_skills",
            [],
        )
    )

    keyword_skills = clean_skill_names(
        analysis.get(
            "keywords",
            [],
        )
    )

    required_lookup = {
        skill.lower()
        for skill in required_skills
    }

    preferred_lookup = {
        skill.lower()
        for skill in preferred_skills
    }

    saved_count = 0

    # -----------------------------------------
    # Required skills
    # -----------------------------------------

    for skill_name in required_skills:

        skill_id = (
            repository.get_or_create_skill(
                skill_name
            )
        )

        repository.create_job_skill(
            job_id=job_id,
            skill_id=skill_id,
            is_required=True,
        )

        saved_count += 1

    # -----------------------------------------
    # Preferred skills
    # -----------------------------------------

    for skill_name in preferred_skills:

        # Don't duplicate something already classified
        # as required.
        if (
            skill_name.lower()
            in required_lookup
        ):
            continue

        skill_id = (
            repository.get_or_create_skill(
                skill_name
            )
        )

        repository.create_job_skill(
            job_id=job_id,
            skill_id=skill_id,
            is_required=False,
        )

        saved_count += 1

    # -----------------------------------------
    # Keyword skills
    # -----------------------------------------
    #
    # The deterministic scraper detects skills into
    # analysis["keywords"] even when it cannot confidently
    # classify them as required or preferred.
    #
    # We still want those skills attached to the job so
    # the matching engine has useful skill information.
    # -----------------------------------------

    for skill_name in keyword_skills:

        normalized = skill_name.lower()

        if (
            normalized in required_lookup
            or normalized in preferred_lookup
        ):
            continue

        skill_id = (
            repository.get_or_create_skill(
                skill_name
            )
        )

        repository.create_job_skill(
            job_id=job_id,
            skill_id=skill_id,
            is_required=False,
        )

        saved_count += 1

    return saved_count


def import_jobs():

    try:
        with open(
            "pnet_jobs.json",
            "r",
            encoding="utf-8",
        ) as file:

            jobs = json.load(file)

    except FileNotFoundError:

        print(
            "ERROR: pnet_jobs.json "
            "was not found."
        )

        return

    except json.JSONDecodeError as exception:

        print(
            "ERROR: pnet_jobs.json "
            f"contains invalid JSON: {exception}"
        )

        return

    repository = JobRepository()

    saved = 0
    updated = 0
    skipped = 0
    skills_added = 0

    print(
        f"Found {len(jobs)} jobs "
        "in pnet_jobs.json."
    )

    for index, job in enumerate(
        jobs,
        start=1,
    ):

        title = (
            job.get("title")
            or "Untitled Job"
        )

        print(
            f"\n[{index}/{len(jobs)}] "
            f"{title}"
        )

        source_url = (
            job.get("source_url")
            or ""
        ).strip()

        if not source_url:

            skipped += 1

            print(
                "  Skipped: missing source URL."
            )

            continue

        analysis = (
            job.get("analysis")
            or {}
        )

        # -----------------------------------------
        # Check whether the job already exists.
        # -----------------------------------------

        existing_job_id = (
            get_existing_job_id(
                source_url
            )
        )

        if existing_job_id is not None:

            # The job already exists, but it may have
            # been imported before skill persistence was
            # implemented.
            added = save_job_skills(
                repository,
                existing_job_id,
                analysis,
            )

            skills_added += added
            updated += 1

            print(
                f"  Existing Job ID: "
                f"{existing_job_id}"
            )

            print(
                f"  Skill links processed: "
                f"{added}"
            )

            continue

        # -----------------------------------------
        # Company
        # -----------------------------------------

        company_name = (
            job.get("company")
            or "Unknown Company"
        )

        location = (
            job.get("location")
            or None
        )

        company_id = (
            repository.get_or_create_company(
                name=company_name,
                location=location,
            )
        )

        # -----------------------------------------
        # Employment type
        # -----------------------------------------

        job_types = (
            job.get("job_types")
            or []
        )

        if isinstance(
            job_types,
            str,
        ):
            job_types = [job_types]

        employment_type = (
            job_types[0]
            if job_types
            else None
        )

        # -----------------------------------------
        # Create Job
        # -----------------------------------------

        job_id = repository.create_job(
            title=title,
            description=(
                job.get("description")
                or ""
            ),
            location=location,
            employment_type=employment_type,
            posted_date=None,
            source=(
                job.get("source")
                or PNET_SOURCE
            ),
            source_url=source_url,
            company_id=company_id,
        )

        saved += 1

        print(
            f"  Saved as Job ID {job_id}"
        )

        # -----------------------------------------
        # Skills
        # -----------------------------------------

        added = save_job_skills(
            repository,
            job_id,
            analysis,
        )

        skills_added += added

        print(
            f"  Skills processed: {added}"
        )

    print(
        "\n==================================="
    )

    print(
        f"New jobs saved:       {saved}"
    )

    print(
        f"Existing jobs updated:{updated}"
    )

    print(
        f"Skipped:              {skipped}"
    )

    print(
        f"Skill links processed:{skills_added}"
    )

    print(
        "==================================="
    )


if __name__ == "__main__":
    import_jobs()