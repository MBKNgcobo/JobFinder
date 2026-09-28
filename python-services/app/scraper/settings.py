
BOT_NAME = "jobfinder"

SPIDER_MODULES = ["app.scraper.spiders"]
NEWSPIDER_MODULE = "app.scraper.spiders"

ROBOTSTXT_OBEY = True

USER_AGENT = "JobFinderPortfolioBot/1.0 (personal portfolio project)"

CONCURRENT_REQUESTS_PER_DOMAIN = 1
DOWNLOAD_DELAY = 2

AUTOTHROTTLE_ENABLED = True
AUTOTHROTTLE_START_DELAY = 2
AUTOTHROTTLE_MAX_DELAY = 10
AUTOTHROTTLE_TARGET_CONCURRENCY = 1.0

FEED_EXPORT_ENCODING = "utf-8"

""" 
ITEM_PIPELINES = {
    "app.scraper.pipelines.PostgreSQLJobPipeline": 300,
} 
"""
