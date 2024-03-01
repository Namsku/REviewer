using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public class Position
    {
        public Position(Bio bio)
        {
            X = new VariableData(Library.HexToNint(bio.Offsets["X"]), bio.Position.X);
            Y = new VariableData(Library.HexToNint(bio.Offsets["Y"]), bio.Position.Y);
            Z = new VariableData(Library.HexToNint(bio.Offsets["Z"]), bio.Position.Z);
        }

        public VariableData? X { get; set; }
        public VariableData? Y { get; set; }
        public VariableData? Z { get; set; }
    }

}
