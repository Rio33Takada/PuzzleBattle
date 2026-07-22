using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Board;
using PuzzleBattle.Domain.Grid;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Domain.Placement
{
    public sealed class PlacedPiece
    {
        private readonly List<PlacedPieceCell> remainingCells;

        internal PlacedPiece(BattlePiece piece, IEnumerable<PlacedPieceCellData> cells)
        {
            Piece = piece ?? throw new ArgumentNullException(nameof(piece));
            remainingCells = cells.Select(cell => new PlacedPieceCell(
                this,
                cell.SourcePosition,
                cell.BoardPosition,
                cell.Power,
                cell.Attribute)).ToList();
        }

        public BattlePiece Piece { get; }

        public IReadOnlyCollection<PlacedPieceCell> RemainingCells => remainingCells;

        internal void Remove(PlacedPieceCell cell)
        {
            if (!remainingCells.Remove(cell))
                throw new InvalidOperationException("The cell does not belong to this placement.");
        }

        internal void Restore(PlacedPieceCell cell)
        {
            if (cell == null)
                throw new ArgumentNullException(nameof(cell));
            if (!ReferenceEquals(cell.Placement, this) || remainingCells.Contains(cell))
                throw new InvalidOperationException("The cell cannot be restored to this placement.");
            remainingCells.Add(cell);
        }
    }

    internal readonly struct PlacedPieceCellData
    {
        public PlacedPieceCellData(
            GridPosition sourcePosition,
            GridPosition boardPosition,
            int power,
            Piece.ElementAttribute attribute)
        {
            SourcePosition = sourcePosition;
            BoardPosition = boardPosition;
            Power = power;
            Attribute = attribute;
        }

        public GridPosition SourcePosition { get; }
        public GridPosition BoardPosition { get; }
        public int Power { get; }
        public Piece.ElementAttribute Attribute { get; }
    }
}
