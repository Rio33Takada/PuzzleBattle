using System;

namespace PuzzleBattle.Domain.Gimmick
{
    public readonly struct GimmickId : IEquatable<GimmickId>
    {
        public GimmickId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("A gimmick ID is required.", nameof(value));
            Value = value;
        }

        public string Value { get; }
        public bool Equals(GimmickId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is GimmickId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value ?? string.Empty;
    }
}
