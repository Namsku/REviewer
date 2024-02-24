using System.Configuration;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using REviewer.Modules.RE;
using REviewer.Modules.SRT;
using REviewer.Modules.Utils;
using static REviewer.Modules.RE.GameData;

namespace REviewer.Modules.Forms
{
    public partial class Race
    {
        private void InitPixelBoyFont()
        {
            PrivateFontCollection privateFontCollection = new();
            byte[] fontData = Properties.Resources.Pixeboy;

            // Load the font into the private font collection
            int dataLength = fontData.Length;
            IntPtr fontPtr = Marshal.AllocCoTaskMem(dataLength);
            Marshal.Copy(fontData, 0, fontPtr, dataLength);
            privateFontCollection.AddMemoryFont(fontPtr, dataLength);
            Marshal.FreeCoTaskMem(fontPtr);

            // Create a new font from the private font collection
            _fontPixelBoy = privateFontCollection.Families[0];
        }

        private void InitLoadState()
        {
            _saves = [];
            _currentID = 0;

            var directoryPath = "saves/";
            var files = System.IO.Directory.GetFiles(directoryPath, "*.dat");

            foreach (var file in files)
            {
                _saves.Add(DeserializeObject<PlayerRaceProgress>(file));
                _currentID = Math.Max(_currentID, _saves.Last().SaveID);
            }
        }

        private void InitKeyRooms()
        {
            var reDataPath = ConfigurationManager.AppSettings["REdata"];
            var json = reDataPath != null ? File.ReadAllText(reDataPath) : throw new ArgumentNullException(nameof(reDataPath));
            var processName = _itemDatabase.GetProcessName();
            var bios = JsonConvert.DeserializeObject<Dictionary<string, KRoom>>(json);

            if (!bios.TryGetValue(processName, out var bio))
            {
                throw new KeyNotFoundException($"Bio with key {processName} not found in JSON.");
            }

            List<string> keyRooms = bio.KeyRooms;

            // add them into _raceDatabase.KeyRooms
            _raceDatabase.KeyRooms = [];

            foreach (var room in keyRooms)
            {
                _raceDatabase.KeyRooms.Add(room, []);
            }
        }

        private void InitInventory()
        {
            for (int i = 0; i < _inventoryCapacitySize; i++)
            {
                int slotNumber = i; // To capture the variable in the closure
                UpdateSlotPicture(slotNumber);
                UpdateSlotCapacity(_slotLabels[slotNumber], slotNumber);
            }

            UpdateInventorySlotSelectedPicture();
            UpdateLastItemFoundPicture();
        }

        private void InitSaveMonitoring()
        {
            if (_raceDatabase.SavePath == null)
            {
                return;
            }


            _raceDatabase.Watcher.Path = _raceDatabase.SavePath;  // replace with your directory
            _raceDatabase.Watcher.Filter = "*.dat";  // watch for .dat files

            // Add event handlers.
            _raceDatabase.Watcher.Created += new FileSystemEventHandler(OnCreated);
            
            _raceDatabase.Watcher.Disposed += (sender, e) =>
            {
                Logger.Instance.Debug("File watcher disposed");
            };

            _raceDatabase.Watcher.Error += (sender, e) =>
            {
                Logger.Instance.Error($"File watcher error: {e.GetException()}");
            };

            // Subscribe to the Changed event
            _raceDatabase.Watcher.Changed += (sender, e) =>
            {
                Logger.Instance.Debug($"File changed: {e.FullPath}");
            };

            // Subscribe to the Created event
            _raceDatabase.Watcher.Created += (sender, e) =>
            {
                Logger.Instance.Debug($"File created: {e.FullPath}");
            };

            // Subscribe to the Deleted event
            _raceDatabase.Watcher.Deleted += (sender, e) =>
            {
                Logger.Instance.Debug($"File deleted: {e.FullPath}");
            };

            // Subscribe to the Renamed event
            _raceDatabase.Watcher.Renamed += (sender, e) =>
            {
                Logger.Instance.Debug($"File renamed from {e.OldFullPath} to {e.FullPath}");
            };

            // Begin watching.
            _raceDatabase.Watcher.EnableRaisingEvents = true;

        }

                private void InitLabels()
        {
            try
            {
                labelTimer.Font = _pixelBoyDefault;
                labelTimer.ForeColor = CustomColors.White;

                CheckHealthLabel(_game.Player.Health.Value);
                labelCharacter.Text = ((Dictionary<byte, string>)_game.Player.Character.Database)[(byte)_game.Player.Character.Value];

                _raceDatabase.Stage = ((_game.Player.Stage.Value & 0x04) + 1).ToString();

                labelSegTimer1.Font = _pixelBoySegments;
                labelSegTimer2.Font = _pixelBoySegments;
                labelSegTimer3.Font = _pixelBoySegments;
                labelSegTimer4.Font = _pixelBoySegments;

                labelSegTimer1.ForeColor = CustomColors.White;
                labelSegTimer2.ForeColor = CustomColors.White;
                labelSegTimer3.ForeColor = CustomColors.White;
                labelSegTimer4.ForeColor = CustomColors.White;

                labelDeaths.Font = _pixelBoyDefault;
                labelDeaths.ForeColor = CustomColors.White;

                labelDebug.Font = _pixelBoyDefault;
                labelDebug.ForeColor = CustomColors.White;

                labelResets.Font = _pixelBoyDefault;
                labelResets.ForeColor = CustomColors.White;

                labelSaves.Font = _pixelBoyDefault;
                labelSaves.ForeColor = CustomColors.White;

                labelCharacter.Font = _pixelBoyDefault;
                labelCharacter.ForeColor = CustomColors.White;

                labelGameCompleted.Font = _pixelBoySegments;
                labelGameCompleted.ForeColor = CustomColors.Red;
                labelGameCompleted.Visible = false;

                _raceWatch.Reset();
                labelTimer.Text = _raceWatch.Elapsed.ToString(@"hh\:mm\:ss\.ff");

                labelDeaths.Text = _raceDatabase.Deaths.ToString();
                labelDebug.Text = _raceDatabase.Debugs.ToString();
                labelResets.Text = _raceDatabase.Resets.ToString();
                labelSaves.Text = _raceDatabase.Saves.ToString();
            }
            catch (Exception e)
            {
                Logger.Instance.Error($"Exception: {e.Message}\nStack Trace: {e.StackTrace}");
            }
        }

        private void InitCharacterHealthState()
        {
            labelHealth.Font = _pixelBoyHealthFont;
            CheckCharacterHealthState(_game.Player.CharacterHealthState.Value);
        }

        private void InitChronometers()
        {
            if (_segmentWatch.Count > 0)
            {
                foreach (var stopwatch in _segmentWatch)
                {
                    stopwatch.Reset();
                }
            }
            else
            {
                _segmentWatch =
                [
                    new RaceWatch(),
                    new RaceWatch(),
                    new RaceWatch(),
                    new RaceWatch()
                ];
            }

            _raceWatch.Reset();
            labelTimer.Text = _raceWatch.Elapsed.ToString(@"hh\:mm\:ss\.ff");
            labelSegTimer1.Text = _raceWatch.Elapsed.ToString(@"hh\:mm\:ss\.ff");
            labelSegTimer2.Text = _raceWatch.Elapsed.ToString(@"hh\:mm\:ss\.ff");
            labelSegTimer3.Text = _raceWatch.Elapsed.ToString(@"hh\:mm\:ss\.ff");
            labelSegTimer4.Text = _raceWatch.Elapsed.ToString(@"hh\:mm\:ss\.ff");
        }
        private void InitInventorySlots()
        {
            _slotLabels = [];
            _pictures = [];

            for (int i = 0; i < 8; i++)
            {
                string labelName = $"labelSlot{i + 1}Quantity";
                string pictureName = $"pictureBoxItemSlot{i + 1}";

                Label label = Controls.Find(labelName, true).FirstOrDefault() as Label;
                PictureBox pictureBox = Controls.Find(pictureName, true).FirstOrDefault() as PictureBox;

                if (label != null)
                    _slotLabels.Add(i, label);

                if (pictureBox != null)
                    _pictures.Add(i, pictureBox);
            }
        }

        private void InitKeyItems()
        {
            _raceDatabase.KeyItems = _itemDatabase.GetKeyItems()?.Select(item => new KeyItem((Property)item, -1, "NEW ROOM TO SAVE HERE")).ToList() ?? [];

            _pictureKeyItems = [];

            for (int i = 0; i < _raceDatabase.KeyItems.Count; i++)
            {
                string pictureName = $"pictureBoxKeyItem{i + 1}";
                PictureBox pictureBox = Controls.Find(pictureName, true).FirstOrDefault() as PictureBox;

                if (pictureBox != null)
                {
                    _pictureKeyItems.Add(i, pictureBox);
                    UpdateKeyItemPicture(pictureBox, _raceDatabase.KeyItems[i]);
                }
            }
        }
    }
}