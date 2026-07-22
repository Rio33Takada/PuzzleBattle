using System;
using System.Collections.Generic;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Piece;
using PuzzleBattle.Domain.Placement;

namespace PuzzleBattle.Domain.Skill
{
    public sealed class TurnOverwriteSummary
    {
        private readonly Dictionary<CharacterId, Dictionary<ElementAttribute, int>> counts =
            new Dictionary<CharacterId, Dictionary<ElementAttribute, int>>();

        public void Add(PiecePlacementResult placementResult)
        {
            if (placementResult == null)
                throw new ArgumentNullException(nameof(placementResult));
            if (!placementResult.Succeeded || placementResult.PlacedPiece == null)
                return;

            CharacterId ownerId = placementResult.PlacedPiece.Piece.OwnerId;
            foreach (Board.PlacedPieceCell overwrittenCell in placementResult.OverwrittenPieceCells)
            {
                if (!counts.TryGetValue(ownerId, out Dictionary<ElementAttribute, int> byAttribute))
                {
                    byAttribute = new Dictionary<ElementAttribute, int>();
                    counts.Add(ownerId, byAttribute);
                }

                byAttribute.TryGetValue(overwrittenCell.Attribute, out int current);
                byAttribute[overwrittenCell.Attribute] = checked(current + 1);
            }
        }

        public int GetCount(CharacterId ownerId, ElementAttribute attribute)
        {
            return counts.TryGetValue(ownerId, out Dictionary<ElementAttribute, int> byAttribute) &&
                   byAttribute.TryGetValue(attribute, out int count)
                ? count
                : 0;
        }
    }
}
