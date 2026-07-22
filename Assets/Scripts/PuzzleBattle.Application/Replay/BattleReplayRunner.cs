using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.BattleResult;
using PuzzleBattle.Domain.Randomness;

namespace PuzzleBattle.Application.Replay
{
    public sealed class BattleReplaySnapshot : IEquatable<BattleReplaySnapshot>
    {
        private readonly KeyValuePair<string, string>[] state;

        public BattleReplaySnapshot(
            int turnNumber,
            BattleResolutionOutcome outcome,
            IEnumerable<KeyValuePair<string, string>> state)
        {
            if (turnNumber <= 0) throw new ArgumentOutOfRangeException(nameof(turnNumber));
            TurnNumber = turnNumber;
            Outcome = outcome;
            this.state = (state ?? throw new ArgumentNullException(nameof(state)))
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .ToArray();
            if (this.state.Any(item => string.IsNullOrWhiteSpace(item.Key)))
                throw new ArgumentException("Snapshot keys are required.", nameof(state));
            if (this.state.Select(item => item.Key).Distinct(StringComparer.Ordinal).Count() != this.state.Length)
                throw new ArgumentException("Snapshot keys must be unique.", nameof(state));
        }

        public int TurnNumber { get; }
        public BattleResolutionOutcome Outcome { get; }
        public IReadOnlyList<KeyValuePair<string, string>> State => state;

        public bool Equals(BattleReplaySnapshot other)
        {
            return other != null && TurnNumber == other.TurnNumber && Outcome == other.Outcome &&
                   state.SequenceEqual(other.state);
        }
        public override bool Equals(object obj) => Equals(obj as BattleReplaySnapshot);
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = ((TurnNumber * 397) ^ (int)Outcome);
                foreach (KeyValuePair<string, string> item in state)
                    hash = (hash * 397) ^ item.GetHashCode();
                return hash;
            }
        }
    }

    public interface IBattleReplayEngine
    {
        void Apply(ReplayPlacementCommand command);
        BattleReplaySnapshot CompleteTurn(int turnNumber);
        bool IsBattleFinished { get; }
    }

    public interface IBattleReplayEngineFactory
    {
        IBattleReplayEngine Create(StageRandomSeed seed);
    }

    public sealed class BattleReplayResult
    {
        internal BattleReplayResult(StageRandomSeed seed, IEnumerable<BattleReplaySnapshot> turns)
        {
            StageSeed = seed;
            TurnSnapshots = turns.ToArray();
            FinalOutcome = TurnSnapshots.Count == 0
                ? BattleResolutionOutcome.Continuing
                : TurnSnapshots[TurnSnapshots.Count - 1].Outcome;
        }
        public StageRandomSeed StageSeed { get; }
        public IReadOnlyList<BattleReplaySnapshot> TurnSnapshots { get; }
        public BattleResolutionOutcome FinalOutcome { get; }
    }

    public sealed class BattleReplayRunner
    {
        private readonly IBattleReplayEngineFactory engineFactory;
        public BattleReplayRunner(IBattleReplayEngineFactory engineFactory) =>
            this.engineFactory = engineFactory ?? throw new ArgumentNullException(nameof(engineFactory));

        public BattleReplayResult RunToCompletion(BattleReplayScript script)
        {
            if (script == null) throw new ArgumentNullException(nameof(script));
            IBattleReplayEngine engine = engineFactory.Create(script.StageSeed)
                ?? throw new InvalidOperationException("Replay engine was not created.");
            var snapshots = new List<BattleReplaySnapshot>();
            foreach (ReplayTurnInput turn in script.Turns)
            {
                if (engine.IsBattleFinished)
                    throw new InvalidOperationException("Replay contains commands after battle completion.");
                foreach (ReplayPlacementCommand command in turn.Placements)
                    engine.Apply(command);
                BattleReplaySnapshot snapshot = engine.CompleteTurn(turn.TurnNumber)
                    ?? throw new InvalidOperationException("Replay engine did not return a turn snapshot.");
                if (snapshot.TurnNumber != turn.TurnNumber)
                    throw new InvalidOperationException("Replay snapshot turn does not match its input turn.");
                snapshots.Add(snapshot);
            }
            if (!engine.IsBattleFinished)
                throw new InvalidOperationException("Replay input ended before battle completion.");
            return new BattleReplayResult(script.StageSeed, snapshots);
        }
    }
}
