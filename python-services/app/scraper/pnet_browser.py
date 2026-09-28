import asyncio
import json
from urllib.parse import urljoin

from playwright.async_api import async_playwright

from app.agents.job_analysis_agent import JobAnalysisAgent
from app.llm.llm_service import LLMService


PNET_URL = (
    "https://www.pnet.co.za/jobs/"
    "software-developer/in-south-africa"
)


def clean_text(
    value: str | None,
) -> str:

    if not value:
        return ""

    return " ".join(
        value.split()
    )


async def get_job_urls(page):

    articles = page.locator(
        'article[data-testid="job-item"]'
    )

    count = await articles.count()

    print(
        f"Found {count} job cards.",
        flush=True,
    )

    job_urls = []

    seen_urls = set()

    for index in range(count):

        article = articles.nth(index)

        title_link = article.locator(
            'a[data-testid="job-item-title"]'
        ).first

        if await title_link.count() == 0:
            continue

        href = await title_link.get_attribute(
            "href"
        )

        if not href:
            continue

        title = clean_text(
            await title_link.inner_text()
        )

        job_url = urljoin(
            PNET_URL,
            href,
        )

        if job_url in seen_urls:
            continue

        seen_urls.add(
            job_url
        )

        job_urls.append(
            {
                "title": title,
                "url": job_url,
            }
        )

    return job_urls


async def extract_full_job(
    page,
    job,
    analysis_agent,
):

    url = job["url"]

    try:

        await page.goto(
            url,
            wait_until="domcontentloaded",
            timeout=60000,
        )

        await page.wait_for_timeout(
            1500
        )

        # =================================
        # TITLE
        # =================================

        title = job["title"]

        title_locator = page.locator(
            "h1"
        ).first

        if await title_locator.count():

            extracted_title = clean_text(
                await title_locator.inner_text()
            )

            if extracted_title:
                title = extracted_title

        # =================================
        # COMPANY
        # =================================

        company = ""

        for selector in [
            '[data-at="job-item-company-name"]',
            '[data-testid="company-name"]',
            ".company-name",
        ]:

            locator = page.locator(
                selector
            ).first

            if await locator.count():

                value = clean_text(
                    await locator.inner_text()
                )

                if value:

                    company = value

                    break

        # =================================
        # LOCATION
        # =================================

        location = ""

        for selector in [
            '[data-at="job-item-location"]',
            '[data-testid="location"]',
            ".job-location",
            ".location",
        ]:

            locator = page.locator(
                selector
            ).first

            if await locator.count():

                value = clean_text(
                    await locator.inner_text()
                )

                if value:

                    location = value

                    break

        # =================================
        # SALARY
        # =================================

        salary = ""

        for selector in [
            '[data-at="job-item-salary-info"]',
            '[data-testid="salary"]',
            ".salary",
        ]:

            locator = page.locator(
                selector
            ).first

            if await locator.count():

                value = clean_text(
                    await locator.inner_text()
                )

                if value:

                    salary = value

                    break

        # =================================
        # DESCRIPTION
        # =================================

        description = ""

        selectors = [
            '[data-at="jobcard-content"]',
            '[data-testid="job-description"]',
            ".job-description",
            ".description",
        ]

        for selector in selectors:

            locator = page.locator(
                selector
            ).first

            if await locator.count():

                value = clean_text(
                    await locator.inner_text()
                )

                if len(value) > 200:

                    description = value

                    break

        # =================================
        # BODY FALLBACK
        # =================================

        if not description:

            body = page.locator(
                "body"
            )

            description = clean_text(
                await body.inner_text()
            )

        # =================================
        # REMOTE
        # =================================

        description_lower = (
            description.lower()
        )

        remote = any(
            term in description_lower
            for term in [
                "remote",
                "work from home",
                "fully remote",
                "remote working",
            ]
        )

        # =================================
        # LLM ANALYSIS
        # =================================

        analysis = {
            "title": title,
            "company": company,
            "summary": description[:1000],
            "required_skills": [],
            "preferred_skills": [],
            "responsibilities": [],
            "experience_requirements": [],
            "education_requirements": [],
            "keywords": [],
        }

        try:

            analysis = await analysis_agent.run(
                {
                    "title": title,
                    "company": company,
                    "description": description,
                    "required_skills": [],
                    "preferred_skills": [],
                }
            )

        except Exception as exception:

            print(
                "[JOB ANALYSIS FALLBACK] "
                f"{type(exception).__name__}: "
                f"{exception}",
                flush=True,
            )

            # IMPORTANT:
            # The job itself is still returned.
            # A failed LLM call must not destroy
            # scraped job data.

        # =================================
        # NORMALIZE ANALYSIS
        # =================================

        if not isinstance(
            analysis,
            dict,
        ):

            analysis = {}

        required_skills = (
            analysis.get(
                "required_skills"
            )
            or []
        )

        preferred_skills = (
            analysis.get(
                "preferred_skills"
            )
            or []
        )

        if not isinstance(
            required_skills,
            list,
        ):

            required_skills = []

        if not isinstance(
            preferred_skills,
            list,
        ):

            preferred_skills = []

        analysis[
            "required_skills"
        ] = required_skills

        analysis[
            "preferred_skills"
        ] = preferred_skills

        # =================================
        # RETURN JOB
        # =================================

        return {
            "source": "pnet.co.za",
            "source_url": url,
            "title": title,
            "company": company,
            "location": location,
            "description": description,
            "salary_text": salary,
            "remote": remote,
            "tags": [],
            "job_types": [],
            "analysis": analysis,
        }

    except Exception as exception:

        print(
            "\nERROR extracting job:",
            flush=True,
        )

        print(
            url,
            flush=True,
        )

        print(
            exception,
            flush=True,
        )

        return None


async def scrape_pnet():

    llm_service = LLMService()

    analysis_agent = JobAnalysisAgent(
        llm_service
    )

    async with async_playwright() as playwright:

        browser = await playwright.chromium.launch(
            headless=True
        )

        search_page = await browser.new_page(
            viewport={
                "width": 1440,
                "height": 900,
            }
        )

        print(
            "Opening PNet...",
            flush=True,
        )

        await search_page.goto(
            PNET_URL,
            wait_until="domcontentloaded",
            timeout=60000,
        )

        await search_page.wait_for_timeout(
            4000
        )

        job_urls = await get_job_urls(
            search_page
        )

        print(
            f"Found {len(job_urls)} "
            "actual job URLs.",
            flush=True,
        )

        job_page = await browser.new_page(
            viewport={
                "width": 1440,
                "height": 900,
            }
        )

        jobs = []

        for index, job in enumerate(
            job_urls,
            start=1,
        ):

            print(
                f"\n[{index}/{len(job_urls)}] "
                f"{job['title']}",
                flush=True,
            )

            result = await extract_full_job(
                job_page,
                job,
                analysis_agent,
            )

            if result:

                jobs.append(
                    result
                )

                print(
                    f"  Company: "
                    f"{result['company']}",
                    flush=True,
                )

                print(
                    f"  Location: "
                    f"{result['location']}",
                    flush=True,
                )

                print(
                    f"  Description length: "
                    f"{len(result['description'])}",
                    flush=True,
                )

                print(
                    f"  Required: "
                    f"{result['analysis']['required_skills']}",
                    flush=True,
                )

                print(
                    f"  Preferred: "
                    f"{result['analysis']['preferred_skills']}",
                    flush=True,
                )

            # Don't hammer PNet or OpenRouter.
            await asyncio.sleep(
                2
            )

        await browser.close()

    # =================================
    # DEDUPLICATE
    # =================================

    unique_jobs = {}

    for job in jobs:

        source_url = job.get(
            "source_url"
        )

        if source_url:
            unique_jobs[
                source_url
            ] = job

    jobs = list(
        unique_jobs.values()
    )

    # =================================
    # SAVE
    # =================================

    with open(
        "pnet_jobs.json",
        "w",
        encoding="utf-8",
    ) as file:

        json.dump(
            jobs,
            file,
            indent=2,
            ensure_ascii=False,
        )

    print(
        "\n====================================",
        flush=True,
    )

    print(
        f"Saved {len(jobs)} "
        "full PNet jobs.",
        flush=True,
    )

    print(
        "File: pnet_jobs.json",
        flush=True,
    )

    print(
        "====================================",
        flush=True,
    )


if __name__ == "__main__":

    asyncio.run(
        scrape_pnet()
    )