using System;

namespace PuzzleBattle.Domain.Randomness
{
    public readonly struct StageRandomSeed : IEquatable<StageRandomSeed>
    {
        public StageRandomSeed(ulong value)
        {
            Value = value;
        }

        public ulong Value { get; }

        public bool Equals(StageRandomSeed other) => Value == other.Value;
        public override bool Equals(object obj) => obj is StageRandomSeed other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(StageRandomSeed left, StageRandomSeed right) => left.Equals(right);
        public static bool operator !=(StageRandomSeed left, StageRandomSeed right) => !left.Equals(right);
        public override string ToString() => Value.ToString();
    }
}
