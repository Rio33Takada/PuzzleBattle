namespace PuzzleBattle.Domain.Combat
{
    public readonly struct HealthChangeResult
    {
        internal HealthChangeResult(
            int previousHitPoints,
            int currentHitPoints,
            int appliedDamage,
            bool becameDead)
        {
            PreviousHitPoints = previousHitPoints;
            CurrentHitPoints = currentHitPoints;
            AppliedDamage = appliedDamage;
            BecameDead = becameDead;
        }

        public int PreviousHitPoints { get; }
        public int CurrentHitPoints { get; }
        public int AppliedDamage { get; }
        public bool BecameDead { get; }
        public bool IsDead => CurrentHitPoints == 0;
    }

    public readonly struct HealingResult
    {
        internal HealingResult(int previousHitPoints, int currentHitPoints, int appliedHealing)
        {
            PreviousHitPoints = previousHitPoints;
            CurrentHitPoints = currentHitPoints;
            AppliedHealing = appliedHealing;
        }

        public int PreviousHitPoints { get; }
        public int CurrentHitPoints { get; }
        public int AppliedHealing { get; }
    }
}
