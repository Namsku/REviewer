using System.ComponentModel;
using System.Windows;

namespace REviewer.Modules.RE.Common
{
    public partial class RootObject : INotifyPropertyChanged
    {
        private int _deaths;
        private int _saves;
        private int _kills;
        private int _resets;
        private int _debug;
        private int _shots;
        private int _hits;
        private int _roomsVisited;

        public Visibility DebugVisibility { get; set; }
        public Visibility HitVisibility { get; set; }

        public int Deaths
        {
            get { return _deaths; }
            set
            {
                if (_deaths != value)
                {
                    _deaths = value;
                    OnPropertyChanged(nameof(Deaths));
                }
            }
        }

        public int Saves
        {
            get { return _saves; }
            set
            {
                if (_saves != value)
                {
                    _saves = value;
                    OnPropertyChanged(nameof(Saves));
                }
            }
        }

        public int Kills
        {
            get { return _kills; }
            set
            {
                if (_kills != value)
                {
                    _kills = value;
                    OnPropertyChanged(nameof(Kills));
                }
            }
        }

        public int Debug
        {
            get { return _debug; }
            set
            {
                if (_debug != value)
                {
                    _debug = value;
                    OnPropertyChanged(nameof(Debug));
                }
            }
        }

        public int Shots
        {
            get { return _shots; }
            set
            {
                if (_shots != value)
                {
                    _shots = value;
                    OnPropertyChanged(nameof(Shots));
                }
            }
        }

        public int Hits
        {
            get { return _hits; }
            set
            {
                if (_hits != value)
                {
                    _hits = value;
                    OnPropertyChanged(nameof(Hits));
                }
            }
        }

        public int RoomsVisited
        {
            get { return _roomsVisited; }
            set
            {
                if (_roomsVisited != value)
                {
                    _roomsVisited = value;
                    OnPropertyChanged(nameof(RoomsVisited));
                }
            }
        }

        public int Resets
        {
            get { return _resets; }
            set
            {
                if (_resets != value)
                {
                    _resets = value;
                    OnPropertyChanged(nameof(Resets));
                }
            }
        }

        public void UpdateStats(int deaths, int saves, int kills, int debug, int shots, int hits, int roomsVisited, int resets)
        {
            Deaths = deaths;
            Saves = saves;
            Kills = kills;
            Debug = debug;
            Shots = shots;
            Hits = hits;
            Resets = resets;
            RoomsVisited = roomsVisited;
        }

        public void InitStats()
        {
            Deaths = 0;
            Saves = 0;
            Kills = 0;
            Debug = 0;
            Shots = 0;
            Resets = 0;
            Hits = 0;
            RoomsVisited = 0;
        }
    }
}