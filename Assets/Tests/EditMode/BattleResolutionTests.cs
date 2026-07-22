using System.Collections.Generic;
using NUnit.Framework;
using PuzzleBattle.Domain.BattleResult;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Combat;
using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;
using BattleCharacter = PuzzleBattle.Domain.Character.Character;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Domain.Tests
{
    public sealed class BattleResolutionTests
    {
        private readonly BattleResolutionService service = new BattleResolutionService();

        [Test]
        public void Evaluate_LivingEnemy_ContinuesBattle()
        {
            PlayerBattleState player = CreatePlayer();
            EnemyBattleState enemy = CreateEnemy("enemy");

            BattleResolution resolution = service.Evaluate(
                player, new[] { enemy }, new BattleAreaState(1, 2));

            Assert.That(resolution.Outcome, Is.EqualTo(BattleResolutionOutcome.Continuing));
            Assert.That(resolution.NextArea, Is.Null);
            Assert.That(resolution.BattleFinished, Is.False);
            Assert.That(player.CurrentHitPoints, Is.EqualTo(player.MaximumHitPoints));
            Assert.That(enemy.CurrentHitPoints, Is.EqualTo(enemy.Stats.MaximumHitPoints));
        }

        [Test]
        public void Evaluate_DeadPlayer_IsDefeat()
        {
            PlayerBattleState player = CreatePlayer();
            player.ReceiveDamage(player.MaximumHitPoints);

            BattleResolution resolution = service.Evaluate(
                player, new[] { CreateEnemy("enemy") }, new BattleAreaState(1, 2));

            Assert.That(resolution.Outcome, Is.EqualTo(BattleResolutionOutcome.Defeat));
            Assert.That(resolution.BattleFinished, Is.True);
        }

        [Test]
        public void Evaluate_AllEnemiesDeadOnNonFinalArea_ReturnsNextAreaWithoutMutation()
        {
            PlayerBattleState player = CreatePlayer();
            EnemyBattleState enemy = CreateEnemy("enemy");
            enemy.ReceiveDamage(enemy.CurrentHitPoints);
            var currentArea = new BattleAreaState(1, 3);

            BattleResolution resolution = service.Evaluate(player, new[] { enemy }, currentArea);

            Assert.That(resolution.Outcome, Is.EqualTo(BattleResolutionOutcome.AreaCleared));
            Assert.That(resolution.CurrentArea, Is.EqualTo(new BattleAreaState(1, 3)));
            Assert.That(resolution.NextArea, Is.EqualTo(new BattleAreaState(2, 3)));
            Assert.That(currentArea.AreaNumber, Is.EqualTo(1));
        }

        [Test]
        public void Evaluate_AllEnemiesDeadOnFinalArea_IsVictory()
        {
            EnemyBattleState enemy = CreateEnemy("enemy");
            enemy.ReceiveDamage(enemy.CurrentHitPoints);

            BattleResolution resolution = service.Evaluate(
                CreatePlayer(), new[] { enemy }, new BattleAreaState(3, 3));

            Assert.That(resolution.Outcome, Is.EqualTo(BattleResolutionOutcome.Victory));
            Assert.That(resolution.BattleFinished, Is.True);
            Assert.That(resolution.NextArea, Is.Null);
        }

        [Test]
        public void Evaluate_SimultaneousDeaths_PrioritizesDefeat()
        {
            PlayerBattleState player = CreatePlayer();
            player.ReceiveDamage(player.CurrentHitPoints);
            EnemyBattleState enemy = CreateEnemy("enemy");
            enemy.ReceiveDamage(enemy.CurrentHitPoints);

            BattleResolution resolution = service.Evaluate(
                player, new[] { enemy }, new BattleAreaState(1, 1));

            Assert.That(resolution.Outcome, Is.EqualTo(BattleResolutionOutcome.Defeat));
        }

        [Test]
        public void Evaluate_OneEnemyRemaining_DoesNotClearArea()
        {
            EnemyBattleState dead = CreateEnemy("dead");
            dead.ReceiveDamage(dead.CurrentHitPoints);
            EnemyBattleState alive = CreateEnemy("alive");

            BattleResolution resolution = service.Evaluate(
                CreatePlayer(), new[] { dead, alive }, new BattleAreaState(1, 1));

            Assert.That(resolution.Outcome, Is.EqualTo(BattleResolutionOutcome.Continuing));
        }

        private static PlayerBattleState CreatePlayer()
        {
            var ownerId = new CharacterId("hero");
            var cell = new GridPosition(0, 0);
            var piece = new BattlePiece(
                new PieceId("piece"),
                ownerId,
                new Dictionary<GridPosition, int> { [cell] = 1 },
                new Dictionary<GridPosition, ElementAttribute>
                {
                    [cell] = new ElementAttribute("fire")
                });
            var levels = new CharacterLevelTable(new[]
            {
                new KeyValuePair<int, CharacterLevelStats>(1, new CharacterLevelStats(10, 1))
            });
            var character = new BattleCharacter(ownerId, 1, levels, new[] { piece });
            return new PlayerBattleState(new PartyFormation(new[] { character }));
        }

        private static EnemyBattleState CreateEnemy(string id)
        {
            return new EnemyBattleState(new EnemyId(id), new EnemyStats(10, 1, 0));
        }
    }
}
