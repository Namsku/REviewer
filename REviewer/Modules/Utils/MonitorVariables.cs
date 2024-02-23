using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using REviewer.Modules.Utils;
using static REviewer.Modules.RE.GameData;

namespace REviewer.Modules.Utils
{
    public class MonitorVariables
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(nint processHandle, nint baseAddress, byte[] buffer, uint size, out int bytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(nint processHandle, nint baseAddress, [Out] byte[] buffer, uint size, out int bytesRead);

        private System.Threading.Timer? _monitoringTimer;
        private nint _processHandle;
        private readonly string _processName;
        private bool _running = true;

        private RootObject? _currentRootObject;

        public MonitorVariables(nint processHandle, string processName)
        {
            _processHandle = processHandle;
            _processName = processName;
        }

        public void UpdateProcessHandle(nint newProcessHandle)
        {
            // Stop the current monitoring process
            Stop();

            // Update the process handle
            _processHandle = newProcessHandle;

            // Restart the monitoring process with the new process handle
            Start(_currentRootObject);
        }

        public void Start(RootObject? rootObject)
        {
            if (rootObject == null)
            {
                Logger.Logging.Error("RootObject is null");
            }

            _currentRootObject = rootObject;
            _monitoringTimer = new System.Threading.Timer(state => Monitor(), rootObject, TimeSpan.Zero, TimeSpan.FromMilliseconds(55));
            Logger.Logging.Info("Started monitoring");
        }

        public void Stop()
        {
            Logger.Logging.Info("Stopping monitoring");
            _monitoringTimer?.Dispose();
            _monitoringTimer = null;
        }

        private bool LocateProcess(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                _processHandle = processes[0].Handle;
                return true;
            }
            return false;
        }

        public void Monitor()
        {
            if (!IsProcessActive())
            {
                _running = false;
                _processHandle = nint.Zero;
            }

            if (!IsProcessActive() && _running == false)
            {
                if (LocateProcess(_processName))
                {
                    _running = true;
                    _processHandle = Process.GetProcessesByName(_processName)[0].Handle;
                    Start(_currentRootObject);
                }
            }

            if (_processHandle != nint.Zero)
            {
                MonitorObject(_currentRootObject);
            }
        }

        private bool IsProcessActive()
        {
            return Process.GetProcessesByName(_processName).Length > 0;
        }

        private void MonitorObject(object obj)
        {
            if (!CanMonitorObject(obj))
            {
                return;
            }

            var properties = obj.GetType().GetProperties();

            foreach (var property in properties)
            {
                MonitorProperty(property, obj);
            }
        }

        private bool CanMonitorObject(object obj)
        {
            if (_processHandle == nint.Zero || obj == null)
            {
                Stop();
                return false;
            }

            return true;
        }

        private void MonitorProperty(PropertyInfo property, object obj)
        {
            lock (property)
            {
                var value = property.GetValue(obj);

                if (value is VariableData variableData)
                {
                    MonitorVariableData(variableData);
                }
                else if (value is IEnumerable enumerable && !(value is string))
                {
                    MonitorEnumerable(enumerable);
                }
                else if (value != null)
                {
                    MonitorObject(value);
                }
            }
        }

        private void MonitorVariableData(VariableData variableData)
        {
            int oldValue = variableData.Value;
            variableData.Value = ReadVariableData(variableData);

            lock (variableData.LockObject)
            {
                if (variableData.IsUpdated)
                {
                    WriteVariableData(variableData, oldValue);
                    variableData.IsUpdated = false;
                }
            }
        }

        private void MonitorEnumerable(IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                MonitorObject(item);
            }
        }

        public int ReadVariableData(VariableData variableData)
        {
            byte[] buffer = new byte[variableData.Size];
            if (!ReadProcessMemory(_processHandle, variableData.Offset, buffer, (uint)buffer.Length, out _))
            {
                Logger.Logging.Error("Failed to read process memory");
                return 0;
            }

            return variableData.Size switch
            {
                4 => BitConverter.ToInt32(buffer, 0),
                1 => buffer[0],
                _ => 0
            };
        }

        public bool WriteVariableData(VariableData variableData, int newValue)
        {
            byte[] buffer = variableData.Size switch
            {
                4 => BitConverter.GetBytes(newValue),
                1 => [(byte)newValue],
                _ => null
            };

            if (buffer == null)
            {
                return false;
            }

            if (!WriteProcessMemory(_processHandle, variableData.Offset, buffer, (uint)buffer.Length, out int bytesWritten))
            {
                Logger.Logging.Error("Failed to write process memory");
                return false;
            }

            return true;
        }
    }
}