using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using REviewer.Modules.RE.Common;
using REviewer.Modules.Utils;
using static REviewer.Modules.RE.GameData;

namespace REviewer.Modules.Utils
{
    public class MonitorVariables(nint processHandle, string processName)
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(nint processHandle, nint baseAddress, byte[] buffer, uint size, out int bytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(nint processHandle, nint baseAddress, [Out] byte[] buffer, uint size, out int bytesRead);

        private const int BYTE_SIZE = 1;
        private const int INT_SIZE = 4;
        private const int MONITORING_INTERVAL = 55;

        private System.Threading.Timer? _monitoringTimer;
        private nint _processHandle = processHandle;
        private readonly string _processName = processName;
        private readonly object _lockObject = new();
        private readonly object _processHandleLock = new();
        private volatile int _running = 1;

        private RootObject? _currentRootObject;
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

        private bool CanMonitorObject(object obj)
        {
            if (_processHandle == 0 || obj == null)
            {
                Stop();
                return false;
            }

            return true;
        }

        private bool IsProcessActive()
        {
            return Process.GetProcessesByName(_processName).Length > 0;
        }

        public void Monitor()
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

            if (isProcessActive && _currentRootObject != null)
            {
                MonitorObject(_currentRootObject);
            }
        }

        private void MonitorObject(object obj)
        {
            if (!CanMonitorObject(obj))
            {
                return;
            }

            var type = obj.GetType();
            var properties = _propertyCache.GetOrAdd(type, t => t.GetProperties());

            foreach (var property in properties)
            {
                MonitorProperty(property, obj);
            }
        }

        private void MonitorProperty(PropertyInfo property, object obj)
        {
            lock (_lockObject)
            {
                var value = property.GetValue(obj);

                if (value is VariableData variableData)
                {
                    MonitorVariableData(variableData);
                }
                else if (value is IEnumerable enumerable && !(value is string))
                {
                    foreach (var item in enumerable)
                    {
                        MonitorObject(item);
                    }
                }
                else if (value != null)
                {
                    MonitorObject(value);
                }
            }
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
            if (buffer == null)
            {
                Logger.Instance.Error($"Failed to read process memory for process {_processName} with handle {_processHandle}");
                throw new InvalidOperationException("Failed to read process memory");
            }

            return variableData.Size switch
            {
                INT_SIZE => BitConverter.ToInt32(buffer, 0),
                BYTE_SIZE => buffer[0],
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
                Logger.Instance.Error($"Failed to read process memory for base address {baseAddress:X} and size {size:X} for process {_processName} with handle {_processHandle}");
                throw new InvalidOperationException("Failed to read process memory");
            }

            return buffer;
        }

        public void Start(RootObject? rootObject)
        {
            if (rootObject == null)
            {
                Logger.Instance.Error("RootObject is null");
                return;
            }

            _currentRootObject = rootObject;
            _monitoringTimer = new System.Threading.Timer(state => Monitor(), rootObject, TimeSpan.Zero, TimeSpan.FromMilliseconds(MONITORING_INTERVAL));
            Logger.Instance.Info("Started monitoring");
        }

        public void Stop()
        {
            Logger.Instance.Info("Stopping monitoring");
            _monitoringTimer?.Dispose();
            _monitoringTimer = null;
            Interlocked.Exchange(ref _running, 0);
        }

        public bool WriteVariableData(VariableData variableData, int newValue)
        {
            byte[] buffer = variableData.Size switch
            {
                INT_SIZE => BitConverter.GetBytes(newValue),
                BYTE_SIZE => [(byte)newValue],
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

        internal void UpdateProcessHandle(nint handle)
        {
            _processHandle = handle;
            Start(_currentRootObject);
        }
    }
}