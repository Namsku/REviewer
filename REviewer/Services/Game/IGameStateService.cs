using System;
using System.ComponentModel;

namespace REviewer.Services.Game
{
    public interface IGameStateService : INotifyPropertyChanged
    {
        int SelectedGame { get; }
        bool IsDead { get; }
        bool IsGameDone { get; }
        bool IsNewGame { get; }
        bool IsRetry { get; }
        int Deaths { get; }
        int Resets { get; }
        int Hits { get; }
        int Saves { get; }
        int Health { get; }
        int MaxHealth { get; }
        int CharacterId { get; }
        int CharacterState { get; }
        string CharacterName { get; }
        int MaxHealthCalculated { get; }

        void SetGame(int gameId);
        void UpdateState(int state, int? health, int? room, int? stage, int? cutscene, int? lastRoom, int? lastCutscene, long? gameSave);
        void UpdateCharacter(int characterId, int characterState, int maxHealth);
        void Initialize(Modules.RE.Json.Bio bio);
        void InitMonitoring(nint virtualMemoryPointer); // New method for setting up VariableData
        event EventHandler MonitoringInitialized;

        // Monitoring Data (VariableData)
        Modules.Utils.VariableData? GameStateData { get; }
        Modules.Utils.VariableData? HealthData { get; }
        Modules.Utils.VariableData? CharacterData { get; }
        Modules.Utils.VariableData? RoomData { get; }
        Modules.Utils.VariableData? StageData { get; }
        Modules.Utils.VariableData? InventorySlotSelectedData { get; }
        Modules.Utils.VariableData? CharacterHealthStateData { get; }
        
        Modules.Utils.VariableData? GameTimerData { get; }
        Modules.Utils.VariableData? GameFrameData { get; }
        Modules.Utils.VariableData? GameSaveData { get; }
        Modules.Utils.VariableData? LastRoomData { get; }
        Modules.Utils.VariableData? LastCutsceneData { get; }
        Modules.Utils.VariableData? CutsceneData { get; }
        Modules.Utils.VariableData? LastItemFoundData { get; }
        Modules.Utils.VariableData? InventoryCapacityUsedData { get; }
        Modules.Utils.VariableData? EnemyPointerData { get; }
        // Add others as needed
    }
}
