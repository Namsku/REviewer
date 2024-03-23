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
        private List<Slot>? _itemBox;
        public List<Slot>? ItemBox
        {
            get { return _itemBox; }
            set
            {
                if (_itemBox != value)
                {
                    _itemBox = value;
                    OnPropertyChanged(nameof(ItemBox));
                }
            }
        }


        private ObservableCollection<ImageItem>? _itemboxImages;
        public ObservableCollection<ImageItem>? ItemboxImages
        {
            get
            {
                return _itemboxImages;
            }
            set
            {
                _itemboxImages = value;
                OnPropertyChanged(nameof(ItemboxImages));
            }
        }
        public void InitItemBox(Bio bio, bool carlos=false)
        {
            var inventory_name_start = carlos ? "CarlosItemBoxStart" : "ItemBoxStart";
            var inventory_name_end = carlos ? "CarlosItemBoxEnd" : "ItemBoxEnd";

            if (ItemBox == null)
            {
                ItemBox = Slot.GenerateSlots(Library.HexToInt(bio.Offsets["ItemBoxStart"]), Library.HexToInt(bio.Offsets["ItemBoxEnd"]));
            }

            ItemboxImages = new ObservableCollection<ImageItem>();

            ItemBox.Clear();

            for (int i = 0; i < ItemBox.Count; i++)
            {
                int index = i;

                ItemboxImages.Add(new ImageItem
                {
                    Source = IDatabase.Items[(byte)ItemBox[i].Item.Value].Img,
                    Width = 45,
                    Height = 60,
                    Opacity = 1,
                });

                // Subscribe to the PropertyChanged event of the Item and Quantity properties
                ItemBox[i].Item.PropertyChanged += (sender, e) => UpdateItemboxImage(index);
                ItemBox[i].Quantity.PropertyChanged += (sender, e) => UpdateItemboxImage(index);

            }

        }

        private void UpdateItemboxImage(int index)
        {
            // Update the ImageItem at the given index in the InventoryImages list
            ItemboxImages[index].Source = IDatabase.Items[(byte)ItemBox[index].Item.Value].Img;
            ItemboxImages[index].Text = ItemBox[index].Quantity.Value.ToString();
        }
    }
}
