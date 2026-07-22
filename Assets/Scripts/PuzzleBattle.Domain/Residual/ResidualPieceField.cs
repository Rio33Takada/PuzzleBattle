using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Grid;

namespace PuzzleBattle.Domain.Residual
{
    public sealed class ResidualPieceField
    {
        private readonly Dictionary<GridPosition, ResidualPiece> residualByPosition =
            new Dictionary<GridPosition, ResidualPiece>();
        private readonly Dictionary<ResidualPieceId, ResidualPiece> residualById =
            new Dictionary<ResidualPieceId, ResidualPiece>();

        public ResidualPieceField(GridShape range)
        {
            Range = range ?? throw new ArgumentNullException(nameof(range));
        }

        public GridShape Range { get; }

        public IReadOnlyCollection<ResidualPiece> ResidualPieces => residualById.Values.ToArray();

        public void Add(ResidualPiece residualPiece)
        {
            if (residualPiece == null)
                throw new ArgumentNullException(nameof(residualPiece));
            if (residualPiece.IsDestroyed)
                throw new ArgumentException("A destroyed residual piece cannot be added.", nameof(residualPiece));
            if (residualById.ContainsKey(residualPiece.Id))
                throw new ArgumentException("Residual piece IDs must be unique.", nameof(residualPiece));

            foreach (GridPosition position in residualPiece.OccupiedPositions)
            {
                if (!Range.IsValid(position))
                    throw new ArgumentException("Every residual piece cell must be inside the field range.", nameof(residualPiece));
                if (residualByPosition.ContainsKey(position))
                    throw new ArgumentException("Residual pieces cannot occupy the same cell.", nameof(residualPiece));
            }

            residualById.Add(residualPiece.Id, residualPiece);
            foreach (GridPosition position in residualPiece.OccupiedPositions)
                residualByPosition.Add(position, residualPiece);
        }

        public ResidualPiece FindAt(GridPosition position)
        {
            residualByPosition.TryGetValue(position, out ResidualPiece residualPiece);
            return residualPiece;
        }

        public ResidualContactResult ResolveEnemyContact(GridPosition position)
        {
            ResidualPiece residualPiece = FindAt(position);
            if (residualPiece == null)
                return new ResidualContactResult(null, false, false, 0);

            bool destroyed = residualPiece.ReceiveContact();
            if (!destroyed)
                return new ResidualContactResult(residualPiece, true, false, 0);

            Remove(residualPiece);

            return new ResidualContactResult(
                residualPiece,
                true,
                true,
                residualPiece.InitialCellCount);
        }

        internal ResidualContactResult DestroyImmediately(ResidualPiece residualPiece)
        {
            if (residualPiece == null)
                throw new ArgumentNullException(nameof(residualPiece));
            if (!residualById.TryGetValue(residualPiece.Id, out ResidualPiece current) ||
                !ReferenceEquals(current, residualPiece))
            {
                return new ResidualContactResult(residualPiece, false, false, 0);
            }

            if (!residualPiece.ForceDestroy())
                return new ResidualContactResult(residualPiece, false, false, 0);

            Remove(residualPiece);
            return new ResidualContactResult(
                residualPiece,
                true,
                true,
                residualPiece.InitialCellCount);
        }

        private void Remove(ResidualPiece residualPiece)
        {
            residualById.Remove(residualPiece.Id);
            foreach (GridPosition occupiedPosition in residualPiece.OccupiedPositions)
                residualByPosition.Remove(occupiedPosition);
        }
    }
}
