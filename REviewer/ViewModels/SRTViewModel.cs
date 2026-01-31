using REviewer.Modules.RE.Common;
using REviewer.Services.Game;
using REviewer.Services.Inventory;
using REviewer.Services.Timer;
using System.ComponentModel;

namespace REviewer.ViewModels
{
    public class SRTViewModel : ViewModelBase
    {
        private readonly IGameStateService _gameStateService;
        private readonly ITimerService _timerService;
        private readonly IInventoryService _inventoryService;

        // Direct reference to RootObject for game data bindings
        private RootObject? _gameData;
        public RootObject? GameData
        {
            get => _gameData;
            set
            {
                if (SetField(ref _gameData, value))
                {
                    OnPropertyChanged(nameof(Health));
                    OnPropertyChanged(nameof(Character));
                    OnPropertyChanged(nameof(InventorySlotSelected));
                    OnPropertyChanged(nameof(CharacterName));
                    OnPropertyChanged(nameof(MaxHealth));
                    OnPropertyChanged(nameof(InventoryImages));
                    OnPropertyChanged(nameof(KeyItemImages));
                    OnPropertyChanged(nameof(ItemboxImages));
                    OnPropertyChanged(nameof(Deaths));
                    OnPropertyChanged(nameof(Saves));
                    OnPropertyChanged(nameof(Resets));
                    OnPropertyChanged(nameof(Hits));
                    OnPropertyChanged(nameof(IGTHumanFormat));
                    OnPropertyChanged(nameof(LastItemFoundImage));
                    OnPropertyChanged(nameof(InventorySlotSelectedImage));
                    OnPropertyChanged(nameof(InventorySlotSelectedQuantity));
                    OnPropertyChanged(nameof(TimerColorBrush));
                    OnPropertyChanged(nameof(ECGPoints));

                    // Subscribe to RootObject property changes
                    if (_gameData != null)
                    {
                        _gameData.PropertyChanged += GameData_PropertyChanged;
                    }
                }
            }
        }

        // Expose RootObject properties directly for binding
        public Modules.Utils.VariableData? Health => _gameData?.Health;
        public Modules.Utils.VariableData? Character => _gameData?.Character;
        public Modules.Utils.VariableData? InventorySlotSelected => _gameData?.InventorySlotSelected;

        // Inventory from RootObject
        public System.Collections.ObjectModel.ObservableCollection<Modules.SRT.ImageItem>? InventoryImages => _gameData?.InventoryImages;
        public System.Collections.ObjectModel.ObservableCollection<Modules.SRT.ImageItem>? KeyItemImages => _gameData?.KeyItemImages;
        public System.Collections.ObjectModel.ObservableCollection<Modules.SRT.ImageItem>? ItemboxImages => _gameData?.ItemboxImages;

        // Selected Item from RootObject
        public object? InventorySlotSelectedImage => _gameData?.InventorySlotSelectedImage;
        public string InventorySlotSelectedQuantity => _gameData?.InventorySlotSelectedQuantity ?? "";

        // Stats from RootObject
        public int Deaths => _gameData?.Deaths ?? 0;
        public int Resets => _gameData?.Resets ?? 0;
        public int Hits => _gameData?.Hits ?? 0;
        public int Saves => _gameData?.Saves ?? 0;

        // Character info from RootObject
        public string CharacterName => _gameData?.CharacterName ?? "ERROR";
        public int MaxHealth => int.TryParse(_gameData?.MaxHealth, out var val) ? val : 0; // MaxHealth in RootObject is string?

        // Timer from RootObject (IGTHumanFormat is string, IGTSHumanFormat is List<string>)
        public string IGTHumanFormat => _gameData?.IGTHumanFormat ?? "00:00:00";
        public string[] IGTSHumanFormat => _gameData?.IGTSHumanFormat?.ToArray() ?? new string[] { "00:00:00", "00:00:00", "00:00:00", "00:00:00" };

        // ECG Points from RootObject
        public System.Windows.Media.PointCollection ECGPoints => _gameData?.ECGPoints ?? new System.Windows.Media.PointCollection();

        // Timer Color from RootObject
        public System.Windows.Media.Brush TimerColorBrush => _gameData?.TimerColorBrush ?? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 255));

        // Last Item Found from RootObject
        public object? LastItemFoundImage => _gameData?.LastItemFoundImage;

        // RE2 Specific from RootObject
        // SherryPicture not found in RootObject, stubbing for now
        public System.Windows.Media.ImageSource? SherryPicture => null;
        // SherryPose not found under that name, assuming PartnerPose
        public string SherryPose => _gameData?.PartnerPose?.Value.ToString() ?? "";

        public int PartnerHPValue => _gameData?.PartnerHP?.Value ?? 0;
        public int PartnerMaxHPValue => _gameData?.PartnerMaxHPValue ?? 0; // Using int property

        // Visibility from RootObject
        public System.Windows.Visibility PartnerVisibility => _gameData?.PartnerVisibility ?? System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility SherryVisibility => _gameData?.SherryVisibility ?? System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility KeyItemsVisibility => _gameData?.KeyItemsVisibility ?? System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility ItemBoxVisibility => _gameData?.ItemBoxVisibility ?? System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility StatsVisibility => _gameData?.StatsVisibility ?? System.Windows.Visibility.Visible;
        public System.Windows.Visibility SegsVisibility => _gameData?.SegsVisibility ?? System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility DebugModeVisibility => _gameData?.DebugModeVisibility ?? System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility HealthBarVisibility => _gameData?.HealthBarVisibility ?? System.Windows.Visibility.Visible;
        public System.Windows.Visibility StandardHealthVisibility => _gameData?.StandardHealthVisibility ?? System.Windows.Visibility.Visible;
        public System.Windows.Visibility ECGHealthVisibility => _gameData?.ECGHealthVisibility ?? System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility LastItemSeenVisibility => _gameData?.LastItemSeenVisibility ?? System.Windows.Visibility.Visible;
        public System.Windows.Visibility HitVisibility => _gameData?.HitVisibility ?? System.Windows.Visibility.Collapsed;

        // View Configuration
        private double _windowWidth;
        public double WindowWidth
        {
            get => _windowWidth;
            set => SetField(ref _windowWidth, value);
        }

        private double _baseScale = 1.0;
        private double _userScale = 1.0;
        public double? WindowScale
        {
            get => _baseScale * _userScale;
            set
            {
                // If setting directly, assume it's setting UserScale relative to current BaseScale
                // But typically we should just control components. For compatibility:
                if (value.HasValue)
                {
                    _userScale = value.Value / _baseScale;
                    OnPropertyChanged(nameof(WindowScale));
                }
            }
        }

        // Streamer Mode properties
        private bool _streamerMode = false;
        public bool StreamerMode
        {
            get => _streamerMode;
            set
            {
                if (SetField(ref _streamerMode, value))
                {
                    UpdateBackgroundBrush();
                }
            }
        }

        private string _chromaKeyColor = "#00FF00";
        public string ChromaKeyColor
        {
            get => _chromaKeyColor;
            set
            {
                if (SetField(ref _chromaKeyColor, value))
                {
                    UpdateBackgroundBrush();
                }
            }
        }

        private bool _transparentMode = false;
        public bool TransparentMode
        {
            get => _transparentMode;
            set
            {
                if (SetField(ref _transparentMode, value))
                {
                    UpdateBackgroundBrush();
                }
            }
        }

        private string? _customBackgroundPath;
        private string? _customBackgroundColor;
        private double _customBackgroundOpacity = 1.0;

        private System.Windows.Media.Brush _backgroundBrush = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(0x1E, 0x1E, 0x1E)); // Default dark background #1E1E1E
        public System.Windows.Media.Brush BackgroundBrush
        {
            get => _backgroundBrush;
            set => SetField(ref _backgroundBrush, value);
        }

        // Item slot brushes from RootObject
        public System.Windows.Media.Brush? ItemSlotBrush => _gameData?.ItemSlotBrush;
        public System.Windows.Media.Brush? ItemSlotBorderBrush => _gameData?.ItemSlotBorderBrush;

        private bool _minimalItemDisplay = false;
        public bool MinimalItemDisplay
        {
            get => _minimalItemDisplay;
            set => SetField(ref _minimalItemDisplay, value);
        }

        private bool _ecgHealthDisplay = false;
        public bool ECGHealthDisplay
        {
            get => _ecgHealthDisplay;
            set
            {
                if (SetField(ref _ecgHealthDisplay, value) && _gameData != null)
                {
                    _gameData.ECGHealthDisplay = value;
                }
            }
        }

        private void UpdateBackgroundBrush()
        {
            // Transparent mode takes priority
            if (_transparentMode)
            {
                BackgroundBrush = System.Windows.Media.Brushes.Transparent;
                return;
            }

            // Streamer/chroma key mode
            if (_streamerMode)
            {
                try
                {
                    BackgroundBrush = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_chromaKeyColor));
                    return;
                }
                catch { }
            }

            // Custom Background Image
            if (!string.IsNullOrEmpty(_customBackgroundPath) && System.IO.File.Exists(_customBackgroundPath))
            {
                try
                {
                    var img = new System.Windows.Media.Imaging.BitmapImage();
                    img.BeginInit();
                    img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    img.UriSource = new System.Uri(_customBackgroundPath, System.UriKind.Absolute);
                    img.EndInit();
                    img.Freeze();

                    var brush = new System.Windows.Media.ImageBrush(img);
                    brush.Opacity = _customBackgroundOpacity;
                    brush.Stretch = System.Windows.Media.Stretch.UniformToFill;
                    brush.TileMode = System.Windows.Media.TileMode.None;
                    brush.AlignmentX = System.Windows.Media.AlignmentX.Center;
                    brush.AlignmentY = System.Windows.Media.AlignmentY.Center;
                    brush.Freeze();
                    BackgroundBrush = brush;
                    return;
                }
                catch { }
            }

            // Custom Background Color
            if (!string.IsNullOrEmpty(_customBackgroundColor))
            {
                try
                {
                    var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_customBackgroundColor);
                    BackgroundBrush = new System.Windows.Media.SolidColorBrush(color);
                    return;
                }
                catch { }
            }

            // Default dark background
            BackgroundBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x1E, 0x1E, 0x1E));
        }

        public SRTViewModel(IGameStateService gameStateService, ITimerService timerService, IInventoryService inventoryService)
        {
            _gameStateService = gameStateService;
            _timerService = timerService;
            _inventoryService = inventoryService;
        }

        public void SetGameData(RootObject gameData)
        {
            GameData = gameData;

            // Match Tracker logic for window width and scale
            WindowWidth = gameData.WindowWidth == 400 ? 400 : 320;
            _baseScale = gameData.WindowWidth == 400 ? 0.75 : 0.7;
            OnPropertyChanged(nameof(WindowScale));
        }

        private void GameData_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Forward all RootObject property changes
            switch (e.PropertyName)
            {
                case nameof(RootObject.WindowWidth):
                    WindowWidth = _gameData.WindowWidth == 400 ? 400 : 320;
                    _baseScale = _gameData.WindowWidth == 400 ? 0.75 : 0.7;
                    OnPropertyChanged(nameof(WindowScale));
                    break;
                case nameof(RootObject.Health):
                    OnPropertyChanged(nameof(Health));
                    break;
                case nameof(RootObject.Character):
                    OnPropertyChanged(nameof(Character));
                    OnPropertyChanged(nameof(CharacterName));
                    break;
                case nameof(RootObject.InventorySlotSelected):
                    OnPropertyChanged(nameof(InventorySlotSelected));
                    OnPropertyChanged(nameof(InventorySlotSelectedImage));
                    OnPropertyChanged(nameof(InventorySlotSelectedQuantity));
                    break;
                case nameof(RootObject.InventoryImages):
                    OnPropertyChanged(nameof(InventoryImages));
                    break;
                case nameof(RootObject.KeyItemImages):
                    OnPropertyChanged(nameof(KeyItemImages));
                    break;
                case nameof(RootObject.ItemboxImages):
                    OnPropertyChanged(nameof(ItemboxImages));
                    break;
                case nameof(RootObject.Deaths):
                    OnPropertyChanged(nameof(Deaths));
                    break;
                case nameof(RootObject.Saves):
                    OnPropertyChanged(nameof(Saves));
                    break;
                case nameof(RootObject.Resets):
                    OnPropertyChanged(nameof(Resets));
                    break;
                case nameof(RootObject.Hits):
                    OnPropertyChanged(nameof(Hits));
                    break;
                case nameof(RootObject.IGTHumanFormat):
                    OnPropertyChanged(nameof(IGTHumanFormat));
                    break;
                case nameof(RootObject.IGTSHumanFormat):
                    OnPropertyChanged(nameof(IGTSHumanFormat));
                    break;
                case nameof(RootObject.ECGPoints):
                    OnPropertyChanged(nameof(ECGPoints));
                    break;
                case nameof(RootObject.LastItemFoundImage):
                    OnPropertyChanged(nameof(LastItemFoundImage));
                    break;
                case nameof(RootObject.InventorySlotSelectedImage):
                    OnPropertyChanged(nameof(InventorySlotSelectedImage));
                    break;
                case nameof(RootObject.InventorySlotSelectedQuantity):
                    OnPropertyChanged(nameof(InventorySlotSelectedQuantity));
                    break;
                case nameof(RootObject.CharacterName):
                    OnPropertyChanged(nameof(CharacterName));
                    break;
                case nameof(RootObject.MaxHealth):
                    OnPropertyChanged(nameof(MaxHealth));
                    break;
                // case nameof(RootObject.SherryPicture): 
                //    OnPropertyChanged(nameof(SherryPicture));
                //    break;
                case nameof(RootObject.PartnerPose): // Changed from SherryPose
                    OnPropertyChanged(nameof(SherryPose));
                    break;
                case nameof(RootObject.PartnerHP): // Changed from Partner
                    OnPropertyChanged(nameof(PartnerHPValue));
                    break;
                case nameof(RootObject.PartnerMaxHPValue): // Changed from PartnerMaxHP
                    OnPropertyChanged(nameof(PartnerMaxHPValue));
                    break;
                case nameof(RootObject.ItemSlotBrush):
                    OnPropertyChanged(nameof(ItemSlotBrush));
                    break;
                case nameof(RootObject.ItemSlotBorderBrush):
                    OnPropertyChanged(nameof(ItemSlotBorderBrush));
                    break;
                default:
                    // For visibility and other properties, just notify
                    OnPropertyChanged(e.PropertyName);
                    break;
            }
        }

        public void UpdateConfiguration(Dictionary<string, bool?> config, MainWindowViewModel mainVM)
        {
            // Window Aesthetics from Main ViewModel
            if (mainVM.OverlayScale > 0)
            {
                _userScale = mainVM.OverlayScale;
            }
            // WindowWidth = 400; // Controlled by GameData now

            // Background configuration
            _customBackgroundPath = mainVM.CustomBackgroundPath;
            _customBackgroundColor = mainVM.CustomBackgroundColor;
            _customBackgroundOpacity = mainVM.CustomBackgroundOpacity;

            // Update background brush based on current settings
            UpdateBackgroundBrush();

            // Trigger PropertyChanges
            OnPropertyChanged(nameof(WindowScale));
            OnPropertyChanged(nameof(WindowWidth));
        }
    }
}
