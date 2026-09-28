import os
import subprocess
import time
from datetime import datetime, timezone


INTERVAL_MINUTES = int(
    os.getenv(
        "SCRAPER_INTERVAL_MINUTES",
        "360"
    )
)


def run_command(command: list[str]) -> bool:
    print(
        f"[SCRAPER] Running: {' '.join(command)}",
        flush=True
    )

    result = subprocess.run(
        command,
        cwd="/app",
        check=False
    )

    if result.returncode != 0:
        print(
            f"[SCRAPER] Command failed "
            f"with code {result.returncode}",
            flush=True
        )

        return False

    print(
        "[SCRAPER] Command completed successfully.",
        flush=True
    )

    return True


def run_scraping_cycle():
    print(
        f"[SCRAPER] Cycle started: "
        f"{datetime.now(timezone.utc).isoformat()}",
        flush=True
    )

    # PNet browser scraper
    pnet_success = run_command(
        [
            "python",
            "-m",
            "app.scraper.pnet_browser"
        ]
    )

    # Import PNet results if scraping succeeded
    if pnet_success:
        run_command(
            [
                "python",
                "-m",
                "app.scraper.import_pnet"
            ]
        )

    # Arbeitnow Scrapy spider
    run_command(
        [
            "scrapy",
            "crawl",
            "arbeitnow_jobs"
        ]
    )

    print(
        "[SCRAPER] Cycle finished.",
        flush=True
    )


def main():
    print(
        "[SCRAPER] Docker worker started.",
        flush=True
    )

    while True:
        try:
            run_scraping_cycle()

        except Exception as exception:
            print(
                f"[SCRAPER ERROR] {exception}",
                flush=True
            )

        print(
            f"[SCRAPER] Sleeping "
            f"{INTERVAL_MINUTES} minutes.",
            flush=True
        )

        time.sleep(
            INTERVAL_MINUTES * 60
        )


if __name__ == "__main__":
    main()