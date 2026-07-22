using System;
using System.Collections.Generic;
using PuzzleBattle.Domain.Combat;
using PuzzleBattle.Domain.Enemy;

namespace PuzzleBattle.Domain.Skill
{
    public sealed class SkillExecutionContext
    {
        public SkillExecutionContext(
            int turnNumber,
            TurnOverwriteSummary overwrites,
            PlayerBattleState player,
            IEnumerable<EnemyBattleState> enemies,
            CharacterCombatModifiers modifiers,
            GrantedPiecePool grantedPieces)
        {
            if (turnNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(turnNumber), turnNumber, "Turn number must be positive.");
            TurnNumber = turnNumber;
            Overwrites = overwrites ?? throw new ArgumentNullException(nameof(overwrites));
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Modifiers = modifiers ?? throw new ArgumentNullException(nameof(modifiers));
            GrantedPieces = grantedPieces ?? throw new ArgumentNullException(nameof(grantedPieces));
            var enemyById = new Dictionary<EnemyId, EnemyBattleState>();
            foreach (EnemyBattleState enemy in enemies ?? throw new ArgumentNullException(nameof(enemies)))
            {
                if (enemy == null || enemyById.ContainsKey(enemy.Id))
                    throw new ArgumentException("Enemies must be non-null and have unique IDs.", nameof(enemies));
                enemyById.Add(enemy.Id, enemy);
            }
            Enemies = enemyById;
        }

        public int TurnNumber { get; }
        public TurnOverwriteSummary Overwrites { get; }
        public PlayerBattleState Player { get; }
        public IReadOnlyDictionary<EnemyId, EnemyBattleState> Enemies { get; }
        public CharacterCombatModifiers Modifiers { get; }
        public GrantedPiecePool GrantedPieces { get; }
    }
}
