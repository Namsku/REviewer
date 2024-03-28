using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public partial class RootObject : INotifyPropertyChanged
    {
        public class KRoom
        {
            public List<string>? KeyRooms { get; set; }
        }

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
            { "BIOHAZARD(R) 3 PC", 0x05 },
            { "biohazard(r) 3 pc", 0x05 },
        };

        private static readonly string[] ITEM_TYPES = new string[] { "Key Item", "Optionnal Key Item", "Nothing" };

        private bool disposed = false;

        public int MAX_INVENTORY_SIZE;
        public int SELECTED_GAME;
        public int SaveID;
        public int CurrentSaveID;
        public Bio? _bio;

        public List<KeyItem>? KeyItems;
        public ItemIDs IDatabase; 
        public event PropertyChangedEventHandler? PropertyChanged;
        public string? FullRoomName;
        public string? LastRoomName;
        public FileSystemWatcher? Watcher;
        public List<JObject>? SaveDatabase;
        public Dictionary<string, List<string>>? KeyRooms { get; set; }

        public Visibility? HealthBarVisibility { get; set; }
        public Visibility? BiorandVisibility { get; set; }
        public Visibility? ItemBoxVisibility { get; set; }
        public Visibility? SherryVisibility { get; set; }
        public Visibility? PartnerVisibility { get; set; }

        
        public RootObject(Bio bio, ItemIDs ids)
        {
            if (bio.Player?.Character?.Database == null)
                throw new ArgumentNullException(nameof(bio));

            _bio = bio;
            IDatabase = ids;
            InitMaxInventoryCapacity(bio.Player.Character.Database[0]);
            InitKeyRooms();

            // Game
            GameState = GetVariableData("GameState", bio.Game.State);
            Unk001 = GetVariableData("GameUnk001", bio.Game.Unk001);
            GameTimer = GetVariableData("GameTimer", bio.Game.Timer);
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
            LastRoom = GetVariableData("LastRoom", bio.Player.LastRoom);
            Unk002 = GetVariableData("GameUnk001", bio.Player.Unk001);
            Event = GetVariableData("Event", bio.Player.Event);
            LastItemFound = GetVariableData("LastItemFound", bio.Player.LastItemFound);
            InventoryCapacityUsed = GetVariableData("InventoryCapacityUsed", bio.Player.InventoryCapacityUsed);
            CharacterHealthState = GetVariableData("CharacterHealthState", bio.Player.CharacterHealthState);
            Health = GetVariableData("CharacterHealth", bio.Player.Health);
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

            // Rebirth
            RebirthDebug = GetVariableData("RebirthDebug", bio.Rebirth.Debug);
            RebirthScreen = GetVariableData("RebirthScreen", bio.Rebirth.Screen);
            RebirthState = GetVariableData("RebirthState", bio.Rebirth.State);

            // ItemBox and Inventory
            InitInventory(bio);
            InitItemBox(bio);
            
            // Timers
            InitTimers();

            // Stats
            InitStats();

            // File Watcher
            InitFileWatcher();

            // Init Save Database
            InitSaveDatabase();

            var processName = IDatabase.GetProcessName().ToLower();

            if (processName == "bio" || processName == "biohazard")
            {
                SELECTED_GAME = 0;
                PartnerVisibility = Visibility.Collapsed;
            }
            else if (processName == "bio2 1.10")
            {
                SELECTED_GAME = 1;
                DebugVisibility = Visibility.Collapsed;
                HitVisibility = Visibility.Visible;
            }
            else if (processName == "biohazard(r) 3 pc")
            {
                SELECTED_GAME = 2;
            }

            VariableData GetVariableData(String key, dynamic value)
            {
                if (bio.Offsets == null) throw new ArgumentNullException(nameof(bio));

                if (bio.Offsets.TryGetValue(key, out string? offset))
                {
                    return new VariableData(Library.HexToInt(offset), value);
                }
                else
                {
                    return null;
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void InitUIConfig(Dictionary<string, bool?> config)
        {
            HealthBarVisibility = GetVisibility(config, "HealthBar");
            BiorandVisibility = GetVisibility(config, "Standard");
            ItemBoxVisibility = GetVisibility(config, "ItemBox");

            if (config.TryGetValue("ChrisInventory", out bool? chrisInventory))
            {
                ChrisInventoryHotfix = chrisInventory == true;
                if (ChrisInventoryHotfix)
                {
                    UpdateInventoryCapacity();
                }
            }

            SherryVisibility = GetVisibility(config, "Sherry");
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
            else if(character == "Sherry" || character == "Claire" || character == "Leon" || character == "Ada")
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
            Dictionary<string, string> db = Library.GetGameList();

            // Dispose of existing watcher if any
            DisposeFileSystemWatcher();

            Watcher = new FileSystemWatcher();
            var processName = IDatabase.GetProcessName();
            Watcher.Path = Library.GetSavePath(processName ?? "UNKNOWN GAME PROCESS ERROR");
            Watcher.Created += SaveFileDetected;
            Watcher.EnableRaisingEvents = true;
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
            if(KeyItems == null) throw new ArgumentNullException(nameof(KeyItems)); 

            JObject jsonObject = new JObject();

            jsonObject["GameState"] = GameState?.Value;
            jsonObject["GameTimer"] = GameTimer?.Value;
            jsonObject["MainMenu"] = MainMenu?.Value;
            jsonObject["SaveContent"] = SaveContent?.Value;
            jsonObject["Character"] = Character?.Value;
            jsonObject["InventorySlotSelected"] = InventorySlotSelected?.Value;
            jsonObject["Stage"] = Stage?.Value;
            jsonObject["Room"] = Room?.Value;
            jsonObject["Cutscene"] = Cutscene?.Value;
            jsonObject["LastRoom"] = LastRoom?.Value;
            jsonObject["Unk002"] = Unk002?.Value;
            jsonObject["Event"] = Event?.Value;
            jsonObject["LastItemFound"] = LastItemFound?.Value;
            jsonObject["InventoryCapacityUsed"] = InventoryCapacityUsed?.Value;
            jsonObject["CharacterHealthState"] = CharacterHealthState?.Value;

            jsonObject["CarlosInventorySlotSelected"] = CarlosInventorySlotSelected?.Value;

            jsonObject["Health"] = Health?.Value;
            jsonObject["LockPick"] = LockPick?.Value;
            jsonObject["PositionX"] = PositionX?.Value;
            jsonObject["PositionY"] = PositionY?.Value;
            jsonObject["PositionZ"] = PositionZ?.Value;
            jsonObject["RebirthDebug"] = RebirthDebug?.Value;
            jsonObject["RebirthScreen"] = RebirthScreen?.Value;
            jsonObject["RebirthState"] = RebirthState?.Value;
            jsonObject["SaveID"] = SaveID;

            jsonObject["Debug"] = Debug;
            jsonObject["Deaths"] = Deaths;
            jsonObject["Resets"] = Resets;
            jsonObject["Saves"] = Saves;
            jsonObject["Hits"] = Hits;

            jsonObject["SegmentsCount"] = SegmentCount;

            jsonObject["Segments"] = JArray.FromObject(IGTSegments ?? new List<int>() { 0, 0, 0, 0 });
            jsonObject["KeyItems"] = JArray.FromObject(KeyItems.Select(item => item.State).ToList());
            jsonObject["KeyRooms"] = JObject.FromObject(KeyRooms ?? new Dictionary<string, List<string>>());

            GenerateJsonSave(jsonObject);
        }

        public static void GenerateJsonSave(JObject objectData)
        {
            var directoryPath = "saves/";
            // Make an exact copy of the object

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

            if (save == null)
            {
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                IGTSegments[i] = Math.Max((int)IGTSegments[i], save["Segments"][i]?.Value<int>() ?? 0);
            }

            SegmentCount = save["SegmentsCount"]?.Value<int>() ?? 0;

            Debug = Math.Max(Debug, save["Debug"]?.Value<int>() ?? 0);
            Deaths = Math.Max(Deaths, save["Deaths"]?.Value<int>() ?? 0);
            Resets = Math.Max(Resets, save["Resets"]?.Value<int>() ?? 0);
            Saves = Math.Max(Saves, save["Saves"]?.Value<int>() ?? 0);

            KeyRooms = save["KeyRooms"]?.ToObject<Dictionary<string, List<string>>>() ?? new Dictionary<string, List<string>>();

            for (int i = 0; i < KeyItems?.Count; i++)
            {
                KeyItems[i].State = save["KeyItems"]?[i]?.Value<int>() ?? 0;
                UpdatePictureKeyItemState(i);
            }
        }

        private void InitSaveDatabase()
        {
            SaveDatabase = new List<JObject>(); // Initialize SaveDatabase as a new List<JObject>
            CurrentSaveID = 0;

            var directoryPath = "saves/";
            var files = System.IO.Directory.GetFiles(directoryPath, "*.json");

            foreach (var file in files)
            {
                var jsonString = File.ReadAllText(file); // Read the contents of the file
                var jsonObject = JObject.Parse(jsonString); // Parse the JSON string into a JObject
                SaveDatabase.Add(jsonObject); // Add the JObject to the SaveDatabase list
                CurrentSaveID = Math.Max(CurrentSaveID, jsonObject["SaveID"].Value<int>()); // Update the CurrentSaveID based on the SaveID value in the JObject
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
