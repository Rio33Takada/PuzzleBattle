using System;
using PuzzleBattle.Domain.Combat;

namespace PuzzleBattle.Domain.Enemy
{
    public sealed class EnemyBattleState
    {
        public EnemyBattleState(EnemyId id, EnemyStats stats, int attackInterval = 1)
        {
            if (string.IsNullOrWhiteSpace(id.Value))
                throw new ArgumentException("An enemy ID is required.", nameof(id));

            Id = id;
            Stats = stats;
            Health = new HealthState(stats.MaximumHitPoints);
            AttackCounter = new EnemyAttackCounter(attackInterval);
        }

        public EnemyId Id { get; }
        public EnemyStats Stats { get; }
        public HealthState Health { get; }
        public EnemyAttackCounter AttackCounter { get; }
        public int CurrentHitPoints => Health.CurrentHitPoints;
        public int Attack => Stats.Attack;
        public int Defense => Stats.Defense;
        public bool IsDead => Health.IsDead;

        public HealthChangeResult ReceiveDamage(int damage)
        {
            return Health.ReceiveDamage(damage);
        }
    }
}
