using System.Collections.Generic;
using PuzzleBattle.Domain.Grid;

namespace PuzzleBattle.Domain.Board
{
    public class BoardObject : GridObject
    {
        public BoardObject(GridObjectId id, IEnumerable<GridPosition> occupiedPositions)
            : base(id, occupiedPositions)
        {
        }
    }
}
