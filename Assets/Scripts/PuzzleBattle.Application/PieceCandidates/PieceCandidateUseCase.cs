using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Randomness;
using BattleBoard = PuzzleBattle.Domain.Board.Board;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Application.PieceCandidates
{
    public sealed class PieceCandidateUseCase
    {
        private readonly IPiecePlacementAvailabilityService availability;

        public PieceCandidateUseCase(IPiecePlacementAvailabilityService availability = null)
        {
            this.availability = availability ?? new PiecePlacementAvailabilityService();
        }

        public PieceCandidateState Generate(
            PartyFormation formation,
            BattleBoard board,
            IRandomSource random)
        {
            if (formation == null)
                throw new ArgumentNullException(nameof(formation));
            if (board == null)
                throw new ArgumentNullException(nameof(board));
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            List<BattlePiece> shuffled = formation.Characters
                .SelectMany(character => character.Pieces)
                .ToList();

            for (int index = shuffled.Count - 1; index > 0; index--)
            {
                int swapIndex = random.NextInt(0, index + 1);
                BattlePiece temporary = shuffled[index];
                shuffled[index] = shuffled[swapIndex];
                shuffled[swapIndex] = temporary;
            }

            return new PieceCandidateState(shuffled, availability, board);
        }
    }
}
