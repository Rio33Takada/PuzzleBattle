using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Residual;

namespace PuzzleBattle.Domain.Stun
{
    public sealed class StunResolutionService
    {
        public StunResolutionResult Resolve(
            IEnumerable<EnemyUnit> enemies,
            GridShape fieldRange,
            ResidualPieceField residualPieces,
            StunSettings settings)
        {
            if (enemies == null)
                throw new ArgumentNullException(nameof(enemies));
            if (fieldRange == null)
                throw new ArgumentNullException(nameof(fieldRange));
            if (residualPieces == null)
                throw new ArgumentNullException(nameof(residualPieces));
            if (!fieldRange.Equals(residualPieces.Range))
                throw new ArgumentException("Enemy and residual piece fields must use the same range.", nameof(residualPieces));

            EnemyUnit[] enemyArray = enemies.ToArray();
            if (enemyArray.Any(enemy => enemy == null))
                throw new ArgumentException("Enemy collection cannot contain null.", nameof(enemies));
            if (enemyArray.Select(enemy => enemy.Id).Distinct().Count() != enemyArray.Length)
                throw new ArgumentException("Enemy IDs must be unique.", nameof(enemies));

            EnemyUnit[] ordered = enemyArray
                .OrderBy(enemy => enemy.Id.Value, StringComparer.Ordinal)
                .ToArray();
            var enemyPositions = new HashSet<GridPosition>(ordered.Select(enemy => enemy.Position));
            var reservedResiduals = new HashSet<ResidualPiece>();
            var results = new List<EnemyStunResult>(ordered.Length);

            foreach (EnemyUnit enemy in ordered)
            {
                if (enemy.BattleState.IsDead)
                {
                    results.Add(new EnemyStunResult(enemy, EnemyStunOutcome.DeadEnemy, 0));
                    continue;
                }

                if (enemy.Stun.IsStunned)
                {
                    bool remainsStunned = enemy.Stun.AdvanceCounter();
                    results.Add(new EnemyStunResult(
                        enemy,
                        remainsStunned ? EnemyStunOutcome.StillStunned : EnemyStunOutcome.Recovered,
                        0));
                    continue;
                }

                GridPosition[] validNeighbors = enemy.Position
                    .GetOrthogonalNeighbors()
                    .Where(fieldRange.IsValid)
                    .ToArray();
                int blankCount = validNeighbors.Count(position =>
                    !enemyPositions.Contains(position) && residualPieces.FindAt(position) == null);
                if (blankCount > 0)
                {
                    results.Add(new EnemyStunResult(enemy, EnemyStunOutcome.NotStunned, blankCount));
                    continue;
                }

                enemy.Stun.Apply(settings);
                foreach (GridPosition neighbor in validNeighbors)
                {
                    ResidualPiece residual = residualPieces.FindAt(neighbor);
                    if (residual != null)
                        reservedResiduals.Add(residual);
                }
                results.Add(new EnemyStunResult(enemy, EnemyStunOutcome.NewlyStunned, 0));
            }

            // Apply destruction only after every enemy has observed the same pre-destruction field state.
            ResidualContactResult[] destroyed = reservedResiduals
                .OrderBy(residual => residual.Id.Value, StringComparer.Ordinal)
                .Select(residualPieces.DestroyImmediately)
                .Where(result => result.Destroyed)
                .ToArray();

            return new StunResolutionResult(results, destroyed);
        }
    }
}
