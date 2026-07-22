using System;
using PuzzleBattle.Domain.BattleResult;
using PuzzleBattle.Domain.Randomness;

namespace PuzzleBattle.Application.BattleFlow
{
    public enum BattleFlowPhase
    {
        NotStarted = 0,
        PlayerTurn = 1,
        ResolvingEnemyTurn = 2,
        AwaitingAreaTransition = 3,
        Finished = 4
    }

    public sealed class BattleFlowSession
    {
        public BattleFlowSession(IRandomSource random)
        {
            Random = random ?? throw new ArgumentNullException(nameof(random));
            Phase = BattleFlowPhase.NotStarted;
            LastOutcome = BattleResolutionOutcome.Continuing;
        }

        public IRandomSource Random { get; }
        public int TurnNumber { get; private set; }
        public BattleFlowPhase Phase { get; private set; }
        public BattleResolutionOutcome LastOutcome { get; private set; }

        internal void StartFirstTurn()
        {
            TurnNumber = 1;
            Phase = BattleFlowPhase.PlayerTurn;
            LastOutcome = BattleResolutionOutcome.Continuing;
        }

        internal void BeginEnemyResolution()
        {
            Phase = BattleFlowPhase.ResolvingEnemyTurn;
        }

        internal void CompleteEnemyResolution(BattleResolutionOutcome outcome)
        {
            LastOutcome = outcome;
            switch (outcome)
            {
                case BattleResolutionOutcome.Continuing:
                    TurnNumber = checked(TurnNumber + 1);
                    Phase = BattleFlowPhase.PlayerTurn;
                    break;
                case BattleResolutionOutcome.AreaCleared:
                    Phase = BattleFlowPhase.AwaitingAreaTransition;
                    break;
                case BattleResolutionOutcome.Defeat:
                case BattleResolutionOutcome.Victory:
                    Phase = BattleFlowPhase.Finished;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null);
            }
        }
    }
}
