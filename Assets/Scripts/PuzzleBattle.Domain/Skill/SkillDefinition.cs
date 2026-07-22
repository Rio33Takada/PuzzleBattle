using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Character;

namespace PuzzleBattle.Domain.Skill
{
    public enum SkillActivationKind
    {
        Active = 0,
        Passive = 1
    }

    public sealed class SkillDefinition
    {
        private readonly ISkillEffect[] effects;

        public SkillDefinition(
            SkillId id,
            CharacterId ownerId,
            SkillActivationKind activationKind,
            int priority,
            ISkillCondition condition,
            IEnumerable<ISkillEffect> effects)
        {
            if (string.IsNullOrWhiteSpace(id.Value))
                throw new ArgumentException("A skill ID is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(ownerId.Value))
                throw new ArgumentException("A skill owner is required.", nameof(ownerId));
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (effects == null)
                throw new ArgumentNullException(nameof(effects));

            this.effects = effects.ToArray();
            if (this.effects.Length == 0 || this.effects.Any(effect => effect == null))
                throw new ArgumentException("A skill must contain at least one valid effect.", nameof(effects));

            Id = id;
            OwnerId = ownerId;
            ActivationKind = activationKind;
            Priority = priority;
            Condition = condition;
        }

        public SkillId Id { get; }
        public CharacterId OwnerId { get; }
        public SkillActivationKind ActivationKind { get; }
        public int Priority { get; }
        public ISkillCondition Condition { get; }
        public IReadOnlyCollection<ISkillEffect> Effects => effects;
    }
}
