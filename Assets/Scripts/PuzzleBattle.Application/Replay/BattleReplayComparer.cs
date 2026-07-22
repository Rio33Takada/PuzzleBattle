using System;

namespace PuzzleBattle.Application.Replay
{
    public sealed class BattleReplayComparison
    {
        internal BattleReplayComparison(
            bool matches,
            int? firstDifferentTurn,
            BattleReplaySnapshot expected,
            BattleReplaySnapshot actual)
        {
            Matches = matches;
            FirstDifferentTurn = firstDifferentTurn;
            Expected = expected;
            Actual = actual;
        }
        public bool Matches { get; }
        public int? FirstDifferentTurn { get; }
        public BattleReplaySnapshot Expected { get; }
        public BattleReplaySnapshot Actual { get; }
    }

    public sealed class BattleReplayComparer
    {
        public BattleReplayComparison Compare(BattleReplayResult expected, BattleReplayResult actual)
        {
            if (expected == null) throw new ArgumentNullException(nameof(expected));
            if (actual == null) throw new ArgumentNullException(nameof(actual));
            if (expected.StageSeed != actual.StageSeed)
                return new BattleReplayComparison(false, 0, null, null);

            int sharedCount = Math.Min(expected.TurnSnapshots.Count, actual.TurnSnapshots.Count);
            for (int index = 0; index < sharedCount; index++)
            {
                BattleReplaySnapshot left = expected.TurnSnapshots[index];
                BattleReplaySnapshot right = actual.TurnSnapshots[index];
                if (!left.Equals(right))
                    return new BattleReplayComparison(false, index + 1, left, right);
            }
            if (expected.TurnSnapshots.Count != actual.TurnSnapshots.Count)
            {
                BattleReplaySnapshot left = sharedCount < expected.TurnSnapshots.Count ? expected.TurnSnapshots[sharedCount] : null;
                BattleReplaySnapshot right = sharedCount < actual.TurnSnapshots.Count ? actual.TurnSnapshots[sharedCount] : null;
                return new BattleReplayComparison(false, sharedCount + 1, left, right);
            }
            return new BattleReplayComparison(true, null, null, null);
        }
    }
}
