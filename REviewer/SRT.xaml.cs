using System.Windows;
using System.Windows.Media;
using REviewer.Modules.RE;
using REviewer.Modules.RE.Common;
using REviewer.Modules.SRT;
using REviewer.Modules.Utils;

namespace REviewer
{
    public partial class SRT : Window
    {
        private readonly string _gameName;
        private readonly RootObject _game;
        private readonly ItemIDs _itemDatabase;
        private MonitorVariables _monitoring;
        private FontFamily? _pixelBoyFont;

        public SRT(RootObject gameData, MonitorVariables monitoring, Dictionary<string,bool?> config, string gameName)
        {
            InitializeComponent();

            _game = gameData;
            _gameName = gameName;
            _monitoring = monitoring;
            _itemDatabase = new ItemIDs(gameName);
            // _raceDatabase = new PlayerRaceProgress(gameName);
            // KeyItems = _itemDatabase.GetKeyItems()?.Select(item => new KeyItem((Property)item, -1, "NEW ROOM TO SAVE HERE")).ToList() ?? [];
            var keyItems = _itemDatabase.GetKeyItems()?.Select(item => new KeyItem((Property)item, -1, "NEW ROOM TO SAVE HERE")).ToList() ?? [];
            _game.InitKeyItemsModel(keyItems);
            _game.InitUIConfig(config);

            InitializeFont();

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
            Debug.FontFamily = _pixelBoyFont;
            Resets.FontFamily = _pixelBoyFont;
            Saves.FontFamily = _pixelBoyFont;
            
            IGTimer.FontFamily = _pixelBoyFont;
            //RealTimer.FontFamily = _pixelBoyFont;

            Logger.Instance.Info($"Font initialized -> {Character.FontFamily}");
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var keyItems = _itemDatabase.GetKeyItems()?.Select(item => new KeyItem((Property)item, -1, "NEW ROOM TO SAVE HERE")).ToList() ?? [];
            _game.InitKeyItemsModel(keyItems);
            _game.InitKeyRooms();
            _game.InitStats();
            _game.InitTimers();
            // Console.WriteLine("Erasing the mess");
        }

    }
}
