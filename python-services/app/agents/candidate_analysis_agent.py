from typing import Any

from app.agents.base_agent import BaseAgent


class CandidateAnalysisAgent(BaseAgent):
    name = "candidate_analysis_agent"

    async def run(
        self,
        input_data: dict[str, Any],
    ) -> dict[str, Any]:


        first_name = input_data.get("first_name", "")
        last_name = input_data.get("last_name", "")

        skills = input_data.get("skills", [])
        projects = input_data.get("projects", [])

        project_analysis = []

        for project in projects:
            project_skills = project.get("skills", [])

            project_analysis.append({
                "name": project.get("name", ""),
                "skills": project_skills,
                "evidence": (
                    f"Practical project exposure to: "
                    f"{', '.join(project_skills)}."
                    if project_skills
                    else "Project experience provided by candidate."
                ),
            })

        return {
            "name": f"{first_name} {last_name}".strip(),
            "summary": input_data.get("summary") or "",
            "years_of_experience": input_data.get(
                "years_of_experience",
                0,
            ),
            "skills": skills,
            "projects": project_analysis,
            "strengths": skills,
            "evidence_items": [
                f"Skill: {skill}"
                for skill in skills
            ],
        }