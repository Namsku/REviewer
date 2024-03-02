using REviewer.Modules.RE.Json;

namespace REviewer.Modules.RE.Common
{
    public class RootObject(Bio bio)
    {
        public Player Player { get; set; } = new Player(bio);
        public Position Position { get; set; } = new Position(bio);
        public Game Game { get; set; } = new Game(bio);
        public Inventory Inventory { get; set; } = new Inventory(bio);
        public ItemBox ItemBox { get; set; } = new ItemBox(bio);
        public Rebirth? Rebirth { get; set; } = new Rebirth(bio);
    }
}
