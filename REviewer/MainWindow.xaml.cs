using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Net.Http.Headers;
using System.Net.Http;

using Newtonsoft.Json;

using REviewer.Modules.RE;
using REviewer.Modules.RE.Common;
using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;
using REviewer.Modules.RE.Enemies;

using Microsoft.WindowsAPICodePack.Dialogs;

using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sigscan.Definitions.Structs;

using Timer = System.Threading.Timer;

namespace REviewer
{
    public partial class MainWindow : Window
    {
        private Process? _process;
        private string? _processName;

        private bool _isProcessRunning = false;
        private bool _isMappingDone = false;

        private Timer? _processWatcher;
        private Timer? _rootObjectWatcher;

        private RootObject? _residentEvilGame;
        private MonitorVariables? _MVariables;
        private MonitorVariables? _MVEnemies;
        private ObservableCollection<EnemyTracking>? _tracking;
        private ItemIDs? _itemIDs;
        private UINotify? _ui;

        public const int BIOHAZARD_1_MK = 0; // Mediakit
        public const int BIOHAZARD_2_SC = 1; // SourceNext
        public const int BIOHAZARD_2_PC = 2; // RE2 - Platinium - China - Claire
        public const int BIOHAZARD_2_PL = 3; // RE2 - Platinium - China - Leon
        public const int BIOHAZARD_3_RB = 4; // Rebirth
        public const int BIOHAZARD_3_CH = 5; // Chinese/Taiwanese
        public const int BIOHAZARD_CV_X = 6; // CVX 

        public const string RPCS3 = "rpcs3";
        public const string PCSX2 = "pcsx2";
        public const string PCSX2QT = "pcsx2-qt";
        public const string PCSX264QT = "pcsx2-qtx64";
        public const string PCSX264QTAV = "pcsx2-qtx64-avx2";
        public const string PCSX264WX = "pcsx2x64";
        public const string PCSX264WXAV = "pcsx2x64-avx2";
        public const string Dolphin = "dolphin";

        private Dolphin.Memory.Access.Dolphin _dolphin;

        public IntPtr VirtualMemoryPointer { get; private set; }
        public IntPtr ProductPointer { get; private set; }
        public int ProductLength { get; private set; }
        public bool IsBigEndian { get; private set; }

        private static readonly List<string> _gameList = new List<string>() { "Bio", "bio2 1.10", "bio2 chn claire", "bio2 chn leon", "BIOHAZARD(R) 3 PC", "Bio3 CHN/TWN", "CVX PS2 US" };
        private static readonly List<string> _gameSelection = new List<string>() {"RE1", "RE2", "RE2C", "RE2C", "RE3", "RE3C", "RECVX"};

        public static string Version => ConfigurationManager.AppSettings["Version"] ?? "None";
        private static Version CurrentVersion = System.Version.Parse(Version.Split('-')[0].Replace("v", ""));

        public SRT? SRT { get; private set; }
        public Tracker? TRK { get; private set; }

        private readonly object _lock = new object();

        public MainWindow()
        {
            InitializeComponent();
            InitializeText();
            InitCheckBoxes();
            InitializeSavedOptions();
            
            InitializeSaveWatcher();
            InitializeProcessWatcher();
            InitializeRootObjectWatcher();

            CheckForNewVersion();

            // Loading Data Context
            DataContext = this._ui;

            Closed += MainWindow_Closed;
        }

        private void InitCheckBoxes()
        {
            _ui ??= new UINotify(ConfigurationManager.AppSettings["Version"] ?? "None");

            switch (ComboBoxGameSelection.SelectedIndex)
            {
                case BIOHAZARD_1_MK:
                    _ui.isBiorandMode = true;
                    _ui.ChallengeVisibility = Visibility.Visible;
                    _ui.ClassicVisibility = Visibility.Visible;
                    _ui.ChrisInventory = Visibility.Visible;
                    _ui.Sherry = Visibility.Collapsed;
                    _ui.DebugModeVisibility = Visibility.Visible;
                    _ui.DdrawBio3 = Visibility.Collapsed;
                    break;
                case BIOHAZARD_2_SC:
                    _ui.isBiorandMode = true;
                    _ui.ChallengeVisibility = Visibility.Visible;
                    _ui.ClassicVisibility = Visibility.Visible;
                    _ui.ChrisInventory = Visibility.Collapsed;
                    _ui.Sherry = Visibility.Visible;
                    _ui.DdrawBio3 = Visibility.Collapsed;
                    break;
                case BIOHAZARD_2_PC:
                    _ui.isBiorandMode = false;
                    _ui.ChallengeVisibility = Visibility.Collapsed;
                    _ui.ClassicVisibility = Visibility.Collapsed;
                    _ui.ChrisInventory = Visibility.Collapsed;
                    _ui.Sherry = Visibility.Visible;
                    _ui.DebugModeVisibility = Visibility.Visible;
                    _ui.DdrawBio3 = Visibility.Collapsed;
                    break;
                case BIOHAZARD_2_PL:
                    _ui.isBiorandMode = false;
                    _ui.ChallengeVisibility = Visibility.Collapsed;
                    _ui.ClassicVisibility = Visibility.Collapsed;
                    _ui.ChrisInventory = Visibility.Collapsed;
                    _ui.Sherry = Visibility.Visible;
                    _ui.DebugModeVisibility = Visibility.Visible;
                    _ui.DdrawBio3 = Visibility.Collapsed;
                    break;
                case BIOHAZARD_3_RB:
                    _ui.isBiorandMode = true;
                    _ui.ChallengeVisibility = Visibility.Visible;
                    _ui.ClassicVisibility = Visibility.Visible;
                    _ui.ChrisInventory = Visibility.Collapsed;
                    _ui.Sherry = Visibility.Collapsed;
                    _ui.DebugModeVisibility = Visibility.Visible;
                    _ui.DdrawBio3 = Visibility.Visible;
                    break;
                case BIOHAZARD_3_CH:
                    _ui.isBiorandMode = false;
                    _ui.ChallengeVisibility = Visibility.Collapsed;
                    _ui.ClassicVisibility = Visibility.Collapsed;
                    _ui.ChrisInventory = Visibility.Collapsed;
                    _ui.Sherry = Visibility.Collapsed;
                    _ui.DebugModeVisibility = Visibility.Collapsed;
                    _ui.DdrawBio3 = Visibility.Collapsed;
                    break;
                case BIOHAZARD_CV_X:
                    _ui.isBiorandMode = true;
                    _ui.ChallengeVisibility = Visibility.Collapsed;
                    _ui.ClassicVisibility = Visibility.Visible;
                    _ui.ChrisInventory = Visibility.Collapsed;
                    _ui.Sherry = Visibility.Collapsed;
                    _ui.DebugModeVisibility = Visibility.Collapsed;
                    _ui.DdrawBio3 = Visibility.Collapsed;
                    break;
            }
        }

        private void UpdateUI(string content, string savePath, int position)
        {
            // Updating the TextBlock on the MainWindow
            if (MD5 != null)
            {
                UpdateUIElement(MD5, content);
                UpdateUIElement(ProcessTextBlock, content);

                var saveContent = "Not Found" == savePath ? "Not Found" : "Found";
                var saveColor = "Not Found" == savePath ? CustomColors.Red : CustomColors.Green;

                // Console.WriteLine($"You have been called -> {savePath} - {saveContent} - {saveColor}");
                Library.UpdateTextBlock(Save, text: saveContent, color: saveColor, isBold: true);

                switch (position)
                {
                    case BIOHAZARD_1_MK:
                        Library.UpdateTextBox(RE1SavePath, text: savePath, isBold: false);
                        break;
                    case BIOHAZARD_2_SC:
                        Library.UpdateTextBox(RE2SavePath, text: savePath, isBold: false);
                        break;
                    case BIOHAZARD_2_PC:
                        break;
                    case BIOHAZARD_2_PL:
                        break;
                    case BIOHAZARD_3_RB:
                        Library.UpdateTextBox(RE3SavePath, text: savePath, isBold: false);
                        break;
                    case BIOHAZARD_3_CH:
                        break;  
                    case BIOHAZARD_CV_X:
                        Library.UpdateTextBox(RECVXSavePath, text: savePath, isBold: false);
                        break;
                }
            }
        }

        private static void UpdateUIElement(TextBlock element, string content)
        {
            Library.UpdateTextBlock(element, text: content, color: CustomColors.Red, isBold: true);
        }

        private void InitializeText()
        {
            const string content = "Not Found";
            var reJson = Library.GetReviewerConfig();

            for (int i = 0; i < _gameSelection.Count; i++)
            {
                if (reJson.TryGetValue(_gameSelection[i], out var saveREPath))
                {
                    UpdateUI(content, saveREPath, i);
                }
            }
        }

        private void InitializeSavedOptions()
        {
            var config = Library.GetOptions();
            _ui.isBiorandMode = config["isBiorandMode"];
            _ui.isHealthBarChecked = config["isHealthBarChecked"];
            _ui.isItemBoxChecked = config["isItemBoxChecked"];
            _ui.isChrisInventoryChecked = config["isChrisInventoryChecked"];
            _ui.isSherryChecked = config["isSherryChecked"];
            _ui.isMinimalistChecked = config["isMinimalistChecked"];
            _ui.isNoSegmentsTimerChecked = config["isNoSegmentsTimerChecked"];
            _ui.isNoStatsChecked = config["isNoStatsChecked"];
            _ui.isNoKeyItemsChecked = config["isNoKeyItemsChecked"];
            _ui.OneHPChallenge = config["OneHPChallenge"];
            _ui.NoDamageChallenge = config["NoDamageChallenge"];
            _ui.NoItemBoxChallenge = config["NoItemBoxChallenge"];
            _ui.DebugMode = config["DebugMode"];
            _ui.isStaticEnemyTrackerWindow = config["StaticEnemyTrackerWindow"];
            _ui.isDdrawChecked = config["Ddraw100"];
        }

        private void InitializeProcessWatcher()
        {
            // Set the game list
            _process = null;
            ProductPointer = IntPtr.Zero;
            VirtualMemoryPointer = IntPtr.Zero;

            _processName = _gameList[ComboBoxGameSelection.SelectedIndex];
            if (_processWatcher == null)
                _processWatcher = new Timer(ProcessWatcherCallback, null, 0, 1000);
        }

        private void InitializeRootObjectWatcher()
        {
            if (_rootObjectWatcher == null)
                _rootObjectWatcher = new Timer(RootObjectWatcherCallback, null, 0, 500);
        }

        private void InitializeSaveWatcher()
        {
            var index = ComboBoxGameSelection.SelectedIndex;

            if (index == BIOHAZARD_2_PC)
            {
                Library.UpdateTextBlock(Save, text: "Found", color: CustomColors.Green, isBold: true);
                return;
            }

            _processName = _gameList[ComboBoxGameSelection.SelectedIndex];
            var selectedGame = Library.GetGameName(_processName ?? "UNKNOWN GAME ERROR");
            var reJson = Library.GetReviewerConfig();

            reJson.TryGetValue(selectedGame, out var savePath);
            savePath ??= "Not Found";

            var saveContent = "Not Found" == savePath ? "Not Found" : "Found";
            var saveColor = "Not Found" == savePath ? CustomColors.Red : CustomColors.Green;
            Library.UpdateTextBlock(Save, text: saveContent, color: saveColor, isBold: true);
        }

        // selecting different window frames

        private void MenuListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            panelMain.Visibility = Visibility.Hidden;
            panelSettings.Visibility = Visibility.Hidden;
            panelAbout.Visibility = Visibility.Hidden;

            const int MAIN_PANEL = 1;
            const int OPTN_PANEL = 2;
            const int HELP_PANEL = 3;

            var selectedItem = (REviewerMenuItem)menuListView.SelectedItem;

            switch (selectedItem.MenuIndex)
            {
                case MAIN_PANEL:
                    panelMain.Visibility = Visibility.Visible;
                    break;
                case OPTN_PANEL:
                    panelSettings.Visibility = Visibility.Visible;
                    break;
                case HELP_PANEL:
                    panelAbout.Visibility = Visibility.Visible;
                    break;
            }
        }

        // Process Watcher


        private void ProcessWatcherCallback(object? state)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (_lock)
                {
                    // Check if the process is running
                    var process_list = Library.GetGameVersions(_processName);

                    for (int i = 0; i < process_list.Count; i++)
                    {
                        // https://github.com/kapdap/re-cvx-srt-provider/
                        // Kapdap you da real mvp
                        // <3
                        if (!_isProcessRunning && Process.GetProcessesByName(process_list[i]).Length > 0 && MD5 != null)
                        {
                            var tmp_process = Process.GetProcessesByName(process_list[i])[0];
                            if (tmp_process.ProcessName.ToLower() == Dolphin)
                            {
                                IntPtr pointer = IntPtr.Zero;

                                _dolphin = _dolphin ?? new Dolphin.Memory.Access.Dolphin(tmp_process);
                                _dolphin.TryGetBaseAddress(out pointer);

                                VirtualMemoryPointer = pointer;
                                ProductPointer = IntPtr.Add(VirtualMemoryPointer, 0x0);
                            }
                            else if (tmp_process.ProcessName.ToLower().StartsWith(PCSX2))
                            {
                                if (tmp_process.ProcessName.ToLower() == PCSX2) // PCSX2 1.6 and earlier
                                {
                                    VirtualMemoryPointer = new IntPtr(0x20000000);
                                    ProductPointer = IntPtr.Add(VirtualMemoryPointer, 0x00015B90);
                                }
                                else // PCSX2 1.7+
                                {
                                    // https://forums.pcsx2.net/Thread-PCSX2-1-7-Cheat-Engine-Script-Compatibility
                                    IntPtr process = NativeWrappers.LoadLibrary(tmp_process.MainModule.FileName);
                                    IntPtr address = NativeWrappers.GetProcAddress(process, "EEmem");

                                    VirtualMemoryPointer = (IntPtr)tmp_process.ReadValue<long>(address);

                                    if (tmp_process.ProcessName.ToLower() == PCSX264WX ||
                                        tmp_process.ProcessName.ToLower() == PCSX264WXAV)
                                        ProductPointer = IntPtr.Add(VirtualMemoryPointer, 0x000155D0);
                                    else
                                        ProductPointer = IntPtr.Add(VirtualMemoryPointer, 0x00012610);

                                    NativeWrappers.FreeLibrary(process);
                                }
                            }
                            /*
                            else // RPCS3
                            {
                                Scanner scanner = new Scanner(tmp_process, tmp_process.MainModule);
                                PatternScanResult result = scanner.FindPattern("50 53 33 5F 47 41 4D 45 00 00 00 00 00 00 00 00 08 00 00 00 00 00 00 00 0F 00 00 00 00 00 00 00 30 30");
                
                                IntPtr pointer = IntPtr.Add(tmp_process.MainModule.BaseAddress, result.Offset);
                                ProductPointer = result.Offset != 0 ? IntPtr.Add(pointer, -0xE0) : IntPtr.Zero;
                            }
                            */

                            // The process is running
                            _isProcessRunning = true;

                            // Get the process
                            _process = Process.GetProcessesByName(process_list[i])[0];
                            string md5Hash = Library.GetProcessMD5Hash(_process);

                            // Updating the TextBlock on the MainWindow
                            Library.UpdateTextBlock(MD5, text: md5Hash, color: CustomColors.Black, isBold: false);
                            Library.UpdateTextBlock(ProcessTextBlock, text: "Found", color: CustomColors.Green, isBold: true);

                            // Set the Exited event handler
                            _process.EnableRaisingEvents = true;
                            _process.Exited += Process_Exited;

                            // Log the event
                            Logger.Instance.Info($"Process {process_list[i]} has been found");
                        }
                    }
                }
            });
        }

        private void RootObjectWatcherCallback(object? state)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (_lock)
                {
                    // Check if the process is running
                    var index = ComboBoxGameSelection.SelectedIndex;

                    var selectedIndex = ComboBoxGameSelection.SelectedIndex;
                    var reJson = Library.GetReviewerConfig();
                    var isSaveFound = reJson.TryGetValue(_gameSelection[selectedIndex], out _);

                    if (index == BIOHAZARD_2_PC || index == BIOHAZARD_2_PL || index == BIOHAZARD_3_CH) isSaveFound = true;

                    if (_isProcessRunning && isSaveFound)
                    {
                        MappingGameVariables();
                        _rootObjectWatcher?.Dispose();
                    }
                }
            });
        }

        private void MappingGameVariables()
        {
            if (_isMappingDone)
            {
                return; // Mapping already done, no need to do it again
            }

            _isMappingDone = true;

            if (_process == null)
            {
                throw new ArgumentNullException(nameof(_process));
            }

            if (_residentEvilGame == null)
            {
                // Mapping the game data (Room IDs, Item IDs, etc.)
                GameData gameData = new GameData(_process.ProcessName);
                _itemIDs = new ItemIDs(_process.ProcessName);
                _residentEvilGame = gameData.GetGameData(_itemIDs, (int) VirtualMemoryPointer);
            }

            if (_tracking == null)
            {
                InitEnemies(_processName ?? "UNKNOWN GAME");
            }

            if (_MVariables == null)
            {
                _MVariables = new MonitorVariables((int) _process.Handle, _process.ProcessName, VirtualMemoryPointer, ProductPointer);
            }
            else
            {
                _MVariables.UpdateProcessHandle(_process.Handle);
            }

            if (_MVEnemies == null)
            {
                _MVEnemies = new MonitorVariables((int) _process.Handle, _process.ProcessName, VirtualMemoryPointer, ProductPointer);
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
            _isMappingDone = false;

            ProductPointer = IntPtr.Zero;
            VirtualMemoryPointer = IntPtr.Zero;

            // Stoping the monitoring 
            _MVariables?.Stop();
            _MVEnemies?.Stop();

            _rootObjectWatcher = new Timer(RootObjectWatcherCallback, null, 0, 100);

            var content = "Not Found";

            // Updating the TextBlock on the MainWindow
            Library.UpdateTextBlock(MD5, text: content, color: CustomColors.Red, isBold: true);
            Library.UpdateTextBlock(ProcessTextBlock, text: content, color: CustomColors.Red, isBold: true);
            
            // Log the event
            Logger.Instance.Info($"Process {_processName} has exited");
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            _processWatcher?.Dispose();
            _rootObjectWatcher?.Dispose();

            Dispose();
        }

        public void Dispose()
        {
            try
            {
                // Unsubscribe from process Exited event
                if (_process != null)
                    _process.Exited -= Process_Exited;

                // Dispose other disposable resources
                _processWatcher?.Dispose();
                _rootObjectWatcher?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error in Dispose method: {ex}");
            }
        }

        // making some checking if the selected game

        private void ComboBoxGameSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected item

            int selectedIndex = ((ComboBox)sender).SelectedIndex;
            _processName = _gameList[selectedIndex];
            _isProcessRunning = false;
            _isMappingDone = false;

            VirtualMemoryPointer = IntPtr.Zero;
            ProductPointer = IntPtr.Zero;

            // Kill the current process monitoring
            _processWatcher?.Dispose();
            _rootObjectWatcher?.Dispose();

            SRT?.Close();
            TRK?.Close();

            _residentEvilGame = null;
            _MVariables = null;
            _tracking = null;

            // Check every second if the process is running

            const string content = "Not Found";
            var reJson = Library.GetReviewerConfig();

            reJson.TryGetValue(_gameSelection[selectedIndex], out var saveREPath);
            saveREPath = saveREPath ?? "Not Found";

            UpdateUI(content, saveREPath, selectedIndex);
            InitCheckBoxes();

            _processWatcher = new Timer(ProcessWatcherCallback, null, 0, 1000);
            _rootObjectWatcher = new Timer(RootObjectWatcherCallback, null, 0, 100);

            Logger.Instance.Info($"Selected game: {_processName} -> Disabling old process watcher to the new one");
        }

        // Events for selecting the Save Path
        private void RE1SavePathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                RE1SavePath.Text = dialog.FileName;
                Library.UpdateConfigFile("RE1", dialog.FileName);
                Library.UpdateTextBlock(Save, text: "Found", color: CustomColors.Green, isBold: true);

            }
        }

        private void RE2SavePathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                RE2SavePath.Text = dialog.FileName;
                Library.UpdateConfigFile("RE2", dialog.FileName);
                Library.UpdateTextBlock(Save, text: "Found", color: CustomColors.Green, isBold: true);

            }
        }

        private void RE3SavePathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                RE3SavePath.Text = dialog.FileName;
                Library.UpdateConfigFile("RE3", dialog.FileName);
                Library.UpdateTextBlock(Save, text: "Found", color: CustomColors.Green, isBold: true);

            }
        }

        private void RECVXSavePathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                RECVXSavePath.Text = dialog.FileName;
                Library.UpdateConfigFile("RECVX", dialog.FileName);
                Library.UpdateTextBlock(Save, text: "Found", color: CustomColors.Green, isBold: true);

            }
        }

        // Event for clicking the Run Button
        private void Run_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsSRTAbleToRun())
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        lock (_lock)
                        {
                            if (IsSRTAbleToRun())
                            {
                                try
                                {
                                    if (_residentEvilGame == null || _MVariables == null || _processName == null)
                                    {
                                        MessageBox.Show($"The game data is not initialized! Please be sure everything is detected! {_residentEvilGame} {_MVariables} {_processName}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }

                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        // Console.WriteLine($"Creating SRT and Tracker -> {_processName}");
                                        var srtConfig = GenerateSRTUIConfig();
                                        SRT = new SRT(_residentEvilGame, _MVariables, srtConfig, _processName ?? "UNKNOWN GAME PROCESS ERROR");
                                        SRT.Show();

                                        if (_tracking != null)
                                        {
                                            TRK = new Tracker(_tracking, _residentEvilGame);
                                            TRK.Show();
                                        }
                                    });
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"An error has occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Instance.Error($"Error in Run_Click method: {ex}");
            }
        }

        public bool IsSRTAbleToRun()
        {
            var db = Library.GetGameList();

            if (string.IsNullOrEmpty(_processName))
            {
                MessageBox.Show("Game not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            var game = db[_processName];
            var configPath = ConfigurationManager.AppSettings["Config"];
            var reJson = Library.GetReviewerConfig();

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

            if (!reJson.TryGetValue(game, out _) && game != "RE2C" && game != "RE3C")
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
                ["Standard"] = BiorandMode.IsChecked,
                ["ItemBox"] = ShowItemBox.IsChecked,
                ["ChrisInventory"] = ChrisInventory.IsChecked,
                ["Sherry"] = Sherry.IsChecked,
                ["NoSegTimers"] = NoSegTimes.IsChecked,
                ["NoStats"] = NoStats.IsChecked,
                ["NoKeyItems"] = NoKeyItems.IsChecked,
                ["Minimalist"] = Minimalist.IsChecked,
                ["OneHP"] = OneHP.IsChecked,
                ["NoDamage"] = NoDamage.IsChecked,
                ["NoItemBox"] = NoItemBox.IsChecked,
                ["DebugMode"] = DebugMode.IsChecked,
                ["StaticEnemyTrackerWindow"] = StaticEnemyTrackerWindow.IsChecked,
                ["Ddraw100"] = DdrawBio3.IsChecked

                // ["IGTimer"] = IGTimerCheckBox.IsChecked,
                // ["RealTimer"] = RealTimerCheckBox.IsChecked
            };

            if (ComboBoxGameSelection.SelectedIndex == BIOHAZARD_2_PC || ComboBoxGameSelection.SelectedIndex == BIOHAZARD_2_PL || ComboBoxGameSelection.SelectedIndex == BIOHAZARD_3_CH)
            {
                srtConfig["Standard"] = false;
            }

            // Show the config elements in the log
            foreach (var item in srtConfig)
            {
                Logger.Instance.Debug($"{item.Key}: {item.Value}");
            }

            return srtConfig;
        }
        private void InitEnemies(string processName)
        {
            processName = char.ToUpper(processName[0]) + processName.Substring(1);
            var reDataPath = ConfigurationManager.AppSettings["REdata"];
            var json = File.ReadAllText(reDataPath) ?? throw new ArgumentNullException(nameof(reDataPath));
            var data = JsonConvert.DeserializeObject<Dictionary<string, Bio>>(json);
            var bio = data?.GetValueOrDefault(processName);
            var pname = processName.ToLower();

            int size = pname switch
            {
                "bio" or "biohazard" => 396,
                "bio2 1.10" or "bio2 1.1" or "bio2 chn claire" or "bio2 chn leon" => 4,
                "biohazard(r) 3 pc" => 4,
                "bio3 chn/twn" => 4,
                _ => 0
            };

            int selectedGame = pname switch
            {
                "bio" or "biohazard" => 0,
                "bio2 1.10" or "bio2 1.1" or "bio2 chn claire" or "bio2 chn leon" => 1,
                "biohazard(r) 3 pc" => 2,
                "bio3 chn/twn" => 3,
                _ => 0
            };

            if (bio?.Offsets == null || !bio.Offsets.ContainsKey("EnemyInfos") || bio.Offsets["EnemyInfos"] == "")
            {
                return;
            }

            var offset = Library.HexToInt(bio.Offsets["EnemyInfos"]);
            var enemyPointer = 0;
            var enemyArraySize = 0;

            switch (selectedGame)
            {
                case 0:
                    enemyArraySize = 16;
                    break;
                case 1:
                    enemyArraySize = 32;
                    break;
                case 2:
                    enemyArraySize = 32;
                    break;
                case 3:
                    enemyArraySize = 40;
                    break;
            }

            bio.Offsets.TryGetValue("EnemyPointer", out var enemyPointerOffset);
            if (enemyPointerOffset != null && enemyPointerOffset != "")
            {
                enemyPointer = Library.HexToInt(enemyPointerOffset);
            }

            var property = bio?.Enemy?.EnemyInfos;

            _tracking ??= new ObservableCollection<EnemyTracking>();

            for (var i = 0; i < enemyArraySize; i++)
            {
                _tracking.Add(new EnemyTracking(offset + (i * size), property, selectedGame, enemyPointer));
            }
        }


        private void CheckForNewVersion()
        {
            Task.Run(async () =>
            {
                try
                {
                    var version = await GetLatestVersionAsync();
                    // Console.WriteLine($"{version} - {CurrentVersion}");
                    if (version > CurrentVersion)
                    {
                        Dispatcher.Invoke(() => versionBox.Visibility = Visibility.Visible);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
        }
        private async Task<Version> GetLatestVersionAsync()
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("REviewer", CurrentVersion.ToString()));
                var response = await client.GetAsync("https://api.github.com/repos/namsku/reviewer/releases/latest");
                // Logger.Instance.Debug(response.ToString());
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var body = JsonConvert.DeserializeObject<VersionCheckBody>(jsonResponse);
                    var tagName = body.tag_name.Split('-')[0].Replace("v", "");
                    return System.Version.Parse(tagName);
                }
            }
            catch (HttpRequestException ex)
            {
                Logger.Instance.Debug($"HTTP request exception: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Logger.Instance.Debug($"JSON deserialization exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.Instance.Debug($"An error occurred: {ex.Message}");
            }

            throw new Exception("Unable to get latest version");
        }

        private void UpdateLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "https://github.com/namsku/REviewer/releases",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Logger.Instance.Debug($"An error occurred while opening the link: {ex.Message}");
            }
        }

        private void Link_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Hyperlink hyperlink)
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = hyperlink.NavigateUri.ToString(),
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Debug($"An error occurred while opening the link: {ex.Message}");
            }
        }

    }

}
