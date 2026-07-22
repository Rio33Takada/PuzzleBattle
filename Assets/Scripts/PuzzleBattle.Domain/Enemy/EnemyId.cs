using System;

namespace PuzzleBattle.Domain.Enemy
{
    public readonly struct EnemyId : IEquatable<EnemyId>
    {
        public EnemyId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("An enemy ID is required.", nameof(value));
            Value = value;
        }

        public string Value { get; }
        public bool Equals(EnemyId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is EnemyId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(EnemyId left, EnemyId right) => left.Equals(right);
        public static bool operator !=(EnemyId left, EnemyId right) => !left.Equals(right);
    }
}
