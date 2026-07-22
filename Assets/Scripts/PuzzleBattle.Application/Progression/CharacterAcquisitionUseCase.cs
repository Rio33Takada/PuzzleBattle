using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Randomness;

namespace PuzzleBattle.Application.Progression
{
    [Flags]
    public enum CharacterAcquisitionMethod { None = 0, Gacha = 1, StageReward = 2, Other = 4 }

    public sealed class CharacterAcquisitionProfile
    {
        public CharacterAcquisitionProfile(CharacterId characterId, CharacterAcquisitionMethod methods, int initialLevel = 1)
        {
            if (methods == CharacterAcquisitionMethod.None) throw new ArgumentException("An acquisition method is required.", nameof(methods));
            if (initialLevel <= 0) throw new ArgumentOutOfRangeException(nameof(initialLevel));
            CharacterId = characterId; Methods = methods; InitialLevel = initialLevel;
        }
        public CharacterId CharacterId { get; }
        public CharacterAcquisitionMethod Methods { get; }
        public int InitialLevel { get; }
        public bool Supports(CharacterAcquisitionMethod method) => (Methods & method) == method;
    }

    public sealed class GachaEntry
    {
        public GachaEntry(CharacterAcquisitionProfile character, int weight)
        {
            Character = character ?? throw new ArgumentNullException(nameof(character));
            if (!character.Supports(CharacterAcquisitionMethod.Gacha)) throw new ArgumentException("Character must support gacha acquisition.", nameof(character));
            if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight));
            Weight = weight;
        }
        public CharacterAcquisitionProfile Character { get; }
        public int Weight { get; }
    }

    public sealed class GachaTable
    {
        public GachaTable(IEnumerable<ResourceCost> costs, IEnumerable<GachaEntry> entries)
        {
            Costs = (costs ?? throw new ArgumentNullException(nameof(costs))).ToArray();
            Entries = (entries ?? throw new ArgumentNullException(nameof(entries))).ToArray();
            if (Costs.Count == 0 || Entries.Count == 0) throw new ArgumentException("Gacha requires costs and entries.");
            TotalWeight = Entries.Aggregate(0, (total, entry) => checked(total + entry.Weight));
        }
        public IReadOnlyList<ResourceCost> Costs { get; }
        public IReadOnlyList<GachaEntry> Entries { get; }
        public int TotalWeight { get; }
    }

    public enum GachaDrawStatus { Succeeded = 0, InsufficientResources = 1 }
    public readonly struct GachaDrawResult
    {
        internal GachaDrawResult(GachaDrawStatus status, CharacterId characterId, bool isNew)
        { Status = status; CharacterId = characterId; IsNew = isNew; }
        public GachaDrawStatus Status { get; }
        public CharacterId CharacterId { get; }
        public bool IsNew { get; }
    }

    public sealed class CharacterAcquisitionUseCase
    {
        private readonly IPlayerProgressRepository repository;
        public CharacterAcquisitionUseCase(IPlayerProgressRepository repository) =>
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));

        public GachaDrawResult Draw(GachaTable table, IRandomSource random)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (random == null) throw new ArgumentNullException(nameof(random));
            PlayerProgress current = repository.Load() ?? throw new InvalidOperationException("Player progress was not found.");
            if (!current.CanAfford(table.Costs))
                return new GachaDrawResult(GachaDrawStatus.InsufficientResources, default, false);

            int roll = random.NextInt(0, table.TotalWeight);
            int boundary = 0;
            GachaEntry selected = null;
            foreach (GachaEntry entry in table.Entries)
            {
                boundary = checked(boundary + entry.Weight);
                if (roll < boundary) { selected = entry; break; }
            }
            if (selected == null) throw new InvalidOperationException("Gacha table did not resolve a result.");

            PlayerProgress updated = current.Copy();
            updated.Spend(table.Costs);
            bool isNew = updated.Acquire(selected.Character.CharacterId, selected.Character.InitialLevel);
            repository.Save(updated);
            return new GachaDrawResult(GachaDrawStatus.Succeeded, selected.Character.CharacterId, isNew);
        }
    }
}
