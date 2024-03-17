﻿using Newtonsoft.Json.Linq;
using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace REviewer.Modules.RE.Common
{
    public class EnnemyTracking : INotifyPropertyChanged
    {
        private Enemy? _enemy;
        public Enemy? Enemy
        {
            get { return _enemy; }
            set
            {
                if (_enemy != value)
                {
                    _enemy = value;
                    OnPropertyChanged(nameof(Enemy));
                }
            }
        }
        private VariableData? _enemyState;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }  

        public EnnemyTracking(nint v, StandardProperty property)
        {
            EnemyState = new VariableData(v, property);
            Enemy = new Enemy();
        }

        public VariableData? EnemyState
        {
            get { return _enemyState; }
            set {
                if (_enemyState != value)
                {
                    if (_enemyState != null)
                    {
                        EnemyState.PropertyChanged -= EnemyState_PropertyChanged;
                    }

                    _enemyState = value;

                    if (_enemyState != null)
                    {
                        EnemyState.PropertyChanged += EnemyState_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(EnemyState));
                }
            }
        }

        private void UpdateEnemy()
        {
            if (Enemy == null || EnemyState == null) return;
            if (Enemy.Visibility == Visibility.Collapsed && Enemy.CurrentHealth != 255)
            {
                Enemy.Visibility = Visibility.Visible;
                return;
            }

            Enemy.CurrentHealth = EnemyState.Value >> 24 & 0xFF;
            Enemy.Flag = (EnemyState.Value >> 16) & 0xFF;
            Enemy.Pose = (EnemyState.Value >> 8) & 0xFF;
            Enemy.Id = EnemyState.Value & 0xFF;

            if (Enemy.CurrentHealth > Enemy.MaxHealth)
            {
                Enemy.MaxHealth = Enemy.CurrentHealth;
            }

            if (Enemy.CurrentHealth == 255 && Enemy.Visibility == Visibility.Visible)
            {
                Enemy.Visibility = Visibility.Collapsed;
                return;
            }

            // Console.WriteLine($"Enemy: {Enemy.CurrentHealth} - {Enemy.Flag} - {Enemy.Pose} - {Enemy.Id}");

            OnPropertyChanged(nameof(Enemy));
        }

        private void EnemyState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                UpdateEnemy(); 
            }
        }
    }
}