namespace REviewer.Modules.RE
{
    public class KeyItem : ICloneable
    {
        // Constructor
        public KeyItem(Property data, int state, string room)
        {
            Data = data;
            State = state;
            Room = room;
        }
        public KeyItem()
        {
        }

        // Public properties
        public Property? Data { get; set; }
        public string? Room { get; set; }
        public int? State { get; set; }

        public object Clone()
        {
            return new KeyItem
            {
                // Copy all properties
                Data = this.Data, // Assuming Property implements ICloneable
                Room = this.Room,
                State = this.State
            };
        }
    }

}
