using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Residual;

namespace PuzzleBattle.Domain.Enemy
{
    public sealed class EnemyMovementService
    {
        public EnemyMovementForecastBatch Forecast(
            IEnumerable<EnemyUnit> enemies,
            GridShape fieldRange,
            ResidualPieceField residualPieces,
            IEnemyMovementPriority priority)
        {
            EnemyUnit[] orderedEnemies = ValidateAndOrder(enemies, fieldRange, residualPieces, priority);
            var currentCells = new HashSet<GridPosition>(orderedEnemies.Select(enemy => enemy.Position));
            var reservedDestinations = new HashSet<GridPosition>();
            var forecasts = new List<EnemyMovementForecast>(orderedEnemies.Length);

            foreach (EnemyUnit enemy in orderedEnemies)
            {
                if (enemy.BattleState.IsDead)
                {
                    forecasts.Add(new EnemyMovementForecast(
                        enemy, enemy.Position, null, EnemyMovementForecastOutcome.DeadEnemy, false));
                    continue;
                }
                if (enemy.Stun.IsStunned)
                {
                    forecasts.Add(new EnemyMovementForecast(
                        enemy, enemy.Position, null, EnemyMovementForecastOutcome.Stunned, false));
                    continue;
                }

                GridPosition? destination = SelectDestination(
                    enemy, fieldRange, priority, currentCells, reservedDestinations);
                if (!destination.HasValue)
                {
                    forecasts.Add(new EnemyMovementForecast(
                        enemy, enemy.Position, null, EnemyMovementForecastOutcome.NoDestination, false));
                    continue;
                }

                reservedDestinations.Add(destination.Value);
                forecasts.Add(new EnemyMovementForecast(
                    enemy,
                    enemy.Position,
                    destination,
                    EnemyMovementForecastOutcome.Planned,
                    residualPieces.FindAt(destination.Value) != null));
            }

            return new EnemyMovementForecastBatch(forecasts);
        }

        public EnemyMovementBatchResult Resolve(
            IEnumerable<EnemyUnit> enemies,
            GridShape fieldRange,
            ResidualPieceField residualPieces,
            IEnemyMovementPriority priority)
        {
            if (enemies == null)
                throw new ArgumentNullException(nameof(enemies));
            if (fieldRange == null)
                throw new ArgumentNullException(nameof(fieldRange));
            if (residualPieces == null)
                throw new ArgumentNullException(nameof(residualPieces));
            if (priority == null)
                throw new ArgumentNullException(nameof(priority));
            if (!fieldRange.Equals(residualPieces.Range))
                throw new ArgumentException("Enemy and residual piece fields must use the same range.", nameof(residualPieces));

            EnemyUnit[] enemyArray = enemies.ToArray();
            if (enemyArray.Any(enemy => enemy == null))
                throw new ArgumentException("Enemy collection cannot contain null.", nameof(enemies));
            EnemyUnit[] orderedEnemies = enemyArray
                .OrderBy(enemy => enemy.Id.Value, StringComparer.Ordinal)
                .ToArray();
            if (orderedEnemies.Select(enemy => enemy.Id).Distinct().Count() != orderedEnemies.Length)
                throw new ArgumentException("Enemy IDs must be unique.", nameof(enemies));
            if (orderedEnemies.Any(enemy => !fieldRange.IsValid(enemy.Position)))
                throw new ArgumentException("Every enemy must be inside the field range.", nameof(enemies));
            if (orderedEnemies.Select(enemy => enemy.Position).Distinct().Count() != orderedEnemies.Length)
                throw new ArgumentException("Enemies cannot share a current cell.", nameof(enemies));

            var currentCells = new HashSet<GridPosition>(orderedEnemies.Select(enemy => enemy.Position));
            var reservedDestinations = new HashSet<GridPosition>();
            var plans = new Dictionary<EnemyId, GridPosition?>();

            foreach (EnemyUnit enemy in orderedEnemies)
            {
                if (enemy.BattleState.IsDead)
                {
                    plans.Add(enemy.Id, null);
                    continue;
                }
                if (enemy.Stun.IsStunned)
                {
                    plans.Add(enemy.Id, null);
                    continue;
                }

                GridPosition? destination = SelectDestination(
                    enemy,
                    fieldRange,
                    priority,
                    currentCells,
                    reservedDestinations);
                plans.Add(enemy.Id, destination);
                if (destination.HasValue)
                    reservedDestinations.Add(destination.Value);
            }

            var results = new List<EnemyMovementResult>(orderedEnemies.Length);
            foreach (EnemyUnit enemy in orderedEnemies)
            {
                GridPosition origin = enemy.Position;
                if (enemy.BattleState.IsDead)
                {
                    results.Add(new EnemyMovementResult(
                        enemy, origin, null, EnemyMovementOutcome.DeadEnemy, null));
                    continue;
                }
                if (enemy.Stun.IsStunned)
                {
                    results.Add(new EnemyMovementResult(
                        enemy, origin, null, EnemyMovementOutcome.Stunned, null));
                    continue;
                }

                GridPosition? destination = plans[enemy.Id];
                if (!destination.HasValue)
                {
                    results.Add(new EnemyMovementResult(
                        enemy, origin, null, EnemyMovementOutcome.NoDestination, null));
                    continue;
                }

                ResidualContactResult contact = residualPieces.ResolveEnemyContact(destination.Value);
                if (residualPieces.FindAt(destination.Value) != null)
                {
                    results.Add(new EnemyMovementResult(
                        enemy,
                        origin,
                        destination,
                        EnemyMovementOutcome.StoppedByResidualPiece,
                        contact));
                    continue;
                }

                enemy.Body.MoveTo(destination.Value);
                results.Add(new EnemyMovementResult(
                    enemy, origin, destination, EnemyMovementOutcome.Moved, contact));
            }

            return new EnemyMovementBatchResult(results);
        }

        private static EnemyUnit[] ValidateAndOrder(
            IEnumerable<EnemyUnit> enemies,
            GridShape fieldRange,
            ResidualPieceField residualPieces,
            IEnemyMovementPriority priority)
        {
            if (enemies == null)
                throw new ArgumentNullException(nameof(enemies));
            if (fieldRange == null)
                throw new ArgumentNullException(nameof(fieldRange));
            if (residualPieces == null)
                throw new ArgumentNullException(nameof(residualPieces));
            if (priority == null)
                throw new ArgumentNullException(nameof(priority));
            if (!fieldRange.Equals(residualPieces.Range))
                throw new ArgumentException("Enemy and residual piece fields must use the same range.", nameof(residualPieces));

            EnemyUnit[] ordered = enemies.ToArray();
            if (ordered.Any(enemy => enemy == null))
                throw new ArgumentException("Enemy collection cannot contain null.", nameof(enemies));
            ordered = ordered.OrderBy(enemy => enemy.Id.Value, StringComparer.Ordinal).ToArray();
            if (ordered.Select(enemy => enemy.Id).Distinct().Count() != ordered.Length)
                throw new ArgumentException("Enemy IDs must be unique.", nameof(enemies));
            if (ordered.Any(enemy => !fieldRange.IsValid(enemy.Position)))
                throw new ArgumentException("Every enemy must be inside the field range.", nameof(enemies));
            if (ordered.Select(enemy => enemy.Position).Distinct().Count() != ordered.Length)
                throw new ArgumentException("Enemies cannot share a current cell.", nameof(enemies));
            return ordered;
        }

        private static GridPosition? SelectDestination(
            EnemyUnit enemy,
            GridShape fieldRange,
            IEnemyMovementPriority priority,
            ISet<GridPosition> currentCells,
            ISet<GridPosition> reservedDestinations)
        {
            foreach (GridDirection direction in priority.GetDirections(enemy))
            {
                if (!TryMove(enemy.Position, direction, out GridPosition candidate))
                    continue;
                if (!fieldRange.IsValid(candidate))
                    continue;
                if (currentCells.Contains(candidate) || reservedDestinations.Contains(candidate))
                    continue;
                return candidate;
            }

            return null;
        }

        private static bool TryMove(
            GridPosition origin,
            GridDirection direction,
            out GridPosition destination)
        {
            try
            {
                destination = direction switch
                {
                    GridDirection.Up => new GridPosition(origin.X, checked(origin.Y + 1)),
                    GridDirection.Right => new GridPosition(checked(origin.X + 1), origin.Y),
                    GridDirection.Down => new GridPosition(origin.X, checked(origin.Y - 1)),
                    GridDirection.Left => new GridPosition(checked(origin.X - 1), origin.Y),
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                };
                return true;
            }
            catch (OverflowException)
            {
                destination = default;
                return false;
            }
        }
    }
}
