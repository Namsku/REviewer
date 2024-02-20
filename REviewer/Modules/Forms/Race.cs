using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing.Text;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using REviewer.Modules.RE;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using static REviewer.Modules.RE.GameData;
using Label = System.Windows.Forms.Label;
using Newtonsoft.Json;
using static REviewer.Modules.RE.ItemIDs;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using MessagePack;

namespace REviewer.Modules.Forms
{
    public partial class Race : Form
    {
        private GameData.RootObject _re1;
        private readonly ItemIDs _db_items;

        private Stopwatch chronometer = new Stopwatch();
        private List<Stopwatch> _segment_chronometers = [];
        private int? previousTimerValue = null;
        private Font PixelBoyHealth;
        private Font PixelBoySegments;
        private Font ResidentEvilFont;
        private Font PixelBoy;
        private Dictionary<int, Label> slotLabels;
        private Dictionary<int, PictureBox> pictures;
        private Dictionary<int, PictureBox>? pictureKeyItems;
        private GameData.PlayerRaceProgress? _race_db;
        private string _gameName;
        private List<GameData.PlayerRaceProgress> _saves;
        private int _old_selected_slot = 0;

        private int _inventory_size = 8;
        Dictionary<byte, List<int>> _health_table = new Dictionary<byte, List<int>>
        {
            { 0 , new List<int> { 140, 106,  71,  36, 0 } },
            { 1 , new List<int> { 96,   73,  49,  25, 0 } }
        };

        public Race(GameData.RootObject GameData, string gameName)
        {
            LoadCustomFontRE();
            LoadCustomFontOrbitron();
            LoadCustomFontPixelBoyHealth();
            LoadCustomFontPixelBoySegments();

            InitializeComponent();
            InitInventorySlots();
            InitLoadState();

            _re1 = GameData;
            _gameName = gameName;
            _db_items = new ItemIDs(gameName);
            _race_db = new PlayerRaceProgress(gameName);
        }

        private void InitLoadState()
        {
            _saves = new List<GameData.PlayerRaceProgress>();

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
                    if (_re1 != null) // Add null check for Race struct
                    {
                        Invoke(action);
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
            PrivateFontCollection privateFontCollection = new PrivateFontCollection();

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

        private void LoadCustomFontRE()
        {
            ResidentEvilFont = LoadCustomFont(Properties.Resources.Pixeboy, 38, FontStyle.Regular, GraphicsUnit.Pixel);
        }

        private void LoadCustomFontOrbitron()
        {
            PixelBoy = LoadCustomFont(Properties.Resources.Pixeboy, 53, FontStyle.Bold, GraphicsUnit.Pixel);
        }

        private void LoadCustomFontPixelBoyHealth()
        {
            PixelBoyHealth = LoadCustomFont(Properties.Resources.Pixeboy, 60, FontStyle.Regular, GraphicsUnit.Pixel);
        }

        private void LoadCustomFontPixelBoySegments()
        {
            PixelBoySegments = LoadCustomFont(Properties.Resources.Pixeboy, 36, FontStyle.Regular, GraphicsUnit.Pixel);
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
            CheckInventoryCapacity(_re1.Inventory.Capacity.Value, pictureBoxItemSlot7, pictureBoxItemSlot8, labelSlot7Quantity, labelSlot8Quantity);
        }

        private void InitKeyRooms()
        {
            var reDataPath = ConfigurationManager.AppSettings["REdata"];
            var json = reDataPath != null ? File.ReadAllText(reDataPath) : throw new ArgumentNullException(nameof(reDataPath));
            var processName = _db_items.GetProcessName();
            var bios = JsonConvert.DeserializeObject<Dictionary<string, KRoom>>(json);

            if (!bios.TryGetValue(processName, out var bio))
            {
                throw new KeyNotFoundException($"Bio with key {processName} not found in JSON.");
            }

            List<string> keyRooms = bio.KeyRooms;

            // add them into _race_db.KeyRooms
            _race_db.KeyRooms = new Dictionary<string, List<string>>();

            foreach (var room in keyRooms)
            {
                _race_db.KeyRooms.Add(room, new List<string>());
            }
        }

        private void InitInventory()
        {
            for (int i = 0; i < 8; i++)
            {
                int slotNumber = i; // To capture the variable in the closure
                UpdateSlotPicture(slotNumber);
                UpdateSlotCapacity(slotLabels[slotNumber], slotNumber);
            }

            UpdateInventorySlotSelectedPicture();
            UpdateLastItemFoundPicture();
        }

        private void InitSaveMonitoring()
        {
            if (_race_db.SavePath != null)
            {
                _race_db.Watcher.Path = _race_db.SavePath;  // replace with your directory
                _race_db.Watcher.Filter = "*.dat";  // watch for .dat files

                // Add event handlers.
                _race_db.Watcher.Created += new FileSystemEventHandler(OnCreated);

                // Begin watching.
                _race_db.Watcher.EnableRaisingEvents = true;
            }
        }

        private void OnCreated(object source, FileSystemEventArgs e) => InvokeUI(() =>
        {
            _race_db.Saves += 1;
            SaveState();
            labelSaves.Text = _race_db.Saves.ToString();

        });

        private void SaveState()
        {
            _race_db._tickTimer = _re1.Game.Timer.Value;
            _race_db._fulltimer = chronometer.Elapsed;
            _race_db._segtimer1 = _segment_chronometers[0].Elapsed;
            _race_db._segtimer2 = _segment_chronometers[1].Elapsed;
            _race_db._segtimer3 = _segment_chronometers[2].Elapsed;
            _race_db._segtimer4 = _segment_chronometers[3].Elapsed;

            SerializeObject(_race_db);
        }

        public void SerializeObject<T>(T obj) where T : PlayerRaceProgress
        {
            var directoryPath = "saves/";
            _saves.Add((PlayerRaceProgress) obj);

            byte[] objectData = MessagePackSerializer.Serialize(obj);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(objectData);
                string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                string filePath = Path.Combine(directoryPath, hashString + ".dat");
                File.WriteAllBytes(filePath, objectData);
            }
        }

        public T DeserializeObject<T>(string filePath)
        {
            byte[] objectData = File.ReadAllBytes(filePath);
            T obj = MessagePackSerializer.Deserialize<T>(objectData);
            return obj;
        }

        private void InitLabels()
        {
            labelTimer.Font = PixelBoy;
            labelTimer.ForeColor = Color.FromArgb(250, 240, 216);

            CheckHealthLabel(_re1.Player.Health.Value);
            labelCharacter.Text = ((Dictionary<byte, string>)_re1.Player.Character.Database)[(byte)_re1.Player.Character.Value];

            _race_db.stage = ((_re1.Player.Stage.Value % 5) + 1).ToString();

            labelSegTimer1.Font = PixelBoySegments;
            labelSegTimer2.Font = PixelBoySegments;
            labelSegTimer3.Font = PixelBoySegments;
            labelSegTimer4.Font = PixelBoySegments;

            labelSegTimer1.ForeColor = Color.FromArgb(250, 240, 216);
            labelSegTimer2.ForeColor = Color.FromArgb(250, 240, 216);
            labelSegTimer3.ForeColor = Color.FromArgb(250, 240, 216);
            labelSegTimer4.ForeColor = Color.FromArgb(250, 240, 216);

            labelDeaths.Font = PixelBoy;
            labelDeaths.ForeColor = Color.FromArgb(250, 240, 216);

            labelDebug.Font = PixelBoy;
            labelDebug.ForeColor = Color.FromArgb(250, 240, 216);

            labelResets.Font = PixelBoy;
            labelResets.ForeColor = Color.FromArgb(250, 240, 216);

            labelSaves.Font = PixelBoy;
            labelSaves.ForeColor = Color.FromArgb(250, 240, 216);

            labelCharacter.Font = PixelBoy;
            labelCharacter.ForeColor = Color.FromArgb(250, 240, 216);

            chronometer.Reset();
            labelTimer.Text = chronometer.Elapsed.ToString(@"hh\:mm\:ss\.ff");

            labelDeaths.Text = _race_db.Deaths.ToString();
            labelDebug.Text = _race_db.Debugs.ToString();
            labelResets.Text = _race_db.Resets.ToString();
            labelSaves.Text = _race_db.Saves.ToString();
        }

        private void InitCharacterHealthState()
        {
            labelHealth.Font = PixelBoyHealth;
            CheckCharacterHealthState(_re1.Player.CharacterHealthState.Value);
        }

        private void InitChronometers()
        {
            if (_segment_chronometers.Count > 0)
            {
                foreach (var stopwatch in _segment_chronometers)
                {
                    stopwatch.Reset();
                }
            }
            else
            {
                _segment_chronometers =
                [
                    new Stopwatch(),
                    new Stopwatch(),
                    new Stopwatch(),
                    new Stopwatch()
                ];
            }
            labelTimer.Text = chronometer.Elapsed.ToString(@"hh\:mm\:ss\.ff");
            labelSegTimer1.Text = chronometer.Elapsed.ToString(@"hh\:mm\:ss\.ff");
            labelSegTimer2.Text = chronometer.Elapsed.ToString(@"hh\:mm\:ss\.ff");
            labelSegTimer3.Text = chronometer.Elapsed.ToString(@"hh\:mm\:ss\.ff");
            labelSegTimer4.Text = chronometer.Elapsed.ToString(@"hh\:mm\:ss\.ff");
        }
        private void InitInventorySlots()
        {
            slotLabels = new Dictionary<int, Label>();
            pictures = new Dictionary<int, PictureBox>();

            for (int i = 0; i < 8; i++)
            {
                string labelName = $"labelSlot{i + 1}Quantity";
                string pictureName = $"pictureBoxItemSlot{i + 1}";

                Label label = Controls.Find(labelName, true).FirstOrDefault() as Label;
                PictureBox pictureBox = Controls.Find(pictureName, true).FirstOrDefault() as PictureBox;

                if (label != null)
                    slotLabels.Add(i, label);

                if (pictureBox != null)
                    pictures.Add(i, pictureBox);
            }
        }

        private void InitKeyItems()
        {
            _race_db.KeyItems = _db_items.GetKeyItems()?.Select(item => new KeyItem((Property)item, -1, "NEW ROOM TO SAVE HERE")).ToList() ?? new List<KeyItem>();

            pictureKeyItems = new Dictionary<int, PictureBox>();

            for (int i = 0; i < _race_db.KeyItems.Count; i++)
            {
                string pictureName = $"pictureBoxKeyItem{i + 1}";
                PictureBox pictureBox = Controls.Find(pictureName, true).FirstOrDefault() as PictureBox;

                if (pictureBox != null)
                {
                    pictureKeyItems.Add(i, pictureBox);
                    UpdateKeyItemPicture(pictureBox, _race_db.KeyItems[i]);
                }
            }
        }


        private static void UpdateKeyItemPicture(PictureBox pictureBox, KeyItem keyItem, int state = 0)
        {
            pictureBox.Image = keyItem.Data.Img;

            pictureBox.BackColor = state switch
            {
                0 => Color.Transparent,
                1 => Color.FromArgb(198, 155, 101),
                2 => Color.FromArgb(159, 185, 118),
                _ => pictureBox.BackColor // Default case
            };
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Close();
        }

        private async void UpdateInventorySlotSelectedPicture()
        {
            // Wait 100 ms to avoid flickering

            await Task.Delay(50);
            var islot = _re1.Player.InventorySlotSelected.Value;
            
            var selected = _re1.Player.InventorySlotSelected.Value - 1;
            byte id = _re1.Player.InventorySlotSelected.Value == 0 ? (byte) 0 : (byte)_re1.Inventory.Slots[selected].Item.Value;

            var image = _db_items.GetPropertyById(id).Img;
            pictureBoxItemSelected.Image = image;
            _old_selected_slot = islot;
        }

        private void UpdateLastItemFoundPicture()
        {
            byte value = (byte)_re1.Player.LastItemFound.Value;
            // if value is key item, update the key item
            if (_db_items.GetPropertyById(value).Type == "Key Item")
            {
                if (_race_db.FullRoomName == null)
                {
                    _race_db.stage = ((_re1.Player.Stage.Value % 5) + 1).ToString();
                    _race_db.room = _re1.Player.Room.Value.ToString("X2");
                    _race_db.FullRoomName = _race_db.stage + _race_db.room;
                }
                UpdateRaceKeyItem(value, _race_db.FullRoomName, 1);
            }

            byte item_id = (byte)_re1.Player.LastItemFound.Value;
            var image = _db_items.GetPropertyById(item_id).Img;
            pictureBoxLastItem.Image = image;
        }

        private static int GetPositionInDatabase(Dictionary<byte, string> database, byte key)
        {
            int position = database.TakeWhile(pair => !pair.Key.Equals(key)).Count();
            return position == database.Count ? 2 : position;
        }

        private void SubscribeToEvents()
        {
            _re1.Player.Room.Updated += Updated_Room;
            _re1.Player.Stage.Updated += Updated_Stage;
            _re1.Player.Character.Updated += Updated_Character;
            _re1.Player.Health.Updated += Updated_Health;
            _re1.Player.CharacterHealthState.Updated += Update_Character_State;

            _re1.Inventory.Capacity.Updated += Updated_Inventory_Capacity;

            _re1.Game.State.Updated += Updated_GameState;
            _re1.Game.Timer.Updated += Updated_Timer;
            _re1.Game.MainMenu.Updated += Updated_Reset;

            AssignEventHandlers();

            _re1.Rebirth.Debug.Updated += Updated_Debug;
            //_re1.Rebirth.Screen.Updated += Updated_Screen;
            _re1.Player.LastItemFound.Updated += Updated_LastItemFound;
            _re1.Player.InventorySlotSelected.Updated += Updated_InventorySlotSelected;
        }

        private void Updated_Reset(object sender, EventArgs e) => InvokeUI(() =>
        {
            if (_re1.Game.MainMenu.Value == 1 && (Int32.Parse(labelHealth.Text) <= _health_table[(byte)_re1.Player.Character.Value][0]))
            {
                _race_db.Resets += 1;
                labelResets.Text = _race_db.Resets.ToString();
            }
        });

        private void Update_Character_State(object sender, EventArgs e) => InvokeUI(() =>
        {
            CheckCharacterHealthState(_re1.Player.CharacterHealthState.Value);
        });

        private void Updated_Inventory_Capacity(object sender, EventArgs e) => InvokeUI(() =>
        {
            CheckInventoryCapacity(_re1.Inventory.Capacity.Value, pictureBoxItemSlot7, pictureBoxItemSlot8, labelSlot7Quantity, labelSlot8Quantity);
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
            _race_db.Debugs = CheckDebugWindow(_re1.Rebirth.Debug.Value) == "Active" ? _race_db.Debugs + 1 : _race_db.Debugs;
            labelDebug.Text = _race_db.Debugs.ToString();
        });

        private void AssignEventHandlers()
        {
            for (int i = 0; i < 8; i++)
            {
                int slotNumber = i; // To capture the variable in the closure
                _re1.Inventory.Slots[i].Item.Updated += (sender, e) => InvokeUI(() =>
                {
                    UpdateSlotPicture(slotNumber);
                });

                _re1.Inventory.Slots[i].Quantity.Updated += (sender, e) => InvokeUI(() =>
                {
                    UpdateSlotCapacity(slotLabels[slotNumber], slotNumber);
                });
            }

        }

        private void UpdateRaceKeyItem(int value, string room, int state)
        {
            // 0 - Not found
            // 1 - Seen
            // 2 - Taken
            // value = GetKeyItemPosition(value, room, state);

            value = GetKeyItemPosition(value, room, state);

            if (_race_db.KeyItems[value].State < state)
            {
                _race_db.KeyItems[value].State = state;
                UpdatePictureKeyItemState(value);
            }

            _race_db.KeyItems[value].Room = room;
        }

        private int GetKeyItemPosition(int value, string room, int state)
        {
            var Name = _db_items.GetPropertyNameById((byte)value);
            var item_box = (_re1.Game.State.Value & 0x0000FF00) == 0x90;
            var found = 0;

            for (int i = 0; i < _race_db.KeyItems.Count; i++)
            {
                if (_race_db.KeyItems[i].Data.Name == Name)
                {
                    found = i;
                    if ((_race_db.KeyItems[i].Room != room || _race_db.KeyItems[i].State != state) && !item_box)
                    {
                        return i;
                    }
                }
            }

            return found != 0 ? found : 0;
        }

        private void UpdatePictureKeyItemState(int value)
        {
            var state = _race_db.KeyItems[value].State;

            // change background color of the picture box
            pictureKeyItems[value].BackColor = state switch
            {
                0 => Color.Transparent,
                1 => Color.FromArgb(198, 155, 101),
                2 => Color.FromArgb(159, 185, 118),
                _ => pictureKeyItems[value].BackColor // Default case
            };
        }

        private void UpdateSlotPicture(int value)
        {
            if (value > _inventory_size)
            {
                return;
            }

            var item_id = _re1.Inventory.Slots[value].Item.Value;
            // if value is key item, update the key item
            if (_db_items.GetPropertyById((byte)_re1.Inventory.Slots[value].Item.Value).Type == "Key Item")
            {
                if ((_re1.Game.State.Value & 0x0000FF00) == 0x8800)
                { 
                    UpdateRaceKeyItem(item_id, _race_db.FullRoomName, 2);
                }
            }

            var id = (byte)GetItemByPosition(_re1.Inventory.Slots[value].Item.Value);
            var image = _db_items.GetPropertyById(id).Img;
            pictures[value].Image = image;

            UpdateSlotCapacity(slotLabels[value], value);
        }
        private void UpdateSlotCapacity(Label label, int value)
        {
            if (value > _inventory_size)
            {
                return;
            }

            Property item = _db_items.GetPropertyById((byte)_re1.Inventory.Slots[value].Item.Value);
            label.Text = _re1.Inventory.Slots[value].Quantity.Value.ToString();
            label.Font = PixelBoySegments;
            label.Visible = !(new[] { "Key Item", "Optionnal Key Item", "Nothing" }).Contains(item.Type);
            label.ForeColor = item.Color switch
            {
                "Yellow" => Color.FromArgb(215, 191, 128),
                "Orange" => Color.FromArgb(198, 155, 101),
                "Red" => Color.FromArgb(205, 116, 118),
                _ => Color.FromArgb(159, 185, 118),
            };
        }


        private void Updated_Health(object sender, EventArgs e) => InvokeUI(() =>
        {
            CheckHealthLabel(_re1.Player.Health.Value);
        });

        private void Updated_Character(object sender, EventArgs e) => InvokeUI(() =>
        {
            byte value = (byte)_re1.Player.Character.Value;
            labelCharacter.Text = ((Dictionary<byte, string>)_re1.Player.Character.Database)[value].ToString();
        });

        private void Updated_Stage(object sender, EventArgs e) => InvokeUI(() => _race_db.stage = ((_re1.Player.Stage.Value % 5) + 1).ToString());

        private void Updated_Room(object sender, EventArgs e) => InvokeUI(() =>
        {
            _race_db.room = _re1.Player.Room.Value.ToString("X2");
            _race_db.LastRoomName = _race_db.FullRoomName;
            _race_db.FullRoomName = _race_db.stage + _race_db.room;

            UpdateKeyRooms();
        });

        private void UpdateKeyRooms()
        {
            string fullRoomName = _race_db.FullRoomName;
            string lastRoomName = _race_db.LastRoomName ?? fullRoomName;

            foreach (var roomName in new[] { lastRoomName, fullRoomName })
            {
                if (_race_db.KeyRooms.ContainsKey(roomName))
                {
                    List<string> keyRooms = _race_db.KeyRooms[roomName];
                    string otherRoomName = roomName == lastRoomName ? fullRoomName : lastRoomName;

                    if (!keyRooms.Contains(otherRoomName) && otherRoomName != roomName)
                    {
                        keyRooms.Add(otherRoomName);

                        if (keyRooms.Count == 2)
                        {
                            UpdateChronometers();
                            _race_db.KeyRooms.Remove(roomName);
                        }

                        // Update the list in the dictionary
                        _race_db.KeyRooms[roomName] = keyRooms;
                    }
                }
            }
        }

        private void UpdateChronometers()
        {
            _segment_chronometers[_race_db.Segments].Stop();
            _race_db.Segments += 1;
            _segment_chronometers[_race_db.Segments].Start();
        }

        private void Updated_GameState(object sender, EventArgs e) => InvokeUI(() =>
        {
            int state = _re1.Game.State.Value;

            if((state & 0xFF000000) == 0x54000000)
            {
                LoadState();
            }

            if ((state & 0x0F000000) == 0x1000000 && _race_db.old_state != 0x1000000)
            {
                _race_db.Deaths += 1;
                labelHealth.Text = "255";
                labelDeaths.Text = _race_db.Deaths.ToString();
            }

            _race_db.old_state = state & 0x0F000000;
        });

        private void LoadState()
        {
            foreach (var save in _saves)
            {
                if (save._tickTimer == _re1.Game.Timer.Value)
                {
                    InitKeyItems();

                    if(_race_db.Segments < save.Segments)
                    {
                        _segment_chronometers[_race_db.Segments].Stop();
                        _race_db.Segments = save.Segments;
                        _segment_chronometers[_race_db.Segments].Start();
                    }
                    
                    
                    if (_race_db.Debugs < save.Debugs)
                    {
                        _race_db.Debugs = save.Debugs;
                    }

                    if (_race_db.Deaths < save.Deaths)
                    {
                        _race_db.Deaths = save.Deaths;
                    }

                    if (_race_db.Resets < save.Resets)
                    {
                        _race_db.Resets = save.Resets;
                    }

                    if(_race_db.Saves < save.Saves)
                    {
                        _race_db.Saves = save.Saves;
                    }
                    
                    _race_db.KeyRooms = save.KeyRooms;
                    _race_db.KeyItems = save.KeyItems;
                }
            }
        }

        private void LoadKeyItems(GameData.KeyItem keyItem)
        {
            // Create a new list to hold the updated key items
            List<KeyItem> updatedKeyItems = new List<KeyItem>();

            // Find the corresponding key item in _race_db.KeyItems
            KeyItem existingKeyItem = _race_db.KeyItems.FirstOrDefault(ki => ki.Data.Name == keyItem.Data.Name);

            // If the key item exists in _race_db.KeyItems, use the existing Property.Img value
            if (existingKeyItem != null)
            {
                keyItem.Data.Img = existingKeyItem.Data.Img;

                // If the state or room is different, call UpdateRaceKeyItem
                if (existingKeyItem.State != keyItem.State || existingKeyItem.Room != keyItem.Room)
                {
                    // Assuming the value parameter for UpdateRaceKeyItem is the index of the key item in the list
                    int value = _race_db.KeyItems.IndexOf(existingKeyItem);
                    UpdateRaceKeyItem(value, keyItem.Room, keyItem.State);
                }
            }

            // Add the key item to the updated list
            updatedKeyItems.Add(keyItem);

            // Replace _race_db.KeyItems with the updated list
            _race_db.KeyItems = updatedKeyItems;
        }

        private void Updated_Timer(object sender, EventArgs e) => InvokeUI(() =>
        {
            labelTimer.Text = chronometer.Elapsed.ToString(@"hh\:mm\:ss\.ff");

            Label[] labelSegTimers = [labelSegTimer1, labelSegTimer2, labelSegTimer3];

            if (_race_db.Segments >= 0 && _race_db.Segments < labelSegTimers.Length)
            {
                labelSegTimers[_race_db.Segments].Text = _segment_chronometers[_race_db.Segments].Elapsed.ToString(@"hh\:mm\:ss\.ff");
            }

            int currentTimerValue = _re1.Game.Timer.Value;

            if (previousTimerValue != currentTimerValue)
            {
                if (!chronometer.IsRunning)
                {
                    chronometer.Start();
                    _segment_chronometers[_race_db.Segments].Start();
                }
            }
            else if (chronometer.IsRunning)
            {
                chronometer.Stop();
                _segment_chronometers[_race_db.Segments].Stop();
            }
        });


        public static string ToHexString(int value) => string.Join(" ", Enumerable.Range(0, 4).Select(i => value.ToString("X8").Substring(i * 2, 2)));



        private void CheckInventoryCapacity(int value, PictureBox slot7, PictureBox slot8, Label capacity7, Label capacity8)
        {
            _inventory_size = (value & 0xFF000000) == 0x08000000 ? 6 : 8;
            bool isVisible = _inventory_size != 6;

            slot7.Visible = isVisible;
            slot8.Visible = isVisible;

            if (isVisible)
            {
                UpdateSlotCapacity(capacity7, 6);
                UpdateSlotCapacity(capacity8, 7);
            }
            else
            {
                capacity7.Visible = isVisible;
                capacity8.Visible = isVisible;
                slot7.Visible = isVisible;
                slot8.Visible = isVisible;
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
            var health_table = _health_table[(byte)_re1.Player.Character.Value];
            var status = _re1.Player.CharacterHealthState.Value;
            Color[] colors = [Color.DarkGreen, Color.FromArgb(159, 185, 118), Color.FromArgb(215, 191, 128), Color.FromArgb(198, 155, 101), Color.FromArgb(205, 116, 118), Color.FromArgb(250, 240, 216)];
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
                CheckHealthLabel(_re1.Player.Health.Value);
            }
            else
            {
                labelHealth.ForeColor = Color.DarkViolet;
            }
        }

        public string CheckDebugWindow(int value)
        {
            if ((value & 0xFFFF) == 0x1)
            {
                return "Active";
            }

            return "Inactive";
        }

        public string CheckRebirthState(int value)
        {
            if ((_re1.Rebirth.Debug.Value & 0xFFFF) == 0x1)
            {
                return "Active";
            }

            return "Inactive";
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // clear everything completely
            _race_db = null;
            pictureKeyItems = null;

            // UnsubscribeAllEvents();
            ErasePlayerData();

            // re-init everything
            _race_db = new GameData.PlayerRaceProgress(_gameName);
            _race_db.Segments = 0;

            InitKeyItems();
            InitKeyRooms();

            InitLabels();
            InitChronometers();

            CheckInventoryCapacity(_re1.Inventory.Capacity.Value, pictureBoxItemSlot7, pictureBoxItemSlot8, labelSlot7Quantity, labelSlot8Quantity);

            InitInventory();
            InitCharacterHealthState();
            // SubscribeToEvents();
        }

        private void ErasePlayerData()
        {
            for (int i = 0; i < 8; i++)
            {
                Write(_re1.Inventory.Slots[i].Item, 0);
                Write(_re1.Inventory.Slots[i].Quantity, 0);
            }

            for (int i = 0; i < 47; i++)
            {
                Write(_re1.ItemBox.Slots[i].Item, 0);
                Write(_re1.ItemBox.Slots[i].Quantity, 0);
            }

            Write(_re1.Player.InventorySlotSelected, 0);
            Write(_re1.Player.LastItemFound, 0);
        }

        private void Write(VariableData v, int value)
        {
            lock (v.LockObject)
            {
                v.IsUpdated = true;
                v.Value = value;
            }
        }

        private void UnsubscribeAllEvents()
        {
            _re1.Player.Room.Updated -= Updated_Room;
            _re1.Player.Stage.Updated -= Updated_Stage;
            _re1.Player.Character.Updated -= Updated_Character;
            _re1.Player.Health.Updated -= Updated_Health;
            _re1.Player.CharacterHealthState.Updated -= Update_Character_State;

            _re1.Inventory.Capacity.Updated -= Updated_Inventory_Capacity;

            _re1.Game.State.Updated -= Updated_GameState;
            _re1.Game.Timer.Updated -= Updated_Timer;
            _re1.Game.MainMenu.Updated -= Updated_Reset;

            _re1.Rebirth.Debug.Updated -= Updated_Debug;

            _re1.Player.LastItemFound.Updated -= Updated_LastItemFound;
            _re1.Player.InventorySlotSelected.Updated -= Updated_InventorySlotSelected;
            //_re1.Rebirth.Screen.Updated -= Updated_Screen;

            for (int i = 0; i < 8; i++)
            {
                _re1.Inventory.Slots[i].Item.Updated -= (sender, e) => { };
                _re1.Inventory.Slots[i].Quantity.Updated -= (sender, e) => { };
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            string seed = "";

            for (int i = 0; i < 8; i++)
            {
                var slot = _re1.Inventory.Slots[i];
                seed += slot.Item.Value.ToString();
                seed += slot.Quantity.Value.ToString();
            }

            SeedChecker seedChecker = new SeedChecker(seed);
            seedChecker.Show();

            Console.WriteLine(seed);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
        }
    }

    public class KRoom
    {
        public List<string> KeyRooms { get; set; }
    }
}
