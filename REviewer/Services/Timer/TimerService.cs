using REviewer.Services.Timer;
using REviewer.Core.Constants;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace REviewer.Services.Timer
{
    public class TimerService : ITimerService
    {
        private TimeSpan _currentIGT;
        public TimeSpan CurrentIGT
        {
            get => _currentIGT;
            private set => SetField(ref _currentIGT, value);
        }

        private string _igtHumanFormat;
        public string IGTHumanFormat
        {
            get => _igtHumanFormat;
            private set => SetField(ref _igtHumanFormat, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void UpdateTimer(int gameId, long? timerValue, long? frameValue, long? gameSave, bool isGameDone, double finalTime)
        {
            if (gameId == GameConstants.BIOHAZARD_1)
            {
                 // Frames / 30.0
                 double seconds = (timerValue ?? 0) / 30.0;
                 CurrentIGT = TimeSpan.FromSeconds(seconds);
                 IGTHumanFormat = CurrentIGT.ToString(@"hh\:mm\:ss\.ff");
            }
            else if (gameId == GameConstants.BIOHAZARD_2)
            {
                if (isGameDone)
                {
                    CurrentIGT = TimeSpan.FromSeconds(finalTime);
                }
                else
                {
                    double seconds = (double)(timerValue ?? 0) + ((frameValue ?? 0) / 60.0);
                    CurrentIGT = TimeSpan.FromSeconds(seconds);
                }
                IGTHumanFormat = CurrentIGT.ToString(@"hh\:mm\:ss\.ff");
            }
            else if (gameId == GameConstants.BIOHAZARD_3)
            {
                // Simplified logic for now, complex Rebirth logic to be properly integrated
                // Assuming basic frame calculation 
                // TODO: Implement full RE3 logic (Rebirth vs China)
                
                if (gameSave.HasValue && gameSave.Value != 0)
                {
                     // Placeholder for Rebirth save state logic
                     double seconds = (gameSave.Value) / 60.0;
                     CurrentIGT = TimeSpan.FromSeconds(seconds);
                }
                else
                {
                     CurrentIGT = TimeSpan.Zero;
                }
                IGTHumanFormat = CurrentIGT.ToString(@"hh\:mm\:ss\.ff");
            }
            else if (gameId == GameConstants.BIOHAZARD_CVX)
            {
                double seconds = (timerValue ?? 0) / 60.0;
                CurrentIGT = TimeSpan.FromSeconds(seconds);
                IGTHumanFormat = CurrentIGT.ToString(@"hh\:mm\:ss\.ff");
            }
            else
            {
                CurrentIGT = TimeSpan.Zero;
                IGTHumanFormat = "00:00:00.00";
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
