using MessagePack;

namespace REviewer.Modules.RE
{
    [MessagePackObject]
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
        [Key(0)]
        public Property? Data { get; set; }
        [Key(1)]
        public string? Room { get; set; }
        [Key(2)]
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
