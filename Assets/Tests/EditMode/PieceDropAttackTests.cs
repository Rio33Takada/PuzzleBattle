using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.DropAttack;
using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Field;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;
using PuzzleBattle.Domain.Placement;
using PuzzleBattle.Domain.Residual;
using PuzzleBattle.Domain.Skill;
using PuzzleBattle.Domain.Stun;
using BattleBoard = PuzzleBattle.Domain.Board.Board;
using BattleCharacter = PuzzleBattle.Domain.Character.Character;
using BattleField = PuzzleBattle.Domain.Field.Field;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Domain.Tests
{
    public sealed class PieceDropAttackTests
    {
        [Test]
        public void Resolve_MultipleCells_DistinguishesEnemyNonTargetAndMiss()
        {
            GridShape range = GridShape.CreateRectangle(3, 1);
            EnemyUnit enemy = CreateEnemy("enemy", 0, defense: 3);
            var decoration = new FieldObject(new GridObjectId("decoration"), new[] { new GridPosition(1, 0) });
            var field = new BattleField(range, new FieldObject[] { enemy.Body, decoration });
            var board = new BattleBoard(range, field);
            BattlePiece piece = CreatePiece("piece", "hero", new[]
            {
                (new GridPosition(0, 0), 2),
                (new GridPosition(1, 0), 2),
                (new GridPosition(2, 0), 2)
            });
            PlacedPiece placed = Place(board, piece, 0);
            BattleCharacter character = CreateCharacter("hero", 5, piece);

            PieceDropAttackResult result = new PieceDropAttackService().Resolve(
                board, new[] { placed }, new[] { character }, new[] { enemy });

            Assert.That(result.Pieces.Single().Cells.Select(cell => cell.Outcome), Is.EqualTo(new[]
            {
                PieceCellDropOutcome.DamagedEnemy,
                PieceCellDropOutcome.NonDamageableTarget,
                PieceCellDropOutcome.Missed
            }));
            Assert.That(result.TotalAppliedDamage, Is.EqualTo(7));
            Assert.That(enemy.BattleState.CurrentHitPoints, Is.EqualTo(93));
        }

        [Test]
        public void Resolve_PreservesPlacementOrderAndTotalsEachPiece()
        {
            GridShape range = GridShape.CreateRectangle(2, 1);
            EnemyUnit firstEnemy = CreateEnemy("first-enemy", 0, defense: 0);
            EnemyUnit secondEnemy = CreateEnemy("second-enemy", 1, defense: 0);
            var field = new BattleField(range, new FieldObject[] { firstEnemy.Body, secondEnemy.Body });
            var board = new BattleBoard(range, field);
            BattlePiece firstPiece = CreatePiece("first", "hero", new[] { (new GridPosition(0, 0), 1) });
            BattlePiece secondPiece = CreatePiece("second", "hero", new[] { (new GridPosition(0, 0), 2) });
            PlacedPiece firstPlaced = Place(board, firstPiece, 0);
            PlacedPiece secondPlaced = Place(board, secondPiece, 1);
            BattleCharacter character = CreateCharacter("hero", 4, firstPiece, secondPiece);

            PieceDropAttackResult result = new PieceDropAttackService().Resolve(
                board,
                new[] { secondPlaced, firstPlaced },
                new[] { character },
                new[] { firstEnemy, secondEnemy });

            Assert.That(result.Pieces.Select(item => item.PlacedPiece.Piece.PieceId.Value),
                Is.EqualTo(new[] { "second", "first" }));
            Assert.That(result.Pieces.Select(item => item.PlacementOrder), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(result.Pieces.Select(item => item.TotalAppliedDamage), Is.EqualTo(new[] { 8, 4 }));
        }

        [Test]
        public void Calculator_ClampsAtDefenseBoundaryAndAppliesStunMultiplier()
        {
            var calculator = new PieceDamageCalculator();

            Assert.That(calculator.Calculate(2, 5, 10, 1m), Is.Zero);
            Assert.That(calculator.Calculate(2, 5, 11, 2m), Is.Zero);
            Assert.That(calculator.Calculate(3, 5, 2, 1.5m), Is.EqualTo(19));
        }

        [Test]
        public void Resolve_UsesActiveEnemyStunMultiplierAndCharacterAttackBonus()
        {
            GridShape range = GridShape.CreateRectangle(1, 1);
            EnemyUnit enemy = CreateEnemy("enemy", 0, defense: 2);
            new StunResolutionService().Resolve(
                new[] { enemy },
                range,
                new ResidualPieceField(range),
                new StunSettings(2, 1.5m));
            var field = new BattleField(range, new[] { enemy.Body });
            var board = new BattleBoard(range, field);
            BattlePiece piece = CreatePiece("piece", "hero", new[] { (new GridPosition(0, 0), 2) });
            PlacedPiece placed = Place(board, piece, 0);
            BattleCharacter character = CreateCharacter("hero", 3, piece);
            var modifiers = new CharacterCombatModifiers();
            modifiers.AddAttack(character.Id, 2);

            PieceDropAttackResult result = new PieceDropAttackService().Resolve(
                board, new[] { placed }, new[] { character }, new[] { enemy }, modifiers);

            // floor(((2 * (3 + 2)) - 2) * 1.5) = 12
            Assert.That(result.TotalAppliedDamage, Is.EqualTo(12));
        }

        private static EnemyUnit CreateEnemy(string id, int x, int defense)
        {
            var enemyId = new EnemyId(id);
            return new EnemyUnit(
                new EnemyBattleState(enemyId, new EnemyStats(100, 1, defense)),
                new EnemyFieldObject(enemyId, new[] { new GridPosition(x, 0) }));
        }

        private static BattlePiece CreatePiece(
            string id,
            string owner,
            IEnumerable<(GridPosition position, int power)> cells)
        {
            var values = cells.ToArray();
            return new BattlePiece(
                new PieceId(id),
                new CharacterId(owner),
                values.ToDictionary(cell => cell.position, cell => cell.power),
                values.ToDictionary(cell => cell.position, _ => new ElementAttribute("fire")));
        }

        private static BattleCharacter CreateCharacter(string id, int attack, params BattlePiece[] pieces)
        {
            var table = new CharacterLevelTable(new[]
            {
                new KeyValuePair<int, CharacterLevelStats>(1, new CharacterLevelStats(10, attack))
            });
            return new BattleCharacter(new CharacterId(id), 1, table, pieces);
        }

        private static PlacedPiece Place(BattleBoard board, BattlePiece piece, int x)
        {
            return new PiecePlacementService().TryPlace(
                board,
                new PiecePlacementRequest(piece, new GridPosition(x, 0))).PlacedPiece;
        }
    }
}
