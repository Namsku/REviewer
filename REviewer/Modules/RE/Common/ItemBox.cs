using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public class ItemBox(Bio bio)
    {
        public List<Slot> Slots { get; set; } = Slot.GenerateSlots(Library.HexToNint(bio.Offsets["ItemBoxStart"]), Library.HexToNint(bio.Offsets["ItemBoxEnd"]));
    }

}
