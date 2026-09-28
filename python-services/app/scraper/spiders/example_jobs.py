import scrapy


class ExampleJobsSpider(scrapy.Spider):

    name = "example_jobs"

    allowed_domains = [
        "example.com"
    ]

    start_urls = [
        "https://example.com/jobs"
    ]

    def parse(self, response):

        jobs = response.css(".job-card")

        for job in jobs:

            yield {
                "title": job.css(
                    ".job-title::text"
                ).get(default="").strip(),

                "company": job.css(
                    ".company::text"
                ).get(default="").strip(),

                "location": job.css(
                    ".location::text"
                ).get(default="").strip(),

                "description": job.css(
                    ".description::text"
                ).get(default="").strip(),

                "source": "Example",

                "source_url": response.url
            }