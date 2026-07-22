using System;
using PuzzleBattle.Domain.Stun;

namespace PuzzleBattle.Domain.Enemy
{
    public sealed class EnemyUnit
    {
        public EnemyUnit(EnemyBattleState battleState, EnemyFieldObject body)
        {
            BattleState = battleState ?? throw new ArgumentNullException(nameof(battleState));
            Body = body ?? throw new ArgumentNullException(nameof(body));
            if (BattleState.Id != Body.EnemyId)
                throw new ArgumentException("Enemy state and body must have the same ID.", nameof(body));
            Stun = new EnemyStunState();
        }

        public EnemyId Id => BattleState.Id;
        public EnemyBattleState BattleState { get; }
        public EnemyFieldObject Body { get; }
        public EnemyStunState Stun { get; }
        public Grid.GridPosition Position => Body.Position;
    }
}
