using System;

namespace PuzzleBattle.Domain.Randomness
{
    /// <summary>
    /// PCG-XSH-RR 32 with a fixed stream. Its sequence is independent of Unity and runtime Random implementations.
    /// </summary>
    public sealed class DeterministicRandomSource : IRandomSource
    {
        private const ulong Multiplier = 6364136223846793005UL;
        private const ulong Increment = 1442695040888963407UL;
        private ulong state;

        public DeterministicRandomSource(StageRandomSeed seed)
        {
            // PCG-recommended two-step initialization keeps a zero seed valid.
            state = 0UL;
            AdvanceState();
            state = unchecked(state + seed.Value);
            AdvanceState();
            GeneratedValueCount = 0UL;
        }

        public ulong GeneratedValueCount { get; private set; }

        public uint NextUInt32()
        {
            uint value = AdvanceState();
            GeneratedValueCount = checked(GeneratedValueCount + 1UL);
            return value;
        }

        public int NextInt(int minimumInclusive, int maximumExclusive)
        {
            if (minimumInclusive >= maximumExclusive)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumExclusive),
                    maximumExclusive,
                    "Maximum must be greater than minimum.");
            }

            uint range = checked((uint)((long)maximumExclusive - minimumInclusive));
            uint rejectionThreshold = unchecked(0u - range) % range;
            uint value;
            do
            {
                value = NextUInt32();
            } while (value < rejectionThreshold);

            return checked((int)(minimumInclusive + (long)(value % range)));
        }

        public double NextUnitDouble()
        {
            // Combine 27 and 26 random bits to produce an exact 53-bit fraction in [0, 1).
            ulong upper = NextUInt32() >> 5;
            ulong lower = NextUInt32() >> 6;
            return (upper * 67108864UL + lower) / 9007199254740992d;
        }

        private uint AdvanceState()
        {
            ulong previous = state;
            state = unchecked(previous * Multiplier + Increment);
            uint xorShifted = (uint)(((previous >> 18) ^ previous) >> 27);
            int rotation = (int)(previous >> 59);
            return (xorShifted >> rotation) | (xorShifted << ((-rotation) & 31));
        }
    }
}
