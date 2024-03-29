﻿using System.ComponentModel;
using System.Windows;
using REviewer.Modules.Utils;


namespace REviewer
{
    public class UINotify : INotifyPropertyChanged
    {
        private string? _version;
        public string? Version
        {
            get { return _version; }
            set
            {
                if (Version != value)
                {
                    _version = value;
                    OnPropertyChanged(nameof(Version));
                }
            }
        }

        private Visibility _visibility;
        public Visibility ChrisInventory
        {
            get { return _visibility; }
            set
            {
                if (_visibility != value)
                {
                    _visibility = value;
                    OnPropertyChanged(nameof(ChrisInventory));
                }
            }
        }

        private Visibility _sherry;
        public Visibility Sherry
        {
            get { return _sherry; }
            set
            {
                if (_sherry != value)
                {
                    _sherry = value;
                    OnPropertyChanged(nameof(Sherry));
                }
            }
        }

        public UINotify(string version)
        {
            Version = version;
            ChrisInventory = Visibility.Collapsed;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isNormalMode;
        public bool isNormalMode { 
            get { return _isNormalMode; }
            set
            {
                if (_isNormalMode != value)
                {
                    _isNormalMode = value;
                    Library.UpdateConfigFile("isNormalMode", _isNormalMode.ToString().ToLower());
                }
            }
        }

        private bool _isBiorandMode;
        public bool isBiorandMode
        {
            get { return _isBiorandMode; }
            set
            {
                if (_isBiorandMode != value)
                {
                    _isBiorandMode = value;
                    Library.UpdateConfigFile("isBiorandMode", _isBiorandMode.ToString().ToLower());
                }
            }
        }

        private bool _isHealthBarChecked;
        public bool isHealthBarChecked
        {
            get { return _isHealthBarChecked; }
            set
            {
                if (_isHealthBarChecked != value)
                {
                    _isHealthBarChecked = value;
                    Library.UpdateConfigFile("isHealthBarChecked", _isHealthBarChecked.ToString().ToLower());
                }
            }
        }

        private bool _isItemBoxChecked;
        public bool isItemBoxChecked
        {             
            get { return _isItemBoxChecked; }
            set
            {
                if (_isItemBoxChecked != value)
                {
                    _isItemBoxChecked = value;
                    Library.UpdateConfigFile("isItemBoxChecked", _isItemBoxChecked.ToString().ToLower());
                }
            }
        }

        private bool _isChrisInventoryChecked;
        public bool isChrisInventoryChecked
        {
            get { return _isChrisInventoryChecked; }
            set
            {
                if (_isChrisInventoryChecked != value)
                {
                    _isChrisInventoryChecked = value;
                    Library.UpdateConfigFile("isChrisInventoryChecked", _isChrisInventoryChecked.ToString().ToLower());
                }
            }
        }

        private bool _isSherryChecked;
        public bool isSherryChecked
        {
            get { return _isSherryChecked; }
            set
            {
                if (_isSherryChecked != value)
                {
                    _isSherryChecked = value;
                    Library.UpdateConfigFile("isSherryChecked", _isSherryChecked.ToString().ToLower());
                }
            }
        }

        private bool _isMinimalistChecked;
        public bool isMinimalistChecked
        {
            get { return _isMinimalistChecked; }
            set
            {
                if (_isMinimalistChecked != value)
                {
                    _isMinimalistChecked = value;
                    Library.UpdateConfigFile("isMinimalistChecked", _isMinimalistChecked.ToString().ToLower());
                }
            }
        }

        private bool _isNoSegmentsTimerChecked;
        public bool isNoSegmentsTimerChecked
        {
            get { return _isNoSegmentsTimerChecked; }
            set
            {
                if (_isNoSegmentsTimerChecked != value)
                {
                    _isNoSegmentsTimerChecked = value;
                    Library.UpdateConfigFile("isNoSegmentsTimerChecked", _isNoSegmentsTimerChecked.ToString().ToLower());
                }
            }
        }

        private bool _isNoStatsChecked;
        public bool isNoStatsChecked
        {
            get { return _isNoStatsChecked; }
            set
            {
                if (_isNoStatsChecked != value)
                {
                    _isNoStatsChecked = value;
                    Library.UpdateConfigFile("isNoStatsChecked", _isNoStatsChecked.ToString().ToLower());
                }
            }
        }
    }

}
