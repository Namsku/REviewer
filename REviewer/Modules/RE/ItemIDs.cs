using System.Configuration;
using System.IO;
using Newtonsoft.Json;

namespace REviewer.Modules.RE
{
    public class Property
    {
        public Property()
        {
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public string Color { get; set; }
        public string Img { get; set; }
    }

    public class ItemIDs
    {
        private readonly string? _processName;
        private readonly Dictionary<string, int> _duplicateItems;
        public Dictionary<byte, Property> Items;
        public Dictionary<string, string> CorrectProcessName = new Dictionary<string, string>()
        {
            { "Bio", "Bio" },
            { "bio", "Bio" },
            { "Biohazard", "Bio" },
            { "biohazard", "Bio" },
            { "Bio2 1.10", "Bio2 1.10" },
            { "bio2 1.10", "Bio2 1.10" },
            { "BIOHAZARD(R) 3 PC", "BIOHAZARD(R) 3 PC" },
            { "biohazard(r) 3 pc", "BIOHAZARD(R) 3 PC" },
            { "bio3", "BIOHAZARD(R) 3 PC" },
            { "Bio3", "BIOHAZARD(R) 3 PC" }
        };

        public class Item
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string? Color { get; set; }
            public string Img { get; set; }
        }

        public class BioHazardItems
        {
            public Dictionary<string, Item> ItemIDs { get; set; }
            public Dictionary<string, int> DupItems { get; set; }
        }

        public ItemIDs(string selectedProcessName)
        {
            _processName = CorrectProcessName[selectedProcessName];
            var reDataPath = ConfigurationManager.AppSettings["REdata"];
            var json = reDataPath != null ? File.ReadAllText(reDataPath) : throw new ArgumentNullException(nameof(reDataPath));

            // _processName is the key to the BioHazardItems object in the JSON
            var bios = JsonConvert.DeserializeObject<Dictionary<string, BioHazardItems>>(json);

            if (bios == null)
            {
                throw new ArgumentNullException("The game data is null");
            }

            if (!bios.TryGetValue(_processName, out BioHazardItems? BioHazardItems))
            {
                throw new KeyNotFoundException($"BioHazardItems with key {_processName} not found in JSON.");
            }

            var Game = _processName.ToString();

            Items = BioHazardItems.ItemIDs.ToDictionary(
                pair => byte.Parse(pair.Key),
                pair => new Property
                {
                    Name = pair.Value.Name,
                    Type = pair.Value.Type,
                    Color = pair.Value.Color ?? "White",
                    Img = pair.Value.Img
                }
            );

            _duplicateItems = BioHazardItems.DupItems;
        }

        public List<string> GetValues()
        {
            return Items.Values.Select(property => property.Name).ToList();
        }

        public List<Property> GetKeyItems()
        {
            var keyItems = Items.Values.Where(property => property.Type == "Key Item").ToList();

            foreach (var item in _duplicateItems)
            {
                var value = GetPropertyIdByName(item.Key);
                for (var i = 0; i < item.Value - 1; i++)
                {
                    keyItems.Insert(keyItems.FindIndex(property => property.Name == Items[value].Name) + 1, Items[value]);
                }
            }

            return keyItems;
        }

        public Property? GetPropertyById(byte id)
        {
            return Items.TryGetValue(id, out var property) ? property : Items[255];
        }

        public byte GetPropertyIdByName(string? name)
        {
            return Items.FirstOrDefault(property => property.Value.Name == name).Key;
        }

        public string GetPropertyNameById(byte id)
        {
            // give me last item in the dictionary if nothing is found
            return Items.TryGetValue(id, out var property) ? property.Name : Items[255].Name;
        }

        public string GetPropertyTypeById(byte id)
        {
            return Items.TryGetValue(id, out var property) ? property.Type : Items[255].Type;
        }

        public string GetPropertyImgById(byte id)
        {
            return Items.TryGetValue(id, out var property) ? property.Img : Items[255].Img;
        }

        public string? GetProcessName()
        {
            return Char.ToUpper(_processName[0]) + _processName.Substring(1);
        }
    }
}