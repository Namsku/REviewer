namespace REviewer.Modules.RE
{
    public class KeyItem
    {
        // Constructor
        public KeyItem(Property data, int state, string room)
        {
            Data = data;
            State = state;
            Room = room;
        }

        // Public properties
        public Property? Data { get; set; }
        public string? Room { get; set; }
        public int? State { get; set; }

    }

}
