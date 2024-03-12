using System.ComponentModel;
using System.Windows.Media;
using REviewer.Modules.RE.Json;

namespace REviewer.Modules.Utils
{
    public class VariableData : INotifyPropertyChanged
    {
        private int _value;
        private double _height;
        private double _width;
        private Brush? _background;
        private FontFamily? _fontFamily;

        public object? Database { get; set; }
        public bool IsUpdated { get; set; } = false;
        public object LockObject { get; set; } = new();
        public IntPtr Offset { get; set; }
        public uint Size { get; set; }
        public Brush? Background {
            get { return _background; } 
            set
            {
                if (_background != value)
                {
                    _background = value;
                    OnPropertyChanged(nameof(Background));
                }
            }
        }

        public double Height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        public double Width
        {
            get { return _width; }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        public int Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public FontFamily? FontFamily
        {
            get { return _fontFamily; }
            set
            {
                if (_fontFamily != value)
                {
                    _fontFamily = value;
                    OnPropertyChanged(nameof(FontFamily));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public VariableData(IntPtr offset, uint size)
        {
            Offset = offset;
            Size = size;
            Database = null;
        }

        public VariableData(IntPtr offset, StandardProperty property)
        {
            Offset = offset;
            Size = (uint)property.Size;
            Database = (Dictionary<byte, string>?) property.Database;
        }

        public VariableData(IntPtr offset, AdvancedProperty property)
        {
            Offset = offset;
            Size = (uint)property.Size;
            Database = (Dictionary<byte, List<int>>?) Library.ConvertDictionnary(property.Database);
        }
    }
}
