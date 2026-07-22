using System;
using System.Collections.Generic;

namespace PuzzleBattle.Domain.Grid
{
    internal static class GridObjectCollectionValidator
    {
        public static void Validate<TObject>(GridShape shape, IReadOnlyCollection<TObject> objects)
            where TObject : GridObject
        {
            var occupiedPositions = new HashSet<GridPosition>();
            var ids = new HashSet<GridObjectId>();

            foreach (TObject gridObject in objects)
            {
                if (gridObject == null)
                    throw new ArgumentException("A grid object collection cannot contain null.", nameof(objects));
                if (!ids.Add(gridObject.Id))
                    throw new ArgumentException("Grid object IDs must be unique within an aggregate.", nameof(objects));

                foreach (GridObjectCell cell in gridObject.Cells)
                {
                    if (!shape.IsValid(cell.Position))
                        throw new ArgumentException("Every occupied cell must be valid in the aggregate range.", nameof(objects));
                    if (!occupiedPositions.Add(cell.Position))
                        throw new ArgumentException("Grid objects cannot occupy the same cell.", nameof(objects));
                }
            }
        }
    }
}
