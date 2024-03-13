using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using REviewer.Modules.RE;
using REviewer.Modules.RE.Common;
using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;

namespace REviewer
{
    public class REviewerMenuItem
    {
        public ImageSource? Image { get; set; }
        public int MenuIndex { get; set; }

    }

    public class UINotify : INotifyPropertyChanged
    {
        private string? _version;
        public string? Version
        {
            get { return _version; }
            set
            {
                if (Version != value)
                {
                    _version = value;
                    OnPropertyChanged(nameof(Version));
                }
            }
        }

        private Visibility _visibility;
        public Visibility ChrisInventory
        {
            get { return _visibility; }
            set
            {
                if (_visibility != value)
                {
                    _visibility = value;
                    OnPropertyChanged(nameof(ChrisInventory));
                }
            }
        }

        public UINotify(string version)
        {
            Version = version;
            ChrisInventory = Visibility.Collapsed;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public partial class MainWindow : Window
    {
        private Process? _process;
        private string? _processName;

        private bool _isProcessRunning = false;
        private bool _isDdrawLoaded = false;
        private bool _isSaveFound = false;

        private Timer? _processWatcher;
        private Timer? _rootObjectWatcher;

        private RootObject? _residentEvilGame;
        private MonitorVariables? _MVariables;
        private MonitorVariables? _MVEnemies;
        private ObservableCollection<EnnemyTracking>? _tracking;
        private ItemIDs? _itemIDs;
        private UINotify _ui;

        private static readonly List<string> _gameList = ["Bio",];
        public static string Version => ConfigurationManager.AppSettings["Version"] ?? "None";

        public MainWindow()
        {
            InitializeComponent();
            InitializeText();
            InitCheckBoxes();
            InitializeSaveWatcher();
            InitializeProcessWatcher();
            InitializeRootObjectWatcher();

            // Loading Data Context
            DataContext = this._ui;
        }

        private void InitCheckBoxes()
        {
            _ui = new UINotify(ConfigurationManager.AppSettings["Version"] ?? "None");

            if (ComboBoxGameSelection.SelectedIndex == 0)
            {
                _ui.ChrisInventory = Visibility.Visible;
            }
            else
            {
                _ui.ChrisInventory = Visibility.Collapsed;
            }
        }

        private void UpdateUI(string content, string savePath)
        {
            // Updating the TextBlock on the MainWindow
            UpdateUIElement(MD5, content);
            UpdateUIElement(ProcessTextBlock, content);
            UpdateUIElement(Rebirth, content);

            _isSaveFound = "Not Found" == savePath ? false : true;
            var saveContent = "Not Found" == savePath ? "Not Found" : "Found";
            var saveColor = "Not Found" == savePath ? CustomColors.Red : CustomColors.Green;
            Library.UpdateTextBlock(Save, text: saveContent, color: saveColor, isBold: true);

            // Updating the TextBox from the Settings panel
            Library.UpdateTextBox(RE1SavePath, text: savePath, isBold: false);
        }

        private static void UpdateUIElement(TextBlock element, string content)
        {
            Library.UpdateTextBlock(element, text: content, color: CustomColors.Red, isBold: true);
        }

        private void InitializeText()
        {
            const string content = "Not Found";
            var configPath = ConfigurationManager.AppSettings["Config"];

            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                throw new ArgumentNullException(nameof(configPath));
            }

            var json = File.ReadAllText(configPath);
            var reJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? throw new ArgumentNullException("The game data is null");

            reJson.TryGetValue("RE1", out var savePath);
            savePath ??= content;

            UpdateUI(content, savePath);
        }

        private void InitializeProcessWatcher()
        {
            // Set the game list
            _process = null;
            _processName = _gameList[0];
            _processWatcher = new Timer(ProcessWatcherCallback, null, 0, 1000);
        }

        private void InitializeRootObjectWatcher()
        {
            _rootObjectWatcher = new Timer(RootObjectWatcherCallback, null, 0, 500);
        }

        private void InitializeSaveWatcher()
        {
            _processName = _gameList[0];
            var selectedGame = Library.GetGameName(_processName ?? "UNKNOWN GAME ERROR");
            var configPath = ConfigurationManager.AppSettings["Config"];

            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                throw new ArgumentNullException(nameof(configPath));
            }

            var json = File.ReadAllText(configPath);
            var reJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? throw new ArgumentNullException("The game data is null");

            reJson.TryGetValue(selectedGame, out var savePath);
            savePath ??= "Not Found";

            var saveContent = "Not Found" == savePath ? "Not Found" : "Found";
            var saveColor = "Not Found" == savePath ? CustomColors.Red : CustomColors.Green;
            Library.UpdateTextBlock(Save, text: saveContent, color: saveColor, isBold: true);
        }

        // selecting different window frames

        private void MenuListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Hide all grids
            panelMain.Visibility = Visibility.Hidden;
            panelSettings.Visibility = Visibility.Hidden;
            panelAbout.Visibility = Visibility.Hidden;

            // Get the selected item
            var selectedItem = (REviewerMenuItem)menuListView.SelectedItem;

            // Show the corresponding grid
            switch (selectedItem.MenuIndex)
            {
                case 1:
                    panelMain.Visibility = Visibility.Visible;
                    break;
                case 2:
                    panelSettings.Visibility = Visibility.Visible;
                    break;
                case 3:
                    panelAbout.Visibility = Visibility.Visible;
                    break;
            }
        }

        // Process Watcher

        
        private void ProcessWatcherCallback(object? state)
        {
            // Check if the process is running
            if (!_isProcessRunning && Process.GetProcessesByName(_processName).Length > 0)
            {
                // The process is running
                _isProcessRunning = true;

                // Get the process
                _process = Process.GetProcessesByName(_processName)[0];
                string md5Hash = Library.GetProcessMD5Hash(_process);

                // Check if the process has the Gemini DLL
                _isDdrawLoaded = Library.IsDdrawLoaded(_process);
                string geminiStatus = _isDdrawLoaded ? "Found" : "Not Found";
                var colorGemini = _isDdrawLoaded ? CustomColors.Green : CustomColors.Red;

                // Updating the TextBlock on the MainWindow
                Library.UpdateTextBlock(MD5, text: md5Hash, color: CustomColors.Black, isBold: false);
                Library.UpdateTextBlock(ProcessTextBlock, text: "Found", color: CustomColors.Green, isBold: true);
                Library.UpdateTextBlock(Rebirth, text: geminiStatus, color: colorGemini, isBold: true);
                
                // Set the Exited event handler
                _process.EnableRaisingEvents = true;
                _process.Exited += Process_Exited;

                // Log the event
                Logger.Instance.Info($"Process {_processName} has been found");
            }
        }


        private void RootObjectWatcherCallback(object? state)
        {
            // Check if the process is running
            if (_isProcessRunning && _isDdrawLoaded && _isSaveFound)
            {
                MappingGameVariables();
                _rootObjectWatcher?.Dispose();
            }
        }

        private void MappingGameVariables()
        {
            if (_process == null)
            {
                throw new ArgumentNullException(nameof(_process));
            }

            if (_residentEvilGame == null)
            {
                // Mapping the game data (Room IDs, Item IDs, etc.)
                GameData gameData = new GameData(_process.ProcessName);
                _itemIDs = new ItemIDs(_process.ProcessName);
                _residentEvilGame = gameData.GetGameData(_itemIDs);
            }

            if(_tracking == null)
            {
                InitEnemies(_processName ?? "UNKNOWN GAME");
            }

            if (_MVariables == null)
            {
                _MVariables = new MonitorVariables(_process.Handle, _process.ProcessName);
            }
            else
            {
                _MVariables.UpdateProcessHandle(_process.Handle);
            }

            if(_MVEnemies == null)
            {
                _MVEnemies = new MonitorVariables(_process.Handle, _process.ProcessName);
            }
            else
            {
                _MVEnemies.UpdateProcessHandle(_process.Handle);
            }

            _MVariables.Start(_residentEvilGame);
            _MVEnemies.Start(_tracking ?? new());
        }

        private void Process_Exited(object? sender, EventArgs e)
        {
            // The process has exited
            _isProcessRunning = false;
            _rootObjectWatcher = new Timer(RootObjectWatcherCallback, null, 0, 500);

            var content = "Not Found";

            // Stoping the monitoring 
            _MVariables?.Stop();

            // Updating the TextBlock on the MainWindow
            Library.UpdateTextBlock(MD5, text: content, color: CustomColors.Red, isBold: true);
            Library.UpdateTextBlock(ProcessTextBlock, text: content, color: CustomColors.Red, isBold: true);
            Library.UpdateTextBlock(Rebirth, text: content, color: CustomColors.Red, isBold: true);

            // Log the event
            Logger.Instance.Info($"Process {_processName} has exited");
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _processWatcher?.Dispose();
            _rootObjectWatcher?.Dispose();
        }

        // making some checking if the selected game

        private void GameSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected item
            int selectedIndex = ((ComboBox)sender).SelectedIndex;
            _processName = _gameList[selectedIndex];

            // Kill the current process monitoring
            _processWatcher?.Dispose();
            _rootObjectWatcher?.Dispose();

            // Check every second if the process is running
            _processWatcher = new Timer(ProcessWatcherCallback, null, 0, 1000);
            _rootObjectWatcher = new Timer(RootObjectWatcherCallback, null, 0, 100);

            Logger.Instance.Info($"Selected game: {_processName} -> Disabling old process watcher to the new one");
        }

        // Events for the options panel

        /*
        private void IGTimer_Checked(object sender, RoutedEventArgs e)
        {
            if (RealTimerCheckBox.IsChecked == true)
            {
                RealTimerCheckBox.IsChecked = false;
            }
        }

        private void RealTimer_Checked(object sender, RoutedEventArgs e)
        {
            if (IGTimerCheckBox.IsChecked == true)
            {
                IGTimerCheckBox.IsChecked = false;
            }
        }

        */

        // Events for selecting the Save Path
        private void RE1SavePathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            var result = dialog.ShowDialog();

            Logger.Instance.Debug(result);

            if (result == true)
            {
                RE1SavePath.Text = dialog.FolderName;
                UpdateConfigFile("RE1", dialog.FolderName);
                Library.UpdateTextBlock(Save, text: "Found", color: CustomColors.Green, isBold: true);

                if (ComboBoxGameSelection.SelectedIndex == 0) _isSaveFound = true;
            }
        }

        private void RE2SavePathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            var result = dialog.ShowDialog();

            Logger.Instance.Debug(result);

            if (result == true)
            {
                RE2SavePath.Text = dialog.FolderName;
                UpdateConfigFile("RE2", dialog.FolderName);
                Library.UpdateTextBlock(Save, text: "Found", color: CustomColors.Green, isBold: true);
            }
        }

        private void RE2SavePathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            var result = dialog.ShowDialog();

            Logger.Instance.Debug(result);

            if (result == true)
            {
                RE2SavePath.Text = dialog.FolderName;
                UpdateConfigFile("RE2", dialog.FolderName);
                Library.UpdateTextBlock(Save, text: "Found", color: CustomColors.Green, isBold: true);
            }
        }

        private void UpdateConfigFile(string game, string path)
        {
            var configPath = ConfigurationManager.AppSettings["Config"];

            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                throw new ArgumentNullException(nameof(configPath));
            }

            var json = File.ReadAllText(configPath);
            var reJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();

            reJson[game] = path;
            File.WriteAllText(configPath, JsonConvert.SerializeObject(reJson, Formatting.Indented));
        }

        // Event for clicking the Run Button
        private void Run_Click(object sender, RoutedEventArgs e)
        {
            if (IsSRTAbleToRun())
            {
                try
                {
                    if( _residentEvilGame == null || _MVariables == null || _tracking == null || _processName == null)
                    {
                        MessageBox.Show("The game data is not initialized! Please be sure everything is detected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var srtConfig = GenerateSRTUIConfig();
                        var SRT = new SRT(_residentEvilGame, _MVariables, srtConfig, _processName ?? "UNKNOWN GAME PROCESS ERROR");
                        SRT.Show();

                        var TRK = new Tracker(_tracking, _residentEvilGame);
                        TRK.Show();
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error has occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public bool IsSRTAbleToRun()
        {
            var db = new Dictionary<string, string> {
                {"Bio", "RE1"},
                {"Bio2", "RE2"},
                {"Bio3", "RE3"},
            };

            if (string.IsNullOrEmpty(_processName))
            {
                MessageBox.Show("Game not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            var game = db[_processName];
            var configPath = ConfigurationManager.AppSettings["Config"];

            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                throw new ArgumentNullException(nameof(configPath));
            }

            var json = File.ReadAllText(configPath);
            var reJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? throw new ArgumentNullException("The game data is null");

            if (!_isProcessRunning)
            {
                MessageBox.Show("The process is not running", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrEmpty(_processName))
            {
                MessageBox.Show("Game not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                throw new ArgumentNullException(nameof(configPath));
            }

            if (!reJson.TryGetValue(game, out _))
            {
                MessageBox.Show("The game save path is not set/not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        public Dictionary<string, bool?> GenerateSRTUIConfig()
        {
            var srtConfig = new Dictionary<string, bool?>
            {
                ["HealthBar"] = HealthBar.IsChecked,
                ["Standard"] = NormalMode.IsChecked,
                ["ItemBox"] = ShowItemBox.IsChecked,
                ["ChrisInventory"] = ChrisInventory.IsChecked,

                // ["IGTimer"] = IGTimerCheckBox.IsChecked,
                // ["RealTimer"] = RealTimerCheckBox.IsChecked
            };

            // Show the config elements in the log
            foreach (var item in srtConfig)
            {
                Logger.Instance.Debug($"{item.Key}: {item.Value}");
            }

            return srtConfig;
        }
        private void InitEnemies(string processName)
        {
            var reDataPath = ConfigurationManager.AppSettings["REdata"];
            var json = reDataPath != null ? File.ReadAllText(reDataPath) : throw new ArgumentNullException(nameof(reDataPath));
            var data = JsonConvert.DeserializeObject<Dictionary<string, Bio>>(json);
            var bio = data?[processName];

            if (bio == null)
            {
                throw new ArgumentNullException(nameof(bio));
            }

            var offset = Library.HexToNint(bio.Offsets["EnnemyInfos"]);
            var property = bio.Ennemy.EnnemyInfos;

            if (_tracking == null)
            {
                _tracking = new ObservableCollection<EnnemyTracking>();
            }

            for (var i = 0; i < 16; i++)
            {
                _tracking.Add(new EnnemyTracking(offset + (i * 396), property));
            }
        }
    }
}