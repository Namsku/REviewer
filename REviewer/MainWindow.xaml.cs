using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using REviewer.Core.Configuration; // Added
using REviewer.Core.Constants;
using REviewer.Core.Memory;
using REviewer.Modules.RE;
using REviewer.Modules.RE.Common;
using REviewer.Modules.RE.Enemies;
using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;
using REviewer.Services.Game;
using REviewer.Services.Inventory;
using REviewer.Services.Race;
using REviewer.Services.Timer;
using REviewer.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Timer = System.Threading.Timer;

namespace REviewer
{
    public partial class MainWindow : Window
    {
        private Process? _process;
        private string? _processName;

        private bool _isProcessRunning = false;
        private bool _isMappingDone = false;

        private System.Threading.Timer? _processWatcher;
        private System.Threading.Timer? _rootObjectWatcher;

        private RootObject? _residentEvilGame;
        private ObservableCollection<EnemyTracking>? _tracking;
        private ItemIDs? _itemIDs;
        private MainWindowViewModel _viewModel;
        private RaceClient? _raceClient;

        // New Services and ViewModel
        private IGameStateService? _gameStateService;
        private ITimerService? _timerService;
        private IInventoryService? _inventoryService;
        private SRTViewModel? _srtViewModel;
        private IMemoryMonitor? _memoryMonitor;

        public Dolphin.Memory.Access.Dolphin _dolphin;

        public IntPtr VirtualMemoryPointer { get; private set; }
        public IntPtr ProductPointer { get; private set; }
        public int ProductLength { get; private set; }
        public bool IsBigEndian { get; private set; }

        private static readonly List<string> _gameList = new List<string>() { "Bio", "bio2 1.10", "bio2 chn claire", "bio2 chn leon", "BIOHAZARD(R) 3 PC", "Bio3 CHN/TWN", "CVX PS2 US" };
        private static readonly List<string> _gameSelection = new List<string>() { "RE1", "RE2", "RE2C", "RE2C", "RE3", "RE3C", "RECVX" };

        public static string Version => ConfigurationManager.AppSettings["Version"] ?? "1.0.1";
        private static System.Version CurrentVersion = GetCurrentVersion();

        private static System.Version GetCurrentVersion()
        {
            try
            {
                var v = Version;
                // Try finding the first numeric part (e.g. from "dev-1.0.1-alpha" -> "1.0.1")
                var match = System.Text.RegularExpressions.Regex.Match(v, @"\d+(\.\d+)*");
                if (match.Success && System.Version.TryParse(match.Value, out var parsed))
                    return parsed;
                return new System.Version(1, 0, 0);
            }
            catch
            {
                return new System.Version(1, 0, 0);
            }
        }
        public Config OverlayConfig { get; private set; }
        public Overlay? OVL { get; private set; }
        public SRT? SRT { get; private set; }
        public Tracker? TRK { get; private set; }

        private readonly object _lock = new object();
        private static readonly HttpClient _httpClient = new HttpClient();


        public MainWindow(
            IGameStateService gameStateService,
            ITimerService timerService,
            IInventoryService inventoryService,
            SRTViewModel srtViewModel,
            IMemoryMonitor memoryMonitor,
            MainWindowViewModel viewModel)
        {
            _gameStateService = gameStateService;
            _timerService = timerService;
            _inventoryService = inventoryService;
            _srtViewModel = srtViewModel;
            _memoryMonitor = memoryMonitor;
            _viewModel = viewModel;

            InitializeComponent();

            // Load Saved Position
            var (x, y) = Library.LoadWindowPosition("Main");
            if (x >= 0 && y >= 0)
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = x;
                Top = y;
            }

            // MVVM: Set DataContext
            DataContext = _viewModel;

            InitializeText();
            InitCheckBoxes();
            // Subscribe to ViewModel changes for aesthetics
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            InitializeSaveWatcher();
            InitializeProcessWatcher();
            InitializeRootObjectWatcher();

            CheckForNewVersion();

            Closed += MainWindow_Closed;
        }

        private void InitCheckBoxes()
        {
            if (_viewModel == null) return;

            // Reset all visibilities to Collapsed and booleans to false
            _viewModel.isBiorandMode = false;
            _viewModel.ChallengeVisibility = Visibility.Collapsed;
            _viewModel.ClassicVisibility = Visibility.Collapsed;
            _viewModel.ChrisInventory = Visibility.Collapsed;
            _viewModel.Sherry = Visibility.Collapsed;
            _viewModel.DebugModeVisibility = Visibility.Collapsed;
            _viewModel.DdrawBio3 = Visibility.Collapsed;

            switch (ComboBoxGameSelection.SelectedIndex)
            {
                case GameConstants.BIOHAZARD_1_MK:
                    _viewModel.isBiorandMode = true;
                    _viewModel.ChallengeVisibility = Visibility.Visible;
                    _viewModel.ClassicVisibility = Visibility.Visible;
                    _viewModel.ChrisInventory = Visibility.Visible;
                    _viewModel.DebugModeVisibility = Visibility.Visible;
                    break;
                case GameConstants.BIOHAZARD_2_SC:
                    _viewModel.isBiorandMode = true;
                    _viewModel.ChallengeVisibility = Visibility.Visible;
                    _viewModel.ClassicVisibility = Visibility.Visible;
                    _viewModel.Sherry = Visibility.Visible;
                    break;
                case GameConstants.BIOHAZARD_2_PC:
                case GameConstants.BIOHAZARD_2_PL:
                    _viewModel.Sherry = Visibility.Visible;
                    _viewModel.DebugModeVisibility = Visibility.Visible;
                    break;
                case GameConstants.BIOHAZARD_3_RB:
                    _viewModel.isBiorandMode = true;
                    _viewModel.ChallengeVisibility = Visibility.Visible;
                    _viewModel.ClassicVisibility = Visibility.Visible;
                    _viewModel.DebugModeVisibility = Visibility.Visible;
                    _viewModel.DdrawBio3 = Visibility.Visible;
                    break;
                case GameConstants.BIOHAZARD_3_CH:
                    // All remain collapsed/false
                    break;
                case GameConstants.BIOHAZARD_CV_X:
                    _viewModel.isBiorandMode = true;
                    _viewModel.ClassicVisibility = Visibility.Visible;
                    break;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
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
                    case GameConstants.BIOHAZARD_1_MK:
                        Library.UpdateTextBox(RE1SavePath, text: savePath, isBold: false);
                        break;
                    case GameConstants.BIOHAZARD_2_SC:
                        Library.UpdateTextBox(RE2SavePath, text: savePath, isBold: false);
                        break;
                    case GameConstants.BIOHAZARD_2_PC:
                        break;
                    case GameConstants.BIOHAZARD_2_PL:
                        break;
                    case GameConstants.BIOHAZARD_3_RB:
                        Library.UpdateTextBox(RE3SavePath, text: savePath, isBold: false);
                        break;
                    case GameConstants.BIOHAZARD_3_CH:
                        break;
                    case GameConstants.BIOHAZARD_CV_X:
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

        // InitializeSavedOptions is now handled by MainWindowViewModel

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

            if (index == GameConstants.BIOHAZARD_2_PC)
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

        // Navigation is now handled by NavHome_Click, NavSettings_Click, NavAbout_Click in #region Navigation

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

                        // Auto-detect game if not running
                        // Aggressive auto-detection removed per user request
                        // The user should be able to manually select a game even if another one is running.
                        // We will rely on manual selection or initial auto-detection only.

                        if (!_isProcessRunning && Process.GetProcessesByName(process_list[i]).Length > 0 && MD5 != null)
                        {
                            var tmp_process = Process.GetProcessesByName(process_list[i])[0];
                            if (tmp_process.ProcessName.ToLower() == GameConstants.DOLPHIN)
                            {
                                IntPtr pointer = IntPtr.Zero;

                                _dolphin = _dolphin ?? new Dolphin.Memory.Access.Dolphin(tmp_process);
                                _dolphin.TryGetBaseAddress(out pointer);

                                VirtualMemoryPointer = pointer;
                                ProductPointer = IntPtr.Add(VirtualMemoryPointer, 0x0);
                            }
                            else if (tmp_process.ProcessName.ToLower().StartsWith(GameConstants.PCSX2))
                            {
                                if (tmp_process.ProcessName.ToLower() == GameConstants.PCSX2) // PCSX2 1.6 and earlier
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

                                    if (tmp_process.ProcessName.ToLower() == GameConstants.PCSX264WX ||
                                        tmp_process.ProcessName.ToLower() == GameConstants.PCSX264WXAV)
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

                            // Updating the hidden TextBlocks (for backward compatibility)
                            Library.UpdateTextBlock(MD5, text: md5Hash, color: CustomColors.Black, isBold: false);
                            Library.UpdateTextBlock(ProcessTextBlock, text: "Found", color: CustomColors.Green, isBold: true);

                            // Update the new visible Status Monitor elements
                            // MD5Status now shows the process name instead of MD5 hash
                            MD5Status.Text = $"[ {process_list[i]} ]";
                            MD5Status.Foreground = (System.Windows.Media.SolidColorBrush)FindResource("Brush.Status.Success");

                            ProcessStatus.Text = "[ HOOKED ]";
                            ProcessStatus.Foreground = (System.Windows.Media.SolidColorBrush)FindResource("Brush.Status.Success");

                            // Update status bar as well
                            StatusMD5.Text = $"Process: {process_list[i]}";
                            StatusProcess.Text = "Status: Hooked";

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

                    if (index == GameConstants.BIOHAZARD_2_PC || index == GameConstants.BIOHAZARD_2_PL || index == GameConstants.BIOHAZARD_3_CH || index == GameConstants.BIOHAZARD_CV_X
                        || index == GameConstants.BIOHAZARD_2_SC || index == GameConstants.BIOHAZARD_3_RB) isSaveFound = true;

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
                _residentEvilGame = gameData.GetGameData(_itemIDs, (int)VirtualMemoryPointer);

                // Initialize Aesthetics from ViewModel
                if (_residentEvilGame != null)
                {
                    _residentEvilGame.CustomBackgroundPath = _viewModel.CustomBackgroundPath;
                    _residentEvilGame.CustomBackgroundColor = _viewModel.CustomBackgroundColor;
                    _residentEvilGame.CustomBackgroundOpacity = _viewModel.CustomBackgroundOpacity;
                }
            }

            if (_tracking == null)
            {
                InitEnemies(_processName ?? "UNKNOWN GAME");
            }

            // Update Memory Monitor with process info
            _memoryMonitor?.UpdateProcessHandle(_process.Handle, _process.ProcessName);

            // Register objects for monitoring
            if (_residentEvilGame != null) _memoryMonitor?.Register(_residentEvilGame);
            if (_tracking != null) _memoryMonitor?.Register(_tracking);

            _memoryMonitor?.Start();

            // Initialize Services and ViewModel
            if (_itemIDs != null && _residentEvilGame != null && _residentEvilGame.Bio != null)
            {
                // Services are already injected via constructor

                // Initialize Services
                _gameStateService?.Initialize(_residentEvilGame.Bio); // Requires Bio object (Module.RE.Json.Bio)
                // _timerService doesn't need explicit init beyond default constructor, logic is in UpdateTimer
                _inventoryService?.Initialize(_residentEvilGame.Bio, VirtualMemoryPointer, Library.GetGameId(_processName ?? ""), _itemIDs);

                // Start Monitoring GameStateService
                _gameStateService?.InitMonitoring(VirtualMemoryPointer);
                if (_gameStateService != null) _memoryMonitor?.Register(_gameStateService);

                // Set Game ID in GameStateService
                _gameStateService?.SetGame(Library.GetGameId(_processName ?? ""));
            }
        }

        private void Process_Exited(object? sender, EventArgs e)
        {
            // The process has exited
            _isProcessRunning = false;
            _isMappingDone = false;

            ProductPointer = IntPtr.Zero;
            VirtualMemoryPointer = IntPtr.Zero;

            // Stop monitoring
            _memoryMonitor?.Stop();

            _rootObjectWatcher = new System.Threading.Timer(RootObjectWatcherCallback, null, 0, 100);

            var content = "Not Found";

            // Updating the hidden TextBlocks (for backward compatibility)
            Library.UpdateTextBlock(MD5, text: content, color: CustomColors.Red, isBold: true);
            Library.UpdateTextBlock(ProcessTextBlock, text: content, color: CustomColors.Red, isBold: true);

            // Update the visible Status Monitor elements on UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                MD5Status.Text = "[ SEARCHING ]";
                MD5Status.Foreground = (System.Windows.Media.SolidColorBrush)FindResource("Brush.Status.Warning");

                ProcessStatus.Text = "[ WAITING ]";
                ProcessStatus.Foreground = (System.Windows.Media.SolidColorBrush)FindResource("Brush.Status.Warning");

                StatusMD5.Text = "Process: Searching";
                StatusProcess.Text = "Status: Waiting";
            });

            // Close all active window frames, including Overlay
            CloseAllWindowFrames();

            // Log the event
            Logger.Instance.Info($"Process {_processName} has exited");
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            // Save Position
            Library.SaveWindowPosition("Main", Left, Top);

            // Close all active window frames, including Overlay
            CloseAllWindowFrames();

            _processWatcher?.Dispose();
            _rootObjectWatcher?.Dispose();

            Dispose();
        }

        private void CloseAllWindowFrames()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Close Overlay window  
                SRT?.Close();

                // Close Tracker window  
                TRK?.Close();

                // Close Overlay window  
                OVL?.Close();
            });

            _raceClient?.Stop();

            // Log closure of all windows  
            Logger.Instance.Info("All window frames have been closed.");
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

                // Ensure all window frames are closed
                CloseAllWindowFrames();
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error in Dispose method: {ex}");
            }
        }

        // making some checking if the selected game

        private void ComboBoxGameSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Guard: Skip during initialization when UI elements aren't ready yet
            if (!IsLoaded) return;

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

            Application.Current.Dispatcher.Invoke(() =>
            {
                SRT?.Close();
                TRK?.Close();

                // Reset visible status monitor elements
                MD5Status.Text = "[ SEARCHING ]";
                MD5Status.Foreground = (System.Windows.Media.SolidColorBrush)FindResource("Brush.Status.Warning");

                ProcessStatus.Text = "[ WAITING ]";
                ProcessStatus.Foreground = (System.Windows.Media.SolidColorBrush)FindResource("Brush.Status.Warning");

                StatusMD5.Text = "Process: Searching";
                StatusProcess.Text = "Status: Waiting";
            });

            _residentEvilGame = null;
            _tracking = null;

            const string content = "Not Found";
            var reJson = Library.GetReviewerConfig();

            reJson.TryGetValue(_gameSelection[selectedIndex], out var saveREPath);
            saveREPath = saveREPath ?? "Not Found";

            UpdateUI(content, saveREPath, selectedIndex);
            InitCheckBoxes();

            _processWatcher = new System.Threading.Timer(ProcessWatcherCallback, null, 0, 1000);
            _rootObjectWatcher = new System.Threading.Timer(RootObjectWatcherCallback, null, 0, 100);

            Logger.Instance.Info($"Selected game: {_processName} -> Resetting status and starting new process watcher");
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
                                    if (_residentEvilGame == null || _memoryMonitor == null || _processName == null)
                                    {
                                        MessageBox.Show($"The game data is not initialized! Please be sure everything is detected! {_residentEvilGame} {_memoryMonitor} {_processName}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }

                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        // Close existing windows before creating new ones
                                        CloseExistingWindows();

                                        // Create SRT window
                                        var srtConfig = GenerateSRTUIConfig();
                                        if (_srtViewModel == null) throw new Exception("SRT ViewModel is not initialized!");

                                        // Wire up RootObject to ViewModel
                                        _srtViewModel.SetGameData(_residentEvilGame);

                                        // Update ViewModel Configuration
                                        _srtViewModel.UpdateConfiguration(srtConfig, _viewModel);

                                        SRT = new SRT(_residentEvilGame, _memoryMonitor!, srtConfig, _processName ?? "UNKNOWN GAME PROCESS ERROR", _srtViewModel);
                                        SRT.Show();

                                        // Create Overlay window
                                        var overlayConfig = new Config(OverlayPosition.SelectedIndex, 16, (int)(_viewModel.OverlayScale * 100));
                                        OVL = new Overlay(_process, overlayConfig, _residentEvilGame);
                                        OVL.Show();

                                        if (_tracking != null)
                                        {
                                            TRK = new Tracker(_tracking, _residentEvilGame);
                                            TRK.Show();

                                            // Auto-dock SRT and Tracker by default
                                            SRT.DockWithTracker(TRK, false);
                                            TRK.DockWithSRT(SRT);
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

        /// <summary>
        /// Closes existing SRT, Overlay, and Tracker windows before creating new ones.
        /// This prevents duplicate windows when re-running the game.
        /// </summary>
        private void CloseExistingWindows()
        {
            try
            {
                if (SRT != null)
                {
                    SRT.Close();
                    SRT = null;
                }

                if (OVL != null)
                {
                    OVL.Close();
                    OVL = null;
                }

                if (TRK != null)
                {
                    TRK.Close();
                    TRK = null;
                }

                Logger.Instance.Info("Closed existing windows for fresh restart");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error closing existing windows: {ex.Message}");
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

            // Bypass check for RE2, RE3, RECVX and their variants
            if (!reJson.TryGetValue(game, out _) && game != "RE2C" && game != "RE3C" && game != "RE2" && game != "RE3" && game != "RECVX")
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

            if (ComboBoxGameSelection.SelectedIndex == GameConstants.BIOHAZARD_2_PC || ComboBoxGameSelection.SelectedIndex == GameConstants.BIOHAZARD_2_PL || ComboBoxGameSelection.SelectedIndex == GameConstants.BIOHAZARD_3_CH)
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
                _tracking.Add(new EnemyTracking(offset + (i * size), property, selectedGame, enemyPointer, _residentEvilGame));
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
                _httpClient.DefaultRequestHeaders.UserAgent.Clear();
                _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("REviewer", CurrentVersion.ToString()));
                var response = await _httpClient.GetAsync("https://api.github.com/repos/namsku/reviewer/releases/latest");
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

        private void SupportKoFi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "https://ko-fi.com/namsku#checkoutModal",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Logger.Instance.Debug($"An error occurred while opening the Ko-fi link: {ex.Message}");
            }
        }

        #region Navigation

        private void NavHome_Click(object sender, RoutedEventArgs e)
        {
            SetActivePanel("Home");
        }

        private void NavAbout_Click(object sender, RoutedEventArgs e)
        {
            SetActivePanel("About");
        }

        private void NavDebug_Click(object sender, RoutedEventArgs e)
        {
            SetActivePanel("Debug");
            LoadDebugLogs();
        }

        private void NavSettings_Click(object sender, RoutedEventArgs e)
        {
            SetActivePanel("Settings");
        }

        private void NavRace_Click(object sender, RoutedEventArgs e)
        {
            SetActivePanel("Race");
        }

        private void SetActivePanel(string panelName)
        {
            // Hide all panels
            panelMain.Visibility = Visibility.Collapsed;
            panelSettings.Visibility = Visibility.Collapsed;
            panelAbout.Visibility = Visibility.Collapsed;
            panelDebug.Visibility = Visibility.Collapsed;
            panelRace.Visibility = Visibility.Collapsed;

            // Reset all button states
            btnHome.Tag = null;
            btnAbout.Tag = null;
            btnDebug.Tag = null;
            btnSettings.Tag = null;
            btnRace.Tag = null;

            // Show selected panel and mark button active
            switch (panelName)
            {
                case "Home":
                    panelMain.Visibility = Visibility.Visible;
                    btnHome.Tag = "Active";
                    break;
                case "About":
                    panelAbout.Visibility = Visibility.Visible;
                    btnAbout.Tag = "Active";
                    break;
                case "Debug":
                    panelDebug.Visibility = Visibility.Visible;
                    btnDebug.Tag = "Active";
                    break;
                case "Settings":
                    panelSettings.Visibility = Visibility.Visible;
                    btnSettings.Tag = "Active";
                    break;
                case "Race":
                    panelRace.Visibility = Visibility.Visible;
                    btnRace.Tag = "Active";
                    break;
            }
        }

        private void LoadDebugLogs()
        {
            try
            {
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "reviewer.log");
                if (File.Exists(logPath))
                {
                    // Read last 100 lines to avoid loading too much data
                    var lines = File.ReadAllLines(logPath);
                    var lastLines = lines.Length > 100 ? lines.Skip(lines.Length - 100) : lines;
                    LogOutput.Text = string.Join(Environment.NewLine, lastLines);
                }
                else
                {
                    LogOutput.Text = "No log file found at: " + logPath;
                }
            }
            catch (Exception ex)
            {
                LogOutput.Text = $"Error loading logs: {ex.Message}";
            }
        }

        private async void ConnectRace_Click(object sender, RoutedEventArgs e)
        {
            if (_residentEvilGame == null)
            {
                MessageBox.Show("Please hook the game first before connecting to a race.", "Game Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Disable button to prevent double clicks
            btnConnectRace.IsEnabled = false;

            if (_raceClient == null)
            {
                _raceClient = new RaceClient(_residentEvilGame);

                // Wire up events
                _raceClient.OnLog += (msg) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RaceLogBox.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {msg}");
                    });
                };

                _raceClient.OnConnectionStatusChanged += (isConnected) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (isConnected)
                        {
                            RaceConnectionStatus.Text = "Connected";
                            RaceConnectionStatus.Foreground = (System.Windows.Media.SolidColorBrush)FindResource("Brush.Status.Success");
                            btnConnectRace.Visibility = Visibility.Collapsed;
                            btnDisconnectRace.Visibility = Visibility.Visible;
                            StatusProcess.Text = $"Status: Racing";
                        }
                        else
                        {
                            RaceConnectionStatus.Text = "Disconnected";
                            RaceConnectionStatus.Foreground = (System.Windows.Media.SolidColorBrush)FindResource("Brush.Status.Error");
                            btnConnectRace.Visibility = Visibility.Visible;
                            btnDisconnectRace.Visibility = Visibility.Collapsed;
                            btnConnectRace.IsEnabled = true;
                            StatusProcess.Text = "Status: Hooked";
                        }
                    });
                };
            }

            string url = RaceServerUrl.Text;
            if (!int.TryParse(RaceServerPort.Text, out int port)) port = 5006;
            string roomId = RaceRoomId.Text;
            string password = RacePassword.Password;
            string runnerId = RaceRunnerColor.SelectedIndex == 0 ? "blue" : "red";
            string gameName = ComboBoxGameSelection.Text;

            try
            {
                await Task.Run(() => _raceClient.Start(url, port, roomId, password, runnerId, gameName));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to race: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                btnConnectRace.IsEnabled = true;
            }
        }

        private void DisconnectRace_Click(object sender, RoutedEventArgs e)
        {
            _raceClient?.Disconnect();
        }

        #endregion


        #region Aesthetics & Themes Handlers

        private void BrowseBackground_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = false,
                Filters = { new CommonFileDialogFilter("Images", "*.png;*.jpg;*.jpeg;*.bmp") }
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _viewModel.CustomBackgroundPath = dialog.FileName;
            }
        }

        private void PickColor_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.ColorDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var c = dialog.Color;
                    var hex = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
                    _viewModel.CustomBackgroundColor = hex;
                }
            }
        }

        private void PickTimerColor_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.ColorDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var c = dialog.Color;
                    var hex = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
                    _viewModel.TimerColor = hex;
                }
            }
        }

        private void ResetTimerColor_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.TimerColor = null; // Will trigger default in getter/converter or null in VM
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_residentEvilGame == null) return;

            if (e.PropertyName == nameof(MainWindowViewModel.CustomBackgroundPath))
                _residentEvilGame.CustomBackgroundPath = _viewModel.CustomBackgroundPath;

            if (e.PropertyName == nameof(MainWindowViewModel.CustomBackgroundColor))
                _residentEvilGame.CustomBackgroundColor = _viewModel.CustomBackgroundColor;

            if (e.PropertyName == nameof(MainWindowViewModel.CustomBackgroundOpacity))
                _residentEvilGame.CustomBackgroundOpacity = _viewModel.CustomBackgroundOpacity;

            if (e.PropertyName == nameof(MainWindowViewModel.TimerColor))
                _residentEvilGame.TimerColor = _viewModel.TimerColor;

            if (e.PropertyName == nameof(MainWindowViewModel.OverlayScale))
            {
                if (OVL != null)
                {
                    OVL.OverlayConfig.scaling = (int)(_viewModel.OverlayScale * 100);
                    OVL.ApplyScaling();
                }
            }
        }

        #endregion

    }

}
