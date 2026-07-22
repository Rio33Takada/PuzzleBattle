using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Piece;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Domain.Character
{
    public sealed class Character
    {
        private readonly BattlePiece[] pieces;
        private readonly ElementAttribute[] attributes;

        public Character(
            CharacterId id,
            int level,
            CharacterLevelTable levelTable,
            IEnumerable<BattlePiece> pieces)
        {
            if (string.IsNullOrWhiteSpace(id.Value))
                throw new ArgumentException("A character ID is required.", nameof(id));
            if (levelTable == null)
                throw new ArgumentNullException(nameof(levelTable));
            if (pieces == null)
                throw new ArgumentNullException(nameof(pieces));

            this.pieces = pieces.ToArray();
            if (this.pieces.Length == 0)
                throw new ArgumentException("A character must own at least one piece.", nameof(pieces));
            if (this.pieces.Any(piece => piece == null))
                throw new ArgumentException("Owned pieces cannot contain null.", nameof(pieces));
            if (this.pieces.Any(piece => piece.OwnerId != id))
                throw new ArgumentException("Every piece must reference its owning character.", nameof(pieces));
            if (this.pieces.Select(piece => piece.PieceId).Distinct().Count() != this.pieces.Length)
                throw new ArgumentException("Owned piece IDs must be unique.", nameof(pieces));

            Id = id;
            Level = level;
            Stats = levelTable.GetStats(level);
            attributes = this.pieces.SelectMany(piece => piece.Attributes).Distinct().ToArray();
        }

        public CharacterId Id { get; }
        public int Level { get; }
        public CharacterLevelStats Stats { get; }
        public IReadOnlyCollection<BattlePiece> Pieces => pieces;
        public IReadOnlyCollection<ElementAttribute> Attributes => attributes;
    }
}
