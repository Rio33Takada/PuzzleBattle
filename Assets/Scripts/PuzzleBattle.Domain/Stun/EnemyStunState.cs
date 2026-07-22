using System;

namespace PuzzleBattle.Domain.Stun
{
    public sealed class EnemyStunState
    {
        public int RemainingTurns { get; private set; }
        public bool IsStunned => RemainingTurns > 0;
        public decimal DamageTakenMultiplier { get; private set; } = 1m;

        internal void Apply(StunSettings settings)
        {
            if (IsStunned)
                throw new InvalidOperationException("An already stunned enemy cannot be stunned again.");

            RemainingTurns = settings.DurationTurns;
            DamageTakenMultiplier = settings.DamageTakenMultiplier;
        }

        internal bool AdvanceCounter()
        {
            if (!IsStunned)
                return false;

            RemainingTurns--;
            if (RemainingTurns == 0)
                DamageTakenMultiplier = 1m;
            return IsStunned;
        }
    }
}
