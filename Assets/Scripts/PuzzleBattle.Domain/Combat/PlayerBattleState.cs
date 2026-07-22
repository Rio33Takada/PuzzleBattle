using System;
using System.Linq;
using PuzzleBattle.Domain.Character;

namespace PuzzleBattle.Domain.Combat
{
    public sealed class PlayerBattleState
    {
        public PlayerBattleState(PartyFormation formation)
        {
            Formation = formation ?? throw new ArgumentNullException(nameof(formation));
            if (Formation.Characters.Count == 0)
                throw new ArgumentException("A battle formation must contain at least one character.", nameof(formation));

            int maximumHitPoints = Formation.Characters.Aggregate(
                0,
                (total, character) => checked(total + character.Stats.BaseHitPoints));
            Health = new HealthState(maximumHitPoints);
        }

        public PartyFormation Formation { get; }
        public HealthState Health { get; }
        public int MaximumHitPoints => Health.MaximumHitPoints;
        public int CurrentHitPoints => Health.CurrentHitPoints;
        public bool IsDefeated => Health.IsDead;

        public HealthChangeResult ReceiveDamage(int damage)
        {
            return Health.ReceiveDamage(damage);
        }
    }
}
