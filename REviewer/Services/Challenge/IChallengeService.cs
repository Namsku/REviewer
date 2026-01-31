namespace REviewer.Services.Challenge
{
    public interface IChallengeService
    {
        // Active Challenges
        bool IsOneHPEnabled { get; set; }
        bool IsNoDamageEnabled { get; set; }
        bool IsNoItemBoxEnabled { get; set; }
        bool IsNoSaveEnabled { get; set; }

        // Challenge State
        bool HasTakenDamage { get; }
        bool HasUsedItemBox { get; }
        int DamageTakenCount { get; }

        // Methods
        void Reset();
        void OnDamageTaken(int amount);
        void OnItemBoxUsed();

        // Events
        event EventHandler? ChallengeFailed;
    }
}
