using System;
using PuzzleBattle.Domain.Enemy;

namespace PuzzleBattle.Application.BattleFlow
{
    public sealed class EnemyMovementForecastTurnStartService : IPlayerTurnStartService
    {
        private readonly Func<EnemyMovementForecastBatch> forecastFactory;

        public EnemyMovementForecastTurnStartService(Func<EnemyMovementForecastBatch> forecastFactory)
        {
            this.forecastFactory = forecastFactory ?? throw new ArgumentNullException(nameof(forecastFactory));
        }

        public EnemyMovementForecastBatch Current { get; private set; }
        public event Action<EnemyMovementForecastBatch> ForecastUpdated;

        public void Execute(BattleFlowSession session)
        {
            Current = forecastFactory()
                ?? throw new InvalidOperationException("Enemy movement forecast was not provided.");
            ForecastUpdated?.Invoke(Current);
        }
    }
}
