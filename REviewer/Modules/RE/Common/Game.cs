using System.ComponentModel;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public partial class RootObject : INotifyPropertyChanged
    {
        private int _previousState;

        public int PreviousState
        {
            get { return _previousState; }
            set
            {
                    _previousState = value;
            }
        }

        private VariableData _state;
        public VariableData GameState
        {
            get { return _state; }
            set
            {
                if (_state != value) { 
                    _state = value;

                    if (_state != null)
                    {
                        _state.PropertyChanged -= GameState_PropertyChanged;
                    }

                    _state = value;

                    if(value != null)
                    {
                        _state.PropertyChanged += GameState_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(GameState));
                }
            }
        }

        private void GameState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                int state = GameState.Value;

                // Console.WriteLine($"{Library.ToHexString(state)} - {Library.ToHexString(state & 0x0F000000)} - {(state & 0x0F000000) == 0x1000000} - {Library.ToHexString(PreviousState)} - {PreviousState != 0x1000000}");

                if ((state & 0x0F000000) == 0x1000000 && PreviousState != 0x1000000)
                {
                    Health.Value = 255;
                    Deaths += 1;

                    OnPropertyChanged(nameof(Deaths));
                    OnPropertyChanged(nameof(Health));
                }

                PreviousState = state & 0x0F000000;
            }
        }


        private VariableData _unk001;
        public VariableData Unk001
        {
            get { return _unk001; }
            set
            {
                _unk001 = value;
                OnPropertyChanged(nameof(Unk001));
            }
        }

        private VariableData _timer;
        public VariableData GameTimer
        {
            get { return _timer; }
            set
            {
                if (_timer != null)
                {
                    _timer.PropertyChanged -= Timer_PropertyChanged;
                }

                _timer = value;

                if (_timer != null)
                {
                    _timer.PropertyChanged += Timer_PropertyChanged;
                }

                OnPropertyChanged(nameof(GameTimer));
            }
        }

        private void Timer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                OnPropertyChanged(nameof(IGTHumanFormat));

                if (SegmentCount >= 0 && SegmentCount < 4)
                {
                    var baseTime = IGTSegments[Math.Max(0, SegmentCount - 1)];
                    IGTSHumanFormat[SegmentCount] = TimeSpan.FromSeconds((GameTimer.Value - baseTime) / 30.0).ToString(@"hh\:mm\:ss\.ff");
                    OnPropertyChanged(nameof(IGTSHumanFormat));
                }

                int currentTimerValue = GameTimer.Value;

                if (PreviousTimerValue == currentTimerValue)
                {
                    return;
                }

            }
        }

        public string IGTHumanFormat
        {
            get
            {
                return TimeSpan.FromSeconds(_timer.Value/30.0).ToString(@"hh\:mm\:ss\.ff");
            }
        }


        private VariableData _mainMenu;
        public VariableData MainMenu
        {
            get { return _mainMenu; }
            set
            {
                if (_mainMenu != value)
                {
                    if (_mainMenu != null)
                    {
                        _mainMenu.PropertyChanged -= MainMenu_PropertyChanged;
                    }

                    _mainMenu = value;

                    if (_mainMenu != null)
                    {
                        _mainMenu.PropertyChanged += MainMenu_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(MainMenu));
                }
            }
        }

        private void MainMenu_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (_mainMenu.Value == 1 && Health.Value <= int.Parse(MaxHealth))
                {
                    Resets += 1;
                }

                // buttonReset.Enabled = _game.Game.MainMenu.Value == 1;
                // buttonReset.Visible = _game.Game.MainMenu.Value == 1;
                // Logger.Instance.Info($"Main Menu -> {_game.Game.MainMenu.Value}");

                OnPropertyChanged(nameof(Resets));
            }
        }

        private VariableData _saveContent;
        public VariableData SaveContent
        {
            get { return _saveContent; }
            set
            {
                if(_saveContent != null)
                {
                    _saveContent.PropertyChanged -= SaveContent_PropertyChanged;
                }

                _saveContent = value;

                if (_saveContent != null)
                {
                    _saveContent.PropertyChanged += SaveContent_PropertyChanged;
                }
                OnPropertyChanged(nameof(SaveContent));
            }
        }

        private static uint ReverseBytes(uint number)
        {
            return ((number & 0x000000FF) << 24) |
                   ((number & 0x0000FF00) << 8) |
                   ((number & 0x00FF0000) >> 8) |
                   ((number & 0xFF000000) >> 24);
        }

        private void SaveContent_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                uint number = (uint)SaveContent.Value;

                if ((number & 0x0000FFFF) == 0xADDE)
                {
                    uint modified = ReverseBytes(number);
                    LoadState((int)modified & 0x0000FFFF);
                }

                OnPropertyChanged(nameof(SaveContent));
            }
        }

    }

}
