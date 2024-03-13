using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using REviewer.Modules.RE.Json;
using REviewer.Modules.SRT;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public partial class RootObject : INotifyPropertyChanged
    {
        public bool ChrisInventoryHotfix = false;

        private VariableData? _inventoryCapacity;
        public VariableData? InventoryCapacity
        {
            get { return _inventoryCapacity; }
            set
            {
                if (_inventoryCapacity != null)
                {
                    _inventoryCapacity.PropertyChanged -= _inventoryCapacity_PropertyChanged;
                }

                _inventoryCapacity = value;

                if (_inventoryCapacity != null)
                {
                    _inventoryCapacity.PropertyChanged += _inventoryCapacity_PropertyChanged;
                }
            }
        }

        private void _inventoryCapacity_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                OnPropertyChanged(nameof(InventoryCapacitySize));
            }
        }

        private int _inventoryCapacitySize;
        public int InventoryCapacitySize
        {
            get { return _inventoryCapacitySize; }
            set
            {
                if (_inventoryCapacity == null) return;
                int[] _inventoryCapacityArray = [6, 8, 8, 6];

                if (ChrisInventoryHotfix) _inventoryCapacityArray = [8, 8, 8, 8];

                if (_inventoryCapacitySize == _inventoryCapacityArray[_inventoryCapacity.Value & 3])
                {
                    // avoiding extra checking for nothing here, it's only necessary when the player is switching between jill and chris.
                    return;
                }

                _inventoryCapacitySize = _inventoryCapacityArray[_inventoryCapacity.Value & 3];
                bool isVisible = _inventoryCapacitySize > 6;

                for (int i = MAX_INVENTORY_SIZE - 2; i < MAX_INVENTORY_SIZE ; i++)
                {
                    InventoryImages[i].Opacity = isVisible ? 1 : 0;
                    InventoryImages[i].TextVisibility = isVisible ? Visibility.Visible : Visibility.Hidden;
                }
                
                OnPropertyChanged(nameof(InventoryCapacitySize));
            }
        }

        private List<Slot>? _inventory;
        public List<Slot>? Inventory
        {
            get { return _inventory; }
            set
            {
                if (_inventory != value)
                {
                    _inventory = value;
                    OnPropertyChanged(nameof(Inventory));
                }
            }
        }

        private ObservableCollection<ImageItem>? _inventoryImages;
        public ObservableCollection<ImageItem>? InventoryImages
        {
            get
            {
                return _inventoryImages;
            }
            set
            {
                _inventoryImages = value;
                OnPropertyChanged(nameof(InventoryImages));
            }
        }
        public void InitInventory(Bio bio)
        {
            InventoryCapacity = new VariableData(Library.HexToNint(bio.Offsets["Capacity"]), 4);
            InventoryCapacity.PropertyChanged += (sender, e) => UpdateInventoryCapacity();

            if (Inventory == null)
            {
                Inventory = Slot.GenerateSlots(Library.HexToNint(bio.Offsets["InventoryStart"]), Library.HexToNint(bio.Offsets["InventoryEnd"]));
            }

            if (InventoryImages == null)
            {
                InventoryImages = new ObservableCollection<ImageItem>();
            }

            for (int i = 0; i < Inventory.Count; i++)
            {
                int index = i; 

                InventoryImages.Add(new ImageItem
                {
                    Source = IDatabase.Items[(byte)Inventory[i].Item.Value].Img,
                    Width = 92,
                    Height = 92,
                    Opacity = i < MAX_INVENTORY_SIZE ? 1 : 0,
                    Text = Inventory[i].Quantity?.Value.ToString(),
                    TextVisibility = Visibility.Hidden,
                });

                // Subscribe to the PropertyChanged event of the Item and Quantity properties
                Inventory[i].Item.PropertyChanged += (sender, e) => UpdateInventoryImage(index);
                Inventory[i].Quantity.PropertyChanged += (sender, e) => UpdateTextInventoryImage(index);

            }
        }

        private void UpdateInventoryCapacity()
        {
            InventoryCapacitySize = InventoryCapacitySize;
            OnPropertyChanged(nameof(InventoryCapacitySize));
        }

        private void UpdateInventoryImage(int index)
        {
            // Update the ImageItem at the given index in the InventoryImages list
            if (index <= InventoryCapacitySize)
            {
                InventoryImages[index].Source = IDatabase.Items[(byte)Inventory[index].Item.Value].Img;

                var item_id = Inventory[index].Item?.Value ?? 255;

                // if value is key item, update the key item
                if (IDatabase.GetPropertyById((byte) Inventory[index].Item.Value).Type == "Key Item")
                {
                    int state = GameState.Value & 0x0000FF00;

                    if (state == 0x8800 || state == 0x8C00)
                    {
                        UpdateRaceKeyItem(item_id, FullRoomName ?? "ERROR ROOM NAME", 2);
                    }
                }
                UpdateTextInventoryImage(index);
            }
        }

        private void UpdateTextInventoryImage(int index) 
        {
            if (index < InventoryCapacitySize)
            {
                Property item = IDatabase.GetPropertyById((byte)Inventory[index].Item.Value);

                // Update the ImageItem at the given index in the InventoryImages list
                InventoryImages[index].Text = Inventory[index].Quantity?.Value.ToString();
                InventoryImages[index].TextVisibility = ITEM_TYPES.Contains(item?.Type) ? Visibility.Hidden : Visibility.Visible;
                InventoryImages[index].Color = CustomColors.Default;

                InventoryImages[index].Color = item.Color switch
                {
                    "Yellow" => CustomColors.Yellow,
                    "Orange" => CustomColors.Orange,
                    "Red" => CustomColors.Red,
                    _ => CustomColors.Default,
                };
            }
        }
    }
}
