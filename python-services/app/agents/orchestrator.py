import asyncio
from typing import Any

from app.agents.candidate_analysis_agent import CandidateAnalysisAgent
from app.agents.cv_tailoring_agent import CvTailoringAgent
from app.agents.cover_letter_agent import CoverLetterAgent
from app.agents.job_analysis_agent import JobAnalysisAgent
from app.agents.matching_agent import MatchingAgent
from app.agents.project_selection_agent import ProjectSelectionAgent
from app.llm.llm_service import LLMService
from app.agents.critic_agent import CriticAgent
from app.agents.revision_agent import RevisionAgent

class AgentOrchestrator:
    def __init__(self):
        self.llm_service = LLMService()

        self.job_analysis_agent = JobAnalysisAgent(self.llm_service)

        self.candidate_analysis_agent = CandidateAnalysisAgent()

        self.matching_agent = MatchingAgent()

        self.critic_agent = CriticAgent(self.llm_service)

        self.revision_agent = RevisionAgent(self.llm_service)

        self.project_selection_agent = ProjectSelectionAgent(
            self.llm_service
        )
        self.cv_tailoring_agent = CvTailoringAgent(
            self.llm_service
        )
        self.cover_letter_agent = CoverLetterAgent(
            self.llm_service
        )

    async def _run_agent(self, name: str, coroutine):
        print(f"[AGENT START] {name}")

        try:
            result = await coroutine
            print(f"[AGENT DONE] {name}")
            return result

        except Exception as exception:
            print(f"[AGENT ERROR] {name}: {exception}")
            raise

    async def prepare_application(
        self,
        user_profile: dict[str, Any],
        job: dict[str, Any],
    ) -> dict[str, Any]:

        # =====================================================
        # ROUND 1
        # Job + Candidate analysis run simultaneously
        # =====================================================

        job_analysis, candidate_analysis = await asyncio.gather(
            self._run_agent(
                "job_analysis",
                self.job_analysis_agent.run(job)
            ),
            self._run_agent(
                "candidate_analysis",
                self.candidate_analysis_agent.run(user_profile)
            ),
        )

        # =====================================================
        # ROUND 2
        # Deterministic matching
        # =====================================================

        matching = self.matching_agent.match(
            user_skills=user_profile.get("skills", []),
            required_skills=job_analysis.get(
                "required_skills",
                []
            ),
            preferred_skills=job_analysis.get(
                "preferred_skills",
                []
            ),
        )

        # =====================================================
        # ROUND 3
        # Project selection + CV tailoring run simultaneously
        #
        # CV receives the complete candidate project list rather
        # than waiting for project selection.
        # =====================================================

        project_selection, cv_tailoring = await asyncio.gather(
            self._run_agent(
                "project_selection",
                self.project_selection_agent.run({
                    "candidate": candidate_analysis,
                    "job": job_analysis,
                    "matching": matching,
                })
            ),

            self._run_agent(
                "cv_tailoring",
                self.cv_tailoring_agent.run({
                    "candidate": candidate_analysis,
                    "job": job_analysis,
                    "matching": matching,
                    "projects": candidate_analysis.get(
                        "projects",
                        []
                    ),
                })
            ),
        )

        # =====================================================
        # ROUND 4
        # Cover letter uses everything produced above
        # =====================================================

        cover_letter = await self._run_agent(
            "cover_letter",
            self.cover_letter_agent.run({
                "candidate": candidate_analysis,
                "job": job_analysis,
                "matching": matching,
                "projects": project_selection,
                "cv": cv_tailoring,
            })
        )

                # ---------------------------------------------------------
        # FIRST CRITIC
        # ---------------------------------------------------------

        critic = await self._run_agent(
            "critic",
            self.critic_agent.run({
                "candidate": candidate_analysis,
                "job": job_analysis,
                "matching": matching,
                "projects": project_selection,
                "cv": cv_tailoring,
                "cover_letter": cover_letter,
            })
        )

        print(
            f"[CRITIC 1 RESULT] "
            f"approved={critic.get('approved', False)}"
        )

        if critic.get("issues"):
            print("[CRITIC 1 ISSUES]")
            for issue in critic["issues"]:
                print(f" - {issue}")

        if critic.get("unsupported_claims"):
            print("[CRITIC 1 UNSUPPORTED CLAIMS]")
            for claim in critic["unsupported_claims"]:
                print(f" - {claim}")

        # ---------------------------------------------------------
        # REVISION
        # ---------------------------------------------------------

        final_application = {
            "cover_letter": cover_letter.get(
                "cover_letter",
                ""
            ),
            "cv_changes": cv_tailoring.get(
                "cv_changes",
                []
            ),
            "recommended_projects": project_selection.get(
                "recommended_projects",
                []
            ),
        }

        if not critic.get("approved", False):

            print(
                "[REVISION REQUIRED] "
                "Critic rejected application."
            )

            final_application = await self._run_agent(
                "revision",
                self.revision_agent.run({
                    "candidate": candidate_analysis,
                    "job": job_analysis,
                    "matching": matching,
                    "projects": project_selection,
                    "cv": cv_tailoring,
                    "cover_letter": cover_letter,
                    "critic": critic,
                })
            )

            print("[REVISION COMPLETE] Application revised.")

        else:

            print(
                "[REVISION NOT REQUIRED] "
                "Application approved."
            )

        # ---------------------------------------------------------
        # SECOND CRITIC
        # ---------------------------------------------------------

        final_critic = await self._run_agent(
            "final_critic",
            self.critic_agent.run({
                "candidate": candidate_analysis,
                "job": job_analysis,
                "matching": matching,
                "projects": final_application.get(
                    "recommended_projects",
                    []
                ),
                "cv": {
                    "cv_changes": final_application.get(
                        "cv_changes",
                        []
                    )
                },
                "cover_letter": {
                    "cover_letter": final_application.get(
                        "cover_letter",
                        ""
                    )
                },
            })
        )

        print(
            f"[FINAL CRITIC RESULT] "
            f"approved={final_critic.get('approved', False)}"
        )

        if final_critic.get("issues"):
            print("[FINAL CRITIC ISSUES]")
            for issue in final_critic["issues"]:
                print(f" - {issue}")

        if final_critic.get("unsupported_claims"):
            print("[FINAL CRITIC UNSUPPORTED CLAIMS]")
            for claim in final_critic["unsupported_claims"]:
                print(f" - {claim}")
        else:
            print("[REVISION NOT REQUIRED] Application approved.")

        print(
            f"[CRITIC RESULT] "
            f"approved={critic.get('approved', False)}"
        )

        if critic.get("issues"):
            print("[CRITIC ISSUES]")
            for issue in critic["issues"]:
                print(f" - {issue}")

        if critic.get("unsupported_claims"):
            print("[CRITIC UNSUPPORTED CLAIMS]")
            for claim in critic["unsupported_claims"]:
                print(f" - {claim}")

        # =====================================================
        # FINAL APPLICATION PACKAGE
        # =====================================================

        return {
            "job_title": job_analysis.get(
                "title",
                job.get("title", "")
            ),

            "company": job_analysis.get(
                "company",
                job.get("company", "")
            ),

            "matched_skills": (
                matching.get(
                    "matched_required_skills",
                    []
                )
                +
                matching.get(
                    "matched_preferred_skills",
                    []
                )
            ),

            "missing_skills": matching.get(
                "missing_required_skills",
                []
            ),

            "recommended_projects": final_application.get(
                "recommended_projects",
                []
            ),

            "relevant_experience": candidate_analysis.get(
                "evidence_items",
                []
            ),

            "cover_letter": final_application.get(
                "cover_letter",
                ""
            ),

            "cv_changes": final_application.get(
                "cv_changes",
                []
            ),

            # Preserve the complete agent outputs.
            "critic": final_critic,
            "initial_critic": critic,
            "job_analysis": job_analysis,
            "candidate_analysis": candidate_analysis,
            "matching": matching,
            "project_selection": project_selection,
            "cv_tailoring": cv_tailoring,
            "cover_letter_agent": cover_letter,
        }