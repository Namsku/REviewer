using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public class Game(Bio bio)
    {
        public VariableData State { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["GameState"]), bio.Game.State);
        public VariableData Unk001 { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["GameUnk001"]), bio.Game.Unk001);
        public VariableData Timer { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["GameTimer"]), bio.Game.Timer);
        public VariableData MainMenu { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["MainMenu"]), bio.Game.MainMenu);
        public VariableData SaveContent { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["SaveContent"]), bio.Game.SaveContent);
    }

}
