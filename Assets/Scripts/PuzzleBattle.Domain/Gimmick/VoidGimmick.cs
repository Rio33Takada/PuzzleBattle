using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Grid;

namespace PuzzleBattle.Domain.Gimmick
{
    public sealed class VoidGimmick : BoardGimmick
    {
        public VoidGimmick(GimmickId id, EnemyId sourceEnemyId, GridPosition position)
            : base(id, sourceEnemyId, position)
        {
        }

        public override bool CanBeOverwrittenByPiece => false;
    }
}
