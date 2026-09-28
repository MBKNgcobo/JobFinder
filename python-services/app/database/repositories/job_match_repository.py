
import json
from datetime import datetime, timezone

from app.database.connection import get_connection


class JobMatchRepository:

    def save_match(
        self,
        user_id: int,
        job_id: int,
        match_score: float,
        analysis: dict
    ) -> int:

        connection = get_connection()

        try:
            cursor = connection.cursor()

            analysis_json = json.dumps(
                analysis,
                ensure_ascii=False
            )

            now = datetime.now(timezone.utc)

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
                VALUES (%s, %s, %s, %s, %s)

                ON CONFLICT ("UserId", "JobId")
                DO UPDATE SET
                    "MatchScore" = EXCLUDED."MatchScore",
                    "Analysis" = EXCLUDED."Analysis",
                    "CreatedAt" = EXCLUDED."CreatedAt"

                RETURNING "Id"
                """,
                (
                    user_id,
                    job_id,
                    match_score,
                    analysis_json,
                    now
                )
            )

            match_id = cursor.fetchone()[0]

            connection.commit()

            return match_id

        except Exception:
            connection.rollback()
            raise

        finally:
            connection.close()

