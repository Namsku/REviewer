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
        public string Hash
        {
            get
            {
                // generate a hash of all the values in the inventory
                string hash = "";
                if (Inventory != null)
                {
                    for (int i = 0; i < Inventory.Count; i++)
                    {
                        var item = Inventory[i].Item;
                        var quantity = Inventory[i].Quantity;
                        hash += item?.Value.ToString() ?? string.Empty;
                        hash += quantity?.Value.ToString() ?? string.Empty;
                    }
                }
                return hash;
            }

        }

        private ImageItem? _inventoryImagesSelected;
        public ImageItem? InventoryImagesSelected
        {
            get { return _inventoryImagesSelected; }
            set
            {
                if (_inventoryImagesSelected != value)
                {
                    // Console.WriteLine($"InventoryImagesSelected changed to {value?.Source}");
                    _inventoryImagesSelected = value;
                    OnPropertyChanged(nameof(InventoryImagesSelected));
                }
            }
        }

        private int? _inventoryEquippedOverlay;

        public int? InventoryEquippedOverlay
        {
            get { return _inventoryEquippedOverlay; }
            set
            {
                if (_inventoryEquippedOverlay != value)
                {
                    _inventoryEquippedOverlay = value;
                    OnPropertyChanged(nameof(InventoryEquippedOverlay));
                }
            }
        }


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

        private void _inventoryEquippedOverlay_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InventoryEquippedOverlay))
            {
                OnPropertyChanged(nameof(InventoryEquippedOverlay));
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

                if (SELECTED_GAME == BIOHAZARD_1)
                {
                    capacity = capacity & 3;
                    _inventoryCapacityArray = new int[] { 6, 8, 8, 6 };
                }
                else if (SELECTED_GAME == BIOHAZARD_CVX)
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

                if (InventoryImages != null)
                {
                    for (int i = MAX_INVENTORY_SIZE - 2; i < MAX_INVENTORY_SIZE; i++)
                    {
                        if (i < InventoryImages.Count && InventoryImages[i] != null)
                        {
                            InventoryImages[i].Opacity = isVisible ? 1 : 0;
                            InventoryImages[i].TextVisibility = isVisible ? Visibility.Visible : Visibility.Hidden;
                        }
                    }
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
                if (SELECTED_GAME == BIOHAZARD_CVX || SELECTED_GAME == BIOHAZARD_2)
                {
                    _inventoryCapacitySize = 11;
                }
                else
                {
                    if (bio.Offsets != null && bio.Offsets.ContainsKey("Capacity"))
                    {
                        InventoryCapacity = new VariableData(Library.HexToInt(bio.Offsets["Capacity"]), 4);
                        InventoryCapacity.PropertyChanged += (sender, e) => UpdateInventoryCapacity();
                    }
                }

                var inventory_name_start = carlos ? "CarlosInventoryStart" : "InventoryStart";
                var inventory_name_end = carlos ? "CarlosInventoryEnd" : "InventoryEnd";
                var items = IDatabase.GetItems();

                if (bio.Offsets != null && bio.Offsets.ContainsKey(inventory_name_start) && bio.Offsets.ContainsKey(inventory_name_end))
                {
                    Inventory = Slot.GenerateSlots(Library.HexToInt(bio.Offsets[inventory_name_start]) + _virtualMemoryPointer, Library.HexToInt(bio.Offsets[inventory_name_end]) + _virtualMemoryPointer, SELECTED_GAME);
                }
                else
                {
                    Inventory = new List<Slot>();
                }
                InventoryImages = new ObservableCollection<ImageItem>();
                InventoryImages.Clear();

                if (Inventory != null)
                {
                    for (int i = 0, j = 0; i < Inventory.Count; i++, j++)
                    {
                        if (j == 12) break;
                        int index = i;

                        var item = Inventory[i].Item;
                        var quantity = Inventory[i].Quantity;
                        string? imgSource = null;
                        if (item != null && items != null && items.Count > (byte)item.Value)
                        {
                            imgSource = items[(byte)item.Value].Img;
                        }

                        InventoryImages.Add(new ImageItem
                        {
                            Source = imgSource,
                            Width = 92,
                            Height = 92,
                            Opacity = i < MAX_INVENTORY_SIZE ? 1 : 0,
                            Text = quantity?.Value.ToString(),
                            TextVisibility = Visibility.Hidden,
                        });

                        // Subscribe to the PropertyChanged event of the Item and Quantity properties
                        if (item != null)
                            item.PropertyChanged += (sender, e) => UpdateInventoryImage(index);
                        if (quantity != null)
                            quantity.PropertyChanged += (sender, e) => UpdateTextInventoryImage(index);

                        var type = Inventory[i].Type;
                        if (type != null) type.PropertyChanged += (sender, e) => UpdateType(index);
                    }
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
            if (Inventory != null && index < Inventory.Count)
            {
                var type = Inventory[index].Type;
                if (type != null && type.Value == 2 && InventoryImages != null && index < InventoryImages.Count)
                {
                    InventoryImages[index].Source = "resources/re2/reserved.png";
                    InventoryImages[index].TextVisibility = Visibility.Collapsed;
                    OnPropertyChanged(nameof(InventoryImages));
                }
            }
        }

        private void UpdateInventoryImage(int index)
        {
            var items = IDatabase.GetItems();

            if (Inventory != null && index < Inventory.Count && InventoryImages != null && index < InventoryImages.Count)
            {
                var item = Inventory[index].Item;
                if (item != null && index <= InventoryCapacitySize && items != null && items.Count > (byte)item.Value)
                {
                    InventoryImages[index].Source = items[(byte)item.Value].Img;

                    var item_id = item?.Value ?? 255;

                    // if value is key item, update the key item
                    var property = IDatabase.GetPropertyById((byte)item.Value);
                    if (property != null && property.Type == "Key Item")
                    {
                        int state = 0;
                        var isValid = true;

                        if (GameState != null && SELECTED_GAME != 400)
                        {
                            state = GameState.Value & 0x0000FF00;
                            isValid = false;
                        }

                        if (SELECTED_GAME == BIOHAZARD_1)
                        {
                            isValid = state == 0x8800 || state == 0x8C00;
                        }
                        else if (SELECTED_GAME == BIOHAZARD_2)
                        {
                            isValid = ((state == 0x4000) && (ItemBoxState?.Value != 1));
                        }
                        else if (SELECTED_GAME == BIOHAZARD_3)
                        {
                            if (GameState != null)
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
        }

        private void UpdateTextInventoryImage(int index)
        {
            if (Inventory != null && index < Inventory.Count && InventoryImages != null && index < InventoryImages.Count)
            {
                string pourcentage_ammo = "";
                int[] ammo_prct_array = new int[] { };

                if (SELECTED_GAME == BIOHAZARD_2)
                    ammo_prct_array = new int[] { 14, 15, 16, 23, 27, 28 };
                else if (SELECTED_GAME == BIOHAZARD_3)
                    ammo_prct_array = new int[] { 14, 15 };

                var item = Inventory[index].Item;
                var quantity = Inventory[index].Quantity;
                if (item != null && SELECTED_GAME > 100 && ammo_prct_array.Contains(item.Value))
                {
                    pourcentage_ammo = "%";
                }

                Property? property = null;
                if (item != null)
                    property = IDatabase.GetPropertyById((byte)item.Value);

                // Update the ImageItem at the given index in the InventoryImages list

                if (InventoryImages[index].Source == "resources/re2/reserved.png")
                {
                    InventoryImages[index].TextVisibility = Visibility.Hidden;
                }
                else
                {
                    InventoryImages[index].Text = (quantity?.Value.ToString() ?? string.Empty) + pourcentage_ammo;
                    InventoryImages[index].TextVisibility = (property != null && ITEM_TYPES.Contains(property.Type)) ? Visibility.Hidden : Visibility.Visible;
                    InventoryImages[index].Color = CustomColors.Default;
                }

                if (property != null)
                {
                    InventoryImages[index].Color = property.Color switch
                    {
                        "Yellow" => CustomColors.Yellow,
                        "Orange" => CustomColors.Orange,
                        "Blue" => CustomColors.Blue,
                        "Red" => CustomColors.Red,
                        _ => CustomColors.Default,
                    };
                }

                if (index == InventoryEquippedOverlay)
                {
                    // Console.WriteLine("SOME CHANGES");
                    InventoryImagesSelected = InventoryImages[index];
                    OnPropertyChanged(nameof(InventoryEquippedOverlay));
                }
            }
        }

        public void DisposeInventory()
        {
            if (Inventory == null) return;
            for (int i = 0; i < Inventory.Count; i++)
            {
                int index = i;
                var item = Inventory[i].Item;
                var quantity = Inventory[i].Quantity;
                var type = Inventory[i].Type;
                if (item != null)
                    item.PropertyChanged -= (sender, e) => UpdateInventoryImage(index);
                if (quantity != null)
                    quantity.PropertyChanged -= (sender, e) => UpdateTextInventoryImage(index);
                if (type != null) type.PropertyChanged -= (sender, e) => UpdateType(index);
            }
        }

    }
}
