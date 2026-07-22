using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Character;

namespace PuzzleBattle.Application.Progression
{
    public sealed class CharacterGrowthStep
    {
        public CharacterGrowthStep(int currentLevel, IEnumerable<ResourceCost> costs)
        {
            if (currentLevel <= 0)
                throw new ArgumentOutOfRangeException(nameof(currentLevel));
            CurrentLevel = currentLevel;
            Costs = (costs ?? throw new ArgumentNullException(nameof(costs))).ToArray();
            if (Costs.Count == 0)
                throw new ArgumentException("Growth requires at least one cost.", nameof(costs));
        }
        public int CurrentLevel { get; }
        public IReadOnlyList<ResourceCost> Costs { get; }
    }

    public sealed class CharacterGrowthPlan
    {
        private readonly Dictionary<int, CharacterGrowthStep> steps;
        public CharacterGrowthPlan(CharacterId characterId, IEnumerable<CharacterGrowthStep> steps)
        {
            CharacterId = characterId;
            this.steps = (steps ?? throw new ArgumentNullException(nameof(steps)))
                .ToDictionary(step => step.CurrentLevel);
        }
        public CharacterId CharacterId { get; }
        public bool TryGetStep(int currentLevel, out CharacterGrowthStep step) => steps.TryGetValue(currentLevel, out step);
    }

    public enum CharacterGrowthStatus { Succeeded = 0, NotOwned = 1, MaximumLevel = 2, InsufficientResources = 3 }

    public readonly struct CharacterGrowthResult
    {
        internal CharacterGrowthResult(CharacterGrowthStatus status, int previousLevel, int currentLevel)
        { Status = status; PreviousLevel = previousLevel; CurrentLevel = currentLevel; }
        public CharacterGrowthStatus Status { get; }
        public int PreviousLevel { get; }
        public int CurrentLevel { get; }
    }

    public sealed class CharacterGrowthUseCase
    {
        private readonly IPlayerProgressRepository repository;
        public CharacterGrowthUseCase(IPlayerProgressRepository repository) =>
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));

        public CharacterGrowthResult Grow(CharacterGrowthPlan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            PlayerProgress current = repository.Load() ?? throw new InvalidOperationException("Player progress was not found.");
            int level = current.GetLevel(plan.CharacterId);
            if (level == 0) return new CharacterGrowthResult(CharacterGrowthStatus.NotOwned, 0, 0);
            if (!plan.TryGetStep(level, out CharacterGrowthStep step))
                return new CharacterGrowthResult(CharacterGrowthStatus.MaximumLevel, level, level);
            if (!current.CanAfford(step.Costs))
                return new CharacterGrowthResult(CharacterGrowthStatus.InsufficientResources, level, level);

            PlayerProgress updated = current.Copy();
            updated.Spend(step.Costs);
            updated.SetLevel(plan.CharacterId, checked(level + 1));
            repository.Save(updated);
            return new CharacterGrowthResult(CharacterGrowthStatus.Succeeded, level, level + 1);
        }
    }
}
