using Xunit;
using REviewer.Services;
using REviewer.Services.Inventory;
using REviewer.Modules.RE.Json;
using REviewer.Core.Constants;
using System;
using System.Collections.Generic;

namespace REviewer.Tests.Services
{
    public class InventoryServiceTests
    {
        [Fact]
        public void Initialize_ShouldSetBioAndOffsets()
        {
            // Arrange
            var service = new InventoryService(new MockMemoryMonitor());
            var bio = new Bio
            {
                Offsets = new Dictionary<string, string>
                {
                    { "InventoryStart", "0x00" },
                    { "InventoryEnd", "0x0A" } // 10 bytes
                }
            };

            // Act
            // Virtual pointer 0 for simplicity
            service.Initialize(bio, IntPtr.Zero, GameConstants.BIOHAZARD_1, null, false);

            // Assert
            Assert.NotNull(service.Inventory);
            
            // Logic: (10 - 0) / 2 = 5.  5 > 10 is False -> inc = 2.
            // 0, 2, 4, 6, 8 -> 5 slots
            Assert.Equal(5, service.Inventory.Count);
        }

        [Fact]
        public void Initialize_Carlos_ShouldUseCarlosOffsets()
        {
            // Arrange
            var service = new InventoryService(new MockMemoryMonitor());
            var bio = new Bio
            {
                Offsets = new Dictionary<string, string>
                {
                    { "CarlosInventoryStart", "0x10" },
                    { "CarlosInventoryEnd", "0x1A" } // 10 bytes
                }
            };

            // Act
            service.Initialize(bio, IntPtr.Zero, GameConstants.BIOHAZARD_3, null, true);

            // Assert
            Assert.NotNull(service.Inventory);
            // Same logic as above
            Assert.Equal(5, service.Inventory.Count);
        }
    }

    public class MockMemoryMonitor : REviewer.Core.Memory.IMemoryMonitor
    {
        public int ReadVariableData(Modules.Utils.VariableData variableData) => 0;
        public void Register(object obj) { }
        public void RegisterVariable(string key, Modules.Utils.VariableData variable) { }
        public void Start() { }
        public void Stop() { }
        public void Unregister(object obj) { }
        public void UnregisterVariable(string key) { }
        public void UpdateProcessHandle(nint handle, string processName) { }
        public bool WriteVariableData(Modules.Utils.VariableData variableData, int newValue) => true;
    }
}
