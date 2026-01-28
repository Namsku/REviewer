using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using REviewer.Modules.RE.Common;
using REviewer.Modules.RE.Enemies;
using REviewer.Modules.RE.Json;
using REviewer.Modules.RE;
using REviewer.Modules.Utils;

namespace REviewer.Services
{
    public class GameMemoryService
    {
        private MonitorVariables? _variableMonitor;
        private MonitorVariables? _enemyMonitor;

        public RootObject? GameData { get; private set; }
        public ObservableCollection<EnemyTracking>? EnemyTrackers { get; private set; }

        public void InitializeMonitors(Process process, IntPtr virtualPtr, IntPtr productPtr)
        {
            if (_variableMonitor == null)
            {
                _variableMonitor = new MonitorVariables(process.Handle, process.ProcessName, virtualPtr, productPtr);
            }
            else
            {
                _variableMonitor.UpdateProcessHandle(process.Handle);
            }

            if (_enemyMonitor == null)
            {
                _enemyMonitor = new MonitorVariables(process.Handle, process.ProcessName, virtualPtr, productPtr);
            }
            else
            {
                _enemyMonitor.UpdateProcessHandle(process.Handle);
            }
        }

        public void CreateGameData(string processName, int virtualPtr)
        {
            var dataLoader = new GameData(processName);
            var itemIDs = new ItemIDs(processName);
            GameData = dataLoader.GetGameData(itemIDs, virtualPtr);
        }

        public void SetEnemyTrackers(ObservableCollection<EnemyTracking> trackers)
        {
            EnemyTrackers = trackers;
        }

        public void StartMonitoring()
        {
            if (GameData != null)
            {
                _variableMonitor?.Start(GameData);
            }

            if (EnemyTrackers != null)
            {
                _enemyMonitor?.Start(EnemyTrackers);
            }
        }

        public void StopMonitoring()
        {
            _variableMonitor?.Stop();
            _enemyMonitor?.Stop();
        }
    }
}
