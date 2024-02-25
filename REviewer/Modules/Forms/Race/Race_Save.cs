using System.Security.Cryptography;
using MessagePack;
using Newtonsoft.Json.Linq;
using REviewer.Modules.SRT;
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

            _currentID += 1;
            _raceDatabase.SaveID = _currentID;
            byte[] bytes = BitConverter.GetBytes(_currentID);

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

                // better to increment at this end of the function
                labelSaves.Text = _raceDatabase.Saves.ToString();
                Logger.Instance.Info($"File -> {e.Name} has been created -> ID {_raceDatabase.SaveID:X}");
            }
            else
            {
                Logger.Instance.Error($"Failed to open file -> {e.Name} after 10 attempts");
            }
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
            Logger.Instance.Info($"Saved SRT state");
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

            foreach (var seg in _segmentWatch)
            {
                _raceDatabase.SegTimers.Add(new RaceWatch(seg.Elapsed));
            }

            SerializeObject(_raceDatabase);
        }

        private void LoadState(int value)
        {
            Logger.Instance.Info($"Searching the correct save state with value -> {value:X}");

            
            var save = _saves.FirstOrDefault(s => s.SaveID == value);

            if (save == null)
            {
                return;
            }

            Logger.Instance.Info($"Loading save from memory - Found with Magic Byte 0xDEAD and ID {value:X}");


            // Reload RaceWatch and Segments 
            //_raceWatch.Reset();
            //_segmentWatch[_raceDatabase.Segments].Reset();

            // Load the save RaceWatch and Segments
            if (save?.Fulltimer?.GetOffset() > _raceWatch.Elapsed)
            {
                _raceWatch.StartFrom(save.Fulltimer.GetOffset());
            }

            for (int i = 0; i < save?.SegTimers?.Count; i++)
            {
                try
                {
                    if (save.SegTimers[i]?.GetOffset() > _segmentWatch[i].Elapsed)
                    {
                        _segmentWatch[i].StartFrom(save.SegTimers[i].GetOffset());
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.Error($"Exception: {e.Message}\nStack Trace: {e.StackTrace}");
                    Logger.Instance.Error($"Index: {i}");
                    Logger.Instance.Error($"Size of SegTimers: {save?.SegTimers?.Count}");
                    Logger.Instance.Error($"Size of SegmentWatch: {_segmentWatch.Count}");
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