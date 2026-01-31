import asyncio
import json
import logging
import struct
from typing import Dict, Set, Optional, Any

from models import RunnerStatus

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

class TCPServer:
    def __init__(self, room_manager, host: str, port: int):
        self.room_manager = room_manager
        self.host = host
        self.port = port
        self.server = None
        # room_id -> Set[StreamWriter]
        self.rooms: Dict[str, Set[asyncio.StreamWriter]] = {}
        # writer -> room_id
        self.writer_to_room: Dict[asyncio.StreamWriter, str] = {}

    async def start(self):
        self.server = await asyncio.start_server(
            self.handle_client, self.host, self.port
        )
        logger.info(f"TCP Server serving on {self.host}:{self.port}")
        async with self.server:
            await self.server.serve_forever()

    async def send_with_framing(self, writer: asyncio.StreamWriter, data: dict):
        """Sends JSON data with 4-byte length-prefix framing."""
        try:
            payload = json.dumps(data).encode('utf-8')
            header = struct.pack('!I', len(payload))
            writer.write(header + payload)
            await writer.drain()
        except Exception as e:
            logger.error(f"Error sending message: {e}")

    async def broadcast_to_room(self, room_id: str, sender_writer: asyncio.StreamWriter, message: dict):
        """Relays message to all clients in a room except the sender (No-Echo)."""
        if room_id not in self.rooms:
            return
        
        for writer in self.rooms[room_id]:
            if writer != sender_writer:
                await self.send_with_framing(writer, message)

    async def handle_client(self, reader: asyncio.StreamReader, writer: asyncio.StreamWriter):
        addr = writer.get_extra_info('peername')
        logger.info(f"New TCP connection from {addr}")
        
        try:
            while True:
                # 1. Read 4-byte header
                header = await reader.readexactly(4)
                if not header:
                    break
                
                length = struct.unpack('!I', header)[0]
                
                # 2. Read 'length' bytes
                payload_data = await reader.readexactly(length)
                if not payload_data:
                    break
                
                message = json.loads(payload_data.decode('utf-8'))
                
                # Check for header/type structure
                msg_header = message.get("header", {})
                msg_type = msg_header.get("type")
                payload = message.get("payload", {})

                # Compatibility with old format (or flat format)
                if not msg_type:
                    msg_type = message.get("type")
                    payload = message

                if msg_type == "JOIN":
                    room_id = payload.get("roomId", payload.get("room_id", "default")).lower()
                    runner_id = payload.get("playerId", payload.get("runner_id", "unknown"))
                    
                    logger.info(f"Runner {runner_id} joining room {room_id}")
                    
                    # FR-Server-Rules: Strict Validation
                    room = self.room_manager.get_room(room_id)
                    if not room:
                        logger.warning(f"Rejected JOIN for non-existent room {room_id}")
                        err_msg = {"header": {"type": "ERROR"}, "payload": {"message": "Room not found"}}
                        await self.send_with_framing(writer, err_msg)
                        writer.close()
                        await writer.wait_closed()
                        continue

                    # Check Password (if room has one)
                    # Note: The client sends password in payload?
                    client_pass = payload.get("password", "")
                    if room.password and room.password != client_pass:
                        logger.warning(f"Rejected JOIN for room {room_id}: Invalid password")
                        err_msg = {"header": {"type": "ERROR"}, "payload": {"message": "Invalid password"}}
                        await self.send_with_framing(writer, err_msg)
                        writer.close()
                        await writer.wait_closed()
                        continue

                    # Track connection
                    if room_id not in self.rooms:
                        self.rooms[room_id] = set()
                    self.rooms[room_id].add(writer)
                    self.writer_to_room[writer] = room_id

                    # FR-05: Private Key Item Recovery (RECONNECT_SYNC)
                    room_state = self.room_manager.get_room_state(room_id)
                    is_reconnect = False
                    if room_state:
                        # Check if this runner had items previously
                        cached_runners = room_state.get("runners", {})
                        if runner_id in cached_runners:
                            is_reconnect = True
                            runner_data = cached_runners[runner_id]
                            key_items = runner_data.get("key_items", {})
                            # Only sync if there's actually something to sync
                            if any(key_items.values()):
                                recovered_items = [k for k, v in key_items.items() if v]
                                sync_msg = {
                                    "header": {"type": "RECONNECT_SYNC"},
                                    "payload": {"restoredKeyItems": recovered_items}
                                }
                                await self.send_with_framing(writer, sync_msg)
                                logger.info(f"Sent RECONNECT_SYNC to {runner_id} in {room_id}: {recovered_items}")

                    # Announce join to others ONLY if not a reconnect
                    # (To keep recovery private and opponents unaware as per PRD)
                    if not is_reconnect:
                        await self.broadcast_to_room(room_id, writer, message)
                    else:
                        logger.info(f"Suppressing JOIN relay for reconnecting runner {runner_id}")

                elif msg_type == "UPDATE_STATE":
                    room_id = payload.get("roomId", payload.get("room_id", "default")).lower()
                    
                    # 1. Update internal cache (silent)
                    # Use models.RunnerStatus for validation if possible
                    try:
                        # Map payload to RunnerStatus if needed
                        # models.RunnerStatus expects specific fields
                        # If the client sends the PRD-compliant structure, we might need to map it
                        status_dict = {
                            "room_id": room_id,
                            "password": payload.get("password", ""), # Might be missing in UPDATE_STATE
                            "runner_id": payload.get("playerId", payload.get("runner_id", "unknown")),
                            "game": payload.get("game", "Unknown"),
                            "health": payload.get("health", 0),
                            "max_health": payload.get("max_health", 0),
                            "inventory": payload.get("inventory", []),
                            "itembox": payload.get("itembox", []),
                            "key_items": payload.get("key_items", payload.get("keyItems", {})),
                            "timestamp": payload.get("timestamp", 0),
                            "host_token": payload.get("hostToken")
                        }
                        # Convert list of strings back to dict for room_manager if needed
                        # The PRD says key_items is a list of strings in the payload?
                        # No, the model says Dict[str, bool]. Let's check model.
                        
                        status = RunnerStatus(**status_dict)
                        self.room_manager.update_runner(status)
                    except Exception as e:
                        logger.warning(f"Failed to update runner cache: {e}")

                    # 2. Relay to OTHERS (No-Echo)
                    await self.broadcast_to_room(room_id, writer, message)

                elif msg_type == "CLOSE_ROOM":
                    room_id = payload.get("roomId", payload.get("room_id")).lower()
                    token = payload.get("hostToken")
                    
                    if room_id:
                        # FR-07: Host Authority Check via RoomManager
                        success = self.room_manager.delete_room(room_id, token)
                        if success:
                            logger.info(f"Room {room_id} closed by host.")
                            # Notify all clients in the room
                            notify_msg = {"header": {"type": "ROOM_CLOSED"}, "payload": {}}
                            if room_id in self.rooms:
                                for w in list(self.rooms[room_id]):     
                                    try:
                                        await self.send_with_framing(w, notify_msg)
                                    except:
                                        pass
                                # Clear connection tracking for this room
                                del self.rooms[room_id]
                        else:
                            logger.warning(f"Failed to close room {room_id}: Invalid token or room not found")

                elif msg_type == "hello":
                    # Old handshake support
                    response = {"type": "welcome", "status": "connected"}
                    await self.send_with_framing(writer, response)

                else:
                    # Generic relay for unknown types if room_id is present
                    room_id = payload.get("roomId", payload.get("room_id"))
                    if room_id:
                        await self.broadcast_to_room(room_id.lower(), writer, message)

        except asyncio.IncompleteReadError:
            pass
        except Exception as e:
            logger.error(f"Error handling client {addr}: {e}")
        finally:
            logger.info(f"Connection closed {addr}")
            room_id = self.writer_to_room.pop(writer, None)
            if room_id and room_id in self.rooms:
                self.rooms[room_id].discard(writer)
                if not self.rooms[room_id]:
                    del self.rooms[room_id]
            writer.close()
            try:
                await writer.wait_closed()
            except:
                pass

async def start_tcp_server(host: str, port: int, room_manager):
    server = TCPServer(room_manager, host, port)
    await server.start()
