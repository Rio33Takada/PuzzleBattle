using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Combat;
using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;
using BattleCharacter = PuzzleBattle.Domain.Character.Character;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Domain.Tests
{
    public sealed class BattleParticipantStateTests
    {
        [Test]
        public void PlayerMaximumHitPoints_IsSumOfFormationCharacterHitPoints()
        {
            var formation = new PartyFormation(new[]
            {
                CreateCharacter("first", 100),
                CreateCharacter("second", 250)
            });

            var player = new PlayerBattleState(formation);

            Assert.That(player.MaximumHitPoints, Is.EqualTo(350));
            Assert.That(player.CurrentHitPoints, Is.EqualTo(350));
        }

        [TestCase(0, 100, 0, false)]
        [TestCase(40, 60, 40, false)]
        [TestCase(100, 0, 100, true)]
        [TestCase(999, 0, 100, true)]
        public void PlayerReceiveDamage_ClampsAtZero(
            int damage,
            int expectedHitPoints,
            int expectedAppliedDamage,
            bool expectedDefeated)
        {
            var player = new PlayerBattleState(
                new PartyFormation(new[] { CreateCharacter("hero", 100) }));

            HealthChangeResult result = player.ReceiveDamage(damage);

            Assert.That(player.CurrentHitPoints, Is.EqualTo(expectedHitPoints));
            Assert.That(result.AppliedDamage, Is.EqualTo(expectedAppliedDamage));
            Assert.That(player.IsDefeated, Is.EqualTo(expectedDefeated));
        }

        [Test]
        public void Enemy_HoldsAttackAndDefenseValues()
        {
            var enemy = new EnemyBattleState(
                new EnemyId("enemy"),
                new EnemyStats(200, 30, 12));

            Assert.That(enemy.CurrentHitPoints, Is.EqualTo(200));
            Assert.That(enemy.Attack, Is.EqualTo(30));
            Assert.That(enemy.Defense, Is.EqualTo(12));
        }

        [Test]
        public void EnemyHitPointsReachZero_EnemyDies()
        {
            var enemy = new EnemyBattleState(
                new EnemyId("enemy"),
                new EnemyStats(50, 10, 3));

            HealthChangeResult result = enemy.ReceiveDamage(50);

            Assert.That(enemy.IsDead, Is.True);
            Assert.That(result.BecameDead, Is.True);
        }

        [Test]
        public void DeadEnemy_AdditionalDamageDoesNotChangeStateOrKillTwice()
        {
            var enemy = new EnemyBattleState(
                new EnemyId("enemy"),
                new EnemyStats(10, 1, 0));
            enemy.ReceiveDamage(10);

            HealthChangeResult result = enemy.ReceiveDamage(100);

            Assert.That(result.AppliedDamage, Is.Zero);
            Assert.That(result.BecameDead, Is.False);
            Assert.That(enemy.CurrentHitPoints, Is.Zero);
        }

        private static BattleCharacter CreateCharacter(string id, int hitPoints)
        {
            CharacterId characterId = new CharacterId(id);
            var levels = new CharacterLevelTable(new[]
            {
                new KeyValuePair<int, CharacterLevelStats>(
                    1,
                    new CharacterLevelStats(hitPoints, 10))
            });
            var position = new GridPosition(0, 0);
            var piece = new BattlePiece(
                new PieceId(id + "-piece"),
                characterId,
                new Dictionary<GridPosition, int> { [position] = 1 },
                new Dictionary<GridPosition, ElementAttribute>
                {
                    [position] = new ElementAttribute("fire")
                });

            return new BattleCharacter(characterId, 1, levels, new[] { piece });
        }
    }
}
