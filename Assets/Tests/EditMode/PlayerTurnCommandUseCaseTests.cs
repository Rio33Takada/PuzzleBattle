using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PuzzleBattle.Application.BattleFlow;
using PuzzleBattle.Application.PieceCandidates;
using PuzzleBattle.Application.PlayerTurn;
using PuzzleBattle.Domain.BattleResult;
using PuzzleBattle.Domain.Board;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Field;
using PuzzleBattle.Domain.Gimmick;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;
using PuzzleBattle.Domain.Placement;
using PuzzleBattle.Domain.Randomness;
using PuzzleBattle.Domain.Skill;
using BattleBoard = PuzzleBattle.Domain.Board.Board;
using BattleCharacter = PuzzleBattle.Domain.Character.Character;
using BattleField = PuzzleBattle.Domain.Field.Field;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Application.Tests
{
    public sealed class PlayerTurnCommandUseCaseTests
    {
        [Test]
        public void ActiveSkill_IsAllowedBeforePlacementAndClosedAfterSuccessfulPlacementAndUndo()
        {
            BattlePiece piece = CreatePiece("piece", 1);
            BattleBoard board = CreateBoard(1);
            PieceCandidateState candidates = CreateCandidates(board, piece);
            var services = new RecordingServices();
            var useCase = CreateUseCase(board, candidates, services);

            useCase.UseActiveSkill(null);
            useCase.Place(piece, new GridPosition(0, 0));

            Assert.That(services.Calls, Is.EqualTo(new[] { "active" }));
            Assert.That(useCase.CanUseActiveSkill, Is.False);
            Assert.Throws<InvalidOperationException>(() => useCase.UseActiveSkill(null));

            useCase.UndoLastPlacement();
            Assert.That(useCase.CanUseActiveSkill, Is.False);
        }

        [Test]
        public void MultipleUndo_RestoresBoardAndCandidateOrderInLifoOrder()
        {
            BattlePiece first = CreatePiece("first", 1);
            BattlePiece second = CreatePiece("second", 1);
            BattleBoard board = CreateBoard(2);
            PieceCandidateState candidates = CreateCandidates(board, first, second);
            var useCase = CreateUseCase(board, candidates, new RecordingServices());

            useCase.Place(first, new GridPosition(0, 0));
            useCase.Place(second, new GridPosition(1, 0));
            Assert.That(useCase.PlacedPieces.Count, Is.EqualTo(2));

            Assert.That(useCase.UndoLastPlacement(), Is.True);
            Assert.That(board.GetPlacedPieceCell(new GridPosition(1, 0)), Is.Null);
            Assert.That(candidates.Candidates[0].Piece, Is.SameAs(second));

            Assert.That(useCase.UndoLastPlacement(), Is.True);
            Assert.That(board.GetPlacedPieceCell(new GridPosition(0, 0)), Is.Null);
            Assert.That(candidates.Candidates.Select(item => item.Piece), Is.EqualTo(new[] { first, second }));
            Assert.That(useCase.UndoLastPlacement(), Is.False);
        }

        [Test]
        public void Undo_RestoresOverwrittenPieceGimmickAndOverwriteCount()
        {
            BattlePiece oldPiece = CreatePiece("old", 1, "water");
            BattlePiece newPiece = CreatePiece("new", 1, "fire");
            BattleBoard board = CreateBoard(2);
            var placement = new PiecePlacementService();
            PlacedPiece oldPlaced = placement.TryPlace(
                board, new PiecePlacementRequest(oldPiece, new GridPosition(0, 0))).PlacedPiece;
            var bomb = new BombGimmick(
                new GimmickId("bomb"), new PuzzleBattle.Domain.Enemy.EnemyId("enemy"),
                new GridPosition(1, 0), 2, 1);
            new BoardGimmickService().TryPlace(board, bomb);

            PiecePlacementResult overwritten = placement.TryPlace(
                board, new PiecePlacementRequest(newPiece, new GridPosition(0, 0)));
            PiecePlacementResult removedBomb = placement.TryPlace(
                board, new PiecePlacementRequest(CreatePiece("bomb-overwriter", 1), new GridPosition(1, 0)));
            Assert.That(board.GetOverwriteCount(newPiece.OwnerId, new ElementAttribute("water")), Is.EqualTo(1));

            var undo = new PiecePlacementUndoService();
            undo.Undo(board, removedBomb);
            undo.Undo(board, overwritten);

            Assert.That(board.GetPlacedPieceCell(new GridPosition(0, 0)).Placement, Is.SameAs(oldPlaced));
            Assert.That(board.GetGimmick(new GridPosition(1, 0)), Is.SameAs(bomb));
            Assert.That(board.GetOverwriteCount(newPiece.OwnerId, new ElementAttribute("water")), Is.Zero);
        }

        [Test]
        public void Confirm_AdvancesBombsThenAppliesPassivesThenContinues()
        {
            BattlePiece piece = CreatePiece("piece", 1);
            BattleBoard board = CreateBoard(1);
            PieceCandidateState candidates = CreateCandidates(board, piece);
            var services = new RecordingServices();
            var useCase = CreateUseCase(board, candidates, services);
            useCase.Place(piece, new GridPosition(0, 0));

            PlayerTurnConfirmationResult result = useCase.Confirm();

            Assert.That(services.Calls, Is.EqualTo(new[] { "bomb", "passive", "continue" }));
            Assert.That(result.Continuation.NextPhase, Is.EqualTo(BattleFlowPhase.ResolvingEnemyTurn));
            Assert.That(useCase.IsConfirmed, Is.True);
            Assert.Throws<InvalidOperationException>(() => useCase.UndoLastPlacement());
            Assert.Throws<InvalidOperationException>(() => useCase.Place(piece, new GridPosition(0, 0)));
        }

        [Test]
        public void Confirm_IsRejectedWhileAnyVisibleCandidateCanBePlaced()
        {
            BattlePiece piece = CreatePiece("piece", 1);
            BattleBoard board = CreateBoard(1);
            var useCase = CreateUseCase(
                board, CreateCandidates(board, piece), new RecordingServices());

            Assert.That(useCase.CanConfirm, Is.False);
            Assert.Throws<InvalidOperationException>(() => useCase.Confirm());
        }

        private static PlayerTurnCommandUseCase CreateUseCase(
            BattleBoard board,
            PieceCandidateState candidates,
            RecordingServices services)
        {
            return new PlayerTurnCommandUseCase(
                board, candidates, services, services, services, services);
        }

        private static PieceCandidateState CreateCandidates(BattleBoard board, params BattlePiece[] pieces)
        {
            var levels = new CharacterLevelTable(new[]
            {
                new KeyValuePair<int, CharacterLevelStats>(1, new CharacterLevelStats(10, 1))
            });
            var character = new BattleCharacter(new CharacterId("hero"), 1, levels, pieces);
            return new PieceCandidateUseCase().Generate(
                new PartyFormation(new[] { character }), board, new IdentityRandom());
        }

        private static BattleBoard CreateBoard(int width)
        {
            GridShape range = GridShape.CreateRectangle(width, 1);
            return new BattleBoard(range, new BattleField(range));
        }

        private static BattlePiece CreatePiece(string id, int power, string attribute = "fire")
        {
            var cell = new GridPosition(0, 0);
            return new BattlePiece(
                new PieceId(id), new CharacterId("hero"),
                new Dictionary<GridPosition, int> { [cell] = power },
                new Dictionary<GridPosition, ElementAttribute>
                {
                    [cell] = new ElementAttribute(attribute)
                });
        }

        private sealed class IdentityRandom : IRandomSource
        {
            public ulong GeneratedValueCount { get; private set; }
            public uint NextUInt32() { GeneratedValueCount++; return 0; }
            public int NextInt(int minimumInclusive, int maximumExclusive)
            {
                GeneratedValueCount++;
                return maximumExclusive - 1;
            }
            public double NextUnitDouble() { GeneratedValueCount += 2; return 0; }
        }

        private sealed class RecordingServices :
            IActiveSkillTurnService,
            IBombConfirmationService,
            IPassiveSkillTurnService,
            IConfirmedPlacementContinuation
        {
            public List<string> Calls { get; } = new List<string>();

            public SkillExecutionResult Execute(SkillDefinition skill)
            {
                Calls.Add("active");
                return null;
            }

            public BombAdvanceResult Advance()
            {
                Calls.Add("bomb");
                return null;
            }

            public IReadOnlyList<SkillExecutionResult> Apply(TurnOverwriteSummary overwrites)
            {
                Calls.Add("passive");
                return Array.Empty<SkillExecutionResult>();
            }

            public BattleTurnResult Continue()
            {
                Calls.Add("continue");
                return new BattleTurnResult(
                    1, BattleResolutionOutcome.Continuing,
                    BattleFlowPhase.ResolvingEnemyTurn, 1);
            }
        }
    }
}
