using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Residual;

namespace PuzzleBattle.Domain.Enemy
{
    public enum EnemyMovementOutcome
    {
        Moved = 0,
        NoDestination = 1,
        StoppedByResidualPiece = 2,
        DeadEnemy = 3,
        Stunned = 4
    }

    public sealed class EnemyMovementResult
    {
        internal EnemyMovementResult(
            EnemyUnit enemy,
            GridPosition origin,
            GridPosition? plannedDestination,
            EnemyMovementOutcome outcome,
            ResidualContactResult residualContact)
        {
            Enemy = enemy;
            Origin = origin;
            PlannedDestination = plannedDestination;
            Outcome = outcome;
            ResidualContact = residualContact;
        }

        public EnemyUnit Enemy { get; }
        public GridPosition Origin { get; }
        public GridPosition? PlannedDestination { get; }
        public GridPosition FinalPosition => Enemy.Position;
        public EnemyMovementOutcome Outcome { get; }
        public ResidualContactResult ResidualContact { get; }
        public bool Moved => Outcome == EnemyMovementOutcome.Moved;
        public int PlayerDamage => ResidualContact?.PlayerDamage ?? 0;
    }

    public sealed class EnemyMovementBatchResult
    {
        internal EnemyMovementBatchResult(IEnumerable<EnemyMovementResult> results)
        {
            Results = results.ToArray();
            TotalPlayerDamage = Results.Sum(result => result.PlayerDamage);
        }

        public IReadOnlyList<EnemyMovementResult> Results { get; }
        public int TotalPlayerDamage { get; }
    }
}
