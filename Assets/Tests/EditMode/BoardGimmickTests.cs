using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Gimmick;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;
using PuzzleBattle.Domain.Placement;
using BattleBoard = PuzzleBattle.Domain.Board.Board;
using BattleField = PuzzleBattle.Domain.Field.Field;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Domain.Tests
{
    public sealed class BoardGimmickTests
    {
        private static readonly ElementAttribute Fire = new ElementAttribute("fire");
        private static readonly ElementAttribute Water = new ElementAttribute("water");
        private static readonly EnemyId Enemy = new EnemyId("enemy");
        private readonly BoardGimmickService gimmicks = new BoardGimmickService();
        private readonly PiecePlacementService pieces = new PiecePlacementService();

        [Test]
        public void TryPlace_EnemyGimmick_RequiresEmptyValidCell()
        {
            BattleBoard board = CreateBoard(2, 2);
            var ice = new IceGimmick(new GimmickId("ice"), Enemy, new GridPosition(0, 0), Fire);

            Assert.That(gimmicks.TryPlace(board, ice).Succeeded, Is.True);
            Assert.That(gimmicks.TryPlace(board, new BombGimmick(
                new GimmickId("bomb"), Enemy, ice.Position, 2, 3)).Failure,
                Is.EqualTo(GimmickPlacementFailure.CellOccupied));
        }

        [Test]
        public void Ice_CannotBeOverwrittenByPiece()
        {
            BattleBoard board = CreateBoard(2, 1);
            gimmicks.TryPlace(board, new IceGimmick(
                new GimmickId("ice"), Enemy, new GridPosition(0, 0), Fire));

            PiecePlacementDecision decision = pieces.Evaluate(board,
                new PiecePlacementRequest(CreatePiece("piece", Water), new GridPosition(0, 0)));

            Assert.That(decision.Failure, Is.EqualTo(PiecePlacementFailure.OverwriteProhibited));
        }

        [TestCase("fire", 1)]
        [TestCase("water", 0)]
        public void Ice_DisappearsOnlyForAdjacentMatchingAttribute(string attribute, int expectedMelted)
        {
            BattleBoard board = CreateBoard(2, 1);
            gimmicks.TryPlace(board, new IceGimmick(
                new GimmickId("ice"), Enemy, new GridPosition(0, 0), Fire));
            PlacedPiece placed = pieces.TryPlace(board, new PiecePlacementRequest(
                CreatePiece("piece", new ElementAttribute(attribute)), new GridPosition(1, 0))).PlacedPiece;

            Assert.That(gimmicks.ResolveIceAdjacentTo(board, placed).Count, Is.EqualTo(expectedMelted));
        }

        [Test]
        public void Bomb_CanBeOverwrittenByPiece()
        {
            BattleBoard board = CreateBoard(1, 1);
            var bomb = new BombGimmick(
                new GimmickId("bomb"), Enemy, new GridPosition(0, 0), 2, 5);
            gimmicks.TryPlace(board, bomb);

            PiecePlacementResult result = pieces.TryPlace(board, new PiecePlacementRequest(
                CreatePiece("piece", Fire), new GridPosition(0, 0)));

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.RemovedGimmicks, Is.EquivalentTo(new[] { bomb }));
        }

        [Test]
        public void Bomb_CountReachesZero_DealsConfiguredDamageAndDisappears()
        {
            BattleBoard board = CreateBoard(2, 1);
            var exploding = new BombGimmick(
                new GimmickId("a"), Enemy, new GridPosition(0, 0), 1, 7);
            var remaining = new BombGimmick(
                new GimmickId("b"), Enemy, new GridPosition(1, 0), 2, 9);
            gimmicks.TryPlace(board, exploding);
            gimmicks.TryPlace(board, remaining);

            BombAdvanceResult result = gimmicks.AdvanceBombsAfterPieceConfirmation(board);

            Assert.That(result.TotalDamage, Is.EqualTo(7));
            Assert.That(result.ExplodedBombs.Single(), Is.SameAs(exploding));
            Assert.That(remaining.RemainingCount, Is.EqualTo(1));
            Assert.That(board.GetGimmick(exploding.Position), Is.Null);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Void_DeletesTargetAtMostOnceAndDoesNotRemain(bool hasTarget)
        {
            BattleBoard board = CreateBoard(1, 1);
            if (hasTarget)
            {
                pieces.TryPlace(board, new PiecePlacementRequest(
                    CreatePiece("piece", Fire), new GridPosition(0, 0)));
            }

            var voidGimmick = new VoidGimmick(
                new GimmickId("void"), Enemy, new GridPosition(0, 0));
            GimmickPlacementResult result = gimmicks.TryPlace(board, voidGimmick);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.RemovedCell, Is.EqualTo(hasTarget));
            Assert.That(board.IsOccupied(voidGimmick.Position), Is.False);
        }

        private static BattleBoard CreateBoard(int width, int height)
        {
            GridShape range = GridShape.CreateRectangle(width, height);
            return new BattleBoard(range, new BattleField(range));
        }

        private static BattlePiece CreatePiece(string id, ElementAttribute attribute)
        {
            var position = new GridPosition(0, 0);
            return new BattlePiece(
                new PieceId(id),
                new CharacterId("hero"),
                new Dictionary<GridPosition, int> { [position] = 1 },
                new Dictionary<GridPosition, ElementAttribute> { [position] = attribute });
        }
    }
}
