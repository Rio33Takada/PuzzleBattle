using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Board;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Combat;
using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Field;
using PuzzleBattle.Domain.Placement;
using PuzzleBattle.Domain.Skill;
using BattleBoard = PuzzleBattle.Domain.Board.Board;
using BattleCharacter = PuzzleBattle.Domain.Character.Character;
using BattleField = PuzzleBattle.Domain.Field.Field;

namespace PuzzleBattle.Domain.DropAttack
{
    public sealed class PieceDropAttackService
    {
        private readonly IPieceDamageCalculator damageCalculator;

        public PieceDropAttackService(IPieceDamageCalculator damageCalculator = null)
        {
            this.damageCalculator = damageCalculator ?? new PieceDamageCalculator();
        }

        public PieceDropAttackResult Resolve(
            BattleBoard board,
            IEnumerable<PlacedPiece> placedPiecesInOrder,
            IEnumerable<BattleCharacter> characters,
            IEnumerable<EnemyUnit> enemies,
            CharacterCombatModifiers modifiers = null)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));
            if (placedPiecesInOrder == null)
                throw new ArgumentNullException(nameof(placedPiecesInOrder));

            Dictionary<CharacterId, BattleCharacter> characterById = IndexCharacters(characters);
            Dictionary<EnemyId, EnemyUnit> enemyById = IndexEnemies(enemies);
            var results = new List<PlacedPieceDropResult>();
            int placementOrder = 0;

            foreach (PlacedPiece placedPiece in placedPiecesInOrder)
            {
                if (placedPiece == null)
                    throw new ArgumentException("Placed pieces cannot contain null.", nameof(placedPiecesInOrder));
                if (!characterById.TryGetValue(placedPiece.Piece.OwnerId, out BattleCharacter owner))
                    throw new ArgumentException("Every placed piece must have a matching character.", nameof(characters));

                int ownerAttack = checked(owner.Stats.BaseAttack +
                    (modifiers?.GetAttackBonus(owner.Id) ?? 0));
                var cellResults = new List<PieceCellDropResult>();
                foreach (PlacedPieceCell cell in placedPiece.RemainingCells
                             .OrderBy(item => item.SourcePosition.Y)
                             .ThenBy(item => item.SourcePosition.X))
                {
                    FieldObject hitObject = FindFieldObject(board.ActiveField, cell.BoardPosition);
                    if (hitObject == null)
                    {
                        cellResults.Add(new PieceCellDropResult(
                            cell, null, null, PieceCellDropOutcome.Missed, 0, 0));
                        continue;
                    }

                    if (!(hitObject is EnemyFieldObject enemyBody) ||
                        !enemyById.TryGetValue(enemyBody.EnemyId, out EnemyUnit enemy) ||
                        !ReferenceEquals(enemy.Body, enemyBody))
                    {
                        cellResults.Add(new PieceCellDropResult(
                            cell, hitObject, null, PieceCellDropOutcome.NonDamageableTarget, 0, 0));
                        continue;
                    }

                    int calculatedDamage = damageCalculator.Calculate(
                        cell.Power,
                        ownerAttack,
                        enemy.BattleState.Defense,
                        enemy.Stun.DamageTakenMultiplier);
                    HealthChangeResult healthChange = enemy.BattleState.ReceiveDamage(calculatedDamage);
                    cellResults.Add(new PieceCellDropResult(
                        cell,
                        hitObject,
                        enemy,
                        PieceCellDropOutcome.DamagedEnemy,
                        calculatedDamage,
                        healthChange.AppliedDamage));
                }

                results.Add(new PlacedPieceDropResult(placementOrder, placedPiece, cellResults));
                placementOrder++;
            }

            return new PieceDropAttackResult(results);
        }

        private static FieldObject FindFieldObject(BattleField field, Grid.GridPosition position)
        {
            return field.Objects.FirstOrDefault(fieldObject =>
                fieldObject.Cells.Any(cell => cell.Position == position));
        }

        private static Dictionary<CharacterId, BattleCharacter> IndexCharacters(
            IEnumerable<BattleCharacter> characters)
        {
            if (characters == null)
                throw new ArgumentNullException(nameof(characters));
            var result = new Dictionary<CharacterId, BattleCharacter>();
            foreach (BattleCharacter character in characters)
            {
                if (character == null || result.ContainsKey(character.Id))
                    throw new ArgumentException("Characters must be non-null and have unique IDs.", nameof(characters));
                result.Add(character.Id, character);
            }
            return result;
        }

        private static Dictionary<EnemyId, EnemyUnit> IndexEnemies(IEnumerable<EnemyUnit> enemies)
        {
            if (enemies == null)
                throw new ArgumentNullException(nameof(enemies));
            var result = new Dictionary<EnemyId, EnemyUnit>();
            foreach (EnemyUnit enemy in enemies)
            {
                if (enemy == null || result.ContainsKey(enemy.Id))
                    throw new ArgumentException("Enemies must be non-null and have unique IDs.", nameof(enemies));
                result.Add(enemy.Id, enemy);
            }
            return result;
        }
    }
}
