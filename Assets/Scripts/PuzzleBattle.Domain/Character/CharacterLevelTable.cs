using System;
using System.Collections.Generic;

namespace PuzzleBattle.Domain.Character
{
    public sealed class CharacterLevelTable
    {
        private readonly Dictionary<int, CharacterLevelStats> statsByLevel;

        public CharacterLevelTable(IEnumerable<KeyValuePair<int, CharacterLevelStats>> entries)
        {
            if (entries == null)
                throw new ArgumentNullException(nameof(entries));

            statsByLevel = new Dictionary<int, CharacterLevelStats>();
            foreach (KeyValuePair<int, CharacterLevelStats> entry in entries)
            {
                if (entry.Key <= 0)
                    throw new ArgumentOutOfRangeException(nameof(entries), entry.Key, "Levels must be positive.");
                if (!statsByLevel.TryAdd(entry.Key, entry.Value))
                    throw new ArgumentException("Level entries must be unique.", nameof(entries));
            }

            if (statsByLevel.Count == 0)
                throw new ArgumentException("At least one level entry is required.", nameof(entries));
        }

        public CharacterLevelStats GetStats(int level)
        {
            if (!statsByLevel.TryGetValue(level, out CharacterLevelStats stats))
                throw new ArgumentOutOfRangeException(nameof(level), level, "The level is not defined in the table.");
            return stats;
        }
    }
}
