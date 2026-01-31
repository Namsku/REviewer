using GlobalHotKey;
using REviewer.Modules.RE;
using REviewer.Modules.RE.Common;
using REviewer.Modules.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using REviewer.Services.Game;
using REviewer.Services.Timer;
using REviewer.Services.Inventory;
using REviewer.Core.Memory;

namespace REviewer
{
    public partial class SRT : Window
    {
        private readonly string _gameName;
        private readonly RootObject _game;
        private readonly ItemIDs _itemDatabase;
        private IMemoryMonitor _monitoring;
        private HotKeyManager? _hotKeyManager;
        private HotKey? _f11;
        private HotKey? _f9;

        // Docking support
        private bool _isDocked = false;
        private Tracker? _dockedTracker;
        private bool _isTrackerAbove = false;

        public bool IsDocked => _isDocked;
        public Tracker? DockedTracker => _dockedTracker;
        public bool IsTrackerAbove => _isTrackerAbove;

        public SRT(RootObject gameData, IMemoryMonitor monitoring, Dictionary<string, bool?> config, string gameName, ViewModels.SRTViewModel viewModel)
        {
            InitializeComponent();

            _game = gameData;
            _gameName = gameName;
            _monitoring = monitoring;
            _itemDatabase = new ItemIDs(gameName);

            var keyItems = _itemDatabase.GetKeyItems()?.Select(item => new KeyItem((Property)item, -1, "NEW ROOM TO SAVE HERE")).ToList() ?? new List<KeyItem>();
            _game.InitKeyItemsModel(keyItems);
            _game.SetMonitoring(monitoring);
            _game.InitUIConfig(config);

            InitHotKey();
            LoadSavedPosition();

            DataContext = viewModel;
            Logger.Instance.Info("Data context is set to SRTViewModel");

            // Subscribe to location changes for docking
            this.LocationChanged += SRT_LocationChanged;

            this.Loaded += (s, e) =>
            {
                _pulseStoryboard = (Storyboard)this.FindResource("ECGSweepStoryboard");
                if (_pulseStoryboard != null)
                {
                    _pulseStoryboard.Begin(this, true);
                    UpdatePulseSpeed();
                }
            };

            _game.PropertyChanged += OnGamePropertyChanged;
        }

        private void LoadSavedPosition()
        {
            var (x, y) = Library.LoadWindowPosition("SRT");
            if (x >= 0 && y >= 0)
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = x;
                this.Top = y;
            }
        }

        private void InitHotKey()
        {
            _hotKeyManager = new HotKeyManager();
            _hotKeyManager.KeyPressed += OnKeyPressed;
            _f11 = _hotKeyManager.Register(Key.F11, ModifierKeys.None);

            if (_gameName == "Bio3 CHN/TWN")
            {
                _f9 = _hotKeyManager.Register(Key.F9, ModifierKeys.None);
            }
        }

        private void OnKeyPressed(object? sender, KeyPressedEventArgs ex)
        {
            switch (ex.HotKey.Key)
            {
                case Key.F11:
                    ResetSRT();
                    break;
                case Key.F9:
                    ResetGame();
                    break;
            }
        }

        #region Pulse Animation

        private Storyboard? _pulseStoryboard;

        private void OnGamePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RootObject.PulseSpeed))
            {
                UpdatePulseSpeed();
            }
        }

        private void UpdatePulseSpeed()
        {
            if (_pulseStoryboard == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                double speed = _game.PulseSpeed;
                if (speed <= 0)
                {
                    _pulseStoryboard.Pause(this);
                }
                else
                {
                    if (_pulseStoryboard.GetIsPaused(this)) _pulseStoryboard.Resume(this);

                    // Set SpeedRatio (Inverse of Duration Factor)
                    // Speed 1.0 = Default (1s cycle)
                    // Speed 0.35 = Fast (Wait, 1.0/0.35 = 2.8x speed)
                    _pulseStoryboard.SetSpeedRatio(this, 1.0 / speed);
                }
            });
        }

        #endregion

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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPopup.IsOpen = !SettingsPopup.IsOpen;
        }

        private void DockButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isDocked && _dockedTracker != null)
            {
                var tracker = _dockedTracker;  // Store reference before clearing
                UndockTracker();
                tracker.UndockFromSRT();  // Notify the tracker
            }
            else
            {
                var tracker = Application.Current.Windows.OfType<Tracker>().FirstOrDefault();
                if (tracker != null)
                {
                    DockWithTracker(tracker, _isTrackerAbove);
                    tracker.DockWithSRT(this);
                }
            }
        }

        public void DockWithTracker(Tracker tracker, bool above = false)
        {
            _dockedTracker = tracker;
            _isDocked = true;
            _isTrackerAbove = above;
            UpdateDockButtonVisual();
            PositionDockedTracker();
        }

        public void UndockTracker()
        {
            _isDocked = false;
            _dockedTracker = null;
            UpdateDockButtonVisual();
        }

        public void ToggleTrackerPosition()
        {
            _isTrackerAbove = !_isTrackerAbove;
            PositionDockedTracker();
        }

        private void SRT_LocationChanged(object? sender, EventArgs e)
        {
            if (_isDocked && _dockedTracker != null)
            {
                PositionDockedTracker();
            }
        }

        private void PositionDockedTracker()
        {
            if (_dockedTracker == null || !_dockedTracker.IsLoaded) return;

            if (_isTrackerAbove)
            {
                _dockedTracker.Left = this.Left;
                _dockedTracker.Top = this.Top - _dockedTracker.ActualHeight;
            }
            else
            {
                _dockedTracker.Left = this.Left;
                _dockedTracker.Top = this.Top + this.ActualHeight;
            }
        }

        private void UpdateDockButtonVisual()
        {
            // Update tooltip based on dock state
            if (DockBtn != null)
            {
                DockBtn.ToolTip = _isDocked ? "Undock Enemy Tracker" : "Dock with Enemy Tracker";
            }
        }

        #endregion

        #region SRT Controls

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ResetSRT();
        }

        private void ResetSRT()
        {
            var keyItems = _itemDatabase.GetKeyItems()?.Select(item => new KeyItem((Property)item, -1, "NEW ROOM TO SAVE HERE")).ToList() ?? new List<KeyItem>();
            _game.InitKeyItemsModel(keyItems);
            _game.InitKeyRooms();
            _game.InitStats();
            _game.InitTimers();
            _game.LastItemFound.Value = 0;
            _game.InventorySlotSelected.Value = 0;
        }

        private void ResetGame()
        {
            _monitoring.WriteVariableData(_game.GameState, 0);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _game.PropertyChanged -= OnGamePropertyChanged;  // Add this

            Library.SaveWindowPosition("SRT", this.Left, this.Top);

            _hotKeyManager.KeyPressed -= OnKeyPressed;

            if (_f9 != null) _hotKeyManager.Unregister(_f9);
            if (_f11 != null) _hotKeyManager.Unregister(_f11);  // Added null check

            _hotKeyManager?.Dispose();  // Add disposal

            if (_isDocked && _dockedTracker != null)
            {
                _dockedTracker.UndockFromSRT();  // Notify tracker properly
                UndockTracker();
            }
        }

        private void Coords_Hex_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string coords = _game.SELECTED_GAME != 200
                    ? $"X: {_game.PositionX.Value:X} Y: {_game.PositionY.Value:X} Z: {_game.PositionZ.Value:X}"
                    : $"X: {_game.PositionX.Value:X} Y: {_game.PositionY.Value:X} Z: {_game.PositionZ.Value:X} R:{_game.PositionR.Value:X}";

                Clipboard.SetText(coords);
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                // Clipboard is in use by another process - silently fail or notify user
            }
        }

        private void Coords_Dec_Click(object sender, RoutedEventArgs e)
        {
            string coords = $"{_game.PositionX.Value}, {_game.PositionY.Value}, {_game.PositionZ.Value}, {_game.PositionR.Value & 0xFFFF}";
            Clipboard.SetText(coords);
        }

        private void ChromaColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_game == null) return; // Guard during initialization

            if (sender is ComboBox combo && combo.SelectedItem is ComboBoxItem item && item.Tag is string color)
            {
                _game.ChromaKeyColor = color;
                Library.UpdateConfigFile("ChromaKeyColor", color);
            }
        }

        private void OverlayPositionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo && combo.SelectedItem is ComboBoxItem item && item.Tag is string positionStr)
            {
                if (int.TryParse(positionStr, out int position))
                {
                    // Find and update the Overlay window's corner position
                    var overlay = Application.Current.Windows.OfType<Overlay>().FirstOrDefault();
                    if (overlay != null)
                    {
                        overlay._cornerPosition = position;
                    }

                    Library.UpdateConfigFile("OverlayPosition", positionStr);
                }
            }
        }

        #endregion
    }
}
