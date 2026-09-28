import re

import scrapy
from bs4 import BeautifulSoup


SKILL_PATTERNS = {
    "Python": [
        r"\bpython\b",
    ],
    "C#": [
        r"\bc#\b",
        r"\bc sharp\b",
    ],
    "C++": [
        r"\bc\+\+\b",
    ],
    "Java": [
        r"\bjava\b",
    ],
    "JavaScript": [
        r"\bjavascript\b",
        r"\bjava script\b",
    ],
    "TypeScript": [
        r"\btypescript\b",
    ],
    "SQL": [
        r"\bsql\b",
    ],
    "PostgreSQL": [
        r"\bpostgresql\b",
        r"\bpostgres\b",
    ],
    "MySQL": [
        r"\bmysql\b",
    ],
    "MongoDB": [
        r"\bmongodb\b",
    ],
    "ASP.NET": [
        r"\basp\.net\b",
        r"\basp net\b",
    ],
    "ASP.NET Core": [
        r"\basp\.net core\b",
        r"\basp net core\b",
    ],
    ".NET": [
        r"\.net\b",
        r"\bdotnet\b",
    ],
    "FastAPI": [
        r"\bfastapi\b",
    ],
    "Django": [
        r"\bdjango\b",
    ],
    "Flask": [
        r"\bflask\b",
    ],
    "React": [
        r"\breact(?:\.js|js)?\b",
    ],
    "Angular": [
        r"\bangular\b",
    ],
    "Vue.js": [
        r"\bvue(?:\.js|js)?\b",
    ],
    "Node.js": [
        r"\bnode(?:\.js|js)?\b",
    ],
    "REST API": [
        r"\brest(?:ful)?\s+api",
        r"\brest apis?\b",
    ],
    "GraphQL": [
        r"\bgraphql\b",
    ],
    "Docker": [
        r"\bdocker\b",
    ],
    "Kubernetes": [
        r"\bkubernetes\b",
        r"\bk8s\b",
    ],
    "Git": [
        r"\bgit\b",
    ],
    "GitHub": [
        r"\bgithub\b",
    ],
    "GitLab": [
        r"\bgitlab\b",
    ],
    "CI/CD": [
        r"\bci\s*/\s*cd\b",
        r"\bcontinuous integration\b",
        r"\bcontinuous delivery\b",
    ],
    "Jenkins": [
        r"\bjenkins\b",
    ],
    "AWS": [
        r"\baws\b",
        r"\bamazon web services\b",
    ],
    "Azure": [
        r"\bazure\b",
    ],
    "Google Cloud": [
        r"\bgoogle cloud\b",
        r"\bgcp\b",
    ],
    "Terraform": [
        r"\bterraform\b",
    ],
    "Linux": [
        r"\blinux\b",
    ],
    "Agile": [
        r"\bagile\b",
    ],
    "Scrum": [
        r"\bscrum\b",
    ],
    "Unit Testing": [
        r"\bunit testing\b",
        r"\bunit tests?\b",
    ],
    "Selenium": [
        r"\bselenium\b",
    ],
    "Playwright": [
        r"\bplaywright\b",
    ],
    "Jira": [
        r"\bjira\b",
    ],
}


def clean_text(
    value: str | None,
) -> str:

    if not value:
        return ""

    return " ".join(
        value.split()
    )


def extract_skills(
    description: str,
) -> list[str]:

    text = description.lower()

    found = []

    for skill, patterns in SKILL_PATTERNS.items():

        for pattern in patterns:

            if re.search(
                pattern,
                text,
            ):

                found.append(skill)
                break

    return found


def classify_skills(
    description: str,
    skills: list[str],
) -> tuple[list[str], list[str]]:

    text = description.lower()

    required_signals = [
        "required",
        "must have",
        "must",
        "essential",
        "mandatory",
        "minimum requirement",
        "requirements include",
        "required skills",
        "you need",
    ]

    preferred_signals = [
        "preferred",
        "advantage",
        "advantageous",
        "nice to have",
        "beneficial",
        "desirable",
        "would be an advantage",
    ]

    required = []
    preferred = []

    for skill in skills:

        patterns = SKILL_PATTERNS.get(
            skill,
            [],
        )

        positions = []

        for pattern in patterns:

            positions.extend(
                match.start()
                for match in re.finditer(
                    pattern,
                    text,
                )
            )

        for position in positions:

            start = max(
                0,
                position - 250,
            )

            end = min(
                len(text),
                position + 250,
            )

            context = text[
                start:end
            ]

            if any(
                signal in context
                for signal in preferred_signals
            ):

                if skill not in preferred:
                    preferred.append(skill)

                break

            if any(
                signal in context
                for signal in required_signals
            ):

                if skill not in required:
                    required.append(skill)

                break

    return required, preferred


def extract_responsibilities(
    description: str,
) -> list[str]:

    lines = re.split(
        r"[\r\n]+|(?<=[.!?])\s+",
        description,
    )

    results = []

    terms = [
        "responsible for",
        "responsibilities include",
        "you will",
        "duties include",
        "role involves",
    ]

    for line in lines:

        cleaned = clean_text(line)

        if len(cleaned) < 25:
            continue

        lower = cleaned.lower()

        if any(
            term in lower
            for term in terms
        ):

            if cleaned not in results:
                results.append(
                    cleaned[:400]
                )

        if len(results) >= 10:
            break

    return results


def extract_experience_requirements(
    description: str,
) -> list[str]:

    lines = re.split(
        r"[\r\n]+|(?<=[.!?])\s+",
        description,
    )

    results = []

    patterns = [
        r"\b\d+\+?\s+years?\b",
        r"\byears?\s+of\s+experience\b",
        r"\bexperience\s+with\b",
        r"\bexperience\s+in\b",
    ]

    for line in lines:

        cleaned = clean_text(line)

        if len(cleaned) < 10:
            continue

        lower = cleaned.lower()

        if any(
            re.search(
                pattern,
                lower,
            )
            for pattern in patterns
        ):

            if cleaned not in results:
                results.append(
                    cleaned[:400]
                )

        if len(results) >= 10:
            break

    return results


def extract_education_requirements(
    description: str,
) -> list[str]:

    lines = re.split(
        r"[\r\n]+|(?<=[.!?])\s+",
        description,
    )

    results = []

    terms = [
        "degree",
        "diploma",
        "bachelor",
        "bsc",
        "b.sc",
        "computer science",
        "information technology",
        "information systems",
        "qualification",
        "tertiary",
    ]

    for line in lines:

        cleaned = clean_text(line)

        if len(cleaned) < 10:
            continue

        lower = cleaned.lower()

        if any(
            term in lower
            for term in terms
        ):

            if cleaned not in results:
                results.append(
                    cleaned[:400]
                )

        if len(results) >= 10:
            break

    return results


def build_analysis(
    title: str,
    company: str,
    description: str,
    tags: list[str] | None = None,
) -> dict:

    tags = tags or []

    detected_skills = extract_skills(
        description
    )

    required_skills, preferred_skills = (
        classify_skills(
            description,
            detected_skills,
        )
    )

    normalized_tags = [
        clean_text(tag)
        for tag in tags
        if clean_text(tag)
    ]

    keywords = []

    for item in (
        detected_skills
        + normalized_tags
    ):

        if item and item not in keywords:
            keywords.append(item)

    return {
        "title": title,
        "company": company,
        "summary": description[:500],
        "required_skills": required_skills,
        "preferred_skills": preferred_skills,
        "responsibilities": extract_responsibilities(
            description
        ),
        "experience_requirements": (
            extract_experience_requirements(
                description
            )
        ),
        "education_requirements": (
            extract_education_requirements(
                description
            )
        ),
        "keywords": keywords,
    }


class ArbeitnowJobsSpider(
    scrapy.Spider
):

    name = "arbeitnow_jobs"

    allowed_domains = [
        "arbeitnow.com",
        "arbeitnow.fr",
    ]

    start_urls = [
        (
            "https://www.arbeitnow.com/"
            "api/job-board-api?page=1"
        )
    ]

    MAX_PAGES = 3

    def __init__(
        self,
        *args,
        **kwargs,
    ):

        super().__init__(
            *args,
            **kwargs,
        )

        self.pages_processed = 0

    def parse(
        self,
        response,
    ):

        self.pages_processed += 1

        data = response.json()

        jobs = data.get(
            "data",
            [],
        )

        self.logger.info(
            "Processing %s jobs "
            "from Arbeitnow page %s.",
            len(jobs),
            self.pages_processed,
        )

        for job in jobs:

            raw_description = (
                job.get(
                    "description",
                    "",
                )
            )

            description = (
                self.clean_html(
                    raw_description
                )
            )

            title = self.clean_html(
                job.get(
                    "title",
                    "",
                )
            )

            company = self.clean_html(
                job.get(
                    "company_name",
                    "",
                )
            )

            location = self.clean_html(
                job.get(
                    "location",
                    "",
                )
            )

            tags = job.get(
                "tags",
                [],
            )

            if not isinstance(
                tags,
                list,
            ):
                tags = []

            analysis = build_analysis(
                title=title,
                company=company,
                description=description,
                tags=tags,
            )

            yield {
                "source": "arbeitnow.com",

                "source_id": job.get(
                    "slug"
                ),

                "title": title,

                "company": company,

                "location": location,

                "description": description,

                "source_url": job.get(
                    "url"
                ),

                "remote": job.get(
                    "remote",
                    False,
                ),

                "tags": tags,

                "job_types": job.get(
                    "job_types",
                    [],
                ),

                "created_at": job.get(
                    "created_at"
                ),

                "analysis": analysis,
            }

        # -----------------------------------------
        # Pagination
        # -----------------------------------------

        if (
            self.pages_processed
            >= self.MAX_PAGES
        ):

            self.logger.info(
                "Reached MAX_PAGES=%s. "
                "Stopping spider.",
                self.MAX_PAGES,
            )

            return

        next_url = (
            data.get(
                "links",
                {},
            ).get(
                "next"
            )
        )

        if next_url:

            yield scrapy.Request(
                url=next_url,
                callback=self.parse,
            )

    @staticmethod
    def clean_html(
        value: str | None,
    ) -> str:

        if not value:
            return ""

        soup = BeautifulSoup(
            value,
            "html.parser",
        )

        for element in soup(
            [
                "script",
                "style",
                "img",
                "svg",
            ]
        ):

            element.decompose()

        text = soup.get_text(
            separator=" ",
            strip=True,
        )

        return " ".join(
            text.split()
        )