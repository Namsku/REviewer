using REviewer.Modules.RE.Json;

namespace REviewer.Modules.Utils
{
    public class VariableData
    {
        // Private fields
        private int _value;

        // Public properties
        public object? Database { get; set; }
        public bool IsUpdated { get; set; } = false;
        public object LockObject { get; } = new object();
        public IntPtr Offset { get; set; }
        public uint Size { get; set; }
        public int Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnUpdated(); // Call method to raise event
                }
            }
        }

        // Event declaration
        public event EventHandler? Updated;

        // Method to raise the Updated event
        protected virtual void OnUpdated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }

        public VariableData(IntPtr offset, uint size)
        {
            Offset = offset;
            Size = size;
            Database = null;
        }

        public VariableData(IntPtr offset, StandardProperty property)
        {
            Offset = offset;
            Size = (uint)property.Size;
            Database = (Dictionary<byte, string>?) property.Database;
        }

        public VariableData(IntPtr offset, AdvancedProperty property)
        {
            Offset = offset;
            Size = (uint)property.Size;
            Database = (Dictionary<byte, List<int>>?) Library.ConvertDictionnary(property.Database);
        }
    }
}
