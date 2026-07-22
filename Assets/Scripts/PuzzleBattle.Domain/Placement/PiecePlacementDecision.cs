using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Grid;

namespace PuzzleBattle.Domain.Placement
{
    public enum PiecePlacementFailure
    {
        None = 0,
        OutsideBoard = 1,
        OverwriteProhibited = 2,
        CoordinateOverflow = 3
    }

    public sealed class PiecePlacementDecision
    {
        internal PiecePlacementDecision(
            PiecePlacementFailure failure,
            IEnumerable<GridPosition> affectedCells)
        {
            Failure = failure;
            AffectedCells = affectedCells.ToArray();
        }

        public bool CanPlace => Failure == PiecePlacementFailure.None;

        public PiecePlacementFailure Failure { get; }

        public IReadOnlyList<GridPosition> AffectedCells { get; }
    }
}
