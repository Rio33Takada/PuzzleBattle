using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PuzzleBattle.Domain.Board;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Field;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;
using PuzzleBattle.Domain.Placement;
using BattleBoard = PuzzleBattle.Domain.Board.Board;
using BattleField = PuzzleBattle.Domain.Field.Field;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Domain.Tests
{
    public sealed class PiecePlacementTests
    {
        private static readonly ElementAttribute Fire = new ElementAttribute("fire");
        private static readonly ElementAttribute Water = new ElementAttribute("water");
        private readonly PiecePlacementService service = new PiecePlacementService();

        [Test]
        public void TryPlace_ValidTranslation_PlacesEveryCellWithoutTransformingShape()
        {
            BattleBoard board = CreateBoard(4, 3);
            BattlePiece piece = CreateTwoCellPiece("piece", new CharacterId("hero"), Fire, Water);

            PiecePlacementResult result = service.TryPlace(
                board,
                new PiecePlacementRequest(piece, new GridPosition(2, 1)));

            Assert.That(result.Succeeded, Is.True);
            Assert.That(board.GetPlacedPieceCell(new GridPosition(2, 1)).SourcePosition,
                Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(board.GetPlacedPieceCell(new GridPosition(3, 1)).SourcePosition,
                Is.EqualTo(new GridPosition(1, 0)));
        }

        [Test]
        public void Evaluate_PieceExtendsOutsideBoard_IsRejectedWithoutMutation()
        {
            BattleBoard board = CreateBoard(2, 2);
            BattlePiece piece = CreateTwoCellPiece("piece", new CharacterId("hero"), Fire, Water);

            PiecePlacementDecision decision = service.Evaluate(
                board,
                new PiecePlacementRequest(piece, new GridPosition(1, 0)));

            Assert.That(decision.CanPlace, Is.False);
            Assert.That(decision.Failure, Is.EqualTo(PiecePlacementFailure.OutsideBoard));
            Assert.That(board.IsOccupied(new GridPosition(1, 0)), Is.False);
        }

        [Test]
        public void Evaluate_OverlapsNonOverwriteableBoardObject_IsRejected()
        {
            GridShape range = GridShape.CreateRectangle(2, 2);
            var obstacle = new BoardObject(
                new GridObjectId("obstacle"),
                new[] { new GridPosition(0, 0) });
            var board = new BattleBoard(range, new BattleField(range), new[] { obstacle });
            BattlePiece piece = CreateSingleCellPiece("piece", new CharacterId("hero"), Fire);

            PiecePlacementDecision decision = service.Evaluate(
                board,
                new PiecePlacementRequest(piece, new GridPosition(0, 0)));

            Assert.That(decision.Failure, Is.EqualTo(PiecePlacementFailure.OverwriteProhibited));
            Assert.That(decision.AffectedCells, Is.EquivalentTo(new[] { new GridPosition(0, 0) }));
        }

        [Test]
        public void TryPlace_OverwritesPieceCells_RecordsPreviousAttributesForNewOwner()
        {
            BattleBoard board = CreateBoard(3, 1);
            CharacterId firstOwner = new CharacterId("first");
            CharacterId secondOwner = new CharacterId("second");
            service.TryPlace(board, new PiecePlacementRequest(
                CreateTwoCellPiece("old", firstOwner, Fire, Water), new GridPosition(0, 0)));

            PiecePlacementResult result = service.TryPlace(board, new PiecePlacementRequest(
                CreateTwoCellPiece("new", secondOwner, Water, Fire), new GridPosition(0, 0)));

            Assert.That(result.Succeeded, Is.True);
            Assert.That(board.GetOverwriteCount(secondOwner, Fire), Is.EqualTo(1));
            Assert.That(board.GetOverwriteCount(secondOwner, Water), Is.EqualTo(1));
            Assert.That(board.GetOverwriteCount(firstOwner, Fire), Is.Zero);
        }

        [Test]
        public void TryPlace_PartialOverlap_RemovesOnlyOverwrittenCellFromOldPiece()
        {
            BattleBoard board = CreateBoard(3, 1);
            PiecePlacementResult oldResult = service.TryPlace(board, new PiecePlacementRequest(
                CreateTwoCellPiece("old", new CharacterId("first"), Fire, Water),
                new GridPosition(0, 0)));

            service.TryPlace(board, new PiecePlacementRequest(
                CreateSingleCellPiece("new", new CharacterId("second"), Fire),
                new GridPosition(1, 0)));

            Assert.That(oldResult.PlacedPiece.RemainingCells.Count, Is.EqualTo(1));
            Assert.That(oldResult.PlacedPiece.RemainingCells.Single().BoardPosition,
                Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(board.GetPlacedPieceCell(new GridPosition(0, 0)).Placement,
                Is.SameAs(oldResult.PlacedPiece));
        }

        private static BattleBoard CreateBoard(int width, int height)
        {
            GridShape range = GridShape.CreateRectangle(width, height);
            return new BattleBoard(range, new BattleField(range));
        }

        private static BattlePiece CreateTwoCellPiece(
            string id,
            CharacterId owner,
            ElementAttribute first,
            ElementAttribute second)
        {
            return new BattlePiece(
                new PieceId(id),
                owner,
                new Dictionary<GridPosition, int>
                {
                    [new GridPosition(0, 0)] = 10,
                    [new GridPosition(1, 0)] = 20
                },
                new Dictionary<GridPosition, ElementAttribute>
                {
                    [new GridPosition(0, 0)] = first,
                    [new GridPosition(1, 0)] = second
                });
        }

        private static BattlePiece CreateSingleCellPiece(
            string id,
            CharacterId owner,
            ElementAttribute attribute)
        {
            var position = new GridPosition(0, 0);
            return new BattlePiece(
                new PieceId(id),
                owner,
                new Dictionary<GridPosition, int> { [position] = 10 },
                new Dictionary<GridPosition, ElementAttribute> { [position] = attribute });
        }
    }
}
