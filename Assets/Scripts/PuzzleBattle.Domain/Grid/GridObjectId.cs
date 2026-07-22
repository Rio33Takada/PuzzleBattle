using System;

namespace PuzzleBattle.Domain.Grid
{
    public readonly struct GridObjectId : IEquatable<GridObjectId>
    {
        public GridObjectId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("A grid object ID is required.", nameof(value));

            Value = value;
        }

        public string Value { get; }

        public bool Equals(GridObjectId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is GridObjectId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        public static bool operator ==(GridObjectId left, GridObjectId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GridObjectId left, GridObjectId right)
        {
            return !left.Equals(right);
        }
    }
}
