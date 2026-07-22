using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Board;
using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Field;
using PuzzleBattle.Domain.Placement;

namespace PuzzleBattle.Domain.DropAttack
{
    public enum PieceCellDropOutcome
    {
        Missed = 0,
        NonDamageableTarget = 1,
        DamagedEnemy = 2
    }

    public sealed class PieceCellDropResult
    {
        internal PieceCellDropResult(
            PlacedPieceCell cell,
            FieldObject hitObject,
            EnemyUnit enemy,
            PieceCellDropOutcome outcome,
            int calculatedDamage,
            int appliedDamage)
        {
            Cell = cell ?? throw new ArgumentNullException(nameof(cell));
            HitObject = hitObject;
            Enemy = enemy;
            Outcome = outcome;
            CalculatedDamage = calculatedDamage;
            AppliedDamage = appliedDamage;
        }

        public PlacedPieceCell Cell { get; }
        public FieldObject HitObject { get; }
        public EnemyUnit Enemy { get; }
        public PieceCellDropOutcome Outcome { get; }
        public int CalculatedDamage { get; }
        public int AppliedDamage { get; }
    }

    public sealed class PlacedPieceDropResult
    {
        internal PlacedPieceDropResult(int placementOrder, PlacedPiece placedPiece, IEnumerable<PieceCellDropResult> cells)
        {
            PlacementOrder = placementOrder;
            PlacedPiece = placedPiece ?? throw new ArgumentNullException(nameof(placedPiece));
            Cells = (cells ?? throw new ArgumentNullException(nameof(cells))).ToArray();
            TotalAppliedDamage = Cells.Sum(cell => cell.AppliedDamage);
        }

        public int PlacementOrder { get; }
        public PlacedPiece PlacedPiece { get; }
        public IReadOnlyList<PieceCellDropResult> Cells { get; }
        public int TotalAppliedDamage { get; }
    }

    public sealed class PieceDropAttackResult
    {
        internal PieceDropAttackResult(IEnumerable<PlacedPieceDropResult> pieces)
        {
            Pieces = (pieces ?? throw new ArgumentNullException(nameof(pieces))).ToArray();
            TotalAppliedDamage = Pieces.Sum(piece => piece.TotalAppliedDamage);
        }

        public IReadOnlyList<PlacedPieceDropResult> Pieces { get; }
        public int TotalAppliedDamage { get; }
    }
}
