using REviewer.Modules.RE.Common;
using REviewer.Core.Constants;
using REviewer.Modules.RE.Enemies;
using REviewer.Modules.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace REviewer
{
    public partial class Tracker : Window, INotifyPropertyChanged
    {


        private readonly RootObject _game;
        private double _windowWidth;

        // Docking support
        private bool _isDocked = false;
        private SRT? _dockedSRT;

        public bool IsDocked => _isDocked;
        public SRT? DockedSRT => _dockedSRT;

        public double WindowWidth
        {
            get { return _windowWidth; }
            set
            {
                if (_windowWidth != value)
                {
                    _windowWidth = value;
                    OnPropertyChanged(nameof(WindowWidth));
                }
            }
        }

        private double _windowHeight;
        public double WindowHeight
        {
            get { return _windowHeight; }
            set
            {
                if (_windowHeight != value)
                {
                    _windowHeight = value;
                    OnPropertyChanged(nameof(WindowHeight));
                }
            }
        }
        public double? WindowScale { get; set; }
        public ObservableCollection<EnemyTracking> Tracking { get; set; }

        public Tracker(ObservableCollection<EnemyTracking> trk, RootObject obj)
        {
            _game = obj ?? throw new ArgumentNullException(nameof(obj));
            Tracking = trk ?? throw new ArgumentNullException(nameof(trk));

            InitializeComponent();
            SubscribeToEvents();
            SetWindowProperties();            
            LoadSavedPosition();

            DataContext = this;
            Closing += Window_Closing;
        }

        private void LoadSavedPosition()
        {
            var (x, y) = Library.LoadWindowPosition("TRK");
            if (x >= 0 && y >= 0)
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = x;
                this.Top = y;
            }
        }

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            Library.SaveWindowPosition("TRK", this.Left, this.Top);
        }

        private void SetWindowProperties()
        {
            WindowWidth = _game.WindowWidth == 400 ? 400 : 320;
            WindowScale = _game.WindowWidth == 400 ? 1 : 0.7;

            if (_game.StaticEnemyTrackerWindow == true)
            {
                WindowHeight = 720;
            } 
            else
            {
                WindowHeight = double.NaN;
            }

            OnPropertyChanged(nameof(WindowWidth));
            OnPropertyChanged(nameof(WindowScale));
        }

        #region Window Controls

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DockButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isDocked && _dockedSRT != null)
            {
                // Undock
                _dockedSRT.UndockTracker();
                UndockFromSRT();
            }
            else
            {
                var srt = Application.Current.Windows.OfType<SRT>().FirstOrDefault();
                if (srt != null)
                {
                    srt.DockWithTracker(this);
                    DockWithSRT(srt);
                }
            }
        }

        private void PositionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isDocked && _dockedSRT != null)
            {
                _dockedSRT.ToggleTrackerPosition();
            }
        }

        public void DockWithSRT(SRT srt)
        {
            _dockedSRT = srt;
            _isDocked = true;
            UpdateDockButtonVisual();
        }

        public void UndockFromSRT()
        {
            _isDocked = false;
            _dockedSRT = null;
            UpdateDockButtonVisual();
        }

        private void UpdateDockButtonVisual()
        {
            if (DockBtn != null)
            {
                DockBtn.ToolTip = _isDocked ? "Undock from SRT" : "Dock with SRT";
            }
        }

        #endregion

        #region Enemy Tracking

        private void SubscribeToEvents()
        {
            if (_game.GameState == null || _game.LastRoom == null) return;
            if (_game.SELECTED_GAME == GameConstants.BIOHAZARD_1)
            {
                _game.GameState.PropertyChanged += GameState_PropertyChanged;
            }
            else if (_game.SELECTED_GAME == GameConstants.BIOHAZARD_2 || _game.SELECTED_GAME == GameConstants.BIOHAZARD_3)
            {
                _game.LastRoom.PropertyChanged += LastRoom_PropertyChanged;
            }
        }

        private void GameState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_game.GameState == null) return;

            var state = _game.GameState.Value & 0xFF000000;
            bool loading = state == 0x46000000 || state == 0x56000000 || state == 0x66000000;

            if (e.PropertyName == nameof(VariableData.Value) && loading)
            {
                foreach (var enemy in Tracking)
                {
                    if (enemy.Enemy == null) continue;
                    enemy.Enemy.Visibility = Visibility.Collapsed;
                    enemy.Enemy.MaxHealth = 0;
                }
            }
        }

        private void LastRoom_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_game.LastRoom == null) return;

            if (e.PropertyName == nameof(VariableData.Value) && _game.LastRoom.Value == 255)
            {
                foreach (var enemy in Tracking)
                {
                    if (enemy.Enemy == null) continue;
                    enemy.Enemy.Visibility = Visibility.Collapsed;
                    enemy.Enemy.MaxHealth = 0;
                }
            }
        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
