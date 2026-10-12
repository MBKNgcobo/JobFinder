# JobFinder

JobFinder is a job discovery, matching, and AI-assisted application-preparation platform: an ASP.NET Core MVC web app, Python agent services with a scraper, and PostgreSQL shared between them.

## Language

**Deterministic Core**:
The parts of JobFinder whose results are fully determined by their inputs — skill matching, candidate/job analysis rules, and schema validation — in contrast to behavior that depends on a live LLM or external job boards.
_Avoid_: pure logic, business logic (as synonyms for this tier)

**CI-Blocking Test**:
A test that runs on every push and pull request and can prevent a merge; it never performs live network calls, using fixtures or stubs instead of the LLM and external job boards.
_Avoid_: smoke test, live test

**Advisory Check**:
A check that runs in CI and reports failures without blocking a merge; the static and security scanning tier runs as advisory checks until they are clean.
_Avoid_: soft gate, lint (as a synonym for the whole scanning tier)

**Manual Regression Checklist**:
The one-page list of flows automation does not cover, to be run before every deployment to the demo host.
_Avoid_: test plan, QA sign-off

**Production Hardening**:
The workstream that makes JobFinder safe to operate on a public host, independent of which hosting platform is chosen.
_Avoid_: ops, DevOps work
