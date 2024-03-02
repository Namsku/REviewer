using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public class Rebirth(Bio bio)
    {
        public VariableData? Debug { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["RebirthDebug"]), bio.Rebirth.Debug);
        public VariableData? Screen { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["RebirthScreen"]), bio.Rebirth.Screen);
        public VariableData? State { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["RebirthState"]), bio.Rebirth.State);
    }

}
