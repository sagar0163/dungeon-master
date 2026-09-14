using NUnit.Framework;
using DungeonLord.Scripts;

namespace DungeonLord.Tests
{
    /// <summary>
    /// Essence earn/spend/cap + rank-up capacity growth.
    /// Mirrors the Essence economy and increasing threshold/capacity invariants from
    /// tests/test_progression.py (TestEssenceEconomy, TestDungeonRankThresholds).
    /// </summary>
    [TestFixture]
    public class EssenceManagerTests
    {
        [Test]
        public void Defaults_InitialEssenceAndCapacity()
        {
            var manager = new EssenceManager();
            Assert.That(manager.CurrentEssence, Is.EqualTo(200));
            Assert.That(manager.EssenceCapacity, Is.EqualTo(1000));
            Assert.That(manager.DungeonRank, Is.EqualTo(1));
        }

        [Test]
        public void AddEssence_IncreasesBalance()
        {
            var manager = new EssenceManager(200);
            manager.AddEssence(150);
            Assert.That(manager.CurrentEssence, Is.EqualTo(350));
        }

        [Test]
        public void AddEssence_CapsAtCapacity()
        {
            var manager = new EssenceManager(200);
            manager.AddEssence(5000);
            Assert.That(manager.CurrentEssence, Is.EqualTo(1000));
            Assert.That(manager.CurrentEssence, Is.LessThanOrEqualTo(manager.EssenceCapacity));
        }

        [Test]
        public void AddEssence_NonPositive_IsIgnored()
        {
            var manager = new EssenceManager(200);
            manager.AddEssence(0);
            manager.AddEssence(-50);
            Assert.That(manager.CurrentEssence, Is.EqualTo(200));
        }

        [Test]
        public void SpendEssence_SufficientFunds()
        {
            var manager = new EssenceManager(500);
            Assert.That(manager.SpendEssence(300), Is.True);
            Assert.That(manager.CurrentEssence, Is.EqualTo(200));
        }

        [Test]
        public void SpendEssence_InsufficientOrInvalid_Rejected()
        {
            var manager = new EssenceManager(200);
            Assert.That(manager.SpendEssence(201), Is.False);
            Assert.That(manager.SpendEssence(0), Is.False);
            Assert.That(manager.SpendEssence(-10), Is.False);
            Assert.That(manager.CurrentEssence, Is.EqualTo(200));
        }

        [Test]
        public void TryRankUp_ConsumesEssenceAndIncrementsRank()
        {
            var manager = new EssenceManager(5000);
            Assert.That(manager.TryRankUp(5000), Is.True);
            Assert.That(manager.DungeonRank, Is.EqualTo(2));
            Assert.That(manager.CurrentEssence, Is.EqualTo(0));
            // Capacity recomputed through the growth formula: 1000 * 1.01
            Assert.That(manager.EssenceCapacity, Is.EqualTo(1010));
        }

        [Test]
        public void TryRankUp_InsufficientEssence_Fails()
        {
            var manager = new EssenceManager(200);
            Assert.That(manager.TryRankUp(5000), Is.False);
            Assert.That(manager.DungeonRank, Is.EqualTo(1));
            Assert.That(manager.CurrentEssence, Is.EqualTo(200));
        }

        [Test]
        public void RankUps_CapacityIncreasesSteadily()
        {
            // Python thresholds: rank2=5000, rank3=15000, rank4=40000, rank5=100000.
            var manager = new EssenceManager(200000);
            var requirements = new long[] { 5000, 15000, 40000, 100000 };

            long previousCapacity = manager.EssenceCapacity;
            for (int i = 0; i < requirements.Length; i++)
            {
                Assert.That(manager.TryRankUp(requirements[i]), Is.True, $"rank-up #{i + 1}");
                Assert.That(manager.EssenceCapacity, Is.GreaterThan(previousCapacity));
                previousCapacity = manager.EssenceCapacity;
            }
            Assert.That(manager.DungeonRank, Is.EqualTo(5));
        }
    }
}