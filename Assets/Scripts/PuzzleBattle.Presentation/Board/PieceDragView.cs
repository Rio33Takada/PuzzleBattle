using System;
using PuzzleBattle.Application.PlayerTurn;
using PuzzleBattle.Domain.Grid;
using UnityEngine;
using UnityEngine.EventSystems;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Presentation.Board
{
    public interface IPiecePlacementInput
    {
        bool TryPlace(BattlePiece piece, GridPosition translation);
    }

    public sealed class PlayerTurnPlacementInput : IPiecePlacementInput
    {
        private readonly PlayerTurnCommandUseCase useCase;

        public PlayerTurnPlacementInput(PlayerTurnCommandUseCase useCase)
        {
            this.useCase = useCase ?? throw new ArgumentNullException(nameof(useCase));
        }

        public bool TryPlace(BattlePiece piece, GridPosition translation)
        {
            try
            {
                return useCase.Place(piece, translation).Succeeded;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
    }

    public sealed class PieceDragView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform draggedVisual;
        [SerializeField] private RectTransform dragCoordinateArea;
        [SerializeField] private BoardGridView boardView;

        private BattlePiece piece;
        private IPiecePlacementInput placementInput;
        private Vector2 originalAnchoredPosition;
        private bool dragging;

        public void Bind(BattlePiece value, IPiecePlacementInput input)
        {
            piece = value;
            placementInput = input;
        }

        public void SetSceneReferences(
            RectTransform visual,
            RectTransform coordinateArea,
            BoardGridView gridView)
        {
            draggedVisual = visual;
            dragCoordinateArea = coordinateArea;
            boardView = gridView;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData == null || draggedVisual == null)
                return;
            originalAnchoredPosition = draggedVisual.anchoredPosition;
            dragging = true;
            UpdateVisual(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging || eventData == null)
                return;
            UpdateVisual(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!dragging)
                return;
            dragging = false;
            if (eventData == null || boardView == null || piece == null || placementInput == null ||
                !boardView.TryGetSnap(eventData.position, eventData.pressEventCamera, out GridSnapResult snap) ||
                !placementInput.TryPlace(piece, snap.GridPosition))
            {
                RestoreVisual();
                return;
            }

            if (draggedVisual != null && draggedVisual.parent is RectTransform parent)
            {
                Vector3 world = boardView.ToWorldPosition(snap.SnappedLocalPosition);
                draggedVisual.anchoredPosition = parent.InverseTransformPoint(world);
            }
        }

        private void UpdateVisual(PointerEventData eventData)
        {
            if (draggedVisual == null || dragCoordinateArea == null)
                return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    dragCoordinateArea, eventData.position, eventData.pressEventCamera, out Vector2 local))
            {
                draggedVisual.anchoredPosition = local;
            }
        }

        private void RestoreVisual()
        {
            if (draggedVisual != null)
                draggedVisual.anchoredPosition = originalAnchoredPosition;
        }
    }
}
