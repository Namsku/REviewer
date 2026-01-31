using System;

namespace REviewer.Services.Enemy
{
    public interface IEnemyTrackingService
    {
        void UpdateEnemyData(IntPtr processHandle, IntPtr memoryPointer);
    }
}
