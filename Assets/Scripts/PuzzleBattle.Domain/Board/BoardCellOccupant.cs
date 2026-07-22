using PuzzleBattle.Domain.Grid;

namespace PuzzleBattle.Domain.Board
{
    public abstract class BoardCellOccupant
    {
        protected BoardCellOccupant(GridPosition boardPosition)
        {
            BoardPosition = boardPosition;
        }

        public GridPosition BoardPosition { get; }

        public abstract bool CanBeOverwritten { get; }
    }

    internal sealed class BoardObjectCellOccupant : BoardCellOccupant
    {
        public BoardObjectCellOccupant(BoardObject parent, GridObjectCell cell)
            : base(cell.Position)
        {
            Parent = parent;
            Cell = cell;
        }

        public BoardObject Parent { get; }

        public GridObjectCell Cell { get; }

        public override bool CanBeOverwritten => false;
    }
}
