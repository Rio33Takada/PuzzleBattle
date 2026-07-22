using System.Collections.Generic;

namespace PuzzleBattle.Domain.Enemy
{
    public interface IEnemyMovementPriority
    {
        IEnumerable<GridDirection> GetDirections(EnemyUnit enemy);
    }
}
