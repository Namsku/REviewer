using System;

namespace REviewer.Services.Save
{
    public interface ISaveService
    {
        void Initialize(string savePath);
        void Watch();
        event EventHandler? SaveChanged;
    }
}
