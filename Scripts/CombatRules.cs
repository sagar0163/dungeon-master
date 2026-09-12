using System;

namespace DungeonLord.Scripts
{
    public class Combatant
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int CurrentHp { get; set; }
        public int MaxHp { get; set; }
        public int ArmorClass { get; set; }
        public int AttackBonus { get; set; }
        public string DamageDice { get; set; } = "1d6";

        public bool IsAlive => CurrentHp > 0;

        public void ApplyDamage(int damage) => CurrentHp = Math.Max(0, CurrentHp - damage);
    }

    public readonly struct AttackResult
    {
        public bool Hit { get; }
        public bool IsCrit { get; }
        public int Damage { get; }
        public int AttackTotal { get; }
        public int DamageTotal { get; }

        public AttackResult(bool hit, bool isCrit, int damage, int attackTotal, int damageTotal)
        {
            Hit = hit;
            IsCrit = isCrit;
            Damage = damage;
            AttackTotal = attackTotal;
            DamageTotal = damageTotal;
        }
    }

    /// <summary>
    /// Shared combat rules (d20 attack vs AC, damage dice, double dice on crit) and the
    /// invader Essence reward formula, mirroring dungeon_master/combat.py and rules.py.
    /// </summary>
    public static class CombatRules
    {
        public static AttackResult ResolveAttack(Combatant attacker, Combatant target, DiceEngine dice)
        {
            if (attacker == null) throw new ArgumentNullException(nameof(attacker));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (dice == null) throw new ArgumentNullException(nameof(dice));

            var attack = dice.Roll(WithBonus("1d20", attacker.AttackBonus));
            bool hit = attack.IsCrit || (!attack.IsFumble && attack.Total >= target.ArmorClass);

            int damageTotal = 0;
            if (hit)
            {
                damageTotal = dice.Roll(attacker.DamageDice).Total;
                if (attack.IsCrit)
                    damageTotal += dice.Roll(attacker.DamageDice).Total;
            }

            return new AttackResult(hit, attack.IsCrit, damageTotal, attack.Total, damageTotal);
        }

        public static int CalculateInvaderEssenceReward(int invaderLevel, int invaderCount, double settlementReputation)
        {
            double baseEssence = 25 * (double)invaderCount * invaderLevel;
            double repMultiplier = 1.0 + (settlementReputation / 100.0);
            return (int)(baseEssence * repMultiplier);
        }

        private static string WithBonus(string expr, int bonus)
        {
            if (bonus > 0) return expr + "+" + bonus;
            if (bonus < 0) return expr + bonus;
            return expr;
        }
    }
}