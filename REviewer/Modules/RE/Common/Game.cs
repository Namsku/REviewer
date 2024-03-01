using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public class Game
    {
        public Game(Bio bio)
        {
            Timer = new VariableData(Library.HexToNint(bio.Offsets["Timer"]), bio.Game.Timer);
            MainMenu = new VariableData(Library.HexToNint(bio.Offsets["MainMenu"]), bio.Game.MainMenu);
            SaveContent = new VariableData(Library.HexToNint(bio.Offsets["SaveContent"]), bio.Game.SaveContent);
        }
        
        public VariableData? State { get; set; }
        public VariableData? Unk001 { get; set; }
        public VariableData? Timer { get; set; }
        public VariableData? MainMenu { get; set; }
        public VariableData? SaveContent { get; set; }
    }

}
