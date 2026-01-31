namespace REviewer.Services.Game
{
    public class GameStateModel
    {
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public bool IsDead { get; set; }
        public int Deaths { get; set; }
        public int Resets { get; set; }
        public int Hits { get; set; }
        public int Saves { get; set; }
        public int CharacterId { get; set; }
        public string CharacterName { get; set; } = "Unknown";
        public bool IsGameDone { get; set; }
        public bool IsNewGame { get; set; }
        public bool IsRetry { get; set; }
    }
}
