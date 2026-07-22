using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleBattle.Domain.Skill
{
    public enum SkillExecutionStatus
    {
        Applied = 0,
        ConditionNotMet = 1,
        WrongActivationKind = 2
    }

    public sealed class SkillExecutionResult
    {
        internal SkillExecutionResult(
            SkillDefinition skill,
            SkillExecutionStatus status,
            IEnumerable<SkillEffectResult> effects)
        {
            Skill = skill;
            Status = status;
            Effects = effects.ToArray();
        }

        public SkillDefinition Skill { get; }
        public SkillExecutionStatus Status { get; }
        public IReadOnlyCollection<SkillEffectResult> Effects { get; }
    }

    public sealed class SkillResolver
    {
        public SkillExecutionResult ExecuteActive(
            SkillDefinition skill,
            SkillExecutionContext context)
        {
            if (skill == null)
                throw new ArgumentNullException(nameof(skill));
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (skill.ActivationKind != SkillActivationKind.Active)
            {
                return new SkillExecutionResult(
                    skill, SkillExecutionStatus.WrongActivationKind, Array.Empty<SkillEffectResult>());
            }

            return ExecuteWhenSatisfied(skill, context);
        }

        public IReadOnlyList<SkillExecutionResult> ApplyPassivesAfterPlacement(
            IEnumerable<SkillDefinition> skills,
            SkillExecutionContext context)
        {
            if (skills == null)
                throw new ArgumentNullException(nameof(skills));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return skills
                .Where(skill => skill != null && skill.ActivationKind == SkillActivationKind.Passive)
                .OrderBy(skill => skill.Priority)
                .ThenBy(skill => skill.Id.Value, StringComparer.Ordinal)
                .Select(skill => ExecuteWhenSatisfied(skill, context))
                .ToArray();
        }

        private static SkillExecutionResult ExecuteWhenSatisfied(
            SkillDefinition skill,
            SkillExecutionContext context)
        {
            if (!skill.Condition.IsSatisfied(skill, context))
            {
                return new SkillExecutionResult(
                    skill, SkillExecutionStatus.ConditionNotMet, Array.Empty<SkillEffectResult>());
            }

            SkillEffectResult[] effects = skill.Effects
                .Select(effect => effect.Apply(skill, context))
                .ToArray();
            return new SkillExecutionResult(skill, SkillExecutionStatus.Applied, effects);
        }
    }
}
