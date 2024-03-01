using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public class Rebirth
    {
        public Rebirth(Bio bio)
        {
            Debug = new VariableData(Library.HexToNint(bio.Offsets["Debug"]), bio.Rebirth.Debug);
            Screen = new VariableData(Library.HexToNint(bio.Offsets["Screen"]), bio.Rebirth.Screen);
            State = new VariableData(Library.HexToNint(bio.Offsets["State"]), bio.Rebirth.State);
        }

        public VariableData? Debug { get; set; }
        public VariableData? Screen { get; set; }
        public VariableData? State { get; set; }
    }

}
