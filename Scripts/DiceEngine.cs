using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DungeonLord.Scripts
{
    public readonly struct RollResult
    {
        public string Expression { get; }
        public int Total { get; }
        public int[] Rolls { get; }
        public int Modifier { get; }
        public string AdvantageMode { get; }
        public bool IsCrit { get; }
        public bool IsFumble { get; }

        public RollResult(string expression, int total, int[] rolls, int modifier, string advantageMode, bool isCrit, bool isFumble)
        {
            Expression = expression;
            Total = total;
            Rolls = rolls;
            Modifier = modifier;
            AdvantageMode = advantageMode;
            IsCrit = isCrit;
            IsFumble = isFumble;
        }
    }

    /// <summary>
    /// Deterministic dice engine mirroring dungeon_master/dice.py (standard notation,
    /// advantage/disadvantage, keep-highest). Same seed -> same sequence of rolls.
    /// </summary>
    public class DiceEngine
    {
        private readonly System.Random _rng;

        public DiceEngine(int? seed = null)
        {
            _rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        }

        public RollResult Roll(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Dice expression cannot be empty.");

            string expr = expression.Trim().ToLowerInvariant();
            string advantageMode = null;

            if (expr.Contains("adv"))
            {
                advantageMode = "advantage";
                expr = expr.Replace("adv", "");
            }
            else if (expr.Contains("dis"))
            {
                advantageMode = "disadvantage";
                expr = expr.Replace("dis", "");
            }

            var kh = Regex.Match(expr, @"^(\d*)d(\d+)kh(\d+)([+-]\d+)?$");
            if (kh.Success)
            {
                int count = ParseCount(kh.Groups[1].Value);
                int sides = int.Parse(kh.Groups[2].Value);
                int keep = int.Parse(kh.Groups[3].Value);
                int mod = ParseMod(kh.Groups[4].Value);

                int[] raw = RollDice(count, sides);
                int total = raw.OrderByDescending(v => v).Take(keep).Sum() + mod;
                return new RollResult(expression, total, raw, mod, advantageMode, false, false);
            }

            var standard = Regex.Match(expr, @"^(\d*)d(\d+)([+-]\d+)?$");
            if (standard.Success)
            {
                int count = ParseCount(standard.Groups[1].Value);
                int sides = int.Parse(standard.Groups[2].Value);
                int mod = ParseMod(standard.Groups[3].Value);

                if (advantageMode != null && count == 1 && sides == 20)
                {
                    int roll1 = _rng.Next(1, sides + 1);
                    int roll2 = _rng.Next(1, sides + 1);
                    int chosen = advantageMode == "advantage" ? Math.Max(roll1, roll2) : Math.Min(roll1, roll2);
                    return new RollResult(expression, chosen + mod, new[] { roll1, roll2 }, mod, advantageMode, chosen == 20, chosen == 1);
                }

                int[] raw = RollDice(count, sides);
                bool isCrit = count == 1 && sides == 20 && raw[0] == 20;
                bool isFumble = count == 1 && sides == 20 && raw[0] == 1;
                return new RollResult(expression, raw.Sum() + mod, raw, mod, advantageMode, isCrit, isFumble);
            }

            if (int.TryParse(expr, out int flat))
                return new RollResult(expression, flat, new[] { flat }, flat, null, false, false);

            throw new ArgumentException($"Invalid dice expression: {expression}");
        }

        private int[] RollDice(int count, int sides)
        {
            var rolls = new int[count];
            for (int i = 0; i < count; i++)
                rolls[i] = _rng.Next(1, sides + 1);
            return rolls;
        }

        private static int ParseCount(string s) => string.IsNullOrEmpty(s) ? 1 : int.Parse(s);
        private static int ParseMod(string s) => string.IsNullOrEmpty(s) ? 0 : int.Parse(s);
    }
}