using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Board;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Gimmick;
using BattleBoard = PuzzleBattle.Domain.Board.Board;

namespace PuzzleBattle.Domain.Placement
{
    public sealed class PiecePlacementService
    {
        public PiecePlacementDecision Evaluate(BattleBoard board, PiecePlacementRequest request)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!TryCreateCellData(request, out PlacedPieceCellData[] targetCells))
            {
                return new PiecePlacementDecision(
                    PiecePlacementFailure.CoordinateOverflow,
                    Array.Empty<GridPosition>());
            }

            GridPosition[] outsideCells = targetCells
                .Where(cell => !board.Range.IsValid(cell.BoardPosition))
                .Select(cell => cell.BoardPosition)
                .ToArray();
            if (outsideCells.Length > 0)
                return new PiecePlacementDecision(PiecePlacementFailure.OutsideBoard, outsideCells);

            GridPosition[] blockedCells = targetCells
                .Where(cell => board.TryGetOccupant(cell.BoardPosition, out BoardCellOccupant occupant) &&
                               !occupant.CanBeOverwritten)
                .Select(cell => cell.BoardPosition)
                .ToArray();
            if (blockedCells.Length > 0)
                return new PiecePlacementDecision(PiecePlacementFailure.OverwriteProhibited, blockedCells);

            return new PiecePlacementDecision(
                PiecePlacementFailure.None,
                targetCells.Select(cell => cell.BoardPosition));
        }

        public PiecePlacementResult TryPlace(BattleBoard board, PiecePlacementRequest request)
        {
            PiecePlacementDecision decision = Evaluate(board, request);
            if (!decision.CanPlace)
                return new PiecePlacementResult(decision, null);

            TryCreateCellData(request, out PlacedPieceCellData[] targetCells);
            var placedPiece = new PlacedPiece(request.Piece, targetCells);
            var removedGimmicks = new List<BoardGimmick>();
            var overwrittenPieceCells = new List<PlacedPieceCell>();

            foreach (PlacedPieceCell newCell in placedPiece.RemainingCells
                         .OrderBy(cell => cell.BoardPosition.Y)
                         .ThenBy(cell => cell.BoardPosition.X))
            {
                if (board.TryGetOccupant(newCell.BoardPosition, out BoardCellOccupant occupant))
                {
                    if (occupant is PlacedPieceCell overwrittenCell)
                    {
                        board.Remove(overwrittenCell);
                        board.RecordOverwrite(request.Piece.OwnerId, overwrittenCell.Attribute);
                        overwrittenPieceCells.Add(overwrittenCell);
                    }
                    else if (occupant is BoardGimmickCell gimmickCell)
                    {
                        board.RemoveOccupant(gimmickCell);
                        removedGimmicks.Add(gimmickCell.Gimmick);
                    }
                    else
                    {
                        throw new InvalidOperationException("An overwriteable occupant type is unsupported.");
                    }
                }

                board.Add(newCell);
            }

            return new PiecePlacementResult(
                decision,
                placedPiece,
                removedGimmicks,
                overwrittenPieceCells);
        }

        private static bool TryCreateCellData(
            PiecePlacementRequest request,
            out PlacedPieceCellData[] cells)
        {
            try
            {
                cells = request.Piece.Cells.Select(cell =>
                {
                    var boardPosition = new GridPosition(
                        checked(cell.Position.X + request.Translation.X),
                        checked(cell.Position.Y + request.Translation.Y));
                    return new PlacedPieceCellData(
                        cell.Position,
                        boardPosition,
                        request.Piece.GetPower(cell.Position),
                        request.Piece.GetAttribute(cell.Position));
                }).ToArray();
                return true;
            }
            catch (OverflowException)
            {
                cells = Array.Empty<PlacedPieceCellData>();
                return false;
            }
        }
    }
}
