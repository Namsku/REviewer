using System.ComponentModel;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public partial class RootObject : INotifyPropertyChanged
    {
        private VariableData _rebirthDebug;
        public VariableData RebirthDebug
        {
            get { return _rebirthDebug; }
            set
            {
                if (_rebirthDebug != value)
                {
                    if (_rebirthDebug != null)
                    {
                        _rebirthDebug.PropertyChanged -= RebirthDebug_PropertyChanged;
                    }

                    _rebirthDebug = value;

                    if (_rebirthDebug != null)
                    {
                        _rebirthDebug.PropertyChanged += RebirthDebug_PropertyChanged;
                    }

                    _rebirthDebug = value;
                    OnPropertyChanged(nameof(RebirthDebug));
                }
            }
        }

        private void RebirthDebug_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (RebirthDebug.Value == 1)
                {
                    Debug += 1;
                    OnPropertyChanged(nameof(Debug));
                }
            }
        }

        private VariableData _rebirthScreen;
        public VariableData RebirthScreen
        {
            get { return _rebirthScreen; }
            set
            {
                _rebirthScreen = value;
                OnPropertyChanged(nameof(_rebirthScreen));
            }
        }

        private VariableData _rebirthState;
        public VariableData RebirthState
        {
            get { return _rebirthState; }
            set
            {
                _rebirthState = value;
                OnPropertyChanged(nameof(_rebirthState));
            }
        }
    }

}
