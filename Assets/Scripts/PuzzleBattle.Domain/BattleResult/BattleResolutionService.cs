using System;
using System.Collections.Generic;
using PuzzleBattle.Domain.Combat;
using PuzzleBattle.Domain.Enemy;

namespace PuzzleBattle.Domain.BattleResult
{
    /// <summary>
    /// Evaluates battle and area completion without mutating participant or area state.
    /// </summary>
    public sealed class BattleResolutionService
    {
        public BattleResolution Evaluate(
            PlayerBattleState player,
            IEnumerable<EnemyBattleState> areaEnemies,
            BattleAreaState currentArea)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));
            if (areaEnemies == null)
                throw new ArgumentNullException(nameof(areaEnemies));

            bool allEnemiesDefeated = true;
            var enemyIds = new HashSet<EnemyId>();
            foreach (EnemyBattleState enemy in areaEnemies)
            {
                if (enemy == null)
                    throw new ArgumentException("Area enemies cannot contain null.", nameof(areaEnemies));
                if (!enemyIds.Add(enemy.Id))
                    throw new ArgumentException("Area enemy IDs must be unique.", nameof(areaEnemies));
                if (!enemy.IsDead)
                    allEnemiesDefeated = false;
            }

            // Defeat has priority when player and final enemy die in the same resolution step.
            if (player.IsDefeated)
            {
                return new BattleResolution(
                    BattleResolutionOutcome.Defeat,
                    currentArea,
                    null);
            }

            if (!allEnemiesDefeated)
            {
                return new BattleResolution(
                    BattleResolutionOutcome.Continuing,
                    currentArea,
                    null);
            }

            if (currentArea.IsFinalArea)
            {
                return new BattleResolution(
                    BattleResolutionOutcome.Victory,
                    currentArea,
                    null);
            }

            return new BattleResolution(
                BattleResolutionOutcome.AreaCleared,
                currentArea,
                currentArea.Advance());
        }
    }
}
