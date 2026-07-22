using System;

namespace PuzzleBattle.Domain.Piece
{
    public readonly struct PieceId : IEquatable<PieceId>
    {
        public PieceId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("A piece ID is required.", nameof(value));
            Value = value;
        }

        public string Value { get; }

        public bool Equals(PieceId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is PieceId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(PieceId left, PieceId right) => left.Equals(right);
        public static bool operator !=(PieceId left, PieceId right) => !left.Equals(right);
    }
}
