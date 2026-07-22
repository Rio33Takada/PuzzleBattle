using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Piece;

namespace PuzzleBattle.Domain.Skill
{
    public interface ISkillCondition
    {
        bool IsSatisfied(SkillDefinition skill, SkillExecutionContext context);
    }

    public sealed class AlwaysSkillCondition : ISkillCondition
    {
        public bool IsSatisfied(SkillDefinition skill, SkillExecutionContext context) => true;
    }

    public sealed class MinimumTurnCondition : ISkillCondition
    {
        public MinimumTurnCondition(int minimumTurn)
        {
            if (minimumTurn <= 0)
                throw new ArgumentOutOfRangeException(nameof(minimumTurn), minimumTurn, "Minimum turn must be positive.");
            MinimumTurn = minimumTurn;
        }

        public int MinimumTurn { get; }

        public bool IsSatisfied(SkillDefinition skill, SkillExecutionContext context)
        {
            return context.TurnNumber >= MinimumTurn;
        }
    }

    public sealed class OverwriteAttributeCountCondition : ISkillCondition
    {
        public OverwriteAttributeCountCondition(ElementAttribute attribute, int requiredCount)
        {
            if (string.IsNullOrWhiteSpace(attribute.Value))
                throw new ArgumentException("An overwrite attribute is required.", nameof(attribute));
            if (requiredCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(requiredCount), requiredCount, "Required count must be positive.");
            Attribute = attribute;
            RequiredCount = requiredCount;
        }

        public ElementAttribute Attribute { get; }
        public int RequiredCount { get; }

        public bool IsSatisfied(SkillDefinition skill, SkillExecutionContext context)
        {
            return context.Overwrites.GetCount(skill.OwnerId, Attribute) >= RequiredCount;
        }
    }

    public sealed class AllSkillConditions : ISkillCondition
    {
        private readonly ISkillCondition[] conditions;

        public AllSkillConditions(IEnumerable<ISkillCondition> conditions)
        {
            if (conditions == null)
                throw new ArgumentNullException(nameof(conditions));
            this.conditions = conditions.ToArray();
            if (this.conditions.Length == 0 || this.conditions.Any(condition => condition == null))
                throw new ArgumentException("At least one valid condition is required.", nameof(conditions));
        }

        public bool IsSatisfied(SkillDefinition skill, SkillExecutionContext context)
        {
            return conditions.All(condition => condition.IsSatisfied(skill, context));
        }
    }
}
