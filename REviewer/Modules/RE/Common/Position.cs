using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public class Position(Bio bio)
    {
        public VariableData? X { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["PositionX"]), bio.Position.X);
        public VariableData? Y { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["PositionY"]), bio.Position.Y);
        public VariableData? Z { get; set; } = new VariableData(Library.HexToNint(bio.Offsets["PositionZ"]), bio.Position.Z);
    }

}
