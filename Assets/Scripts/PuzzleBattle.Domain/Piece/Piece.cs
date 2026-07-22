using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Board;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Grid;

namespace PuzzleBattle.Domain.Piece
{
    public sealed class Piece : BoardObject
    {
        private readonly Dictionary<GridPosition, int> powerByCell;
        private readonly Dictionary<GridPosition, ElementAttribute> attributeByCell;
        private readonly ElementAttribute[] attributes;

        public Piece(
            PieceId id,
            CharacterId ownerId,
            IReadOnlyDictionary<GridPosition, int> powerByCell,
            IReadOnlyDictionary<GridPosition, ElementAttribute> attributeByCell)
            : base(new GridObjectId(RequirePieceId(id)), RequirePowerMap(powerByCell).Keys)
        {
            if (string.IsNullOrWhiteSpace(ownerId.Value))
                throw new ArgumentException("A piece owner is required.", nameof(ownerId));
            if (attributeByCell == null)
                throw new ArgumentNullException(nameof(attributeByCell));
            if (!new HashSet<GridPosition>(powerByCell.Keys).SetEquals(attributeByCell.Keys))
                throw new ArgumentException("Power and attribute maps must describe exactly the same cells.", nameof(attributeByCell));
            if (powerByCell.Values.Any(power => power < 0))
                throw new ArgumentOutOfRangeException(nameof(powerByCell), "Cell power cannot be negative.");
            if (attributeByCell.Values.Any(attribute => string.IsNullOrWhiteSpace(attribute.Value)))
                throw new ArgumentException("Every cell must have a valid attribute.", nameof(attributeByCell));

            PieceId = id;
            OwnerId = ownerId;
            this.powerByCell = new Dictionary<GridPosition, int>(powerByCell);
            this.attributeByCell = new Dictionary<GridPosition, ElementAttribute>(attributeByCell);
            attributes = this.attributeByCell.Values.Distinct().ToArray();
        }

        public PieceId PieceId { get; }
        public CharacterId OwnerId { get; }
        public IReadOnlyCollection<ElementAttribute> Attributes => attributes;

        public int GetPower(GridPosition cell) => powerByCell.TryGetValue(cell, out int value)
            ? value
            : throw new ArgumentOutOfRangeException(nameof(cell), cell, "The cell is not part of this piece.");

        public ElementAttribute GetAttribute(GridPosition cell) => attributeByCell.TryGetValue(cell, out ElementAttribute value)
            ? value
            : throw new ArgumentOutOfRangeException(nameof(cell), cell, "The cell is not part of this piece.");

        private static string RequirePieceId(PieceId id)
        {
            if (string.IsNullOrWhiteSpace(id.Value))
                throw new ArgumentException("A piece ID is required.", nameof(id));
            return id.Value;
        }

        private static IReadOnlyDictionary<GridPosition, int> RequirePowerMap(
            IReadOnlyDictionary<GridPosition, int> powerMap)
        {
            if (powerMap == null)
                throw new ArgumentNullException(nameof(powerMap));
            return powerMap;
        }
    }
}
