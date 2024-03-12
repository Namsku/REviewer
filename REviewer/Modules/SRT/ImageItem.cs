using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace REviewer.Modules.SRT
{
    /// <summary>
    /// Interaction logic for SRT.xaml
    /// </summary>
    /// 
    public class ImageItem : INotifyPropertyChanged
    {
        private string? _source;
        public string? Source { 
            get
            {
                return _source; 
            }
            
            set
            {
                _source = value; 
                OnPropertyChanged(nameof(Source));
            } 
        }

        private double _opacity;
        public double Opacity
        {
            get
            {
                return _opacity;
            }
            set
            {                 
                _opacity = value;
                OnPropertyChanged(nameof(Opacity));
                       
            }
        }

        private Brush? _background;

        public Brush? Background
        {
            get
            {
                return _background;
            }
            set
            {
                _background = value;
                OnPropertyChanged(nameof(Background));
            }
        }
        
        private double _height;
        public double Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
                OnPropertyChanged(nameof(Height));
            }
        }

        private double _width;
        public double Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
                OnPropertyChanged(nameof(Width));
            }
        }

        public string? _text;
        public string? Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        private Brush? _color;

        public Brush? Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }

        private Visibility? _textVisibility;

        public Visibility? TextVisibility
        {
            get
            {
                return _textVisibility;
            }
            set
            {
                _textVisibility = value;
                OnPropertyChanged(nameof(TextVisibility));
            }
        }

        private Visibility? _visibility;

        public Visibility? Visibility
        {
            get
            {
                return _visibility;
            }
            set
            {
                _visibility = value;
                OnPropertyChanged(nameof(Visibility));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
