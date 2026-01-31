using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows;
using REviewer.Modules.Utils;
using REviewer.Services;
using REviewer.Services.Game;
using REviewer.Services.Timer;
using REviewer.Services.Inventory;
using REviewer.Services.Configuration;
using REviewer.Services.Processes;
using REviewer.Core.Memory;

namespace REviewer.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly ProcessWatcherService _processWatcher;
        private readonly ConfigurationService _configService;
        private readonly GameMemoryService _gameMemory;

        private string _statusText = "Not Found";
        private SolidColorBrush _statusColor = Brushes.Red;
        private string _md5Hash = "Not Found";
        private Dictionary<string, string> _gameList;
        private string _selectedGameKey;

        // Aesthetics & Themes
        private string? _customBackgroundPath;
        public string? CustomBackgroundPath
        {
            get => _customBackgroundPath;
            set
            {
                if (_customBackgroundPath != value)
                {
                    _customBackgroundPath = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("CustomBackgroundPath", value ?? "");
                }
            }
        }

        private string? _customBackgroundColor;
        public string? CustomBackgroundColor
        {
            get => _customBackgroundColor;
            set
            {
                if (_customBackgroundColor != value)
                {
                    _customBackgroundColor = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("CustomBackgroundColor", value ?? "");
                }
            }
        }

        private string? _timerColor;
        public string? TimerColor
        {
            get => _timerColor;
            set
            {
                if (_timerColor != value)
                {
                    _timerColor = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("TimerColor", value ?? "");
                }
            }
        }

        private double _overlayScale = 1.0;
        public double OverlayScale
        {
            get => _overlayScale;
            set
            {
                if (_overlayScale != value)
                {
                    _overlayScale = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("OverlayScale", value.ToString("F2"));
                }
            }
        }

        private double _customBackgroundOpacity = 1.0;
        public double CustomBackgroundOpacity
        {
            get => _customBackgroundOpacity;
            set
            {
                if (_customBackgroundOpacity != value)
                {
                    _customBackgroundOpacity = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("CustomBackgroundOpacity", value.ToString("F2"));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindowViewModel(ProcessWatcherService processWatcher, ConfigurationService configService, GameMemoryService gameMemory)
        {
            _processWatcher = processWatcher;
            _configService = configService;
            _gameMemory = gameMemory;

            _gameList = _configService.GetGameList();
            _selectedGameKey = string.Empty;
            
            // Load persisted UI options
            InitializeSavedOptions();

            // Load Aesthetics
            _customBackgroundPath = Library.GetSetting("CustomBackgroundPath", "");
            _customBackgroundColor = Library.GetSetting("CustomBackgroundColor", "");
            _customBackgroundOpacity = Library.GetSetting("CustomBackgroundOpacity", 1.0);
            _overlayScale = Library.GetSetting("OverlayScale", 1.0);
            _timerColor = Library.GetSetting("TimerColor", "");

            _processWatcher.OnProcessFound += OnProcessFound;
            _processWatcher.OnProcessExited += OnProcessExited;
            
            _processWatcher.Start();
        }

        private void InitializeSavedOptions()
        {
            var config = _configService.GetOptions();
            _isBiorandMode = config.GetValueOrDefault("isBiorandMode", false);
            _isHealthBarChecked = config.GetValueOrDefault("isHealthBarChecked", false);
            _isItemBoxChecked = config.GetValueOrDefault("isItemBoxChecked", false);
            _isChrisInventoryChecked = config.GetValueOrDefault("isChrisInventoryChecked", false);
            _isSherryChecked = config.GetValueOrDefault("isSherryChecked", false);
            _isMinimalistChecked = config.GetValueOrDefault("isMinimalistChecked", false);
            _isNoSegmentsTimerChecked = config.GetValueOrDefault("isNoSegmentsTimerChecked", false);
            _isNoStatsChecked = config.GetValueOrDefault("isNoStatsChecked", false);
            _isNoKeyItemsChecked = config.GetValueOrDefault("isNoKeyItemsChecked", false);
            _oneHPChallenge = config.GetValueOrDefault("OneHPChallenge", false);
            _noDamageChallenge = config.GetValueOrDefault("NoDamageChallenge", false);
            _noItemBoxChallenge = config.GetValueOrDefault("NoItemBoxChallenge", false);
            _debugMode = config.GetValueOrDefault("DebugMode", false);
            _staticEnemyTrackerWindow = config.GetValueOrDefault("StaticEnemyTrackerWindow", false);
            _isDdrawChecked = config.GetValueOrDefault("Ddraw100", false);
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public SolidColorBrush StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(); }
        }

        public string MD5Hash
        {
            get => _md5Hash;
            set { _md5Hash = value; OnPropertyChanged(); }
        }

        public Dictionary<string, string> GameList
        {
            get => _gameList;
            set { _gameList = value; OnPropertyChanged(); }
        }

        public string SelectedGameKey
        {
            get => _selectedGameKey;
            set
            {
                if (_selectedGameKey != value)
                {
                    _selectedGameKey = value;
                    OnPropertyChanged();
                    _processWatcher.SetGameKey(value);
                }
            }
        }

        private void OnProcessFound(object? sender, Process process)
        {
            StatusText = "Found";
            StatusColor = Brushes.Green;
            MD5Hash = Library.GetProcessMD5Hash(process);
            
            // Further initialization (GameMemoryService) would happen here
            // _gameMemory.InitializeMonitors(...)
        }

        private void OnProcessExited(object? sender, EventArgs e)
        {
            StatusText = "Not Found";
            StatusColor = Brushes.Red;
            MD5Hash = "Not Found";
            
            _gameMemory.StopMonitoring();
        }

        // UINotify Properties
        private string? _version;
        public string? Version
        {
            get { return _version; }
            set { if (_version != value) { _version = value; OnPropertyChanged(); } }
        }

        private Visibility _chrisInventory = Visibility.Collapsed;
        public Visibility ChrisInventory
        {
            get { return _chrisInventory; }
            set { if (_chrisInventory != value) { _chrisInventory = value; OnPropertyChanged(); } }
        }

        private Visibility _classicVisibility = Visibility.Collapsed;
        public Visibility ClassicVisibility
        {
            get { return _classicVisibility; }
            set { if (_classicVisibility != value) { _classicVisibility = value; OnPropertyChanged(); } }
        }

        private Visibility _challengeVisibility = Visibility.Collapsed;
        public Visibility ChallengeVisibility
        {
            get { return _challengeVisibility; }
            set { if (_challengeVisibility != value) { _challengeVisibility = value; OnPropertyChanged(); } }
        }

        private Visibility _sherry = Visibility.Collapsed;
        public Visibility Sherry
        {
            get { return _sherry; }
            set { if (_sherry != value) { _sherry = value; OnPropertyChanged(); } }
        }

        private Visibility _debugModeVisibility = Visibility.Collapsed;
        public Visibility DebugModeVisibility
        {
            get { return _debugModeVisibility; }
            set { if (_debugModeVisibility != value) { _debugModeVisibility = value; OnPropertyChanged(); } }
        }

        private Visibility _dDrawBio3 = Visibility.Collapsed;
        public Visibility DdrawBio3
        {
            get { return _dDrawBio3; }
            set { if (_dDrawBio3 != value) { _dDrawBio3 = value; OnPropertyChanged(); } }
        }

        private bool _isBiorandMode;
        public bool isBiorandMode
        {
            get { return _isBiorandMode; }
            set
            {
                if (_isBiorandMode != value)
                {
                    _isBiorandMode = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("isBiorandMode", _isBiorandMode.ToString().ToLower());
                }
            }
        }

        private bool _isHealthBarChecked;
        public bool isHealthBarChecked
        {
            get { return _isHealthBarChecked; }
            set
            {
                if (_isHealthBarChecked != value)
                {
                    _isHealthBarChecked = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("isHealthBarChecked", _isHealthBarChecked.ToString().ToLower());
                }
            }
        }

        private bool _isItemBoxChecked;
        public bool isItemBoxChecked
        {             
            get { return _isItemBoxChecked; }
            set
            {
                if (_isItemBoxChecked != value)
                {
                    _isItemBoxChecked = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("isItemBoxChecked", _isItemBoxChecked.ToString().ToLower());
                }
            }
        }

        private bool _isChrisInventoryChecked;
        public bool isChrisInventoryChecked
        {
            get { return _isChrisInventoryChecked; }
            set
            {
                if (_isChrisInventoryChecked != value)
                {
                    _isChrisInventoryChecked = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("isChrisInventoryChecked", _isChrisInventoryChecked.ToString().ToLower());
                }
            }
        }

        private bool _isSherryChecked;
        public bool isSherryChecked
        {
            get { return _isSherryChecked; }
            set
            {
                if (_isSherryChecked != value)
                {
                    _isSherryChecked = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("isSherryChecked", _isSherryChecked.ToString().ToLower());
                }
            }
        }

        private bool _isMinimalistChecked;
        public bool isMinimalistChecked
        {
            get { return _isMinimalistChecked; }
            set
            {
                if (_isMinimalistChecked != value)
                {
                    _isMinimalistChecked = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("isMinimalistChecked", _isMinimalistChecked.ToString().ToLower());
                }
            }
        }

        private bool _isNoSegmentsTimerChecked;
        public bool isNoSegmentsTimerChecked
        {
            get { return _isNoSegmentsTimerChecked; }
            set
            {
                if (_isNoSegmentsTimerChecked != value)
                {
                    _isNoSegmentsTimerChecked = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("isNoSegmentsTimerChecked", _isNoSegmentsTimerChecked.ToString().ToLower());
                }
            }
        }

        private bool _isNoStatsChecked;
        public bool isNoStatsChecked
        {
            get { return _isNoStatsChecked; }
            set
            {
                if (_isNoStatsChecked != value)
                {
                    _isNoStatsChecked = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("isNoStatsChecked", _isNoStatsChecked.ToString().ToLower());
                }
            }
        }

        private bool _isNoKeyItemsChecked;
        public bool isNoKeyItemsChecked
        {
            get { return _isNoKeyItemsChecked; }
            set
            {
                if (_isNoKeyItemsChecked != value)
                {
                    _isNoKeyItemsChecked = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("isNoKeyItemsChecked", _isNoKeyItemsChecked.ToString().ToLower());
                }
            }
        }

        private bool _oneHPChallenge;
        public bool OneHPChallenge
        {
            get { return _oneHPChallenge; }
            set
            {
                if (_oneHPChallenge != value)
                {
                    _oneHPChallenge = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("OneHPChallenge", _oneHPChallenge.ToString().ToLower());
                }
            }
        }

        private bool _noDamageChallenge;
        public bool NoDamageChallenge
        {
            get { return _noDamageChallenge; }
            set
            {
                if (_noDamageChallenge != value)
                {
                    _noDamageChallenge = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("NoDamageChallenge", _noDamageChallenge.ToString().ToLower());
                }
            }
        }

        private bool _noItemBoxChallenge;
        public bool NoItemBoxChallenge
        {
            get { return _noItemBoxChallenge; }
            set
            {
                if (_noItemBoxChallenge != value)
                {
                    _noItemBoxChallenge = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("NoItemBoxChallenge", _noItemBoxChallenge.ToString().ToLower());
                }
            }
        }

        private bool _debugMode;
        public bool DebugMode
        {
            get { return _debugMode; }
            set
            {
                if (_debugMode != value)
                {
                    _debugMode = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("DebugMode", _debugMode.ToString().ToLower());
                }
            }
        }

        private bool _staticEnemyTrackerWindow;
        public bool isStaticEnemyTrackerWindow
        {
            get { return _staticEnemyTrackerWindow; }
            set
            {
                if (_staticEnemyTrackerWindow != value)
                {
                    _staticEnemyTrackerWindow = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("StaticEnemyTrackerWindow", _staticEnemyTrackerWindow.ToString().ToLower());
                }
            }
        }

        private bool _isDdrawChecked;
        public bool isDdrawChecked
        {
            get { return _isDdrawChecked; }
            set
            {
                if (_isDdrawChecked != value)
                {
                    _isDdrawChecked = value;
                    OnPropertyChanged();
                    Library.UpdateConfigFile("Ddraw100", _isDdrawChecked.ToString().ToLower());
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
