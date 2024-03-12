using System.ComponentModel;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public partial class RootObject : INotifyPropertyChanged
    {
        private VariableData _positionX;
        public VariableData PositionX
        {
            get { return _positionX; }
            set
            {
                _positionX = value;
                OnPropertyChanged(nameof(PositionX));
            }
        }

        private VariableData _positionY;
        public VariableData PositionY
        {
            get { return _positionY; }
            set
            {
                _positionY = value;
                OnPropertyChanged(nameof(PositionY));
            }
        }
        private VariableData _positionZ;
        public VariableData PositionZ
        {
            get { return _positionZ; }
            set
            {
                _positionZ = value;
                OnPropertyChanged(nameof(PositionZ));
            }
        }
    }

}
