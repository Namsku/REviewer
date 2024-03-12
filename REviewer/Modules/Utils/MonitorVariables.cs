using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media;
using REviewer.Modules.RE.Common;

namespace REviewer.Modules.Utils
{
    public class MonitorVariables(nint processHandle, string processName)
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(nint processHandle, nint baseAddress, byte[] buffer, uint size, out int bytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(nint processHandle, nint baseAddress, [Out] byte[] buffer, uint size, out int bytesRead);

        private nint _processHandle = processHandle;
        private readonly string _processName = processName;
        private readonly object _processHandleLock = new();

        private const int BYTE_SIZE = 1;
        private const int INT_SIZE = 4;
        private const int MONITORING_INTERVAL = 55;

        private System.Threading.Timer? _monitoringTimer;

        private volatile int _running = 1;

        private RootObject? _currentRootObject;
        private ObservableCollection<EnnemyTracking>? _enemyTracking;
        private readonly object _lockObject = new();

        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

        private List<string> _debug = new();

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

            if (isProcessActive)
            {
                if(_currentRootObject != null)
                {
                    MonitorObject(_currentRootObject);
                }
                else if (_enemyTracking != null)
                {
                    MonitorObject(_enemyTracking);
                }
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
                if (property.PropertyType != typeof(System.String)
                    && property.PropertyType != typeof(System.Int32)
                        && property.Name != "KeyItemImages"
                            && property.PropertyType != typeof(ImageSource)
                             && property.PropertyType != typeof(Brush)
                                && property.PropertyType != typeof(Brushes)
                                    && property.PropertyType != typeof(List<string>)
                                )
                {
                    MonitorProperty(property, obj);
                }
            }
        }

        private void MonitorProperty(PropertyInfo property, object obj)
        {
            
            if (obj == null)
            {
                return;
            }

            lock (_lockObject) 
            {
                if (obj is ObservableCollection<EnnemyTracking> ennemyTrackings)
                {
                    foreach (var item in ennemyTrackings)
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
                Stop();
                return [0];
                // throw new InvalidOperationException("Failed to read process memory");
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
            Logger.Instance.Info("Started RootObject monitoring");
        }

        public void Start(ObservableCollection<EnnemyTracking> ennemyTrackings)
        {
            if (ennemyTrackings == null)
            {
                Logger.Instance.Error("EnnemyTrackings is null");
                return;
            }

            _enemyTracking = ennemyTrackings;
            _monitoringTimer = new System.Threading.Timer(state => Monitor(), ennemyTrackings, TimeSpan.Zero, TimeSpan.FromMilliseconds(MONITORING_INTERVAL));
            Logger.Instance.Info("Started Enemy monitoring");
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