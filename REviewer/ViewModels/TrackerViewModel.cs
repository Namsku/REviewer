using REviewer.Core.Memory;
using REviewer.Services.Game;
using REviewer.Services.Timer;
using REviewer.Services.Inventory;

namespace REviewer.ViewModels
{
    public class TrackerViewModel : ViewModelBase
    {
        private readonly IGameStateService _gameStateService;
        private readonly IInventoryService _inventoryService;

        public IGameStateService GameState => _gameStateService;
        public IInventoryService Inventory => _inventoryService;

        // Tracker might need access to EnemyTrackingService later
        // For now, minimal dependencies

        public TrackerViewModel(IGameStateService gameStateService, IInventoryService inventoryService)
        {
            _gameStateService = gameStateService;
            _inventoryService = inventoryService;
        }
    }
}
