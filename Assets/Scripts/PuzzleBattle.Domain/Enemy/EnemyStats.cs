using System;

namespace PuzzleBattle.Domain.Enemy
{
    public readonly struct EnemyStats : IEquatable<EnemyStats>
    {
        public EnemyStats(int maximumHitPoints, int attack, int defense)
        {
            if (maximumHitPoints <= 0)
                throw new ArgumentOutOfRangeException(nameof(maximumHitPoints), maximumHitPoints, "Maximum HP must be positive.");
            if (attack < 0)
                throw new ArgumentOutOfRangeException(nameof(attack), attack, "Attack cannot be negative.");
            if (defense < 0)
                throw new ArgumentOutOfRangeException(nameof(defense), defense, "Defense cannot be negative.");

            MaximumHitPoints = maximumHitPoints;
            Attack = attack;
            Defense = defense;
        }

        public int MaximumHitPoints { get; }
        public int Attack { get; }
        public int Defense { get; }

        public bool Equals(EnemyStats other)
        {
            return MaximumHitPoints == other.MaximumHitPoints &&
                   Attack == other.Attack &&
                   Defense == other.Defense;
        }

        public override bool Equals(object obj) => obj is EnemyStats other && Equals(other);
        public override int GetHashCode() => ((MaximumHitPoints * 397) ^ Attack) * 397 ^ Defense;
    }
}
