using System;

namespace DungeonLord.Scripts
{
    public static class LevelingEngine
    {
        /// <summary>
        /// Compounding growth formula:
        /// - Each level's base value is the previous level's calculated value.
        /// - Standard level-up: +1% growth.
        /// - 10th-level milestone: +10% bonus added to the +1% (total +11% on milestone 10, 20, 30, etc.).
        /// - 25th-level milestone: +25% bonus added to the +1% (total +26% on milestone 25, 50, 75, etc., replacing 10th bonus).
        /// - Milestone bonuses apply only on that specific level step, compounding into subsequent levels.
        /// </summary>
        public static float CalculateAttribute(float initialBaseValue, int level)
        {
            if (level <= 1) return initialBaseValue;

            float current = initialBaseValue;
            for (int lvl = 2; lvl <= level; lvl++)
            {
                float stepPct = 0.01f;

                if (lvl % 25 == 0)
                {
                    stepPct += 0.25f; // +25% milestone burst
                }
                else if (lvl % 10 == 0)
                {
                    stepPct += 0.10f; // +10% milestone burst
                }

                current *= (1.0f + stepPct);
            }

            return current;
        }
    }
}
