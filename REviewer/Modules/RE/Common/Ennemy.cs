using REviewer.Modules.Utils;
using System.ComponentModel;
using System.Windows;

namespace REviewer.Modules.RE.Common
{
    public class Enemy : INotifyPropertyChanged
    {
        private int _maxHealth;
        private int _currentHealth;
        private int _pose;
        private int _flag;
        private int _id;

        public int OldState;
        public int CurrentState;

        private Visibility _visibility = Visibility.Collapsed;

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
