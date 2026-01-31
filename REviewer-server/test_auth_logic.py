import asyncio
import json
import struct
import logging

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

SERVER_HOST = '127.0.0.1'
SERVER_PORT = 5006
API_PORT = 8000

async def send_msg(writer, msg_type, payload):
    msg = {"header": {"type": msg_type}, "payload": payload}
    json_data = json.dumps(msg).encode('utf-8')
    header = struct.pack('!I', len(json_data))
    writer.write(header + json_data)
    await writer.drain()

async def read_msg(reader):
    try:
        header_data = await reader.readexactly(4)
        length = struct.unpack('!I', header_data)[0]
        json_data = await reader.readexactly(length)
        return json.loads(json_data.decode('utf-8'))
    except asyncio.IncompleteReadError:
        return None

async def test_join_non_existent_room():
    logger.info("--- Test Case 1: Join Non-Existent Room ---")
    try:
        reader, writer = await asyncio.open_connection(SERVER_HOST, SERVER_PORT)
        
        # Room ID not created via API
        join_payload = {
            "roomId": "phantom_room",
            "password": "",
            "playerId": "tester",
            "game": "Bio"
        }
        await send_msg(writer, "JOIN", join_payload)
        
        # Expect ERROR or Close
        response = await read_msg(reader)
        if response and response['header']['type'] == 'ERROR':
            logger.info("✅ SUCCESS: Server rejected non-existent room.")
            logger.info(f"Server Message: {response['payload']['message']}")
        else:
            logger.error(f"❌ FAILURE: Server did not reject non-existent room correctly. Got: {response}")
            
        writer.close()
        await writer.wait_closed()
        
    except Exception as e:
        logger.error(f"❌ EXCEPTION: {e}")

async def create_room_via_api(room_id, password=""):
    import httpx
    async with httpx.AsyncClient() as client:
        # Note: Actual API for creating rooms might be different based on main.py
        # Assuming typical REST endpoint
        url = f"http://{SERVER_HOST}:{API_PORT}/api/rooms"
        data = {
            "room_id": room_id,
            "password": password,
            "game": "Bio"
        }
        try:
            resp = await client.post(url, json=data)
            if resp.status_code == 200:
                logger.info(f"Created room {room_id} via API")
                return True
            else:
                logger.error(f"Failed to create room: {resp.text}")
                return False
        except Exception as e:
            logger.error(f"Failed to call API: {e}")
            return False

async def test_join_invalid_password():
    logger.info("--- Test Case 2: Join Invalid Password ---")
    room_id = "locked_room"
    if not await create_room_via_api(room_id, "secret123"):
        return

    try:
        reader, writer = await asyncio.open_connection(SERVER_HOST, SERVER_PORT)
        
        join_payload = {
            "roomId": room_id,
            "password": "wrong_password",
            "playerId": "hacker",
            "game": "Bio"
        }
        await send_msg(writer, "JOIN", join_payload)
        
        response = await read_msg(reader)
        if response and response['header']['type'] == 'ERROR':
            logger.info("✅ SUCCESS: Server rejected invalid password.")
            logger.info(f"Server Message: {response['payload']['message']}")
        else:
            logger.error(f"❌ FAILURE: Server did not reject invalid password. Got: {response}")
            
        writer.close()
        await writer.wait_closed()
        
    except Exception as e:
        logger.error(f"❌ EXCEPTION: {e}")

async def test_join_valid():
    logger.info("--- Test Case 3: Join Valid Room & Password ---")
    room_id = "public_room"
    await create_room_via_api(room_id, "open")

    try:
        reader, writer = await asyncio.open_connection(SERVER_HOST, SERVER_PORT)
        
        join_payload = {
            "roomId": room_id,
            "password": "open",
            "playerId": "legit_user",
            "game": "Bio"
        }
        await send_msg(writer, "JOIN", join_payload)
        
        # Protocol: Server sends JOIN confirmation to others, but not sender (No-Echo).
        # We expect connection to stay open.
        
        try:
            # Wait a bit
            response = await asyncio.wait_for(read_msg(reader), timeout=2.0)
            if response and response['header']['type'] == 'ERROR':
                logger.error(f"❌ FAILURE: Server sent error on valid join: {response}")
            else:
                logger.info("❓ Received message on valid join (could be OK if broadcast logic changed).")
        except asyncio.TimeoutError:
            logger.info("✅ SUCCESS: Connection stayed open and no Error received.")
            
        writer.close()
        await writer.wait_closed()
        
    except Exception as e:
        logger.error(f"❌ EXCEPTION: {e}")

async def main():
    logger.info("Starting Auth Logic Tests...")
    await test_join_non_existent_room()
    await test_join_invalid_password()
    await test_join_valid()

if __name__ == "__main__":
    asyncio.run(main())
