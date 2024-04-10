using REviewer.Modules.Utils;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace REviewer.Modules.RE.Enemies
{
    public class Enemy : INotifyPropertyChanged
    {
        private int _maxHealth;
        private int _currentHealth;
        private int _pose;
        private int _flag;
        private int _id;
        private string _name = "ENEMY";

        public int OldState;
        public int CurrentState;

        private SolidColorBrush _backgroundColor;
        public SolidColorBrush BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                if (_backgroundColor != value)
                {
                    _backgroundColor = value;
                    OnPropertyChanged(nameof(BackgroundColor));
                }
            }
        }

        private Visibility _visibility = Visibility.Collapsed;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public int MaxHealth
        {
            get { return _maxHealth; }
            set
            {
                if (_maxHealth != value)
                {
                    _maxHealth = value;
                    OnPropertyChanged(nameof(MaxHealth));
                }
            }
        }

        public int CurrentHealth
        {
            get { return _currentHealth; }
            set
            {
                if (_currentHealth != value)
                {
                    _currentHealth = value;
                    OnPropertyChanged(nameof(CurrentHealth));
                }
            }
        }

        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                if (_visibility != value)
                {
                    _visibility = value;
                    OnPropertyChanged(nameof(Visibility));
                }
            }
        }

        public int Pose
        {
            get { return _pose; }
            set
            {
                if (_pose != value)
                {
                    _pose = value;
                    OnPropertyChanged(nameof(Pose));
                }
            }
        }

        public int Flag
        {
            get { return _flag; }
            set
            {
                if (_flag != value)
                {
                    _flag = value;
                    OnPropertyChanged(nameof(Flag));
                }
            }
        }

        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
