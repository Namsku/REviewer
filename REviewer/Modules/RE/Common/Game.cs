using System.ComponentModel;
using System.Printing.IndexedProperties;
using System.Reflection;
using System.Runtime.InteropServices;
using REviewer.Modules.Utils;
using REviewer.Core.Constants;

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
        public bool isDDraw100 = false;
        public double FinalInGameTime = 0;



        private double _saveState;
        public int FontSize = 24;

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

        private System.Timers.Timer? _srtTimer;
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
                if (_state != value)
                {
                    if (_state != null)
                    {
                        _state.PropertyChanged -= GameState_PropertyChanged;
                    }

                    _state = value;

                    if (value != null && _state != null)
                    {
                        _state.PropertyChanged += GameState_PropertyChanged!;
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

                    if (value != null && _system != null)
                    {
                        _system.PropertyChanged += System_PropertyChanged!;
                    }

                    OnPropertyChanged(nameof(GameSystem));
                }
            }
        }

        private void System_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                // Console.WriteLine($"System changed -> {GameSystem?.Value:X}");
                bool itembox = (GameSystem?.Value & 0x00000F00) == 0x200;

                if (itembox && NoItemBox)
                {
                    if (GameSystem != null && Monitoring != null)
                        Monitoring.WriteVariableData(GameSystem, 0);
                    if (GameState != null && Monitoring != null)
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

                if (SELECTED_GAME == GameConstants.BIOHAZARD_1)
                {
                    isDead = (state & 0x0F000000) == 0x1000000 && PreviousState != 0x1000000;
                    PreviousState = state & 0x0F000000;
                    var item_box = (GameState?.Value & 0x0000FF00) == 0x9000;

                    if (item_box && NoItemBox)
                    {
                        if (GameState != null && Monitoring != null)
                            Monitoring.WriteVariableData(GameState, (int)((long)(GameState.Value & 0xF0FFFFFF) + 0x01000000));
                    }
                }
                else if (SELECTED_GAME == GameConstants.BIOHAZARD_2)
                {
                    isDead = Health.Value > 1000 && state != 0x00000000 && OldHealth > 65000;
                }
                else if (SELECTED_GAME == GameConstants.BIOHAZARD_3)
                {
                    long vvv = GameState.Value & 0xFF000000;
                    isDead = Health.Value > 200 && (vvv == 0xA8000000 || vvv == 0x88000000);
                    isRetry = GameState.Value == 0 && LastRoom != null && LastRoom.Value == 0xFF && LastCutscene != null && LastCutscene.Value == 0xFF;
                    isGameDone = ((GameState.Value & 0x00000F00) == 0x200 && Stage != null && Room != null && ((Stage.Value == 6 && Room.Value == 0) || (Stage.Value == 6 && Room.Value == 3)));
                    isNewGame = (isGameDone == true && GameSave != null && GameSave.Value == 0);
                }
                else if (SELECTED_GAME == GameConstants.BIOHAZARD_CVX)
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

                if (isGameDone == true && FinalInGameTime == 0 && GameTimer != null)
                {
                    FinalInGameTime = GameTimer.Value / 60.0;
                    OnPropertyChanged(nameof(IGTHumanFormat));
                }

                if (isNewGame)
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
                    _timer.PropertyChanged += Timer_PropertyChanged!;
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
                    _retry.PropertyChanged += Retry_PropertyChanged!;
                }

                OnPropertyChanged(nameof(GameRetry));
            }
        }

        private void Retry_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (GameRetry != null)
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
                    _frame.PropertyChanged += Frame_PropertyChanged!;
                }

                OnPropertyChanged(nameof(GameFrame));
                OnPropertyChanged(nameof(IGTHumanFormat));
            }
        }


        private void Frame_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (SegmentCount >= 0 && SegmentCount < 4 && IGTSegments != null && _timer != null && _frame != null && IGTSHumanFormat != null)
                {
                    var baseTime = IGTSegments[Math.Max(0, SegmentCount - 1)];
                    IGTSHumanFormat[SegmentCount] = TimeSpan.FromSeconds((double)(_timer.Value) + (_frame.Value / 60.0) - baseTime).ToString(@"hh\:mm\:ss\.ff");
                    OnPropertyChanged(nameof(IGTSHumanFormat));
                }

                OnPropertyChanged(nameof(IGTHumanFormat));
            }
        }


        private void Timer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                OnPropertyChanged(nameof(IGTHumanFormat));

                double frames = SELECTED_GAME == GameConstants.BIOHAZARD_CVX ? 60.0 : 30.0;

                if (IGTSegments == null || GameTimer == null || IGTSHumanFormat == null) return;

                if (SegmentCount >= 0 && SegmentCount < 4)
                {
                    var baseTime = IGTSegments[Math.Max(0, SegmentCount - 1)];
                    IGTSHumanFormat[SegmentCount] = TimeSpan.FromSeconds((GameTimer.Value - baseTime) / frames).ToString(@"hh\:mm\:ss\.ff");
                    OnPropertyChanged(nameof(IGTSHumanFormat));
                }
            }
        }

        public string IGTHumanFormat
        {
            get
            {
                if (SELECTED_GAME == GameConstants.BIOHAZARD_1)
                {
                    return _timer != null ? TimeSpan.FromSeconds(_timer.Value / 30.0).ToString(@"hh\:mm\:ss\.ff") : "0";
                }
                else if (SELECTED_GAME == GameConstants.BIOHAZARD_2)
                {
                    if (isGameDone)
                    {
                        return TimeSpan.FromSeconds(FinalInGameTime).ToString(@"hh\:mm\:ss\.ff");
                    }
                    return (_timer != null && _frame != null) ? TimeSpan.FromSeconds((double)(_timer.Value) + (_frame.Value / 60.0)).ToString(@"hh\:mm\:ss\.ff") : "0";
                }
                else if (SELECTED_GAME == GameConstants.BIOHAZARD_3)
                {
                    if (_gameSave != null && _gameSave.Offset == 0xAFF884)
                    {
                        return IGTimerBioHazard3China();
                    }

                    return IGTimerBioHazard3Rebirth();
                }
                else if (SELECTED_GAME == GameConstants.BIOHAZARD_CVX)
                {
                    return GameTimer != null ? TimeSpan.FromSeconds(GameTimer.Value / 60.0).ToString(@"hh\:mm\:ss\.ff") : "0";
                }

                return 0.ToString();
            }
        }

        public string IGTimerBioHazard3Rebirth()
        {
            if (isDDraw100 == true)
            {
                if (GameState != null && (GameState.Value & 0x4000 & 0x4000) == 0x4000)
                {
                    return _gameSave != null ? TimeSpan.FromSeconds((_gameSave.Value) / 60.0).ToString(@"hh\:mm\:ss\.ff") : "0";
                }
                else
                {
                    if (GameFrame != null && GameSave != null && GameTimer != null)
                        return TimeSpan.FromSeconds((GameFrame.Value + GameSave.Value - GameTimer.Value) / 60.0).ToString(@"hh\:mm\:ss\.ff");
                    else
                        return "0";
                }
            }

            return IGTRebirthDDraw101();
        }

        public string IGTRebirthDDraw101()
        {

            if (GameState != null && ((GameState.Value & 0x4000) == 0x4000 || GameState.Value == 0 || GameState.Value == 0x100))
            {

                if (timerRunning && _srtTimer != null)
                {
                    timerRunning = false;
                    _srtTimer.Stop();
                    _saveState = 0;
                }

                if (Cutscene != null && Room != null && Stage != null && Cutscene.Value == 2 && Room.Value == 0xE && (Stage.Value % 5) == 4 && _gameSave != null)
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
                if (_gameSave != null)
                    _saveState = (_gameSave.Value) / 60.0 * 1000;

                return TimeSpan.FromMilliseconds(SrtTimeHotfix + _saveState).ToString(@"hh\:mm\:ss\.ff");
            }
        }

        public string IGTimerBioHazard3China()
        {
            if (SELECTED_GAME == GameConstants.BIOHAZARD_3 && _gameSave != null && _gameSave.Offset == 0xAFF884)
            {
                if (GameFrame != null && GameFrame.Value == 0 && GameFramePointer != null && GameFramePointer.Offset == 0x53705C)
                {
                    GameFramePointer = new VariableData(0x53706C, 4);
                }
                else if (GameFrame != null && GameFrame.Value == 0 && GameFramePointer != null && GameFramePointer.Offset == 0x53706C)
                {
                    GameFramePointer = new VariableData(0x53705C, 4);
                }
            }

            if (GameState != null && (GameState.Value & 0x4000) == 0x4000)
            {
                return GameTimer != null ? TimeSpan.FromSeconds(GameTimer.Value / 60.0).ToString(@"hh\:mm\:ss\.ff") : "0";
            }
            else
            {
                if (GameFrame != null && GameSave != null && GameTimer != null)
                    return TimeSpan.FromSeconds((GameFrame.Value - GameSave.Value + GameTimer.Value) / 60.0).ToString(@"hh\:mm\:ss\.ff");
                else
                    return "0";
            }
        }

        public void StartSRTTimer()
        {
            timerRunning = true;
            SrtTimeHotfix = 0;

            _srtTimer = new System.Timers.Timer(20);
            _srtTimer.Elapsed += TimerElapsed!;
            _srtTimer.Start();
        }

        private void TimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
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
                        _gameSave.PropertyChanged += GameSave_PropertyChanged!;
                    }

                    OnPropertyChanged(nameof(GameSave));
                }
            }
        }

        private void GameSave_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (SELECTED_GAME == GameConstants.BIOHAZARD_CVX)
                {
                    if (GameSave != null)
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
                        _mainMenu.PropertyChanged += MainMenu_PropertyChanged!;
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

                if (SELECTED_GAME == GameConstants.BIOHAZARD_1)
                {
                    if (_mainMenu != null && _mainMenu.Value == 1 && Health != null && Health.Value <= int.Parse(MaxHealth))
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
                if (_saveContent != null)
                {
                    _saveContent.PropertyChanged -= SaveContent_PropertyChanged;
                }

                _saveContent = value;

                if (_saveContent != null)
                {
                    _saveContent.PropertyChanged += SaveContent_PropertyChanged!;
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
                if (SaveContent != null)
                {
                    uint number = (uint)SaveContent.Value;

                    if ((number & 0x0000FFFF) == 0xADDE)
                    {
                        uint modified = ReverseBytes(number);
                        LoadState((int)modified & 0x0000FFFF);
                    }
                }

                OnPropertyChanged(nameof(SaveContent));
            }
        }

        private VariableData? _gameFramePointer;
        public VariableData? GameFramePointer
        {
            get { return _gameFramePointer; }
            set
            {
                if (_gameFramePointer != null)
                {
                    _gameFramePointer.PropertyChanged -= GameFramePointer_PropertyChanged;
                }

                _gameFramePointer = value;

                if (_gameFramePointer != null)
                {
                    _gameFramePointer.PropertyChanged += GameFramePointer_PropertyChanged!;
                }
                OnPropertyChanged(nameof(GameFramePointer));
            }
        }

        private void GameFramePointer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (GameFramePointer != null && GameFramePointer.Value != 0)
                {
                    GameFrame = new VariableData(GameFramePointer.Value + 0x5ac, 4);
                }
            }
        }
    }

}
