import asyncio
import json
import logging
from typing import Dict, Optional
from models import RunnerStatus

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

class UDPServerProtocol(asyncio.DatagramProtocol):
    def __init__(self, room_manager):
        self.room_manager = room_manager

    def connection_made(self, transport):
        self.transport = transport
        logger.info("UDP Server started")

    def datagram_received(self, data, addr):
        try:
            message = data.decode()
            payload = json.loads(message)
            
            # Basic validation
            status = RunnerStatus(**payload)
            
            # Process with room manager
            self.room_manager.update_runner(status)
            
        except Exception as e:
            logger.error(f"Error processing packet from {addr}: {e}")

async def start_udp_server(host: str, port: int, room_manager):
    loop = asyncio.get_running_loop()
    transport, protocol = await loop.create_datagram_endpoint(
        lambda: UDPServerProtocol(room_manager),
        local_addr=(host, port)
    )
    return transport
