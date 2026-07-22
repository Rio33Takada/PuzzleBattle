namespace PuzzleBattle.Domain.Randomness
{
    public interface IRandomSource
    {
        ulong GeneratedValueCount { get; }
        uint NextUInt32();
        int NextInt(int minimumInclusive, int maximumExclusive);
        double NextUnitDouble();
    }
}
