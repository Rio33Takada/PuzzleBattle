using PuzzleBattle.Domain.Board;

namespace PuzzleBattle.Domain.Gimmick
{
    public sealed class BoardGimmickCell : BoardCellOccupant
    {
        public BoardGimmickCell(BoardGimmick gimmick)
            : base(gimmick.Position)
        {
            Gimmick = gimmick;
        }

        public BoardGimmick Gimmick { get; }
        public override bool CanBeOverwritten => Gimmick.CanBeOverwrittenByPiece;
    }
}
