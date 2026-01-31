import pytest
import pytest_asyncio
import asyncio
import httpx
from room_manager import RoomManager
from models import RunnerStatus
import logging

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Mock Room Manager for direct logic testing (Server Unit Test)
@pytest.fixture
def room_manager():
    rm = RoomManager()
    # Clean state
    rm.rooms = {}
    return rm

def test_room_initialization(room_manager):
    """Test that a room starts with Unknown game."""
    room_manager.create_room("test_room", "", "")
    room = room_manager.get_room("test_room")
    assert room.game == "Unknown"
    assert len(room.detected_games) == 0

def test_dynamic_game_detection(room_manager):
    """Test that the first runner sets the game."""
    room_manager.create_room("auto_room", "", "")
    room = room_manager.get_room("auto_room")
    
    status = RunnerStatus(
        room_id="auto_room",
        password="",
        runner_id="runner1",
        game="Resident Evil 2",
        health=100,
        max_health=100,
        inventory=[],
        itembox=[],
        key_items={},
        timestamp=0
    )
    
    room.update_runner(status)
    
    assert room.game == "Resident Evil 2"
    assert "Resident Evil 2" in room.detected_games

def test_multi_game_detection(room_manager):
    """Test that subsequent runners with different games are detected."""
    room_manager.create_room("multi_room", "", "Resident Evil 2") # Pre-set game or auto-detected
    room = room_manager.get_room("multi_room")
    
    # Update with same game
    status1 = RunnerStatus(
        room_id="multi_room",
        password="",
        runner_id="r1",
        game="Resident Evil 2",
        health=100,
        max_health=100,
        inventory=[],
        itembox=[],
        key_items={},
        timestamp=0
    )
    room.update_runner(status1)
    
    # Update with NEW game
    status2 = RunnerStatus(
        room_id="multi_room",
        password="",
        runner_id="r2",
        game="Resident Evil 3",
        health=100,
        max_health=100,
        inventory=[],
        itembox=[],
        key_items={},
        timestamp=0
    )
    room.update_runner(status2)
    
    assert room.game == "Resident Evil 2" # Should NOT auto-switch if already set
    assert "Resident Evil 3" in room.detected_games

def test_manual_game_switch(room_manager):
    """Test manual game switching logic."""
    room_manager.create_room("switch_room", "", "Game A")
    room = room_manager.get_room("switch_room")
    
    assert room.game == "Game A"
    
    room.set_game("Game B")
    
    assert room.game == "Game B"
    # Basic check that load_key_items was called (would log warning if 'Game B' has no items, but shouldn't crash)

# Integration Test needing run_server? 
# We can test the API logic via direct call or httpx if server was running.
# For unit testing, logic tests above are good.
