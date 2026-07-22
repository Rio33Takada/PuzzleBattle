using System;
using System.Linq;
using PuzzleBattle.Domain.Board;
using PuzzleBattle.Domain.Gimmick;
using BattleBoard = PuzzleBattle.Domain.Board.Board;

namespace PuzzleBattle.Domain.Placement
{
    public sealed class PiecePlacementUndoService
    {
        public void Undo(BattleBoard board, PiecePlacementResult placement)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));
            if (placement == null)
                throw new ArgumentNullException(nameof(placement));
            if (!placement.Succeeded || placement.PlacedPiece == null)
                throw new InvalidOperationException("Only a successful placement can be undone.");

            PlacedPieceCell[] placedCells = placement.PlacedPiece.RemainingCells
                .OrderByDescending(cell => cell.BoardPosition.Y)
                .ThenByDescending(cell => cell.BoardPosition.X)
                .ToArray();
            foreach (PlacedPieceCell cell in placedCells)
                board.Remove(cell);

            foreach (PlacedPieceCell overwritten in placement.OverwrittenPieceCells
                         .OrderBy(cell => cell.BoardPosition.Y)
                         .ThenBy(cell => cell.BoardPosition.X))
            {
                overwritten.Placement.Restore(overwritten);
                board.Add(overwritten);
                board.RemoveOverwriteRecord(
                    placement.PlacedPiece.Piece.OwnerId,
                    overwritten.Attribute);
            }

            foreach (BoardGimmick gimmick in placement.RemovedGimmicks
                         .OrderBy(item => item.Position.Y)
                         .ThenBy(item => item.Position.X))
            {
                board.Add(new BoardGimmickCell(gimmick));
            }
        }
    }
}
