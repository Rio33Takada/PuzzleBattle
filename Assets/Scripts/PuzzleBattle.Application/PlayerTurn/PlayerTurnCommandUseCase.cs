using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Application.BattleFlow;
using PuzzleBattle.Application.PieceCandidates;
using PuzzleBattle.Domain.Gimmick;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Placement;
using PuzzleBattle.Domain.Skill;
using BattleBoard = PuzzleBattle.Domain.Board.Board;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Application.PlayerTurn
{
    public sealed class PlayerTurnConfirmationResult
    {
        internal PlayerTurnConfirmationResult(
            BombAdvanceResult bombs,
            IReadOnlyList<SkillExecutionResult> passives,
            BattleTurnResult continuation)
        {
            Bombs = bombs;
            Passives = passives ?? throw new ArgumentNullException(nameof(passives));
            Continuation = continuation ?? throw new ArgumentNullException(nameof(continuation));
        }

        public BombAdvanceResult Bombs { get; }
        public IReadOnlyList<SkillExecutionResult> Passives { get; }
        public BattleTurnResult Continuation { get; }
    }

    public sealed class PlayerTurnCommandUseCase
    {
        private readonly BattleBoard board;
        private readonly PieceCandidateState candidates;
        private readonly PiecePlacementService placement;
        private readonly PiecePlacementUndoService undo;
        private readonly IActiveSkillTurnService activeSkills;
        private readonly IBombConfirmationService bombs;
        private readonly IPassiveSkillTurnService passiveSkills;
        private readonly IConfirmedPlacementContinuation continuation;
        private readonly List<PlacementRecord> placements = new List<PlacementRecord>();
        private bool activeSkillWindowOpen = true;

        public PlayerTurnCommandUseCase(
            BattleBoard board,
            PieceCandidateState candidates,
            IActiveSkillTurnService activeSkills,
            IBombConfirmationService bombs,
            IPassiveSkillTurnService passiveSkills,
            IConfirmedPlacementContinuation continuation,
            PiecePlacementService placement = null,
            PiecePlacementUndoService undo = null)
        {
            this.board = board ?? throw new ArgumentNullException(nameof(board));
            this.candidates = candidates ?? throw new ArgumentNullException(nameof(candidates));
            this.activeSkills = activeSkills ?? throw new ArgumentNullException(nameof(activeSkills));
            this.bombs = bombs ?? throw new ArgumentNullException(nameof(bombs));
            this.passiveSkills = passiveSkills ?? throw new ArgumentNullException(nameof(passiveSkills));
            this.continuation = continuation ?? throw new ArgumentNullException(nameof(continuation));
            this.placement = placement ?? new PiecePlacementService();
            this.undo = undo ?? new PiecePlacementUndoService();
        }

        public bool IsConfirmed { get; private set; }
        public bool CanUseActiveSkill => !IsConfirmed && activeSkillWindowOpen;
        public bool CanUndo => !IsConfirmed && placements.Count > 0;
        public bool CanConfirm
        {
            get
            {
                if (IsConfirmed)
                    return false;
                candidates.Refresh(board);
                return candidates.CanConfirm;
            }
        }
        public IReadOnlyList<PlacedPiece> PlacedPieces =>
            placements.Select(record => record.Placement.PlacedPiece).ToArray();

        public SkillExecutionResult UseActiveSkill(SkillDefinition skill)
        {
            EnsureNotConfirmed();
            if (!activeSkillWindowOpen)
                throw new InvalidOperationException("Active skills can only be used before the first successful placement.");
            return activeSkills.Execute(skill);
        }

        public PiecePlacementResult Place(BattlePiece piece, GridPosition translation)
        {
            EnsureNotConfirmed();
            if (piece == null)
                throw new ArgumentNullException(nameof(piece));

            candidates.Refresh(board);
            PieceCandidate candidate = candidates.Candidates
                .FirstOrDefault(item => ReferenceEquals(item.Piece, piece));
            if (candidate == null)
                throw new InvalidOperationException("Only a visible candidate can be placed.");
            if (!candidate.ValidTranslations.Contains(translation))
                throw new InvalidOperationException("The candidate cannot be placed at the requested translation.");

            PiecePlacementResult result = placement.TryPlace(
                board,
                new PiecePlacementRequest(piece, translation));
            if (!result.Succeeded)
                return result;

            ConsumedPieceCandidate consumed = candidates.ConsumePlaced(piece, board);
            placements.Add(new PlacementRecord(result, consumed));
            activeSkillWindowOpen = false;
            return result;
        }

        public bool UndoLastPlacement()
        {
            EnsureNotConfirmed();
            if (placements.Count == 0)
                return false;

            int lastIndex = placements.Count - 1;
            PlacementRecord record = placements[lastIndex];
            undo.Undo(board, record.Placement);
            candidates.RestoreConsumed(record.ConsumedCandidate, board);
            placements.RemoveAt(lastIndex);
            return true;
        }

        public PlayerTurnConfirmationResult Confirm()
        {
            EnsureNotConfirmed();
            candidates.Refresh(board);
            if (!candidates.CanConfirm)
                throw new InvalidOperationException("Every placement-possible candidate must be placed before confirmation.");

            IsConfirmed = true;
            BombAdvanceResult bombResult = bombs.Advance();
            var overwrites = new TurnOverwriteSummary();
            foreach (PlacementRecord record in placements)
                overwrites.Add(record.Placement);
            IReadOnlyList<SkillExecutionResult> passiveResults = passiveSkills.Apply(overwrites);
            BattleTurnResult next = continuation.Continue();
            return new PlayerTurnConfirmationResult(bombResult, passiveResults, next);
        }

        private void EnsureNotConfirmed()
        {
            if (IsConfirmed)
                throw new InvalidOperationException("Player-turn commands are unavailable after confirmation.");
        }

        private sealed class PlacementRecord
        {
            public PlacementRecord(PiecePlacementResult placement, ConsumedPieceCandidate consumedCandidate)
            {
                Placement = placement;
                ConsumedCandidate = consumedCandidate;
            }

            public PiecePlacementResult Placement { get; }
            public ConsumedPieceCandidate ConsumedCandidate { get; }
        }
    }
}
