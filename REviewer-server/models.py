from pydantic import BaseModel
from typing import Dict, List, Optional

class RunnerStatus(BaseModel):
    room_id: str
    password: str
    runner_id: str  # "blue" or "red"
    game: str
    health: int
    max_health: int
    inventory: list
    itembox: list
    key_items: Dict[str, bool]
    max_inventory_slots: int = 8
    timestamp: float
    host_token: Optional[str] = None
