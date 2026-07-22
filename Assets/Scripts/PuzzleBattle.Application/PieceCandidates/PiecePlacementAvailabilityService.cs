using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Placement;
using BattleBoard = PuzzleBattle.Domain.Board.Board;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Application.PieceCandidates
{
    public interface IPiecePlacementAvailabilityService
    {
        IReadOnlyList<GridPosition> FindValidTranslations(BattleBoard board, BattlePiece piece);
    }

    public sealed class PiecePlacementAvailabilityService : IPiecePlacementAvailabilityService
    {
        private readonly PiecePlacementService placementService;

        public PiecePlacementAvailabilityService(PiecePlacementService placementService = null)
        {
            this.placementService = placementService ?? new PiecePlacementService();
        }

        public IReadOnlyList<GridPosition> FindValidTranslations(BattleBoard board, BattlePiece piece)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));
            if (piece == null)
                throw new ArgumentNullException(nameof(piece));

            var translations = new HashSet<GridPosition>();
            foreach (GridPosition target in board.Range.ValidCells)
            {
                foreach (GridObjectCell sourceCell in piece.Cells)
                {
                    if (!TrySubtract(target, sourceCell.Position, out GridPosition translation) ||
                        !translations.Add(translation))
                    {
                        continue;
                    }

                    var request = new PiecePlacementRequest(piece, translation);
                    if (!placementService.Evaluate(board, request).CanPlace)
                        translations.Remove(translation);
                }
            }

            return translations
                .OrderBy(position => position.Y)
                .ThenBy(position => position.X)
                .ToArray();
        }

        private static bool TrySubtract(
            GridPosition target,
            GridPosition source,
            out GridPosition translation)
        {
            try
            {
                translation = new GridPosition(
                    checked(target.X - source.X),
                    checked(target.Y - source.Y));
                return true;
            }
            catch (OverflowException)
            {
                translation = default;
                return false;
            }
        }
    }
}
