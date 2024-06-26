﻿using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using REviewer.Modules.RE.Common;
using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;


namespace REviewer.Modules.RE
{

    public class GameData
    {
        private int _virtualMemoryPointer { get; set; }
        private Dictionary<string, Bio>? _data { get; set; }
        private string? _gameName { get; set; }

        public Dictionary<string, string> CorrectProcessName = Library.GetGameProcesses();

        public GameData(string gameName)
        {
            var reDataPath = ConfigurationManager.AppSettings["REdata"];
            var json = reDataPath != null ? File.ReadAllText(reDataPath) : throw new ArgumentNullException(nameof(reDataPath));
            _data = JsonConvert.DeserializeObject<Dictionary<string, Bio>>(json);
            _gameName = CorrectProcessName[gameName];
        }

        public RootObject GetGameData(ItemIDs ids, int virtualMemoryPointer)
        {
            if (_data == null || _gameName == null)
            {
                throw new ArgumentNullException("The game data or game name is null");
            }

            _virtualMemoryPointer = virtualMemoryPointer;

            var obj = new RootObject(_data[_gameName], ids, _virtualMemoryPointer);

            Logger.Instance.Info("RootObject has been created");
            return obj; 
        }
    }
}