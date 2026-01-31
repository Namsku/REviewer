using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using REviewer.Services.Game;
using REviewer.Core.Constants;
using REviewer.Core.Memory;
using REviewer.Modules.Utils;

namespace REviewer.Services.Enemy
{
    public class EnemyTrackingService : IEnemyTrackingService, INotifyPropertyChanged
    {
        private readonly IGameStateService _gameState;
        private readonly IMemoryMonitor _monitor;
        private Modules.RE.Enemies.Enemy? _selectedEnemy;
        private List<Modules.RE.Enemies.Enemy>? _enemies;
        
        // Tracking State
        private VariableData? _enemyHP;
        private VariableData? _enemyID;
        // We use _gameState.EnemyPointerData as the source reference

        // Sentinel Logic
        private int _lastValidPointer = 0;
        private DateTime _lastSetupTime = DateTime.MinValue;
        private DateTime _lastPurgeTime = DateTime.MinValue;
        private const int PURGE_DEBOUNCE_MS = 150;

        // Bestiaries (Simplified for now - can be moved to JSON/Resource)
        private Dictionary<byte, string> RE1_Bestiary = new Dictionary<byte, string> { { 0, "Zombie" }, { 6, "Hunter" }, { 12, "Tyrant" }, { 32, "Chris" }, { 33, "Jill" } };
        private Dictionary<byte, string> RE2_Bestiary = new Dictionary<byte, string> { { 16, "Zombie" }, { 34, "Licker" }, { 42, "Mr.X" }, { 48, "G1" }, { 80, "Leon" } };
        private Dictionary<byte, string> RE3_Bestiary = new Dictionary<byte, string> { { 16, "Zombie" }, { 52, "Nemesis" }, { 80, "Carlos" }, { 91, "Jill" } };

        public EnemyTrackingService(IGameStateService gameState, IMemoryMonitor monitor)
        {
            _gameState = gameState;
            _monitor = monitor;
            _selectedEnemy = new Modules.RE.Enemies.Enemy();
            
            // Subscribe to GameState changes if needed?
            // Actually UpdateEnemyData is called periodically.
        }

        public Modules.RE.Enemies.Enemy? SelectedEnemy
        {
            get => _selectedEnemy;
            set
            {
                 if (_selectedEnemy != value)
                 {
                     _selectedEnemy = value;
                     OnPropertyChanged(nameof(SelectedEnemy));
                 }
            }
        }
        
        public List<Modules.RE.Enemies.Enemy>? Enemies
        {
            get => _enemies;
            set
            {
                if (_enemies != value)
                {
                    _enemies = value;
                    OnPropertyChanged(nameof(Enemies));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void UpdateEnemyData(IntPtr processHandle, IntPtr memoryPointer)
        {
            if (_gameState.EnemyPointerData == null) return;
            
            // Subscribe to EnemyPointerData changes only? 
            // Or check value in loop? 
            // Since this method is called in loop, we check value.
            
            int pointerValue = _gameState.EnemyPointerData.Value;
            int gameId = _gameState.SelectedGame;
            
            if (gameId == GameConstants.BIOHAZARD_1)
            {
                // RE1 logic: Packed format? 
                // EnemyTracking.cs: EnemyState.Value >> 24 ...
                // If EnemyPointerData CONTAINS the data (not pointer):
                // RE1 usually had static address for "Enemy State".
                UpdateEnemyRE1(pointerValue);
            }
            else
            {
                // RE2/RE3 logic: Pointer based.
                UpdateEnemyRE2andRE3(pointerValue, gameId);
            }
        }
        
        private void UpdateEnemyRE1(int value)
        {
             if (_selectedEnemy == null) return;
             _selectedEnemy.CurrentHealth = value >> 24 & 0xFF;
             _selectedEnemy.Flag = value >> 16 & 0xFF;
             _selectedEnemy.Pose = value >> 8 & 0xFF;
             _selectedEnemy.Id = value & 0xFF; // ID logic
             
             if (RE1_Bestiary.TryGetValue((byte)_selectedEnemy.Id, out var name)) _selectedEnemy.Name = name;
             else _selectedEnemy.Name = "Unknown";
             
             // Max Health logic simplified
             if (_selectedEnemy.CurrentHealth > _selectedEnemy.MaxHealth) _selectedEnemy.MaxHealth = _selectedEnemy.CurrentHealth;
        }

        private void UpdateEnemyRE2andRE3(int pointerValue, int gameId)
        {
            // Logic ported from EnemyTracking.cs
            int positionHp = (gameId == GameConstants.BIOHAZARD_2) ? 0x156 : 0xCC;
            int positionId = (gameId == GameConstants.BIOHAZARD_2) ? 0x8 : 0x4A;
            
            // Sentinel checks (skipping exact pointer constants for brevity, assuming 0 or invalid)
            bool isSentinel = pointerValue == 0; 
            
            if (isSentinel)
            {
                PurgeEnemy();
            }
            else
            {
                SetupNewEnemy(pointerValue, positionHp, positionId, gameId);
                UpdateSelectedEnemyValues(gameId);
            }
        }
        
        private void PurgeEnemy()
        {
            if ((DateTime.Now - _lastSetupTime).TotalMilliseconds < PURGE_DEBOUNCE_MS) return;
            _lastPurgeTime = DateTime.Now;
            
            if (_enemyHP != null) { _monitor.UnregisterVariable("EnemyHP"); _enemyHP = null; }
            if (_enemyID != null) { _monitor.UnregisterVariable("EnemyID"); _enemyID = null; }
            
            if (_selectedEnemy != null) 
            {
                _selectedEnemy.Visibility = System.Windows.Visibility.Collapsed;
                _selectedEnemy.CurrentHealth = 0;
            }
        }
        
        private void SetupNewEnemy(int pointerValue, int hpOffset, int idOffset, int gameId)
        {
             if ((DateTime.Now - _lastPurgeTime).TotalMilliseconds < PURGE_DEBOUNCE_MS) return;
             if (pointerValue == _lastValidPointer && _enemyHP != null) return;
             
             _lastSetupTime = DateTime.Now;
             _lastValidPointer = pointerValue;
             
             // Unregister old
             if (_enemyHP != null) _monitor.UnregisterVariable("EnemyHP");
             if (_enemyID != null) _monitor.UnregisterVariable("EnemyID");
             
             // Register new
             _enemyHP = new VariableData(pointerValue + hpOffset, 4);
             _enemyID = new VariableData(pointerValue + idOffset, 1);
             
             _monitor.RegisterVariable("EnemyHP", _enemyHP);
             _monitor.RegisterVariable("EnemyID", _enemyID);
             
             if (_selectedEnemy != null) _selectedEnemy.Visibility = System.Windows.Visibility.Visible;
        }
        
        private void UpdateSelectedEnemyValues(int gameId)
        {
            if (_selectedEnemy == null) return;
            
            if (_enemyHP != null) _selectedEnemy.CurrentHealth = _enemyHP.Value & 0xFFFF;
            if (_enemyID != null) 
            {
                _selectedEnemy.Id = _enemyID.Value;
                var dict = (gameId == GameConstants.BIOHAZARD_2) ? RE2_Bestiary : RE3_Bestiary;
                _selectedEnemy.Name = dict.TryGetValue((byte)_enemyID.Value, out var name) ? name : "Unknown";
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
