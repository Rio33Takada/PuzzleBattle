using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;
using BattleCharacter = PuzzleBattle.Domain.Character.Character;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Domain.Tests
{
    public sealed class CharacterPieceTests
    {
        private static readonly ElementAttribute Fire = new ElementAttribute("fire");
        private static readonly ElementAttribute Water = new ElementAttribute("water");

        [Test]
        public void Piece_HoldsDifferentPowerAndAttributeForEachCell()
        {
            var ownerId = new CharacterId("hero");
            BattlePiece piece = CreatePiece("piece", ownerId, Fire, Water);

            Assert.That(piece.GetPower(new GridPosition(0, 0)), Is.EqualTo(10));
            Assert.That(piece.GetPower(new GridPosition(1, 0)), Is.EqualTo(20));
            Assert.That(piece.GetAttribute(new GridPosition(0, 0)), Is.EqualTo(Fire));
            Assert.That(piece.GetAttribute(new GridPosition(1, 0)), Is.EqualTo(Water));
            Assert.That(piece.OwnerId, Is.EqualTo(ownerId));
        }

        [Test]
        public void Piece_MapKeysDoNotMatch_IsRejected()
        {
            var power = new Dictionary<GridPosition, int> { [new GridPosition(0, 0)] = 1 };
            var attributes = new Dictionary<GridPosition, ElementAttribute>
            {
                [new GridPosition(1, 0)] = Fire
            };

            Assert.Throws<ArgumentException>(() => new BattlePiece(
                new PieceId("piece"), new CharacterId("hero"), power, attributes));
        }

        [Test]
        public void Character_UsesStatsForSelectedLevel()
        {
            CharacterLevelTable levels = CreateLevelTable();
            CharacterId ownerId = new CharacterId("hero");

            var character = new BattleCharacter(ownerId, 2, levels, new[]
            {
                CreatePiece("piece", ownerId, Fire, Water)
            });

            Assert.That(character.Level, Is.EqualTo(2));
            Assert.That(character.Stats.BaseHitPoints, Is.EqualTo(200));
            Assert.That(character.Stats.BaseAttack, Is.EqualTo(25));
        }

        [Test]
        public void Character_AttributesAreUnionOfOwnedPieceAttributes()
        {
            CharacterId ownerId = new CharacterId("hero");
            var character = new BattleCharacter(ownerId, 1, CreateLevelTable(), new[]
            {
                CreatePiece("first", ownerId, Fire, Water),
                CreateSingleCellPiece("second", ownerId, Fire)
            });

            Assert.That(character.Attributes, Is.EquivalentTo(new[] { Fire, Water }));
        }

        [Test]
        public void Character_ZeroOwnedPieces_IsRejected()
        {
            Assert.Throws<ArgumentException>(() => new BattleCharacter(
                new CharacterId("hero"), 1, CreateLevelTable(), Array.Empty<BattlePiece>()));
        }

        [Test]
        public void Character_PieceOwnedByAnotherCharacter_IsRejected()
        {
            BattlePiece foreignPiece = CreateSingleCellPiece(
                "foreign", new CharacterId("other"), Fire);

            Assert.Throws<ArgumentException>(() => new BattleCharacter(
                new CharacterId("hero"), 1, CreateLevelTable(), new[] { foreignPiece }));
        }

        [Test]
        public void PartyFormation_FourCharacters_IsAccepted()
        {
            BattleCharacter[] characters = Enumerable.Range(1, 4)
                .Select(index => CreateCharacter($"hero-{index}"))
                .ToArray();

            var formation = new PartyFormation(characters);

            Assert.That(formation.Characters.Count, Is.EqualTo(4));
        }

        [Test]
        public void PartyFormation_FiveCharacters_IsRejected()
        {
            BattleCharacter[] characters = Enumerable.Range(1, 5)
                .Select(index => CreateCharacter($"hero-{index}"))
                .ToArray();

            Assert.Throws<ArgumentException>(() => new PartyFormation(characters));
        }

        private static BattleCharacter CreateCharacter(string id)
        {
            var characterId = new CharacterId(id);
            return new BattleCharacter(characterId, 1, CreateLevelTable(), new[]
            {
                CreateSingleCellPiece($"{id}-piece", characterId, Fire)
            });
        }

        private static CharacterLevelTable CreateLevelTable()
        {
            return new CharacterLevelTable(new[]
            {
                new KeyValuePair<int, CharacterLevelStats>(1, new CharacterLevelStats(100, 10)),
                new KeyValuePair<int, CharacterLevelStats>(2, new CharacterLevelStats(200, 25))
            });
        }

        private static BattlePiece CreatePiece(
            string id,
            CharacterId ownerId,
            ElementAttribute first,
            ElementAttribute second)
        {
            var power = new Dictionary<GridPosition, int>
            {
                [new GridPosition(0, 0)] = 10,
                [new GridPosition(1, 0)] = 20
            };
            var attributes = new Dictionary<GridPosition, ElementAttribute>
            {
                [new GridPosition(0, 0)] = first,
                [new GridPosition(1, 0)] = second
            };
            return new BattlePiece(new PieceId(id), ownerId, power, attributes);
        }

        private static BattlePiece CreateSingleCellPiece(string id, CharacterId ownerId, ElementAttribute attribute)
        {
            var position = new GridPosition(0, 0);
            return new BattlePiece(
                new PieceId(id),
                ownerId,
                new Dictionary<GridPosition, int> { [position] = 1 },
                new Dictionary<GridPosition, ElementAttribute> { [position] = attribute });
        }
    }
}
