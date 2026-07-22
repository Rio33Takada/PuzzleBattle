using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Gimmick;
using BattleField = PuzzleBattle.Domain.Field.Field;

namespace PuzzleBattle.Domain.Board
{
    public sealed class Board
    {
        private readonly BoardObject[] objects;
        private readonly Dictionary<GridPosition, BoardCellOccupant> occupants;
        private readonly OverwriteLedger overwriteLedger;

        public Board(GridShape range, BattleField activeField, IEnumerable<BoardObject> objects = null)
        {
            Range = range ?? throw new ArgumentNullException(nameof(range));
            ActiveField = activeField ?? throw new ArgumentNullException(nameof(activeField));
            if (!Range.Equals(ActiveField.Range))
                throw new ArgumentException("The board range must match the active field range.", nameof(range));

            this.objects = objects == null ? Array.Empty<BoardObject>() : objects.ToArray();
            GridObjectCollectionValidator.Validate(Range, this.objects);
            occupants = new Dictionary<GridPosition, BoardCellOccupant>();
            foreach (BoardObject boardObject in this.objects)
            {
                foreach (GridObjectCell cell in boardObject.Cells)
                    occupants.Add(cell.Position, new BoardObjectCellOccupant(boardObject, cell));
            }

            overwriteLedger = new OverwriteLedger();
        }

        public GridShape Range { get; }

        public BattleField ActiveField { get; }

        public IReadOnlyCollection<BoardObject> Objects => objects;

        public PlacedPieceCell GetPlacedPieceCell(GridPosition position)
        {
            return occupants.TryGetValue(position, out BoardCellOccupant occupant)
                ? occupant as PlacedPieceCell
                : null;
        }

        public BoardGimmick GetGimmick(GridPosition position)
        {
            return occupants.TryGetValue(position, out BoardCellOccupant occupant) &&
                   occupant is BoardGimmickCell gimmickCell
                ? gimmickCell.Gimmick
                : null;
        }

        public IReadOnlyCollection<BoardGimmick> GetGimmicks()
        {
            return occupants.Values
                .OfType<BoardGimmickCell>()
                .Select(cell => cell.Gimmick)
                .ToArray();
        }

        public bool IsOccupied(GridPosition position)
        {
            return occupants.ContainsKey(position);
        }

        public int GetOverwriteCount(Character.CharacterId ownerId, Piece.ElementAttribute attribute)
        {
            return overwriteLedger.GetCount(ownerId, attribute);
        }

        internal bool TryGetOccupant(GridPosition position, out BoardCellOccupant occupant)
        {
            return occupants.TryGetValue(position, out occupant);
        }

        internal void Remove(PlacedPieceCell cell)
        {
            if (cell == null)
                throw new ArgumentNullException(nameof(cell));
            if (!occupants.TryGetValue(cell.BoardPosition, out BoardCellOccupant current) || !ReferenceEquals(current, cell))
                throw new InvalidOperationException("The placed piece cell is not present on this board.");

            occupants.Remove(cell.BoardPosition);
            cell.Placement.Remove(cell);
        }

        internal void Add(PlacedPieceCell cell)
        {
            if (cell == null)
                throw new ArgumentNullException(nameof(cell));
            occupants.Add(cell.BoardPosition, cell);
        }

        internal void Add(BoardGimmickCell cell)
        {
            if (cell == null)
                throw new ArgumentNullException(nameof(cell));
            occupants.Add(cell.BoardPosition, cell);
        }

        internal void RemoveOccupant(BoardCellOccupant occupant)
        {
            if (occupant == null)
                throw new ArgumentNullException(nameof(occupant));
            if (!occupants.TryGetValue(occupant.BoardPosition, out BoardCellOccupant current) ||
                !ReferenceEquals(current, occupant))
            {
                throw new InvalidOperationException("The occupant is not present on this board.");
            }

            if (occupant is PlacedPieceCell placedPieceCell)
                placedPieceCell.Placement.Remove(placedPieceCell);

            occupants.Remove(occupant.BoardPosition);
        }

        internal void RecordOverwrite(
            Character.CharacterId ownerId,
            Piece.ElementAttribute overwrittenAttribute)
        {
            overwriteLedger.Record(ownerId, overwrittenAttribute);
        }

        internal void RemoveOverwriteRecord(
            Character.CharacterId ownerId,
            Piece.ElementAttribute overwrittenAttribute)
        {
            overwriteLedger.Remove(ownerId, overwrittenAttribute);
        }
    }
}
