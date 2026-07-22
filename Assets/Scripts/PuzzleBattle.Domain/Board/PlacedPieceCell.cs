using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;
using PuzzleBattle.Domain.Placement;

namespace PuzzleBattle.Domain.Board
{
    public sealed class PlacedPieceCell : BoardCellOccupant
    {
        internal PlacedPieceCell(
            PlacedPiece placement,
            GridPosition sourcePosition,
            GridPosition boardPosition,
            int power,
            ElementAttribute attribute)
            : base(boardPosition)
        {
            Placement = placement;
            SourcePosition = sourcePosition;
            Power = power;
            Attribute = attribute;
        }

        public PlacedPiece Placement { get; }
        public GridPosition SourcePosition { get; }
        public int Power { get; }
        public ElementAttribute Attribute { get; }
        public override bool CanBeOverwritten => true;
    }
}
