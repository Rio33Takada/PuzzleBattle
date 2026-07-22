using System;
using PuzzleBattle.Domain.Randomness;

namespace PuzzleBattle.Application.Progression
{
    public readonly struct StageId : IEquatable<StageId>
    {
        public StageId(string value) { if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("A stage ID is required.", nameof(value)); Value = value; }
        public string Value { get; }
        public bool Equals(StageId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is StageId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
    }

    public sealed class StageDefinition
    {
        public StageDefinition(StageId id, StageRandomSeed randomSeed) { Id = id; RandomSeed = randomSeed; }
        public StageId Id { get; }
        public StageRandomSeed RandomSeed { get; }
    }

    public interface IStageRepository { StageDefinition Find(StageId id); }
    public interface IStageBattleStarter { void Start(StageDefinition stage, IRandomSource random); }

    public sealed class StageSelectionUseCase
    {
        private readonly IStageRepository stages;
        private readonly IStageBattleStarter battleStarter;
        public StageSelectionUseCase(IStageRepository stages, IStageBattleStarter battleStarter)
        { this.stages = stages ?? throw new ArgumentNullException(nameof(stages)); this.battleStarter = battleStarter ?? throw new ArgumentNullException(nameof(battleStarter)); }

        public StageDefinition SelectAndStart(StageId id)
        {
            StageDefinition stage = stages.Find(id);
            if (stage == null) throw new InvalidOperationException("The selected stage was not found.");
            battleStarter.Start(stage, new DeterministicRandomSource(stage.RandomSeed));
            return stage;
        }
    }
}
