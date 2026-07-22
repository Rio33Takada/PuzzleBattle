using System;

namespace PuzzleBattle.Domain.Character
{
    public readonly struct CharacterId : IEquatable<CharacterId>
    {
        public CharacterId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("A character ID is required.", nameof(value));
            Value = value;
        }

        public string Value { get; }

        public bool Equals(CharacterId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is CharacterId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(CharacterId left, CharacterId right) => left.Equals(right);
        public static bool operator !=(CharacterId left, CharacterId right) => !left.Equals(right);
    }
}
