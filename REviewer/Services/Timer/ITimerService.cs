using System;
using System.ComponentModel;

namespace REviewer.Services.Timer
{
    public interface ITimerService : INotifyPropertyChanged
    {
        TimeSpan CurrentIGT { get; }
        string IGTHumanFormat { get; }
        void UpdateTimer(int gameId, long? timerValue, long? frameValue, long? gameSave, bool isGameDone, double finalTime);
    }
}
