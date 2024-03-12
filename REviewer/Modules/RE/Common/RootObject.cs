﻿using System.ComponentModel;
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
            public List<string> KeyRooms { get; set; }
        }

        private static readonly string[] ITEM_TYPES = ["Key Item", "Optionnal Key Item", "Nothing"];

        public int MAX_INVENTORY_SIZE;
        public int SaveID;
        public int CurrentSaveID;

        public List<KeyItem> KeyItems;
        public ItemIDs IDatabase; 
        public event PropertyChangedEventHandler? PropertyChanged;
        public string FullRoomName;
        public string LastRoomName;
        public FileSystemWatcher Watcher;
        public List<JObject> SaveDatabase;
        public Dictionary<string, List<string>> KeyRooms { get; set; }

        public Visibility HealthBarVisibility { get; set; }
        public Visibility BiorandVisibility { get; set; }
        public Visibility ItemBoxVisibility { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void InitUIConfig(Dictionary<string, bool?> config)
        {
            HealthBarVisibility = (bool) config["HealthBar"] ? Visibility.Visible : Visibility.Collapsed;
            BiorandVisibility = (bool) !config["Standard"] ? Visibility.Visible : Visibility.Collapsed;
            ItemBoxVisibility = (bool) config["ItemBox"] ? Visibility.Visible : Visibility.Collapsed;
        }

        public void InitMaxInventoryCapacity(string character)
        {
            if (character == "Chris")
            {
                MAX_INVENTORY_SIZE = 8;
            }
            else
            {
                MAX_INVENTORY_SIZE = 11;
            }
        }

        public void InitKeyRooms()
        {
            var reDataPath = ConfigurationManager.AppSettings["REdata"];
            var json = reDataPath != null ? File.ReadAllText(reDataPath) : throw new ArgumentNullException(nameof(reDataPath));
            var processName = IDatabase.GetProcessName();
            var bios = JsonConvert.DeserializeObject<Dictionary<string, KRoom>>(json);

            if (!bios.TryGetValue(processName, out var bio))
            {
                throw new KeyNotFoundException($"Bio with key {processName} not found in JSON.");
            }

            List<string> keyRooms = bio.KeyRooms;

            // add them into KeyRooms
            KeyRooms = [];

            foreach (var room in keyRooms)
            {
                // Console.WriteLine(room);
                KeyRooms.Add(room, []);
            }
        }

        public void InitFileWatcher()
        {
            Dictionary<string, string> db = new Dictionary<string, string>
            {
                { "Bio", "RE1" },
                { "Bio2", "RE2" },
                { "Bio3", "RE3" }
            };


            if (Watcher != null)
            {
                Watcher.EnableRaisingEvents = false;
                Watcher.Dispose();
            }

            if (Watcher == null)
            {
                Watcher = new FileSystemWatcher();
            }

            var processName = IDatabase.GetProcessName();
            Watcher.Path = Library.GetSavePath(processName);  // replace with your directory
            Watcher.Filter = "*.dat";  // watch for .dat files

            // Add event handlers.
            Watcher.Created += new FileSystemEventHandler(SaveFileDetected);

            // Begin watching.
            Watcher.EnableRaisingEvents = true;
        }

        private void SaveState()
        {
            JObject jsonObject = new JObject();

            jsonObject["GameState"] = GameState.Value;
            jsonObject["GameTimer"] = GameTimer.Value;
            jsonObject["MainMenu"] = MainMenu.Value;
            jsonObject["SaveContent"] = SaveContent.Value;
            jsonObject["Character"] = Character.Value;
            jsonObject["InventorySlotSelected"] = InventorySlotSelected.Value;
            jsonObject["Stage"] = Stage.Value;
            jsonObject["Room"] = Room.Value;
            jsonObject["Cutscene"] = Cutscene.Value;
            jsonObject["LastRoom"] = LastRoom.Value;
            jsonObject["Unk002"] = Unk002.Value;
            jsonObject["Event"] = Event.Value;
            jsonObject["LastItemFound"] = LastItemFound.Value;
            jsonObject["InventoryCapacityUsed"] = InventoryCapacityUsed.Value;
            jsonObject["CharacterHealthState"] = CharacterHealthState.Value;
            jsonObject["Health"] = Health.Value;
            jsonObject["LockPick"] = LockPick.Value;
            jsonObject["PositionX"] = PositionX.Value;
            jsonObject["PositionY"] = PositionY.Value;
            jsonObject["PositionZ"] = PositionZ.Value;
            jsonObject["RebirthDebug"] = RebirthDebug.Value;
            jsonObject["RebirthScreen"] = RebirthScreen.Value;
            jsonObject["RebirthState"] = RebirthState.Value;
            jsonObject["SaveID"] = SaveID;

            jsonObject["Debug"] = Debug;
            jsonObject["Deaths"] = Deaths;
            jsonObject["Resets"] = Resets;
            jsonObject["Saves"] = Saves;

            jsonObject["SegmentsCount"] = SegmentCount;

            jsonObject["Segments"] = JArray.FromObject(IGTSegments);
            jsonObject["KeyItems"] = JArray.FromObject(KeyItems.Select(item => item.State).ToList());
            jsonObject["KeyRooms"] = JObject.FromObject(KeyRooms);

            GenerateJsonSave(jsonObject);
        }

        public void GenerateJsonSave(JObject objectData)
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
            var save = SaveDatabase.FirstOrDefault(s => s["SaveID"].Value<int>() == value);

                if (save == null)
            {
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                IGTSegments[i] = Math.Max((int)IGTSegments[i], (int)save["Segments"][i]);
            }

            SegmentCount = save?["SegmentsCount"].Value<int>() ?? 0;

            Debug = Math.Max(Debug, save?["Debug"].Value<int>() ?? 0);
            Deaths = Math.Max(Deaths, save["Deaths"].Value<int>());
            Resets = Math.Max(Resets, save["Resets"].Value<int>());
            Saves = Math.Max(Saves, save["Saves"].Value<int>());

            KeyRooms = save["KeyRooms"].ToObject<Dictionary<string, List<string>>>();

            for (int i = 0; i < KeyItems.Count; i++)
            {
                KeyItems[i].State = save["KeyItems"][i].Value<int>();
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

        public RootObject(Bio bio, ItemIDs ids)
        {
            IDatabase = ids;
            InitMaxInventoryCapacity(bio.Player.Character.Database[0]);

            // KeyRooms
            InitKeyRooms();

            // Game
            GameState = new VariableData(Library.HexToNint(bio.Offsets["GameState"]), bio.Game.State);
            Unk001 = new VariableData(Library.HexToNint(bio.Offsets["GameUnk001"]), bio.Game.Unk001);
            GameTimer = new VariableData(Library.HexToNint(bio.Offsets["GameTimer"]), bio.Game.Timer);
            MainMenu = new VariableData(Library.HexToNint(bio.Offsets["MainMenu"]), bio.Game.MainMenu);
            SaveContent = new VariableData(Library.HexToNint(bio.Offsets["SaveContent"]), bio.Game.SaveContent);

            // Player
            Character = new VariableData(Library.HexToNint(bio.Offsets["Character"]), bio.Player.Character);
            InventorySlotSelected = new VariableData(Library.HexToNint(bio.Offsets["InventorySlotSelected"]), bio.Player.InventorySlotSelected);
            Stage = new VariableData(Library.HexToNint(bio.Offsets["Stage"]), bio.Player.Stage);
            Room = new VariableData(Library.HexToNint(bio.Offsets["Room"]), bio.Player.Room);
            Cutscene = new VariableData(Library.HexToNint(bio.Offsets["Cutscene"]), bio.Player.Cutscene);
            LastRoom = new VariableData(Library.HexToNint(bio.Offsets["LastRoom"]), bio.Player.LastRoom);
            Unk002 = new VariableData(Library.HexToNint(bio.Offsets["GameUnk001"]), bio.Player.Unk001);
            Event = new VariableData(Library.HexToNint(bio.Offsets["Event"]), bio.Player.Event);
            LastItemFound = new VariableData(Library.HexToNint(bio.Offsets["LastItemFound"]), bio.Player.LastItemFound);
            InventoryCapacityUsed = new VariableData(Library.HexToNint(bio.Offsets["InventoryCapacityUsed"]), bio.Player.InventoryCapacityUsed);
            CharacterHealthState = new VariableData(Library.HexToNint(bio.Offsets["CharacterHealthState"]), bio.Player.CharacterHealthState);
            Health = new VariableData(Library.HexToNint(bio.Offsets["CharacterHealth"]), bio.Player.Health);
            LockPick = new VariableData(Library.HexToNint(bio.Offsets["LockPick"]), bio.Player.LockPick);

            // Position
            PositionX = new VariableData(Library.HexToNint(bio.Offsets["PositionX"]), bio.Position.X);
            PositionY = new VariableData(Library.HexToNint(bio.Offsets["PositionY"]), bio.Position.Y);
            PositionZ = new VariableData(Library.HexToNint(bio.Offsets["PositionZ"]), bio.Position.Z);

            // Rebirth
            RebirthDebug = new VariableData(Library.HexToNint(bio.Offsets["RebirthDebug"]), bio.Rebirth.Debug);
            RebirthScreen = new VariableData(Library.HexToNint(bio.Offsets["RebirthScreen"]), bio.Rebirth.Screen);
            RebirthState = new VariableData(Library.HexToNint(bio.Offsets["RebirthState"]), bio.Rebirth.State);

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
        }


    }
}
