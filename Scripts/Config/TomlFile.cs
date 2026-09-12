using System;
using System.Collections.Generic;
using System.IO;

namespace DungeonLord.Scripts.Config
{
    /// <summary>
    /// TOML section: ordered key/value pairs (preserves file order — important for
    /// deterministic weighted pools across the Python and C# generators).
    /// </summary>
    public class TomlSection
    {
        public List<KeyValuePair<string, string>> Pairs { get; } = new();

        public string Get(string key)
        {
            foreach (var pair in Pairs)
            {
                if (pair.Key == key) return pair.Value;
            }
            return null;
        }
    }

    /// <summary>
    /// Minimal, dependency-free TOML-subset reader used by the game to load
    /// balance config from data/*.toml. Supports:
    ///   [section] / [section.nested] tables, key = value (integers and quoted
    ///   strings), and '#' comments. Values such as booleans/floats are not
    ///   needed by current configs and are kept as raw strings.
    /// </summary>
    public static class TomlFile
    {
        public static Dictionary<string, TomlSection> Parse(string text)
        {
            var sections = new Dictionary<string, TomlSection>(StringComparer.Ordinal);
            string current = "";

            foreach (var rawLine in text.Split('\n'))
            {
                string line = StripComment(rawLine).Trim();
                if (line.Length == 0) continue;

                if (line[0] == '[' && line[line.Length - 1] == ']')
                {
                    current = line.Substring(1, line.Length - 2).Trim();
                    if (!sections.ContainsKey(current)) sections[current] = new TomlSection();
                    continue;
                }

                int eq = line.IndexOf('=');
                if (eq <= 0) continue;

                string key = line.Substring(0, eq).Trim();
                string value = line.Substring(eq + 1).Trim();
                if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
                {
                    value = value.Substring(1, value.Length - 2);
                }

                if (!sections.TryGetValue(current, out var section))
                {
                    section = new TomlSection();
                    sections[current] = section;
                }
                section.Pairs.Add(new KeyValuePair<string, string>(key, value));
            }

            return sections;
        }

        public static Dictionary<string, TomlSection> ParseFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"TOML file not found: {path}");
            }
            return Parse(File.ReadAllText(path));
        }

        private static string StripComment(string line)
        {
            bool inString = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inString = !inString;
                }
                else if (c == '#' && !inString)
                {
                    return line.Substring(0, i);
                }
            }
            return line;
        }
    }
}