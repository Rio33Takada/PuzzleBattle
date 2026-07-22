using System.Collections.Generic;
using PuzzleBattle.Domain.Grid;

namespace PuzzleBattle.Domain.Field
{
    public class FieldObject : GridObject
    {
        public FieldObject(GridObjectId id, IEnumerable<GridPosition> occupiedPositions)
            : base(id, occupiedPositions)
        {
        }
    }
}
