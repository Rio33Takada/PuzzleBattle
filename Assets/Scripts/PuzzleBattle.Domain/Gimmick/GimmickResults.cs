using System.Collections.Generic;
using System.Linq;

namespace PuzzleBattle.Domain.Gimmick
{
    public enum GimmickPlacementFailure
    {
        None = 0,
        OutsideBoard = 1,
        CellOccupied = 2,
        DuplicateId = 3
    }

    public sealed class GimmickPlacementResult
    {
        internal GimmickPlacementResult(GimmickPlacementFailure failure, bool removedCell)
        {
            Failure = failure;
            RemovedCell = removedCell;
        }

        public bool Succeeded => Failure == GimmickPlacementFailure.None;
        public GimmickPlacementFailure Failure { get; }
        public bool RemovedCell { get; }
    }

    public sealed class BombAdvanceResult
    {
        internal BombAdvanceResult(IEnumerable<BombGimmick> explodedBombs)
        {
            ExplodedBombs = explodedBombs.ToArray();
            TotalDamage = ExplodedBombs.Sum(bomb => bomb.Damage);
        }

        public IReadOnlyCollection<BombGimmick> ExplodedBombs { get; }
        public int TotalDamage { get; }
    }
}
