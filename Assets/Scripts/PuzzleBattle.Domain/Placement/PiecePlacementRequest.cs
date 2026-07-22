using System;
using PuzzleBattle.Domain.Grid;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Domain.Placement
{
    /// <summary>
    /// Describes translation only. Rotation and reflection are intentionally not representable.
    /// </summary>
    public sealed class PiecePlacementRequest
    {
        public PiecePlacementRequest(BattlePiece piece, GridPosition translation)
        {
            Piece = piece ?? throw new ArgumentNullException(nameof(piece));
            Translation = translation;
        }

        public BattlePiece Piece { get; }

        public GridPosition Translation { get; }
    }
}
