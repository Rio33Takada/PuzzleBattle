using System.Collections.Generic;
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
    public sealed class ResidualPieceTests
    {
        private readonly ResidualPieceFactory factory = new ResidualPieceFactory();

        [TestCase(true, false)]
        [TestCase(false, true)]
        public void CreateWhenMissedFieldObject_CreatesOnlyForMiss(
            bool hitFieldObject,
            bool expectedCreated)
        {
            PlacedPiece placedPiece = PlacePiece(2);

            ResidualPiece residual = factory.CreateWhenMissedFieldObject(
                new ResidualPieceId("residual"), placedPiece, hitFieldObject);

            Assert.That(residual != null, Is.EqualTo(expectedCreated));
        }

        [Test]
        public void ResidualPiece_InitialDurabilityEqualsCellCount()
        {
            ResidualPiece residual = factory.CreateWhenMissedFieldObject(
                new ResidualPieceId("residual"), PlacePiece(3), false);

            Assert.That(residual.InitialCellCount, Is.EqualTo(3));
            Assert.That(residual.Durability, Is.EqualTo(3));
        }

        [Test]
        public void ResolveEnemyContact_DecreasesDurabilityUntilZero()
        {
            ResidualPiece residual = factory.CreateWhenMissedFieldObject(
                new ResidualPieceId("residual"), PlacePiece(2), false);
            var field = new ResidualPieceField(GridShape.CreateRectangle(3, 1));
            field.Add(residual);

            ResidualContactResult first = field.ResolveEnemyContact(new GridPosition(0, 0));
            ResidualContactResult second = field.ResolveEnemyContact(new GridPosition(1, 0));

            Assert.That(first.Contacted, Is.True);
            Assert.That(first.Destroyed, Is.False);
            Assert.That(residual.Durability, Is.Zero);
            Assert.That(second.Destroyed, Is.True);
            Assert.That(second.PlayerDamage, Is.EqualTo(2));
        }

        [Test]
        public void ResolveEnemyContact_WhenDestroyed_RemovesEveryOccupiedCell()
        {
            ResidualPiece residual = factory.CreateWhenMissedFieldObject(
                new ResidualPieceId("residual"), PlacePiece(1), false);
            var field = new ResidualPieceField(GridShape.CreateRectangle(2, 1));
            field.Add(residual);

            field.ResolveEnemyContact(new GridPosition(0, 0));

            Assert.That(field.FindAt(new GridPosition(0, 0)), Is.Null);
            Assert.That(field.ResidualPieces, Is.Empty);
        }

        [Test]
        public void ResolveEnemyContact_AfterDestruction_DoesNotDealDamageTwice()
        {
            ResidualPiece residual = factory.CreateWhenMissedFieldObject(
                new ResidualPieceId("residual"), PlacePiece(1), false);
            var field = new ResidualPieceField(GridShape.CreateRectangle(1, 1));
            field.Add(residual);

            ResidualContactResult first = field.ResolveEnemyContact(new GridPosition(0, 0));
            ResidualContactResult second = field.ResolveEnemyContact(new GridPosition(0, 0));

            Assert.That(first.PlayerDamage, Is.EqualTo(1));
            Assert.That(second.Contacted, Is.False);
            Assert.That(second.PlayerDamage, Is.Zero);
        }

        [Test]
        public void EnemyFieldObject_IsFieldObjectWithEnemyIdentity()
        {
            var enemy = new EnemyFieldObject(
                new EnemyId("enemy"),
                new[] { new GridPosition(0, 0) });

            Assert.That(enemy.EnemyId, Is.EqualTo(new EnemyId("enemy")));
            Assert.That(enemy.Cells.Count, Is.EqualTo(1));
        }

        private static PlacedPiece PlacePiece(int cellCount)
        {
            GridShape range = GridShape.CreateRectangle(cellCount, 1);
            var board = new BattleBoard(range, new BattleField(range));
            BattlePiece piece = CreatePiece(cellCount);
            return new PiecePlacementService().TryPlace(
                board,
                new PiecePlacementRequest(piece, new GridPosition(0, 0))).PlacedPiece;
        }

        private static BattlePiece CreatePiece(int cellCount)
        {
            var power = new Dictionary<GridPosition, int>();
            var attributes = new Dictionary<GridPosition, ElementAttribute>();
            var attribute = new ElementAttribute("fire");
            for (int x = 0; x < cellCount; x++)
            {
                var position = new GridPosition(x, 0);
                power.Add(position, 1);
                attributes.Add(position, attribute);
            }

            return new BattlePiece(
                new PieceId("piece"),
                new CharacterId("hero"),
                power,
                attributes);
        }
    }
}
