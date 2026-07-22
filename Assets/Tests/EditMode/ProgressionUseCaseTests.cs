using System;
using System.Collections.Generic;
using NUnit.Framework;
using PuzzleBattle.Application.Progression;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Randomness;

namespace PuzzleBattle.Application.Tests
{
    public sealed class ProgressionUseCaseTests
    {
        private static readonly ResourceId Gold = new ResourceId("gold");
        private static readonly CharacterId Hero = new CharacterId("hero");

        [Test]
        public void Grow_InsufficientResources_DoesNotSaveOrChangeLevel()
        {
            var repository = new MemoryProgressRepository(CreateProgress(4, 1));
            var plan = CreateGrowthPlan(5);

            CharacterGrowthResult result = new CharacterGrowthUseCase(repository).Grow(plan);

            Assert.That(result.Status, Is.EqualTo(CharacterGrowthStatus.InsufficientResources));
            Assert.That(repository.SaveCount, Is.Zero);
            Assert.That(repository.Current.GetResource(Gold), Is.EqualTo(4));
            Assert.That(repository.Current.GetLevel(Hero), Is.EqualTo(1));
        }

        [Test]
        public void Grow_WithResources_DeductsCostAndRaisesOneLevelAtomically()
        {
            var repository = new MemoryProgressRepository(CreateProgress(10, 1));

            CharacterGrowthResult result = new CharacterGrowthUseCase(repository).Grow(CreateGrowthPlan(6));

            Assert.That(result.Status, Is.EqualTo(CharacterGrowthStatus.Succeeded));
            Assert.That(result.CurrentLevel, Is.EqualTo(2));
            Assert.That(repository.Current.GetResource(Gold), Is.EqualTo(4));
            Assert.That(repository.Current.GetLevel(Hero), Is.EqualTo(2));
            Assert.That(repository.SaveCount, Is.EqualTo(1));
        }

        [TestCase(0, "a")]
        [TestCase(9, "a")]
        [TestCase(10, "b")]
        [TestCase(29, "b")]
        public void Gacha_UsesHalfOpenWeightBoundaries(int roll, string expectedCharacter)
        {
            var repository = new MemoryProgressRepository(CreateProgress(100));
            GachaTable table = CreateGachaTable();

            GachaDrawResult result = new CharacterAcquisitionUseCase(repository)
                .Draw(table, new FixedRandom(roll));

            Assert.That(result.Status, Is.EqualTo(GachaDrawStatus.Succeeded));
            Assert.That(result.CharacterId, Is.EqualTo(new CharacterId(expectedCharacter)));
            Assert.That(result.IsNew, Is.True);
            Assert.That(repository.Current.GetResource(Gold), Is.EqualTo(90));
        }

        [Test]
        public void Gacha_InsufficientResources_DoesNotConsumeRandomOrSave()
        {
            var repository = new MemoryProgressRepository(CreateProgress(9));
            var random = new FixedRandom(0);

            GachaDrawResult result = new CharacterAcquisitionUseCase(repository)
                .Draw(CreateGachaTable(), random);

            Assert.That(result.Status, Is.EqualTo(GachaDrawStatus.InsufficientResources));
            Assert.That(random.GeneratedValueCount, Is.Zero);
            Assert.That(repository.SaveCount, Is.Zero);
        }

        [Test]
        public void Gacha_DuplicateCharacterStillConsumesCostAndReportsNotNew()
        {
            var repository = new MemoryProgressRepository(new PlayerProgress(
                new[] { new KeyValuePair<ResourceId, int>(Gold, 20) },
                new[] { new KeyValuePair<CharacterId, int>(new CharacterId("a"), 3) }));

            GachaDrawResult result = new CharacterAcquisitionUseCase(repository)
                .Draw(CreateGachaTable(), new FixedRandom(0));

            Assert.That(result.IsNew, Is.False);
            Assert.That(repository.Current.GetLevel(new CharacterId("a")), Is.EqualTo(3));
            Assert.That(repository.Current.GetResource(Gold), Is.EqualTo(10));
        }

        [Test]
        public void StageSelection_StartsSelectedStageWithItsDeterministicSeed()
        {
            var stage = new StageDefinition(new StageId("stage-1"), new StageRandomSeed(123UL));
            var stages = new FixedStageRepository(stage);
            var starter = new RecordingBattleStarter();

            StageDefinition selected = new StageSelectionUseCase(stages, starter)
                .SelectAndStart(stage.Id);

            var replay = new DeterministicRandomSource(stage.RandomSeed);
            Assert.That(selected, Is.SameAs(stage));
            Assert.That(starter.Stage, Is.SameAs(stage));
            Assert.That(starter.FirstRandomValue, Is.EqualTo(replay.NextUInt32()));
        }

        private static PlayerProgress CreateProgress(int gold, int heroLevel = 0)
        {
            var characters = heroLevel > 0
                ? new[] { new KeyValuePair<CharacterId, int>(Hero, heroLevel) }
                : Array.Empty<KeyValuePair<CharacterId, int>>();
            return new PlayerProgress(
                new[] { new KeyValuePair<ResourceId, int>(Gold, gold) }, characters);
        }

        private static CharacterGrowthPlan CreateGrowthPlan(int cost)
        {
            return new CharacterGrowthPlan(Hero, new[]
            {
                new CharacterGrowthStep(1, new[] { new ResourceCost(Gold, cost) })
            });
        }

        private static GachaTable CreateGachaTable()
        {
            return new GachaTable(
                new[] { new ResourceCost(Gold, 10) },
                new[]
                {
                    new GachaEntry(new CharacterAcquisitionProfile(
                        new CharacterId("a"), CharacterAcquisitionMethod.Gacha), 10),
                    new GachaEntry(new CharacterAcquisitionProfile(
                        new CharacterId("b"), CharacterAcquisitionMethod.Gacha), 20)
                });
        }

        private sealed class MemoryProgressRepository : IPlayerProgressRepository
        {
            public MemoryProgressRepository(PlayerProgress progress) { Current = progress.Copy(); }
            public PlayerProgress Current { get; private set; }
            public int SaveCount { get; private set; }
            public PlayerProgress Load() => Current.Copy();
            public void Save(PlayerProgress progress) { Current = progress.Copy(); SaveCount++; }
        }

        private sealed class FixedRandom : IRandomSource
        {
            private readonly int value;
            public FixedRandom(int value) { this.value = value; }
            public ulong GeneratedValueCount { get; private set; }
            public uint NextUInt32() { GeneratedValueCount++; return (uint)value; }
            public int NextInt(int minimumInclusive, int maximumExclusive)
            { GeneratedValueCount++; return value; }
            public double NextUnitDouble() { GeneratedValueCount += 2; return 0; }
        }

        private sealed class FixedStageRepository : IStageRepository
        {
            private readonly StageDefinition stage;
            public FixedStageRepository(StageDefinition stage) { this.stage = stage; }
            public StageDefinition Find(StageId id) => id.Equals(stage.Id) ? stage : null;
        }

        private sealed class RecordingBattleStarter : IStageBattleStarter
        {
            public StageDefinition Stage { get; private set; }
            public uint FirstRandomValue { get; private set; }
            public void Start(StageDefinition stage, IRandomSource random)
            { Stage = stage; FirstRandomValue = random.NextUInt32(); }
        }
    }
}
