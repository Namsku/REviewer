namespace REviewer.Modules.RE.Json
{
    public class GameJson
    {
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }

        public GameJson()
        {
            AdditionalData = new Dictionary<string, JToken>();
        }
    }

    public class Item
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
        public string? Color { get; set; }
        public required string Img { get; set; }
    }

    public class Property
    {
        public required int Size { get; set; }
        public dictionary<string, string>? Database { get; set; }
    }

    public class Player
    {
        public required Property Character { get; set; }
        public required Property Health { get; set; }
        public required Property CharacterHealthState { get; set; }
        public required Property LastItemFound { get; set; }
        public required Property LockPick { get; set; }
        public required Property InventoryCapacityUsed { get; set; }
        public required Property InventorySelected { get; set; }
        public required Property Stage { get; set; }
        public required Property Room { get; set; }
        public required Property LastRoom { get; set; }
        public required Property Cutscene { get; set; }
        public required Property Unk001 { get; set; }
        public required Property Event { get; set; }
    }

    public class Game
    {
        public required Property State { get; set; }
        public required Property Timer { get; set; }
        public required Property Unk001 { get; set; }
        public required Property MainMenu { get; set; }
        public required Property SaveContent { get; set; }
    }

    public class Position
    {
        public required Property X { get; set; }
        public required Property Y { get; set; }
        public required Property Z { get; set; }
    }

    public class Rebirth
    {
        public required Property Debug { get; set; }
        public required Property Screen { get; set; }
        public required Property State { get; set; }
    }

    public class Bio
    {
        public required Dictionary<string, Item> ItemIDs { get; set; }
        public required Dictionary<string, int> DupItems { get; set; }
        public required Dictionary<string, list> KeyRooms { get; set; }
        public required Dictionary<string, list> RoomIDs { get; set; }
        public required Dictionary<string, list> Offsets { get; set; }
        public required Player Player { get; set; }
        public required Game Game { get; set; }
        public required Position Position { get; set; }
        public required Rebirth Rebirth { get; set; }
    }
}
```