namespace PuzzleBattle.Domain.Placement
{
    public sealed class PiecePlacementResult
    {
        internal PiecePlacementResult(
            PiecePlacementDecision decision,
            PlacedPiece placedPiece,
            System.Collections.Generic.IEnumerable<Gimmick.BoardGimmick> removedGimmicks = null,
            System.Collections.Generic.IEnumerable<Board.PlacedPieceCell> overwrittenPieceCells = null)
        {
            Decision = decision;
            PlacedPiece = placedPiece;
            RemovedGimmicks = removedGimmicks == null
                ? System.Array.Empty<Gimmick.BoardGimmick>()
                : System.Linq.Enumerable.ToArray(removedGimmicks);
            OverwrittenPieceCells = overwrittenPieceCells == null
                ? System.Array.Empty<Board.PlacedPieceCell>()
                : System.Linq.Enumerable.ToArray(overwrittenPieceCells);
        }

        public bool Succeeded => Decision.CanPlace;
        public PiecePlacementDecision Decision { get; }
        public PlacedPiece PlacedPiece { get; }
        public System.Collections.Generic.IReadOnlyCollection<Gimmick.BoardGimmick> RemovedGimmicks { get; }
        public System.Collections.Generic.IReadOnlyCollection<Board.PlacedPieceCell> OverwrittenPieceCells { get; }
    }
}
