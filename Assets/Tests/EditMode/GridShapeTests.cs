using System;
using System.Linq;
using NUnit.Framework;
using PuzzleBattle.Domain.Grid;

namespace PuzzleBattle.Domain.Tests
{
    public sealed class GridShapeTests
    {
        [Test]
        public void CreateRectangle_CreatesEveryCellAsValid()
        {
            GridShape shape = GridShape.CreateRectangle(3, 2);

            Assert.That(shape.ValidCells.Count, Is.EqualTo(6));
            Assert.That(shape.IsValid(new GridPosition(0, 0)), Is.True);
            Assert.That(shape.IsValid(new GridPosition(2, 1)), Is.True);
            Assert.That(shape.AreValidCellsContinuous(), Is.True);
        }

        [Test]
        public void Constructor_AllowsHoleInOtherwiseContinuousShape()
        {
            GridPosition[] cells =
            {
                new GridPosition(0, 0), new GridPosition(1, 0), new GridPosition(2, 0),
                new GridPosition(0, 1),                         new GridPosition(2, 1),
                new GridPosition(0, 2), new GridPosition(1, 2), new GridPosition(2, 2)
            };

            var shape = new GridShape(3, 3, cells);

            Assert.That(shape.IsValid(new GridPosition(1, 1)), Is.False);
            Assert.That(shape.AreValidCellsContinuous(), Is.True);
        }

        [Test]
        public void AreValidCellsContinuous_ReturnsFalseForIsolatedCell()
        {
            var shape = new GridShape(3, 1, new[]
            {
                new GridPosition(0, 0),
                new GridPosition(2, 0)
            });

            Assert.That(shape.AreValidCellsContinuous(), Is.False);
        }

        [Test]
        public void EmptyValidCellSet_IsContinuousByDefinition()
        {
            var shape = new GridShape(2, 2, Array.Empty<GridPosition>());

            Assert.That(shape.AreValidCellsContinuous(), Is.True);
        }

        [Test]
        public void GetValidNeighbors_ExcludesHoleDiagonalAndOutOfBoundsCells()
        {
            var shape = new GridShape(2, 2, new[]
            {
                new GridPosition(0, 0),
                new GridPosition(1, 1)
            });

            Assert.That(shape.GetValidNeighbors(new GridPosition(0, 0)), Is.Empty);
            Assert.That(shape.GetValidNeighbors(new GridPosition(-1, 0)), Is.Empty);
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(2, 0)]
        [TestCase(0, 2)]
        public void Constructor_RejectsValidCellOutsideBounds(int x, int y)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new GridShape(2, 2, new[] { new GridPosition(x, y) }));
        }

        [Test]
        public void ValidCells_CannotBeChangedThroughOriginalCollection()
        {
            GridPosition[] source = { new GridPosition(0, 0) };
            var shape = new GridShape(2, 1, source);

            source[0] = new GridPosition(1, 0);

            Assert.That(shape.ValidCells.Single(), Is.EqualTo(new GridPosition(0, 0)));
        }
    }
}
