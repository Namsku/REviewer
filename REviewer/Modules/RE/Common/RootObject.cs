using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using REviewer.Modules.RE.Enemies;
using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using REviewer.Core.Memory;
using REviewer.Services.Game;
using REviewer.Services.Timer;
using REviewer.Services.Inventory;
using REviewer.Core.Constants;
using REviewer.Core.Configuration;
using System.Windows.Media;

namespace REviewer.Modules.RE.Common
{
    public partial class RootObject : INotifyPropertyChanged
    {
        public class KRoom
        {
            public List<string>? KeyRooms { get; set; }
        }

        public Bio Bio { get; set; }

        public Dictionary<string, int> MAX_STAGE_ID = new Dictionary<string, int>
        {
            { "Bio", 0x05 },
            { "bio", 0x05 },
            { "Biohazard", 0x05 },
            { "biohazard", 0x05 },
            { "bio2", 0x07 },
            { "bio2 1.10", 0x07 },
            { "Bio2 1.10", 0x07 },
            { "Bio2 1.1", 0x07 },
            { "Bio2 v1.1", 0x07 },
            { "re2mm", 0x07 },
            { "RE2MM", 0x07 },
            { "REVisited", 0x07 },
            { "UEv3", 0x07 },
            { "toos_ver2_0_0", 0x07 },
            { "toos(HARD)_ver2_0_0", 0x07},
            { "bunny", 0x07 },
            { "bunny2", 0x07 },
            { "CLAIRE", 0x07 },
            { "LEON", 0x07 },
            { "Leon", 0x07 },
            { "bio2 chn claire", 0x07 },
            { "bio2 chn leon", 0x07 },
            { "Bio2 chn claire", 0x07 },
            { "Bio2 chn leon", 0x07 },
            { "Irregular1.8", 0x07 },
            { "BIOHAZARD(R) 3 PC", 0x05 },
            { "biohazard(r) 3 pc", 0x05 },
            { "Bio3 CHN/TWN", 0x05 },
            { "CVX PS2 US", 0x09 },
        };

        private static readonly string[] ITEM_TYPES = new string[] { "Key Item", "Optionnal Key Item", "Nothing" };

        private bool disposed = false;

        public int MAX_INVENTORY_SIZE;
        public int SELECTED_GAME;
        public int SaveID;
        public int CurrentSaveID;
        public Bio? _bio;
        private int _virtualMemoryPointer;
        public List<KeyItem>? KeyItems;
        public ItemIDs IDatabase;
        public event PropertyChangedEventHandler? PropertyChanged;
        public string? FullRoomName;
        public string? LastRoomName;
        public FileSystemWatcher? Watcher;
        public List<JObject>? SaveDatabase;
        public IMemoryMonitor? Monitoring;

        public bool OneHP = false;
        public bool NoDamage = false;
        public bool NoItemBox = false;
        public bool StaticEnemyTrackerWindow = false;

        private Enemy _selectedEnemy = new Enemy();
        public Enemy SelectedEnemy
        {
            get => _selectedEnemy;
            set
            {
                if (_selectedEnemy != value)
                {
                    _selectedEnemy = value;
                    OnPropertyChanged(nameof(SelectedEnemy));
                }
            }
        }

        public Enemy SelectedHiddenEnemy = new Enemy();

        public Dictionary<string, List<string>>? KeyRooms { get; set; }
        public Visibility? HealthBarVisibility { get; set; }
        public Visibility? BiorandVisibility { get; set; }
        public Visibility? ItemBoxVisibility { get; set; }
        public Visibility? SherryVisibility { get; set; }
        public Visibility? PartnerVisibility { get; set; }
        public Visibility? StatsVisibility { get; set; }
        public Visibility? SegsVisibility { get; set; }
        public Visibility? KeyItemsVisibility { get; set; }
        public Visibility? LastItemSeenVisibility { get; set; }
        public Visibility? DebugModeVisibility { get; set; }


        private int? _windowWidth;
        public int? WindowWidth
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
        public double? WindowScale { get; set; }
        public double? WindowCenter { get; set; }

        // Streamer Mode properties for OBS chroma key support
        private bool _streamerMode = false;
        public bool StreamerMode
        {
            get => _streamerMode;
            set
            {
                if (_streamerMode != value)
                {
                    _streamerMode = value;
                    OnPropertyChanged(nameof(StreamerMode));
                    OnPropertyChanged(nameof(BackgroundBrush));
                }
            }
        }

        private string _chromaKeyColor = "#00FF00";
        public string ChromaKeyColor
        {
            get => _chromaKeyColor;
            set
            {
                if (_chromaKeyColor != value)
                {
                    _chromaKeyColor = value;
                    OnPropertyChanged(nameof(ChromaKeyColor));
                    OnPropertyChanged(nameof(BackgroundBrush));
                }
            }
        }

        // Transparent Background Mode
        private bool _transparentMode = false;
        public bool TransparentMode
        {
            get => _transparentMode;
            set
            {
                if (_transparentMode != value)
                {
                    _transparentMode = value;
                    OnPropertyChanged(nameof(TransparentMode));
                    OnPropertyChanged(nameof(BackgroundBrush));
                }
            }
        }

        // Minimal Item Display Mode (hides empty item box slots)
        private bool _minimalItemDisplay = false;
        public bool MinimalItemDisplay
        {
            get => _minimalItemDisplay;
            set
            {
                if (_minimalItemDisplay != value)
                {
                    _minimalItemDisplay = value;
                    OnPropertyChanged(nameof(MinimalItemDisplay));
                    OnPropertyChanged(nameof(ItemSlotBrush));
                    OnPropertyChanged(nameof(ItemSlotBorderBrush));
                }
            }
        }

        // Pulse Speed for Heartbeat Animation
        private double _pulseSpeed = 1.0;
        public double PulseSpeed
        {
            get => _pulseSpeed;
            set
            {
                if (_pulseSpeed != value)
                {
                    _pulseSpeed = value;
                    OnPropertyChanged(nameof(PulseSpeed));
                }
            }
        }

        // ECG Health Display Mode
        private bool _ecgHealthDisplay = false;
        public bool ECGHealthDisplay
        {
            get => _ecgHealthDisplay;
            set
            {
                if (_ecgHealthDisplay != value)
                {
                    _ecgHealthDisplay = value;
                    OnPropertyChanged(nameof(ECGHealthDisplay));
                    OnPropertyChanged(nameof(StandardHealthVisibility));
                    OnPropertyChanged(nameof(ECGHealthVisibility));
                }
            }
        }
        
        // Helper visibility properties
        public Visibility StandardHealthVisibility => !_ecgHealthDisplay ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ECGHealthVisibility => _ecgHealthDisplay ? Visibility.Visible : Visibility.Collapsed;

        // Custom Background Properties
        private string? _customBackgroundPath;
        public string? CustomBackgroundPath
        {
            get => _customBackgroundPath;
            set
            {
                if (_customBackgroundPath != value)
                {
                    _customBackgroundPath = value;
                    OnPropertyChanged(nameof(CustomBackgroundPath));
                    OnPropertyChanged(nameof(BackgroundBrush));
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
                    OnPropertyChanged(nameof(CustomBackgroundColor));
                    OnPropertyChanged(nameof(BackgroundBrush));
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
                    OnPropertyChanged(nameof(CustomBackgroundOpacity));
                    OnPropertyChanged(nameof(BackgroundBrush));
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
                    OnPropertyChanged(nameof(TimerColor));
                    OnPropertyChanged(nameof(TimerColorBrush));
                }
            }
        }
        
        public System.Windows.Media.Brush TimerColorBrush
        {
             get
             {
                 if (!string.IsNullOrEmpty(_timerColor))
                 {
                     try
                     {
                         var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_timerColor);
                         return new System.Windows.Media.SolidColorBrush(color);
                     }
                     catch { }
                 }
                 // Default Cyan/Teal
                 return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 255)); 
             }
        }

        private System.Windows.Media.PointCollection _ecgPoints = new System.Windows.Media.PointCollection();
        public System.Windows.Media.PointCollection ECGPoints
        {
            get => _ecgPoints;
            set
            {
                if (_ecgPoints != value)
                {
                    _ecgPoints = value;
                    OnPropertyChanged(nameof(ECGPoints));
                }
            }
        }

        // Computed property for background - returns transparent, chroma key, custom, or default dark
        public System.Windows.Media.Brush BackgroundBrush
        {
            get
            {
                // Transparent mode takes priority
                if (_transparentMode)
                {
                    return System.Windows.Media.Brushes.Transparent;
                }
                
                // Streamer/chroma key mode
                if (_streamerMode)
                {
                    try
                    {
                        return new System.Windows.Media.SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_chromaKeyColor));
                    }
                    catch { }
                }

                // Custom Background Image
                if (!string.IsNullOrEmpty(_customBackgroundPath) && File.Exists(_customBackgroundPath))
                {
                    try
                    {
                        var img = new System.Windows.Media.Imaging.BitmapImage();
                        img.BeginInit();
                        img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        img.UriSource = new Uri(_customBackgroundPath, UriKind.Absolute);
                        img.EndInit();
                        img.Freeze();

                        var brush = new System.Windows.Media.ImageBrush(img);
                        brush.Opacity = _customBackgroundOpacity;
                        brush.Stretch = System.Windows.Media.Stretch.UniformToFill;
                        brush.TileMode = System.Windows.Media.TileMode.None;
                        brush.AlignmentX = System.Windows.Media.AlignmentX.Center;
                        brush.AlignmentY = System.Windows.Media.AlignmentY.Center;
                        brush.Freeze();
                        return brush;
                    }
                    catch { }
                }

                // Custom Background Color
                if (!string.IsNullOrEmpty(_customBackgroundColor))
                {
                    try
                    {
                        var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_customBackgroundColor);
                        var brush = new System.Windows.Media.SolidColorBrush(color);
                        brush.Opacity = _customBackgroundOpacity;
                        brush.Freeze();
                        return brush;
                    }
                    catch { }
                }
                
                // Default dark background
                return new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1E1E1E"));
            }
        }

        // Computed property for item slot background
        public System.Windows.Media.Brush ItemSlotBrush
        {
            get
            {
                if (_minimalItemDisplay)
                {
                    return BackgroundBrush; // Match the window background
                }
                return new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2D2D30"));
            }
        }

        // Computed property for item slot border
        public System.Windows.Media.Brush ItemSlotBorderBrush
        {
            get
            {
                if (_minimalItemDisplay)
                {
                    return System.Windows.Media.Brushes.Transparent;
                }
                return new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3C3C3C"));
            }
        }
        public RootObject(Bio bio, ItemIDs ids, int virtualMemoryPointer)
        {
            if (bio.Player?.Character?.Database == null)
                throw new ArgumentNullException(nameof(bio));

            _bio = bio;
            _virtualMemoryPointer = virtualMemoryPointer;
            IDatabase = ids;

            PrivateImageItem.Text = "";
            PrivateImageItem.TextVisibility = Visibility.Hidden;

            SelectedHiddenEnemy.MaxHealth = 0;
            SelectedHiddenEnemy.CurrentHealth = 0;
            SelectedHiddenEnemy.Name = "DEBUG";
            SelectedHiddenEnemy.Visibility = Visibility.Collapsed;

            LastItemSeenVisibility = Visibility.Visible;
            PartnerVisibility = Visibility.Visible;

            var processName = IDatabase.GetProcessName().ToLower();

            if (processName == "bio" || processName == "biohazard")
            {
                SELECTED_GAME = 100;
                PartnerVisibility = Visibility.Collapsed;
            }
            else if (processName == "bio2 1.10" || processName == "bio2 chn leon" || processName == "bio2 chn claire" || processName == "claire" || processName == "leon")
            {
                SELECTED_GAME = 200;
                HitVisibility = Visibility.Visible;
            }
            else if (processName == "biohazard(r) 3 pc" || processName == "bio3 chn/twn")
            {
                SELECTED_GAME = 300;
            }
            else if (processName == "cvx ps2 us")
            {
                SELECTED_GAME = 400;
                PartnerVisibility = Visibility.Collapsed;
                // LastItemSeenVisibility = Visibility.Collapsed;
            }

            InitMaxInventoryCapacity(bio.Player.Character.Database[0]);
            InitKeyRooms();

            // Game
            GameState = GetVariableData("GameState", bio.Game.State);
            GameSystem = GetVariableData("GameSystem", bio.Game.System);
            GameFramePointer = GetVariableData("GameFramePointer", bio.Game.FramePointer);
            Unk001 = GetVariableData("GameUnk001", bio.Game.Unk001);
            GameTimer = GetVariableData("GameTimer", bio.Game.Timer);
            GameRetry = GetVariableData("GameRetry", bio.Game.Retry);
            GameFrame = GetVariableData("GameFrame", bio.Game.Frame);
            GameSave = GetVariableData("GameSave", bio.Game.Save);
            MainMenu = GetVariableData("MainMenu", bio.Game.MainMenu);
            SaveContent = GetVariableData("SaveContent", bio.Game.SaveContent);

            // Player
            Character = GetVariableData("Character", bio.Player.Character);
            InventorySlotSelected = GetVariableData("InventorySlotSelected", bio.Player.InventorySlotSelected);
            Stage = GetVariableData("Stage", bio.Player.Stage);
            Room = GetVariableData("Room", bio.Player.Room);
            Cutscene = GetVariableData("Cutscene", bio.Player.Cutscene);
            LastCutscene = GetVariableData("LastCutscene", bio.Player.LastCutscene);
            LastRoom = GetVariableData("LastRoom", bio.Player.LastRoom);
            Unk002 = GetVariableData("GameUnk001", bio.Player.Unk001);
            Event = GetVariableData("Event", bio.Player.Event);
            LastItemFound = GetVariableData("LastItemFound", bio.Player.LastItemFound);
            InventoryCapacityUsed = GetVariableData("InventoryCapacityUsed", bio.Player.InventoryCapacityUsed);
            CharacterHealthState = GetVariableData("CharacterHealthState", bio.Player.CharacterHealthState);
            Health = GetVariableData("CharacterHealth", bio.Player.Health);
            CharacterMaxHealth = GetVariableData("CharacterMaxHealth", bio.Player.CharacterMaxHealth);
            LockPick = GetVariableData("LockPick", bio.Player.LockPick);
            PartnerPointer = GetVariableData("PartnerPointer", bio.Player.PartnerPointer);
            ItemBoxState = GetVariableData("ItemBoxState", bio.Player.ItemBoxState);
            HitFlag = GetVariableData("HitFlag", bio.Player.HitFlag);

            // Carlos RE3
            CarlosInventorySlotSelected = GetVariableData("CarlosInventorySlotSelected", bio.Player.CarlosInventorySlotSelected);
            CarlosLastItemFound = GetVariableData("CarlosLastItemSeen", bio.Player.CarlosLastItemFound);

            // Position
            PositionX = GetVariableData("PositionX", bio.Position.X);
            PositionY = GetVariableData("PositionY", bio.Position.Y);
            PositionZ = GetVariableData("PositionZ", bio.Position.Z);
            PositionR = GetVariableData("PositionR", bio.Position.R);

            // Rebirth
            // RebirthDebug = GetVariableData("RebirthDebug", bio.Rebirth.Debug);
            // RebirthScreen = GetVariableData("RebirthScreen", bio.Rebirth.Screen);
            // RebirthState = GetVariableData("RebirthState", bio.Rebirth.State);

            // ItemBox and Inventory
            InitInventory(bio);
            InitItemBox(bio);

            // Initialize ECG Pattern
            UpdateECGPattern(0);

            // Timers
            InitTimers();

            // Stats
            InitStats();

            // File Watcher
            // if (processName != "bio2 chn claire" && processName != "bio2 chn leon" && processName != "bio3 chn/twn")
            // {
            //    InitFileWatcher();
            // }

            // Init Save Database
            InitSaveDatabase();


            VariableData GetVariableData(String key, dynamic value)
            {
                if (bio.Offsets == null) throw new ArgumentNullException(nameof(bio));

                if (bio.Offsets.TryGetValue(key, out string? offset))
                {
                    return new VariableData(Library.HexToInt(offset) + virtualMemoryPointer, value);
                }
                else
                {
                    return null;
                }
            }
        }

        public void SetMonitoring(IMemoryMonitor monitor)
        {
            Monitoring = monitor;
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void InitUIConfig(Dictionary<string, bool?> config)
        {
            if (config.TryGetValue("ChrisInventory", out bool? chrisInventory))
            {
                ChrisInventoryHotfix = chrisInventory == true;
                if (ChrisInventoryHotfix)
                {
                    UpdateInventoryCapacity();
                }
            }

            if (config.TryGetValue("Minimalist", out bool? minimalist))
            {
                if (minimalist == true)
                {
                    WindowWidth = 320;
                    WindowScale = 0.47;
                    WindowCenter = 0.2;
                }
                else
                {
                    WindowWidth = 400;
                    WindowScale = 0.7;
                    WindowCenter = 0.5;
                }
            }

            OneHP = config.TryGetValue("OneHP", out bool? oneHP) && oneHP == true;

            if (OneHP)
            {
                Monitoring.WriteVariableData(Health, 1);
            }

            NoDamage = config.TryGetValue("NoDamage", out bool? noDamage) && noDamage == true;
            NoItemBox = config.TryGetValue("NoItemBox", out bool? noItemBox) && noItemBox == true;
            StaticEnemyTrackerWindow = config.TryGetValue("StaticEnemyTrackerWindow", out bool? staticEnemyTrackerWindow) && staticEnemyTrackerWindow == true;
            isDDraw100 = config.TryGetValue("Ddraw100", out bool? ddraw100) && ddraw100 == true;


            HealthBarVisibility = GetVisibility(config, "HealthBar");
            BiorandVisibility = GetVisibility(config, "Standard");
            ItemBoxVisibility = GetVisibility(config, "ItemBox");
            SherryVisibility = GetVisibility(config, "Sherry");
            DebugModeVisibility = GetVisibility(config, "DebugMode");

            StatsVisibility = GetVisibility(config, "NoStats") == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            SegsVisibility = GetVisibility(config, "NoSegTimers") == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            KeyItemsVisibility = GetVisibility(config, "NoKeyItems") == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            if (BiorandVisibility == Visibility.Visible)
            {
                StatsVisibility = StatsVisibility == Visibility.Collapsed ? Visibility.Collapsed : Visibility.Visible;
                SegsVisibility = SegsVisibility == Visibility.Collapsed ? Visibility.Collapsed : Visibility.Visible;
                KeyItemsVisibility = KeyItemsVisibility == Visibility.Collapsed ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                StatsVisibility = Visibility.Collapsed;
                SegsVisibility = Visibility.Collapsed;
                KeyItemsVisibility = Visibility.Collapsed;
            }
        }
        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F9)
            {
                // Do something when F9 is pressed
                MessageBox.Show("F9 key pressed");
            }
        }
        private Visibility GetVisibility(Dictionary<string, bool?> config, string key)
        {
            if (config.TryGetValue(key, out bool? value))
            {
                return value == true ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed; // Default value if key not found or value is null
        }

        public void InitMaxInventoryCapacity(string character)
        {
            if (character == "Chris")
            {
                MAX_INVENTORY_SIZE = 8;
            }
            else if (character == "Sherry" || character == "Claire" || character == "Leon" || character == "Ada")
            {
                MAX_INVENTORY_SIZE = 11;
            }
            else
            {
                MAX_INVENTORY_SIZE = 10;
            }
        }

        public void InitKeyRooms()
        {
            var reDataPath = ConfigurationManager.AppSettings["REdata"];
            if (reDataPath == null)
            {
                // Handle missing REdata path gracefully
                // Log error or provide default behavior
                return;
            }

            var json = File.ReadAllText(reDataPath);
            var processName = IDatabase.GetProcessName();
            if (processName == null)
            {
                // Handle missing process name gracefully
                // Log error or provide default behavior
                return;
            }

            var bios = JsonConvert.DeserializeObject<Dictionary<string, KRoom>>(json);
            if (bios == null || !bios.TryGetValue(processName, out var bio) || bio.KeyRooms == null)
            {
                // Handle missing or invalid data gracefully
                // Log error or provide default behavior
                return;
            }

            // Use lazy loading instead of eagerly creating a new dictionary
            KeyRooms = new Dictionary<string, List<string>>(bio.KeyRooms.Count);
            foreach (var room in bio.KeyRooms)
            {
                KeyRooms.Add(room, new List<string>());
            }
        }


        public void InitFileWatcher()
        {
            DisposeFileSystemWatcher(); // Ensure previous watcher is disposed

            if (Watcher == null) // Avoid creating multiple watchers
            {
                var processName = IDatabase.GetProcessName();
                var savePath = Library.GetSavePath(processName ?? "UNKNOWN GAME PROCESS ERROR");

                if (string.IsNullOrEmpty(savePath) || !Directory.Exists(savePath))
                {
                    //Logger.Instance.Warning($"Save path not configured or doesn't exist for {processName}, file watcher disabled");
                    return;
                }

                Watcher = new FileSystemWatcher();
                Watcher.Path = savePath;
                Watcher.Created += SaveFileDetected;
                Watcher.EnableRaisingEvents = true;
            }
        }

        private void DisposeFileSystemWatcher()
        {
            if (Watcher != null)
            {
                Watcher.EnableRaisingEvents = false;
                Watcher.Created -= SaveFileDetected;
                Watcher.Dispose();
                Watcher = null;
            }
        }

        private void SaveState()
        {
            if (KeyItems == null) throw new ArgumentNullException(nameof(KeyItems));

            JObject jsonObject = new JObject
            {
                ["GameState"] = GameState?.Value,
                ["GameTimer"] = GameTimer?.Value,
                ["MainMenu"] = MainMenu?.Value,
                ["SaveContent"] = SaveContent?.Value,
                ["Character"] = Character?.Value,
                ["InventorySlotSelected"] = InventorySlotSelected?.Value,
                ["Stage"] = Stage?.Value,
                ["Room"] = Room?.Value,
                ["Cutscene"] = Cutscene?.Value,
                ["LastRoom"] = LastRoom?.Value,
                ["Unk002"] = Unk002?.Value,
                ["Event"] = Event?.Value,
                ["LastItemFound"] = LastItemFound?.Value,
                ["InventoryCapacityUsed"] = InventoryCapacityUsed?.Value,
                ["CharacterHealthState"] = CharacterHealthState?.Value,
                ["CarlosInventorySlotSelected"] = CarlosInventorySlotSelected?.Value,
                ["Health"] = Health?.Value,
                ["LockPick"] = LockPick?.Value,
                ["PositionX"] = PositionX?.Value,
                ["PositionY"] = PositionY?.Value,
                ["PositionZ"] = PositionZ?.Value,
                ["SaveID"] = SaveID,

                // SRT Specific Stats
                ["Deaths"] = Deaths,
                ["Resets"] = Resets,
                ["Saves"] = Saves,
                ["Kills"] = Kills,
                ["Shots"] = Shots,
                ["Hits"] = Hits,
                ["RoomsVisited"] = RoomsVisited,
                ["Debug"] = Debug,
                ["SegmentCount"] = SegmentCount,
                ["Segments"] = JArray.FromObject(IGTSegments ?? new List<int>())
            };

            // Serialize only necessary data
            jsonObject["KeyItems"] = JArray.FromObject(KeyItems.Select(item => item.State).ToList());
            jsonObject["KeyRooms"] = JObject.FromObject(KeyRooms ?? new Dictionary<string, List<string>>());

            GenerateJsonSave(jsonObject);
        }

        public static void GenerateJsonSave(JObject objectData)
        {
            var directoryPath = "saves/";

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string jsonString = objectData.ToString();

            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(jsonString));
            string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            // Append a unique identifier to the filename
            string filePath = Path.Combine(directoryPath, hashString + "_" + Guid.NewGuid().ToString() + ".json");
            File.WriteAllText(filePath, jsonString);
            Logger.Instance.Info($"Saved SRT state");
        }

        private void SaveFileDetected(object source, FileSystemEventArgs e)
        {
            Saves += 1;

            CurrentSaveID += 1;
            SaveID = CurrentSaveID;
            byte[] bytes = BitConverter.GetBytes(CurrentSaveID);

            // Open the created file and modify the first 4 bytes
            FileStream fs = null;
            int attempts = 0;
            while (fs == null && attempts < 10)
            {
                try
                {
                    fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.ReadWrite);
                }
                catch (IOException)
                {
                    attempts++;
                    Thread.Sleep(1000); // Wait for the file to be available
                }
            }

            if (fs != null)
            {
                using (fs)
                {
                    fs.Seek(0x10, SeekOrigin.Begin);
                    fs.WriteByte(0xDE);
                    fs.WriteByte(0xAD);
                    fs.WriteByte(bytes[1]);
                    fs.WriteByte(bytes[0]);
                }

                // Saving the SRT state on a binary file (Serialized object)
                SaveState();

                OnPropertyChanged(nameof(Saves));
            }
            else
            {
                Logger.Instance.Error($"Failed to open file -> {e.Name} after 10 attempts");
            }
        }

        private void LoadState(int value)
        {
            var save = SaveDatabase?.FirstOrDefault(s => s["SaveID"]?.Value<int>() == value);

            if (save == null) return;
            if (IGTSegments == null) return;

            // Restore Segments
            var savedSegments = save["Segments"]?.ToObject<List<int>>();
            if (savedSegments != null)
            {
                for (int i = 0; i < Math.Min(IGTSegments.Count, savedSegments.Count); i++)
                {
                    IGTSegments[i] = savedSegments[i];
                }
            }

            SegmentCount = save["SegmentCount"]?.Value<int>() ?? save["SegmentsCount"]?.Value<int>() ?? 0;

            // Restore Stats (Direct assignment to allow state reversion)
            Debug = save["Debug"]?.Value<int>() ?? 0;
            Deaths = save["Deaths"]?.Value<int>() ?? 0;
            Resets = save["Resets"]?.Value<int>() ?? 0;
            Saves = save["Saves"]?.Value<int>() ?? 0;
            Kills = save["Kills"]?.Value<int>() ?? 0;
            Shots = save["Shots"]?.Value<int>() ?? 0;
            Hits = save["Hits"]?.Value<int>() ?? 0;
            RoomsVisited = save["RoomsVisited"]?.Value<int>() ?? 0;

            KeyRooms = save["KeyRooms"]?.ToObject<Dictionary<string, List<string>>>() ?? new Dictionary<string, List<string>>();

            if (KeyItems != null)
            {
                for (int i = 0; i < KeyItems.Count; i++)
                {
                    KeyItems[i].State = save["KeyItems"]?[i]?.Value<int>() ?? 0;
                    UpdatePictureKeyItemState(i);
                }
            }

            // Force UI update for human readable formats
            OnPropertyChanged(nameof(IGTSHumanFormat));
            OnPropertyChanged(nameof(IGTHumanFormat));
        }

        private void InitSaveDatabase()
        {
            SaveDatabase = new List<JObject>();
            CurrentSaveID = 0;

            var directoryPath = "saves/";

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                return;
            }

            var files = Directory.EnumerateFiles(directoryPath, "*.json"); // Use EnumerateFiles for better performance

            foreach (var file in files)
            {
                var jsonString = File.ReadAllText(file);
                var jsonObject = JObject.Parse(jsonString);
                SaveDatabase.Add(jsonObject);
                CurrentSaveID = Math.Max(CurrentSaveID, jsonObject["SaveID"].Value<int>());
            }
        }

        public void Dispose()
        {
            Dispose(true);

            if (GameState != null)
            {
                GameState.PropertyChanged -= GameState_PropertyChanged;
            }

            if (GameTimer != null)
            {
                GameTimer.PropertyChanged -= Timer_PropertyChanged;
            }

            if (GameFrame != null)
            {
                GameFrame.PropertyChanged -= Frame_PropertyChanged;
            }

            if (MainMenu != null)
            {
                MainMenu.PropertyChanged -= MainMenu_PropertyChanged;
            }

            if (SaveContent != null)
            {
                SaveContent.PropertyChanged -= SaveContent_PropertyChanged;
            }

            DisposeInventory();
            DisposeItemBox();

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    DisposeFileSystemWatcher();
                }

                // Dispose unmanaged resources

                disposed = true;
            }
        }

    }
}

