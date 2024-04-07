﻿using REviewer.Modules.RE.Common;
using REviewer.Modules.RE.Enemies;
using REviewer.Modules.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace REviewer
{
    public partial class Tracker : Window, INotifyPropertyChanged
    {
        private readonly RootObject _game;
        private double _windowWidth;

        public double WindowWidth
        {
            get { return _windowWidth; }
            set
            {
                if (_windowWidth != value)
                {
                    _windowWidth = value;
                    OnPropertyChanged(nameof(WindowWidth));
                }
            }
        }
        public double? WindowScale { get; set; }
        public ObservableCollection<EnemyTracking> Tracking { get; set; }
        public Tracker(ObservableCollection<EnemyTracking> trk, RootObject obj)
        {
            _game = obj ?? throw new ArgumentNullException(nameof(obj));
            Tracking = trk ?? throw new ArgumentNullException(nameof(trk));


            InitializeComponent();
            SubscribeToEvents();
            SetWindowProperties();

            DataContext = this;
        }

        private void SetWindowProperties()
        {
            WindowWidth = _game.WindowWidth == 400 ? 530 : 355;
            WindowScale = _game.WindowWidth == 400 ? 1 : 0.7;

            OnPropertyChanged(nameof(WindowWidth)); // Raise PropertyChanged event for WindowWidth
            OnPropertyChanged(nameof(WindowScale)); // Raise PropertyChanged event for WindowScale
        }

        private void SubscribeToEvents()
        {
            if (_game.GameState == null || _game.LastRoom == null) return;
            if (_game.SELECTED_GAME == 0)
            {
                _game.GameState.PropertyChanged += GameState_PropertyChanged;
            }
            else if (_game.SELECTED_GAME == 1 || _game.SELECTED_GAME == 2)
            {
                _game.LastRoom.PropertyChanged += LastRoom_PropertyChanged;
            }
        }

        private void GameState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_game.GameState == null) return;

            var state = _game.GameState.Value & 0xFF000000;
            bool loading = state == 0x46000000 || state == 0x56000000 || state == 0x66000000;

            if (e.PropertyName == nameof(VariableData.Value) && loading)
            {
                foreach (var enemy in Tracking)
                {
                    if (enemy.Enemy == null) continue;
                    enemy.Enemy.Visibility = Visibility.Collapsed;
                    enemy.Enemy.MaxHealth = 0;
                }
            }
        }

        private void LastRoom_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_game.LastRoom == null) return;

            if (e.PropertyName == nameof(VariableData.Value) && _game.LastRoom.Value == 255)
            {
                foreach (var enemy in Tracking)
                {
                    if (enemy.Enemy == null) continue;
                    enemy.Enemy.Visibility = Visibility.Collapsed;
                    enemy.Enemy.MaxHealth = 0;
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
