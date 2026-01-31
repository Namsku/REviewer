using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using REviewer.Modules.RE;
using REviewer.Modules.RE.Common;
using REviewer.Modules.Utils;

namespace REviewer.Services.Race
{
    public class RaceClient : IDisposable
    {
        // Events for UI
        public event Action<string>? OnLog;
        public event Action<bool>? OnConnectionStatusChanged;

        private readonly RootObject _rootObject;
        private CancellationTokenSource? _cts;
        private string _serverUrl = "127.0.0.1";
        private int _port = 5006;
        private string _roomId = "default";
        private string _password = "";
        private string _runnerId = "blue";
        private string _gameName = "Unknown";
        private string _hostToken = Guid.NewGuid().ToString();

        public RaceClient(RootObject rootObject)
        {
            _rootObject = rootObject;
        }

        public void Start(string url, int port, string roomId, string password, string runnerId, string gameName)
        {
            _serverUrl = url;
            _port = port;
            _roomId = roomId;
            _password = password;
            _runnerId = runnerId;
            _gameName = gameName;

            Stop();
            _cts = new CancellationTokenSource();
            _ = ConnectLoop(_cts.Token);
            OnLog?.Invoke($"Connecting to {_serverUrl}:{_port}...");
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            OnConnectionStatusChanged?.Invoke(false);
            OnLog?.Invoke("Disconnected.");
        }

        public void Disconnect()
        {
            Stop();
        }

        private async Task ConnectLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using (var client = new TcpClient())
                    {
                        await client.ConnectAsync(_serverUrl, _port);
                        
                        using (var stream = client.GetStream())
                        {
                            OnLog?.Invoke("Connected to server.");
                            OnConnectionStatusChanged?.Invoke(true);

                            // Handshake (JOIN)
                            var joinMessage = new
                            {
                                header = new { type = "JOIN", requestId = Guid.NewGuid().ToString() },
                                payload = new
                                {
                                    roomId = _roomId,
                                    password = _password,
                                    playerId = _runnerId,
                                    game = _rootObject.IDatabase?.GetProcessName() ?? "Unknown"
                                }
                            };
                            await SendWithFraming(stream, joinMessage);

                            // Start Send and Receive loops
                            var sendTask = SendDataLoop(stream, token);
                            var receiveTask = ReceiveLoop(stream, token);

                            await Task.WhenAny(sendTask, receiveTask);
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnLog?.Invoke($"RaceClient Connection Error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"RaceClient connection error: {ex.Message}");
                    OnConnectionStatusChanged?.Invoke(false);
                    
                    if (!token.IsCancellationRequested)
                    {
                        // Wait before reconnecting
                        OnLog?.Invoke("Retrying in 3 seconds...");
                        await Task.Delay(3000, token);
                    }
                }
            }
        }

        private async Task SendWithFraming(NetworkStream stream, object data)
        {
            string json = JsonConvert.SerializeObject(data);
            byte[] payload = Encoding.UTF8.GetBytes(json);
            byte[] header = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(payload.Length));

            await stream.WriteAsync(header, 0, header.Length);
            await stream.WriteAsync(payload, 0, payload.Length);
            await stream.FlushAsync();
        }

        private async Task SendDataLoop(NetworkStream stream, CancellationToken token)
        {
            DateTime lastSentTime = DateTime.MinValue;
            DateTime lastHealthSentTime = DateTime.MinValue;
            string lastNonHealthJson = "";
            int lastSentHealth = -1;

            while (!token.IsCancellationRequested)
            {
                // Gather data
                int currentHealth = _rootObject.Health?.Value ?? 0;
                
                // Construct payload parts for change detection
                // We exclude timestamp and health from the main check to handle them separately
                // Determine Max Inventory Slots
                // Default to RootObject value, but override for specific cases
                int maxSlots = _rootObject.InventoryCapacitySize > 0 ? _rootObject.InventoryCapacitySize : 8;

                // FIX: RE1 Chris (ID 0) should always have 6 slots max.
                // Sometimes memory values might be misleading or unset.
                bool isRe1 = _gameName.StartsWith("Resident Evil 1") || _gameName.StartsWith("Biohazard 1");
                
                if (isRe1 && _rootObject.Character?.Value == 0) // Chris
                {
                    maxSlots = 6;
                }

                // Construct payload parts for change detection
                // We exclude timestamp and health from the main check to handle them separately
                var nonHealthData = new
                {
                    roomId = _roomId,
                    playerId = _runnerId,
                    game = _gameName,
                    max_health = int.TryParse(_rootObject.MaxHealth, out int mh) ? mh : (_rootObject.CharacterMaxHealth?.Value ?? 0),
                    max_inventory_slots = maxSlots,
                    inventory = GetInventoryData(),
                    itembox = GetItemBoxData(),
                    key_items = GetKeyItemsData()
                };

                string currentNonHealthJson = JsonConvert.SerializeObject(nonHealthData);
                bool isNonHealthChanged = currentNonHealthJson != lastNonHealthJson;
                bool isHealthChanged = currentHealth != lastSentHealth;
                bool isHeartbeat = (DateTime.UtcNow - lastSentTime).TotalSeconds >= 10;
                
                bool shouldSend = false;

                // Priority 1: Heartbeat (Keep connection alive / sync periodically)
                if (isHeartbeat)
                {
                    shouldSend = true;
                }
                // Priority 2: Critical Data Changed (Inventory, Items, etc) -> Send Immediately
                else if (isNonHealthChanged)
                {
                    shouldSend = true;
                }
                // Priority 3: Only Health Changed -> Debounce (Max 2 per sec)
                else if (isHealthChanged)
                {
                    if ((DateTime.UtcNow - lastHealthSentTime).TotalMilliseconds >= 500)
                    {
                        shouldSend = true;
                    }
                }

                if (shouldSend)
                {
                    // Construct full payload
                    var fullPayload = new
                    {
                        header = new { type = "UPDATE_STATE" },
                        payload = new
                        {
                            roomId = _roomId,
                            playerId = _runnerId,
                            game = _gameName,
                            health = currentHealth,
                            max_health = nonHealthData.max_health,
                            max_inventory_slots = nonHealthData.max_inventory_slots,
                            inventory = nonHealthData.inventory,
                            itembox = nonHealthData.itembox,
                            key_items = nonHealthData.key_items,
                            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                            hostToken = _hostToken
                        }
                    };

                    await SendWithFraming(stream, fullPayload);

                    // Update tracking variables
                    lastSentTime = DateTime.UtcNow;
                    if (isHealthChanged || isNonHealthChanged || isHeartbeat) // Effectively if we sent health
                    {
                        lastHealthSentTime = DateTime.UtcNow; // Reset debounce timer
                        lastSentHealth = currentHealth;
                    }
                    if (isNonHealthChanged || isHeartbeat)
                    {
                        lastNonHealthJson = currentNonHealthJson;
                    }
                }

                await Task.Delay(100, token); // Check every 100ms for responsiveness
            }
        }

        private async Task ReceiveLoop(NetworkStream stream, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // 1. Read 4-byte header
                byte[] header = new byte[4];
                int bytesRead = await stream.ReadAsync(header, 0, 4, token);
                if (bytesRead < 4) break;

                int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(header, 0));
                
                // 2. Read 'length' bytes
                byte[] payloadData = new byte[length];
                int totalRead = 0;
                while (totalRead < length)
                {
                    int r = await stream.ReadAsync(payloadData, totalRead, length - totalRead, token);
                    if (r <= 0) break;
                    totalRead += r;
                }
                if (totalRead < length) break;

                string json = Encoding.UTF8.GetString(payloadData);
                HandleIncomingMessage(json);
            }
        }

        private void HandleIncomingMessage(string json)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<dynamic>(json);
                string type = message?.header?.type ?? message?.type;

                if (type == "ERROR")
                {
                    string msg = message?.payload?.message ?? "Unknown Error";
                    OnLog?.Invoke($"Server Error: {msg}");
                    // If critical auth error, stop reconnecting
                    if (msg == "Room not found" || msg == "Invalid password")
                    {
                        Stop(); 
                    }
                }
                else if (type == "RECONNECT_SYNC")
                {
                    var restoredItems = message?.payload?.restoredKeyItems?.ToObject<List<string>>();
                    if (restoredItems != null && _rootObject.KeyItems != null)
                    {
                        Logger.Instance.Info($"Received RECONNECT_SYNC: {string.Join(", ", restoredItems)}");
                        // Apply synchronization to local memory state if possible
                        // Note: REviewer usually reads from memory. 
                        // Synchronizing might involve setting flags if the game allows or just updating the UI display.
                        // However, the PRD says "Client A restores their tracker state".
                        // In REviewer, tracker state is driven by memory scanning.
                        // If we are racing, we might need to reflect these restored items in the UI overlay/tracker.
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error processing incoming message: {ex.Message}");
            }
        }

        private List<object> GetInventoryData()
        {
            var list = new List<object>();
            if (_rootObject.Inventory == null) return list;

            var items = _rootObject.IDatabase.GetItems();
            foreach (var slot in _rootObject.Inventory)
            {
                if (slot.Item != null && slot.Item.Value != 0)
                {
                    string? img = null;
                    if (items != null && items.Count > (byte)slot.Item.Value)
                    {
                        img = items[(byte)slot.Item.Value].Img;
                    }

                    list.Add(new
                    {
                        id = slot.Item.Value,
                        quantity = slot.Quantity?.Value ?? 0,
                        img = img
                    });
                }
            }
            return list;
        }

        private List<object> GetItemBoxData()
        {
            var list = new List<object>();
            if (_rootObject.ItemBox == null) return list;

            var items = _rootObject.IDatabase.GetItems();
            foreach (var slot in _rootObject.ItemBox)
            {
                if (slot.Item != null && slot.Item.Value != 0)
                {
                    string? img = null;
                    if (items != null && items.Count > (byte)slot.Item.Value)
                    {
                        img = items[(byte)slot.Item.Value].Img;
                    }

                    list.Add(new
                    {
                        id = slot.Item.Value,
                        quantity = slot.Quantity?.Value ?? 0,
                        img = img
                    });
                }
            }
            return list;
        }

        public async Task CloseRoom()
        {
            try
            {
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync(_serverUrl, _port);
                    using (var stream = client.GetStream())
                    {
                         var msg = new
                         {
                             header = new { type = "CLOSE_ROOM" },
                             payload = new
                             {
                                 roomId = _roomId,
                                 hostToken = _hostToken
                             }
                         };
                         await SendWithFraming(stream, msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error closing room: {ex.Message}");
            }
        }

        private Dictionary<string, bool> GetKeyItemsData()
        {
            var dict = new Dictionary<string, bool>();
            if (_rootObject.KeyItems == null) return dict;

            foreach (var item in _rootObject.KeyItems)
            {
                if (item.Data != null && !string.IsNullOrEmpty(item.Data.Name))
                {
                    dict[item.Data.Name] = item.State > 0;
                }
            }
            return dict;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
