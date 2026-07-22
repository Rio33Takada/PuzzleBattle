using System;
using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Grid;

namespace PuzzleBattle.Domain.Gimmick
{
    public sealed class BombGimmick : BoardGimmick
    {
        public BombGimmick(
            GimmickId id,
            EnemyId sourceEnemyId,
            GridPosition position,
            int initialCount,
            int damage)
            : base(id, sourceEnemyId, position)
        {
            if (initialCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(initialCount), initialCount, "Initial count must be positive.");
            if (damage < 0)
                throw new ArgumentOutOfRangeException(nameof(damage), damage, "Damage cannot be negative.");

            RemainingCount = initialCount;
            Damage = damage;
        }

        public int RemainingCount { get; private set; }
        public int Damage { get; }
        public override bool CanBeOverwrittenByPiece => true;

        internal bool Advance()
        {
            if (RemainingCount <= 0)
                throw new InvalidOperationException("An exploded bomb cannot advance again.");
            RemainingCount--;
            return RemainingCount == 0;
        }
    }
}
