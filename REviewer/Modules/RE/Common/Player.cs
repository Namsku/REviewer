using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public class Player(Bio bio)
    {
        public VariableData? Character { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["Character"]), bio.Player.Character);
        public VariableData? InventorySlotSelected { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["InventorySlotSelected"]), bio.Player.InventorySlotSelected);
        public VariableData? Stage { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["Stage"]), bio.Player.Stage);
        public VariableData? Room { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["Room"]), bio.Player.Room);
        public VariableData? Cutscene { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["Cutscene"]), bio.Player.Cutscene);
        public VariableData? LastRoom { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["LastRoom"]), bio.Player.LastRoom);
        public VariableData? Unk001 { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["GameUnk001"]), bio.Player.Unk001);
        public VariableData? Event { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["Event"]), bio.Player.Event);
        public VariableData? LastItemFound { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["LastItemFound"]), bio.Player.LastItemFound);
        public VariableData? InventoryCapacityUsed { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["InventoryCapacityUsed"]), bio.Player.InventoryCapacityUsed);
        public VariableData? CharacterHealthState { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["CharacterHealthState"]), bio.Player.CharacterHealthState);
        public VariableData? Health { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["CharacterHealth"]), bio.Player.Health);
        public VariableData? LockPick { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["LockPick"]), bio.Player.LockPick);
    }
}
