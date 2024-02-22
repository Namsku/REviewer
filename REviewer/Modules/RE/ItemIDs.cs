using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using MessagePack;
using Newtonsoft.Json;

namespace REviewer.Modules.RE
{
    [MessagePackObject]
    public class Property : ICloneable
    {
        public Property()
        {
        }

        [Key(0)]
        public required string Name { get; set; }
        [Key(1)]
        public required string Type { get; set; }
        [Key(2)]
        public required string Color { get; set; }
        [IgnoreMember]
        public required Bitmap Img { get; set; }

        public object Clone()
        {
            // Create a new Bitmap object from the existing one
            Bitmap clonedImg = null;
            if (Img != null)
            {
                clonedImg = new Bitmap(Img);
            }

            return new Property
            {
                // Copy all properties
                Name = this.Name,
                Type = this.Type,
                Color = this.Color,
                Img = clonedImg
            };
        }
    }

    public class ItemIDs
    {
        private readonly string? _processName;
        private readonly Dictionary<string, int> _duplicateItems;
        public Dictionary<byte, Property> Items;

        public class Item
        {
            public required string Name { get; set; }
            public required string Type { get; set; }
            public string? Color { get; set; }
            public required string Img { get; set; }
        }

        public class Bio
        {
            public required Dictionary<string, Item> ItemIDs { get; set; }
            public required Dictionary<string, int> DupItems { get; set; }
        }

        public ItemIDs(string selectedProcessName)
        {
            _processName = selectedProcessName;
            var reDataPath = ConfigurationManager.AppSettings["REdata"];
            var json = reDataPath != null ? File.ReadAllText(reDataPath) : throw new ArgumentNullException(nameof(reDataPath));

            // _processName is the key to the Bio object in the JSON
            var bios = JsonConvert.DeserializeObject<Dictionary<string, Bio>>(json);

            if (!bios.TryGetValue(_processName, out var bio))
            {
                throw new KeyNotFoundException($"Bio with key {_processName} not found in JSON.");
            }

            Items = bio.ItemIDs.ToDictionary(
                pair => byte.Parse(pair.Key),
                pair => new Property
                {
                    Name = pair.Value.Name,
                    Type = pair.Value.Type,
                    Color = pair.Value.Color,
                    Img = (Bitmap)Properties.Resources.ResourceManager.GetObject(pair.Value.Img)
                }
            );

            _duplicateItems = bio.DupItems;
        }

        public List<string> GetValues()
        {
            return Items.Values.Select(property => property.Name).ToList();
        }

        public List<Property> GetKeyItems()
        {
            var keyItems = Items.Values.Where(property => property.Type == "Key Item").ToList();

            // Automatically add the duplicate items with _duplicateItems and put the correct amount of them in the list with correct ID
            // This is the old version
            // keyItems.Insert(keyItems.FindIndex(property => property.Name == "Battery") + 1, Items[39]);
            // keyItems.InsertRange(keyItems.FindIndex(property => property.Name == "MO Disk") + 1, new List<Property> { Items[40], Items[40] });
            
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

        public byte GetPropertyIdByName(string name)
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

        public Bitmap GetPropertyImgById(byte id)
        {
            return Items.TryGetValue(id, out var property) ? property.Img : Items[255].Img;
        }

        public string GetProcessName()
        {
            return _processName;
        }
    }
}