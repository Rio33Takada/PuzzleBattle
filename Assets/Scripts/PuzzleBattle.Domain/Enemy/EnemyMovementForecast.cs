using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Grid;

namespace PuzzleBattle.Domain.Enemy
{
    public enum EnemyMovementForecastOutcome
    {
        Planned = 0,
        NoDestination = 1,
        Stunned = 2,
        DeadEnemy = 3
    }

    public sealed class EnemyMovementForecast
    {
        internal EnemyMovementForecast(
            EnemyUnit enemy,
            GridPosition origin,
            GridPosition? destination,
            EnemyMovementForecastOutcome outcome,
            bool willContactResidual)
        {
            Enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
            Origin = origin;
            Destination = destination;
            Outcome = outcome;
            WillContactResidual = willContactResidual;
        }
        public EnemyUnit Enemy { get; }
        public GridPosition Origin { get; }
        public GridPosition? Destination { get; }
        public EnemyMovementForecastOutcome Outcome { get; }
        public bool WillContactResidual { get; }
    }

    public sealed class EnemyMovementForecastBatch
    {
        internal EnemyMovementForecastBatch(IEnumerable<EnemyMovementForecast> forecasts)
        {
            Forecasts = forecasts.ToArray();
        }
        public IReadOnlyList<EnemyMovementForecast> Forecasts { get; }
    }
}
