using REviewer.Modules.RE;

namespace REviewer.Modules.SRT
{
    public class PlayerRaceProgress
    {
        // Key Elements
        public List<KeyItem>? KeyItems { get; set; }
        public Dictionary<string, List<string>>? KeyRooms { get; set; }
        public List<string>? Rooms { get; set; } = new List<string>();

        // Stats
        public Dictionary<string, int>? Stats { get; set; }

        // Room Infos
        public Dictionary<string, string>? RoomInfos { get; set; }

        // Timers
        public int? Fulltimer { get; set; }
        public List<int?> SegTimers { get; set; } = new List<int?> { 0, 0, 0, 0 };

        // Save
        public string? SavePath { get; set; }
        public int? SaveID { get; set; }

        // GameState
        public int? PreviousState { get; set; }

        public PlayerRaceProgress(string savePath)
        {
            SavePath = savePath;
        }

        public PlayerRaceProgress Clone()
        {
            if (KeyItems == null)
            {
                throw new ArgumentNullException("KeyItems is null");
            }

            if (KeyRooms == null)
            {
                throw new ArgumentNullException("KeyRooms is null");
            }

            if (SavePath == null)
            {
                throw new ArgumentNullException("SavePath is null");
            }

            if (Stats == null)
            {
                throw new ArgumentNullException("Stats is null");
            }

            if (RoomInfos == null)
            {
                throw new ArgumentNullException("RoomInfos is null");
            }

            var clone = new PlayerRaceProgress(SavePath)
            {
                // Copy all properties
                KeyItems = new List<KeyItem>(KeyItems),
                KeyRooms = new Dictionary<string, List<string>>(KeyRooms),
                Stats = new Dictionary<string, int>(Stats),
                RoomInfos = new Dictionary<string, string>(RoomInfos),
                PreviousState = PreviousState,

                Fulltimer = Fulltimer,
                SegTimers = new List<int?>(SegTimers),
                SaveID = SaveID,
            };

            return clone;
        }
    }
}
