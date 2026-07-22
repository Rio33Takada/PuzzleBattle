using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;
using PuzzleBattle.Domain.Randomness;

namespace PuzzleBattle.Application.Replay
{
    public sealed class ReplayPlacementCommand
    {
        public ReplayPlacementCommand(
            CharacterId ownerId,
            PieceId pieceId,
            GridPosition translation)
        {
            if (string.IsNullOrWhiteSpace(ownerId.Value))
                throw new ArgumentException("A command owner is required.", nameof(ownerId));
            if (string.IsNullOrWhiteSpace(pieceId.Value))
                throw new ArgumentException("A command piece is required.", nameof(pieceId));
            OwnerId = ownerId;
            PieceId = pieceId;
            Translation = translation;
        }
        public CharacterId OwnerId { get; }
        public PieceId PieceId { get; }
        public GridPosition Translation { get; }
    }

    public sealed class ReplayTurnInput
    {
        public ReplayTurnInput(int turnNumber, IEnumerable<ReplayPlacementCommand> placements)
        {
            if (turnNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(turnNumber));
            TurnNumber = turnNumber;
            Placements = (placements ?? throw new ArgumentNullException(nameof(placements))).ToArray();
            if (Placements.Any(command => command == null))
                throw new ArgumentException("Replay commands cannot contain null.", nameof(placements));
        }
        public int TurnNumber { get; }
        public IReadOnlyList<ReplayPlacementCommand> Placements { get; }
    }

    public sealed class BattleReplayScript
    {
        public BattleReplayScript(StageRandomSeed stageSeed, IEnumerable<ReplayTurnInput> turns)
        {
            StageSeed = stageSeed;
            Turns = (turns ?? throw new ArgumentNullException(nameof(turns)))
                .OrderBy(turn => turn.TurnNumber)
                .ToArray();
            for (int index = 0; index < Turns.Count; index++)
            {
                if (Turns[index] == null || Turns[index].TurnNumber != index + 1)
                    throw new ArgumentException("Replay turns must be contiguous and start at one.", nameof(turns));
            }
        }
        public StageRandomSeed StageSeed { get; }
        public IReadOnlyList<ReplayTurnInput> Turns { get; }
    }

    public interface IBattleReplayRepository
    {
        void Save(string replayId, BattleReplayScript script);
        BattleReplayScript Load(string replayId);
    }

    public sealed class BattleReplayRecorder
    {
        private readonly StageRandomSeed seed;
        private readonly List<ReplayTurnInput> turns = new List<ReplayTurnInput>();
        public BattleReplayRecorder(StageRandomSeed seed) { this.seed = seed; }

        public void RecordTurn(IEnumerable<ReplayPlacementCommand> placements)
        {
            turns.Add(new ReplayTurnInput(turns.Count + 1, placements));
        }

        public BattleReplayScript Build() => new BattleReplayScript(seed, turns);

        public void Save(string replayId, IBattleReplayRepository repository)
        {
            if (string.IsNullOrWhiteSpace(replayId))
                throw new ArgumentException("A replay ID is required.", nameof(replayId));
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));
            repository.Save(replayId, Build());
        }
    }
}
