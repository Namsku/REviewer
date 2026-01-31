using Xunit;
using REviewer.Services;
using REviewer.Services.Timer;
using REviewer.Core.Constants;
using System;

namespace REviewer.Tests.Services
{
    public class TimerServiceTests
    {
        [Fact]
        public void UpdateTimer_RE1_ShouldFormatCorrectly()
        {
            // Arrange
            var service = new TimerService();
            // 30 fps
            // 90 frames = 3 seconds
            long timerValue = 90; 

            // Act
            service.UpdateTimer(GameConstants.BIOHAZARD_1, timerValue, null, null, false, 0);

            // Assert
            Assert.Equal(TimeSpan.FromSeconds(3), service.CurrentIGT);
            Assert.Equal("00:00:03.00", service.IGTHumanFormat);
        }

        [Fact]
        public void UpdateTimer_RE2_Running_ShouldFormatCorrectly()
        {
            // Arrange
            var service = new TimerService();
            // RE2 uses timer + frame/60.0
            // Timer = 10 seconds, Frame = 30 (0.5s)
            long timerValue = 10;
            long frameValue = 30;

            // Act
            service.UpdateTimer(GameConstants.BIOHAZARD_2, timerValue, frameValue, null, false, 0);

            // Assert
            Assert.Equal(TimeSpan.FromSeconds(10.5), service.CurrentIGT);
            Assert.Equal("00:00:10.50", service.IGTHumanFormat);
        }

        [Fact]
        public void UpdateTimer_RE2_GameDone_ShouldUseFinalTime()
        {
            // Arrange
            var service = new TimerService();
            double finalTime = 120.5; // 2 min 0.5 sec

            // Act
            service.UpdateTimer(GameConstants.BIOHAZARD_2, 0, 0, null, true, finalTime);

            // Assert
            Assert.Equal(TimeSpan.FromSeconds(120.5), service.CurrentIGT);
            // 120.5s = 2m 0.5s
            Assert.Equal("00:02:00.50", service.IGTHumanFormat);
        }

        [Fact]
        public void UpdateTimer_CVX_ShouldFormatCorrectly()
        {
            // Arrange
            var service = new TimerService();
            // CVX is 60fps I think? Based on Game.cs: GameTimer.Value / 60.0
            // 120 frames = 2 seconds
            long timerValue = 120;

            // Act
            service.UpdateTimer(GameConstants.BIOHAZARD_CVX, timerValue, null, null, false, 0);

            // Assert
            Assert.Equal(TimeSpan.FromSeconds(2), service.CurrentIGT);
            Assert.Equal("00:00:02.00", service.IGTHumanFormat);
        }
    }
}
