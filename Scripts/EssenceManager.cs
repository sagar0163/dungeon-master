using System;

namespace DungeonLord.Scripts
{
    public class EssenceManager
    {
        public long CurrentEssence { get; private set; }
        public int DungeonRank { get; private set; } = 1;
        public long EssenceCapacity { get; private set; } = 1000;

        public event Action<long> OnEssenceChanged;
        public event Action<int> OnRankUp;

        public EssenceManager(long initialEssence = 200)
        {
            CurrentEssence = initialEssence;
        }

        public void AddEssence(long amount)
        {
            if (amount <= 0) return;
            CurrentEssence = Math.Min(CurrentEssence + amount, EssenceCapacity);
            OnEssenceChanged?.Invoke(CurrentEssence);
        }

        public bool SpendEssence(long amount)
        {
            if (amount <= 0 || CurrentEssence < amount) return false;
            CurrentEssence -= amount;
            OnEssenceChanged?.Invoke(CurrentEssence);
            return true;
        }

        public bool TryRankUp(long requiredEssence)
        {
            if (CurrentEssence < requiredEssence) return false;

            SpendEssence(requiredEssence);
            DungeonRank++;
            EssenceCapacity = (long)LevelingEngine.CalculateAttribute(1000, DungeonRank);
            OnRankUp?.Invoke(DungeonRank);
            return true;
        }
    }
}
