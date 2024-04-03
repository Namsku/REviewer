using REviewer.Modules.RE.Common;
using REviewer.Modules.RE.Enemies;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace REviewer.Modules.Utils
{
    public class MonitorVariables
    {
        private readonly object _processHandleLock = new();
        private readonly object _lockObject = new();
        private readonly string _processName;
        private nint _processHandle;
        private volatile int _running = 1;
        private System.Threading.Timer? _monitoringTimer;
        private RootObject? _currentRootObject;
        private ObservableCollection<EnnemyTracking>? _enemyTracking;

        private const int MonitoringInterval = 55;
        private const int ByteSize = 1;
        private const int IntSize = 4;

        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

        public MonitorVariables(nint processHandle, string processName, IntPtr virtualMemoryPointer , IntPtr productPointer)
        {
            _processHandle = processHandle;
            _processName = processName;
        }

        public void Start(RootObject? rootObject)
        {
            if (rootObject == null)
            {
                Logger.Instance.Error("RootObject is null");
                return;
            }

            _currentRootObject = rootObject;
            StartMonitoring(rootObject);
        }

        public void Start(ObservableCollection<EnnemyTracking> enemyTrackings)
        {
            if (enemyTrackings == null)
            {
                Logger.Instance.Error("EnemyTrackings is null");
                return;
            }

            _enemyTracking = enemyTrackings;
            StartMonitoring(enemyTrackings);
        }

        public void Stop()
        {
            Logger.Instance.Info("Stopping monitoring");
            _monitoringTimer?.Dispose();
            _monitoringTimer = null;
            Interlocked.Exchange(ref _running, 0);
        }

        private void StartMonitoring(object monitoredObject)
        {
            _monitoringTimer = new System.Threading.Timer(state => Monitor(monitoredObject), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(MonitoringInterval));
            Logger.Instance.Info($"Started monitoring for {monitoredObject.GetType().Name}");
        }

        private void Monitor(object obj)
        {
            bool isProcessActive;
            lock (_processHandleLock)
            {
                isProcessActive = IsProcessActive();
                if (!isProcessActive)
                {
                    _running = 0;
                    _processHandle = 0;
                }
            }

            if (isProcessActive)
            {
                MonitorObject(obj);
            }
        }

        private void MonitorObject(object obj)
        {
            if (!CanMonitorObject(obj))
            {
                Logger.Instance.Debug($"Monitoring {obj} impossible");
                return;
            }

            var type = obj.GetType();
            var properties = _propertyCache.GetOrAdd(type, t => t.GetProperties());

            foreach (var property in properties)
            {
                if (!IsValidProperty(property))
                    continue;

                MonitorProperty(property, obj);
            }
        }

        private bool IsValidProperty(PropertyInfo property)
        {
            return property.PropertyType != typeof(string)
                   && property.PropertyType != typeof(int)
                   && property.Name != "KeyItemImages"
                   && property.PropertyType != typeof(System.Windows.Media.ImageSource)
                   && property.PropertyType != typeof(System.Windows.Media.SolidColorBrush)
                   && property.PropertyType != typeof(System.Windows.Media.Brush)
                   && property.PropertyType != typeof(System.Windows.Media.Brushes)
                   && property.PropertyType != typeof(List<string>);
        }

        private bool CanMonitorObject(object obj)
        {
            if (_processHandle == 0 || obj == null)
            {
                Stop();
                return false;
            }

            return true;
        }

        private void MonitorProperty(PropertyInfo property, object obj)
        {
            if (obj == null)
            {
                return;
            }

            lock (_lockObject)
            {
                if (obj is ObservableCollection<EnnemyTracking> enemyTrackings)
                {
                    foreach (var item in enemyTrackings)
                    {
                        MonitorObject(item);
                    }

                    return;
                }

                var value = property?.GetValue(obj);

                if (value is VariableData variableData)
                {
                    MonitorVariableData(variableData);
                }
                else if (value is IEnumerable enumerable && value is not string && value is not char)
                {
                    foreach (var item in enumerable)
                    {
                        MonitorObject(item);
                    }
                }
                else if (value != null && value is not string && value is not char)
                {
                    MonitorObject(value);
                }
            }
        }

        private bool IsProcessActive()
        {
            return Process.GetProcessesByName(_processName).Length > 0;
        }

        private void MonitorVariableData(VariableData variableData)
        {
            if (variableData.IsUpdated)
            {
                WriteVariableData(variableData, variableData.Value);
                variableData.IsUpdated = false;
            }
            else
            {
                variableData.Value = ReadVariableData(variableData);
            }
        }

        public int ReadVariableData(VariableData variableData)
        {
            byte[] buffer = ReadProcessMemory(variableData.Offset, (uint)variableData.Size);

            if (buffer == null) throw new InvalidOperationException("Buffer should not be null");

            return variableData.Size switch
            {
                IntSize => BitConverter.ToInt32(buffer, 0),
                ByteSize => buffer[0],
                _ => throw new InvalidOperationException("Invalid variable size"),
            };
        }

        public byte[] ReadProcessMemory(nint baseAddress, uint size)
        {
            byte[] buffer = new byte[size];
            bool success;
            lock (_processHandleLock)
            {
                success = ReadProcessMemory(_processHandle, baseAddress, buffer, size, out _);
            }

            if (!success)
            {
                // Logger.Instance.Error($"Failed to read process memory for base address {baseAddress:X} and size {size:X} for process {_processName} with handle {_processHandle}");
                // Stop();
                return buffer;
                // throw new InvalidOperationException("Failed to read process memory");
            }

            return buffer;
        }

        public bool WriteVariableData(VariableData variableData, int newValue)
        {
            byte[] buffer = variableData.Size switch
            {
                IntSize => BitConverter.GetBytes(newValue),
                ByteSize => new byte[] { (byte)newValue },
                _ => throw new InvalidOperationException("Invalid variable size"),
            };

            bool success;
            lock (_processHandleLock)
            {
                success = WriteProcessMemory(_processHandle, variableData.Offset, buffer, (uint)buffer.Length, out int bytesWritten);

                if (!success)
                {
                    Logger.Instance.Error($"Failed to write process memory for process {_processName} with handle {_processHandle}. Value: {newValue}, Bytes Written: {bytesWritten}");
                    return false;
                }

                return true;
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(nint processHandle, nint baseAddress, byte[] buffer, uint size, out int bytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(nint processHandle, nint baseAddress, [Out] byte[] buffer, uint size, out int bytesRead);

        internal void UpdateProcessHandle(nint handle)
        {
            _processHandle = handle;

            if (_currentRootObject != null)
            {
                Start(_currentRootObject);
            }
            else if (_enemyTracking != null)
            {
                Start(_enemyTracking);
            }
        }
    }
}

