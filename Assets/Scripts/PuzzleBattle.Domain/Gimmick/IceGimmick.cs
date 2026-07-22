using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;

namespace PuzzleBattle.Domain.Gimmick
{
    public sealed class IceGimmick : BoardGimmick
    {
        public IceGimmick(
            GimmickId id,
            EnemyId sourceEnemyId,
            GridPosition position,
            ElementAttribute meltingAttribute)
            : base(id, sourceEnemyId, position)
        {
            if (string.IsNullOrWhiteSpace(meltingAttribute.Value))
                throw new System.ArgumentException("A melting attribute is required.", nameof(meltingAttribute));
            MeltingAttribute = meltingAttribute;
        }

        public ElementAttribute MeltingAttribute { get; }
        public override bool CanBeOverwrittenByPiece => false;
    }
}
