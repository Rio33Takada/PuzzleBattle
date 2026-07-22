using System;

namespace PuzzleBattle.Domain.Character
{
    public readonly struct CharacterLevelStats : IEquatable<CharacterLevelStats>
    {
        public CharacterLevelStats(int baseHitPoints, int baseAttack)
        {
            if (baseHitPoints <= 0)
                throw new ArgumentOutOfRangeException(nameof(baseHitPoints), baseHitPoints, "Base HP must be positive.");
            if (baseAttack < 0)
                throw new ArgumentOutOfRangeException(nameof(baseAttack), baseAttack, "Base attack cannot be negative.");

            BaseHitPoints = baseHitPoints;
            BaseAttack = baseAttack;
        }

        public int BaseHitPoints { get; }
        public int BaseAttack { get; }

        public bool Equals(CharacterLevelStats other) =>
            BaseHitPoints == other.BaseHitPoints && BaseAttack == other.BaseAttack;
        public override bool Equals(object obj) => obj is CharacterLevelStats other && Equals(other);
        public override int GetHashCode() => (BaseHitPoints * 397) ^ BaseAttack;
    }
}
