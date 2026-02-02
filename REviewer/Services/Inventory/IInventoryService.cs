using System; // For IntPtr
using System.ComponentModel;

namespace REviewer.Services.Inventory
{
    public interface IInventoryService : INotifyPropertyChanged
    {
        // Add inventory related properties and methods here
        // List<Slot> Inventory { get; }
        System.Collections.Generic.List<Modules.RE.Common.Slot>? Inventory { get; }
        System.Collections.ObjectModel.ObservableCollection<Modules.SRT.ImageItem>? InventoryImages { get; }
        
        void Initialize(object bioObj, nint virtualMemoryPointer, int selectedGame, Modules.RE.ItemIDs itemIds, bool carlos = false);
        void RefreshInventory();

        bool IsItemBoxOpen { get; }
        event EventHandler? ItemBoxAccessed;
    }
}
