using Newtonsoft.Json;
using System.IO;

namespace REviewer.Modules.RE
{
    public class RoomIDs
    {
        private readonly Dictionary<string, string> _ids;

        public RoomIDs(string jsonFilePath, string processName)
        {
            if (File.Exists(jsonFilePath))
            {
                var json = File.ReadAllText(jsonFilePath);
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(json);

                if (dictionary.TryGetValue(processName, out var processDictionary) &&
                    processDictionary.TryGetValue("RoomIDs", out var roomIDs))
                {
                    _ids = roomIDs;
                }
                else
                {
                    _ids = [];
                }
            }
            else
            {
                _ids = [];
            }
        }


        // Get room name from room ID if not found return "umbrella"
        public string GetRoomName(string roomID)
        {
            return _ids.TryGetValue(roomID, out var roomName) ? roomName : "umbrella";
        }
    }
}