using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Character;

namespace PuzzleBattle.Application.Progression
{
    public readonly struct ResourceId : IEquatable<ResourceId>
    {
        public ResourceId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("A resource ID is required.", nameof(value));
            Value = value;
        }
        public string Value { get; }
        public bool Equals(ResourceId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is ResourceId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
    }

    public sealed class ResourceCost
    {
        public ResourceCost(ResourceId resourceId, int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Cost must be positive.");
            ResourceId = resourceId;
            Amount = amount;
        }
        public ResourceId ResourceId { get; }
        public int Amount { get; }
    }

    public sealed class PlayerProgress
    {
        private readonly Dictionary<ResourceId, int> resources;
        private readonly Dictionary<CharacterId, int> characterLevels;

        public PlayerProgress(
            IEnumerable<KeyValuePair<ResourceId, int>> resources = null,
            IEnumerable<KeyValuePair<CharacterId, int>> characterLevels = null)
        {
            this.resources = resources == null
                ? new Dictionary<ResourceId, int>()
                : resources.ToDictionary(entry => entry.Key, entry => RequireNonNegative(entry.Value));
            this.characterLevels = characterLevels == null
                ? new Dictionary<CharacterId, int>()
                : characterLevels.ToDictionary(entry => entry.Key, entry => RequirePositive(entry.Value));
        }

        public IReadOnlyDictionary<ResourceId, int> Resources => resources;
        public IReadOnlyDictionary<CharacterId, int> CharacterLevels => characterLevels;
        public int GetResource(ResourceId id) => resources.TryGetValue(id, out int amount) ? amount : 0;
        public bool Owns(CharacterId id) => characterLevels.ContainsKey(id);
        public int GetLevel(CharacterId id) => characterLevels.TryGetValue(id, out int level) ? level : 0;

        public PlayerProgress Copy() => new PlayerProgress(resources, characterLevels);

        internal bool CanAfford(IEnumerable<ResourceCost> costs) =>
            AggregateCosts(costs).All(cost => GetResource(cost.Key) >= cost.Value);

        internal void Spend(IEnumerable<ResourceCost> costs)
        {
            KeyValuePair<ResourceId, int>[] values = AggregateCosts(costs).ToArray();
            if (values.Any(cost => GetResource(cost.Key) < cost.Value))
                throw new InvalidOperationException("Resources are insufficient.");
            foreach (KeyValuePair<ResourceId, int> cost in values)
                resources[cost.Key] = checked(GetResource(cost.Key) - cost.Value);
        }

        internal void SetLevel(CharacterId id, int level)
        {
            characterLevels[id] = RequirePositive(level);
        }

        internal bool Acquire(CharacterId id, int initialLevel)
        {
            if (Owns(id))
                return false;
            characterLevels.Add(id, RequirePositive(initialLevel));
            return true;
        }

        private static int RequireNonNegative(int value) => value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "Resource amount cannot be negative.");
        private static int RequirePositive(int value) => value > 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "Character level must be positive.");

        private static IEnumerable<KeyValuePair<ResourceId, int>> AggregateCosts(IEnumerable<ResourceCost> costs)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));
            return costs.GroupBy(cost => cost.ResourceId)
                .Select(group => new KeyValuePair<ResourceId, int>(
                    group.Key,
                    group.Aggregate(0, (total, cost) => checked(total + cost.Amount))));
        }
    }

    public interface IPlayerProgressRepository
    {
        PlayerProgress Load();
        void Save(PlayerProgress progress);
    }
}
