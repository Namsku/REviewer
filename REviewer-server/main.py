import asyncio
import logging
import os
from fastapi import FastAPI, WebSocket, WebSocketDisconnect, Request
from fastapi.staticfiles import StaticFiles
from fastapi.templating import Jinja2Templates
from fastapi.middleware.cors import CORSMiddleware
from contextlib import asynccontextmanager
from pydantic import BaseModel
from typing import Optional
from room_manager import RoomManager
from tcp_server import start_tcp_server

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Start TCP Server on port 5006
    # Note: start_tcp_server handles the serving in a task usually, 
    # but my implementation has await serve_forever().
    # I need to run it in a background task so it doesn't block FastAPI startup.
    
    server_task = asyncio.create_task(start_tcp_server("0.0.0.0", 5006, room_manager))
    logger.info("Startup complete: Web server on 8000, TCP on 5006")
    yield
    # Cleanup (cancel task)
    server_task.cancel()
    try:
        await server_task
    except asyncio.CancelledError:
        pass

app = FastAPI(title="REviewer Race Server", lifespan=lifespan)
room_manager = RoomManager()
templates = Jinja2Templates(directory="templates")

# Mount the resources from local directory (since we copied them)
RESOURCES_DIR = os.path.join(os.getcwd(), "Resources")
if os.path.exists(RESOURCES_DIR):
    app.mount("/resources", StaticFiles(directory=RESOURCES_DIR), name="resources")
else:
    logger.error("Resources directory not found!")

# CORS for local development
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

class RoomCreate(BaseModel):
    room_id: str
    password: str
    game: str
    blue_name: str = "Runner A"
    red_name: str = "Runner B"

@app.get("/api/games")
async def get_games():
    return room_manager.get_available_games()

@app.get("/api/stats")
async def get_stats():
    return room_manager.get_stats()

@app.get("/api/history")
async def get_history():
    return room_manager.get_history()

@app.get("/api/rooms")
async def get_active_rooms():
    return room_manager.get_active_rooms()

@app.post("/api/rooms")
async def create_room(room: RoomCreate):
    success = room_manager.create_room(room.room_id, room.password, room.game, room.blue_name, room.red_name)
    if success:
        return {"status": "ok", "message": f"Room {room.room_id} created"}
    else:
        return {"status": "error", "message": "Room already exists"}

@app.post("/api/rooms/{room_id}/archive")
async def archive_room(room_id: str):
    success = room_manager.archive_room(room_id)
    if success:
        return {"status": "ok", "message": f"Room {room_id} archived"}
    return {"status": "error", "message": "Room not found"}

@app.get("/")
async def get_dashboard(request: Request, room: str = "default"):
    return templates.TemplateResponse("index.html", {"request": request, "room_id": room})

class GameUpdate(BaseModel):
    game: str

@app.put("/api/rooms/{room_id}/game")
async def update_room_game(room_id: str, update: GameUpdate):
    room = room_manager.get_room(room_id)
    if not room:
        return {"status": "error", "message": "Room not found"}
    
    room.set_game(update.game)
    return {"status": "ok", "message": f"Game switched to {update.game}"}

@app.get("/api/rooms/{room_id}")
async def get_room(room_id: str):
    state = room_manager.get_room_state(room_id)
    if state:
        return state
    return {"status": "error", "message": "Room not found"}

@app.websocket("/ws/{room_id}")
async def websocket_endpoint(websocket: WebSocket, room_id: str):
    await websocket.accept()
    logger.info(f"WebSocket client connected to room {room_id}")
    try:
        while True:
            # Send current room state every 500ms
            state = room_manager.get_room_state(room_id)
            if state:
                await websocket.send_json(state)
            await asyncio.sleep(0.5)
    except WebSocketDisconnect:
        logger.info(f"WebSocket client disconnected from room {room_id}")


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
