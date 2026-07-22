using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Placement;

namespace PuzzleBattle.Domain.Residual
{
    public sealed class ResidualPiece
    {
        private readonly GridPosition[] occupiedPositions;

        internal ResidualPiece(ResidualPieceId id, PlacedPiece source)
        {
            if (string.IsNullOrWhiteSpace(id.Value))
                throw new ArgumentException("A residual piece ID is required.", nameof(id));
            Source = source ?? throw new ArgumentNullException(nameof(source));

            occupiedPositions = source.RemainingCells
                .Select(cell => cell.BoardPosition)
                .Distinct()
                .OrderBy(position => position.Y)
                .ThenBy(position => position.X)
                .ToArray();
            if (occupiedPositions.Length == 0)
                throw new ArgumentException("A residual piece must contain at least one cell.", nameof(source));

            Id = id;
            InitialCellCount = occupiedPositions.Length;
            Durability = InitialCellCount;
        }

        public ResidualPieceId Id { get; }
        public PlacedPiece Source { get; }
        public IReadOnlyCollection<GridPosition> OccupiedPositions => occupiedPositions;
        public int InitialCellCount { get; }
        public int Durability { get; private set; }
        public bool IsDestroyed { get; private set; }

        internal bool ReceiveContact()
        {
            if (IsDestroyed)
                return false;

            Durability--;
            if (Durability > 0)
                return false;

            IsDestroyed = true;
            return true;
        }

        internal bool ForceDestroy()
        {
            if (IsDestroyed)
                return false;

            Durability = 0;
            IsDestroyed = true;
            return true;
        }
    }
}
