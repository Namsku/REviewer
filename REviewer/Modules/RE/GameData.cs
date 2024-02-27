using System;
using System.Configuration;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using REviewer.Modules.Utils;
using static REviewer.Modules.RE.ItemIDs;

namespace REviewer.Modules.RE
{

    public class GameData
    {
        public int InventoryStartOffset { get; set; }
        public int InventoryEndOffset { get; set; }
        public int ItemBoxStartOffset { get; set; }
        public int ItemBoxEndOffset { get; set; }
        public int CapacityOffset { get; set; }
        public int CharacterOffset { get; set; }
        public int InventorySlotSelectedOffset { get; set; }
        public int StageOffset { get; set; }
        public int RoomOffset { get; set; }
        public int CutsceneOffset { get; set; }
        public int LastRoomOffset { get; set; }
        public int Unk001Offset { get; set; }
        public int Unk002Offset { get; set; }
        public int EventOffset { get; set; }
        public int LastItemFoundOffset { get; set; }
        public int InventoryCapacityUsedOffset { get; set; }
        public int CharacterHealthStateOffset { get; set; }
        public int GameStateOffset { get; set; }
        public int TimerOffset { get; set; }
        public int PositionXOffset { get; set; }
        public int PositionYOffset { get; set; }
        public int PositionZOffset { get; set; }
        public int RebirthDebugOffset { get; set; }
        public int RebirthStateOffset { get; set; }
        public int CharacterHealth { get; set; }
        public int MainMenuOffset { get; set; }
        public int RebirthOffset { get; set; }
        public int LockPickOffset { get; set; }
        public int SaveContentOffset { get; set; }
        private JObject? _data { get; set; }
        private string? _gameName { get; set; }

        public GameData LoadFromJson(string gameName)
        {
            var reDataPath = ConfigurationManager.AppSettings["REdata"];
            var json = reDataPath != null ? File.ReadAllText(reDataPath) : throw new ArgumentNullException(nameof(reDataPath));
            var data = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(json);
            var offsets = data[gameName]["Offsets"].ToObject<GameData>();
            
            offsets._gameName = gameName;
            offsets._data = data;

            return offsets;
        }

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
            public VariableData? Health { get; set; }
            public VariableData? LockPick { get; set; }
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
            public VariableData? SaveContent { get; set; }
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

        private List<Slot> GenerateSlots(int startOffset, int endOffset)
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

        public Inventory GenerateInventory()
        {
            return new Inventory
            {
                Capacity = new VariableData { Offset = (IntPtr)CapacityOffset, Size = 4, Database = null },
                Slots = GenerateSlots(InventoryStartOffset, InventoryEndOffset)
            };
        }

        public ItemBox GenerateItemBox()
        {
            return new ItemBox { Slots = GenerateSlots(ItemBoxStartOffset, ItemBoxEndOffset) };
        }
        public Player CreatePlayer()
        {
            _data[_gameName]["Player"].ToObject<Player>();
        }

        public RootObject GenerateGameData()
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
                            { 3, "'Becca" },
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
                    LockPick = new VariableData { Offset = (IntPtr)LockPickOffset, Size = 1, Database = null },
                    CharacterHealthState = new VariableData
                    {
                        Offset = (IntPtr)CharacterHealthStateOffset,
                        Size = 1,
                        Database = null,
                    },
                },
                Game = new Game
                {
                    State = new VariableData { Offset = (IntPtr)GameStateOffset, Size = 4, Database = null },
                    Unk001 = new VariableData { Offset = (IntPtr)Unk002Offset, Size = 4, Database = null },
                    Timer = new VariableData { Offset = (IntPtr)TimerOffset, Size = 4, Database = null, Value = 0 },
                    MainMenu = new VariableData { Offset = (IntPtr)MainMenuOffset, Size = 1, Database = null },
                    SaveContent = new VariableData { Offset = (IntPtr)SaveContentOffset, Size = 4, Database = null },
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