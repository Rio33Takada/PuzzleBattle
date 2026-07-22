using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleBattle.Domain.Grid
{
    /// <summary>
    /// Immutable grid bounds and its set of valid cells.
    /// </summary>
    public sealed class GridShape : IEquatable<GridShape>
    {
        private readonly HashSet<GridPosition> validCells;
        private readonly GridPosition[] validCellSnapshot;

        public GridShape(int width, int height, IEnumerable<GridPosition> validCells)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be positive.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be positive.");
            if (validCells == null)
                throw new ArgumentNullException(nameof(validCells));

            Width = width;
            Height = height;
            this.validCells = new HashSet<GridPosition>(validCells);

            foreach (GridPosition validCell in this.validCells)
            {
                if (!Contains(validCell))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(validCells),
                        validCell,
                        "Every valid cell must be inside the grid bounds.");
                }
            }

            validCellSnapshot = this.validCells
                .OrderBy(position => position.Y)
                .ThenBy(position => position.X)
                .ToArray();
        }

        public int Width { get; }

        public int Height { get; }

        public IReadOnlyCollection<GridPosition> ValidCells => validCellSnapshot;

        public static GridShape CreateRectangle(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be positive.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be positive.");

            var cells = new List<GridPosition>(checked(width * height));
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    cells.Add(new GridPosition(x, y));
            }

            return new GridShape(width, height, cells);
        }

        public bool Contains(GridPosition position)
        {
            return position.X >= 0 && position.X < Width &&
                   position.Y >= 0 && position.Y < Height;
        }

        public bool IsValid(GridPosition position)
        {
            return validCells.Contains(position);
        }

        public IEnumerable<GridPosition> GetValidNeighbors(GridPosition position)
        {
            if (!IsValid(position))
                yield break;

            foreach (GridPosition neighbor in position.GetOrthogonalNeighbors())
            {
                if (IsValid(neighbor))
                    yield return neighbor;
            }
        }

        public bool AreValidCellsContinuous()
        {
            if (validCells.Count == 0)
                return true;

            var visited = new HashSet<GridPosition>();
            var pending = new Queue<GridPosition>();
            pending.Enqueue(validCellSnapshot[0]);
            visited.Add(validCellSnapshot[0]);

            while (pending.Count > 0)
            {
                GridPosition current = pending.Dequeue();
                foreach (GridPosition neighbor in GetValidNeighbors(current))
                {
                    if (!visited.Add(neighbor))
                        continue;

                    pending.Enqueue(neighbor);
                }
            }

            return visited.Count == validCells.Count;
        }

        public bool Equals(GridShape other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Width == other.Width &&
                   Height == other.Height &&
                   validCells.SetEquals(other.validCells);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GridShape);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Width * 397) ^ Height;
                foreach (GridPosition position in validCellSnapshot)
                    hashCode = (hashCode * 397) ^ position.GetHashCode();
                return hashCode;
            }
        }
    }
}
