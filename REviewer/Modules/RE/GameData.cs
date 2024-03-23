using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using REviewer.Modules.RE.Common;
using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;


namespace REviewer.Modules.RE
{

    public class GameData
    {
        private Dictionary<string, Bio>? _data { get; set; }
        private string? _gameName { get; set; }

        public Dictionary<string, string> CorrectProcessName = new Dictionary<string, string>()
        {
            { "Bio", "Bio" },
            { "bio", "Bio" },
            { "biohazard", "Bio" },
            { "Biohazard", "Bio" },
            { "Bio2 1.10", "Bio2 1.10" },
            { "bio2 1.10", "Bio2 1.10" },
            { "BIOHAZARD(R) 3 PC", "BIOHAZARD(R) 3 PC" },
            { "biohazard(r) 3 pc", "BIOHAZARD(R) 3 PC" },
            { "bio3", "BIOHAZARD(R) 3 PC" },
            { "Bio3", "BIOHAZARD(R) 3 PC" }
        };

        public GameData(string gameName)
        {
            var reDataPath = ConfigurationManager.AppSettings["REdata"];
            var json = reDataPath != null ? File.ReadAllText(reDataPath) : throw new ArgumentNullException(nameof(reDataPath));
            _data = JsonConvert.DeserializeObject<Dictionary<string, Bio>>(json);
            _gameName = CorrectProcessName[gameName];
        }

        public RootObject GetGameData(ItemIDs ids)
        {
            if (_data == null || _gameName == null)
            {
                throw new ArgumentNullException("The game data or game name is null");
            }

            var obj = new RootObject(_data[_gameName], ids);

            Logger.Instance.Info("RootObject has been created");
            return obj; 
        }
    }
}