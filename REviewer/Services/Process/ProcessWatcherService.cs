using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using REviewer.Modules.Utils;

namespace REviewer.Services.Processes
{
    public class ProcessWatcherService : IDisposable
    {
        private System.Threading.Timer? _timer;
        private Process? _process;
        private bool _isProcessRunning;
        private string _gameKey;
        private readonly object _lock = new object();

        public event EventHandler<Process>? OnProcessFound;
        public event EventHandler? OnProcessExited;

        public ProcessWatcherService()
        {
            _gameKey = string.Empty;
        }

        public void SetGameKey(string gameKey)
        {
            lock (_lock)
            {
                _gameKey = gameKey;
                // Optionally reset state
                _isProcessRunning = false;
                _process = null;
            }
        }

        public void Start()
        {
            _timer = new System.Threading.Timer(ScanForProcess, null, 0, 1000);
        }

        public void Stop()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void ScanForProcess(object? state)
        {
            lock (_lock)
            {
                if (string.IsNullOrEmpty(_gameKey)) return;

                if (_isProcessRunning)
                {
                    if (_process == null || _process.HasExited)
                    {
                        HandleProcessExit();
                    }
                    return;
                }

                var processNames = Library.GetGameVersions(_gameKey);
                foreach (var name in processNames)
                {
                    var processes = Process.GetProcessesByName(name);
                    if (processes.Length > 0)
                    {
                        var process = processes[0];
                        // Validate MD5 if needed, similar to MainWindow logic
                        // For now we assume found.
                        
                        _process = process;
                        _isProcessRunning = true;
                        _process.EnableRaisingEvents = true;
                        _process.Exited += (s, e) => HandleProcessExit();

                        OnProcessFound?.Invoke(this, _process);
                        break;
                    }
                }
            }
        }

        private void HandleProcessExit()
        {
            lock (_lock)
            {
                _isProcessRunning = false;
                _process = null;
                OnProcessExited?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _process?.Dispose();
        }
    }
}
