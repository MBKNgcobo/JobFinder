class ApplicationAgent:
    """
    Prepares a tailored application based on a user's profile
    and a specific job.

    Deterministic MVP implementation.
    """

    def prepare_application(
        self,
        user_profile: dict,
        job: dict,
    ) -> dict:

        # -------------------------------------------------
        # USER SKILLS
        # -------------------------------------------------

        user_skills = self._normalize(
            user_profile.get("skills", [])
        )

        # -------------------------------------------------
        # JOB SKILLS
        # -------------------------------------------------

        required_skills = self._normalize(
            job.get("required_skills", [])
        )

        preferred_skills = self._normalize(
            job.get("preferred_skills", [])
        )

        # -------------------------------------------------
        # MATCHING
        # -------------------------------------------------

        matched_required = (
            user_skills & required_skills
        )

        matched_preferred = (
            user_skills & preferred_skills
        )

        missing_required = (
            required_skills - user_skills
        )

        # Combine required + preferred matches
        matched_skills = (
            matched_required
            | matched_preferred
        )

        # -------------------------------------------------
        # PROJECT RECOMMENDATIONS
        # -------------------------------------------------

        recommended_projects = (
            self._recommend_projects(
                user_profile.get("projects", []),
                matched_skills,
            )
        )

        # -------------------------------------------------
        # RELEVANT EXPERIENCE
        # -------------------------------------------------

        relevant_experience = (
            self._find_relevant_experience(
                user_profile.get("experience", []),
                matched_skills,
            )
        )

        # -------------------------------------------------
        # CV CHANGES
        # -------------------------------------------------

        cv_changes = self._generate_cv_changes(
            matched_skills,
            missing_required,
        )

        # -------------------------------------------------
        # COVER LETTER
        # -------------------------------------------------

        cover_letter = self._generate_cover_letter(
            user_profile=user_profile,
            job=job,
            matched_skills=matched_skills,
        )

        # -------------------------------------------------
        # RESPONSE
        # -------------------------------------------------

        return {
            "job_title": job.get("title", ""),
            "company": job.get("company", ""),

            # These names MUST match the C# DTO
            "matched_skills": sorted(
                matched_skills
            ),

            "missing_skills": sorted(
                missing_required
            ),

            "recommended_projects":
                recommended_projects,

            "relevant_experience":
                relevant_experience,

            "cover_letter":
                cover_letter,

            "cv_changes":
                cv_changes,
        }

    # =====================================================
    # PROJECTS
    # =====================================================

    def _recommend_projects(
        self,
        projects,
        matched_skills: set[str],
    ) -> list[str]:

        recommendations = []

        for project in projects:

            # Current MVC sends project names as strings.
            if isinstance(project, str):

                if project.strip():
                    recommendations.append(
                        project
                    )

                continue

            # Supports richer project dictionaries later.
            if isinstance(project, dict):

                project_name = (
                    project.get("name")
                    or project.get("title")
                )

                project_skills = self._normalize(
                    project.get("skills", [])
                )

                overlap = (
                    project_skills
                    & matched_skills
                )

                if overlap and project_name:
                    recommendations.append(
                        project_name
                    )

        return recommendations

    # =====================================================
    # EXPERIENCE
    # =====================================================

    def _find_relevant_experience(
        self,
        experiences,
        matched_skills: set[str],
    ) -> list[str]:

        recommendations = []

        for experience in experiences:

            if isinstance(experience, str):

                if experience.strip():
                    recommendations.append(
                        experience
                    )

                continue

            if isinstance(experience, dict):

                role = experience.get("role")
                company = experience.get("company")

                experience_skills = (
                    self._normalize(
                        experience.get(
                            "skills",
                            []
                        )
                    )
                )

                overlap = (
                    experience_skills
                    & matched_skills
                )

                if overlap:

                    if role and company:
                        recommendations.append(
                            f"{role} at {company}"
                        )

                    elif role:
                        recommendations.append(
                            role
                        )

        return recommendations

    # =====================================================
    # CV CHANGES
    # =====================================================

    def _generate_cv_changes(
        self,
        matched_skills: set[str],
        missing_required: set[str],
    ) -> list[str]:

        changes = []

        if matched_skills:

            skills = ", ".join(
                sorted(matched_skills)
            )

            changes.append(
                f"Highlight practical experience "
                f"with: {skills}."
            )

        if missing_required:

            skills = ", ".join(
                sorted(missing_required)
            )

            changes.append(
                "Do not claim missing skills. "
                "Consider demonstrating transferable "
                f"experience related to: {skills}."
            )

        return changes

    # =====================================================
    # COVER LETTER
    # =====================================================

    def _generate_cover_letter(
        self,
        user_profile: dict,
        job: dict,
        matched_skills: set[str],
    ) -> str:

        first_name = (
            user_profile.get(
                "first_name",
                ""
            )
        )

        last_name = (
            user_profile.get(
                "last_name",
                ""
            )
        )

        full_name = (
            f"{first_name} {last_name}"
        ).strip()

        if not full_name:
            full_name = "Candidate"

        job_title = (
            job.get(
                "title",
                "this position"
            )
        )

        company = (
            job.get(
                "company",
                "your company"
            )
        )

        if matched_skills:

            skills_text = ", ".join(
                sorted(matched_skills)
            )

        else:

            skills_text = (
                "relevant technical skills"
            )

        return (
            f"Dear Hiring Manager,\n\n"

            f"My name is {full_name}, and I am "
            f"interested in the {job_title} position "
            f"at {company}. "

            f"My background includes experience "
            f"with {skills_text}, which aligns with "
            f"several of the requirements of this role.\n\n"

            f"I am particularly interested in the "
            f"opportunity to apply my technical "
            f"experience while continuing to develop "
            f"professionally within your organisation.\n\n"

            f"Thank you for considering my application.\n\n"

            f"Kind regards,\n"
            f"{full_name}"
        )

    # =====================================================
    # NORMALIZATION
    # =====================================================

    @staticmethod
    def _normalize(
        items: list[str],
    ) -> set[str]:

        return {
            item.lower().strip()
            for item in items
            if item
            and isinstance(item, str)
            and item.strip()
        }