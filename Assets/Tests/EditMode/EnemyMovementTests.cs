using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;
using PuzzleBattle.Domain.Placement;
using PuzzleBattle.Domain.Residual;
using BattleBoard = PuzzleBattle.Domain.Board.Board;
using BattleField = PuzzleBattle.Domain.Field.Field;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Domain.Tests
{
    public sealed class EnemyMovementTests
    {
        private readonly EnemyMovementService service = new EnemyMovementService();

        [Test]
        public void Resolve_MovesOneCellInOrthogonalPriorityOrder()
        {
            GridShape range = GridShape.CreateRectangle(3, 3);
            EnemyUnit enemy = CreateEnemy("enemy", 1, 1);

            EnemyMovementResult result = service.Resolve(
                new[] { enemy },
                range,
                new ResidualPieceField(range),
                new FixedDirectionPriority(new[] { GridDirection.Up, GridDirection.Right }))
                .Results.Single();

            Assert.That(result.Moved, Is.True);
            Assert.That(enemy.Position, Is.EqualTo(new GridPosition(1, 2)));
        }

        [Test]
        public void Resolve_PlanningIgnoresResidualButRemainingResidualStopsMovement()
        {
            GridShape range = GridShape.CreateRectangle(3, 1);
            EnemyUnit enemy = CreateEnemy("enemy", 0, 0);
            ResidualPieceField residuals = CreateResidualField(range, 2, 1);

            EnemyMovementResult result = service.Resolve(
                new[] { enemy }, range, residuals, RightOnly()).Results.Single();

            Assert.That(result.PlannedDestination, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(result.Outcome, Is.EqualTo(EnemyMovementOutcome.StoppedByResidualPiece));
            Assert.That(enemy.Position, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(residuals.FindAt(new GridPosition(1, 0)).Durability, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_DestroyedResidualAllowsMovementAndReturnsPlayerDamage()
        {
            GridShape range = GridShape.CreateRectangle(2, 1);
            EnemyUnit enemy = CreateEnemy("enemy", 0, 0);
            ResidualPieceField residuals = CreateResidualField(range, 1, 1);

            EnemyMovementResult result = service.Resolve(
                new[] { enemy }, range, residuals, RightOnly()).Results.Single();

            Assert.That(result.Moved, Is.True);
            Assert.That(result.PlayerDamage, Is.EqualTo(1));
            Assert.That(enemy.Position, Is.EqualTo(new GridPosition(1, 0)));
        }

        [Test]
        public void Resolve_SameDestinationCompetition_LowerEnemyIdWinsReservation()
        {
            GridShape range = GridShape.CreateRectangle(3, 1);
            EnemyUnit lower = CreateEnemy("a", 0, 0);
            EnemyUnit higher = CreateEnemy("b", 2, 0);
            var priority = new PerEnemyPriority(new Dictionary<string, GridDirection[]>
            {
                ["a"] = new[] { GridDirection.Right },
                ["b"] = new[] { GridDirection.Left }
            });

            EnemyMovementBatchResult result = service.Resolve(
                new[] { higher, lower }, range, new ResidualPieceField(range), priority);

            Assert.That(lower.Position, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(higher.Position, Is.EqualTo(new GridPosition(2, 0)));
            Assert.That(result.Results.Select(item => item.Enemy.Id.Value), Is.EqualTo(new[] { "a", "b" }));
        }

        [Test]
        public void Resolve_PositionExchange_IsProhibited()
        {
            GridShape range = GridShape.CreateRectangle(2, 1);
            EnemyUnit first = CreateEnemy("a", 0, 0);
            EnemyUnit second = CreateEnemy("b", 1, 0);
            var priority = new PerEnemyPriority(new Dictionary<string, GridDirection[]>
            {
                ["a"] = new[] { GridDirection.Right },
                ["b"] = new[] { GridDirection.Left }
            });

            EnemyMovementBatchResult result = service.Resolve(
                new[] { first, second }, range, new ResidualPieceField(range), priority);

            Assert.That(result.Results.All(item => item.Outcome == EnemyMovementOutcome.NoDestination), Is.True);
            Assert.That(first.Position, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(second.Position, Is.EqualTo(new GridPosition(1, 0)));
        }

        [Test]
        public void EnemyFieldObject_MultipleParts_IsRejectedInInitialVersion()
        {
            Assert.Throws<ArgumentException>(() => new EnemyFieldObject(
                new EnemyId("enemy"),
                new[] { new GridPosition(0, 0), new GridPosition(1, 0) }));
        }

        private static FixedDirectionPriority RightOnly()
        {
            return new FixedDirectionPriority(new[] { GridDirection.Right });
        }

        private static EnemyUnit CreateEnemy(string id, int x, int y)
        {
            var enemyId = new EnemyId(id);
            return new EnemyUnit(
                new EnemyBattleState(enemyId, new EnemyStats(10, 1, 0)),
                new EnemyFieldObject(enemyId, new[] { new GridPosition(x, y) }));
        }

        private static ResidualPieceField CreateResidualField(
            GridShape range,
            int cellCount,
            int translationX)
        {
            GridShape boardRange = GridShape.CreateRectangle(cellCount, 1);
            var board = new BattleBoard(boardRange, new BattleField(boardRange));
            var power = new Dictionary<GridPosition, int>();
            var attributes = new Dictionary<GridPosition, ElementAttribute>();
            for (int x = 0; x < cellCount; x++)
            {
                var position = new GridPosition(x, 0);
                power.Add(position, 1);
                attributes.Add(position, new ElementAttribute("fire"));
            }

            var piece = new BattlePiece(
                new PieceId("piece"), new CharacterId("hero"), power, attributes);
            PlacedPiece placed = new PiecePlacementService().TryPlace(
                board, new PiecePlacementRequest(piece, new GridPosition(0, 0))).PlacedPiece;

            // Re-place on a board using the target field coordinates before residualization.
            GridShape targetBoardRange = range;
            var targetBoard = new BattleBoard(targetBoardRange, new BattleField(targetBoardRange));
            placed = new PiecePlacementService().TryPlace(
                targetBoard,
                new PiecePlacementRequest(piece, new GridPosition(translationX, 0))).PlacedPiece;
            ResidualPiece residual = new ResidualPieceFactory().CreateWhenMissedFieldObject(
                new ResidualPieceId("residual"), placed, false);
            var residualField = new ResidualPieceField(range);
            residualField.Add(residual);
            return residualField;
        }

        private sealed class PerEnemyPriority : IEnemyMovementPriority
        {
            private readonly IReadOnlyDictionary<string, GridDirection[]> directions;

            public PerEnemyPriority(IReadOnlyDictionary<string, GridDirection[]> directions)
            {
                this.directions = directions;
            }

            public IEnumerable<GridDirection> GetDirections(EnemyUnit enemy)
            {
                return directions[enemy.Id.Value];
            }
        }
    }
}
