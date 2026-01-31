using REviewer.Core.Memory;
using REviewer.Services.Game;
using REviewer.Services.Timer;

namespace REviewer.ViewModels
{
    public class OverlayViewModel : ViewModelBase
    {
        private readonly IGameStateService _gameStateService;
        private readonly ITimerService _timerService;

        public IGameStateService GameState => _gameStateService;
        public ITimerService Timer => _timerService;

        public OverlayViewModel(IGameStateService gameStateService, ITimerService timerService)
        {
            _gameStateService = gameStateService;
            _timerService = timerService;
        }
    }
}
