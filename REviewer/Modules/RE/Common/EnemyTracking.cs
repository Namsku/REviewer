using Newtonsoft.Json.Linq;
using NLog;
using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace REviewer.Modules.RE.Common
{
    public class EnnemyTracking : INotifyPropertyChanged
    {
        Dictionary<byte, string> RE1_Bestiary = new Dictionary<byte, string>
        {
            { 0, "Zombie" },
            { 1, "Zombie N" },
            { 2, "Doggo" },
            { 3, "Spider" },
            { 4, "B. Tiger" },
            { 5, "Crow" },
            { 6, "Hunter" },
            { 7, "Wasp" },
            { 8, "Plant 42" },
            { 9, "Chimera" },
            { 10, "Snake" },
            { 11, "Neptune" },
            { 12, "Tyran" },
            { 13, "Yawn 1" },
            { 14, "Plant42 R" },
            { 15, "Plant42 V" },
            { 16, "Sr. Tyran" },
            { 17, "Zombie R" },
            { 18, "Yawn 2" },
            { 19, "Cobweb" },
            { 20, "Hands L" },
            { 21, "Hands R" },
            { 32, "Chris" },
            { 33, "Jill" },
            { 34, "Barry" },
            { 35, "Rebecca" },
            { 36, "Wesker" },
            { 37, "Kenneth" },
            { 38, "Forrest" },
            { 39, "Richard" },
            { 40, "Enrico" },
            { 41, "Kenneth" },
            { 42, "Barry" },
            { 43, "Barry" },
            { 44, "Rebecca" },
            { 45, "Barry" },
            { 46, "Wesker" },
            { 47, "Chris" },
            { 48, "Jill" },
            { 49, "Chris" },
            { 50, "Jill" }
        };

        public Dictionary<byte, string> RE2_Bestiary = new Dictionary<byte, string>
        {
            {16, "Zombie"},
            {17, "Brad"},
            {18, "Zombie M"},
            {19, "Zombie F"},
            {21, "Zombie T"},
            {22, "Zombie S"},
            {23, "Zombie N"},
            {24, "Zombie G"},
            {30, "Zombie G"},
            {31, "Zombie R"},
            {32, "Doggo"},
            {33, "Crow"},
            {34, "Licker"},
            {35, "Croco"},
            {36, "Licker"},
            {37, "Spider"},
            {38, "Spider"},
            {39, "G-Embr."},
            {40, "G-Adult"},
            {41, "Insect"},
            {42, "Mr.X"},
            {43, "Sr.X"},
            {45, "Arms"},
            {46, "Ivy"},
            {47, "Vines"},
            {48, "G1"},
            {49, "G2"},
            {50, "G3"},
            {51, "G4"},
            {52, "G5"},
            {57, "Ivy P"},
            {58, "G Moth"},
            {59, "Maggots"},
            {64, "Irons"},
            {65, "Ada"},
            {66, "Irons"},
            {67, "Ada"},
            {68, "Ben"},
            {69, "Sherry"},
            {70, "Ben"},
            {71, "Annette"},
            {72, "Kendo"},
            {73, "Annette"},
            {74, "Marvin"},
            {75, "Mayors"},
            {79, "Sherry"},
            {80, "Leon"},
            {81, "Claire"},
            {84, "Leon"},
            {85, "Claire"},
            {88, "Leon"},
            {89, "Claire"},
            {90, "Leon"}
        };


        public int SelectedGame;

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

        private VariableData? _enemyHP;

        public VariableData? EnemyHP
        {
            get { return _enemyHP; }
            set
            {
                if (_enemyHP != value)
                {
                    if (_enemyHP != null)
                    {
                        _enemyHP.PropertyChanged -= EnemyHP_PropertyChanged;
                    }

                    _enemyHP = value;

                    if (_enemyHP != null)
                    {
                        _enemyHP.PropertyChanged += EnemyHP_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(EnemyHP));
                }
            }
        }

        private int? _enemyMaxHP;

        public int? EnemyMaxHP
        {
            get { return _enemyMaxHP; }
            set
            {
                if (_enemyMaxHP != value)
                {
                    _enemyMaxHP = value;
                    OnPropertyChanged(nameof(EnemyMaxHP));
                }
            }
        }

        private VariableData? _enemyID;

        public VariableData? EnemyID
        {
            get { return _enemyID; }
            set
            {
                if (_enemyID != value)
                {
                    if (_enemyID != null)
                    {
                        _enemyID.PropertyChanged -= EnemyID_PropertyChanged;
                    }

                    _enemyID = value;

                    if (_enemyID != null)
                    {
                        _enemyID.PropertyChanged += EnemyID_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(EnemyID));
                }
            }
        }

        private void EnemyHP_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (SelectedGame == 1)
                {
                    // Take only the first 2 bytes
                    Enemy.CurrentHealth = EnemyHP.Value & 0xFFFF;

                    if (EnemyMaxHP == 0)
                    {
                        Enemy.MaxHealth = Enemy.CurrentHealth;
                        EnemyMaxHP = Enemy.CurrentHealth;
                    }

                    OnPropertyChanged(nameof(Enemy));
                }
                    
            }
        }

        private void EnemyMaxHP_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if (SelectedGame == 1)
                {
                    // Enemy.MaxHealth = EnemyMaxHP.Value & 0x0000FFFF;
                    // OnPropertyChanged(nameof(Enemy));
                }
            }
        }

        private void EnemyID_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {
                if(SelectedGame == 0)
                {                    
                    Enemy.Name = RE1_Bestiary.TryGetValue((byte)EnemyID.Value, out string enemyName) ? enemyName : "Unknown";
                    OnPropertyChanged(nameof(Enemy));
                }
                else if (SelectedGame == 1)
                {
                    Enemy.Name = RE2_Bestiary.TryGetValue((byte)EnemyID.Value, out string enemyName) ? enemyName : "Unknown";
                    OnPropertyChanged(nameof(Enemy));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }  

        public EnnemyTracking(nint v, StandardProperty property, int selectedGame)
        {
            EnemyState = new VariableData(v, property);
            Enemy = new Enemy();
            SelectedGame = selectedGame;

            if (SelectedGame == 0) 
            { 
                EnemyID = new VariableData(v - 132, 1);
            }
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
                if (SelectedGame == 0)
                    UpdateEnemy();
                else if (SelectedGame == 1)
                    UpdateEnemyRE2();
            }
        }

        private void UpdateEnemyRE2()
        {
            if (_enemyState == null) return;

            if (_enemyState.Value == 0x98E544)
            {
                EnemyHP = null;
                EnemyID = null;
                EnemyMaxHP = 0;

                Enemy.Visibility = Visibility.Collapsed;
            } 
            else
            {
                EnemyHP = new VariableData(_enemyState.Value + 0x156, 4);
                EnemyID = new VariableData(_enemyState.Value + 0x8, 1);
                EnemyMaxHP = 0;

                Enemy.Visibility = Visibility.Visible;
            }
        }
    }
}
