using System.Linq;
using NUnit.Framework;
using PuzzleBattle.Domain.Randomness;

namespace PuzzleBattle.Domain.Tests
{
    public sealed class DeterministicRandomSourceTests
    {
        [Test]
        public void SameStageSeed_ProducesSameSequence()
        {
            var first = new DeterministicRandomSource(new StageRandomSeed(12345UL));
            var second = new DeterministicRandomSource(new StageRandomSeed(12345UL));

            uint[] firstValues = Enumerable.Range(0, 20).Select(_ => first.NextUInt32()).ToArray();
            uint[] secondValues = Enumerable.Range(0, 20).Select(_ => second.NextUInt32()).ToArray();

            Assert.That(secondValues, Is.EqualTo(firstValues));
        }

        [Test]
        public void FixedAlgorithm_ProducesStableKnownVector()
        {
            var random = new DeterministicRandomSource(new StageRandomSeed(12345UL));

            uint[] values = Enumerable.Range(0, 5).Select(_ => random.NextUInt32()).ToArray();

            Assert.That(values, Is.EqualTo(new uint[]
            {
                1411482639u,
                3165192603u,
                3360792183u,
                2433038347u,
                628889468u
            }));
        }

        [Test]
        public void DifferentStageSeeds_ProduceDifferentSequences()
        {
            var first = new DeterministicRandomSource(new StageRandomSeed(1UL));
            var second = new DeterministicRandomSource(new StageRandomSeed(2UL));

            uint[] firstValues = Enumerable.Range(0, 8).Select(_ => first.NextUInt32()).ToArray();
            uint[] secondValues = Enumerable.Range(0, 8).Select(_ => second.NextUInt32()).ToArray();

            Assert.That(secondValues, Is.Not.EqualTo(firstValues));
        }

        [Test]
        public void CallsAdvanceSequenceAndExposeConsumedValueCount()
        {
            var advanced = new DeterministicRandomSource(new StageRandomSeed(99UL));
            var replay = new DeterministicRandomSource(new StageRandomSeed(99UL));

            advanced.NextUInt32();
            advanced.NextUInt32();
            uint third = advanced.NextUInt32();
            uint replayThird = Enumerable.Range(0, 3).Select(_ => replay.NextUInt32()).Last();

            Assert.That(third, Is.EqualTo(replayThird));
            Assert.That(advanced.GeneratedValueCount, Is.EqualTo(3UL));
        }

        [Test]
        public void NextInt_StaysInsideRequestedRangeIncludingExtremeBounds()
        {
            var random = new DeterministicRandomSource(new StageRandomSeed(777UL));

            int[] values = Enumerable.Range(0, 1000)
                .Select(_ => random.NextInt(-10, 15))
                .ToArray();
            int extreme = random.NextInt(int.MinValue, int.MaxValue);

            Assert.That(values.All(value => value >= -10 && value < 15), Is.True);
            Assert.That(extreme, Is.GreaterThanOrEqualTo(int.MinValue));
            Assert.That(extreme, Is.LessThan(int.MaxValue));
        }

        [Test]
        public void NextUnitDouble_IsReproducibleAndLessThanOne()
        {
            var first = new DeterministicRandomSource(new StageRandomSeed(42UL));
            var second = new DeterministicRandomSource(new StageRandomSeed(42UL));

            double[] values = Enumerable.Range(0, 100).Select(_ => first.NextUnitDouble()).ToArray();
            double[] replay = Enumerable.Range(0, 100).Select(_ => second.NextUnitDouble()).ToArray();

            Assert.That(replay, Is.EqualTo(values));
            Assert.That(values.All(value => value >= 0d && value < 1d), Is.True);
            Assert.That(first.GeneratedValueCount, Is.EqualTo(200UL));
        }

        [Test]
        public void NextInt_RejectsEmptyOrReversedRange()
        {
            var random = new DeterministicRandomSource(new StageRandomSeed(1UL));

            Assert.Throws<System.ArgumentOutOfRangeException>(() => random.NextInt(5, 5));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => random.NextInt(6, 5));
            Assert.That(random.GeneratedValueCount, Is.Zero);
        }
    }
}
