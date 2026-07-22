using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PuzzleBattle.Application.PieceCandidates;
using PuzzleBattle.Domain.Board;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Field;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;
using PuzzleBattle.Domain.Randomness;
using BattleBoard = PuzzleBattle.Domain.Board.Board;
using BattleCharacter = PuzzleBattle.Domain.Character.Character;
using BattleField = PuzzleBattle.Domain.Field.Field;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Application.Tests
{
    public sealed class PieceCandidateUseCaseTests
    {
        [Test]
        public void Generate_ShufflesAllPiecesAndExposesFirstThree()
        {
            BattlePiece[] pieces = CreatePieces(5);
            PartyFormation formation = CreateFormation(pieces);
            var availability = new FakeAvailability(pieces.Select(piece => piece.PieceId.Value));
            var random = new ZeroRandomSource();

            PieceCandidateState state = new PieceCandidateUseCase(availability).Generate(
                formation, CreateBoard(), random);

            Assert.That(state.Candidates.Select(item => item.Piece.PieceId.Value),
                Is.EqualTo(new[] { "piece-2", "piece-3", "piece-4" }));
            Assert.That(state.RemainingPieceCount, Is.EqualTo(5));
            Assert.That(random.GeneratedValueCount, Is.EqualTo(4UL));
        }

        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(3, false)]
        public void Generate_ReportsZeroToThreePlaceableCandidates(int placeableCount, bool canConfirm)
        {
            BattlePiece[] pieces = CreatePieces(3);
            string[] placeableIds = pieces
                .Take(placeableCount)
                .Select(piece => piece.PieceId.Value)
                .ToArray();
            var availability = new FakeAvailability(placeableIds);

            PieceCandidateState state = new PieceCandidateUseCase(availability).Generate(
                CreateFormation(pieces), CreateBoard(), new IdentityRandomSource());

            Assert.That(state.Candidates.Count(candidate => candidate.CanPlace), Is.EqualTo(placeableCount));
            Assert.That(state.CanConfirm, Is.EqualTo(canConfirm));
        }

        [Test]
        public void ConsumePlaced_RemovesVisiblePieceAndReplenishesCandidateWindow()
        {
            BattlePiece[] pieces = CreatePieces(4);
            var availability = new FakeAvailability(pieces.Select(piece => piece.PieceId.Value));
            BattleBoard board = CreateBoard();
            PieceCandidateState state = new PieceCandidateUseCase(availability).Generate(
                CreateFormation(pieces), board, new IdentityRandomSource());

            BattlePiece consumed = state.Candidates[1].Piece;
            state.ConsumePlaced(consumed, board);

            Assert.That(state.Candidates.Count, Is.EqualTo(3));
            Assert.That(state.Candidates.Select(item => item.Piece), Has.No.Member(consumed));
            Assert.That(state.Candidates.Select(item => item.Piece.PieceId.Value),
                Does.Contain("piece-4"));
            Assert.That(state.RemainingPieceCount, Is.EqualTo(3));
        }

        [Test]
        public void Refresh_ReevaluatesTemporaryPlacementAvailability()
        {
            BattlePiece[] pieces = CreatePieces(3);
            var availability = new FakeAvailability(pieces.Select(piece => piece.PieceId.Value));
            BattleBoard board = CreateBoard();
            PieceCandidateState state = new PieceCandidateUseCase(availability).Generate(
                CreateFormation(pieces), board, new IdentityRandomSource());
            Assert.That(state.CanConfirm, Is.False);

            availability.PlaceableIds.Clear();
            state.Refresh(board);

            Assert.That(state.Candidates.All(candidate => !candidate.CanPlace), Is.True);
            Assert.That(state.CanConfirm, Is.True);
        }

        [Test]
        public void RealAvailability_FindsOffsetTranslationAndRejectsBlockedBoard()
        {
            var local = new GridPosition(5, 5);
            var piece = new BattlePiece(
                new PieceId("offset"),
                new CharacterId("hero"),
                new Dictionary<GridPosition, int> { [local] = 1 },
                new Dictionary<GridPosition, ElementAttribute>
                {
                    [local] = new ElementAttribute("fire")
                });
            var service = new PiecePlacementAvailabilityService();
            BattleBoard blank = CreateBoard();
            GridShape range = GridShape.CreateRectangle(1, 1);
            var blocker = new BoardObject(new GridObjectId("blocker"), new[] { new GridPosition(0, 0) });
            var blocked = new BattleBoard(range, new BattleField(range), new[] { blocker });

            IReadOnlyList<GridPosition> blankTranslations = service.FindValidTranslations(blank, piece);
            IReadOnlyList<GridPosition> blockedTranslations = service.FindValidTranslations(blocked, piece);

            Assert.That(blankTranslations, Is.EqualTo(new[] { new GridPosition(-5, -5) }));
            Assert.That(blockedTranslations, Is.Empty);
        }

        [Test]
        public void ConsumePlaced_RejectsPlacementImpossibleCandidate()
        {
            BattlePiece[] pieces = CreatePieces(3);
            BattleBoard board = CreateBoard();
            PieceCandidateState state = new PieceCandidateUseCase(new FakeAvailability(Array.Empty<string>()))
                .Generate(CreateFormation(pieces), board, new IdentityRandomSource());

            Assert.Throws<InvalidOperationException>(() =>
                state.ConsumePlaced(state.Candidates[0].Piece, board));
        }

        private static BattlePiece[] CreatePieces(int count)
        {
            var owner = new CharacterId("hero");
            var cell = new GridPosition(0, 0);
            return Enumerable.Range(1, count).Select(index => new BattlePiece(
                new PieceId("piece-" + index),
                owner,
                new Dictionary<GridPosition, int> { [cell] = 1 },
                new Dictionary<GridPosition, ElementAttribute>
                {
                    [cell] = new ElementAttribute("fire")
                })).ToArray();
        }

        private static PartyFormation CreateFormation(BattlePiece[] pieces)
        {
            var levels = new CharacterLevelTable(new[]
            {
                new KeyValuePair<int, CharacterLevelStats>(1, new CharacterLevelStats(10, 1))
            });
            var character = new BattleCharacter(new CharacterId("hero"), 1, levels, pieces);
            return new PartyFormation(new[] { character });
        }

        private static BattleBoard CreateBoard()
        {
            GridShape range = GridShape.CreateRectangle(1, 1);
            return new BattleBoard(range, new BattleField(range));
        }

        private sealed class FakeAvailability : IPiecePlacementAvailabilityService
        {
            public FakeAvailability(IEnumerable<string> placeableIds)
            {
                PlaceableIds = new HashSet<string>(placeableIds);
            }

            public HashSet<string> PlaceableIds { get; }

            public IReadOnlyList<GridPosition> FindValidTranslations(BattleBoard board, BattlePiece piece)
            {
                return PlaceableIds.Contains(piece.PieceId.Value)
                    ? new[] { new GridPosition(0, 0) }
                    : Array.Empty<GridPosition>();
            }
        }

        private class IdentityRandomSource : IRandomSource
        {
            public ulong GeneratedValueCount { get; protected set; }
            public uint NextUInt32() { GeneratedValueCount++; return 0; }
            public virtual int NextInt(int minimumInclusive, int maximumExclusive)
            {
                GeneratedValueCount++;
                return maximumExclusive - 1;
            }
            public double NextUnitDouble() { GeneratedValueCount += 2; return 0d; }
        }

        private sealed class ZeroRandomSource : IdentityRandomSource
        {
            public override int NextInt(int minimumInclusive, int maximumExclusive)
            {
                GeneratedValueCount++;
                return minimumInclusive;
            }
        }
    }
}
