using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;
using PuzzleBattle.Domain.Placement;
using PuzzleBattle.Domain.Residual;
using PuzzleBattle.Domain.Stun;
using BattleBoard = PuzzleBattle.Domain.Board.Board;
using BattleField = PuzzleBattle.Domain.Field.Field;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Domain.Tests
{
    public sealed class StunResolutionTests
    {
        private static readonly StunSettings Settings = new StunSettings(2, 1.5m);
        private readonly StunResolutionService service = new StunResolutionService();

        [Test]
        public void Resolve_NoAdjacentBlank_StunsAndDestroysReservedResidualsAfterEvaluation()
        {
            GridShape range = GridShape.CreateRectangle(3, 3);
            EnemyUnit enemy = CreateEnemy("enemy", 1, 1);
            ResidualPieceField residuals = SurroundCenter(range);

            StunResolutionResult result = service.Resolve(new[] { enemy }, range, residuals, Settings);

            Assert.That(result.EnemyResults.Single().Outcome, Is.EqualTo(EnemyStunOutcome.NewlyStunned));
            Assert.That(result.DestroyedResiduals.Count, Is.EqualTo(4));
            Assert.That(result.TotalPlayerDamage, Is.EqualTo(4));
            Assert.That(enemy.Stun.IsStunned, Is.True);
        }

        [Test]
        public void Resolve_OneAdjacentBlank_DoesNotStunOrDestroyResiduals()
        {
            GridShape range = GridShape.CreateRectangle(3, 3);
            EnemyUnit enemy = CreateEnemy("enemy", 1, 1);
            var residuals = new ResidualPieceField(range);
            AddResidual(residuals, range, "up", 1, 2);
            AddResidual(residuals, range, "right", 2, 1);
            AddResidual(residuals, range, "down", 1, 0);

            StunResolutionResult result = service.Resolve(new[] { enemy }, range, residuals, Settings);

            Assert.That(result.EnemyResults.Single().AdjacentBlankCount, Is.EqualTo(1));
            Assert.That(enemy.Stun.IsStunned, Is.False);
            Assert.That(residuals.ResidualPieces.Count, Is.EqualTo(3));
        }

        [Test]
        public void Resolve_AlreadyStunned_AdvancesCounterAndRecoversAtZero()
        {
            GridShape range = GridShape.CreateRectangle(3, 3);
            EnemyUnit enemy = CreateEnemy("enemy", 1, 1);
            service.Resolve(new[] { enemy }, range, SurroundCenter(range), Settings);
            var emptyResiduals = new ResidualPieceField(range);

            StunResolutionResult second = service.Resolve(new[] { enemy }, range, emptyResiduals, Settings);
            StunResolutionResult third = service.Resolve(new[] { enemy }, range, emptyResiduals, Settings);

            Assert.That(second.EnemyResults.Single().Outcome, Is.EqualTo(EnemyStunOutcome.StillStunned));
            Assert.That(third.EnemyResults.Single().Outcome, Is.EqualTo(EnemyStunOutcome.Recovered));
            Assert.That(enemy.Stun.RemainingTurns, Is.Zero);
        }

        [Test]
        public void StunnedEnemy_MovementIsStopped()
        {
            GridShape range = GridShape.CreateRectangle(3, 3);
            EnemyUnit enemy = CreateEnemy("enemy", 1, 1);
            service.Resolve(new[] { enemy }, range, SurroundCenter(range), Settings);

            EnemyMovementResult movement = new EnemyMovementService().Resolve(
                new[] { enemy },
                range,
                new ResidualPieceField(range),
                new FixedDirectionPriority(new[] { GridDirection.Up }))
                .Results.Single();

            Assert.That(movement.Outcome, Is.EqualTo(EnemyMovementOutcome.Stunned));
            Assert.That(enemy.Position, Is.EqualTo(new GridPosition(1, 1)));
        }

        [Test]
        public void DamageMultiplier_IsActiveOnlyWhileStunned()
        {
            GridShape range = GridShape.CreateRectangle(3, 3);
            EnemyUnit enemy = CreateEnemy("enemy", 1, 1);
            service.Resolve(new[] { enemy }, range, SurroundCenter(range), Settings);

            Assert.That(enemy.Stun.DamageTakenMultiplier, Is.EqualTo(1.5m));

            var emptyResiduals = new ResidualPieceField(range);
            service.Resolve(new[] { enemy }, range, emptyResiduals, Settings);
            service.Resolve(new[] { enemy }, range, emptyResiduals, Settings);

            Assert.That(enemy.Stun.DamageTakenMultiplier, Is.EqualTo(1m));
        }

        [Test]
        public void StunnedEnemy_AttackCounterAdvancesAndZeroSuppressesAttackThenResets()
        {
            GridShape range = GridShape.CreateRectangle(3, 3);
            EnemyUnit enemy = CreateEnemy("enemy", 1, 1, attackInterval: 2);
            service.Resolve(new[] { enemy }, range, SurroundCenter(range), Settings);
            var actionService = new StunnedEnemyActionService();

            StunnedAttackCounterResult first = actionService.AdvanceAttackCounter(enemy);
            StunnedAttackCounterResult second = actionService.AdvanceAttackCounter(enemy);

            Assert.That(first.CurrentCounter, Is.EqualTo(1));
            Assert.That(first.AttackSuppressed, Is.False);
            Assert.That(second.AttackSuppressed, Is.True);
            Assert.That(second.CurrentCounter, Is.EqualTo(2));
        }

        [Test]
        public void Resolve_ReversedEnemyInput_ProducesSameResults()
        {
            (string forward, int forwardDamage) = ResolveTwoEnemies(false);
            (string reverse, int reverseDamage) = ResolveTwoEnemies(true);

            Assert.That(reverse, Is.EqualTo(forward));
            Assert.That(reverseDamage, Is.EqualTo(forwardDamage));
        }

        private (string outcomes, int damage) ResolveTwoEnemies(bool reverse)
        {
            GridShape range = GridShape.CreateRectangle(5, 3);
            EnemyUnit first = CreateEnemy("a", 1, 1);
            EnemyUnit second = CreateEnemy("b", 3, 1);
            var residuals = new ResidualPieceField(range);
            foreach ((string id, int x, int y) in new[]
                     {
                         ("a-u", 1, 2), ("a-r", 2, 1), ("a-d", 1, 0), ("a-l", 0, 1),
                         ("b-u", 3, 2), ("b-r", 4, 1), ("b-d", 3, 0)
                     })
            {
                AddResidual(residuals, range, id, x, y);
            }

            EnemyUnit[] enemies = reverse ? new[] { second, first } : new[] { first, second };
            StunResolutionResult result = service.Resolve(enemies, range, residuals, Settings);
            string outcomes = string.Join(",", result.EnemyResults.Select(item =>
                item.Enemy.Id.Value + ":" + item.Outcome));
            return (outcomes, result.TotalPlayerDamage);
        }

        private static ResidualPieceField SurroundCenter(GridShape range)
        {
            var residuals = new ResidualPieceField(range);
            AddResidual(residuals, range, "up", 1, 2);
            AddResidual(residuals, range, "right", 2, 1);
            AddResidual(residuals, range, "down", 1, 0);
            AddResidual(residuals, range, "left", 0, 1);
            return residuals;
        }

        private static void AddResidual(
            ResidualPieceField residuals,
            GridShape range,
            string id,
            int x,
            int y)
        {
            var local = new GridPosition(0, 0);
            var piece = new BattlePiece(
                new PieceId(id + "-piece"),
                new CharacterId("hero"),
                new Dictionary<GridPosition, int> { [local] = 1 },
                new Dictionary<GridPosition, ElementAttribute>
                {
                    [local] = new ElementAttribute("fire")
                });
            var board = new BattleBoard(range, new BattleField(range));
            PlacedPiece placed = new PiecePlacementService().TryPlace(
                board,
                new PiecePlacementRequest(piece, new GridPosition(x, y))).PlacedPiece;
            ResidualPiece residual = new ResidualPieceFactory().CreateWhenMissedFieldObject(
                new ResidualPieceId(id), placed, false);
            residuals.Add(residual);
        }

        private static EnemyUnit CreateEnemy(string id, int x, int y, int attackInterval = 1)
        {
            var enemyId = new EnemyId(id);
            return new EnemyUnit(
                new EnemyBattleState(enemyId, new EnemyStats(10, 1, 0), attackInterval),
                new EnemyFieldObject(enemyId, new[] { new GridPosition(x, y) }));
        }
    }
}
