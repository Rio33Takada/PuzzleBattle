using System;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Enemy;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Domain.Skill
{
    public readonly struct SkillEffectResult
    {
        public SkillEffectResult(string effectType, int appliedAmount)
        {
            EffectType = effectType;
            AppliedAmount = appliedAmount;
        }

        public string EffectType { get; }
        public int AppliedAmount { get; }
    }

    public interface ISkillEffect
    {
        SkillEffectResult Apply(SkillDefinition skill, SkillExecutionContext context);
    }

    public sealed class IncreaseAttackEffect : ISkillEffect
    {
        public IncreaseAttackEffect(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            Amount = amount;
        }

        public int Amount { get; }

        public SkillEffectResult Apply(SkillDefinition skill, SkillExecutionContext context)
        {
            context.Modifiers.AddAttack(skill.OwnerId, Amount);
            return new SkillEffectResult(nameof(IncreaseAttackEffect), Amount);
        }
    }

    public sealed class HealPlayerEffect : ISkillEffect
    {
        public HealPlayerEffect(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            Amount = amount;
        }

        public int Amount { get; }

        public SkillEffectResult Apply(SkillDefinition skill, SkillExecutionContext context)
        {
            Combat.HealingResult result = context.Player.Health.Restore(Amount);
            return new SkillEffectResult(nameof(HealPlayerEffect), result.AppliedHealing);
        }
    }

    public sealed class DamageEnemyEffect : ISkillEffect
    {
        public DamageEnemyEffect(EnemyId targetEnemyId, int damage)
        {
            if (damage < 0)
                throw new ArgumentOutOfRangeException(nameof(damage));
            TargetEnemyId = targetEnemyId;
            Damage = damage;
        }

        public EnemyId TargetEnemyId { get; }
        public int Damage { get; }

        public SkillEffectResult Apply(SkillDefinition skill, SkillExecutionContext context)
        {
            if (!context.Enemies.TryGetValue(TargetEnemyId, out EnemyBattleState enemy))
                throw new InvalidOperationException("The target enemy does not exist.");
            Combat.HealthChangeResult result = enemy.ReceiveDamage(Damage);
            return new SkillEffectResult(nameof(DamageEnemyEffect), result.AppliedDamage);
        }
    }

    public sealed class GrantPieceEffect : ISkillEffect
    {
        public GrantPieceEffect(BattlePiece piece)
        {
            Piece = piece ?? throw new ArgumentNullException(nameof(piece));
        }

        public BattlePiece Piece { get; }

        public SkillEffectResult Apply(SkillDefinition skill, SkillExecutionContext context)
        {
            context.GrantedPieces.Add(Piece);
            return new SkillEffectResult(nameof(GrantPieceEffect), 1);
        }
    }
}
