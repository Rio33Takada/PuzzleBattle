using System;

namespace PuzzleBattle.Domain.Stun
{
    public readonly struct StunSettings
    {
        public StunSettings(int durationTurns, decimal damageTakenMultiplier)
        {
            if (durationTurns <= 0)
                throw new ArgumentOutOfRangeException(nameof(durationTurns), durationTurns, "Stun duration must be positive.");
            if (damageTakenMultiplier <= 0m)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(damageTakenMultiplier), damageTakenMultiplier, "Damage multiplier must be positive.");
            }

            DurationTurns = durationTurns;
            DamageTakenMultiplier = damageTakenMultiplier;
        }

        public int DurationTurns { get; }
        public decimal DamageTakenMultiplier { get; }
    }
}
