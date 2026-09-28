
from datetime import datetime, timezone

from app.database.connection import get_connection


class JobRepository:

    def job_exists(self, source_url: str) -> bool:
        """
        Check whether a job has already been imported.
        """

        if not source_url:
            return False

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

                return cursor.fetchone() is not None

        finally:
            connection.close()

    def get_or_create_company(
        self,
        name: str,
        location: str | None = None,
    ) -> int:

        connection = get_connection()

        try:
            with connection.cursor() as cursor:

                cursor.execute(
                    """
                    SELECT "Id"
                    FROM "Companies"
                    WHERE LOWER("Name") = LOWER(%s)
                    LIMIT 1;
                    """,
                    (name,),
                )

                existing = cursor.fetchone()

                if existing:
                    return existing[0]

                cursor.execute(
                    """
                    INSERT INTO "Companies"
                    (
                        "Name",
                        "Location"
                    )
                    VALUES (%s, %s)
                    RETURNING "Id";
                    """,
                    (name, location),
                )

                company_id = cursor.fetchone()[0]

                connection.commit()

                return company_id

        except Exception:
            connection.rollback()
            raise

        finally:
            connection.close()

    def get_or_create_skill(
        self,
        name: str,
    ) -> int:

        connection = get_connection()

        try:
            with connection.cursor() as cursor:

                cursor.execute(
                    """
                    SELECT "Id"
                    FROM "Skills"
                    WHERE LOWER("Name") = LOWER(%s)
                    LIMIT 1;
                    """,
                    (name,),
                )

                existing = cursor.fetchone()

                if existing:
                    return existing[0]

                cursor.execute(
                    """
                    INSERT INTO "Skills"
                    (
                        "Name"
                    )
                    VALUES (%s)
                    RETURNING "Id";
                    """,
                    (name,),
                )

                skill_id = cursor.fetchone()[0]

                connection.commit()

                return skill_id

        except Exception:
            connection.rollback()
            raise

        finally:
            connection.close()

    def create_job(
        self,
        title: str,
        description: str,
        location: str | None,
        employment_type: str | None,
        posted_date: datetime | None,
        source: str,
        source_url: str,
        company_id: int,
    ) -> int:

        connection = get_connection()

        try:
            with connection.cursor() as cursor:

                cursor.execute(
                    """
                    INSERT INTO "Jobs"
                    (
                        "Title",
                        "Description",
                        "Location",
                        "EmploymentType",
                        "PostedDate",
                        "Source",
                        "SourceUrl",
                        "CompanyId"
                    )
                    VALUES
                    (
                        %s,
                        %s,
                        %s,
                        %s,
                        %s,
                        %s,
                        %s,
                        %s
                    )
                    RETURNING "Id";
                    """,
                    (
                        title,
                        description,
                        location,
                        employment_type,
                        posted_date,
                        source,
                        source_url,
                        company_id,
                    ),
                )

                job_id = cursor.fetchone()[0]

                connection.commit()

                return job_id

        except Exception:
            connection.rollback()
            raise

        finally:
            connection.close()

    def create_job_skill(
        self,
        job_id: int,
        skill_id: int,
        is_required: bool,
    ) -> None:

        connection = get_connection()

        try:
            with connection.cursor() as cursor:

                cursor.execute(
                    """
                    INSERT INTO "JobSkills"
                    (
                        "JobId",
                        "SkillId",
                        "IsRequired"
                    )
                    VALUES (%s, %s, %s)
                    ON CONFLICT ("JobId", "SkillId")
                    DO NOTHING;
                    """,
                    (
                        job_id,
                        skill_id,
                        is_required,
                    ),
                )

                connection.commit()

        except Exception:
            connection.rollback()
            raise

        finally:
            connection.close()

    @staticmethod
    def convert_timestamp(timestamp) -> datetime | None:
        """
        Convert Unix timestamp from Arbeitnow
        into a UTC datetime.
        """

        if not timestamp:
            return None

        return datetime.fromtimestamp(
            timestamp,
            tz=timezone.utc,
        )

