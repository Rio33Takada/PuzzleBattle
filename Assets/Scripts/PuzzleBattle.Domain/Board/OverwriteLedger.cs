using System.Collections.Generic;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Piece;

namespace PuzzleBattle.Domain.Board
{
    internal sealed class OverwriteLedger
    {
        private readonly Dictionary<CharacterId, Dictionary<ElementAttribute, int>> counts =
            new Dictionary<CharacterId, Dictionary<ElementAttribute, int>>();

        public void Record(CharacterId ownerId, ElementAttribute attribute)
        {
            if (!counts.TryGetValue(ownerId, out Dictionary<ElementAttribute, int> byAttribute))
            {
                byAttribute = new Dictionary<ElementAttribute, int>();
                counts.Add(ownerId, byAttribute);
            }

            byAttribute.TryGetValue(attribute, out int current);
            byAttribute[attribute] = checked(current + 1);
        }

        public int GetCount(CharacterId ownerId, ElementAttribute attribute)
        {
            return counts.TryGetValue(ownerId, out Dictionary<ElementAttribute, int> byAttribute) &&
                   byAttribute.TryGetValue(attribute, out int count)
                ? count
                : 0;
        }

        public void Remove(CharacterId ownerId, ElementAttribute attribute)
        {
            if (!counts.TryGetValue(ownerId, out Dictionary<ElementAttribute, int> byAttribute) ||
                !byAttribute.TryGetValue(attribute, out int current) || current <= 0)
            {
                throw new System.InvalidOperationException("The overwrite count cannot be removed.");
            }

            if (current == 1)
            {
                byAttribute.Remove(attribute);
                if (byAttribute.Count == 0)
                    counts.Remove(ownerId);
                return;
            }

            byAttribute[attribute] = current - 1;
        }
    }
}
