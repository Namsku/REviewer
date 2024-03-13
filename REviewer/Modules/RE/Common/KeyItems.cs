using System.Collections.ObjectModel;
using REviewer.Modules.SRT;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.ComponentModel;
using REviewer.Modules.Utils;
using System.Windows;

namespace REviewer.Modules.RE.Common
{
    public partial class RootObject : INotifyPropertyChanged
    {
        private ObservableCollection<ImageItem>? _keyItemImages;
        public ObservableCollection<ImageItem> KeyItemImages
        {
            get
            {
               if (_keyItemImages == null)
               {
                   _keyItemImages = new ObservableCollection<ImageItem>();
               }
                return _keyItemImages;
            }
            set
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _keyItemImages = value;
                    OnPropertyChanged(nameof(KeyItemImages));
                });
            }
        }

        private void UpdateRaceKeyItem(int value, string room, int state, bool force = false)
        {
            if (KeyItems == null)
            {
                return;
            }

            value = GetKeyItemPosition(value, room, state);

            // If 'force' is true or the current state is less than the new state, update the state and picture
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
            var state = KeyItems[value].State;

            KeyItemImages[value].Opacity = state switch
            {
                1 => 1,
                2 => 1,
                _ => 0.15 // Default case
            };

            // change background color of the picture box
            KeyItemImages[value].Background = state switch
            {
                -1 => Brushes.Transparent,
                0 => Brushes.Transparent,
                1 => CustomColors.Orange,
                2 => CustomColors.Green,
                _ => Brushes.Transparent // Default case
            };

            OnPropertyChanged(nameof(KeyItemImages));
        }

        private int GetKeyItemPosition(int value, string room, int state)
        {
            var name = IDatabase.GetPropertyNameById((byte)value);
            var item_box = (GameState.Value & 0x0000FF00) == 0x90;
            var position = -1;

            for (int i = 0; i < KeyItems.Count; i++)
            {
                if (KeyItems[i].Data.Name == name)
                {
                    position = i;
                    var _raceState = KeyItems[i].State;

                    if (KeyItems[i].Room == room && _raceState <= state && !item_box && _raceState < 2)
                    {
                        return i;
                    }

                    if (KeyItems[i].State == -1)
                    {
                        return i;
                    }
                }
            }

            return position;
        }

        public void InitKeyItemsModel(List<KeyItem> keyItems)
        {
            KeyItems = keyItems;

            if (keyItems == null)
            {
                throw new ArgumentNullException(nameof(keyItems));
            }


            KeyItemImages.Clear();

            Logger.Instance.Info("Creating/Generating KeyItems Images");

            foreach (var keyItem in keyItems)
            {
                if (keyItem.Data == null)
                {
                    throw new ArgumentNullException($"The key item data is null");
                }


                Application.Current.Dispatcher.Invoke(() =>
                {
                    KeyItemImages.Add(new ImageItem
                    {
                        Source = keyItem.Data.Img,
                        Width = 70,
                        Height = 64,
                        Opacity = 0.15,
                        Background = Brushes.Transparent
                    });
                });

            }
        }
    }
}
