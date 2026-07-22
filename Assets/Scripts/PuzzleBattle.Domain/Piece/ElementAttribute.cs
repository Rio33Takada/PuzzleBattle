using System;

namespace PuzzleBattle.Domain.Piece
{
    public readonly struct ElementAttribute : IEquatable<ElementAttribute>
    {
        public ElementAttribute(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("An element attribute is required.", nameof(value));
            Value = value;
        }

        public string Value { get; }

        public bool Equals(ElementAttribute other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is ElementAttribute other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(ElementAttribute left, ElementAttribute right) => left.Equals(right);
        public static bool operator !=(ElementAttribute left, ElementAttribute right) => !left.Equals(right);
    }
}
