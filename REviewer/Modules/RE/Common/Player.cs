using REviewer.Modules.Utils;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
namespace REviewer.Modules.RE.Common
{
    public partial class RootObject : INotifyPropertyChanged
    {
        private VariableData? _character;
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
                OnPropertyChanged(nameof(CharacterName));
                OnPropertyChanged(nameof(MaxHealth));
            }
        }

        public string? CharacterName
        {
            get {
                if (_character?.Database == null) return "ERROR";

                var length = ((Dictionary<byte, string>)_character.Database).Count - 1;
                return ((Dictionary<byte, string>)_character.Database)[(byte)(_character.Value & length)].ToString();
            }
        }

        public string? MaxHealth
        {
            get {
                if (_character?.Database == null) return "ERR";
                if (_health?.Database == null) return "0";

                var length = ((Dictionary<byte, string>)_character.Database).Count - 1;
                return ((Dictionary<byte, List<int>>)_health.Database)[(byte)(_character.Value & length)][0].ToString();
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
                // Wait 50 ms to avoid flickering
                await Task.Delay(50);
                OnPropertyChanged(nameof(InventorySlotSelectedImage));
            }
        }

        public string InventorySlotSelectedImage
        {
            get
            {
                if (_inventorySlotSelected != null && IDatabase != null)
                {
                    var selected = InventorySlotSelected.Value - 1;
                    byte id = InventorySlotSelected?.Value == 0 ? (byte)0 : (byte)Inventory[selected].Item.Value;
                    return IDatabase.Items[id].Img;
                }
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

                    if(Room != null)
                    {
                        Room.PropertyChanged += Room_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(Room));
                }
            }
        }

        private void Room_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (Stage == null || Room == null) return;

                var sss = (((int)Stage.Value % 5) + 1).ToString() ?? "1";
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
            if(FullRoomName == null || KeyRooms == null) return;

            string fullRoomName = FullRoomName;
            string lastRoomName = LastRoomName ?? fullRoomName;

            foreach (var roomName in new[] { lastRoomName, fullRoomName })
            {
                if (KeyRooms.TryGetValue(roomName, out List<string>? value))
                {
                    // Console.WriteLine(roomName);
                    List<string> keyRooms = value;
                    string otherRoomName = roomName == lastRoomName ? fullRoomName : lastRoomName;

                    if (!keyRooms.Contains(otherRoomName) && otherRoomName != roomName)
                    {

                        keyRooms.Add(otherRoomName);

                        if (keyRooms.Count == 2)
                        {
                            UpdateChronometers();
                            KeyRooms.Remove(roomName);
                        }
                    }

                    // Update the list in the dictionary
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
                    byte value = (byte) (LastItemFound?.Value ?? 255);
                    var state = GameState?.Value & 0xF0000000;

                    
                    if (LastItemFound?.Value == 0x31)
                    {
                        UpdateRaceKeyItem(0x31, "Internal Room", 2, true);
                    }

                    if (IDatabase.Items[value].Type == "Key Item") // && FullRoomName == null)
                    {
                        var sss = (((int) Stage.Value % 5) + 1).ToString();
                        var rrr = ((int) Room.Value).ToString("X2");
                        FullRoomName = sss + rrr;

                        if ((state != 0x8000000 || state != 0x9000000) && value != 61)
                        {
                            UpdateRaceKeyItem(value, FullRoomName, 1);
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

                if ((_characterHealthState?.Value & 0x40) == 0 && (_characterHealthState?.Value & 0x20) == 0 && (_characterHealthState?.Value & 0x04) == 0 && (_characterHealthState?.Value & 0x02) == 0)
                {
                    UpdateHealthColor();
                }
                else
                {
                    Health.Background = CustomColors.Lavender;
                }

                OnPropertyChanged(nameof(Health));
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
            var health_table = ((Dictionary<byte, List<int>>)Health.Database)[(byte)(Character.Value & size)];

            Brush[] colors = [CustomColors.Blue, CustomColors.Default, CustomColors.Yellow, CustomColors.Orange, CustomColors.Red, CustomColors.White];

            if ((status & 0x20) != 0 || (status & 0x40) != 0 || (status & 0x04) != 0 || (status & 0x02) != 0)
            {
                return;
            }

            if (_health?.Value == health_table[0])
            {
                _health.Background = CustomColors.Default;
                return;
            }

            for (int i = 0; i < health_table.Count - 1; i++)
            {
                if (health_table[i] > _health?.Value && _health.Value >= health_table[i + 1])
                {
                    _health.Background = colors[i + 1];
                }
            }
        }

        private VariableData? _lockPick;
        public VariableData? LockPick
        {
            get { return _lockPick; }
            set
            {
                if (_lockPick != value)
                {
                    if(_lockPick != null)
                    {
                        _lockPick.PropertyChanged -= LockPick_PropertyChanged;
                    }
                    
                    _lockPick = value;
                    
                    if(_lockPick != null)
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
                if (LockPick?.Value == 0x08)
                {
                    UpdateRaceKeyItem(0x31, "Internal Room", 2, true);
                }

                OnPropertyChanged(nameof(LockPick));
            }
        }
    }
}