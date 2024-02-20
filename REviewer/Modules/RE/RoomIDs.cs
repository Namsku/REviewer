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
                _ids = dictionary.ContainsKey(processName) && dictionary[processName].ContainsKey("RoomIDs") 
                    ? dictionary[processName]["RoomIDs"] 
                    : new Dictionary<string, string>();
            }
            else
            {
                _ids = new Dictionary<string, string>();
            }
        }


        // Get room name from room ID if not found return "UNKNOWN"
        public string GetRoomName(string roomID)
        {
            return _ids.TryGetValue(roomID, out var roomName) ? roomName : "UNKNOWN";
        }
    }
}