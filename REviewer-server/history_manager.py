import json
import os
import time
from typing import List, Dict
from models import RunnerStatus
import logging

logger = logging.getLogger(__name__)

HISTORY_FILE = "history.json"

class HistoryManager:
    def __init__(self):
        self.history = []
        self._load_history()

    def _load_history(self):
        if os.path.exists(HISTORY_FILE):
            try:
                with open(HISTORY_FILE, "r", encoding="utf-8") as f:
                    self.history = json.load(f)
            except Exception as e:
                logger.error(f"Failed to load history: {e}")
                self.history = []

    def _save_history(self):
        try:
            with open(HISTORY_FILE, "w", encoding="utf-8") as f:
                json.dump(self.history, f, indent=2)
        except Exception as e:
            logger.error(f"Failed to save history: {e}")

    def add_race_record(self, room_state: Dict):
        """
        Archive a room state into history.
        We expect room_state to be the output of Room.get_state()
        """
        # Determine winner? For now just store the final state.
        # Maybe calculate duration if we had start time.
        
        record = {
            "room_id": room_state["room_id"],
            "game": room_state["game"],
            "runner_names": room_state["runner_names"],
            "timestamp": time.time(),
            # Simplified runner data for history
            "runners": {
                k: {
                    "health": v["health"],
                    "key_items_count": len([i for i in room_state["shared_key_items"] if i.get(k)])
                } for k, v in room_state["runners"].items()
            }
        }
        
        self.history.append(record)
        self._save_history()

    def get_all_history(self):
        # Return most recent first
        return sorted(self.history, key=lambda x: x["timestamp"], reverse=True)

    def get_stats(self):
        total_races = len(self.history)
        
        # Calculate most popular game
        game_counts = {}
        for r in self.history:
            g = r.get("game", "Unknown")
            game_counts[g] = game_counts.get(g, 0) + 1
            
        most_popular = "N/A"
        if game_counts:
            most_popular = max(game_counts, key=game_counts.get)

        return {
            "total_races": total_races,
            "most_popular_game": most_popular
        }
