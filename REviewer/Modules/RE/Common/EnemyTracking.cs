using REviewer.Modules.RE.Json;
using REviewer.Modules.Utils;
using System.ComponentModel;
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
            { 14, "Plant R" },
            { 15, "Plant V" },
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
            {19, "Misty"},
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
            {43, "Uber X"},
            {45, "Arms"},
            {46, "Ivy"},
            {47, "Vines"},
            {48, "G1"},
            {49, "G2"},
            {50, "G3"},
            {51, "G4"},
            {52, "G4"},
            {53, "G5"},
            {54, "G5"},
            {55, "G5 Arms"},
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

        public Dictionary<byte, string> RE3_Bestiary = new Dictionary<byte, string>
        {
            { 16, "Zombie" },
            { 17, "Zombie" },
            { 18, "Zombie" },
            { 19, "Zombie G" },
            { 20, "Zombie R" },
            { 21, "Zombie G" },
            { 22, "Zombie G" },
            { 23, "Zombie G" },
            { 24, "Zombie N" },
            { 25, "Zombie 5" },
            { 26, "Zombie 6" },
            { 27, "Zombie L" },
            { 28, "Zombie G" },
            { 29, "Zombie P" },
            { 30, "Zombie G" },
            { 31, "Zombie G" },
            { 32, "Doggo" },
            { 33, "Crow" },
            { 34, "Hunter" },
            { 35, "Brain S." },
            { 36, "Hunter G" },
            { 37, "Spider" },
            { 38, "Spidy" },
            { 39, "Brain S." },
            { 40, "Brain S." },
            { 44, "UMB.S" },
            { 45, "Arm" },
            { 47, "Marvin" },
            { 48, "G.Digger" },
            { 50, "S.Worm" },
            { 52, "Nemmy" },
            { 53, "Nemmy 2" },
            { 54, "Nemmy 3" },
            { 55, "Nemmy 3" },
            { 56, "Nemmy 4" },
            { 57, "Nemmy 4" },
            { 58, "Nem. Up" },
            { 59, "Jill KO" },
            { 63, "Helicop." },
            { 64, "Nemmy KO" },
            { 80, "Carlos" },
            { 81, "Mikhail" },
            { 82, "Nichol." },
            { 83, "Brad" },
            { 84, "Dario" },
            { 85, "Murphy" },
            { 86, "Tyrel" },
            { 87, "Marvin" },
            { 88, "Brad" },
            { 89, "Dario" },
            { 90, "P.Girl" },
            { 91, "Jill" },
            { 92, "Carlos" },
            { 95, "Jill" },
            { 96, "Nichol." },
            { 103, "Irons" }
        };

        public Dictionary<byte, string> RECVX_Bestiary = new Dictionary<byte, string> 
        {
            { 254, "Unknown" },
            { 255, "None" },
            { 1, "Zombie" },
            { 2, "GlupWorm" },
            { 3, "Spider" },
            { 4, "Doggo" },
            { 5, "Hunter" },
            { 6, "Moth" },
            { 7, "Bat" },
            { 9, "B.snatch" },
            { 12, "Alexia" },
            { 13, "Alexia B" },
            { 14, "Alexia C" },
            { 15, "Nosferatu" },
            { 17, "Mr.Steve" },
            { 19, "Tyrant" },
            { 21, "Albinoid C." },
            { 22, "Albinoid A." },
            { 23, "Big Spider" },
            { 26, "Zombie" },
            { 29, "Tenticle" },
            { 30, "Y.Alexia" }
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

        private VariableData? _enemySelected;
        private VariableData? EnemySelected
        {
            get { return _enemySelected; }
            set
            {
                if (_enemySelected != null)
                {
                    _enemySelected.PropertyChanged -= EnemySelected_PropertyChanged;
                }

                _enemyHP = value;

                if (_enemySelected != null)
                {
                    _enemySelected.PropertyChanged += EnemySelected_PropertyChanged;
                }

                OnPropertyChanged(nameof(EnemySelected));
            }
        }

        private void EnemySelected_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableData.Value))
            {

            }
        }

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
                if (SelectedGame > 0)
                {
                    // Take only the first 2 bytes
                    Enemy.CurrentHealth = EnemyHP.Value & 0xFFFF;

                    if (EnemyMaxHP == 0)
                    {
                        Enemy.MaxHealth = Enemy.CurrentHealth;

                        if(SelectedGame == 2)
                        {
                            EnemyMaxHP = ((EnemyHP.Value >> 16) & 0xFFFF);
                        }
                        else 
                        { 
                            var hp = Enemy.CurrentHealth;
                            if (hp > 60000 || hp < 2000 || ( hp > 20000 && hp < 21000))
                                EnemyMaxHP = Enemy.CurrentHealth;
                        }
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
                    if (Enemy.Name == "Unknown")
                    {
                        Logger.Instance.Error($"RE1 - Enemy ID -> {EnemyID.Value} not Found");
                    }
                }
                else if (SelectedGame == 1)
                {
                    Enemy.Name = RE2_Bestiary.TryGetValue((byte)EnemyID.Value, out string enemyName) ? enemyName : "Unknown";
                    if (Enemy.Name == "Unknown")
                    {
                        Logger.Instance.Error($"RE2 - Enemy ID -> {EnemyID.Value} not Found");
                    }
                    OnPropertyChanged(nameof(Enemy));
                }
                else if (SelectedGame == 2)
                {
                    // Console.WriteLine($"Enemy ID -> {EnemyID.Value}");
                    Enemy.Name = RE3_Bestiary.TryGetValue((byte)EnemyID.Value, out string enemyName) ? enemyName : "Unknown";
                    if (Enemy.Name == "Unknown")
                    {
                        Logger.Instance.Error($"RE3 - Enemy ID -> {EnemyID.Value} not Found");
                    }

                    OnPropertyChanged(nameof(Enemy));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }  

        public EnnemyTracking(int v, StandardProperty property, int selectedGame)
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

            if (Enemy.CurrentHealth == 255 && Enemy.Id < 30)
            {
                Enemy.MaxHealth = 0;
                return;
            }

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
                {
                    UpdateEnemy();
                }
                else
                {
                    UpdateEnemyRE2andRE3();
                }
            }
        }

        private void UpdateEnemyRE2andRE3()
        {
            if (_enemyState == null) return;
            int position_hp = SelectedGame == 1 ? 0x156 : 0xCC;
            int position_id = SelectedGame == 1 ? 0x8 : 0x4a;

            // Console.WriteLine($"{position_hp} - {position_id} - {Library.ToHexString(_enemyState.Value)}");

            if (_enemyState.Value == 0x98E544 || _enemyState.Value == 0x0A62290 )
            {
                // Console.WriteLine($"Purging");
                EnemyHP = null;
                EnemyID = null;
                EnemyMaxHP = 0;

                Enemy.Visibility = Visibility.Collapsed;
            } 
            else
            {
                // Console.WriteLine($"New Enemy Detected");
                EnemyHP = new VariableData(_enemyState.Value + position_hp, 4);
                EnemyID = new VariableData(_enemyState.Value + position_id, 1);
                EnemyMaxHP = 0;

                Enemy.Visibility = Visibility.Visible;
            }
        }
    }
}
