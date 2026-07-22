using System.Linq;
using NUnit.Framework;
using PuzzleBattle.Domain.Grid;

namespace PuzzleBattle.Domain.Tests
{
    public sealed class GridPositionTests
    {
        [TestCase(0, 0, 0, 1, true)]
        [TestCase(0, 0, 1, 0, true)]
        [TestCase(0, 0, 0, -1, true)]
        [TestCase(0, 0, -1, 0, true)]
        [TestCase(0, 0, 1, 1, false)]
        [TestCase(0, 0, 0, 0, false)]
        [TestCase(0, 0, 0, 2, false)]
        public void IsOrthogonallyAdjacentTo_UsesOnlyFourDirections(
            int x,
            int y,
            int otherX,
            int otherY,
            bool expected)
        {
            var position = new GridPosition(x, y);
            var other = new GridPosition(otherX, otherY);

            Assert.That(position.IsOrthogonallyAdjacentTo(other), Is.EqualTo(expected));
        }

        [Test]
        public void IsOrthogonallyAdjacentTo_IntBoundaries_DoesNotOverflow()
        {
            var minimum = new GridPosition(int.MinValue, 0);
            var maximum = new GridPosition(int.MaxValue, 0);

            Assert.That(minimum.IsOrthogonallyAdjacentTo(maximum), Is.False);
        }

        [Test]
        public void GetOrthogonalNeighbors_ReturnsExactlyFourUniqueCells()
        {
            GridPosition[] neighbors = new GridPosition(3, 4)
                .GetOrthogonalNeighbors()
                .ToArray();

            Assert.That(neighbors, Is.EquivalentTo(new[]
            {
                new GridPosition(3, 5),
                new GridPosition(4, 4),
                new GridPosition(3, 3),
                new GridPosition(2, 4)
            }));
        }
    }
}
