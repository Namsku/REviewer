using System;
using System.ComponentModel;
using REviewer.Services.Game;
using REviewer.Services.Inventory;
using REviewer.Core.Memory;

namespace REviewer.Services.Challenge
{
    public class ChallengeService : IChallengeService
    {
        private readonly IGameStateService _gameState;
        private readonly IInventoryService _inventory;
        private readonly IMemoryMonitor _monitor;
        private int _lastHits;

        public ChallengeService(IGameStateService gameState, IInventoryService inventory, IMemoryMonitor monitor)
        {
            _gameState = gameState;
            _inventory = inventory;
            _monitor = monitor;

            _gameState.PropertyChanged += OnGameStateChanged;
            _inventory.ItemBoxAccessed += OnItemBoxAccessed;
        }

        public bool IsOneHPEnabled { get; set; }
        public bool IsNoDamageEnabled { get; set; }
        public bool IsNoItemBoxEnabled { get; set; }
        public bool IsNoSaveEnabled { get; set; }

        public bool HasTakenDamage { get; private set; }
        public bool HasUsedItemBox { get; private set; }
        public int DamageTakenCount { get; private set; }

        public event EventHandler? ChallengeFailed;

        public void Reset()
        {
            HasTakenDamage = false;
            HasUsedItemBox = false;
            DamageTakenCount = 0;
            _lastHits = 0;
        }

        public void OnDamageTaken(int amount)
        {
            if (amount > 0)
            {
                HasTakenDamage = true;
                DamageTakenCount += amount;
                if (IsNoDamageEnabled) ChallengeFailed?.Invoke(this, EventArgs.Empty);
            }
        }

        public void OnItemBoxUsed()
        {
            HasUsedItemBox = true;
            if (IsNoItemBoxEnabled) ChallengeFailed?.Invoke(this, EventArgs.Empty);
        }

        private void OnGameStateChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IGameStateService.Health))
            {
                HandleHealthChange();
            }
            if (e.PropertyName == nameof(IGameStateService.Hits))
            {
                HandleHitsChange();
            }
        }

        private void HandleHealthChange()
        {
            // OneHP Logic
            if (IsOneHPEnabled)
            {
                if (_gameState.Health > 1)
                {
                   // Enforce 1 HP
                   // We use GameStateData instead of HealthData directly? No, HealthData.
                   // IGameStateService exposes HealthData.
                   if (_gameState.HealthData != null)
                   {
                       _monitor.WriteVariableData(_gameState.HealthData, 1);
                   }
                }
            }
        }

        private void HandleHitsChange()
        {
            int currentHits = _gameState.Hits;
            if (currentHits > _lastHits)
            {
                int diff = currentHits - _lastHits;
                // OnDamageTaken logic
                OnDamageTaken(1); // Hits usually increment by 1. Amount? GameState doesn't verify amount per hit.
                
                _lastHits = currentHits;
            }
        }
        
        private void OnItemBoxAccessed(object? sender, EventArgs e)
        {
            OnItemBoxUsed();
        }
    }
}
