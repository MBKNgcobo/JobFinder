import scrapy
from bs4 import BeautifulSoup

from app.agents.job_analysis_agent import JobAnalysisAgent
from app.llm.llm_service import LLMService

class PnetJobsSpider(scrapy.Spider):
    name = "pnet_jobs"

    allowed_domains = [
        "pnet.co.za",
    ]

    start_urls = [
        "https://www.pnet.co.za/jobs/software-developer"
    ]

    MAX_PAGES = 3

    custom_settings = {
        "DOWNLOAD_DELAY": 3,
        "CONCURRENT_REQUESTS_PER_DOMAIN": 1,
        "AUTOTHROTTLE_ENABLED": True,
        "AUTOTHROTTLE_START_DELAY": 3,
        "AUTOTHROTTLE_MAX_DELAY": 15,
        "AUTOTHROTTLE_TARGET_CONCURRENCY": 1.0,
    }

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)

        self.llm_service = LLMService()
        self.analysis_agent = JobAnalysisAgent(self.llm_service)
        self.pages_processed = 0

    async def parse(self, response):
        self.pages_processed += 1

        self.logger.info(
            "Processing PNet page %s: %s",
            self.pages_processed,
            response.url,
        )

        # We deliberately start conservatively.
        # The selectors may need adjustment after seeing PNet's
        # live HTML from your local Scrapy run.

        job_links = response.css(
            'a[href*="/jobs/"]::attr(href)'
        ).getall()

        seen = set()

        for href in job_links:
            if not href:
                continue

            absolute_url = response.urljoin(href)

            if absolute_url in seen:
                continue

            seen.add(absolute_url)

            yield scrapy.Request(
                url=absolute_url,
                callback=self.parse_job,
            )

        # Do not blindly crawl arbitrary URLs.
        # We only follow the next results page when it is explicitly
        # present in the pagination markup.

        next_page = response.css(
            'a[rel="next"]::attr(href)'
        ).get()

        if (
            next_page
            and self.pages_processed < self.MAX_PAGES
        ):
            yield scrapy.Request(
                url=response.urljoin(next_page),
                callback=self.parse,
            )

    async def parse_job(self, response):
        title = self.extract_first(
            response,
            [
                "h1::text",
                '[data-testid="job-title"]::text',
                ".job-title::text",
            ],
        )

        company = self.extract_first(
            response,
            [
                '[data-testid="company-name"]::text',
                ".company-name::text",
                ".job-company::text",
            ],
        )

        location = self.extract_first(
            response,
            [
                '[data-testid="location"]::text',
                ".job-location::text",
                ".location::text",
            ],
        )

        description = self.extract_description(response)

        if not title or not description:
            self.logger.warning(
                "Could not extract enough information from %s",
                response.url,
            )
            return

        analysis = await self.analysis_agent.run(
    {
        "title": self.clean_text(title),
        "company": self.clean_text(company),
        "description": description,
        "required_skills": [],
        "preferred_skills": [],
    }
)

        yield {
            "source": "pnet.co.za",
            "source_id": response.url,
            "title": self.clean_text(title),
            "company": self.clean_text(company),
            "location": self.clean_text(location),
            "description": description,
            "source_url": response.url,
            "remote": self.detect_remote(description),
            "tags": [],
            "job_types": [],
            "analysis": analysis,
        }

    @staticmethod
    def extract_first(response, selectors):
        for selector in selectors:
            value = response.css(selector).get()

            if value and value.strip():
                return value.strip()

        return ""

    @staticmethod
    def extract_description(response):
        selectors = [
            '[data-testid="job-description"]',
            ".job-description",
            ".description",
            "main",
        ]

        for selector in selectors:
            element = response.css(selector)

            if element:
                text = element.xpath(".//text()").getall()

                cleaned = " ".join(
                    value.strip()
                    for value in text
                    if value.strip()
                )

                if len(cleaned) >= 100:
                    return cleaned

        return ""

    @staticmethod
    def clean_text(value):
        if not value:
            return ""

        soup = BeautifulSoup(
            value,
            "html.parser",
        )

        return " ".join(
            soup.get_text(
                separator=" ",
                strip=True,
            ).split()
        )

    @staticmethod
    def detect_remote(description):
        description_lower = description.lower()

        remote_terms = [
            "remote",
            "work from home",
            "fully remote",
            "remote working",
            "remote position",
        ]

        return any(
            term in description_lower
            for term in remote_terms
        )