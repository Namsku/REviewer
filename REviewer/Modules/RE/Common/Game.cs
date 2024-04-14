using System.ComponentModel;
using System.Printing.IndexedProperties;
using System.Runtime.InteropServices;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public partial class RootObject : INotifyPropertyChanged
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public bool isGameDone = false;
        public bool isNewGame = false;
        public double FinalInGameTime = 0;

        public int BIOHAZARD_1 = 100;
        public int BIOHAZARD_2 = 200;
        public int BIOHAZARD_3 = 300;
        public int BIOHAZARD_CVX = 400;

        private double _saveState;

        private double _srtTimeHotfix;
        public double SrtTimeHotfix
        {
            get { return _srtTimeHotfix; }
            set
            {
                _srtTimeHotfix = value;
                OnPropertyChanged(nameof(SrtTimeHotfix));
            }
        }

        private System.Timers.Timer _srtTimer;
        public bool timerRunning = false;

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


        private VariableData? _system;
        public VariableData? GameSystem
        {
            get { return _system; }
            set
            {
                if (_system != value)
                {
                    if (_system != null)
                    {
                        _system.PropertyChanged -= System_PropertyChanged;
                    }

                    _system = value;

                    if (value != null)
                    {
                        _system.PropertyChanged += System_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(GameSystem));
                }
            }
        }

        private void System_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(VariableData.Value))
            {
                // Console.WriteLine($"System changed -> {GameSystem.Value:X}");
                bool itembox = (GameSystem?.Value & 0x00000F00) == 0x200;  

                if (itembox && NoItemBox)
                {
                    Monitoring.WriteVariableData(GameSystem, 0);
                    Monitoring.WriteVariableData(GameState, 0);
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
                bool isRetry = false;

                if (SELECTED_GAME == BIOHAZARD_1)
                {
                    isDead = (state & 0x0F000000) == 0x1000000 && PreviousState != 0x1000000;
                    PreviousState = state & 0x0F000000;
                    var item_box = (GameState?.Value & 0x0000FF00) == 0x9000;

                    if (item_box && NoItemBox)
                    {
                        Monitoring.WriteVariableData(GameState, (int)((long)(GameState.Value & 0xF0FFFFFF) + 0x01000000));
                    }
                }
                else if (SELECTED_GAME == BIOHAZARD_2)
                {
                    isDead = Health.Value > 1000 && state != 0x00000000 && OldHealth > 65000;
                }
                else if (SELECTED_GAME == BIOHAZARD_3)
                {
                    long vvv = GameState.Value & 0xFF000000;
                    isDead = Health.Value > 200 && (vvv == 0xA8000000 || vvv == 0x88000000);
                    isRetry = GameState.Value == 0 && LastRoom.Value == 0xFF && LastCutscene.Value == 0xFF;
                    isGameDone = ((GameState.Value & 0x00000F00) == 0x200 && ((Stage.Value == 6 && Room.Value == 0) || (Stage.Value == 6 && Room.Value == 3)));
                    isNewGame = (isGameDone == true && GameSave.Value == 0);
                }
                else if (SELECTED_GAME == BIOHAZARD_CVX)
                {
                    isDead = Health.Value < 0;
                }

                // Console.WriteLine($"{Library.ToHexString(state)} - {Library.ToHexString(state & 0x0F000000)} - {(state & 0x0F000000) == 0x1000000} - {Library.ToHexString(PreviousState)} - {PreviousState != 0x1000000}");

                if (isDead)
                {
                    Health.Value = 255;
                    Deaths += 1;

                    OnPropertyChanged(nameof(Deaths));
                }

                if(isGameDone = true && FinalInGameTime == 0)
                {
                    FinalInGameTime = GameTimer.Value / 60.0;
                    OnPropertyChanged(nameof(IGTHumanFormat));
                }

                if(isNewGame)
                {
                    FinalInGameTime = 0;
                    isGameDone = false;
                    OnPropertyChanged(nameof(IGTHumanFormat));
                }

                if (isRetry)
                {
                    Resets += 1;
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

        private VariableData? _retry;
        public VariableData? GameRetry
        {
            get { return _retry; }
            set
            {
                if (_retry != null)
                {
                    _retry.PropertyChanged -= Retry_PropertyChanged;
                }

                _retry = value;

                if (_retry != null)
                {
                    _retry.PropertyChanged += Retry_PropertyChanged;
                }

                OnPropertyChanged(nameof(GameRetry));
            }
        }

        private void Retry_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                Resets = GameRetry.Value;
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

                double frames = SELECTED_GAME == 400 ? 60.0 : 30.0;

                if(IGTSegments == null || GameTimer == null) return;

                if (SegmentCount >= 0 && SegmentCount < 4)
                {
                    var baseTime = IGTSegments[Math.Max(0, SegmentCount - 1)];
                    IGTSHumanFormat[SegmentCount] = TimeSpan.FromSeconds((GameTimer.Value - baseTime) / frames).ToString(@"hh\:mm\:ss\.ff");
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
                if (SELECTED_GAME == 100)
                {
                    return TimeSpan.FromSeconds(_timer.Value / 30.0).ToString(@"hh\:mm\:ss\.ff");
                }
                else if (SELECTED_GAME == 200)
                {
                    if (isGameDone)
                    {
                        return TimeSpan.FromSeconds(FinalInGameTime).ToString(@"hh\:mm\:ss\.ff");
                    }
                    return TimeSpan.FromSeconds((double)(_timer.Value) + (_frame.Value / 60.0)).ToString(@"hh\:mm\:ss\.ff");
                }
                else if (SELECTED_GAME == 300)
                {
                    if ((GameState.Value & 0x4000) == 0x4000 || GameState.Value == 0 || GameState.Value == 0x100)
                    {
                        if (timerRunning)
                        {
                            // Console.WriteLine("Timer stopped");
                            timerRunning = false;
                            _srtTimer.Stop();
                            _saveState = 0;
                        }

                        if (Cutscene.Value == 2 && Room.Value == 0xE && (Stage.Value % 5) == 4)
                        {
                            return TimeSpan.FromMilliseconds(((_gameSave.Value) / 60.0 * 1000)).ToString(@"hh\:mm\:ss\.ff");
                        }
                        return TimeSpan.FromMilliseconds(SrtTimeHotfix + _saveState).ToString(@"hh\:mm\:ss\.ff");
                    }
                    else
                    {
                        if (!timerRunning)
                        {
                            // Console.WriteLine("Timer started");
                            StartSRTTimer();
                        }
                        _saveState = (_gameSave.Value) / 60.0 * 1000;

                        return TimeSpan.FromMilliseconds(SrtTimeHotfix + _saveState).ToString(@"hh\:mm\:ss\.ff");
                    }
                }
                else if (SELECTED_GAME == 400)
                {
                    return TimeSpan.FromSeconds(GameTimer.Value / 60.0).ToString(@"hh\:mm\:ss\.ff");
                }

                return 0.ToString();
            }
        }

        public void StartSRTTimer()
        {
            timerRunning = true;
            SrtTimeHotfix = 0;

            _srtTimer = new System.Timers.Timer(20);
            _srtTimer.Elapsed += TimerElapsed;
            _srtTimer.Start();
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SrtTimeHotfix += 20;
            OnPropertyChanged(nameof(IGTHumanFormat));
        }

        private VariableData? _gameSave;

        public VariableData? GameSave
        {
            get { return _gameSave; }
            set
            {
                if (_gameSave != value)
                {
                    if (_gameSave != null)
                    {
                        _gameSave.PropertyChanged -= GameSave_PropertyChanged;
                    }

                    _gameSave = value;

                    if (_gameSave != null)
                    {
                        _gameSave.PropertyChanged += GameSave_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(GameSave));
                }
            }
        }

        private void GameSave_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (SELECTED_GAME == 400)
                {
                    Saves = GameSave.Value;
                } 
                else 
                { 
                    OnPropertyChanged(nameof(IGTHumanFormat));
                }
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

                if (SELECTED_GAME == 100)
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
