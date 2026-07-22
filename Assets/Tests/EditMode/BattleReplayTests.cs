using System;
using System.Collections.Generic;
using NUnit.Framework;
using PuzzleBattle.Application.Replay;
using PuzzleBattle.Domain.BattleResult;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;
using PuzzleBattle.Domain.Randomness;

namespace PuzzleBattle.Application.Tests
{
    public sealed class BattleReplayTests
    {
        [Test]
        public void SameSeedAndPlacementSequence_ReproducesEveryTurnAndFinalResult()
        {
            BattleReplayScript script = CreateScript(12345UL);
            var runner = new BattleReplayRunner(new TestReplayEngineFactory(2));

            BattleReplayResult first = runner.RunToCompletion(script);
            BattleReplayResult replay = runner.RunToCompletion(script);
            BattleReplayComparison comparison = new BattleReplayComparer().Compare(first, replay);

            Assert.That(comparison.Matches, Is.True);
            Assert.That(comparison.FirstDifferentTurn, Is.Null);
            Assert.That(replay.FinalOutcome, Is.EqualTo(BattleResolutionOutcome.Victory));
            Assert.That(replay.TurnSnapshots, Is.EqualTo(first.TurnSnapshots));
        }

        [Test]
        public void ChangedPlacement_ReportsFirstDivergentTurn()
        {
            var runner = new BattleReplayRunner(new TestReplayEngineFactory(2));
            BattleReplayResult expected = runner.RunToCompletion(CreateScript(12345UL));
            BattleReplayResult changed = runner.RunToCompletion(CreateScript(12345UL, 9));

            BattleReplayComparison comparison = new BattleReplayComparer().Compare(expected, changed);

            Assert.That(comparison.Matches, Is.False);
            Assert.That(comparison.FirstDifferentTurn, Is.EqualTo(1));
            Assert.That(comparison.Expected, Is.Not.Null);
            Assert.That(comparison.Actual, Is.Not.Null);
        }

        [Test]
        public void ChangedStageSeed_IsRejectedBeforeTurnComparison()
        {
            var runner = new BattleReplayRunner(new TestReplayEngineFactory(2));
            BattleReplayResult expected = runner.RunToCompletion(CreateScript(1UL));
            BattleReplayResult changed = runner.RunToCompletion(CreateScript(2UL));

            BattleReplayComparison comparison = new BattleReplayComparer().Compare(expected, changed);

            Assert.That(comparison.Matches, Is.False);
            Assert.That(comparison.FirstDifferentTurn, Is.Zero);
        }

        [Test]
        public void RecorderAndRepository_PreserveSeedCommandsAndEmptyTurns()
        {
            var recorder = new BattleReplayRecorder(new StageRandomSeed(77UL));
            recorder.RecordTurn(new[] { CreatePlacement(3) });
            recorder.RecordTurn(Array.Empty<ReplayPlacementCommand>());
            var repository = new MemoryReplayRepository();

            recorder.Save("battle-77", repository);
            BattleReplayScript loaded = repository.Load("battle-77");

            Assert.That(loaded.StageSeed.Value, Is.EqualTo(77UL));
            Assert.That(loaded.Turns.Count, Is.EqualTo(2));
            Assert.That(loaded.Turns[0].Placements[0].Translation, Is.EqualTo(new GridPosition(3, 0)));
            Assert.That(loaded.Turns[1].Placements, Is.Empty);
        }

        [Test]
        public void Runner_RejectsIncompleteAndPostCompletionInputs()
        {
            var runner = new BattleReplayRunner(new TestReplayEngineFactory(2));
            var incomplete = new BattleReplayScript(
                new StageRandomSeed(1UL),
                new[] { new ReplayTurnInput(1, Array.Empty<ReplayPlacementCommand>()) });
            var excessive = new BattleReplayScript(
                new StageRandomSeed(1UL),
                new[]
                {
                    new ReplayTurnInput(1, Array.Empty<ReplayPlacementCommand>()),
                    new ReplayTurnInput(2, Array.Empty<ReplayPlacementCommand>()),
                    new ReplayTurnInput(3, Array.Empty<ReplayPlacementCommand>())
                });

            Assert.Throws<InvalidOperationException>(() => runner.RunToCompletion(incomplete));
            Assert.Throws<InvalidOperationException>(() => runner.RunToCompletion(excessive));
        }

        private static BattleReplayScript CreateScript(ulong seed, int firstX = 1)
        {
            return new BattleReplayScript(
                new StageRandomSeed(seed),
                new[]
                {
                    new ReplayTurnInput(1, new[] { CreatePlacement(firstX) }),
                    new ReplayTurnInput(2, new[] { CreatePlacement(2) })
                });
        }

        private static ReplayPlacementCommand CreatePlacement(int x)
        {
            return new ReplayPlacementCommand(
                new CharacterId("hero"),
                new PieceId("piece-a"),
                new GridPosition(x, 0));
        }

        private sealed class TestReplayEngineFactory : IBattleReplayEngineFactory
        {
            private readonly int finalTurn;
            public TestReplayEngineFactory(int finalTurn) { this.finalTurn = finalTurn; }
            public IBattleReplayEngine Create(StageRandomSeed seed) =>
                new TestReplayEngine(new DeterministicRandomSource(seed), finalTurn);
        }

        private sealed class TestReplayEngine : IBattleReplayEngine
        {
            private readonly DeterministicRandomSource random;
            private readonly int finalTurn;
            private int placementScore;

            public TestReplayEngine(DeterministicRandomSource random, int finalTurn)
            {
                this.random = random;
                this.finalTurn = finalTurn;
            }

            public bool IsBattleFinished { get; private set; }

            public void Apply(ReplayPlacementCommand command)
            {
                if (command == null) throw new ArgumentNullException(nameof(command));
                placementScore = checked(placementScore + command.Translation.X * 31 + command.Translation.Y);
            }

            public BattleReplaySnapshot CompleteTurn(int turnNumber)
            {
                placementScore = checked(placementScore + random.NextInt(0, 1000));
                IsBattleFinished = turnNumber >= finalTurn;
                BattleResolutionOutcome outcome = IsBattleFinished
                    ? BattleResolutionOutcome.Victory
                    : BattleResolutionOutcome.Continuing;
                return new BattleReplaySnapshot(
                    turnNumber,
                    outcome,
                    new[]
                    {
                        new KeyValuePair<string, string>("random-count", random.GeneratedValueCount.ToString()),
                        new KeyValuePair<string, string>("score", placementScore.ToString())
                    });
            }
        }

        private sealed class MemoryReplayRepository : IBattleReplayRepository
        {
            private readonly Dictionary<string, BattleReplayScript> scripts =
                new Dictionary<string, BattleReplayScript>(StringComparer.Ordinal);

            public void Save(string replayId, BattleReplayScript script) { scripts[replayId] = script; }
            public BattleReplayScript Load(string replayId) { return scripts[replayId]; }
        }
    }
}
