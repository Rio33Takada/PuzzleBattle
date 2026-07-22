using System;
using PuzzleBattle.Domain.Enemy;

namespace PuzzleBattle.Domain.Stun
{
    public readonly struct StunnedAttackCounterResult
    {
        internal StunnedAttackCounterResult(
            int previousCounter,
            int currentCounter,
            bool attackSuppressed)
        {
            PreviousCounter = previousCounter;
            CurrentCounter = currentCounter;
            AttackSuppressed = attackSuppressed;
        }

        public int PreviousCounter { get; }
        public int CurrentCounter { get; }
        public bool AttackSuppressed { get; }
    }

    public sealed class StunnedEnemyActionService
    {
        public StunnedAttackCounterResult AdvanceAttackCounter(EnemyUnit enemy)
        {
            if (enemy == null)
                throw new ArgumentNullException(nameof(enemy));
            if (!enemy.Stun.IsStunned)
                throw new InvalidOperationException("Only a stunned enemy can use the stunned action flow.");

            int previous = enemy.BattleState.AttackCounter.Remaining;
            bool reachedZero = enemy.BattleState.AttackCounter.AdvanceAndResetWhenReady();
            return new StunnedAttackCounterResult(
                previous,
                enemy.BattleState.AttackCounter.Remaining,
                reachedZero);
        }
    }
}
