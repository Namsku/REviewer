using System.Configuration;
using System.Diagnostics;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using REviewer.Modules.RE;
using static REviewer.Modules.RE.GameData;
using Label = System.Windows.Forms.Label;
using Newtonsoft.Json;
using System.Security.Cryptography;
using MessagePack;
using REviewer.Modules.Common;
using REviewer.Modules.Common.REviewer.Modules.Common;

namespace REviewer.Modules.Forms
{
    public partial class Race : Form
    {
        private readonly GameData.RootObject _game;
        private readonly ItemIDs _itemDatabase;

        private readonly Stopwatch _raceWatch = new();
        private List<Stopwatch> _segmentWatch = [];

        private readonly int? _previousTimerValue = null;
        private int _previousSelectedSlot = 0;
        private readonly int _inventoryCapacitySize = 8;

        private Font _pixelBoyHealthFont;
        private Font _pixelBoySegments;
        private Font _pixelBoyDefault;

        private Dictionary<int, Label> _slotLabels;
        private Dictionary<int, PictureBox> _pictures;
        private Dictionary<int, PictureBox>? _pictureKeyItems;

        private readonly string _gameName;

        private GameData.PlayerRaceProgress? _raceDatabase;
        private List<GameData.PlayerRaceProgress> _saves;

        private readonly Dictionary<byte, List<int>> _healthTable = new()
        {
            { 0 , new List<int> { 140, 106,  71,  36, 0 } },
            { 1 , new List<int> { 96,   73,  49,  25, 0 } },
            { 2 , new List<int> { 96,   73,  49,  25, 0 } },
            { 3 , new List<int> { 96,   73,  49,  25, 0 } },
        };

        private static readonly string[] _itemTypes = ["Key Item", "Optionnal Key Item", "Nothing"];

        public Race(GameData.RootObject GameData, string gameName)
        {
            LoadDefaultPixelBoyFont();
            LoadCustomFontPixelBoyHealth();
            LoadCustomFontPixelBoySegments();

            InitializeComponent();
            InitInventorySlots();
            InitLoadState();

            _game = GameData;
            _gameName = gameName;
            _itemDatabase = new ItemIDs(gameName);
            _raceDatabase = new PlayerRaceProgress(gameName);
        }

        private void InitLoadState()
        {
            _saves = [];

            var directoryPath = "saves/";
            var files = System.IO.Directory.GetFiles(directoryPath, "*.dat");

            foreach (var file in files)
            {
                _saves.Add(DeserializeObject<GameData.PlayerRaceProgress>(file));
            }
        }

        private void InvokeUI(Action action)
        {
            if (!IsDisposed)
            {
                if (InvokeRequired)
                {
                    if (_game != null) // Add null check for Race struct
                    {
                        try
                        {
                            Invoke(action);
                        }
                        catch (ObjectDisposedException)
                        {
                            // Handle the ObjectDisposedException gracefully
                        }
                    }
                }
                else
                {
                    action();
                }
            }
        }

        private static Font LoadCustomFont(byte[] fontData, float size, FontStyle style, GraphicsUnit unit)
        {
            // Create a private font collection
            PrivateFontCollection privateFontCollection = new();

            // Load the font into the private font collection
            int dataLength = fontData.Length;
            IntPtr fontPtr = Marshal.AllocCoTaskMem(dataLength);
            Marshal.Copy(fontData, 0, fontPtr, dataLength);
            privateFontCollection.AddMemoryFont(fontPtr, dataLength);
            Marshal.FreeCoTaskMem(fontPtr);

            // Create a new font from the private font collection
            FontFamily fontFamily = privateFontCollection.Families[0];
            return new Font(fontFamily, size, style, unit);
        }

        private void LoadDefaultPixelBoyFont()
        {
            _pixelBoyDefault = LoadCustomFont(Properties.Resources.Pixeboy, 53, FontStyle.Bold, GraphicsUnit.Pixel);
        }

        private void LoadCustomFontPixelBoyHealth()
        {
            _pixelBoyHealthFont = LoadCustomFont(Properties.Resources.Pixeboy, 60, FontStyle.Regular, GraphicsUnit.Pixel);
        }

        private void LoadCustomFontPixelBoySegments()
        {
            _pixelBoySegments = LoadCustomFont(Properties.Resources.Pixeboy, 36, FontStyle.Regular, GraphicsUnit.Pixel);
        }

        private void Race_Load(object sender, EventArgs e)
        {
            InitKeyItems();
            InitKeyRooms();
            InitSaveMonitoring();

            InitLabels();
            InitChronometers();

            InitInventory();
            InitCharacterHealthState();
            SubscribeToEvents();
            CheckInventoryCapacity(_game.Inventory.Capacity.Value, pictureBoxItemSlot7, pictureBoxItemSlot8, labelSlot7Quantity, labelSlot8Quantity);
        }

        private void InitKeyRooms()
        {
            var reDataPath = ConfigurationManager.AppSettings["REdata"];
            var json = reDataPath != null ? File.ReadAllText(reDataPath) : throw new ArgumentNullException(nameof(reDataPath));
            var processName = _itemDatabase.GetProcessName();
            var bios = JsonConvert.DeserializeObject<Dictionary<string, KRoom>>(json);

            if (!bios.TryGetValue(processName, out var bio))
            {
                throw new KeyNotFoundException($"Bio with key {processName} not found in JSON.");
            }

            List<string> keyRooms = bio.KeyRooms;

            // add them into _raceDatabase.KeyRooms
            _raceDatabase.KeyRooms = [];

            foreach (var room in keyRooms)
            {
                _raceDatabase.KeyRooms.Add(room, []);
            }
        }

        private void InitInventory()
        {
            for (int i = 0; i < 8; i++)
            {
                int slotNumber = i; // To capture the variable in the closure
                UpdateSlotPicture(slotNumber);
                UpdateSlotCapacity(_slotLabels[slotNumber], slotNumber);
            }

            UpdateInventorySlotSelectedPicture();
            UpdateLastItemFoundPicture();
        }

        private void InitSaveMonitoring()
        {
            if (_raceDatabase.SavePath == null)
            {
                return;
            }


            _raceDatabase.Watcher.Path = _raceDatabase.SavePath;  // replace with your directory
            _raceDatabase.Watcher.Filter = "*.dat";  // watch for .dat files

            // Add event handlers.
            _raceDatabase.Watcher.Created += new FileSystemEventHandler(OnCreated);

            // Subscribe to the Changed event
            _raceDatabase.Watcher.Changed += (sender, e) =>
            {
                Logger.Logging.Debug($"File changed: {e.FullPath}");
            };

            // Subscribe to the Created event
            _raceDatabase.Watcher.Created += (sender, e) =>
            {
                Logger.Logging.Debug($"File created: {e.FullPath}");
            };

            // Subscribe to the Deleted event
            _raceDatabase.Watcher.Deleted += (sender, e) =>
            {
                Logger.Logging.Debug($"File deleted: {e.FullPath}");
            };

            // Subscribe to the Renamed event
            _raceDatabase.Watcher.Renamed += (sender, e) =>
            {
                Logger.Logging.Debug($"File renamed from {e.OldFullPath} to {e.FullPath}");
            };

            // Begin watching.
            _raceDatabase.Watcher.EnableRaisingEvents = true;

        }

        private void OnCreated(object source, FileSystemEventArgs e) => InvokeUI(() =>
        {
            var uid = UniqueIdGenerator.GenerateUniqueId();
            _raceDatabase.Saves += 1;
            _raceDatabase.UUID = uid;
            _raceDatabase.RealItembox = GetItemBoxData();

            SaveState();
            labelSaves.Text = _raceDatabase.Saves.ToString();

            // Open the file
            using (var stream = new FileStream(e.FullPath, FileMode.Open, FileAccess.ReadWrite))
            {
                // Go to position 0x2C4 and read the first 0x10 bytes
                stream.Position = 0x2C4;
                byte[] bytes = new byte[0x10];
                stream.Read(bytes, 0, bytes.Length);

                // Replace the last 0x10 bytes with the bytes read from position 0x2C4
                stream.Position = stream.Length - 0x10;
                stream.Write(bytes, 0, bytes.Length);

                // Generate the new bytes to write at position 0x2C4
                byte[] newBytes = new byte[16];
                BitConverter.GetBytes((ushort)0xDEAD).CopyTo(newBytes, 0);
                Array.Copy(uid, 0, newBytes, 2, Math.Min(14, uid.Length));

                // Write the new bytes at position 0x2C4
                stream.Position = 0x2C4;
                stream.Write(newBytes, 0, newBytes.Length);
            }
        });

        private void SaveState()
        {
            _raceDatabase.TickTimer = _game.Game.Timer.Value;
            _raceDatabase.Fulltimer = _raceWatch.Elapsed;
            _raceDatabase.Segtimer1 = _segmentWatch[0].Elapsed;
            _raceDatabase.Segtimer2 = _segmentWatch[1].Elapsed;
            _raceDatabase.Segtimer3 = _segmentWatch[2].Elapsed;
            _raceDatabase.Segtimer4 = _segmentWatch[3].Elapsed;

            SerializeObject(_raceDatabase);
        }

        public void SerializeObject<T>(T obj) where T : PlayerRaceProgress
        {
            var directoryPath = "saves/";
            // Make an exact copy of the object
            T copy_obj = (T)obj.Clone();

            _saves.Add(copy_obj);

            byte[] objectData = MessagePackSerializer.Serialize(copy_obj);

            byte[] hashBytes = SHA256.HashData(objectData);
            string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            // Append a unique identifier to the filename
            string filePath = Path.Combine(directoryPath, hashString + "_" + Guid.NewGuid().ToString() + ".dat");
            File.WriteAllBytes(filePath, objectData);
        }

        public static T DeserializeObject<T>(string filePath)
        {
            byte[] objectData = File.ReadAllBytes(filePath);
            T obj = MessagePackSerializer.Deserialize<T>(objectData);
            return obj;
        }

        private void InitLabels()
        {
            labelTimer.Font = _pixelBoyDefault;
            labelTimer.ForeColor = CustomColors.White;

            CheckHealthLabel(_game.Player.Health.Value);
            labelCharacter.Text = ((Dictionary<byte, string>)_game.Player.Character.Database)[(byte)_game.Player.Character.Value];

            _raceDatabase.Stage = ((_game.Player.Stage.Value & 0x05) + 1).ToString();

            labelSegTimer1.Font = _pixelBoySegments;
            labelSegTimer2.Font = _pixelBoySegments;
            labelSegTimer3.Font = _pixelBoySegments;
            labelSegTimer4.Font = _pixelBoySegments;

            labelSegTimer1.ForeColor = CustomColors.White;
            labelSegTimer2.ForeColor = CustomColors.White;
            labelSegTimer3.ForeColor = CustomColors.White;
            labelSegTimer4.ForeColor = CustomColors.White;

            labelDeaths.Font = _pixelBoyDefault;
            labelDeaths.ForeColor = CustomColors.White;

            labelDebug.Font = _pixelBoyDefault;
            labelDebug.ForeColor = CustomColors.White;

            labelResets.Font = _pixelBoyDefault;
            labelResets.ForeColor = CustomColors.White;

            labelSaves.Font = _pixelBoyDefault;
            labelSaves.ForeColor = CustomColors.White;

            labelCharacter.Font = _pixelBoyDefault;
            labelCharacter.ForeColor = CustomColors.White;

            labelGameCompleted.Font = _pixelBoySegments;
            labelGameCompleted.ForeColor = CustomColors.White;
            labelGameCompleted.Visible = false;

            _raceWatch.Reset();
            labelTimer.Text = _raceWatch.Elapsed.ToString(@"hh\:mm\:ss\.ff");

            labelDeaths.Text = _raceDatabase.Deaths.ToString();
            labelDebug.Text = _raceDatabase.Debugs.ToString();
            labelResets.Text = _raceDatabase.Resets.ToString();
            labelSaves.Text = _raceDatabase.Saves.ToString();
        }

        private void InitCharacterHealthState()
        {
            labelHealth.Font = _pixelBoyHealthFont;
            CheckCharacterHealthState(_game.Player.CharacterHealthState.Value);
        }

        private void InitChronometers()
        {
            if (_segmentWatch.Count > 0)
            {
                foreach (var stopwatch in _segmentWatch)
                {
                    stopwatch.Reset();
                }
            }
            else
            {
                _segmentWatch =
                [
                    new Stopwatch(),
                    new Stopwatch(),
                    new Stopwatch(),
                    new Stopwatch()
                ];
            }
            labelTimer.Text = _raceWatch.Elapsed.ToString(@"hh\:mm\:ss\.ff");
            labelSegTimer1.Text = _raceWatch.Elapsed.ToString(@"hh\:mm\:ss\.ff");
            labelSegTimer2.Text = _raceWatch.Elapsed.ToString(@"hh\:mm\:ss\.ff");
            labelSegTimer3.Text = _raceWatch.Elapsed.ToString(@"hh\:mm\:ss\.ff");
            labelSegTimer4.Text = _raceWatch.Elapsed.ToString(@"hh\:mm\:ss\.ff");
        }
        private void InitInventorySlots()
        {
            _slotLabels = [];
            _pictures = [];

            for (int i = 0; i < 8; i++)
            {
                string labelName = $"labelSlot{i + 1}Quantity";
                string pictureName = $"pictureBoxItemSlot{i + 1}";

                Label label = Controls.Find(labelName, true).FirstOrDefault() as Label;
                PictureBox pictureBox = Controls.Find(pictureName, true).FirstOrDefault() as PictureBox;

                if (label != null)
                    _slotLabels.Add(i, label);

                if (pictureBox != null)
                    _pictures.Add(i, pictureBox);
            }
        }

        private void InitKeyItems()
        {
            _raceDatabase.KeyItems = _itemDatabase.GetKeyItems()?.Select(item => new KeyItem((Property)item, -1, "NEW ROOM TO SAVE HERE")).ToList() ?? [];

            _pictureKeyItems = [];

            for (int i = 0; i < _raceDatabase.KeyItems.Count; i++)
            {
                string pictureName = $"pictureBoxKeyItem{i + 1}";
                PictureBox pictureBox = Controls.Find(pictureName, true).FirstOrDefault() as PictureBox;

                if (pictureBox != null)
                {
                    _pictureKeyItems.Add(i, pictureBox);
                    UpdateKeyItemPicture(pictureBox, _raceDatabase.KeyItems[i]);
                }
            }
        }


        private static void UpdateKeyItemPicture(PictureBox pictureBox, KeyItem keyItem, int state = 0)
        {
            pictureBox.Image = keyItem.Data.Img;

            pictureBox.BackColor = state switch
            {
                0 => Color.Transparent,
                1 => CustomColors.Orange,
                2 => CustomColors.Default,
                _ => pictureBox.BackColor // Default case
            };
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Close();
        }

        private async void UpdateInventorySlotSelectedPicture()
        {
            // Wait 100 ms to avoid flickering

            await Task.Delay(50);
            var islot = _game.Player.InventorySlotSelected.Value;

            var selected = _game.Player.InventorySlotSelected.Value - 1;
            byte id = _game.Player.InventorySlotSelected.Value == 0 ? (byte)0 : (byte)_game.Inventory.Slots[selected].Item.Value;

            var image = _itemDatabase.GetPropertyById(id).Img;
            pictureBoxItemSelected.Image = image;
            _previousSelectedSlot = islot;
        }

        private void UpdateLastItemFoundPicture()
        {
            byte value = (byte)_game.Player.LastItemFound.Value;
            if (_itemDatabase.GetPropertyById(value).Type == "Key Item" && _raceDatabase.FullRoomName == null)
            {
                _raceDatabase.Stage = ((_game.Player.Stage.Value % 5) + 1).ToString();
                _raceDatabase.Room = _game.Player.Room.Value.ToString("X2");
                _raceDatabase.FullRoomName = _raceDatabase.Stage + _raceDatabase.Room;
                UpdateRaceKeyItem(value, _raceDatabase.FullRoomName, 1);
            }

            byte item_id = (byte)_game.Player.LastItemFound.Value;
            pictureBoxLastItem.Image = _itemDatabase.GetPropertyById(item_id).Img;
        }

        private static int GetPositionInDatabase(Dictionary<byte, string> database, byte key)
        {
            int position = database.TakeWhile(pair => !pair.Key.Equals(key)).Count();
            return position == database.Count ? 2 : position;
        }

        private void SubscribeToEvents()
        {
            _game.Player.Room.Updated += Updated_Room;
            _game.Player.Stage.Updated += Updated_Stage;
            _game.Player.Character.Updated += Updated_Character;
            _game.Player.Health.Updated += Updated_Health;
            _game.Player.CharacterHealthState.Updated += Update_Character_State;

            _game.Inventory.Capacity.Updated += Updated_Inventory_Capacity;

            _game.Game.State.Updated += Updated_GameState;
            _game.Game.Timer.Updated += Updated_Timer;
            _game.Game.MainMenu.Updated += Updated_Reset;

            AssignEventHandlers();

            _game.Rebirth.Debug.Updated += Updated_Debug;
            //_game.Rebirth.Screen.Updated += Updated_Screen;
            _game.Player.LastItemFound.Updated += Updated_LastItemFound;
            _game.Player.InventorySlotSelected.Updated += Updated_InventorySlotSelected;
        }

        private void Updated_Reset(object sender, EventArgs e) => InvokeUI(() =>
        {

            int health = Int32.Parse(labelHealth.Text);

            
            if (_game.Game.MainMenu.Value == 1 && !labelGameCompleted.Visible && (health <= _healthTable[(byte)_game.Player.Character.Value][0]))
            {
                _raceDatabase.Resets += 1;
                labelResets.Text = _raceDatabase.Resets.ToString();
            }

            buttonReset.Enabled = _game.Game.MainMenu.Value == 1 ? true : false;
        });

        private void Update_Character_State(object sender, EventArgs e) => InvokeUI(() =>
        {
            CheckCharacterHealthState(_game.Player.CharacterHealthState.Value);
        });

        private void Updated_Inventory_Capacity(object sender, EventArgs e) => InvokeUI(() =>
        {
            CheckInventoryCapacity(_game.Inventory.Capacity.Value, pictureBoxItemSlot7, pictureBoxItemSlot8, labelSlot7Quantity, labelSlot8Quantity);
        });

        private void Updated_InventorySlotSelected(object sender, EventArgs e) => InvokeUI(() =>
        {
            UpdateInventorySlotSelectedPicture();
        });

        private void Updated_LastItemFound(object sender, EventArgs e) => InvokeUI(() =>
        {
            UpdateLastItemFoundPicture();
        });

        private void Updated_Debug(object sender, EventArgs e) => InvokeUI(() =>
        {
            _raceDatabase.Debugs = CheckDebugWindow(_game.Rebirth.Debug.Value) == "Active" ? _raceDatabase.Debugs + 1 : _raceDatabase.Debugs;
            labelDebug.Text = _raceDatabase.Debugs.ToString();
        });

        private void AssignEventHandlers()
        {
            for (int i = 0; i < 8; i++)
            {
                int slotNumber = i; // To capture the variable in the closure
                _game.Inventory.Slots[i].Item.Updated += (sender, e) => InvokeUI(() =>
                {
                    UpdateSlotPicture(slotNumber);
                });

                _game.Inventory.Slots[i].Quantity.Updated += (sender, e) => InvokeUI(() =>
                {
                    UpdateSlotCapacity(_slotLabels[slotNumber], slotNumber);
                });
            }

        }

        private void UpdateRaceKeyItem(int value, string room, int state, bool force = false)
        {
            value = GetKeyItemPosition(value, room, state);

            // If 'force' is true or the current state is less than the new state, update the state and picture
            if (force || _raceDatabase.KeyItems[value].State < state)
            {
                _raceDatabase.KeyItems[value].State = state;
                UpdatePictureKeyItemState(value);
            }

            _raceDatabase.KeyItems[value].Room = room;
        }

        private int GetKeyItemPosition(int value, string room, int state)
        {
            var name = _itemDatabase.GetPropertyNameById((byte)value);
            var item_box = (_game.Game.State.Value & 0x0000FF00) == 0x90;

            for (int i = 0; i < _raceDatabase.KeyItems.Count; i++)
            {
                if (_raceDatabase.KeyItems[i].Data.Name == name && (_raceDatabase.KeyItems[i].Room != room || _raceDatabase.KeyItems[i].State != state) && !item_box)
                {
                    return i;
                }
            }

            return 0;
        }

        private void UpdatePictureKeyItemState(int value)
        {
            var state = _raceDatabase.KeyItems[value].State;

            // change background color of the picture box
            _pictureKeyItems[value].BackColor = state switch
            {
                -1 => Color.Transparent,
                0 => Color.Transparent,
                1 => CustomColors.Orange,
                2 => CustomColors.Default,
                _ => Color.Transparent // Default case
            };
        }

        private void UpdateSlotPicture(int value)
        {
            if (value > _inventoryCapacitySize)
            {
                return;
            }

            var item_id = _game.Inventory.Slots[value].Item.Value;
            // if value is key item, update the key item
            if (_itemDatabase.GetPropertyById((byte)_game.Inventory.Slots[value].Item.Value).Type == "Key Item")
            {
                if ((_game.Game.State.Value & 0x0000FF00) == 0x8800)
                {
                    UpdateRaceKeyItem(item_id, _raceDatabase.FullRoomName, 2);
                }
            }

            var id = (byte)GetItemByPosition(_game.Inventory.Slots[value].Item.Value);
            var image = _itemDatabase.GetPropertyById(id).Img;
            _pictures[value].Image = image;

            UpdateSlotCapacity(_slotLabels[value], value);
        }
        private void UpdateSlotCapacity(Label label, int value)
        {
            if (value > _inventoryCapacitySize)
            {
                return;
            }

            Property item = _itemDatabase.GetPropertyById((byte)_game.Inventory.Slots[value].Item.Value);
            label.Text = _game.Inventory.Slots[value].Quantity.Value.ToString();
            label.Font = _pixelBoySegments;
            label.Visible = !(_itemTypes).Contains(item.Type);
            label.ForeColor = item.Color switch
            {
                "Yellow" => CustomColors.Yellow,
                "Orange" => CustomColors.Orange,
                "Red" => CustomColors.Red,
                _ => CustomColors.Default,
            };
        }


        private void Updated_Health(object sender, EventArgs e) => InvokeUI(() =>
        {
            CheckHealthLabel(_game.Player.Health.Value);
        });

        private void Updated_Character(object sender, EventArgs e) => InvokeUI(() =>
        {
            byte value = (byte)_game.Player.Character.Value;
            labelCharacter.Text = ((Dictionary<byte, string>)_game.Player.Character.Database)[value].ToString();
        });

        private void Updated_Stage(object sender, EventArgs e) => InvokeUI(() => _raceDatabase.Stage = ((_game.Player.Stage.Value % 5) + 1).ToString());

        private void Updated_Room(object sender, EventArgs e) => InvokeUI(() =>
        {
            _raceDatabase.Room = _game.Player.Room.Value.ToString("X2");
            _raceDatabase.LastRoomName = _raceDatabase.FullRoomName;
            _raceDatabase.FullRoomName = _raceDatabase.Stage + _raceDatabase.Room;

            UpdateKeyRooms();
        });

        private void UpdateKeyRooms()
        {
            string fullRoomName = _raceDatabase.FullRoomName;
            string lastRoomName = _raceDatabase.LastRoomName ?? fullRoomName;

            foreach (var roomName in new[] { lastRoomName, fullRoomName })
            {
                if (_raceDatabase.KeyRooms.TryGetValue(roomName, out List<string>? value))
                {
                    List<string> keyRooms = value;
                    string otherRoomName = roomName == lastRoomName ? fullRoomName : lastRoomName;

                    if (!keyRooms.Contains(otherRoomName) && otherRoomName != roomName)
                    {
                        keyRooms.Add(otherRoomName);

                        if (keyRooms.Count == 2)
                        {
                            UpdateChronometers();
                            _raceDatabase.KeyRooms.Remove(roomName);
                        }
                    }

                    // Update the list in the dictionary
                    _raceDatabase.KeyRooms[roomName] = keyRooms;
                }
            }
        }

        private void UpdateChronometers()
        {
            _segmentWatch[_raceDatabase.Segments].Stop();
            _raceDatabase.Segments += 1;
            _segmentWatch[_raceDatabase.Segments].Start();
        }

        private void Updated_GameState(object sender, EventArgs e) => InvokeUI(() =>
        {
            int state = _game.Game.State.Value;

            if ((state & 0xFF000000) == 0x54000000)
            {
                LoadState();
            }

            if ((state & 0x0FFF0000) == 0x1000000 && _raceDatabase.PreviousState != 0x1000000)
            {
                _raceDatabase.Deaths += 1;
                labelHealth.Text = "255";
                labelDeaths.Text = _raceDatabase.Deaths.ToString();
            } 
            else if ((state & 0x0FFF0000) == 0x1100000 && _raceDatabase.PreviousState != 0x2000000)
            {
                labelHealth.Text = "WP!";
                labelGameCompleted.Visible = true;
                _raceWatch.Stop();
                _segmentWatch[_raceDatabase.Segments].Stop();   
            }

            _raceDatabase.PreviousState = state & 0x0F000000;
        });
        private void LoadState()
        {
            byte[] inventory_data = GetItemBoxData();
            byte[] modded = inventory_data.Skip(2).ToArray();

            var save = _saves.FirstOrDefault(s => s.UUID.SequenceEqual(modded));

            if (save == null)
            {
                return;
            }

            if (_raceDatabase.Segments < save.Segments)
            {
                _segmentWatch[_raceDatabase.Segments].Stop();
                _raceDatabase.Segments = save.Segments;
                _segmentWatch[_raceDatabase.Segments].Start();
            }

            _raceDatabase.Debugs = Math.Max(_raceDatabase.Debugs, save.Debugs);
            _raceDatabase.Deaths = Math.Max(_raceDatabase.Deaths, save.Deaths);
            _raceDatabase.Resets = Math.Max(_raceDatabase.Resets, save.Resets);
            _raceDatabase.Saves = Math.Max(_raceDatabase.Saves, save.Saves);

            labelDebug.Text = _raceDatabase.Debugs.ToString();
            labelDeaths.Text = _raceDatabase.Deaths.ToString();
            labelResets.Text = _raceDatabase.Resets.ToString();
            labelSaves.Text = _raceDatabase.Saves.ToString();

            _raceDatabase.KeyRooms = save.KeyRooms;

            LoadKeyItems(save.KeyItems);

            for (int i = 0; i < 8; i++)
            {
                Write(_game.ItemBox.Slots[i].Item, (int)save.RealItembox[2 * i]);
                Write(_game.ItemBox.Slots[i].Quantity, (int)save.RealItembox[2 * i + 1]);
            }
        }

        private byte[] GetItemBoxData()
        {
            byte[] inventory = new byte[16];
            for (int i = 0; i < 8; i++)
            {
                inventory[2 * i] = (byte)_game.ItemBox.Slots[i].Item.Value;
                inventory[2 * i + 1] = (byte)_game.ItemBox.Slots[i].Quantity.Value;
            }

            return inventory;
        }

        private void LoadKeyItems(List<GameData.KeyItem> keyItems)
        {
            int i = 0;

            foreach (var keyItem in keyItems)
            {
                // Find the corresponding key item in _raceDatabase.KeyItems
                KeyItem existingKeyItem = _raceDatabase.KeyItems.FirstOrDefault(ki => ki.Data.Name == keyItem.Data.Name);

                if (existingKeyItem == null)
                {
                    return;
                }

                // If the key item exists in _raceDatabase.KeyItems, use the existing Property.Img value

                if (existingKeyItem.State != keyItem.State || existingKeyItem.Room != keyItem.Room)
                {
                    Logger.Logging.Debug($"Updating Key Item: Name={keyItem.Data.Name}, Room={keyItem.Room}, State={keyItem.State}, ExistingKeyItem: Name={existingKeyItem.Data.Name}, Room={existingKeyItem.Room}, State={existingKeyItem.State}");
                    // Assuming the value parameter for UpdateRaceKeyItem is the index of the key item in the list
                    int value = _itemDatabase.GetPropertyIdByName(keyItem.Data.Name);
                    UpdateRaceKeyItem(value, keyItem.Room, keyItem.State, true);
                }
                i++;

            }
        }

        private void Updated_Timer(object sender, EventArgs e) => InvokeUI(() =>
        {
            labelTimer.Text = _raceWatch.Elapsed.ToString(@"hh\:mm\:ss\.ff");

            Label[] labelSegTimers = [labelSegTimer1, labelSegTimer2, labelSegTimer3];

            if (_raceDatabase.Segments >= 0 && _raceDatabase.Segments < labelSegTimers.Length)
            {
                labelSegTimers[_raceDatabase.Segments].Text = _segmentWatch[_raceDatabase.Segments].Elapsed.ToString(@"hh\:mm\:ss\.ff");
            }

            int currentTimerValue = _game.Game.Timer.Value;

            if (_previousTimerValue == currentTimerValue)
            {
                return;
            }
            if (_raceWatch.IsRunning)
            {
                _raceWatch.Stop();
                _segmentWatch[_raceDatabase.Segments].Stop();
            }
            else
            {
                _raceWatch.Start();
                _segmentWatch[_raceDatabase.Segments].Start();
            }
        });


        public static string ToHexString(int value) => string.Join(" ", Enumerable.Range(0, 4).Select(i => value.ToString("X8").Substring(i * 2, 2)));



        private void CheckInventoryCapacity(int value, PictureBox slot7, PictureBox slot8, Label capacity7, Label capacity8)
        {
            bool isVisible = (value & 0xFF000000) != 0x08000000;

            slot7.Visible = isVisible;
            slot8.Visible = isVisible;

            if (isVisible)
            {
                UpdateSlotCapacity(capacity7, 6);
                UpdateSlotCapacity(capacity8, 7);
            }
            else
            {
                capacity7.Visible = false;
                capacity8.Visible = false;
                slot7.Visible = false;
                slot8.Visible = false;
            }
        }

        private static int GetItemPosition(int value) => value switch
        {
            111 => 76,
            112 => 77,
            _ => value
        };


        private static int GetItemByPosition(int value) => value switch
        {
            76 => 111,
            77 => 112,
            _ => value
        };

        public static string CheckActionMade(int value) => (value & 0xFF) switch
        {
            0x01 => "Item not taken",
            0x80 => "Event",
            0x81 => "Selecting No",
            _ => "-"
        };

        public static string CheckActionPosition(int value) => (value & 0xFF) switch
        {
            0x40 => "Pushing",
            0x80 => "Above Object",
            _ => "Normal"
        };


        private void CheckHealthLabel(int value)
        {
            var health_table = _healthTable[(byte)(_game.Player.Character.Value % 4)];
            var status = _game.Player.CharacterHealthState.Value;
            Color[] colors = [Color.DarkGreen, CustomColors.Default, CustomColors.Yellow, CustomColors.Orange, CustomColors.Red, CustomColors.White];
            labelHealth.Text = value.ToString();


            if ((status & 0x40) != 0 || (status & 0x04) != 0 || (status & 0x02) != 0)
            {
                return;
            }

            if (value == health_table[0])
            {
                labelHealth.ForeColor = colors[0];
                return;
            }

            for (int i = 0; i < health_table.Count - 1; i++)
            {
                if (health_table[i] > value && value >= health_table[i + 1])
                {
                    labelHealth.ForeColor = colors[i + 1];
                    return;
                }
            }

            labelHealth.ForeColor = colors[health_table.Count];
        }

        private void CheckCharacterHealthState(int value)
        {
            if ((value & 0x40) == 0 && (value & 0x04) == 0 && (value & 0x02) == 0)
            {
                CheckHealthLabel(_game.Player.Health.Value);
            }
            else
            {
                labelHealth.ForeColor = Color.DarkViolet;
            }
        }

        public static string CheckDebugWindow(int value) => (value & 0xFFFF) == 0x1 ? "Active" : "Inactive";

        private void Label2_Click(object sender, EventArgs e)
        {

        }

        private void ButtonReset_Click(object sender, EventArgs e)
        {
            // clear everything completely
            _raceDatabase = null;
            _pictureKeyItems = null;

            // UnsubscribeAllEvents();
            ErasePlayerData();

            // re-init everything
            _raceDatabase = new GameData.PlayerRaceProgress(_gameName)
            {
                Segments = 0
            };

            InitKeyItems();
            InitKeyRooms();

            InitLabels();
            InitChronometers();

            CheckInventoryCapacity(_game.Inventory.Capacity.Value, pictureBoxItemSlot7, pictureBoxItemSlot8, labelSlot7Quantity, labelSlot8Quantity);

            InitInventory();
            InitCharacterHealthState();

            // SubscribeToEvents();
        }

        private void ErasePlayerData()
        {
            for (int i = 0; i < 8; i++)
            {
                Write(_game.Inventory.Slots[i].Item, 0);
                Write(_game.Inventory.Slots[i].Quantity, 0);
            }

            for (int i = 0; i < 47; i++)
            {
                Write(_game.ItemBox.Slots[i].Item, 0);
                Write(_game.ItemBox.Slots[i].Quantity, 0);
            }

            Write(_game.Player.InventorySlotSelected, 0);
            Write(_game.Player.LastItemFound, 0);
        }

        private static void Write(VariableData v, int value)
        {
            lock (v.LockObject)
            {
                v.IsUpdated = true;
                v.Value = value;
            }
        }

        private void UnsubscribeAllEvents()
        {
            _game.Player.Room.Updated -= Updated_Room;
            _game.Player.Stage.Updated -= Updated_Stage;
            _game.Player.Character.Updated -= Updated_Character;
            _game.Player.Health.Updated -= Updated_Health;
            _game.Player.CharacterHealthState.Updated -= Update_Character_State;

            _game.Inventory.Capacity.Updated -= Updated_Inventory_Capacity;

            _game.Game.State.Updated -= Updated_GameState;
            _game.Game.Timer.Updated -= Updated_Timer;
            _game.Game.MainMenu.Updated -= Updated_Reset;

            _game.Rebirth.Debug.Updated -= Updated_Debug;

            _game.Player.LastItemFound.Updated -= Updated_LastItemFound;
            _game.Player.InventorySlotSelected.Updated -= Updated_InventorySlotSelected;
            //_game.Rebirth.Screen.Updated -= Updated_Screen;

            for (int i = 0; i < 8; i++)
            {
                _game.Inventory.Slots[i].Item.Updated -= (sender, e) => { };
                _game.Inventory.Slots[i].Quantity.Updated -= (sender, e) => { };
            }

        }

        private void Button4_Click(object sender, EventArgs e)
        {
            string seed = "";

            for (int i = 0; i < 8; i++)
            {
                var slot = _game.Inventory.Slots[i];
                seed += slot.Item.Value.ToString();
                seed += slot.Quantity.Value.ToString();
            }

            SeedChecker seedChecker = new(seed);
            seedChecker.Show();

            Console.WriteLine(seed);
        }

        private void Button3_Click(object sender, EventArgs e)
        {

        }
    }

    public class KRoom
    {
        public List<string> KeyRooms { get; set; }
    }
}
