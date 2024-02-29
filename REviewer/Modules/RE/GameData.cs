using System.Configuration;
using Newtonsoft.Json;
using REviewer.Modules.RE.Common;
using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;


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
        private Dictionary<string, Bio>? _data { get; set; }
        private string? _gameName { get; set; }

        public GameData(string gameName)
        {
            var reDataPath = ConfigurationManager.AppSettings["REdata"];
            var json = reDataPath != null ? File.ReadAllText(reDataPath) : throw new ArgumentNullException(nameof(reDataPath));
            _data = JsonConvert.DeserializeObject<Dictionary<string, Bio>>(json);
            _gameName = gameName;
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

        public RootObject GenerateGameData()
        {
            return new RootObject
            {
                Player = new Player(_data[_gameName].Player),
                Game = new Game(_data[_gameName].Game),
                Position = new Position(_data[_gameName].Position),
                Inventory = GenerateInventory(),
                ItemBox = GenerateItemBox(),
                Rebirth = new Rebirth(_data[_gameName].Rebirth)
            };
        }
    }
}