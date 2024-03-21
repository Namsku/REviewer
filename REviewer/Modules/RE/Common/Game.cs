﻿using System.ComponentModel;
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

        private VariableData? _state;
        public VariableData? GameState
        {
            get { return _state; }
            set
            {
                if (_state != value) { 
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
                if (Health == null || GameState == null) return;

                int state = GameState.Value;
                bool isDead = false;

                if(SELECTED_GAME == 0)
                {
                    isDead = (state & 0x0F000000) == 0x1000000 && PreviousState != 0x1000000;
                    PreviousState = state & 0x0F000000;
                } 
                else if (SELECTED_GAME == 1)
                {
                    isDead = Health.Value > 200 && state != 0x00000000;
                }

                // Console.WriteLine($"{Library.ToHexString(state)} - {Library.ToHexString(state & 0x0F000000)} - {(state & 0x0F000000) == 0x1000000} - {Library.ToHexString(PreviousState)} - {PreviousState != 0x1000000}");

                if (isDead)
                {
                    Health.Value = 255;
                    Deaths += 1;

                    OnPropertyChanged(nameof(Deaths));
                    OnPropertyChanged(nameof(Health));
                }
            }
        }


        private VariableData? _unk001;
        public VariableData? Unk001
        {
            get { return _unk001; }
            set
            {
                _unk001 = value;
                OnPropertyChanged(nameof(Unk001));
            }
        }

        private VariableData? _timer;
        public VariableData? GameTimer
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

        private VariableData? _frame;

        public VariableData? GameFrame
        {
            get { return _frame; }
            set
            {
                if (_frame != null)
                {
                    _frame.PropertyChanged -= Frame_PropertyChanged;
                }

                _frame = value;

                if (_frame != null)
                {
                    _frame.PropertyChanged += Frame_PropertyChanged;
                }

                OnPropertyChanged(nameof(GameFrame));
                OnPropertyChanged(nameof(IGTHumanFormat));
            }
        }

        private void Timer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                OnPropertyChanged(nameof(IGTHumanFormat));

                if(IGTSegments == null || GameTimer == null) return;

                if (SegmentCount >= 0 && SegmentCount < 4)
                {
                    var baseTime = IGTSegments[Math.Max(0, SegmentCount - 1)];
                    IGTSHumanFormat[SegmentCount] = TimeSpan.FromSeconds((GameTimer.Value - baseTime) / 30.0).ToString(@"hh\:mm\:ss\.ff");
                    OnPropertyChanged(nameof(IGTSHumanFormat));
                }
            }
        }

        private void Frame_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (SegmentCount >= 0 && SegmentCount < 4)
                {
                    var baseTime = IGTSegments[Math.Max(0, SegmentCount - 1)];
                    IGTSHumanFormat[SegmentCount] = TimeSpan.FromSeconds((double)(_timer.Value) + (_frame.Value / 60.0) - baseTime).ToString(@"hh\:mm\:ss\.ff");
                    OnPropertyChanged(nameof(IGTSHumanFormat));
                }

                OnPropertyChanged(nameof(IGTHumanFormat));
            }
        }

        public string IGTHumanFormat
        {
            get
            {
                if (SELECTED_GAME == 0)
                {
                    return TimeSpan.FromSeconds(_timer.Value / 30.0).ToString(@"hh\:mm\:ss\.ff");
                }
                else if (SELECTED_GAME == 1)
                {
                    return TimeSpan.FromSeconds((double)(_timer.Value) + (_frame.Value / 60.0)).ToString(@"hh\:mm\:ss\.ff");
                }

                return 0.ToString();
            }
        }


        private VariableData? _mainMenu;
        public VariableData? MainMenu
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
                if (MaxHealth == null) return;

                if (SELECTED_GAME == 0)
                {
                    if (_mainMenu.Value == 1 && Health?.Value <= int.Parse(MaxHealth))
                    {
                        Resets += 1;
                    }
                }

                // buttonReset.Enabled = _game.Game.MainMenu.Value == 1;
                // buttonReset.Visible = _game.Game.MainMenu.Value == 1;
                // Logger.Instance.Info($"Main Menu -> {_game.Game.MainMenu.Value}");

                OnPropertyChanged(nameof(Resets));
            }
        }

        private VariableData? _saveContent;
        public VariableData? SaveContent
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
