using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public class Inventory(Bio bio)
    {
        public VariableData Capacity { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["Capacity"]), 1);
        public List<Slot> Slots { get; set; } = Slot.GenerateSlots(Library.HexToNint(bio.Offsets["InventoryStart"]), Library.HexToNint(bio.Offsets["InventoryEnd"]));
    }
}
