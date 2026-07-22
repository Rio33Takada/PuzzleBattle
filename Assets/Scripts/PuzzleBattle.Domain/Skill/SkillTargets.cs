using System;
using System.Collections.Generic;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Piece;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Domain.Skill
{
    public sealed class CharacterCombatModifiers
    {
        private readonly Dictionary<CharacterId, int> attackBonuses = new Dictionary<CharacterId, int>();

        public void AddAttack(CharacterId characterId, int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Attack increase cannot be negative.");
            attackBonuses.TryGetValue(characterId, out int current);
            attackBonuses[characterId] = checked(current + amount);
        }

        public int GetAttackBonus(CharacterId characterId)
        {
            return attackBonuses.TryGetValue(characterId, out int value) ? value : 0;
        }
    }

    public sealed class GrantedPiecePool
    {
        private readonly List<BattlePiece> pieces = new List<BattlePiece>();
        public IReadOnlyCollection<BattlePiece> Pieces => pieces;

        public void Add(BattlePiece piece)
        {
            if (piece == null)
                throw new ArgumentNullException(nameof(piece));
            pieces.Add(piece);
        }
    }
}
