import time
import logging
import json
import os
from typing import Dict, List, Optional
from models import RunnerStatus

logger = logging.getLogger(__name__)

# Load game data helper
def load_game_data():
    if os.path.exists("game_data.json"):
        with open("game_data.json", "r", encoding="utf-8") as f:
            return json.load(f)
    return {}

GAME_DATA = load_game_data()

class Room:
    def __init__(self, room_id: str, password: str, game: str = "Unknown", blue_name: str = "Runner A", red_name: str = "Runner B", host_token: str = None):
        self.room_id = room_id
        self.password = password
        self.game = game
        self.runners: Dict[str, RunnerStatus] = {}  # "blue" or "red"
        self.runner_names = {"blue": blue_name, "red": red_name}
        self.host_token = host_token
        self.last_update = time.time()
        
        # Room-specific key items list from game_data.json
        self.key_items_progress = []
        self.detected_games = set()
        
        if self.game and self.game != "Unknown":
            self.load_key_items()
            
    def load_key_items(self):
        # Reload game data to ensure we have the latest definitions
        current_data = load_game_data()
        
        # Fuzzy/Prefix matching for game name (e.g. "Resident Evil 1 - MediaKit" -> "Resident Evil 1")
        base_items = []
        matched_game_key = None
        
        game = self.game
        
        # Try exact match first
        if game in current_data:
            base_items = current_data[game]
            matched_game_key = game
        else:
            # Try splitting by " - " to handle suffixes like " - MediaKit"
            simple_name = game.split(" - ")[0].strip()
            if simple_name in current_data:
                base_items = current_data[simple_name]
                matched_game_key = simple_name
            else:
                # Fallback to prefix match just in case
                best_match = None
                for key in current_data.keys():
                    if game.startswith(key):
                        if best_match is None or len(key) > len(best_match):
                            best_match = key
                if best_match:
                    base_items = current_data[best_match]
                    matched_game_key = best_match
        
        if not base_items:
            logger.warning(f"No items found for game '{game}'. Available: {list(current_data.keys())}")
            # Reset lists if no match found, basically staying in 'Unknown' state visually
            self.key_items_progress = [] 
            return

        self.key_items_progress = []
        for item in base_items:
            self.key_items_progress.append({
                "name": item["name"],
                "img": item["img"],
                "type": item["type"],
                "blue": False,
                "red": False
            })
        logger.info(f"Loaded {len(self.key_items_progress)} key items for game '{game}'")

    def set_game(self, new_game: str):
        """Manually switches the active game for the room."""
        if new_game == self.game:
            return
        
        logger.info(f"Switching game for room {self.room_id}: {self.game} -> {new_game}")
        self.game = new_game
        self.load_key_items()
        
        # Reset runner found status for new game items?
        # Yes, we must re-evaluate against new list.
        # But we don't have historical data for the new game if they were playing old game.
        # So we just re-run update logic on next tick or force re-eval now?
        # Force re-eval if we have runner state in memory:
        for runner in self.runners.values():
            runner_col = runner.runner_id
            
            # Check against new progress list
            found_counts = {}
            for name, found in runner.key_items.items():
                if found:
                    found_counts[name] = found_counts.get(name, 0) + 1
            
            temp_counts = found_counts.copy()
            for item in self.key_items_progress:
                name = item["name"]
                if temp_counts.get(name, 0) > 0:
                    item[runner_col] = True
                    temp_counts[name] -= 1
                else:
                    item[runner_col] = False

    def update_runner(self, status: RunnerStatus):
        if status.password != self.password:
            logger.warning(f"Invalid password for room {self.room_id}")
            return
        
        # FR-07: Host Token Authority
        if status.host_token and not self.host_token:
            self.host_token = status.host_token
            logger.info(f"Host token set for room {self.room_id}")

        # Dynamic Game Detection
        if status.game and status.game != "Unknown":
            self.detected_games.add(status.game)
            
            if self.game == "Unknown":
                self.game = status.game
                logger.info(f"Auto-detected game for room {self.room_id}: {self.game}")
                self.load_key_items()

        if status.game != self.game:
            # Just track it (done above), allow UI to switch.
            pass

        self.runners[status.runner_id] = status
        self.last_update = time.time()
        
        # Update shared key items progress
        # status.key_items is a Dict[str, bool]
        runner_col = status.runner_id # "blue" or "red"
        
        # We need to be careful with duplicate items
        # Let's track how many of each item the runner has found
        found_counts = {}
        for name, found in status.key_items.items():
            if found:
                found_counts[name] = found_counts.get(name, 0) + 1
        
        # Reset progress for this runner for this game's list
        temp_counts = found_counts.copy()
        for item in self.key_items_progress:
            name = item["name"]
            if temp_counts.get(name, 0) > 0:
                item[runner_col] = True
                temp_counts[name] -= 1
            else:
                item[runner_col] = False

    def get_state(self):
        return {
            "room_id": self.room_id,
            "game": self.game,
            "runner_names": self.runner_names,
            "runners": {k: v.dict() for k, v in self.runners.items()},
            "shared_key_items": self.key_items_progress,
            "timestamp": self.last_update,
            "has_host": bool(self.host_token),
            "detected_games": list(self.detected_games)
        }

from history_manager import HistoryManager

ROOMS_FILE = "rooms_state.json"

class RoomManager:
    def __init__(self):
        self.rooms: Dict[str, Room] = {}
        self.history_manager = HistoryManager()
        self._load_rooms()

    def _save_rooms(self):
        data = {}
        for rid, room in self.rooms.items():
            data[rid] = {
                "room_id": room.room_id,
                "password": room.password,
                "game": room.game,
                "runner_names": room.runner_names,
                "runners": {k: v.dict() for k, v in room.runners.items()},
                "key_items_progress": room.key_items_progress,
                "host_token": room.host_token
            }
        try:
            with open(ROOMS_FILE, "w", encoding="utf-8") as f:
                json.dump(data, f, indent=2)
        except Exception as e:
            logger.error(f"Failed to save rooms: {e}")

    def _load_rooms(self):
        if not os.path.exists(ROOMS_FILE):
             return
        try:
            with open(ROOMS_FILE, "r", encoding="utf-8") as f:
                data = json.load(f)
                for rid, rdata in data.items():
                    # Reconstruct room
                    # Check if game exists in current GAME_DATA needed? Maybe not strictly.
                    room = Room(rdata["room_id"], rdata["password"], rdata["game"], 
                                rdata["runner_names"]["blue"], rdata["runner_names"]["red"],
                                host_token=rdata.get("host_token"))
                    
                    # Restore runners
                    for k, v in rdata.get("runners", {}).items():
                        room.runners[k] = RunnerStatus(**v)
                    
                    # Restore progress if available
                    if "key_items_progress" in rdata:
                        # Migration: Re-initialize from current GAME_DATA to pick up new items
                        # But preserve the 'blue'/'red' found status from the saved state.
                        saved_progress = rdata["key_items_progress"]
                        
                        # Create a lookup for saved status
                        # Dict key = item name. Value = {blue: bool, red: bool}
                        # Problem: Duplicates. "Small Key" appears multiple times.
                        # We need to migrate by Index? Or by Name counting?
                        # By Name Counting is safest if list order changed.
                        
                        saved_status_map = {}
                        for item in saved_progress:
                            name = item["name"]
                            if name not in saved_status_map:
                                saved_status_map[name] = []
                            saved_status_map[name].append({"blue": item.get("blue", False), "red": item.get("red", False)})
                        
                        # Now map to the FRESH list in 'room' (which was just init'd with fresh data)
                        new_progress = room.key_items_progress
                        
                        for item in new_progress:
                            name = item["name"]
                            if name in saved_status_map and saved_status_map[name]:
                                # Pop the first saved status for this name
                                status = saved_status_map[name].pop(0)
                                item["blue"] = status["blue"]
                                item["red"] = status["red"]
                        
                        # We intentionally discard any leftovers in saved_status_map (old stuff removed?)
                        # And any new items in new_progress just stay False (default)
                        
                        room.key_items_progress = new_progress
                        
                    self.rooms[rid] = room
            logger.info(f"Restored {len(self.rooms)} rooms from state.")
        except Exception as e:
            logger.error(f"Failed to load rooms: {e}")

    def create_room(self, room_id: str, password: str, game: str, blue_name: str = "Runner A", red_name: str = "Runner B", host_token: str = None):
        # Enforce lowercase room_id
        room_id = room_id.lower()
        # Allow recreation / update of existing room
        self.rooms[room_id] = Room(room_id, password, game, blue_name, red_name, host_token)
        self._save_rooms()
        return True

    def delete_room(self, room_id: str, token: str = None):
        """Deletes a room. If token provided (FR-07), validates authority."""
        room_id = room_id.lower()
        if room_id in self.rooms:
            room = self.rooms[room_id]
            # If room has a host token, must match provided token
            if room.host_token and token != room.host_token:
                logger.warning(f"Unauthorized delete attempt for room {room_id}")
                return False
            
            del self.rooms[room_id]
            self._save_rooms()
            return True
        return False
    
    def get_room(self, room_id: str):
        return self.rooms.get(room_id.lower())

    def get_room_state(self, room_id: str):
        room = self.get_room(room_id)
        if room:
            return room.get_state()
        return None

    def archive_room(self, room_id: str):
        room_id = room_id.lower()
        if room_id in self.rooms:
            state = self.rooms[room_id].get_state()
            self.history_manager.add_race_record(state)
            del self.rooms[room_id]
            self._save_rooms()
            return True
        return False



    def get_active_rooms(self):
        # Return summary of active rooms
        active = []
        for r in self.rooms.values():
            active.append({
                "room_id": r.room_id,
                "game": r.game,
                "runner_names": r.runner_names,
                "runners_count": len(r.runners),
                "last_update": r.last_update
            })
        return active

    def get_stats(self):
        stats = self.history_manager.get_stats()
        stats["active_sessions"] = len(self.rooms)
        return stats

    def get_history(self):
        return self.history_manager.get_all_history()

    def update_runner(self, status: RunnerStatus):
        if status.room_id in self.rooms:
            self.rooms[status.room_id].update_runner(status)
        else:
            # FR-Server-Rules: Disable auto-creation.
            # Clients must join existing rooms created via API/Web Interface.
            logger.warning(f"Rejected update for non-existent room {status.room_id} from {status.runner_id}")
            return
        
        # Debounce/Check frequency? For now save on every update might be too heavy?
        # Actually standard race updates are frequent. Writing JSON 60 times/sec per client is bad.
        # But we need persistence.
        # OPTIMIZATION: Only save if significant change or periodically?
        # For this requirement "crash recovery", maybe just accept IO cost or use a background saver?
        # Let's trust OS bufffering for now, or just save on Create/Delete and maybe periodically?
        # No, user wants connection back. The runner state is in memory.
        # If I don't save here, the state is lost on restart.
        # Let's save.
        self._save_rooms()

    def get_room_state(self, room_id: str):
        if room_id in self.rooms:
            return self.rooms[room_id].get_state()
        return None

    def get_available_games(self):
        global GAME_DATA
        GAME_DATA = load_game_data()
        return list(GAME_DATA.keys())
