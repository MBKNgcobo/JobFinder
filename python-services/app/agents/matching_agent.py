class MatchingAgent:

    REQUIRED_WEIGHT = 0.80
    PREFERRED_WEIGHT = 0.20

    def match(
        self,
        user_skills: list[str],
        required_skills: list[str],
        preferred_skills: list[str] | None = None,
    ) -> dict:

        preferred_skills = preferred_skills or []

        user_set = self._normalize(user_skills)
        required_set = self._normalize(required_skills)
        preferred_set = self._normalize(preferred_skills)

        matched_required = (
            user_set & required_set
        )

        missing_required = (
            required_set - user_set
        )

        matched_preferred = (
            user_set & preferred_set
        )

        missing_preferred = (
            preferred_set - user_set
        )

        required_score = (
            self._calculate_percentage(
                matched_required,
                required_set,
            )
        )

        preferred_score = (
            self._calculate_percentage(
                matched_preferred,
                preferred_set,
            )
        )

        # -----------------------------------------
        # CALCULATE SKILL SCORE
        # -----------------------------------------

        if required_set and preferred_set:

            skill_score = (
                required_score
                * self.REQUIRED_WEIGHT
                +
                preferred_score
                * self.PREFERRED_WEIGHT
            )

        elif required_set:

            skill_score = required_score

        elif preferred_set:

            skill_score = preferred_score

        else:

            skill_score = 0.0

        return {
            "match_score": round(skill_score, 2),
            "skill_score": round(skill_score, 2),

            "required_score": round(
                required_score,
                2,
            ),

            "preferred_score": round(
                preferred_score,
                2,
            ),

            "matched_required_skills": sorted(
                matched_required
            ),

            "missing_required_skills": sorted(
                missing_required
            ),

            "matched_preferred_skills": sorted(
                matched_preferred
            ),

            "missing_preferred_skills": sorted(
                missing_preferred
            ),
        }

    @staticmethod
    def _normalize(
        skills: list[str],
    ) -> set[str]:

        return {
            skill.lower().strip()
            for skill in skills
            if skill and skill.strip()
        }

    @staticmethod
    def _calculate_percentage(
        matched: set[str],
        total: set[str],
    ) -> float:

        if not total:
            return 0.0

        return (
            len(matched)
            / len(total)
        ) * 100