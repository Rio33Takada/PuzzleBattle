using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleBattle.Domain.Grid
{
    public abstract class GridObject
    {
        private GridObjectCell[] cells;

        protected GridObject(GridObjectId id, IEnumerable<GridPosition> occupiedPositions)
        {
            if (string.IsNullOrWhiteSpace(id.Value))
                throw new ArgumentException("A grid object ID is required.", nameof(id));
            if (occupiedPositions == null)
                throw new ArgumentNullException(nameof(occupiedPositions));

            GridPosition[] positions = ValidatePositions(occupiedPositions);

            Id = id;
            SetCells(positions);
        }

        public GridObjectId Id { get; }

        public IReadOnlyCollection<GridObjectCell> Cells => cells;

        protected void Relocate(IEnumerable<GridPosition> occupiedPositions)
        {
            SetCells(ValidatePositions(occupiedPositions));
        }

        private GridPosition[] ValidatePositions(IEnumerable<GridPosition> occupiedPositions)
        {
            if (occupiedPositions == null)
                throw new ArgumentNullException(nameof(occupiedPositions));

            GridPosition[] positions = occupiedPositions.ToArray();
            if (positions.Length == 0)
                throw new ArgumentException("A grid object must occupy at least one cell.", nameof(occupiedPositions));
            if (positions.Distinct().Count() != positions.Length)
                throw new ArgumentException("A grid object cannot contain duplicate cells.", nameof(occupiedPositions));
            if (!AreContinuous(positions))
                throw new ArgumentException("A grid object must occupy continuous cells.", nameof(occupiedPositions));

            return positions;
        }

        private void SetCells(IEnumerable<GridPosition> positions)
        {
            cells = positions
                .OrderBy(position => position.Y)
                .ThenBy(position => position.X)
                .Select(position => new GridObjectCell(this, position))
                .ToArray();
        }

        private static bool AreContinuous(IReadOnlyCollection<GridPosition> positions)
        {
            var all = new HashSet<GridPosition>(positions);
            var visited = new HashSet<GridPosition>();
            var pending = new Queue<GridPosition>();
            GridPosition first = positions.First();
            visited.Add(first);
            pending.Enqueue(first);

            while (pending.Count > 0)
            {
                GridPosition current = pending.Dequeue();
                foreach (GridPosition neighbor in current.GetOrthogonalNeighbors())
                {
                    if (!all.Contains(neighbor) || !visited.Add(neighbor))
                        continue;

                    pending.Enqueue(neighbor);
                }
            }

            return visited.Count == all.Count;
        }
    }
}
