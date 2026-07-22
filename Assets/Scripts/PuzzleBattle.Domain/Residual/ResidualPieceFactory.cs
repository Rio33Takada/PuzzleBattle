using System;
using PuzzleBattle.Domain.Placement;

namespace PuzzleBattle.Domain.Residual
{
    public sealed class ResidualPieceFactory
    {
        public ResidualPiece CreateWhenMissedFieldObject(
            ResidualPieceId id,
            PlacedPiece placedPiece,
            bool hitFieldObject)
        {
            if (placedPiece == null)
                throw new ArgumentNullException(nameof(placedPiece));

            return hitFieldObject ? null : new ResidualPiece(id, placedPiece);
        }
    }
}
