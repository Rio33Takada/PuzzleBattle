using System;

namespace PuzzleBattle.Domain.Residual
{
    public readonly struct ResidualPieceId : IEquatable<ResidualPieceId>
    {
        public ResidualPieceId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("A residual piece ID is required.", nameof(value));
            Value = value;
        }

        public string Value { get; }
        public bool Equals(ResidualPieceId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is ResidualPieceId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value ?? string.Empty;
    }
}
