namespace REviewer.Modules.RE.Common
{
    public class RootObject
    {
        public RootObject(Bio bio)
        {
            Name = bio.Name;
            Process = bio.Process;
            Player = new Player(bio);
            Position = new Position(bio);
            Game = new Game(bio);
            Inventory = new Inventory(bio);
            ItemBox = new ItemBox(bio);
            Rebirth = new Rebirth(bio);
        }
        
        public string? Name { get; set; }
        public string? Process { get; set; }
        public Player? Player { get; set; }
        public Position? Position { get; set; }
        public Game? Game { get; set; }
        public Inventory? Inventory { get; set; }
        public ItemBox? ItemBox { get; set; }
        public Rebirth? Rebirth { get; set; }
    }
}
