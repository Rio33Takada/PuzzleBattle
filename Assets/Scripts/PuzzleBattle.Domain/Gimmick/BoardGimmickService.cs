using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Board;
using PuzzleBattle.Domain.Placement;
using BattleBoard = PuzzleBattle.Domain.Board.Board;

namespace PuzzleBattle.Domain.Gimmick
{
    public sealed class BoardGimmickService
    {
        public GimmickPlacementResult TryPlace(BattleBoard board, BoardGimmick gimmick)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));
            if (gimmick == null)
                throw new ArgumentNullException(nameof(gimmick));
            if (!board.Range.IsValid(gimmick.Position))
                return new GimmickPlacementResult(GimmickPlacementFailure.OutsideBoard, false);
            if (board.GetGimmicks().Any(existing => existing.Id.Equals(gimmick.Id)))
                return new GimmickPlacementResult(GimmickPlacementFailure.DuplicateId, false);

            if (gimmick is VoidGimmick)
            {
                bool removed = false;
                if (board.TryGetOccupant(gimmick.Position, out BoardCellOccupant occupant))
                {
                    board.RemoveOccupant(occupant);
                    removed = true;
                }

                return new GimmickPlacementResult(GimmickPlacementFailure.None, removed);
            }

            if (board.IsOccupied(gimmick.Position))
                return new GimmickPlacementResult(GimmickPlacementFailure.CellOccupied, false);

            board.Add(new BoardGimmickCell(gimmick));
            return new GimmickPlacementResult(GimmickPlacementFailure.None, false);
        }

        public IReadOnlyCollection<IceGimmick> ResolveIceAdjacentTo(
            BattleBoard board,
            PlacedPiece placedPiece)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));
            if (placedPiece == null)
                throw new ArgumentNullException(nameof(placedPiece));

            var melted = new List<IceGimmick>();
            IceGimmick[] iceGimmicks = board.GetGimmicks().OfType<IceGimmick>().ToArray();
            foreach (IceGimmick ice in iceGimmicks)
            {
                bool hasMatchingNeighbor = placedPiece.RemainingCells.Any(cell =>
                    cell.Attribute == ice.MeltingAttribute &&
                    cell.BoardPosition.IsOrthogonallyAdjacentTo(ice.Position));
                if (!hasMatchingNeighbor)
                    continue;

                var occupant = (BoardGimmickCell)GetRequiredOccupant(board, ice.Position);
                board.RemoveOccupant(occupant);
                melted.Add(ice);
            }

            return melted;
        }

        public BombAdvanceResult AdvanceBombsAfterPieceConfirmation(BattleBoard board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            var exploded = new List<BombGimmick>();
            BombGimmick[] bombs = board.GetGimmicks().OfType<BombGimmick>().ToArray();
            foreach (BombGimmick bomb in bombs.OrderBy(item => item.Id.Value, StringComparer.Ordinal))
            {
                if (!bomb.Advance())
                    continue;

                board.RemoveOccupant(GetRequiredOccupant(board, bomb.Position));
                exploded.Add(bomb);
            }

            return new BombAdvanceResult(exploded);
        }

        private static BoardCellOccupant GetRequiredOccupant(BattleBoard board, Grid.GridPosition position)
        {
            if (!board.TryGetOccupant(position, out BoardCellOccupant occupant))
                throw new InvalidOperationException("Expected board occupant was not found.");
            return occupant;
        }
    }
}
