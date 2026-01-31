using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using REviewer.Modules.RE.Common;
using REviewer.Modules.RE.Enemies;
using REviewer.Modules.RE.Json;
using REviewer.Modules.RE;
using REviewer.Modules.Utils;
using REviewer.Core.Memory;

namespace REviewer.Services.Game
{
    public class GameMemoryService
    {
        private readonly IMemoryMonitor _memoryMonitor;

        public RootObject? GameData { get; private set; }
        public ObservableCollection<EnemyTracking>? EnemyTrackers { get; private set; }

        public GameMemoryService(IMemoryMonitor memoryMonitor)
        {
            _memoryMonitor = memoryMonitor;
        }

        public void InitializeMonitors(Process process, IntPtr virtualPtr, IntPtr productPtr)
        {
            _memoryMonitor.UpdateProcessHandle(process.Handle, process.ProcessName);
        }

        public void CreateGameData(string processName, int virtualPtr)
        {
            var dataLoader = new GameData(processName);
            var itemIDs = new ItemIDs(processName);
            GameData = dataLoader.GetGameData(itemIDs, virtualPtr);
            
            if (GameData != null)
            {
                _memoryMonitor.Register(GameData);
            }
        }

        public void SetEnemyTrackers(ObservableCollection<EnemyTracking> trackers)
        {
            EnemyTrackers = trackers;
            if (EnemyTrackers != null)
            {
                _memoryMonitor.Register(EnemyTrackers);
            }
        }

        public void StartMonitoring()
        {
            _memoryMonitor.Start();
        }

        public void StopMonitoring()
        {
            _memoryMonitor.Stop();
        }
    }
}
