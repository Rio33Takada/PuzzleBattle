using System.Linq;
using NUnit.Framework;
using PuzzleBattle.Application.BattleFlow;
using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Randomness;
using PuzzleBattle.Domain.Residual;

namespace PuzzleBattle.Domain.Tests
{
    public sealed class EnemyMovementForecastTests
    {
        [Test]
        public void Forecast_IsDeterministicAndDoesNotMoveEnemies()
        {
            GridShape range = GridShape.CreateRectangle(4, 1);
            EnemyUnit first = CreateEnemy("b", 0);
            EnemyUnit second = CreateEnemy("a", 2);
            var priority = new FixedDirectionPriority(new[] { GridDirection.Right });

            EnemyMovementForecastBatch forecast = new EnemyMovementService().Forecast(
                new[] { first, second }, range, new ResidualPieceField(range), priority);

            Assert.That(forecast.Forecasts.Select(item => item.Enemy.Id.Value),
                Is.EqualTo(new[] { "a", "b" }));
            Assert.That(forecast.Forecasts.Select(item => item.Destination.Value),
                Is.EqualTo(new[] { new GridPosition(3, 0), new GridPosition(1, 0) }));
            Assert.That(first.Position, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(second.Position, Is.EqualTo(new GridPosition(2, 0)));
        }

        [Test]
        public void PlayerTurnStart_RefreshesForecastBeforeEachPlayerTurn()
        {
            GridShape range = GridShape.CreateRectangle(2, 1);
            EnemyUnit enemy = CreateEnemy("enemy", 0);
            var movement = new EnemyMovementService();
            var residuals = new ResidualPieceField(range);
            var priority = new FixedDirectionPriority(new[] { GridDirection.Right });
            var service = new EnemyMovementForecastTurnStartService(() =>
                movement.Forecast(new[] { enemy }, range, residuals, priority));
            int updateCount = 0;
            service.ForecastUpdated += _ => updateCount++;
            var session = new BattleFlowSession(
                new DeterministicRandomSource(new StageRandomSeed(1UL)));

            service.Execute(session);
            service.Execute(session);

            Assert.That(updateCount, Is.EqualTo(2));
            Assert.That(service.Current.Forecasts.Single().Destination,
                Is.EqualTo(new GridPosition(1, 0)));
        }

        private static EnemyUnit CreateEnemy(string id, int x)
        {
            var enemyId = new EnemyId(id);
            return new EnemyUnit(
                new EnemyBattleState(enemyId, new EnemyStats(10, 1, 0)),
                new EnemyFieldObject(enemyId, new[] { new GridPosition(x, 0) }));
        }
    }
}
