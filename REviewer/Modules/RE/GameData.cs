using System.Configuration;
using MessagePack;

namespace REviewer.Modules.RE
{

    public class GameData
    {
        private const int InventoryStartOffset = 0xC38814;
        private const int InventoryEndOffset = 0xC38824;
        private const int ItemBoxStartOffset = 0xC387B4;
        private const int ItemBoxEndOffset = 0xC38814;
        private const int CapacityOffset = 0xC351B5;
        private const int CharacterOffset = 0xC386F9;
        private const int InventorySlotSelectedOffset = 0xC38719;
        private const int StageOffset = 0xC386F0;
        private const int RoomOffset = 0xC386F1;
        private const int CutsceneOffset = 0xC386F2;
        private const int LastRoomOffset = 0xC386F3;
        private const int Unk001Offset = 0xC386F4;
        private const int Unk002Offset = 0xC33097;
        private const int EventOffset = 0xC386F5;
        private const int LastItemFoundOffset = 0xC386F6;
        private const int InventoryCapacityUsedOffset = 0xC386F7;
        private const int CharacterHealthStateOffset = 0xC35290;
        private const int GameStateOffset = 0xC33090;
        private const int TimerOffset = 0xAA8E10;
        private const int PositionXOffset = 0xC2EAB0;
        private const int PositionYOffset = 0xC2EAB4;
        private const int PositionZOffset = 0xC2EAB8;
        private const int RebirthDebugOffset = 0x1217ACA6;
        private const int RebirthStateOffset = 0xC30020;
        private const int CharacterHealth = 0xC3523C;
        private const int MainMenuOffset = 0xAA8E57;
        private const int RebirthOffset = 0xC3002C;

        public class VariableData
        {
            // Private fields
            private int _value;

            // Public properties
            public object? Database { get; set; }
            public bool IsUpdated { get; set; } = false;
            public object LockObject { get; } = new object();
            public IntPtr Offset { get; set; }
            public uint Size { get; set; }
            public int Value
            {
                get { return _value; }
                set
                {
                    if (_value != value)
                    {
                        _value = value;
                        OnUpdated(); // Call method to raise event
                    }
                }
            }

            // Event declaration
            public event EventHandler? Updated;

            // Method to raise the Updated event
            protected virtual void OnUpdated()
            {
                Updated?.Invoke(this, EventArgs.Empty);
            }
}

        [MessagePackObject]
        public class KeyItem : ICloneable
        {
            // Constructor
            public KeyItem(Property data, int state, string room)
            {
                Data = data;
                State = state;
                Room = room;
            }
            public KeyItem()
            {
            }

            // Public properties
            [Key(0)]
            public Property Data { get; set; }
            [Key(1)]
            public string Room { get; set; }
            [Key(2)]
            public int State { get; set; }

            public object Clone()
            {
                return new KeyItem
                {
                    // Copy all properties
                    Data = (Property)this.Data.Clone(), // Assuming Property implements ICloneable
                    Room = this.Room,
                    State = this.State
                };
            }
        }


        [MessagePackObject]
        public class PlayerRaceProgress : ICloneable
        {
            public PlayerRaceProgress()
            {
            }

            // Key Elements
            [Key(0)]
            public List<KeyItem> KeyItems { get; set; }

            [Key(1)]
            public Dictionary<string, List<string>> KeyRooms { get; set; }

            // Stats
            [Key(2)]
            public int Saves { get; set; }

            [Key(3)]
            public int Deaths { get; set; }

            [Key(4)]
            public int Resets { get; set; }

            [Key(5)]
            public int Debugs { get; set; }

            [Key(6)]
            public int Segments { get; set; }

            // Room Infos
            [Key(7)]
            public int old_state { get; set; }

            [Key(8)]
            public string stage { get; set; }

            [Key(9)]
            public string room { get; set; }

            [Key(10)]
            public string LastRoomName { get; set; }

            [Key(11)]
            public string FullRoomName { get; set; }

            [Key(12)]
            public string? SavePath;

            [Key(13)]
            public TimeSpan? _fulltimer { get; set; }
            [Key(14)]
            public TimeSpan? _segtimer1 { get; set; }
            [Key(15)]
            public TimeSpan? _segtimer2 { get; set; }
            [Key(16)]
            public TimeSpan? _segtimer3 { get; set; }
            [Key(17)]
            public TimeSpan? _segtimer4 { get; set; }

            [Key(18)]
            public int _tickTimer { get; set; }
            [Key(19)]
            public byte[] UUID { get; set; }
            [Key(20)]
            public byte[] _real_itembox { get; set; }

            [IgnoreMember]
            public FileSystemWatcher Watcher = new();

            public PlayerRaceProgress(string savePath)
            {
                var common = new Common();
                SavePath = common.GetSavePath(savePath);
            }

            public object Clone()
            {
                return new PlayerRaceProgress
                {
                    // Copy all properties
                    KeyItems = this.KeyItems.Select(item => (KeyItem)item.Clone()).ToList(),
                    KeyRooms = new Dictionary<string, List<string>>(this.KeyRooms),
                    Saves = this.Saves,
                    Deaths = this.Deaths,
                    Resets = this.Resets,
                    Debugs = this.Debugs,
                    Segments = this.Segments,
                    old_state = this.old_state,
                    stage = this.stage,
                    room = this.room,
                    LastRoomName = this.LastRoomName,
                    FullRoomName = this.FullRoomName,
                    SavePath = this.SavePath,
                    _fulltimer = this._fulltimer,
                    _segtimer1 = this._segtimer1,
                    _segtimer2 = this._segtimer2,
                    _segtimer3 = this._segtimer3,
                    _segtimer4 = this._segtimer4,
                    _tickTimer = this._tickTimer,
                    UUID = (byte[])this.UUID?.Clone(),
                    _real_itembox = (byte[])this._real_itembox?.Clone(),
                    // FileSystemWatcher is not cloneable, so we create a new one
                    Watcher = new FileSystemWatcher()
                };
            }
        }

        public class RootObject
        {
            public string? Name { get; set; }
            public string? Process { get; set; }
            public Player? Player { get; set; }
            public Position? Position { get; set; }
            public Game? Game { get; set; }
            public Inventory? Inventory { get; set; }
            public ItemBox? ItemBox { get; set; }
            public Rebirth? Rebirth { get; set; }
        }

        public class Player
        {
            public VariableData? Character { get; set; }
            public VariableData? InventorySlotSelected { get; set; }
            public VariableData? Stage { get; set; }
            public VariableData? Room { get; set; }
            public VariableData? Cutscene { get; set; }
            public VariableData? LastRoom { get; set; }
            public VariableData? Unk001 { get; set; }
            public VariableData? Event { get; set; }
            public VariableData? LastItemFound { get; set; }
            public VariableData? InventoryCapacityUsed { get; set; }
            public VariableData? CharacterHealthState { get; set; }
            public VariableData? Health { get; internal set; }
        }

        public class Position
        {
            public VariableData? X { get; set; }
            public VariableData? Y { get; set; }
            public VariableData? Z { get; set; }
        }

        public class Game
        {
            public VariableData? State { get; set; }
            public VariableData? Unk001 { get; set; }
            public VariableData? Timer { get; set; }
            public VariableData? MainMenu { get; set; }
        }

        public class Slot
        {
            public VariableData? Item { get; set; }
            public VariableData? Quantity { get; set; }
        }

        public class Inventory
        {
            public VariableData? Capacity { get; set; }
            public List<Slot>? Slots { get; set; }
        }

        public class ItemBox
        {
            public List<Slot>? Slots { get; set; }
        }

        public class Rebirth
        {
            public VariableData? Debug { get; set; }
            public VariableData? Screen { get; set; }
            public VariableData? State { get; set; }
        }

        private static List<Slot> GenerateSlots(int startOffset, int endOffset)
        {
            List<Slot> slots = [];

            for (int i = startOffset; i < endOffset; i += 2)
            {
                slots.Add(new Slot
                {
                    Item = new VariableData { Offset = (IntPtr)i, Size = 1, Database = new Dictionary<byte, string>() },
                    Quantity = new VariableData { Offset = (IntPtr)(i + 1), Size = 1, Database = null }
                });
            }

            return slots;
        }

        public static Inventory GenerateInventory()
        {
            return new Inventory
            {
                Capacity = new VariableData { Offset = (IntPtr)CapacityOffset, Size = 4, Database = null },
                Slots = GenerateSlots(InventoryStartOffset, InventoryEndOffset)
            };
        }

        public static ItemBox GenerateItemBox()
        {
            return new ItemBox { Slots = GenerateSlots(ItemBoxStartOffset, ItemBoxEndOffset) };
        }

        public static RootObject GenerateGameData()
        {
            return new RootObject
            {
                Player = new Player
                {
                    Character = new VariableData
                    {
                        Offset = (IntPtr)CharacterOffset,
                        Size = 1,
                        Database = new Dictionary<byte, string>
                        {
                            { 0, "Chris"},
                            { 1, "Jill"},
                            { 2, "Barry" },
                            { 3, "Rebecca" },
                        },
                    },
                    InventorySlotSelected = new VariableData { Offset = (IntPtr)InventorySlotSelectedOffset, Size = 1, Database = null },
                    Stage = new VariableData { Offset = (IntPtr)StageOffset, Size = 1, Database = null },
                    Room = new VariableData { Offset = (IntPtr)RoomOffset, Size = 1, Database = null },
                    Cutscene = new VariableData { Offset = (IntPtr)CutsceneOffset, Size = 1, Database = null },
                    LastRoom = new VariableData { Offset = (IntPtr)LastRoomOffset, Size = 1, Database = null },
                    Unk001 = new VariableData { Offset = (IntPtr)Unk001Offset, Size = 1, Database = null },
                    Event = new VariableData
                    {
                        Offset = (IntPtr)EventOffset,
                        Size = 1,
                        Database = new Dictionary<byte, string>
                        {
                            { 0, "Nothing"},
                            { 1, "Refused Item"},
                            { 128, "Item/Text Show" },
                            { 129, "No Selected" },
                        },
                    },
                    LastItemFound = new VariableData { Offset = (IntPtr)LastItemFoundOffset, Size = 1, Database = new Dictionary<byte, string>() },
                    InventoryCapacityUsed = new VariableData { Offset = (IntPtr)InventoryCapacityUsedOffset, Size = 1, Database = null },
                    Health = new VariableData { Offset = (IntPtr)CharacterHealth, Size = 1, Database = null },
                    CharacterHealthState = new VariableData
                    {
                        Offset = (IntPtr)CharacterHealthStateOffset,
                        Size = 1,
                        Database = new Dictionary<byte, string>
                        {
                            // BINARY 00000000 representation of the values below
                            // 0x10 = 00010000 16
                            // 0x12 = 00010010 18
                            // 0x1E = 00011110 30
                            // 0x50 = 01010000 80
                            // 0x52 = 01010010 82
                            // 0x59 = 01011110 94

                            { 16, "Normal"},
                            { 18, "Poison"},
                            { 26, "???" },
                            { 30, "Yawn Poison"},
                            { 50, "Poison + Yawn Poison" },
                            { 80, "Gaz"},
                            { 82, "Gaz + Poison"},
                            { 94, "Gaz + Yawn Poison"},
                            { 114, "G+P+YP (WTF)"},
                        },
                    },
                },
                Game = new Game
                {
                    State = new VariableData { Offset = (IntPtr)GameStateOffset, Size = 4, Database = null },
                    Unk001 = new VariableData { Offset = (IntPtr)Unk002Offset, Size = 4, Database = null },
                    Timer = new VariableData { Offset = (IntPtr)TimerOffset, Size = 4, Database = null, Value = 0 },
                    MainMenu = new VariableData { Offset = (IntPtr)MainMenuOffset, Size = 1, Database = null },
                },
                Position = new Position
                {
                    X = new VariableData { Offset = (IntPtr)PositionXOffset, Size = 4, Database = null },
                    Y = new VariableData { Offset = (IntPtr)PositionYOffset, Size = 4, Database = null },
                    Z = new VariableData { Offset = (IntPtr)PositionZOffset, Size = 4, Database = null },
                },
                Inventory = GenerateInventory(),
                ItemBox = GenerateItemBox(),
                Rebirth = new Rebirth
                {
                    Debug = new VariableData { Offset = (IntPtr)RebirthDebugOffset, Size = 4, Database = null },
                    Screen = new VariableData { Offset = (IntPtr)RebirthOffset, Size = 4, Database = null },
                    State = new VariableData { Offset = (IntPtr)RebirthStateOffset, Size = 4, Database = null },
                },

            };
        }
    }
}