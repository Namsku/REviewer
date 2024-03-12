using Newtonsoft.Json;
using REviewer.Modules.RE.Common;
using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace REviewer
{
    /// <summary>
    /// Interaction logic for Tracker.xaml
    /// </summary>
    public partial class Tracker : Window
    {
        private RootObject _game;
        public ObservableCollection<EnnemyTracking> Tracking { get; set; }
        public Tracker(ObservableCollection<EnnemyTracking> trk, RootObject obj)
        {
            InitializeComponent();
            _game = obj;
            Tracking = trk;

            _game.GameState.PropertyChanged += GameState_PropertyChanged;

            DataContext = this;
        }

        private void GameState_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var state = _game.GameState.Value & 0xFF000000;
            bool loading = state == 0x46000000 || state == 0x56000000 || state == 0x66000000;
            if (e.PropertyName == nameof(VariableData.Value) && loading)
            {
                foreach (var enemy in Tracking)
                {
                    enemy.Enemy.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
