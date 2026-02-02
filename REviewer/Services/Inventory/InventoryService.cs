using REviewer.Services.Inventory;
using REviewer.Core.Memory;
using REviewer.Modules.RE;
using REviewer.Modules.RE.Json;
using REviewer.Modules.RE.Common;
using System.ComponentModel;
using System.Windows; // For Visibility
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using REviewer.Core.Constants;
using REviewer.Modules.Utils;
using System;
using System.Collections.ObjectModel; // Added for ObservableCollection

namespace REviewer.Services.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly IMemoryMonitor _monitor;
        private Modules.Utils.VariableData? _itemBoxState;

        public InventoryService(IMemoryMonitor monitor)
        {
            _monitor = monitor;
        }

        public bool IsItemBoxOpen => _itemBoxState?.Value != 0;
        public event EventHandler? ItemBoxAccessed;

        public System.Collections.Generic.List<Modules.RE.Common.Slot>? Inventory
        {
            get => _inventory;
            private set => SetField(ref _inventory, value);
        }

        public System.Collections.ObjectModel.ObservableCollection<Modules.SRT.ImageItem>? InventoryImages
        {
            get => _inventoryImages;
            private set => SetField(ref _inventoryImages, value);
        }
        private System.Collections.ObjectModel.ObservableCollection<Modules.SRT.ImageItem>? _inventoryImages;
        private System.Collections.Generic.List<Modules.RE.Common.Slot>? _inventory;

        private nint _virtualMemoryPointer;
        private Bio? _bio;
        private int _selectedGame;
        private bool _carlos;
        private Modules.RE.ItemIDs? _itemIds;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Initialize(object bioObj, nint virtualMemoryPointer, int selectedGame, Modules.RE.ItemIDs itemIds, bool carlos = false)
        {
            _virtualMemoryPointer = virtualMemoryPointer;
            _selectedGame = selectedGame;
            _carlos = carlos;
            _itemIds = itemIds;
            
            if (bioObj is Bio bio)
            {
                _bio = bio;
                InitializeItemBoxState(bio);
            }

            RefreshInventory();
        }

        private void InitializeItemBoxState(Bio bio)
        {
            if (bio.Offsets != null && bio.Offsets.TryGetValue("ItemBoxState", out string? offset))
            {
                // Unregister old if exists? Service is Singleton but Init called on game switch.
                // Assuming re-use, we should unregister old.
                if (_itemBoxState != null) _monitor.UnregisterVariable("InventoryService.ItemBoxState");

                _itemBoxState = new Modules.Utils.VariableData(Modules.Utils.Library.HexToInt(offset) + _virtualMemoryPointer, bio.Player.ItemBoxState);
                _monitor.RegisterVariable("InventoryService.ItemBoxState", _itemBoxState);
            }
        }

        public void RefreshInventory()
        {
            if (_bio == null) return;
            
            // ItemBox Monitor Check
            if (_itemBoxState != null && _itemBoxState.Value != 0)
            {
                ItemBoxAccessed?.Invoke(this, EventArgs.Empty);
            }

            var inventory_name_start = _carlos ? "CarlosInventoryStart" : "InventoryStart";
            var inventory_name_end = _carlos ? "CarlosInventoryEnd" : "InventoryEnd";

            if (_bio.Offsets != null && _bio.Offsets.ContainsKey(inventory_name_start) && _bio.Offsets.ContainsKey(inventory_name_end))
            {
                nint start = (nint)Modules.Utils.Library.HexToInt(_bio.Offsets[inventory_name_start]) + _virtualMemoryPointer;
                nint end = (nint)Modules.Utils.Library.HexToInt(_bio.Offsets[inventory_name_end]) + _virtualMemoryPointer;
                
                Inventory = Modules.RE.Common.Slot.GenerateSlots(start, end, _selectedGame);
            }
            else
            {
                Inventory = new System.Collections.Generic.List<Modules.RE.Common.Slot>();
            }

            // Populate InventoryImages
            InventoryImages = new System.Collections.ObjectModel.ObservableCollection<Modules.SRT.ImageItem>();
            if (Inventory != null && _itemIds != null)
            {
                var items = _itemIds.GetItems();

                for (int i = 0; i < Inventory.Count; i++)
                {
                    var slot = Inventory[i];
                    var item = slot.Item;
                    var quantity = slot.Quantity;
                    
                    var imageItem = new Modules.SRT.ImageItem
                    {
                        Width = 92,
                        Height = 92,
                        Opacity = 1, // Placeholder logic
                        TextVisibility = Visibility.Hidden
                    };
                    
                    InventoryImages.Add(imageItem);
                    
                    void UpdateImage() {
                        if (item == null || items == null) return;
                        var val = (byte)item.Value;
                        if (items.TryGetValue(val, out var property)) {
                            imageItem.Source = property.Img;
                            
                            // Text Logic
                            // Note: Logic from RootObject checked ammunition types etc.
                            // Simplified for now: Show quantity if > 1 or specific type
                            if (quantity != null)
                            {
                                imageItem.Text = quantity.Value.ToString();
                                // Visibility logic based on type/quantity
                                imageItem.TextVisibility = (property.Type == "Weapon" || quantity.Value > 1) ? Visibility.Visible : Visibility.Hidden;
                            }
                            
                            // Color Logic
                            switch(property.Color)
                            {
                                case "Yellow": imageItem.Color = Modules.Utils.CustomColors.Yellow; break;
                                case "Orange": imageItem.Color = Modules.Utils.CustomColors.Orange; break;
                                case "Blue": imageItem.Color = Modules.Utils.CustomColors.Blue; break;
                                case "Red": imageItem.Color = Modules.Utils.CustomColors.Red; break;
                                default: imageItem.Color = Modules.Utils.CustomColors.Default; break;
                            }
                        } else {
                            imageItem.Source = null;
                            imageItem.TextVisibility = Visibility.Hidden;
                        }
                    }
                    
                    if (item != null) item.PropertyChanged += (s, e) => UpdateImage();
                    if (quantity != null) quantity.PropertyChanged += (s, e) => UpdateImage();
                    UpdateImage();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
