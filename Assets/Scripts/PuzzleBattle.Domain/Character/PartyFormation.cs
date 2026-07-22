using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleBattle.Domain.Character
{
    public sealed class PartyFormation
    {
        public const int MaximumCharacterCount = 4;
        private readonly Character[] characters;

        public PartyFormation(IEnumerable<Character> characters)
        {
            if (characters == null)
                throw new ArgumentNullException(nameof(characters));

            this.characters = characters.ToArray();
            if (this.characters.Length > MaximumCharacterCount)
                throw new ArgumentException($"A formation can contain at most {MaximumCharacterCount} characters.", nameof(characters));
            if (this.characters.Any(character => character == null))
                throw new ArgumentException("A formation cannot contain null.", nameof(characters));
            if (this.characters.Select(character => character.Id).Distinct().Count() != this.characters.Length)
                throw new ArgumentException("A character cannot appear in a formation more than once.", nameof(characters));
        }

        public IReadOnlyList<Character> Characters => characters;
    }
}
