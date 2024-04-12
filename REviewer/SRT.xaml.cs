using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

using REviewer.Modules.RE;
using REviewer.Modules.RE.Common;
using REviewer.Modules.SRT;
using REviewer.Modules.Utils;
using GlobalHotKey;
using System.Windows.Controls;

namespace REviewer
{
    public partial class SRT : Window
    {
        private readonly string _gameName;
        private readonly RootObject _game;
        private readonly ItemIDs _itemDatabase;
        private MonitorVariables _monitoring;
        private FontFamily? _pixelBoyFont;
        private HotKeyManager _hotKeyManager;
        private HotKey _f11;
        public SRT(RootObject gameData, MonitorVariables monitoring, Dictionary<string,bool?> config, string gameName)
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

            InitializeFont();
            InitHotKey();

            DataContext = this._game;
            Logger.Instance.Info("Data context is set");
        }

        private void InitializeFont()
        {
            _pixelBoyFont = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/Fonts/#Pixeboy");

            Character.FontFamily = _pixelBoyFont;
            Health.FontFamily = _pixelBoyFont;

            CurrentHealth.FontFamily = _pixelBoyFont;
            HealthDelimited.FontFamily = _pixelBoyFont;
            MaxHealth.FontFamily = _pixelBoyFont;

            Death.FontFamily = _pixelBoyFont;
            // Debug.FontFamily = _pixelBoyFont;
            Hit.FontFamily = _pixelBoyFont;
            Resets.FontFamily = _pixelBoyFont;
            Saves.FontFamily = _pixelBoyFont;
            
            IGTimer.FontFamily = _pixelBoyFont;
            //RealTimer.FontFamily = _pixelBoyFont;

            Logger.Instance.Info($"Font initialized -> {Character.FontFamily}");
        }

        private void InitHotKey()
        {
            _hotKeyManager = new HotKeyManager();
            _hotKeyManager.KeyPressed += OnF11Pressed;
            _f11 = _hotKeyManager.Register(Key.F11, ModifierKeys.None);
        }

        private void OnF11Pressed(object sender, GlobalHotKey.KeyPressedEventArgs e)
        {
            Console.WriteLine("F11 Pressed");
            // Do something when F9 is pressed
            ResetSRT();
        }

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
            // Console.WriteLine("Erasing the mess");
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Console.WriteLine("Closing SRT");
            _hotKeyManager.KeyPressed -= OnF11Pressed;
            _hotKeyManager.Unregister(_f11);
        }

        private void Coords_Hex_Click(object sender, RoutedEventArgs e)
        {
            string coords;

            if (_game.SELECTED_GAME != 200)
            {
                coords = $"X: {_game.PositionX.Value:X} Y: {_game.PositionY.Value:X} Z: {_game.PositionZ.Value:X}";
            } else
            {
                coords = $"X: {_game.PositionX.Value:X} Y: {_game.PositionY.Value:X} Z: {_game.PositionZ.Value:X} R:{_game.PositionR.Value:X}";
            }
            
            Clipboard.SetText(coords);
        }

        private void Coords_Dec_Click(object sender, RoutedEventArgs e)
        {
            string coords = $"{_game.PositionX.Value}, {_game.PositionY.Value}, {_game.PositionZ.Value}, {_game.PositionR.Value & 0xFFFF}";
            Clipboard.SetText(coords);
        }
    }
}
