using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.DropAttack;
using PuzzleBattle.Domain.Enemy;
using UnityEngine;

namespace PuzzleBattle.Presentation.Battle
{
    public readonly struct EnemyPieceDamagePresentation
    {
        public EnemyPieceDamagePresentation(EnemyId enemyId, int totalDamage)
        {
            EnemyId = enemyId;
            TotalDamage = totalDamage;
        }
        public EnemyId EnemyId { get; }
        public int TotalDamage { get; }
    }

    public sealed class PieceDropPresentationItem
    {
        internal PieceDropPresentationItem(
            PlacedPieceDropResult source,
            IEnumerable<EnemyPieceDamagePresentation> enemyDamages)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            EnemyDamages = enemyDamages.ToArray();
        }
        public PlacedPieceDropResult Source { get; }
        public IReadOnlyList<EnemyPieceDamagePresentation> EnemyDamages { get; }
    }

    public static class PieceDropPresentationProjector
    {
        public static IReadOnlyList<PieceDropPresentationItem> Project(PieceDropAttackResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            return result.Pieces
                .OrderBy(piece => piece.PlacementOrder)
                .Select(piece => new PieceDropPresentationItem(
                    piece,
                    piece.Cells
                        .Where(cell => cell.Enemy != null && cell.AppliedDamage > 0)
                        .GroupBy(cell => cell.Enemy.Id)
                        .OrderBy(group => group.Key.Value, StringComparer.Ordinal)
                        .Select(group => new EnemyPieceDamagePresentation(
                            group.Key,
                            group.Aggregate(0, (total, cell) => checked(total + cell.AppliedDamage))))))
                .ToArray();
        }
    }

    public interface IPieceDropVisual
    {
        void ShowDrop(PieceDropPresentationItem piece);
    }

    public interface IEnemyDamageDisplay
    {
        void ShowDamage(EnemyId enemyId, int totalDamage);
    }

    public sealed class PieceDropSequenceView : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour dropVisualBehaviour = null;
        [SerializeField] private MonoBehaviour damageDisplayBehaviour = null;
        [SerializeField, Min(0f)] private float secondsPerPiece = 0.25f;
        private IPieceDropVisual dropVisual;
        private IEnemyDamageDisplay damageDisplay;
        private Coroutine running;

        public event Action SequenceCompleted;

        private void Awake()
        {
            dropVisual = dropVisualBehaviour as IPieceDropVisual;
            damageDisplay = damageDisplayBehaviour as IEnemyDamageDisplay;
        }

        public void Bind(IPieceDropVisual visual, IEnemyDamageDisplay display)
        {
            dropVisual = visual;
            damageDisplay = display;
        }

        public void SetSecondsPerPiece(float value)
        {
            secondsPerPiece = Mathf.Max(0f, value);
        }

        public Coroutine Play(PieceDropAttackResult result)
        {
            if (result == null || !isActiveAndEnabled)
                return null;
            if (running != null)
                StopCoroutine(running);
            running = StartCoroutine(PlaySequence(result));
            return running;
        }

        public IEnumerator PlaySequence(PieceDropAttackResult result)
        {
            if (result == null)
                yield break;
            IReadOnlyList<PieceDropPresentationItem> sequence = PieceDropPresentationProjector.Project(result);
            foreach (PieceDropPresentationItem item in sequence)
            {
                dropVisual?.ShowDrop(item);
                if (secondsPerPiece > 0f)
                    yield return new WaitForSeconds(secondsPerPiece);
                else
                    yield return null;

                foreach (EnemyPieceDamagePresentation damage in item.EnemyDamages)
                    damageDisplay?.ShowDamage(damage.EnemyId, damage.TotalDamage);
            }
            running = null;
            SequenceCompleted?.Invoke();
        }
    }
}
