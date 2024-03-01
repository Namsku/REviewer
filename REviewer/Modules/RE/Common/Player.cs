using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public class Player
    {

        public Player(Bio bio)
        {
            Character = new VariableData(Library.HexToNint(bio.Offsets["Character"]), bio.Player.Character);
            InventorySlotSelected = new VariableData(Library.HexToNint(bio.Offsets["InventorySlotSelected"]), bio.Player.InventorySlotSelected);
            Stage = new VariableData(Library.HexToNint(bio.Offsets["Stage"]), bio.Player.Stage);
            Room = new VariableData(Library.HexToNint(bio.Offsets["Room"]), bio.Player.Room);
            Cutscene = new VariableData(Library.HexToNint(bio.Offsets["Cutscene"]), bio.Player.Cutscene);
            LastRoom = new VariableData(Library.HexToNint(bio.Offsets["LastRoom"]), bio.Player.LastRoom);
            Unk001 = new VariableData(Library.HexToNint(bio.Offsets["Unk001"]), bio.Player.Unk001);
            Event = new VariableData(Library.HexToNint(bio.Offsets["Event"]), bio.Player.Event);
            LastItemFound = new VariableData(Library.HexToNint(bio.Offsets["LastItemFound"]), bio.Player.LastItemFound);
            InventoryCapacityUsed = new VariableData(Library.HexToNint(bio.Offsets["InventoryCapacityUsed"]), bio.Player.InventoryCapacityUsed);
            CharacterHealthState = new VariableData(Library.HexToNint(bio.Offsets["CharacterHealthState"]), bio.Player.CharacterHealthState);
            Health = new VariableData(Library.HexToNint(bio.Offsets["Health"]), bio.Player.Health);
            LockPick = new VariableData(Library.HexToNint(bio.Offsets["LockPick"]), bio.Player.LockPick);
        }

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
