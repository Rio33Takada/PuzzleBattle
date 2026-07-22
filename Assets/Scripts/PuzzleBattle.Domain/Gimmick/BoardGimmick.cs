using System;
using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Grid;

namespace PuzzleBattle.Domain.Gimmick
{
    public abstract class BoardGimmick
    {
        protected BoardGimmick(GimmickId id, EnemyId sourceEnemyId, GridPosition position)
        {
            if (string.IsNullOrWhiteSpace(id.Value))
                throw new ArgumentException("A gimmick ID is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(sourceEnemyId.Value))
                throw new ArgumentException("A source enemy is required.", nameof(sourceEnemyId));

            Id = id;
            SourceEnemyId = sourceEnemyId;
            Position = position;
        }

        public GimmickId Id { get; }
        public EnemyId SourceEnemyId { get; }
        public GridPosition Position { get; }
        public abstract bool CanBeOverwrittenByPiece { get; }
    }
}
