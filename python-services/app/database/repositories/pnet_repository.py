import json

from app.database.repositories.pnet_repository import (
    PnetRepository,
)


def main():

    with open(
        "pnet_jobs.json",
        "r",
        encoding="utf-8",
    ) as file:
        jobs = json.load(file)

    print(
        f"Loaded {len(jobs)} jobs."
    )

    repository = PnetRepository()

    result = repository.save_jobs(
        jobs
    )

    print()
    print("================================")
    print("PNet import complete")
    print("================================")
    print(
        f"Created: {result['created']}"
    )
    print(
        f"Skipped: {result['skipped']}"
    )
    print(
        f"Failed: {result['failed']}"
    )


if __name__ == "__main__":
    main()