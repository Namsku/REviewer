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
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Color { get; set; }
        public string? Img { get; set; }
    }

    public class StandardProperty
    {
        public int Size { get; set; }
        public Dictionary<byte, string>? Database { get; set; }
    }

    public class AdvancedProperty
    {
        public int Size { get; set; }
        public Dictionary<string, List<string>>? Database { get; set; }
    }

    public class JPlayer
    {
        public StandardProperty? Character { get; set; }
        public AdvancedProperty? Health { get; set; }
        public StandardProperty? CharacterHealthState { get; set; }
        public StandardProperty? LastItemFound { get; set; }
        public StandardProperty? CarlosLastItemFound { get; set; }
        public StandardProperty? LockPick { get; set; }
        public StandardProperty? InventoryCapacityUsed { get; set; }
        public StandardProperty? InventorySlotSelected { get; set; }
        public StandardProperty? CarlosInventorySlotSelected { get; set; }
        public StandardProperty? ItemBoxState { get; set; }
        public StandardProperty? Stage { get; set; }
        public StandardProperty? Room { get; set; }
        public StandardProperty? LastRoom { get; set; }
        public StandardProperty? Cutscene { get; set; }
        public StandardProperty? Unk001 { get; set; }
        public StandardProperty? Event { get; set; }
        public StandardProperty? PartnerPointer { get; set; }
        public StandardProperty? HitFlag { get; set; }
        public StandardProperty? EnemyCount { get; set; }
    }

    public class JGame
    {
        public StandardProperty? State { get; set; }
        public StandardProperty? Timer { get; set; }
        public StandardProperty? Save { get; set; }
        public StandardProperty? Frame { get; set; }
        public StandardProperty? Unk001 { get; set; }
        public StandardProperty? MainMenu { get; set; }
        public StandardProperty? SaveContent { get; set; }
    }

    public class JPosition
    {
        public StandardProperty? X { get; set; }
        public StandardProperty? Y { get; set; }
        public StandardProperty? Z { get; set; }
    }

    public class JRebirth
    {
        public StandardProperty? Debug { get; set; }
        public StandardProperty? Screen { get; set; }
        public StandardProperty? State { get; set; }
    }

    public class JEnemy
    {
        public StandardProperty? EnemyInfos { get; set; }
    }

    public class Bio
    {
        public Dictionary<string, string>? Info { get; set; }
        public Dictionary<string, JItem>? ItemIDs { get; set; }
        public Dictionary<string, int>? DupItems { get; set; }
        public List<string>? KeyRooms { get; set; }
        public Dictionary<string, string>? RoomIDs { get; set; }
        public Dictionary<string, string>? Offsets { get; set; }
        public JPlayer? Player { get; set; }
        public JGame? Game { get; set; }
        public JPosition? Position { get; set; }
        public JRebirth? Rebirth { get; set; }
        public JEnemy? Ennemy { get; set; }
    }
}