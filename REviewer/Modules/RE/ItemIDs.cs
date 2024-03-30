using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE
{
    public class Property
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Img { get; set; } = string.Empty;
    }

    public class ItemIDs
    {
        private readonly string _processName;
        private readonly Dictionary<string, int>? _duplicateItems;
        private readonly Dictionary<byte, Property> _items;
        private readonly byte _defaultItemId = 255;

        public ItemIDs(string selectedProcessName)
        {
            _processName = CorrectProcessName.GetValueOrDefault(selectedProcessName, selectedProcessName);
            var reDataPath = ConfigurationManager.AppSettings["REdata"];

            if (reDataPath == null)
                throw new ArgumentNullException(nameof(reDataPath), "REdata path is not specified in the configuration.");

            using var streamReader = new StreamReader(reDataPath);
            var json = streamReader.ReadToEnd();
            var bios = JsonConvert.DeserializeObject<Dictionary<string, BioHazardItems>>(json);

            if (bios == null)
                throw new ArgumentNullException("The game data is null");

            if (!bios.TryGetValue(_processName, out var bioHazardItems))
                throw new KeyNotFoundException($"BioHazardItems with key {_processName} not found in JSON.");

            _items = bioHazardItems.ItemIDs?.ToDictionary(
                pair => byte.Parse(pair.Key),
                pair => new Property
                {
                    Name = pair.Value.Name ?? string.Empty,
                    Type = pair.Value.Type ?? string.Empty,
                    Color = pair.Value.Color ?? "White",
                    Img = pair.Value.Img ?? string.Empty
                }
            ) ?? throw new ArgumentNullException(nameof(bioHazardItems.ItemIDs), "ItemIDs dictionary is null.");

            _duplicateItems = bioHazardItems.DupItems;
        }

        public Dictionary<byte, Property> GetItems()
        {
            return _items;
        }
        public List<string> GetValues()
        {
            return _items.Values.Select(property => property.Name).ToList();
        }

        public List<Property> GetKeyItems()
        {
            var keyItems = new List<Property>(_items.Values.Count); // Pre-allocate the list size

            foreach (var property in _items.Values)
            {
                if (property.Type == "Key Item")
                {
                    keyItems.Add(property);

                    if (_duplicateItems != null && _duplicateItems.TryGetValue(property.Name, out var count))
                    {
                        for (int i = 1; i < count; i++)
                        {
                            keyItems.Add(property); // Add duplicates
                        }
                    }
                }
            }

            return keyItems;
        }

        public Property GetPropertyById(byte id)
        {
            return _items.TryGetValue(id, out var property) ? property : _items[_defaultItemId];
        }

        public byte GetPropertyIdByName(string name)
        {
            return _items.FirstOrDefault(property => property.Value.Name == name).Key;
        }

        public string? GetPropertyNameById(byte id)
        {
            return _items.TryGetValue(id, out var property) ? property.Name : _items[_defaultItemId].Name;
        }

        public string? GetPropertyTypeById(byte id)
        {
            return _items.TryGetValue(id, out var property) ? property.Type : _items[_defaultItemId].Type;
        }

        public string? GetPropertyImgById(byte id)
        {
            return _items.TryGetValue(id, out var property) ? property.Img : _items[_defaultItemId].Img;
        }

        public string GetProcessName()
        {
            return Char.ToUpper(_processName[0]) + _processName[1..];
        }

        public class BioHazardItems
        {
            public Dictionary<string, Property>? ItemIDs { get; set; }
            public Dictionary<string, int>? DupItems { get; set; }
        }

        private static readonly Dictionary<string, string> CorrectProcessName = Library.GetGameProcesses();
    }
}
