using System;
using System.Linq;
using NUnit.Framework;
using PuzzleBattle.Domain.Board;
using PuzzleBattle.Domain.Field;
using PuzzleBattle.Domain.Grid;
using BattleBoard = PuzzleBattle.Domain.Board.Board;
using BattleField = PuzzleBattle.Domain.Field.Field;

namespace PuzzleBattle.Domain.Tests
{
    public sealed class GridAggregateTests
    {
        [Test]
        public void BoardObject_OneCell_PreservesParentChildRelationship()
        {
            var boardObject = new BoardObject(
                new GridObjectId("piece-1"),
                new[] { new GridPosition(0, 0) });

            GridObjectCell cell = boardObject.Cells.Single();
            Assert.That(cell.Parent, Is.SameAs(boardObject));
            Assert.That(cell.Position, Is.EqualTo(new GridPosition(0, 0)));
        }

        [Test]
        public void FieldObject_MultipleContinuousCells_IsCreated()
        {
            var fieldObject = new FieldObject(
                new GridObjectId("wall-1"),
                new[]
                {
                    new GridPosition(0, 0),
                    new GridPosition(1, 0),
                    new GridPosition(1, 1)
                });

            Assert.That(fieldObject.Cells.Count, Is.EqualTo(3));
            Assert.That(fieldObject.Cells.All(cell => cell.Parent == fieldObject), Is.True);
        }

        [Test]
        public void GridObject_NoCells_IsRejected()
        {
            Assert.Throws<ArgumentException>(() => new BoardObject(
                new GridObjectId("empty"),
                Array.Empty<GridPosition>()));
        }

        [Test]
        public void GridObject_NonContinuousCells_IsRejected()
        {
            Assert.Throws<ArgumentException>(() => new BoardObject(
                new GridObjectId("split"),
                new[] { new GridPosition(0, 0), new GridPosition(2, 0) }));
        }

        [Test]
        public void Field_ObjectOnInvalidCell_IsRejected()
        {
            var range = new GridShape(2, 1, new[] { new GridPosition(0, 0) });
            var fieldObject = new FieldObject(
                new GridObjectId("outside"),
                new[] { new GridPosition(1, 0) });

            Assert.Throws<ArgumentException>(() => new BattleField(range, new[] { fieldObject }));
        }

        [Test]
        public void Field_OverlappingObjects_AreRejected()
        {
            GridShape range = GridShape.CreateRectangle(2, 1);
            var first = new FieldObject(new GridObjectId("first"), new[] { new GridPosition(0, 0) });
            var second = new FieldObject(new GridObjectId("second"), new[] { new GridPosition(0, 0) });

            Assert.Throws<ArgumentException>(() => new BattleField(range, new[] { first, second }));
        }

        [Test]
        public void Board_RangeMatchingActiveField_IsCreated()
        {
            GridShape fieldRange = GridShape.CreateRectangle(2, 2);
            var field = new BattleField(fieldRange);
            var equivalentBoardRange = GridShape.CreateRectangle(2, 2);

            var board = new BattleBoard(equivalentBoardRange, field);

            Assert.That(board.Range, Is.EqualTo(field.Range));
            Assert.That(board.ActiveField, Is.SameAs(field));
        }

        [Test]
        public void Board_RangeDifferentFromActiveField_IsRejected()
        {
            var field = new BattleField(GridShape.CreateRectangle(2, 2));
            GridShape boardRange = GridShape.CreateRectangle(3, 2);

            Assert.Throws<ArgumentException>(() => new BattleBoard(boardRange, field));
        }

        [Test]
        public void BoardObject_OutsideBoardRange_IsRejected()
        {
            GridShape range = GridShape.CreateRectangle(2, 2);
            var field = new BattleField(range);
            var boardObject = new BoardObject(
                new GridObjectId("outside"),
                new[] { new GridPosition(2, 0) });

            Assert.Throws<ArgumentException>(() => new BattleBoard(range, field, new[] { boardObject }));
        }
    }
}
