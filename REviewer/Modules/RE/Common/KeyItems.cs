using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using REviewer.Modules.SRT;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{
    public partial class RootObject : INotifyPropertyChanged
    {
        private ObservableCollection<ImageItem> _keyItemImages = new ObservableCollection<ImageItem>();
        public ObservableCollection<ImageItem> KeyItemImages
        {
            get => _keyItemImages;
            set
            {
                _keyItemImages = value;
                OnPropertyChanged(nameof(KeyItemImages));
            }
        }

        private void UpdateRaceKeyItem(int value, string room, int state, bool force = false)
        {
            if (KeyItems == null) return;

            value = GetKeyItemPosition(value, room, state);

            if (force || KeyItems[value].State < state)
            {
                Logger.Instance.Info($"Updating key item {value} to state {state} in room {room}");
                KeyItems[value].State = state;
                UpdatePictureKeyItemState(value);
            }

            KeyItems[value].Room = room;
        }

        private void UpdatePictureKeyItemState(int value)
        {
            if (KeyItems == null) return;

            var state = KeyItems[value].State;
            var opacity = state switch
            {
                1 => 1,
                2 => 1,
                _ => 0.15 // Default case
            };
            KeyItemImages[value].Opacity = opacity;

            var background = state switch
            {
                -1 => Brushes.Transparent,
                0 => Brushes.Transparent,
                1 => CustomColors.Orange,
                2 => CustomColors.Green,
                _ => Brushes.Transparent // Default case
            };
            KeyItemImages[value].Background = background;
        }

        private int GetKeyItemPosition(int value, string room, int state)
        {
            var name = IDatabase.GetPropertyNameById((byte)value);
            var item_box = (GameState?.Value & 0x0000FF00) == 0x90;
            var position = -1;

            for (int i = 0; i < KeyItems?.Count; i++)
            {
                var keyItem = KeyItems[i];
                if (keyItem?.Data?.Name == name)
                {
                    position = i;
                    var _raceState = keyItem?.State;

                    if (keyItem?.Room == room && _raceState <= state && !item_box && _raceState < 2)
                    {
                        return i;
                    }

                    if (keyItem?.State == -1)
                    {
                        return i;
                    }
                }
            }

            return position;
        }

        public void InitKeyItemsModel(List<KeyItem> keyItems)
        {
            if (keyItems == null)
                throw new ArgumentNullException(nameof(keyItems));

            KeyItems = keyItems;

            KeyItemImages.Clear();

            Logger.Instance.Info("Creating/Generating KeyItems Images");

            foreach (var keyItem in keyItems)
            {
                if (keyItem.Data == null)
                    throw new ArgumentNullException($"The key item data is null");

                KeyItemImages.Add(new ImageItem
                {
                    Source = keyItem.Data.Img,
                    Width = 70,
                    Height = 64,
                    Opacity = 0.15,
                    Background = Brushes.Transparent
                });
            }
        }
    }
}
