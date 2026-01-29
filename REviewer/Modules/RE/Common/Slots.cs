using System.ComponentModel;
using System.Windows;
using System.Printing.IndexedProperties;
using System.Windows.Media;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{

    public class Slot : INotifyPropertyChanged
    {
        private VariableData? _item;
        private VariableData? _quantity;
        private VariableData? _type;
        private string? _image;
        private int _position;

        private System.Windows.Visibility _visibility = System.Windows.Visibility.Visible;
        public System.Windows.Visibility Visibility
        {
            get => _visibility;
            set
            {
                if (_visibility != value)
                {
                    _visibility = value;
                    OnPropertyChanged(nameof(Visibility));
                }
            }
        }

        public VariableData? Item
        {
            get { return _item; }
            set
            {
                if (_item != null) _item.PropertyChanged -= Item_PropertyChanged;
                _item = value;
                if (_item != null) _item.PropertyChanged += Item_PropertyChanged;
                UpdateVisibility();
                //Console.WriteLine(_item.Value);
                OnPropertyChanged(nameof(Item));
            }
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            if (_item != null && _item.Value == 0)
            {
                Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                Visibility = System.Windows.Visibility.Visible;
            }
        }

        public VariableData? Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                // Console.WriteLine(_quantity.Value);
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public VariableData? Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        public int Position
        {
            get { return _position; }
            set
            {
                _position = value;
                OnPropertyChanged(nameof(Position));
            }
        }

        public string? Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }

        public Slot()
        {
            //Item = new VariableData(0, 1);
            //Quantity = new VariableData(0, 1);
            //Type = new VariableData(0, 1);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static List<Slot> GenerateSlots(int startOffset, int endOffset, int game)
        {
            List<Slot> slots = new List<Slot>();

            int inc = ((endOffset - startOffset) / 2) > 10 ? 4 : 2;

            for (int i = startOffset; i < endOffset; i += inc)
            {
                if (game < 400)
                {
                    slots.Add(new Slot
                    {
                        Item = new VariableData(i, 1),
                        Quantity = new VariableData(i + 1, 1),
                        Type = inc > 2 ? new VariableData(i + 2, 1) : null,
                        Position = (int)i
                    });
                }
                else
                {
                    // Console.WriteLine("CVX Slot created");
                    slots.Add(new Slot
                    {
                        Item = new VariableData(i + 2, 1),
                        Quantity = new VariableData(i, 1),
                        Type = inc > 2 ? new VariableData(i + 3, 1) : null,
                        Position = (int)i,
                        
                    });
        }
            }

            return slots;
        }
    }
}
