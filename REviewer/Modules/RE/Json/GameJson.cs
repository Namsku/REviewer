using Newtonsoft.Json;

namespace REviewer.Modules.RE.Json
{
    public class JGameJson
    {
        [JsonExtensionData]
        public IDictionary<string, Newtonsoft.Json.Linq.JToken> AdditionalData { get; set; }

        public JGameJson()
        {
            AdditionalData = new Dictionary<string, Newtonsoft.Json.Linq.JToken>();
        }
    }

    public class JItem
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
        public string? Color { get; set; }
        public required string Img { get; set; }
    }

    public class StandardProperty
    {
        public required int Size { get; set; }
        public Dictionary<string, string>? Database { get; set; }
    }

    public class AdvancedProperty
    {
        public required int Size { get; set; }
        public Dictionary<string, List<string>>? Database { get; set; }
    }

    public class JPlayer
    {
        public required StandardProperty Character { get; set; }
        public required AdvancedProperty Health { get; set; }
        public required StandardProperty CharacterHealthState { get; set; }
        public required StandardProperty LastItemFound { get; set; }
        public required StandardProperty LockPick { get; set; }
        public required StandardProperty InventoryCapacityUsed { get; set; }
        public required StandardProperty InventorySlotSelected { get; set; }
        public required StandardProperty Stage { get; set; }
        public required StandardProperty Room { get; set; }
        public required StandardProperty LastRoom { get; set; }
        public required StandardProperty Cutscene { get; set; }
        public required StandardProperty Unk001 { get; set; }
        public required StandardProperty Event { get; set; }
    }

    public class JGame
    {
        public required StandardProperty State { get; set; }
        public required StandardProperty Timer { get; set; }
        public required StandardProperty Unk001 { get; set; }
        public required StandardProperty MainMenu { get; set; }
        public required StandardProperty SaveContent { get; set; }
    }

    public class JPosition
    {
        public required StandardProperty X { get; set; }
        public required StandardProperty Y { get; set; }
        public required StandardProperty Z { get; set; }
    }

    public class JRebirth
    {
        public required StandardProperty Debug { get; set; }
        public required StandardProperty Screen { get; set; }
        public required StandardProperty State { get; set; }
    }

    public class Bio
    {
        public required Dictionary<string, JItem> ItemIDs { get; set; }
        public required Dictionary<string, int> DupItems { get; set; }
        public required List<string> KeyRooms { get; set; }
        public required Dictionary<string, string> RoomIDs { get; set; }
        public required Dictionary<string, string> Offsets { get; set; }
        public required JPlayer Player { get; set; }
        public required JGame Game { get; set; }
        public required JPosition Position { get; set; }
        public required JRebirth Rebirth { get; set; }
    }
}