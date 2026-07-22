using System;
using System.Collections.Generic;
using PuzzleBattle.Application.BattleFlow;
using PuzzleBattle.Domain.Gimmick;
using PuzzleBattle.Domain.Skill;
using BattleBoard = PuzzleBattle.Domain.Board.Board;

namespace PuzzleBattle.Application.PlayerTurn
{
    public interface IActiveSkillTurnService
    {
        SkillExecutionResult Execute(SkillDefinition skill);
    }

    public interface IBombConfirmationService
    {
        BombAdvanceResult Advance();
    }

    public interface IPassiveSkillTurnService
    {
        IReadOnlyList<SkillExecutionResult> Apply(TurnOverwriteSummary overwrites);
    }

    public interface IConfirmedPlacementContinuation
    {
        BattleTurnResult Continue();
    }

    public sealed class SkillTurnService : IActiveSkillTurnService, IPassiveSkillTurnService
    {
        private readonly SkillResolver resolver;
        private readonly Func<SkillExecutionContext> activeContextFactory;
        private readonly Func<TurnOverwriteSummary, SkillExecutionContext> passiveContextFactory;
        private readonly IReadOnlyCollection<SkillDefinition> passiveSkills;

        public SkillTurnService(
            SkillResolver resolver,
            Func<SkillExecutionContext> activeContextFactory,
            Func<TurnOverwriteSummary, SkillExecutionContext> passiveContextFactory,
            IReadOnlyCollection<SkillDefinition> passiveSkills)
        {
            this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            this.activeContextFactory = activeContextFactory ?? throw new ArgumentNullException(nameof(activeContextFactory));
            this.passiveContextFactory = passiveContextFactory ?? throw new ArgumentNullException(nameof(passiveContextFactory));
            this.passiveSkills = passiveSkills ?? throw new ArgumentNullException(nameof(passiveSkills));
        }

        public SkillExecutionResult Execute(SkillDefinition skill)
        {
            return resolver.ExecuteActive(skill, activeContextFactory());
        }

        public IReadOnlyList<SkillExecutionResult> Apply(TurnOverwriteSummary overwrites)
        {
            return resolver.ApplyPassivesAfterPlacement(
                passiveSkills,
                passiveContextFactory(overwrites));
        }
    }

    public sealed class BoardBombConfirmationService : IBombConfirmationService
    {
        private readonly BattleBoard board;
        private readonly BoardGimmickService gimmicks;

        public BoardBombConfirmationService(BattleBoard board, BoardGimmickService gimmicks = null)
        {
            this.board = board ?? throw new ArgumentNullException(nameof(board));
            this.gimmicks = gimmicks ?? new BoardGimmickService();
        }

        public BombAdvanceResult Advance() => gimmicks.AdvanceBombsAfterPieceConfirmation(board);
    }

    public sealed class BattleTurnContinuation : IConfirmedPlacementContinuation
    {
        private readonly BattleTurnUseCase useCase;
        private readonly BattleFlowSession session;

        public BattleTurnContinuation(BattleTurnUseCase useCase, BattleFlowSession session)
        {
            this.useCase = useCase ?? throw new ArgumentNullException(nameof(useCase));
            this.session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public BattleTurnResult Continue() => useCase.ResolveEnemyTurn(session);
    }
}
