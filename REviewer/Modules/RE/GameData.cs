using System.Configuration;
using Newtonsoft.Json;
using REviewer.Modules.RE.Common;
using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;


namespace REviewer.Modules.RE
{

    public class GameData
    {
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
    }
}