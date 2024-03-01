using MessagePack;
using REviewer.Modules.RE;
using REviewer.Modules.Utils;

namespace REviewer.Modules.SRT
{
    [MessagePackObject]
    public class PlayerRaceProgress : ICloneable
    {
        public PlayerRaceProgress()
        {
        }

        // Key Elements
        [Key(0)]
        public List<KeyItem> KeyItems { get; set; }

        [Key(1)]
        public Dictionary<string, List<string>> KeyRooms { get; set; }

        // Stats
        [Key(2)]
        public int Saves { get; set; }

        [Key(3)]
        public int Deaths { get; set; }

        [Key(4)]
        public int Resets { get; set; }

        [Key(5)]
        public int Debugs { get; set; }

        [Key(6)]
        public int Segments { get; set; }

        // Room Infos
        [Key(7)]
        public int PreviousState { get; set; }

        [Key(8)]
        public string Stage { get; set; }

        [Key(9)]
        public string Room { get; set; }

        [Key(10)]
        public string LastRoomName { get; set; }

        [Key(11)]
        public string FullRoomName { get; set; }

        [Key(12)]
        public string? SavePath;

        [Key(13)]
        public int? Fulltimer { get; set; }

        [Key(14)]
        public List<int?> SegTimers = [0,0,0,0];

        [Key(15)]
        public int TickTimer { get; set; }
        [Key(16)]
        public int SaveID { get; set; }
        [Key(27)]
        public byte[] RealItembox { get; set; }

        [Key(28)]
        public List<string> Rooms = [];

        [IgnoreMember]
        public FileSystemWatcher Watcher = new();

        public PlayerRaceProgress(string savePath)
        {
            SavePath = Library.GetSavePath(savePath);
        }

        public object Clone()
        {
            return new PlayerRaceProgress
            {
                // Copy all properties
                KeyItems = KeyItems.Select(item => (KeyItem)item.Clone()).ToList(),
                KeyRooms = new Dictionary<string, List<string>>(KeyRooms),
                Saves = Saves,
                Deaths = Deaths,
                Resets = Resets,
                Debugs = Debugs,
                Segments = Segments,
                PreviousState = PreviousState,
                Stage = Stage,
                Room = Room,
                LastRoomName = LastRoomName,
                FullRoomName = FullRoomName,
                SavePath = SavePath,
                Fulltimer = Fulltimer,
                SegTimers = SegTimers,
                TickTimer = TickTimer,
                SaveID = SaveID,
                RealItembox = (byte[])RealItembox?.Clone(),
                // FileSystemWatcher is not cloneable, so we create a new one
                // Watcher = new FileSystemWatcher()
            };
        }
    }
}
