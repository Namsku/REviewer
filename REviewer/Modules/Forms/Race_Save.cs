using System.Security.Cryptography;
using MessagePack;
using REviewer.Modules.Utils;
using static REviewer.Modules.RE.GameData;

namespace REviewer.Modules.Forms
{
    public partial class Race 
    {
        private void OnCreated(object source, FileSystemEventArgs e) => InvokeUI(() =>
        {
            _raceDatabase.Saves += 1;
            _raceDatabase.RealItembox = GetItemBoxData();
            byte[] dumpSaveFromMemory = _monitorVariables.ReadProcessMemory(GameData.SaveContentOffset, 0x4B0);
            // 0x80 at 0x205 should be set to 0x00 for some reason i really don't want to understand now
            dumpSaveFromMemory[0x205] = 0x80;

            // Get Sha256 hash string of the save
            _raceDatabase.sha256Hash = BitConverter.ToString(SHA256.HashData(dumpSaveFromMemory)).Replace("-", "").ToLower();

            // Saving the SRT state on a binary file (Serialized object)
            SaveState();

            // better to increment at this end of the function
            labelSaves.Text = _raceDatabase.Saves.ToString();
            Logger.Logging.Info($"File -> {e.Name} has been created -> {_raceDatabase.sha256Hash}");
        });

        public void SerializeObject<T>(T obj) where T : PlayerRaceProgress
        {
            var directoryPath = "saves/";
            // Make an exact copy of the object
            T copy_obj = (T)obj.Clone();

            _saves.Add(copy_obj);

            byte[] objectData = MessagePackSerializer.Serialize(copy_obj);

            byte[] hashBytes = SHA256.HashData(objectData);
            string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            // Append a unique identifier to the filename
            string filePath = Path.Combine(directoryPath, hashString + "_" + Guid.NewGuid().ToString() + ".dat");
            File.WriteAllBytes(filePath, objectData);
            Logger.Logging.Info($"Saved SRT state");
        }

        public static T DeserializeObject<T>(string filePath)
        {
            byte[] objectData = File.ReadAllBytes(filePath);
            T obj = MessagePackSerializer.Deserialize<T>(objectData);
            return obj;
        }

        private void SaveState()
        {
            _raceDatabase.TickTimer = _game.Game.Timer.Value;

            _raceDatabase.Fulltimer = new RaceWatch(_raceWatch.Elapsed);
            foreach (var _seg in _segmentWatch)
            {
                _raceDatabase.SegTimers.Add(new RaceWatch(_seg.Elapsed));
            }

            SerializeObject(_raceDatabase);
        }

        private void LoadState()
        {
            // byte[] inventory_data = GetItemBoxData();
            // byte[] modded = inventory_data.Skip(2).ToArray();
            byte[] dumpSaveFromMemory = _monitorVariables.ReadProcessMemory(GameData.SaveContentOffset, 0x4B0);
            // 0x80 at 0x205 should be set to 0x00 for some reason i really don't want to understand now

            if (dumpSaveFromMemory == null)
            {
                return;
            }

            dumpSaveFromMemory[0x205] = 0x80;
            string sha256_dump = BitConverter.ToString(SHA256.HashData(dumpSaveFromMemory)).Replace("-", "").ToLower();
            Logger.Logging.Debug($"Save dump from memroy -> {sha256_dump}");
            var save = _saves.FirstOrDefault(s => s.sha256Hash.Equals(sha256_dump));

            if (save == null)
            {
                return;
            }

            Logger.Logging.Info($"Loading save from memory - Found with SHA256 Hash -> {sha256_dump}");


            // Reload RaceWatch and Segments 
            _raceWatch.Reset();
            _segmentWatch[_raceDatabase.Segments].Reset();

            // Load the save RaceWatch and Segments
            if (save?.Fulltimer?.GetOffset() > _raceWatch.Elapsed)
            {
                _raceWatch.StartFrom(save.Fulltimer.GetOffset());
            }

            for (int i = 0; i < save?.SegTimers?.Count; i++)
            {
                if (save.SegTimers[i]?.GetOffset() > _segmentWatch[i].Elapsed)
                {
                    _segmentWatch[i].StartFrom(save.SegTimers[i].GetOffset());
                }
            }

            if (_raceDatabase.Segments < save?.Segments)
            {
                _segmentWatch[_raceDatabase.Segments].Stop();
                _raceDatabase.Segments = save?.Segments ?? 0;
                _segmentWatch[_raceDatabase.Segments].Start();
            }

            _raceDatabase.Debugs = Math.Max(_raceDatabase.Debugs, save?.Debugs ?? 0);
            _raceDatabase.Deaths = Math.Max(_raceDatabase.Deaths, save.Deaths);
            _raceDatabase.Resets = Math.Max(_raceDatabase.Resets, save.Resets);
            _raceDatabase.Saves = Math.Max(_raceDatabase.Saves, save.Saves);

            labelDebug.Text = _raceDatabase.Debugs.ToString();
            labelDeaths.Text = _raceDatabase.Deaths.ToString();
            labelResets.Text = _raceDatabase.Resets.ToString();
            labelSaves.Text = _raceDatabase.Saves.ToString();

            _raceDatabase.KeyRooms = save.KeyRooms;

            LoadKeyItems(save.KeyItems);

            for (int i = 0; i < 8; i++)
            {
                Write(_game.ItemBox.Slots[i].Item, (int)save.RealItembox[2 * i]);
                Write(_game.ItemBox.Slots[i].Quantity, (int)save.RealItembox[2 * i + 1]);
            }

            UpdateLastItemFoundPicture();
        }
    }
}