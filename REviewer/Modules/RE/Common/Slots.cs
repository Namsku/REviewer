using System.ComponentModel;
using System.Windows.Media;
using REviewer.Modules.Utils;

namespace REviewer.Modules.RE.Common
{

    public class Slot : INotifyPropertyChanged
    {
        private VariableData? _item;
        private VariableData? _quantity;
        private string? _image;
        private int _position;

        public VariableData? Item
        {
            get { return _item; }
            set
            {
                _item = value;
                OnPropertyChanged(nameof(Item));
            }
        }

        public VariableData? Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
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
            Item = new VariableData(0, 1);
            Quantity = new VariableData(0, 1);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static List<Slot> GenerateSlots(nint startOffset, nint endOffset)
        {
            List<Slot> slots = [];

            for (nint i = startOffset; i < endOffset; i += 4)
            {
                slots.Add(new Slot
                {
                    Item = new VariableData(i, 1),
                    Quantity = new VariableData(i + 1, 1),
                    Position = (int) i
                }); 
            }

            return slots;
        }
    }
}
