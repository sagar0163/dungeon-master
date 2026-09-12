using NUnit.Framework;
using DungeonLord.Scripts;

namespace DungeonLord.Tests
{
    /// <summary>
    /// Mirrors tests/test_progression.py (TestGrowthFormula, TestEssenceEconomy) and the
    /// attribute checks in tests/test_rules.py. Both layers must agree on the same numbers.
    /// </summary>
    [TestFixture]
    public class LevelingEngineTests
    {
        private const float Tolerance = 1e-4f;

        [Test]
        public void Level1_Base_NoGrowth()
        {
            Assert.That(LevelingEngine.CalculateAttribute(1.0f, 1), Is.EqualTo(1.0f).Within(Tolerance));
            Assert.That(LevelingEngine.CalculateAttribute(100.0f, 1), Is.EqualTo(100.0f).Within(Tolerance));
        }

        [Test]
        public void Level2_FirstStep()
        {
            // 1.0 * 1.01
            Assert.That(LevelingEngine.CalculateAttribute(1.0f, 2), Is.EqualTo(1.01f).Within(Tolerance));
            Assert.That(LevelingEngine.CalculateAttribute(100.0f, 2), Is.EqualTo(101.0f).Within(Tolerance));
        }

        [Test]
        public void Level10_First10Milestone()
        {
            // 8 levels at +1% (1.01^8), level 10 at +11% -> 1.2019709 multiplier / 120.19709 at base 100
            Assert.That(LevelingEngine.CalculateAttribute(1.0f, 10), Is.EqualTo(1.2019709f).Within(Tolerance));
            Assert.That(LevelingEngine.CalculateAttribute(100.0f, 10), Is.EqualTo(120.19709f).Within(Tolerance));
        }

        [Test]
        public void Level11_After10Milestone()
        {
            // Level 10 value * 1.01
            Assert.That(LevelingEngine.CalculateAttribute(1.0f, 11), Is.EqualTo(1.2139906f).Within(Tolerance));
        }

        [Test]
        public void Level25_First25Milestone()
        {
            // Compounding milestone step at level 25 -> 1.9132219 multiplier / 191.32219 at base 100
            Assert.That(LevelingEngine.CalculateAttribute(1.0f, 25), Is.EqualTo(1.9132219f).Within(Tolerance));
            Assert.That(LevelingEngine.CalculateAttribute(100.0f, 25), Is.EqualTo(191.32219f).Within(Tolerance));
        }

        [Test]
        public void Compounding_NextLevelBuildsOnPrevious()
        {
            var v9 = LevelingEngine.CalculateAttribute(1.0f, 9);
            var v10 = LevelingEngine.CalculateAttribute(1.0f, 10);
            var v11 = LevelingEngine.CalculateAttribute(1.0f, 11);

            Assert.That(v10, Is.EqualTo(v9 * 1.11f).Within(Tolerance));
            Assert.That(v11, Is.EqualTo(v10 * 1.01f).Within(Tolerance));
        }

        [Test]
        public void EssenceCapacity_GrowsSteadilyPerRank()
        {
            // Mirrors the "thresholds/capacity increase per rank" invariants.
            float previous = LevelingEngine.CalculateAttribute(1000.0f, 1);
            for (int rank = 2; rank <= 5; rank++)
            {
                float capacity = LevelingEngine.CalculateAttribute(1000.0f, rank);
                Assert.That(capacity, Is.GreaterThan(previous));
                previous = capacity;
            }
        }
    }
}