using System;

namespace REviewer.Core.Memory
{
    public interface IMemoryMonitor
    {
        void RegisterVariable(string key, Modules.Utils.VariableData variable);
        void UnregisterVariable(string key);
        void Register(object obj); // Legacy support for RootObject
        void Unregister(object obj); // Legacy support for RootObject
        void Start();
        void Stop();
        void UpdateProcessHandle(nint handle, string processName);
        int ReadVariableData(Modules.Utils.VariableData variableData);
        bool WriteVariableData(Modules.Utils.VariableData variableData, int newValue);
    }
}
