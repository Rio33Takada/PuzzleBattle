using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Residual;

namespace PuzzleBattle.Domain.Stun
{
    public enum EnemyStunOutcome
    {
        NotStunned = 0,
        NewlyStunned = 1,
        StillStunned = 2,
        Recovered = 3,
        DeadEnemy = 4
    }

    public sealed class EnemyStunResult
    {
        internal EnemyStunResult(EnemyUnit enemy, EnemyStunOutcome outcome, int adjacentBlankCount)
        {
            Enemy = enemy;
            Outcome = outcome;
            AdjacentBlankCount = adjacentBlankCount;
        }

        public EnemyUnit Enemy { get; }
        public EnemyStunOutcome Outcome { get; }
        public int AdjacentBlankCount { get; }
    }

    public sealed class StunResolutionResult
    {
        internal StunResolutionResult(
            IEnumerable<EnemyStunResult> enemyResults,
            IEnumerable<ResidualContactResult> destroyedResiduals)
        {
            EnemyResults = enemyResults.ToArray();
            DestroyedResiduals = destroyedResiduals.ToArray();
            TotalPlayerDamage = DestroyedResiduals.Sum(result => result.PlayerDamage);
        }

        public IReadOnlyList<EnemyStunResult> EnemyResults { get; }
        public IReadOnlyCollection<ResidualContactResult> DestroyedResiduals { get; }
        public int TotalPlayerDamage { get; }
    }
}
