using REviewer.Modules.Utils;
using REviewer.Modules.RE.Common;
using REviewer.Modules.RE.Enemies;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace REviewer.Core.Memory
{
    public class MemoryMonitor : IMemoryMonitor, IDisposable
    {
        private readonly object _processHandleLock = new();
        private readonly object _registrationLock = new();
        
        // PRD Requirement: Flat list of registered VariableData
        // We use a Dictionary for fast lookup/unregistration, and a List for the hot path iteration
        private readonly Dictionary<string, VariableData> _registeredVariables = new();
        private readonly List<VariableData> _flatList = new(); 

        private nint _processHandle;
        private string? _processName;
        private volatile int _isRunning = 0;
        private System.Threading.Timer? _monitoringTimer;

        private const int MonitoringInterval = 55;
        private const int ByteSize = 1;
        private const int IntSize = 4;

        private readonly byte[] _buffer4 = new byte[4];
        private readonly byte[] _buffer1 = new byte[1];

        // Cache for legacy object recursive scanning
        private static readonly ConcurrentDictionary<Type, List<CompiledPropertyAccessor>> _accessorCache = new();

        private class CompiledPropertyAccessor
        {
            public Func<object, object?> Getter { get; init; } = null!;
            public Type PropertyType { get; init; } = null!;
            public string Name { get; init; } = null!;
        }

        // Compliant PRD Methods
        public void RegisterVariable(string key, VariableData variable)
        {
            lock (_registrationLock)
            {
                if (!_registeredVariables.ContainsKey(key))
                {
                    _registeredVariables[key] = variable;
                    RebuildFlatList();
                }
            }
        }

        public void UnregisterVariable(string key)
        {
            lock (_registrationLock)
            {
                if (_registeredVariables.Remove(key))
                {
                    RebuildFlatList();
                }
            }
        }

        // Legacy / Batch Helper: Recursively scans an object ONCE and registers all VariableData found
        public void Register(object obj)
        {
            lock (_registrationLock)
            {
                ScanAndRegisterRecursive(obj, "");
                RebuildFlatList();
            }
        }

        public void Unregister(object obj)
        {
             // For simplicity in this legacy method, we clear everything if the RootObject is unregistered
             // or specialized logic could strictly track object ownership.
             // Given the lifecycle, mostly we just Stop() or clear all.
             // Implementing a clear for safety if this is called.
             lock (_registrationLock)
             {
                 _registeredVariables.Clear();
                 RebuildFlatList();
             }
        }

        private void RebuildFlatList()
        {
            _flatList.Clear();
            _flatList.AddRange(_registeredVariables.Values);
        }

        private void ScanAndRegisterRecursive(object obj, string parentKey)
        {
            if (obj == null) return;

            // Prevent cycles? RootObject is complex. Simple recursion limit or visited set might be needed if cyclic.
            // RootObject is mostly tree-like.

            if (obj is VariableData variableData)
            {
                // Register this variable
                string key = string.IsNullOrEmpty(parentKey) ? Guid.NewGuid().ToString() : parentKey;
                if (!_registeredVariables.ContainsKey(key))
                {
                    _registeredVariables[key] = variableData;
                }
                return;
            }
            
            if (obj is IEnumerable enumerable && obj is not string && obj is not char)
            {
                 // Handle lists? REviewer uses List<VariableData> sometimes?
                 // VariableData is not IEnumerable.
                 int index = 0;
                 foreach (var item in enumerable)
                 {
                     if (item != null) ScanAndRegisterRecursive(item, $"{parentKey}[{index++}]");
                 }
                 return;
            }

            var type = obj.GetType();
            // Skip primitives and system types early
            if (type.IsPrimitive || type == typeof(string) || type.Module.ScopeName == "CommonLanguageRuntimeLibrary")
                return;

            var accessors = _accessorCache.GetOrAdd(type, CompileAccessors);

            foreach (var accessor in accessors)
            {
                try
                {
                    var val = accessor.Getter(obj);
                    if (val != null)
                    {
                        string newKey = string.IsNullOrEmpty(parentKey) ? accessor.Name : $"{parentKey}.{accessor.Name}";
                        ScanAndRegisterRecursive(val, newKey);
                    }
                }
                catch { }
            }
        }

        public void Start()
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 0)
            {
                _monitoringTimer = new System.Threading.Timer(_ => MonitorLoop(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(MonitoringInterval));
                Logger.Instance.Info("MemoryMonitor started");
            }
        }

        public void Stop()
        {
            if (Interlocked.CompareExchange(ref _isRunning, 0, 1) == 1)
            {
                _monitoringTimer?.Dispose();
                _monitoringTimer = null;
                Logger.Instance.Info("MemoryMonitor stopped");
            }
        }

        public void UpdateProcessHandle(nint handle, string processName)
        {
            lock (_processHandleLock)
            {
                _processHandle = handle;
                _processName = processName;
            }
        }

        private void MonitorLoop()
        {
            bool isProcessActive;
            lock (_processHandleLock)
            {
                isProcessActive = IsProcessActive();
                if (!isProcessActive)
                {
                    _processHandle = 0;
                    return;
                }
            }

            // High Performance Loop: Iterate flat list only
            // No lock needed for iteration if we assume _flatList reference is swapped on change (Copy-On-Write style)
            // But List is not thread safe. _flatList is rebuilt under lock.
            // We should grab a reference to the array or list.
            
            List<VariableData> snapshot;
            lock (_registrationLock)
            {
                // Cheap copy of reference or content? List<T> copy is cheapish if pointers.
                snapshot = new List<VariableData>(_flatList);
            }

            foreach (var variableData in snapshot)
            {
                MonitorVariableData(variableData);
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

        private static List<CompiledPropertyAccessor> CompileAccessors(Type type)
        {
            var result = new List<CompiledPropertyAccessor>();
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                if (property.GetIndexParameters().Length > 0 || !property.CanRead) continue;
                if (!IsValidProperty(property)) continue;

                try
                {
                    var param = Expression.Parameter(typeof(object), "obj");
                    var cast = Expression.Convert(param, type);
                    var propAccess = Expression.Property(cast, property);
                    var box = Expression.Convert(propAccess, typeof(object));
                    var lambda = Expression.Lambda<Func<object, object?>>(box, param).Compile();

                    result.Add(new CompiledPropertyAccessor
                    {
                        Getter = lambda,
                        PropertyType = property.PropertyType,
                        Name = property.Name
                    });
                }
                catch { }
            }

            return result;
        }

        private static bool IsValidProperty(PropertyInfo property)
        {
            return property.PropertyType != typeof(string)
                   && property.PropertyType != typeof(int)
                   && property.PropertyType != typeof(int?)
                   && property.Name != "KeyItemImages"
                   && property.Name != "SelectedGame"
                   && property.Name != "GameObject"
                   && !property.PropertyType.Name.StartsWith("Dictionary")
                    // Allow Lists so we can traverse them in helper
                   // && !property.PropertyType.Name.StartsWith("List`1") 
                   && property.PropertyType != typeof(System.Windows.Media.ImageSource)
                   && property.PropertyType != typeof(System.Windows.Media.Brush);
        }

        private bool IsProcessActive()
        {
            if (_processHandle == 0) return false;
            int exitCode = 0;
            return NativeWrappers.GetExitCodeProcess(_processHandle, ref exitCode) && exitCode == 259;
        }

        public int ReadVariableData(VariableData variableData)
        {
            byte[] buffer = ReadProcessMemory(variableData.Offset, (uint)variableData.Size);
            return variableData.Size switch
            {
                IntSize => BitConverter.ToInt32(buffer, 0),
                ByteSize => buffer[0],
                _ => 0
            };
        }

        private byte[] ReadProcessMemory(nint baseAddress, uint size)
        {
            byte[] buffer = size switch
            {
                4 => _buffer4,
                1 => _buffer1,
                _ => new byte[size]
            };

            bool success;
            lock (_processHandleLock)
            {
                success = ReadProcessMemory(_processHandle, baseAddress, buffer, size, out _);
            }

            if (!success) Array.Clear(buffer, 0, (int)size);
            return buffer;
        }

        public bool WriteVariableData(VariableData variableData, int newValue)
        {
            byte[] buffer = variableData.Size switch
            {
                IntSize => BitConverter.GetBytes(newValue),
                ByteSize => new byte[] { (byte)newValue },
                _ => null!
            };

            if (buffer == null) return false;

            lock (_processHandleLock)
            {
                return WriteProcessMemory(_processHandle, variableData.Offset, buffer, (uint)buffer.Length, out _);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(nint processHandle, nint baseAddress, byte[] buffer, uint size, out int bytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(nint processHandle, nint baseAddress, [Out] byte[] buffer, uint size, out int bytesRead);

        public void Dispose()
        {
            Stop();
        }
    }
}
