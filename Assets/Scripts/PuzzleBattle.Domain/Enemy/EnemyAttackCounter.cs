using System;

namespace PuzzleBattle.Domain.Enemy
{
    public sealed class EnemyAttackCounter
    {
        public EnemyAttackCounter(int interval)
        {
            if (interval <= 0)
                throw new ArgumentOutOfRangeException(nameof(interval), interval, "Attack interval must be positive.");
            Interval = interval;
            Remaining = interval;
        }

        public int Interval { get; }
        public int Remaining { get; private set; }

        internal bool AdvanceAndResetWhenReady()
        {
            Remaining--;
            if (Remaining > 0)
                return false;

            Remaining = Interval;
            return true;
        }
    }
}
