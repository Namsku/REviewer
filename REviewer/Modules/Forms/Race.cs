using REviewer.Modules.RE;
using Label = System.Windows.Forms.Label;
using REviewer.Modules.Utils;
using REviewer.Modules.SRT;
using REviewer.Modules.RE.Common;

namespace REviewer.Modules.Forms
{
    public partial class Race : Form
    {
        private readonly RootObject _game;
        private readonly ItemIDs _itemDatabase;
        private int _raceWatch = new();
        private List<int> _segmentWatch = [0,0,0,0];

        private readonly int? _previousTimerValue = null;
        private int _previousSelectedSlot = 0;
        private int _inventoryCapacitySize = 8;

        private Font _pixelBoyHealthFont;
        private Font _pixelBoySegments;
        private Font _pixelBoyDefault;

        private FontFamily _fontPixelBoy;

        private Dictionary<int, Label> _slotLabels;
        private Dictionary<int, PictureBox> _pictures;
        private Dictionary<int, PictureBox>? _pictureKeyItems;

        private readonly string _gameName;

        private PlayerRaceProgress? _raceDatabase;
        private List<PlayerRaceProgress> _saves;

        private MonitorVariables? _monitorVariables;

        private int _currentID;
        public void SetMonitorVariables(MonitorVariables monitorVariables)
        {
            _monitorVariables = monitorVariables;
        }

        private static readonly string[] _itemTypes = ["Key Item", "Optionnal Key Item", "Nothing"];

        public Race(RootObject GameData, string gameName)
        {
            // Font Loading
            InitPixelBoyFont();
            LoadDefaultPixelBoyFont();
            LoadCustomFontPixelBoyHealth();
            LoadCustomFontPixelBoySegments();

            // Initialize the form
            InitializeComponent();

            // Init the Key Items and Key Rooms from the loaded game data
            InitLoadState();

            // Init the Inventory
            InitInventorySlots();

            // Init the main classes
            _game = GameData;
            _gameName = gameName;
            _itemDatabase = new ItemIDs(gameName);
            _raceDatabase = new PlayerRaceProgress(gameName);
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
                            Logger.Instance.Error("ObjectDisposedException in InvokeUI");
                        }
                    }
                }
                else
                {
                    action();
                }
            }
        }

        private void LoadDefaultPixelBoyFont()
        {
            _pixelBoyDefault = new Font(_fontPixelBoy, 53, FontStyle.Bold, GraphicsUnit.Pixel);
        }

        private void LoadCustomFontPixelBoyHealth()
        {
            _pixelBoyHealthFont = new Font(_fontPixelBoy, 60, FontStyle.Regular, GraphicsUnit.Pixel);
        }

        private void LoadCustomFontPixelBoySegments()
        {
            _pixelBoySegments = new Font(_fontPixelBoy, 36, FontStyle.Regular, GraphicsUnit.Pixel);
        }

        private void Race_Load(object sender, EventArgs e)
        {
            InitSaveMonitoring();

            InitLabels();
            InitChronometers();

            InitKeyItems();
            InitKeyRooms();

            InitInventory();
            InitCharacterHealthState();
            SubscribeToEvents();
            CheckInventoryCapacity(_game.Inventory.Capacity.Value, pictureBoxItemSlot7, pictureBoxItemSlot8, labelSlot7Quantity, labelSlot8Quantity);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Close();
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
            _game.Player.LockPick.Updated += Updated_Lockpick;

            _game.Inventory.Capacity.Updated += Updated_Inventory_Capacity;

            _game.Game.State.Updated += Updated_GameState;
            _game.Game.Timer.Updated += Updated_Timer;
            _game.Game.MainMenu.Updated += Updated_Reset;
            _game.Game.SaveContent.Updated += Updated_SaveState;

            CreateSlotsUpdatedEvents();

            _game.Rebirth.Debug.Updated += Updated_Debug;
            //_game.Rebirth.Screen.Updated += Updated_Screen;
            _game.Player.LastItemFound.Updated += Updated_LastItemFound;
            _game.Player.InventorySlotSelected.Updated += Updated_InventorySlotSelected;
        }

        private void CreateSlotsUpdatedEvents()
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

        private int GetKeyItemPosition(int value, string room, int state)
        {

            var name = _itemDatabase.GetPropertyNameById((byte)value);
            var item_box = (_game.Game.State.Value & 0x0000FF00) == 0x90;
            var position = 0;

            for (int i = 0; i < _raceDatabase.KeyItems.Count; i++)
            {
                if (_raceDatabase.KeyItems[i].Data.Name == name) position = i;
                if (_raceDatabase.KeyItems[i].Data.Name == name && (_raceDatabase.KeyItems[i].Room == room && _raceDatabase.KeyItems[i].State < state) && !item_box) return i;
                if (_raceDatabase.KeyItems[i].Data.Name == name && _raceDatabase.KeyItems[i].State == -1) return i;
            }

            return position;
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

        private void LoadKeyItems(List<KeyItem> keyItems)
        {
            int i = 0;

            foreach (var keyItem in keyItems)
            {
                // Find the corresponding key item in _raceDatabase.KeyItems
                KeyItem? existingKeyItem = _raceDatabase.KeyItems.FirstOrDefault(ki => ki.Data.Name == keyItem.Data.Name);

                if (existingKeyItem == null)
                {
                    return;
                }

                // If the key item exists in _raceDatabase.KeyItems, use the existing Property.Img value

                if (existingKeyItem.State != keyItem.State || existingKeyItem.Room != keyItem.Room)
                {
                    Logger.Instance.Debug($"Updating Key Item: Name={keyItem.Data.Name}, Room={keyItem.Room}, State={keyItem.State}, ExistingKeyItem: Name={existingKeyItem.Data.Name}, Room={existingKeyItem.Room}, State={existingKeyItem.State}");
                    // Assuming the value parameter for UpdateRaceKeyItem is the index of the key item in the list
                    int value = _itemDatabase.GetPropertyIdByName(keyItem.Data.Name);
                    UpdateRaceKeyItem(value, keyItem.Room, keyItem.State, true);
                }
                i++;

            }
        }




        private void CheckInventoryCapacity(int value, PictureBox slot7, PictureBox slot8, Label capacity7, Label capacity8)
        {
            int[] _inventoryCapacityArray = [6, 8, 8, 6];

            if (_inventoryCapacitySize == _inventoryCapacityArray[value & 3])
            {
                // avoiding extra checking for nothing here, it's only necessary when the player is switching between jill and chris.
                return;
            }

            _inventoryCapacitySize = _inventoryCapacityArray[value & 3];
            bool isVisible = _inventoryCapacitySize > 6;

            slot7.Visible = isVisible;
            slot8.Visible = isVisible;
            capacity7.Visible = isVisible;
            capacity8.Visible = isVisible;

            if (isVisible)
            {
                UpdateSlotPicture(6);
                UpdateSlotCapacity(capacity7, 6);

                UpdateSlotPicture(7);
                UpdateSlotCapacity(capacity8, 7);
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
            var health_table = ((Dictionary<byte, List<int>>)_game.Player.Health.Database)[(byte)(_game.Player.Character.Value & 0x03)];
            var status = _game.Player.CharacterHealthState.Value;
            Color[] colors = [CustomColors.Blue, CustomColors.Default, CustomColors.Yellow, CustomColors.Orange, CustomColors.Red, CustomColors.White];
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
                labelHealth.ForeColor = CustomColors.Lavender;
            }
        }

        public static string CheckDebugWindow(int value) => (value & 0xFFFF) == 0x1 ? "Active" : "Inactive";

        private void ButtonReset_Click(object sender, EventArgs e) => InvokeUI(() =>
        {

            // clear everything completely
            _raceDatabase = null;
            _pictureKeyItems = null;

            // UnsubscribeAllEvents();
            ErasePlayerData();

            labelDeaths.Text = "0";
            labelDebug.Text = "0";
            labelResets.Text = "0";
            labelSaves.Text = "0";

            // re-init everything
            _raceDatabase = new PlayerRaceProgress(_gameName)
            {
                Segments = 0
            };

            InitSaveMonitoring();
            InitChronometers();
            InitKeyItems();
            InitKeyRooms();
            InitInventory();

            InitCharacterHealthState();

            CheckInventoryCapacity(_game.Inventory.Capacity.Value, pictureBoxItemSlot7, pictureBoxItemSlot8, labelSlot7Quantity, labelSlot8Quantity);

            labelGameCompleted.Visible = false;
            // SubscribeToEvents();
        });

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
            Write(_game.Player.LockPick, 0);
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
            _game.Game.SaveContent.Updated -= Updated_SaveState;

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
        }

    }

    public class KRoom
    {
        public List<string> KeyRooms { get; set; }
    }
}
