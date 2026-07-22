using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Field;
using PuzzleBattle.Domain.Grid;

namespace PuzzleBattle.Domain.Enemy
{
    /// <summary>
    /// Field-side body used as the boundary for enemy movement and contact damage.
    /// </summary>
    public sealed class EnemyFieldObject : FieldObject
    {
        public EnemyFieldObject(EnemyId enemyId, IEnumerable<GridPosition> occupiedPositions)
            : base(new GridObjectId(RequireId(enemyId)), occupiedPositions)
        {
            if (Cells.Count != 1)
                throw new ArgumentException("The initial enemy body must contain exactly one part.", nameof(occupiedPositions));
            EnemyId = enemyId;
        }

        public EnemyId EnemyId { get; }

        public GridPosition Position => Cells.Single().Position;

        internal void MoveTo(GridPosition destination)
        {
            Relocate(new[] { destination });
        }

        private static string RequireId(EnemyId enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId.Value))
                throw new ArgumentException("An enemy ID is required.", nameof(enemyId));
            return enemyId.Value;
        }
    }
}
