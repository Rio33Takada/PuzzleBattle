using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleBattle.Domain.Enemy
{
    public sealed class FixedDirectionPriority : IEnemyMovementPriority
    {
        private readonly GridDirection[] directions;

        public FixedDirectionPriority(IEnumerable<GridDirection> directions)
        {
            if (directions == null)
                throw new ArgumentNullException(nameof(directions));
            this.directions = directions.Distinct().ToArray();
            if (this.directions.Length == 0)
                throw new ArgumentException("At least one movement direction is required.", nameof(directions));
        }

        public IEnumerable<GridDirection> GetDirections(EnemyUnit enemy)
        {
            if (enemy == null)
                throw new ArgumentNullException(nameof(enemy));
            return directions;
        }
    }
}
