from dataclasses import dataclass
from typing import Optional


@dataclass
class ScrapedJob:
    title: str
    description: str
    company: str
    location: Optional[str]
    source: str
    source_url: str