
import json

from app.database.connection import get_connection


class MatchingRepository:
    """
    Handles database operations required by the matching system.
    """
    def get_jobs_with_skills(
        self,
        location: str | None = None,
    ) -> list[dict]:

        connection = get_connection()

        try:
            with connection.cursor() as cursor:

                if location:
                    cursor.execute(
                        """
                        SELECT
                            j."Id",
                            j."Title",
                            j."Description",
                            j."Location",
                            j."EmploymentType",
                            j."SalaryMin",
                            j."SalaryMax",
                            j."PostedDate",
                            j."Source",
                            j."SourceUrl",
                            c."Name",
                            s."Name",
                            js."IsRequired"
                        FROM "Jobs" j
                        INNER JOIN "Companies" c
                            ON j."CompanyId" = c."Id"
                        LEFT JOIN "JobSkills" js
                            ON j."Id" = js."JobId"
                        LEFT JOIN "Skills" s
                            ON js."SkillId" = s."Id"
                        WHERE LOWER(j."Location")
                            LIKE LOWER(%s)
                        ORDER BY j."Id";
                        """,
                        (f"%{location}%",),
                    )

                else:
                    cursor.execute(
                        """
                        SELECT
                            j."Id",
                            j."Title",
                            j."Description",
                            j."Location",
                            j."EmploymentType",
                            j."SalaryMin",
                            j."SalaryMax",
                            j."PostedDate",
                            j."Source",
                            j."SourceUrl",
                            c."Name",
                            s."Name",
                            js."IsRequired"
                        FROM "Jobs" j
                        INNER JOIN "Companies" c
                            ON j."CompanyId" = c."Id"
                        LEFT JOIN "JobSkills" js
                            ON j."Id" = js."JobId"
                        LEFT JOIN "Skills" s
                            ON js."SkillId" = s."Id"
                        ORDER BY j."Id";
                        """
                    )

                rows = cursor.fetchall()

                jobs = {}

                for row in rows:

                    job_id = row[0]

                    if job_id not in jobs:
                        jobs[job_id] = {
                        "id": row[0],
                        "title": row[1],
                        "description": row[2],
                        "location": row[3],
                        "employment_type": row[4],
                        "salary_min": row[5],
                        "salary_max": row[6],
                        "posted_date": row[7],
                        "source": row[8],
                        "source_url": row[9],
                        "company": row[10],
                        "required_skills": [],
                        "preferred_skills": [],
                        }

                    skill_name = row[11]
                    is_required = row[12]

                    if skill_name:

                        if is_required:
                            jobs[job_id]["required_skills"].append(
                                skill_name
                            )
                        else:
                            jobs[job_id]["preferred_skills"].append(
                                skill_name
                            )

                return list(jobs.values())

        finally:
            connection.close()
    def get_job_ids_by_location(
        self,
        location: str | None = None,
    ) -> list[int]:
        connection = get_connection()

        try:
            with connection.cursor() as cursor:
                if location:
                    cursor.execute(
                        """
                        SELECT "Id"
                        FROM "Jobs"
                        WHERE LOWER("Location")
                            LIKE LOWER(%s)
                        ORDER BY "Id";
                        """,
                        (f"%{location}%",),
                    )
                else:
                    cursor.execute(
                        """
                        SELECT "Id"
                        FROM "Jobs"
                        ORDER BY "Id";
                        """
                    )

                rows = cursor.fetchall()
                return [row[0] for row in rows]

        finally:
            connection.close()
    def get_all_job_ids(self) -> list[int]:
        """
        Return all job IDs currently stored in PostgreSQL.
        """

        connection = get_connection()

        try:
            with connection.cursor() as cursor:

                cursor.execute(
                    """
                    SELECT "Id"
                    FROM "Jobs"
                    ORDER BY "Id";
                    """
                )

                rows = cursor.fetchall()

                return [row[0] for row in rows]

        finally:
            connection.close()

    def get_job_details(self, job_id: int) -> dict | None:
        """
        Return basic information about a job.
        """

        connection = get_connection()

        try:
            with connection.cursor() as cursor:

                cursor.execute(
                    """
                    SELECT
                        j."Id",
                        j."Title",
                        j."Description",
                        j."Location",
                        j."EmploymentType",
                        j."SalaryMin",
                        j."SalaryMax",
                        j."PostedDate",
                        j."Source",
                        j."SourceUrl",
                        c."Name"
                    FROM "Jobs" j
                    INNER JOIN "Companies" c
                        ON j."CompanyId" = c."Id"
                    WHERE j."Id" = %s;
                    """,
                    (job_id,),
                )

                row = cursor.fetchone()

                if not row:
                    return None

                return {
                    "id": row[0],
                    "title": row[1],
                    "description": row[2],
                    "location": row[3],
                    "employment_type": row[4],
                    "salary_min": row[5],
                    "salary_max": row[6],
                    "posted_date": row[7],
                    "source": row[8],
                    "source_url": row[9],
                    "company": row[10],
                }

        finally:
            connection.close()


    def get_user_skills(self, user_id: int) -> list[str]:
        connection = get_connection()

        try:
            with connection.cursor() as cursor:

                cursor.execute(
                    """
                    SELECT s."Name"
                    FROM "UserSkills" us
                    INNER JOIN "Skills" s
                        ON us."SkillId" = s."Id"
                    WHERE us."UserId" = %s
                    ORDER BY s."Name";
                    """,
                    (user_id,),
                )

                rows = cursor.fetchall()

                return [row[0] for row in rows]

        finally:
            connection.close()

    def get_job_skills(
        self,
        job_id: int,
    ) -> dict:

        connection = get_connection()

        try:
            with connection.cursor() as cursor:

                cursor.execute(
                    """
                    SELECT
                        s."Name",
                        js."IsRequired"
                    FROM "JobSkills" js
                    INNER JOIN "Skills" s
                        ON js."SkillId" = s."Id"
                    WHERE js."JobId" = %s
                    ORDER BY s."Name";
                    """,
                    (job_id,),
                )

                rows = cursor.fetchall()

                required_skills = []
                preferred_skills = []

                for skill_name, is_required in rows:

                    if is_required:
                        required_skills.append(skill_name)
                    else:
                        preferred_skills.append(skill_name)

                return {
                    "required_skills": required_skills,
                    "preferred_skills": preferred_skills,
                }

        finally:
            connection.close()

    def save_job_match(
        self,
        user_id: int,
        job_id: int,
        match_score: float,
        analysis: dict,
    ) -> int:

        connection = get_connection()

        try:
            with connection.cursor() as cursor:

                cursor.execute(
                    """
                    SELECT "Id"
                    FROM "JobMatches"
                    WHERE "UserId" = %s
                      AND "JobId" = %s
                    LIMIT 1;
                    """,
                    (
                        user_id,
                        job_id,
                    ),
                )

                existing = cursor.fetchone()

                analysis_json = json.dumps(
                    analysis
                )

                if existing:

                    match_id = existing[0]

                    cursor.execute(
                        """
                        UPDATE "JobMatches"
                        SET
                            "MatchScore" = %s,
                            "Analysis" = %s,
                            "CreatedAt" = CURRENT_TIMESTAMP
                        WHERE "Id" = %s;
                        """,
                        (
                            match_score,
                            analysis_json,
                            match_id,
                        ),
                    )

                else:

                    cursor.execute(
                        """
                        INSERT INTO "JobMatches"
                        (
                            "UserId",
                            "JobId",
                            "MatchScore",
                            "Analysis",
                            "CreatedAt"
                        )
                        VALUES
                        (
                            %s,
                            %s,
                            %s,
                            %s,
                            CURRENT_TIMESTAMP
                        )
                        RETURNING "Id";
                        """,
                        (
                            user_id,
                            job_id,
                            match_score,
                            analysis_json,
                        ),
                    )

                    match_id = cursor.fetchone()[0]

                connection.commit()

                return match_id

        except Exception:
            connection.rollback()
            raise

        finally:
            connection.close()

