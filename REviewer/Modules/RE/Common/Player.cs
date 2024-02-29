using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
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
}
