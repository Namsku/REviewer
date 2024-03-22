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
        public Dictionary<byte, string>? Database { get; set; }
    }

    public class AdvancedProperty
    {
        public required int Size { get; set; }
        public Dictionary<string, List<string>>? Database { get; set; }
    }

    public class JPlayer
    {
        public StandardProperty? Character { get; set; }
        public AdvancedProperty Health { get; set; }
        public StandardProperty? CharacterHealthState { get; set; }
        public StandardProperty? LastItemFound { get; set; }
        public StandardProperty? LockPick { get; set; }
        public StandardProperty? InventoryCapacityUsed { get; set; }
        public StandardProperty? InventorySlotSelected { get; set; }
        public StandardProperty? CarlosInventorySlotSelected { get; set; }
        public StandardProperty? Stage { get; set; }
        public StandardProperty? Room { get; set; }
        public StandardProperty? LastRoom { get; set; }
        public StandardProperty? Cutscene { get; set; }
        public StandardProperty? Unk001 { get; set; }
        public StandardProperty? Event { get; set; }
        public StandardProperty? PartnerPointer { get; set; }
    }

    public class JGame
    {
        public required StandardProperty State { get; set; }
        public required StandardProperty Timer { get; set; }
        public StandardProperty? Frame { get; set; }
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
        public StandardProperty? Debug { get; set; }
        public StandardProperty? Screen { get; set; }
        public StandardProperty? State { get; set; }
    }

    public class JEnnemy
    {
        public StandardProperty? EnnemyInfos { get; set; }
    }

    public class Bio
    {
        public required Dictionary<string, string> Info { get; set; }
        public required Dictionary<string, JItem> ItemIDs { get; set; }
        public required Dictionary<string, int> DupItems { get; set; }
        public required List<string> KeyRooms { get; set; }
        public required Dictionary<string, string> RoomIDs { get; set; }
        public required Dictionary<string, string> Offsets { get; set; }
        public required JPlayer Player { get; set; }
        public required JGame Game { get; set; }
        public required JPosition Position { get; set; }
        public required JRebirth Rebirth { get; set; }
        public required JEnnemy Ennemy { get; set; }
    }
}