using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{

    public class Slot
    {
        public VariableData? Item { get; set; }
        public VariableData? Quantity { get; set; } 

        public static List<Slot> GenerateSlots(nint startOffset, nint endOffset)
        {
            List<Slot> slots = [];

            for (nint i = startOffset; i < endOffset; i += 2)
            {
                slots.Add(new Slot
                {
                    Item = new VariableData(i, 1),
                    Quantity = new VariableData(i + 1, 1)
                });
            }

            return slots;
        }
    }
}
