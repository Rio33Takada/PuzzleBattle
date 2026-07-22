using PuzzleBattle.Domain.BattleResult;

namespace PuzzleBattle.Application.BattleFlow
{
    public interface IBattleStartService
    {
        void Execute(BattleFlowSession session);
    }

    public interface IPlayerTurnStartService
    {
        void Execute(BattleFlowSession session);
    }

    public interface IStunTurnService
    {
        void Execute(BattleFlowSession session);
    }

    public interface IEnemyMovementTurnService
    {
        void Execute(BattleFlowSession session);
    }

    /// <summary>Applies every damage event produced by the current resolution step.</summary>
    public interface IDamageResolutionService
    {
        void Execute(BattleFlowSession session);
    }

    public interface IDestructionResolutionService
    {
        void Execute(BattleFlowSession session);
    }

    public interface IDeathEvaluationService
    {
        void Execute(BattleFlowSession session);
    }

    /// <summary>Evaluates defeat before victory, as defined by the domain battle resolution.</summary>
    public interface IBattleOutcomeEvaluationService
    {
        BattleResolutionOutcome Evaluate(BattleFlowSession session);
    }

    public interface IEnemyTurnEndService
    {
        void Execute(BattleFlowSession session);
    }
}
