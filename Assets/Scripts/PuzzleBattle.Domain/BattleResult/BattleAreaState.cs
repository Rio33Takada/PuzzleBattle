using System;

namespace PuzzleBattle.Domain.BattleResult
{
    public readonly struct BattleAreaState : IEquatable<BattleAreaState>
    {
        public BattleAreaState(int areaNumber, int totalAreaCount)
        {
            if (totalAreaCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(totalAreaCount), totalAreaCount, "Total area count must be positive.");
            if (areaNumber <= 0 || areaNumber > totalAreaCount)
                throw new ArgumentOutOfRangeException(nameof(areaNumber), areaNumber, "Area number must be within the battle range.");

            AreaNumber = areaNumber;
            TotalAreaCount = totalAreaCount;
        }

        public int AreaNumber { get; }
        public int TotalAreaCount { get; }
        public bool IsFinalArea => AreaNumber == TotalAreaCount;

        public BattleAreaState Advance()
        {
            if (IsFinalArea)
                throw new InvalidOperationException("The final area cannot advance.");
            return new BattleAreaState(AreaNumber + 1, TotalAreaCount);
        }

        public bool Equals(BattleAreaState other) =>
            AreaNumber == other.AreaNumber && TotalAreaCount == other.TotalAreaCount;

        public override bool Equals(object obj) => obj is BattleAreaState other && Equals(other);
        public override int GetHashCode() => (AreaNumber * 397) ^ TotalAreaCount;
        public static bool operator ==(BattleAreaState left, BattleAreaState right) => left.Equals(right);
        public static bool operator !=(BattleAreaState left, BattleAreaState right) => !left.Equals(right);
    }
}
