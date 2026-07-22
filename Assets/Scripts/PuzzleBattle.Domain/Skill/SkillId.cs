using System;

namespace PuzzleBattle.Domain.Skill
{
    public readonly struct SkillId : IEquatable<SkillId>
    {
        public SkillId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("A skill ID is required.", nameof(value));
            Value = value;
        }

        public string Value { get; }
        public bool Equals(SkillId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is SkillId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value ?? string.Empty;
    }
}
