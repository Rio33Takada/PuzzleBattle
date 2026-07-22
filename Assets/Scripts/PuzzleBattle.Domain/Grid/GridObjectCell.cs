namespace PuzzleBattle.Domain.Grid
{
    public sealed class GridObjectCell
    {
        internal GridObjectCell(GridObject parent, GridPosition position)
        {
            Parent = parent;
            Position = position;
        }

        public GridObject Parent { get; }

        public GridPosition Position { get; }
    }
}
