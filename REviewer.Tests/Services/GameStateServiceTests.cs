using Xunit;
using REviewer.Services;
using REviewer.Services.Game;
using REviewer.Core.Constants;

namespace REviewer.Tests.Services
{
    public class GameStateServiceTests
    {
        [Fact]
        public void SetGame_ShouldResetState()
        {
            // Arrange
            var service = new GameStateService();
            service.Deaths = 5;
            service.Resets = 3;

            // Act
            service.SetGame(GameConstants.BIOHAZARD_2);

            // Assert
            Assert.Equal(GameConstants.BIOHAZARD_2, service.SelectedGame);
            Assert.Equal(0, service.Deaths);
            Assert.Equal(0, service.Resets);
            Assert.False(service.IsDead);
            Assert.False(service.IsGameDone);
        }

        [Fact]
        public void UpdateState_RE1_ShouldDetectDeath()
        {
            // Arrange
            var service = new GameStateService();
            service.SetGame(GameConstants.BIOHAZARD_1);
            
            // Initial state (alive)
            // 0x1000000 is death state in RE1 logic from Game.cs: (state & 0x0F000000) == 0x1000000
            // Previous state must not be death.
            service.UpdateState(0x00000000, null, null, null, null, null, null, null);

            // Act - Transition to death
            service.UpdateState(0x01000000, null, null, null, null, null, null, null);

            // Assert
            Assert.True(service.IsDead);
            Assert.Equal(1, service.Deaths);
        }

        [Fact]
        public void UpdateState_RE2_ShouldDetectDeath()
        {
            // Arrange
            var service = new GameStateService();
            service.SetGame(GameConstants.BIOHAZARD_2);
            
            // RE2 Death Logic: Health.Value > 1000 && state != 0 && OldHealth > 65000
            
            // 1. Set OldHealth by updating with high health first
            service.UpdateState(0x00000001, 65001, null, null, null, null, null, null);

            // Act - Update with death conditions
            // State != 0, Health > 1000
            service.UpdateState(0x00000001, 1001, null, null, null, null, null, null);

            // Assert
            Assert.True(service.IsDead);
            Assert.Equal(1, service.Deaths);
        }
    }
}
