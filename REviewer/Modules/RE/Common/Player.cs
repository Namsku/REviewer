using REviewer.Modules.RE.Json;
using REviewer.Modules.SRT;
using REviewer.Modules.Utils;
using System.ComponentModel;
using System.Diagnostics;
using System.Printing.IndexedProperties;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Text;
using REviewer.Core.Constants;
namespace REviewer.Modules.RE.Common
{
    public partial class RootObject : INotifyPropertyChanged
    {
        private VariableData? _character;
        public ImageItem PrivateImageItem = new ImageItem();
        
        public VariableData? Character
        {
            get { return _character; }
            set
            {
                if (_character != value)
                {
                    if (_character != null)
                    {
                        _character.PropertyChanged -= Character_PropertyChanged;
                    }

                    _character = value;

                    if (_character != null)
                    {
                        _character.PropertyChanged += Character_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(Character));
                }
            }
        }

        private void Character_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (SELECTED_GAME == GameConstants.BIOHAZARD_3)
                {
                    if (_bio != null)
                    {
                        if (Character?.Value == 0x08)
                        {
                            InitInventory(_bio, true);
                            InitItemBox(_bio, true);
                        }
                        else
                        {
                            InitInventory(_bio);
                            InitItemBox(_bio);
                        }
                    }
                }

                OnPropertyChanged(nameof(CharacterName));
                OnPropertyChanged(nameof(MaxHealth));
            }
        }

        public string? CharacterName
        {
            get
            {
                if (_character?.Database == null) return "ERROR";

                var length = ((Dictionary<byte, string>)_character.Database).Count - 1;
                return ((Dictionary<byte, string>)_character.Database)[(byte)(_character.Value & length)].ToString();
            }
        }

        public string? _maxHealth;
        public string? MaxHealth
        {
            set
            {
                if (_maxHealth != value)
                {
                    _maxHealth = value;
                    OnPropertyChanged(nameof(CarlosInventorySlotSelected));
                }
            }
            get
            {
                if (SELECTED_GAME == GameConstants.BIOHAZARD_2) return _maxHealth;
                if (_character?.Database == null) return "ERR";
                if (_health?.Database == null) return "0";

                var length = ((Dictionary<byte, string>)_character.Database).Count - 1;
                return ((Dictionary<byte, List<int>>)_health.Database)[(byte)(_character.Value & length)][0].ToString();
            }
        }

        private VariableData? _carlosInventorySlotSelected;

        public VariableData? CarlosInventorySlotSelected
        {
            get { return _carlosInventorySlotSelected; }
            set
            {
                if (_carlosInventorySlotSelected != value)
                {
                    if (_carlosInventorySlotSelected != null)
                    {
                        _carlosInventorySlotSelected.PropertyChanged -= CarlosInventorySlotSelected_PropertyChanged;
                    }

                    _carlosInventorySlotSelected = value;

                    if (_carlosInventorySlotSelected != null)
                    {
                        _carlosInventorySlotSelected.PropertyChanged += CarlosInventorySlotSelected_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(CarlosInventorySlotSelected));
                }
            }
        }

        private async void CarlosInventorySlotSelected_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                await Task.Delay(50);
                if (InventorySlotSelected != null && CarlosInventorySlotSelected != null)
                {
                    InventorySlotSelected.Value = CarlosInventorySlotSelected.Value;
                }
                OnPropertyChanged(nameof(InventorySlotSelectedImage));
            }
        }

        private VariableData? _inventorySlotSelected;
        public VariableData? InventorySlotSelected
        {
            get { return _inventorySlotSelected; }
            set
            {
                if (_inventorySlotSelected != value)
                {
                    if (_inventorySlotSelected != null)
                    {
                        _inventorySlotSelected.PropertyChanged -= InventorySlotSelected_PropertyChanged;
                    }

                    _inventorySlotSelected = value;

                    if (_inventorySlotSelected != null)
                    {
                        _inventorySlotSelected.PropertyChanged += InventorySlotSelected_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(InventorySlotSelected));
                }
            }
        }

        private async void InventorySlotSelected_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                await Task.Delay(50);
                OnPropertyChanged(nameof(InventorySlotSelectedImage));
                OnPropertyChanged(nameof(InventorySlotSelectedQuantity));
            }
        }

        public string InventorySlotSelectedQuantity
        {
            get
            {
                if (Inventory == null || InventorySlotSelected == null) return "";
                
                int index = InventorySlotSelected.Value;
                
                // Handle RE1 weirdness where 0x80 is nothing/equipping?
                if (index < 0 || index >= Inventory.Count) return "";
                
                // For RE1, index 0 might be special or 1-based, need to match Image logic
                // The Image logic uses: var selected = InventorySlotSelected.Value - 1; for RE1/CVX
                
                int selectedIndex = index;
                if (SELECTED_GAME == GameConstants.BIOHAZARD_1 || SELECTED_GAME == GameConstants.BIOHAZARD_CVX)
                {
                     selectedIndex = index - 1;
                }
                
                if (selectedIndex >= 0 && selectedIndex < Inventory.Count)
                {
                     var qty = Inventory[selectedIndex].Quantity?.Value ?? 0;
                     var item = Inventory[selectedIndex].Item?.Value ?? 0;
                     
                     // Don't show quantity for items that don't satisfy criteria (similar to UpdateTextInventoryImage)
                     // But user asked for "Counter of what you have on item selected"
                     // We probably want to show it even if it's 1, or maybe match inventory logic?
                     // Let's just return the raw quantity for now, or maybe formatted
                     
                     // Check if item has infinite ammo or percentage?
                     return qty.ToString();
                }
                
                return "";
            }
        }

        public string InventorySlotSelectedImage
        {
            get
            {
                var items = IDatabase.GetItems();

                // Console.WriteLine(_inventorySlotSelected.Value);

                if (_inventorySlotSelected != null && IDatabase != null)
                {
                    byte id = 0;

                    if (_inventorySlotSelected.Value == 0x80)
                    {
                        InventoryImagesSelected = PrivateImageItem;
                        OnPropertyChanged(nameof(InventoryImagesSelected));
                        return "./resources/re1/nothing.png";
                    }

                    if ((SELECTED_GAME == GameConstants.BIOHAZARD_1 || SELECTED_GAME == GameConstants.BIOHAZARD_CVX) && InventorySlotSelected != null && Inventory != null)
                    {
                        var selected = InventorySlotSelected.Value - 1;

                        id = InventorySlotSelected.Value == 0 ? (byte)0 :
                            (selected >= 0 && selected < Inventory.Count && Inventory[selected]?.Item != null)
                                ? (byte)Inventory[selected].Item.Value : (byte)0;


                        if (selected == -1)
                        {
                            InventoryImagesSelected = PrivateImageItem;
                            OnPropertyChanged(nameof(InventoryImagesSelected));
                        }
                        else
                        {
                            InventoryEquippedOverlay = selected;
                            InventoryImagesSelected = InventoryImages[selected];
                        }
                    }
                    else if ((SELECTED_GAME == GameConstants.BIOHAZARD_2 || SELECTED_GAME == GameConstants.BIOHAZARD_3) && Character != null && Inventory != null)
                    {
                        int? vvv = null;
                        if (Character.Value == 0x8 && CarlosInventorySlotSelected != null)
                            vvv = CarlosInventorySlotSelected.Value;
                        else if (InventorySlotSelected != null)
                            vvv = InventorySlotSelected.Value;

                        InventoryEquippedOverlay = vvv;
                        InventoryImagesSelected = InventoryImages[vvv ?? 00];
                        // Console.WriteLine($"Changing {vvv}");

                        if (vvv != null)
                        {
                            var selected = vvv.Value;
                            selected = selected < 0 ? 0 : selected;
                            id = (selected == 0xFF || selected == 0x80) ? (byte)0 :
                                (selected >= 0 && selected < Inventory.Count && Inventory[selected]?.Item != null)
                                    ? (byte)Inventory[selected].Item.Value : (byte)0;
                        }
                    }
                    return items[id].Img;
                }

                InventoryImagesSelected.TextVisibility = Visibility.Hidden;
                return "./resources/re1/unknown.png";
            }
        }

        private VariableData? _stage;
        public VariableData? Stage
        {
            get { return _stage; }
            set
            {
                if (_stage != value)
                {
                    _stage = value;
                    OnPropertyChanged(nameof(Stage));
                }
            }
        }

        private VariableData? _room;
        public VariableData? Room
        {
            get { return _room; }
            set
            {
                if (_room != value)
                {
                    if (Room != null)
                    {
                        Room.PropertyChanged -= Room_PropertyChanged;
                    }

                    _room = value;

                    if (Room != null)
                    {
                        Room.PropertyChanged += Room_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(Room));
                }
            }
        }

        private void Room_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var processName = IDatabase.GetProcessName();
            var max_stage_id = MAX_STAGE_ID[processName ?? "ERR"];

            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (Stage == null || Room == null) return;

                var sss = (((int)Stage.Value % max_stage_id) + 1).ToString();
                var rrr = ((int)Room.Value).ToString("X2");
                LastRoomName = FullRoomName ?? "000";
                FullRoomName = sss + rrr;

                UpdateKeyRooms();
            }
        }

        private VariableData? _cutscene;
        public VariableData? Cutscene
        {
            get { return _cutscene; }
            set
            {
                if (_cutscene != value)
                {
                    _cutscene = value;
                    OnPropertyChanged(nameof(Cutscene));
                }
            }
        }

        private VariableData? _lastCutscene;
        public VariableData? LastCutscene
        {
            get { return _lastCutscene; }
            set
            {
                if (_lastCutscene != value)
                {
                    _lastCutscene = value;
                    OnPropertyChanged(nameof(LastCutscene));
                }
            }
        }

        private VariableData? _lastRoom;
        public VariableData? LastRoom
        {
            get { return _lastRoom; }
            set
            {
                if (_lastRoom != value)
                {
                    _lastRoom = value;
                    OnPropertyChanged(nameof(LastRoom));
                }
            }
        }


        private void UpdateKeyRooms()
        {
            if (FullRoomName == null || KeyRooms == null) return;

            string fullRoomName = FullRoomName;
            string lastRoomName = LastRoomName ?? fullRoomName;

            foreach (var roomName in new[] { lastRoomName, fullRoomName })
            {
                if (KeyRooms.TryGetValue(roomName, out List<string>? value))
                {
                    List<string> keyRooms = value;
                    string otherRoomName = roomName == lastRoomName ? fullRoomName : lastRoomName;

                    if (!keyRooms.Contains(otherRoomName) && otherRoomName != roomName)
                    {
                        keyRooms.Add(otherRoomName);

                        if (keyRooms.Count == 2 && lastRoomName != "503")
                        {
                            UpdateChronometers();
                            KeyRooms.Remove(roomName);
                        }
                        else if (fullRoomName == "505" && lastRoomName == "503")
                        {
                            UpdateChronometers();
                            KeyRooms.Remove(roomName);
                        }
                    }

                    KeyRooms[roomName] = keyRooms;
                }
            }
        }

        private VariableData? _unk002;
        public VariableData? Unk002
        {
            get { return _unk002; }
            set
            {
                if (_unk002 != value)
                {
                    _unk002 = value;
                    OnPropertyChanged(nameof(Unk002));
                }
            }
        }

        private VariableData? _event;
        public VariableData? Event
        {
            get { return _event; }
            set
            {
                if (_event != value)
                {
                    _event = value;
                    OnPropertyChanged(nameof(Event));
                }
            }
        }


        private VariableData? _carlosLastItemFound;
        public VariableData? CarlosLastItemFound
        {
            get { return _carlosLastItemFound; }
            set
            {
                if (_carlosLastItemFound != value)
                {
                    if (_carlosLastItemFound != null)
                    {
                        _carlosLastItemFound.PropertyChanged -= CarlosLastItemFound_PropertyChanged;
                    }

                    _carlosLastItemFound = value;

                    if (_carlosLastItemFound != null)
                    {
                        _carlosLastItemFound.PropertyChanged += CarlosLastItemFound_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(CarlosLastItemFound));
                }
            }
        }

        private void CarlosLastItemFound_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                OnPropertyChanged(nameof(LastItemFoundImage));
            }
        }

        private VariableData? _lastItemFound;
        public VariableData? LastItemFound
        {
            get { return _lastItemFound; }
            set
            {
                if (_lastItemFound != value)
                {
                    if (_lastItemFound != null)
                    {
                        _lastItemFound.PropertyChanged -= LastItemFound_PropertyChanged;
                    }

                    _lastItemFound = value;

                    if (_lastItemFound != null)
                    {
                        _lastItemFound.PropertyChanged += LastItemFound_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(LastItemFound));
                }
            }
        }

        private void LastItemFound_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                OnPropertyChanged(nameof(LastItemFoundImage));
            }
        }

        public string? LastItemFoundImage
        {
            get
            {
                if (_lastItemFound != null && IDatabase != null)
                {
                    int? vvv = null;
                    if (Character != null && Character.Value == 8 && CarlosLastItemFound != null)
                        vvv = CarlosLastItemFound.Value;
                    else if (LastItemFound != null)
                        vvv = LastItemFound.Value;

                    byte value = (byte)(LastItemFound?.Value ?? 255);
                    var state = GameState != null ? (GameState.Value & 0xF0000000) : 0;
                    var items = IDatabase.GetItems();

                    if (LastItemFound?.Value == 0x31)
                    {
                        UpdateRaceKeyItem(0x31, "Internal Room", 2, true);
                    }

                    if (IDatabase.GetPropertyTypeById(value) == "Key Item")
                    {
                        var sss = (Stage != null ? (((int)Stage.Value % 5) + 1).ToString() : "1");
                        var rrr = (Room != null ? ((int)Room.Value).ToString("X2") : "00");
                        FullRoomName = sss + rrr;

                        if (((state != 0x8000000 && state != 0x9000000)) && value != 61)
                        {
                            UpdateRaceKeyItem(value, FullRoomName, 1);
                        }
                    }

                    if (SELECTED_GAME == GameConstants.BIOHAZARD_2)
                    {
                        if (value == 80)
                        {
                            Random random = new Random();
                            List<string> easter_egg = new List<string> { "orca.png", "death2024.png", "namsku.png" };
                            int randomIndex = random.Next(0, easter_egg.Count);
                            return "./resources/re2/" + easter_egg[randomIndex];
                        }
                    }

                    return IDatabase.GetPropertyImgById(value);
                }
                return null;
            }
        }

        private VariableData? _inventoryCapacityUsed;
        public VariableData? InventoryCapacityUsed
        {
            get { return _inventoryCapacityUsed; }
            set
            {
                if (_inventoryCapacityUsed != value)
                {
                    _inventoryCapacityUsed = value;
                    OnPropertyChanged(nameof(InventoryCapacityUsed));
                }
            }
        }

        private VariableData? _characterHealthState;
        public VariableData? CharacterHealthState
        {
            get { return _characterHealthState; }
            set
            {
                if (_characterHealthState != value)
                {
                    if (_characterHealthState != null)
                    {
                        _characterHealthState.PropertyChanged -= CharacterHealthState_Updated;
                    }

                    _characterHealthState = value;

                    if (_characterHealthState != null)
                    {
                        _characterHealthState.PropertyChanged += CharacterHealthState_Updated;
                    }

                    OnPropertyChanged(nameof(CharacterHealthState));
                }
            }
        }

        private void CharacterHealthState_Updated(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (Health == null || CharacterHealthState == null) return;
                bool state = false;

                if (SELECTED_GAME == GameConstants.BIOHAZARD_1)
                {
                    state = (_characterHealthState?.Value & 0x40) == 0 && (_characterHealthState?.Value & 0x20) == 0 && (_characterHealthState?.Value & 0x04) == 0 && (_characterHealthState?.Value & 0x02) == 0;
                }
                else if (SELECTED_GAME == GameConstants.BIOHAZARD_2)
                {
                    state = (_characterHealthState?.Value != 0x15);
                }
                else if (SELECTED_GAME == GameConstants.BIOHAZARD_3)
                {
                    state = (_characterHealthState?.Value == 0x04) || (_characterHealthState?.Value == 0x00);
                }
                else if (SELECTED_GAME == GameConstants.BIOHAZARD_CVX && _characterHealthState != null)
                {
                    state = (_characterHealthState.Value != 5) && (_characterHealthState.Value != 7);
                }

                if (state)
                {
                    UpdateHealthColor();
                }
                else if (Health != null)
                {
                    Health.Background = CustomColors.Lavender;
                }

                OnPropertyChanged(nameof(Health));
            }
        }

        public int OldHealth = 0;

        private VariableData? _characterMaxHealth;
        public VariableData? CharacterMaxHealth
        {
            get { return _characterMaxHealth; }
            set
            {
                if (_characterMaxHealth != value)
                {
                    if (_characterMaxHealth != null)
                    {
                        _characterMaxHealth.PropertyChanged -= MaxHealth_PropertyChanged;
                    }

                    _characterMaxHealth = value;

                    if (_characterMaxHealth != null)
                    {
                        _characterMaxHealth.PropertyChanged += MaxHealth_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(CharacterMaxHealth));
                }
            }
        }

        private void MaxHealth_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (SELECTED_GAME == GameConstants.BIOHAZARD_2 && CharacterMaxHealth != null)
                {
                    CharacterMaxHealth.Value = CharacterMaxHealth.Value & 0xFFFF;
                    MaxHealth = CharacterMaxHealth.Value.ToString();
                    OnPropertyChanged(nameof(MaxHealth));
                }
            }
        }

        private VariableData? _health;
        public VariableData? Health
        {
            get { return _health; }
            set
            {
                if (_health != value)
                {
                    if (_health != null)
                    {
                        _health.PropertyChanged -= Health_PropertyChanged;
                    }

                    _health = value;

                    if (_health != null)
                    {
                        _health.PropertyChanged += Health_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(Health));
                }
            }
        }

        private void Health_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (SELECTED_GAME == GameConstants.BIOHAZARD_1 && Health != null && Character != null)
                {
                    if (Health.Value < OldHealth)
                    {
                        if (Health.Value != 88 && Character.Value != 3)
                        {
                            if (NoDamage && (
                                !(Health.Value == 140 && Character.Value == 0) ||
                                !(Health.Value == 96 && Character.Value != 1)
                                )
                            )
                            {
                                if (GameState != null)
                                    Monitoring.WriteVariableData(GameState, (int)((long)(GameState.Value & 0xF0FFFFFF) + 0x01000000));
                            }

                            Hits += 1;
                        }
                    }
                }
                else if (SELECTED_GAME == GameConstants.BIOHAZARD_2 && Health != null && GameState != null)
                {
                    Health.Value = Health.Value & 0xFFFF;
                    var state = (GameState.Value & 0xF0000000) != 0x40000000;

                    if (Health.Value < OldHealth && Health.Value != 255 && Health.Value <= 1200)
                    {
                        if (NoDamage)
                        {
                            Monitoring.WriteVariableData(GameState, 0);
                        }

                        Hits += 1;
                    }

                    if (LastRoom != null && Health.Value == 0 && LastRoom.Value == 255 && state && (GameState.Value & 0x20000) != 0x20000)
                    {
                        Resets += 1;
                        PartnerVisibility = Visibility.Hidden;
                        OnPropertyChanged(nameof(PartnerVisibility));
                    }
                }
                else if (SELECTED_GAME == GameConstants.BIOHAZARD_3 && Health != null && GameState != null)
                {
                    if (Health.Value < OldHealth)
                    {
                        if (NoDamage)
                        {
                            Monitoring.WriteVariableData(GameState, 0);
                        }

                        Hits += 1;
                    }
                }
                else if (SELECTED_GAME == GameConstants.BIOHAZARD_CVX && Health != null)
                {
                    if (Health.Value < OldHealth)
                    {
                        Hits += 1;
                    }
                    if (Health.Value < 0 && OldHealth >= 0)
                    {
                        Deaths += 1;
                    }
                }

                if (OneHP && Health != null && Health.Value > 1)
                {
                    Monitoring.WriteVariableData(Health, 1);
                }

                if (Health != null)
                {
                    OldHealth = Health.Value;
                }
                UpdateHealthColor();
            }
        }

        private void UpdateHealthColor()
        {
            if (Character?.Database == null || Health?.Database == null)
            {
                return;
            }

            var size = ((Dictionary<byte, string>)Character.Database).Count - 1;
            var status = CharacterHealthState?.Value;
            var state = false;
            var health_table = ((Dictionary<byte, List<int>>)Health.Database)[(byte)(Character.Value & size)];

            Brush[] colors = new Brush[] { CustomColors.Blue, CustomColors.Default, CustomColors.Yellow, CustomColors.Orange, CustomColors.Red, CustomColors.White };

            if (SELECTED_GAME == GameConstants.BIOHAZARD_1)
            {
                state = (status & 0x20) != 0 || (status & 0x40) != 0 || (status & 0x04) != 0 || (status & 0x02) != 0;
            }
            else if (SELECTED_GAME == GameConstants.BIOHAZARD_2)
            {
                state = (status == 0x15);
            }
            else if (SELECTED_GAME == GameConstants.BIOHAZARD_3)
            {
                state = (status != 0x04);
            }
            else if (SELECTED_GAME == GameConstants.BIOHAZARD_CVX && _characterHealthState != null)
            {
                state = (_characterHealthState.Value == 5) || (_characterHealthState.Value == 7);
            }

            if (state)
            {
                // If special state (poison/dead?), keep current pulse or handle specifically
                // For now, if health is 0, stop pulse
                if (_health?.Value == 0) PulseSpeed = 0;
                return;
            }

            if (_health?.Value >= health_table[0])
            {
                _health.Background = CustomColors.Default;
                PulseSpeed = 0.5;
                return;
            }

            for (int i = 0; i < health_table.Count - 1; i++)
            {
                if (health_table[i] > _health?.Value && _health.Value >= health_table[i + 1])
                {
                    _health.Background = colors[i + 1];

                    // Scale Pulse Speed (SpeedRatio)
                    // Fine (Green) -> Slow (Low Ratio)
                    // Danger (Red) -> Fast (High Ratio)
                    switch (i)
                    {
                        case 0: PulseSpeed = 0.5; break;  // Fine
                        case 1: PulseSpeed = 1.0; break;  // Caution
                        case 2: PulseSpeed = 2.0; break;  // Danger
                        case 3: PulseSpeed = 4.0; break;  // Critical
                        default: PulseSpeed = 4.0; break;
                    }
                }

                // Update ECG Pattern based on state
                UpdateECGPattern(i);
            }
        }

        private void UpdateECGPattern(int state)
        {
            // state: 0=Fine(Green), 1=Caution(Yellow), 2=Danger(Orange), 3=Critical(Red)
            
            // Generate a full-width waveform (approx 600px)
            // We want it to look like a real ECG trace.
            int width = 600;
            StringBuilder sb = new StringBuilder();
            
            double x = 0;
            double baseline = 40;
            
            // Determine HR based on state
            // Fine: ~60 BPM (1 pulse per 150px)
            // Caution: ~90 BPM (1 pulse per 100px)
            // Danger: ~120 BPM (1 pulse per 75px)
            // Critical: ~160 BPM (1 pulse per 50px)
            int spacing = state switch
            {
                0 => 150,
                1 => 100,
                2 => 85,
                3 => 75,
                _ => 150
            };

            while (x < width)
            {
                // Baseline
                sb.Append($"{x},{baseline} ");
                
                // Realistic P-QRS-T complex
                // P Wave (small hump)
                sb.Append($"{x + 5},{baseline - 2} {x + 10},{baseline - 4} {x + 15},{baseline - 2} {x + 20},{baseline} ");
                
                // QRS Complex (sharp spike)
                sb.Append($"{x + 23},{baseline} {x + 25},{baseline + 5} {x + 28},{baseline - 25} {x + 31},{baseline + 10} {x + 33},{baseline} ");
                
                // T Wave (medium hump)
                sb.Append($"{x + 45},{baseline} {x + 52},{baseline - 6} {x + 60},{baseline - 8} {x + 68},{baseline - 6} {x + 75},{baseline} ");
                
                x += spacing;

                // For Danger/Critical, add some random "noise" or instability
                if (state >= 2)
                {
                    baseline = 40 + (new Random().Next(-2, 3));
                }
            }
            
            // Ensure last point is at width
            sb.Append($"{width},40");

            try
            {
                var collection = System.Windows.Media.PointCollection.Parse(sb.ToString());
                collection.Freeze();
                ECGPoints = collection;
            }
            catch {}
        }

        private VariableData? _lockPick;
        public VariableData? LockPick
        {
            get { return _lockPick; }
            set
            {
                if (_lockPick != value)
                {
                    if (_lockPick != null)
                    {
                        _lockPick.PropertyChanged -= LockPick_PropertyChanged;
                    }

                    _lockPick = value;

                    if (_lockPick != null)
                    {
                        _lockPick.PropertyChanged += LockPick_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(LockPick));
                }
            }
        }

        public void LockPick_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (LockPick != null && LockPick.Value == 0x08)
                {
                    UpdateRaceKeyItem(0x31, "Internal Room", 2, true);
                }

                OnPropertyChanged(nameof(LockPick));
            }
        }


        private VariableData _partnerPose;

        public Dictionary<int, string> pose_database = new Dictionary<int, string>
            {
                {0, "???"},
                {1, "Following"},
                {2, "???"},
                {3, "Idle"},
                {4, "Sit Down" },
                {5, "Stop" },
                {6, "Sit Up"},
            };

        public string SherryPose { get; set; }
        public string SherryPicture { get; set; }

        public VariableData PartnerPose
        {
            get
            {
                return _partnerPose;
            }
            set
            {
                if (_partnerPose != null)
                {
                    _partnerPose.PropertyChanged -= PartnerPose_PropertyChanged;
                }

                _partnerPose = value;

                if (_partnerPose != null)
                {
                    _partnerPose.PropertyChanged += PartnerPose_PropertyChanged;
                }

                OnPropertyChanged(nameof(PartnerPose));
            }
        }

        private void PartnerPose_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                string pose = "???";
                if (PartnerPose != null)
                {
                    pose_database.TryGetValue(PartnerPose.Value, out pose);
                }
                SherryPose = pose ?? "???";
                if (pose == "Stop")
                {
                    SherryPicture = "resources/re2/sherry-thug.png";
                }
                else
                {
                    SherryPicture = "resources/re2/sherry.png";
                }

                OnPropertyChanged(nameof(SherryPose));
                OnPropertyChanged(nameof(SherryPicture));
            }
        }

        private VariableData? _partnerPointer;
        public VariableData? PartnerPointer
        {

            get { return _partnerPointer; }
            set
            {
                if (_partnerPointer != value)
                {
                    if (_partnerPointer != null)
                    {
                        _partnerPointer.PropertyChanged -= PartnerPointer_PropertyChanged;
                    }

                    _partnerPointer = value;

                    if (_partnerPointer != null)
                    {
                        _partnerPointer.PropertyChanged += PartnerPointer_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(PartnerPointer));
                }
            }
        }

        private VariableData _partnerHP;

        public VariableData PartnerHP
        {
            get { return _partnerHP; }
            set
            {
                if (_partnerHP != value)
                {
                    if (_partnerHP != null)
                    {
                        _partnerHP.PropertyChanged -= PartnerHP_PropertyChanged;
                    }

                    _partnerHP = value;

                    if (_partnerHP != null)
                    {
                        _partnerHP.PropertyChanged += PartnerHP_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(PartnerHP));
                }
            }
        }

        public int PartnerHPValue { get; set; }
        private void PartnerHP_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                PartnerHPValue = (int)(_partnerHP.Value & 0xFFFF);
                OnPropertyChanged(nameof(PartnerHPValue));
            }
        }

        public int PartnerMaxHPValue { get; set; }

        private VariableData _partnerMaxHP;
        public VariableData PartnerMaxHP
        {
            get { return _partnerMaxHP; }
            set
            {
                if (_partnerMaxHP != null)
                {
                    _partnerMaxHP.PropertyChanged -= PartnerMaxHP_PropertyChanged;
                }

                _partnerMaxHP = value;

                if (_partnerMaxHP != null)
                {
                    _partnerMaxHP.PropertyChanged += PartnerMaxHP_PropertyChanged;
                }

                OnPropertyChanged(nameof(PartnerMaxHP));
            }
        }

        private void PartnerMaxHP_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                PartnerMaxHPValue = (int)(_partnerMaxHP.Value & 0xFFFF);
                OnPropertyChanged(nameof(PartnerMaxHPValue));
            }
        }

        private void PartnerPointer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value) && PartnerPointer != null)
            {
                int position_hp = SELECTED_GAME == GameConstants.BIOHAZARD_2 ? 0x156 : 0xCC;
                int position_id = SELECTED_GAME == GameConstants.BIOHAZARD_2 ? 0x8 : 0x4a;

                if (PartnerPointer.Value == 0x98E544 || PartnerPointer.Value == 0x0A62290 || PartnerPointer.Value == 0xAA2964 || PartnerPointer.Value == 0xAFFDB0)
                {
                    // PartnerHP, PartnerMaxHP, PartnerPose are non-nullable, so assign dummy values
                    PartnerHP = new VariableData(0, 1);
                    PartnerMaxHP = new VariableData(0, 1);
                    PartnerPose = new VariableData(0, 1);
                    SherryPose = "No Sherry in this room";
                    SherryPicture = "resources/re2/no-sherry.png";
                    PartnerVisibility = Visibility.Hidden;
                    OnPropertyChanged(nameof(SherryPose));
                    OnPropertyChanged(nameof(SherryPicture));
                }
                else
                {
                    PartnerPose = new VariableData(PartnerPointer.Value + 0x6, 1);
                    PartnerHP = new VariableData(PartnerPointer.Value + position_hp, 4);
                    PartnerMaxHP = new VariableData(PartnerPointer.Value + position_hp + 2, 4);
                    PartnerVisibility = Visibility.Visible;
                }

                OnPropertyChanged(nameof(PartnerVisibility));
                OnPropertyChanged(nameof(PartnerPointer));
            }
        }

        private VariableData? _itemboxState;
        public VariableData? ItemBoxState
        {
            get { return _itemboxState; }
            set
            {
                if (_itemboxState != value)
                {
                    if (_itemboxState != null)
                    {
                        _itemboxState.PropertyChanged -= ItemBoxState_PropertyChanged;
                    }

                    _itemboxState = value;

                    if (_itemboxState != null)
                    {
                        _itemboxState.PropertyChanged += ItemBoxState_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(ItemBoxState));
                }
            }
        }

        private void ItemBoxState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (ItemBoxState != null && ItemBoxState.Value == 0x01 && NoItemBox && GameState != null && Monitoring != null)
                {
                    Monitoring.WriteVariableData(GameState, 0);
                }
            }
        }

        private VariableData? _hitFlag;
        public VariableData? HitFlag
        {
            get { return _hitFlag; }
            set
            {
                if (value != _hitFlag)
                {
                    if (_hitFlag != null)
                    {
                        _hitFlag.PropertyChanged -= HitFlag_PropertyChanged;
                    }

                    _hitFlag = value;

                    if (_hitFlag != null)
                    {
                        _hitFlag.PropertyChanged += HitFlag_PropertyChanged;
                    }
                }
            }
        }
        private void HitFlag_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (HitFlag != null && HitFlag.Value != 0)
                {
                    Hits += 1;
                    OnPropertyChanged(nameof(Hits));
                }
            }
        }
    }


}