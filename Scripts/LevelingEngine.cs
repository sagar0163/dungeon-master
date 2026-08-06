using System;

namespace DungeonLord.Scripts
{
    public static class LevelingEngine
    {
        /// <summary>
        /// Shared percentage-based growth formula for Dungeon Lord personal stats and Dungeon Rank stats.
        /// attribute = base * (1 + 0.01 * level + milestone_bonus(level))
        /// </summary>
        public static float CalculateAttribute(float baseValue, int level)
        {
            if (level <= 1) return baseValue;

            float linearFactor = 0.01f * (level - 1);
            float milestoneBonus = CalculateMilestoneBonus(level);

            return baseValue * (1.0f + linearFactor + milestoneBonus);
        }

        private static float CalculateMilestoneBonus(int level)
        {
            float bonus = 0.0f;

            // Count 25-level major milestones first
            int majorMilestones = level / 25;
            bonus += majorMilestones * 0.25f;

            // Count 10-level milestones, excluding those that fall on 25-level milestones
            for (int lvl = 10; lvl <= level; lvl += 10)
            {
                if (lvl % 25 != 0)
                {
                    bonus += 0.10f;
                }
            }

            return bonus;
        }
    }
}
