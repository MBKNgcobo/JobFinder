from abc import ABC, abstractmethod
from typing import Any


class BaseAgent(ABC):

    name: str = "base_agent"

    @abstractmethod
    async def run(self, input_data: dict[str, Any]) -> dict[str, Any]:
        raise NotImplementedError