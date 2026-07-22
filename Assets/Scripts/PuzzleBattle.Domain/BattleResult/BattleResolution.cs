using System;

namespace PuzzleBattle.Domain.BattleResult
{
    public enum BattleResolutionOutcome
    {
        Continuing = 0,
        Defeat = 1,
        AreaCleared = 2,
        Victory = 3
    }

    public sealed class BattleResolution
    {
        internal BattleResolution(
            BattleResolutionOutcome outcome,
            BattleAreaState currentArea,
            BattleAreaState? nextArea)
        {
            if (outcome == BattleResolutionOutcome.AreaCleared && !nextArea.HasValue)
                throw new ArgumentException("An area-clear result requires the next area.", nameof(nextArea));
            if (outcome != BattleResolutionOutcome.AreaCleared && nextArea.HasValue)
                throw new ArgumentException("Only an area-clear result can contain the next area.", nameof(nextArea));

            Outcome = outcome;
            CurrentArea = currentArea;
            NextArea = nextArea;
        }

        public BattleResolutionOutcome Outcome { get; }
        public BattleAreaState CurrentArea { get; }
        public BattleAreaState? NextArea { get; }
        public bool BattleFinished =>
            Outcome == BattleResolutionOutcome.Defeat || Outcome == BattleResolutionOutcome.Victory;
    }
}
