using System;
using PuzzleBattle.Domain.BattleResult;

namespace PuzzleBattle.Application.BattleFlow
{
    public sealed class BattleTurnResult
    {
        public BattleTurnResult(
            int resolvedTurnNumber,
            BattleResolutionOutcome outcome,
            BattleFlowPhase nextPhase,
            int nextTurnNumber)
        {
            ResolvedTurnNumber = resolvedTurnNumber;
            Outcome = outcome;
            NextPhase = nextPhase;
            NextTurnNumber = nextTurnNumber;
        }

        public int ResolvedTurnNumber { get; }
        public BattleResolutionOutcome Outcome { get; }
        public BattleFlowPhase NextPhase { get; }
        public int NextTurnNumber { get; }
    }

    public sealed class BattleTurnUseCase
    {
        private readonly IBattleStartService battleStart;
        private readonly IPlayerTurnStartService playerTurnStart;
        private readonly IStunTurnService stun;
        private readonly IEnemyMovementTurnService movement;
        private readonly IDamageResolutionService damage;
        private readonly IDestructionResolutionService destruction;
        private readonly IDeathEvaluationService death;
        private readonly IBattleOutcomeEvaluationService outcome;
        private readonly IEnemyTurnEndService enemyTurnEnd;

        public BattleTurnUseCase(
            IBattleStartService battleStart,
            IPlayerTurnStartService playerTurnStart,
            IStunTurnService stun,
            IEnemyMovementTurnService movement,
            IDamageResolutionService damage,
            IDestructionResolutionService destruction,
            IDeathEvaluationService death,
            IBattleOutcomeEvaluationService outcome,
            IEnemyTurnEndService enemyTurnEnd)
        {
            this.battleStart = battleStart ?? throw new ArgumentNullException(nameof(battleStart));
            this.playerTurnStart = playerTurnStart ?? throw new ArgumentNullException(nameof(playerTurnStart));
            this.stun = stun ?? throw new ArgumentNullException(nameof(stun));
            this.movement = movement ?? throw new ArgumentNullException(nameof(movement));
            this.damage = damage ?? throw new ArgumentNullException(nameof(damage));
            this.destruction = destruction ?? throw new ArgumentNullException(nameof(destruction));
            this.death = death ?? throw new ArgumentNullException(nameof(death));
            this.outcome = outcome ?? throw new ArgumentNullException(nameof(outcome));
            this.enemyTurnEnd = enemyTurnEnd ?? throw new ArgumentNullException(nameof(enemyTurnEnd));
        }

        public void StartBattle(BattleFlowSession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (session.Phase != BattleFlowPhase.NotStarted)
                throw new InvalidOperationException("Battle can only start from the not-started phase.");

            battleStart.Execute(session);
            session.StartFirstTurn();
            playerTurnStart.Execute(session);
        }

        public BattleTurnResult ResolveEnemyTurn(BattleFlowSession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (session.Phase != BattleFlowPhase.PlayerTurn)
                throw new InvalidOperationException("Enemy turn can only resolve after the player turn.");

            int resolvedTurn = session.TurnNumber;
            session.BeginEnemyResolution();

            stun.Execute(session);
            movement.Execute(session);
            damage.Execute(session);
            destruction.Execute(session);
            death.Execute(session);
            BattleResolutionOutcome resolution = outcome.Evaluate(session);

            if (resolution == BattleResolutionOutcome.Continuing)
                enemyTurnEnd.Execute(session);

            session.CompleteEnemyResolution(resolution);

            if (resolution == BattleResolutionOutcome.Continuing)
                playerTurnStart.Execute(session);

            return new BattleTurnResult(
                resolvedTurn,
                resolution,
                session.Phase,
                session.TurnNumber);
        }
    }
}
