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
                int[] _inventoryCapacityArray = new int[] { };
                int capacity = _inventoryCapacity.Value;

                if (SELECTED_GAME == 100) {
                    capacity = capacity & 3;
                    _inventoryCapacityArray = new int[] { 6, 8, 8, 6 };
                }
                else if (SELECTED_GAME == 400)
                {
                    InventoryCapacitySize = 11;
                    _inventoryCapacityArray = new int[] { 11, };
                    return;
                }
                else
                {
                    capacity = 0;
                    _inventoryCapacityArray = new int[] { 11, };
                }
                
                if (ChrisInventoryHotfix) _inventoryCapacityArray = new int[] { 8, 8, 8, 8 };

                if (_inventoryCapacitySize == _inventoryCapacityArray[capacity])
                {
                    // avoiding extra checking for nothing here, it's only necessary when the player is switching between jill and chris.
                    return;
                }

                _inventoryCapacitySize = _inventoryCapacityArray[capacity];
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
        public void InitInventory(Bio bio, bool carlos = false)
        {
            DisposeInventory(); // Dispose previous objects

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (SELECTED_GAME == 400 || SELECTED_GAME == 200)
                {
                    _inventoryCapacitySize = 11;
                } 
                else
                { 
                    InventoryCapacity = new VariableData(Library.HexToInt(bio.Offsets["Capacity"]), 4);
                    InventoryCapacity.PropertyChanged += (sender, e) => UpdateInventoryCapacity();
                }

                var inventory_name_start = carlos ? "CarlosInventoryStart" : "InventoryStart";
                var inventory_name_end = carlos ? "CarlosInventoryEnd" : "InventoryEnd";
                var items = IDatabase.GetItems();

                Inventory = Slot.GenerateSlots(Library.HexToInt(bio.Offsets[inventory_name_start]) + _virtualMemoryPointer, Library.HexToInt(bio.Offsets[inventory_name_end]) + _virtualMemoryPointer, SELECTED_GAME);
                InventoryImages = new ObservableCollection<ImageItem>();
                InventoryImages.Clear();

                for (int i = 0, j = 0; i < Inventory.Count; i++, j++)
                {
                    if (j == 12) break;
                    int index = i;

                    InventoryImages.Add(new ImageItem
                    {
                        Source = items[(byte)Inventory[i].Item.Value].Img,
                        Width = 92,
                        Height = 92,
                        Opacity = i < MAX_INVENTORY_SIZE ? 1 : 0,
                        Text = Inventory[i].Quantity?.Value.ToString(),
                        TextVisibility = Visibility.Hidden,
                    });

                    // Subscribe to the PropertyChanged event of the Item and Quantity properties
                    Inventory[i].Item.PropertyChanged += (sender, e) => UpdateInventoryImage(index);
                    Inventory[i].Quantity.PropertyChanged += (sender, e) => UpdateTextInventoryImage(index);

                    if (Inventory[i].Type != null) Inventory[i].Type.PropertyChanged += (sender, e) => UpdateType(index);
                }
            });

        }

        private void UpdateInventoryCapacity()
        {
            InventoryCapacitySize = InventoryCapacitySize;
            OnPropertyChanged(nameof(InventoryCapacitySize));
        }

        private void UpdateType(int index)
        {
            // Console.WriteLine($"UpdateType -> {Inventory[index].Type.Value}");
            if(Inventory[index].Type.Value == 2)
            {
                InventoryImages[index].Source = "resources/re2/reserved.png";
                InventoryImages[index].TextVisibility = Visibility.Collapsed;
                OnPropertyChanged(nameof(InventoryImages));
            }
        }

        private void UpdateInventoryImage(int index)
        {
            var items = IDatabase.GetItems();

            Console.WriteLine($"UpdateInventoryImage -> {index} -> {InventoryCapacitySize} {Inventory[index].Item.Value}");

            if (index <= InventoryCapacitySize)
            {
                InventoryImages[index].Source = items[(byte)Inventory[index].Item.Value].Img;

                var item_id = Inventory[index].Item?.Value ?? 255;

                // if value is key item, update the key item
                if (IDatabase.GetPropertyById((byte) Inventory[index].Item.Value).Type == "Key Item")
                {
                    int state = 0;
                    var isValid = true;

                    if (SELECTED_GAME != 400)
                    {
                        state = GameState.Value & 0x0000FF00;
                        isValid = false;
                    }


                    if (SELECTED_GAME == 100)
                    {
                        isValid = state == 0x8800 || state == 0x8C00;
                    }
                    else if (SELECTED_GAME == 200)
                    {
                        isValid = ((state == 0x4000) && (ItemBoxState?.Value != 1));
                    }
                    else if (SELECTED_GAME == 300)
                    {
                        state = GameState.Value & 0x0F000000;
                        isValid = state == 0x09000000;
                    }

                    if (isValid)
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
                string pourcentage_ammo = "";
                int[] ammo_prct_array = new int[] { };

                if (SELECTED_GAME == 200)
                    ammo_prct_array = new int[] { 14, 15, 16, 23, 27, 28 };
                else if (SELECTED_GAME == 300)
                    ammo_prct_array = new int[] { 14, 15 };

                if (SELECTED_GAME > 100 && ammo_prct_array.Contains(Inventory[index].Item.Value))
                {
                    pourcentage_ammo = "%";
                }

                Property item = IDatabase.GetPropertyById((byte)Inventory[index].Item.Value);

                // Update the ImageItem at the given index in the InventoryImages list

                if (InventoryImages[index].Source == "resources/re2/reserved.png")
                {
                    InventoryImages[index].TextVisibility = Visibility.Hidden;
                }
                else
                {
                    InventoryImages[index].Text = Inventory[index].Quantity?.Value.ToString() + pourcentage_ammo;
                    InventoryImages[index].TextVisibility = ITEM_TYPES.Contains(item?.Type) ? Visibility.Hidden : Visibility.Visible;
                    InventoryImages[index].Color = CustomColors.Default;
                }

                InventoryImages[index].Color = item.Color switch
                {
                    "Yellow" => CustomColors.Yellow,
                    "Orange" => CustomColors.Orange,
                    "Blue" => CustomColors.Blue,
                    "Red" => CustomColors.Red,
                    _ => CustomColors.Default,
                };
            }
        }

        public void DisposeInventory()
        {
            if (Inventory == null) return;
            for (int i = 0; i < Inventory.Count; i++)
            {
                int index = i;
                Inventory[i].Item.PropertyChanged -= (sender, e) => UpdateInventoryImage(index);
                Inventory[i].Quantity.PropertyChanged -= (sender, e) => UpdateTextInventoryImage(index);
                if (Inventory[i].Type != null) Inventory[i].Type.PropertyChanged -= (sender, e) => UpdateType(index);
            }
        }

    }
}
