using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Grid;
using BattleBoard = PuzzleBattle.Domain.Board.Board;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Application.PieceCandidates
{
    public sealed class PieceCandidate
    {
        internal PieceCandidate(BattlePiece piece, IEnumerable<GridPosition> validTranslations)
        {
            Piece = piece ?? throw new ArgumentNullException(nameof(piece));
            ValidTranslations = (validTranslations ?? throw new ArgumentNullException(nameof(validTranslations))).ToArray();
        }

        public BattlePiece Piece { get; }
        public IReadOnlyList<GridPosition> ValidTranslations { get; }
        public bool CanPlace => ValidTranslations.Count > 0;
    }

    public sealed class PieceCandidateState
    {
        public const int MaximumVisibleCandidates = 3;
        private readonly List<BattlePiece> remainingPieces;
        private readonly IPiecePlacementAvailabilityService availability;
        private PieceCandidate[] candidates = Array.Empty<PieceCandidate>();

        internal PieceCandidateState(
            IEnumerable<BattlePiece> shuffledPieces,
            IPiecePlacementAvailabilityService availability,
            BattleBoard board)
        {
            remainingPieces = (shuffledPieces ?? throw new ArgumentNullException(nameof(shuffledPieces))).ToList();
            if (remainingPieces.Any(piece => piece == null))
                throw new ArgumentException("Shuffled pieces cannot contain null.", nameof(shuffledPieces));
            this.availability = availability ?? throw new ArgumentNullException(nameof(availability));
            Refresh(board);
        }

        public IReadOnlyList<PieceCandidate> Candidates => candidates;
        public int RemainingPieceCount => remainingPieces.Count;
        public bool CanConfirm => candidates.All(candidate => !candidate.CanPlace);

        public void Refresh(BattleBoard board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            candidates = remainingPieces
                .Take(MaximumVisibleCandidates)
                .Select(piece => new PieceCandidate(
                    piece,
                    availability.FindValidTranslations(board, piece)))
                .ToArray();
        }

        public ConsumedPieceCandidate ConsumePlaced(BattlePiece piece, BattleBoard board)
        {
            if (piece == null)
                throw new ArgumentNullException(nameof(piece));
            PieceCandidate candidate = candidates.FirstOrDefault(item => ReferenceEquals(item.Piece, piece));
            if (candidate == null)
                throw new InvalidOperationException("Only a visible candidate can be consumed.");
            if (!candidate.CanPlace)
                throw new InvalidOperationException("A placement-impossible candidate cannot be consumed.");

            int originalIndex = remainingPieces.IndexOf(piece);
            remainingPieces.RemoveAt(originalIndex);
            Refresh(board);
            return new ConsumedPieceCandidate(piece, originalIndex);
        }

        public void RestoreConsumed(ConsumedPieceCandidate consumed, BattleBoard board)
        {
            if (consumed == null)
                throw new ArgumentNullException(nameof(consumed));
            if (remainingPieces.Contains(consumed.Piece))
                throw new InvalidOperationException("The consumed piece has already been restored.");
            if (consumed.OriginalIndex < 0 || consumed.OriginalIndex > remainingPieces.Count)
                throw new InvalidOperationException("The consumed piece position is no longer valid.");

            remainingPieces.Insert(consumed.OriginalIndex, consumed.Piece);
            Refresh(board);
        }
    }

    public sealed class ConsumedPieceCandidate
    {
        internal ConsumedPieceCandidate(BattlePiece piece, int originalIndex)
        {
            Piece = piece;
            OriginalIndex = originalIndex;
        }

        public BattlePiece Piece { get; }
        internal int OriginalIndex { get; }
    }
}
