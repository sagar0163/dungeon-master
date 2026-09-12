using System.Linq;
using NUnit.Framework;
using DungeonLord.Scripts;

namespace DungeonLord.Tests
{
    /// <summary>
    /// Issue #10: deterministic invader-wave integration. Covers SPEC FR-6.3 (A* to core),
    /// trap triggering, shared combat rules (dice/combat), Essence credit via EssenceManager
    /// (FR-3.1), and NFR-4 determinism (same seed -> same outcome).
    /// </summary>
    [TestFixture]
    public class InvaderWaveTests
    {
        private const int Seed = 42;

        private static DungeonGrid BuildTrapGauntlet()
        {
            var grid = new DungeonGrid(6, 10, 1);
            grid.SetTileType(1, 1, TileType.SpawnPoint);
            for (int y = 2; y <= 7; y++)
                grid.SetTileType(1, y, TileType.Corridor);
            grid.SetTileType(1, 4, TileType.Trap);
            grid.GetTile(1, 4, 0).TrapId = "spike_pit";
            grid.SetTileType(1, 7, TileType.LordChamber);
            return grid;
        }

        private static DungeonGrid BuildGarrisonDungeon()
        {
            var grid = new DungeonGrid(6, 8, 1);
            grid.SetTileType(1, 1, TileType.SpawnPoint);
            for (int y = 2; y <= 5; y++)
                grid.SetTileType(1, y, TileType.Corridor);
            grid.SetTileType(1, 4, TileType.Room);
            grid.GetTile(1, 4, 0).GarrisonedMonsters.Add("goblin_1");
            grid.SetTileType(1, 5, TileType.LordChamber);
            return grid;
        }

        [Test]
        public void PartySpawnedAtEntrance_AStarPathfindsToCore()
        {
            var sim = new InvaderWaveSimulator(BuildTrapGauntlet(), new EssenceManager(200), Seed);
            var outcome = sim.Run();

            Assert.That(outcome, Is.EqualTo(WaveOutcome.ReachedCore));
            Assert.That(sim.Path.First(), Is.EqualTo(new Pathfinding.Cell(1, 1, 0)));
            Assert.That(sim.Path.Last(), Is.EqualTo(new Pathfinding.Cell(1, 7, 0)));
            Assert.That(sim.Log.Any(e => e.Type == WaveEventType.MovedTo && e.X == 1 && e.Y == 4 && e.Z == 0), Is.True);
            Assert.That(sim.Log.Any(e => e.Type == WaveEventType.ReachedCore && e.X == 1 && e.Y == 7), Is.True);
        }

        [Test]
        public void SteppingOnTrapTile_TriggersTrapEffect()
        {
            var sim = new InvaderWaveSimulator(BuildTrapGauntlet(), new EssenceManager(200), Seed);
            sim.Run();

            var trapEvent = sim.Log.FirstOrDefault(e => e.Type == WaveEventType.TrapTriggered);
            Assert.That(trapEvent, Is.Not.Null);
            Assert.That(trapEvent.X, Is.EqualTo(1));
            Assert.That(trapEvent.Y, Is.EqualTo(4));
            Assert.That(trapEvent.Detail, Does.StartWith("spike_pit"));
            Assert.That(sim.Party.All(m => m.CurrentHp < m.MaxHp), Is.True, "trap must damage every surviving member");
        }

        [Test]
        public void CombatResolvesWithSharedRules_KillCreditsEssence()
        {
            var essence = new EssenceManager(200);
            var sim = new InvaderWaveSimulator(BuildGarrisonDungeon(), essence, Seed);
            sim.SettlementReputation = 10;
            sim.SetParty(new[]
            {
                new Combatant { Id = "canary", Name = "canary", Level = 1, CurrentHp = 1, MaxHp = 1, ArmorClass = 5, AttackBonus = 0, DamageDice = "1d1" }
            });

            var outcome = sim.Run();

            Assert.That(sim.Log.Any(e => e.Type == WaveEventType.CombatRound), Is.True, "combat must resolve via shared dice/combat rules");
            var kill = sim.Log.FirstOrDefault(e => e.Type == WaveEventType.PartyMemberKilled);
            Assert.That(kill, Is.Not.Null, "a kill must occur");
            Assert.That(kill.Detail, Does.StartWith("canary"));
            long expected = CombatRules.CalculateInvaderEssenceReward(1, 1, 10);
            Assert.That(essence.CurrentEssence, Is.EqualTo(200 + expected));
            Assert.That(outcome, Is.EqualTo(WaveOutcome.Wiped));
        }

        [Test]
        public void SameSeed_ProducesIdenticalTranscript()
        {
            var a = new InvaderWaveSimulator(BuildTrapGauntlet(), new EssenceManager(200), 999);
            var b = new InvaderWaveSimulator(BuildTrapGauntlet(), new EssenceManager(200), 999);

            a.Run();
            b.Run();

            Assert.That(Transcript(a), Is.EqualTo(Transcript(b)));
        }

        [Test]
        public void DifferentSeed_ProducesDifferentTranscript()
        {
            var a = new InvaderWaveSimulator(BuildTrapGauntlet(), new EssenceManager(200), 1);
            var b = new InvaderWaveSimulator(BuildTrapGauntlet(), new EssenceManager(200), 2);

            a.Run();
            b.Run();

            Assert.That(Transcript(a), Is.Not.EqualTo(Transcript(b)));
        }

        [Test]
        public void EssenceRewardRule_MatchesReferenceNumbers()
        {
            Assert.That(CombatRules.CalculateInvaderEssenceReward(2, 3, 20), Is.EqualTo(180));
            Assert.That(CombatRules.CalculateInvaderEssenceReward(3, 4, 50), Is.EqualTo(450));
            Assert.That(CombatRules.CalculateInvaderEssenceReward(1, 1, 10), Is.EqualTo(27));
        }

        [Test]
        public void DiceEngine_SeededRolls_AreReproducible()
        {
            var a = new DiceEngine(999);
            var b = new DiceEngine(999);

            var rollA = a.Roll("2d6+3");
            var rollB = b.Roll("2d6+3");
            Assert.That(rollA.Total, Is.EqualTo(rollB.Total));

            var advA = a.Roll("1d20adv+3");
            var advB = b.Roll("1d20adv+3");
            Assert.That(advA.Total, Is.EqualTo(advB.Total));
        }

        private static string Transcript(InvaderWaveSimulator sim)
        {
            return string.Join("\n", sim.Log) + "\nOUTCOME|" + sim.Outcome + "\nPATH|" + string.Join(";", sim.Path);
        }
    }
}