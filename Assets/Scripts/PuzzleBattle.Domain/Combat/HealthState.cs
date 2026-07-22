using System;

namespace PuzzleBattle.Domain.Combat
{
    public sealed class HealthState
    {
        public HealthState(int maximumHitPoints)
        {
            if (maximumHitPoints <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumHitPoints), maximumHitPoints, "Maximum HP must be positive.");
            }

            MaximumHitPoints = maximumHitPoints;
            CurrentHitPoints = maximumHitPoints;
        }

        public int MaximumHitPoints { get; }
        public int CurrentHitPoints { get; private set; }
        public bool IsDead => CurrentHitPoints == 0;

        public HealthChangeResult ReceiveDamage(int damage)
        {
            if (damage < 0)
                throw new ArgumentOutOfRangeException(nameof(damage), damage, "Damage cannot be negative.");

            int previous = CurrentHitPoints;
            if (IsDead || damage == 0)
                return new HealthChangeResult(previous, previous, 0, false);

            int appliedDamage = Math.Min(previous, damage);
            CurrentHitPoints = previous - appliedDamage;
            return new HealthChangeResult(
                previous,
                CurrentHitPoints,
                appliedDamage,
                previous > 0 && CurrentHitPoints == 0);
        }

        public HealingResult Restore(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Healing cannot be negative.");

            int previous = CurrentHitPoints;
            if (IsDead || amount == 0)
                return new HealingResult(previous, previous, 0);

            int missing = MaximumHitPoints - previous;
            int applied = Math.Min(missing, amount);
            CurrentHitPoints = previous + applied;
            return new HealingResult(previous, CurrentHitPoints, applied);
        }
    }
}
