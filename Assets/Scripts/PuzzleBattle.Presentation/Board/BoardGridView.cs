using PuzzleBattle.Domain.Grid;
using UnityEngine;

namespace PuzzleBattle.Presentation.Board
{
    public sealed class BoardGridView : MonoBehaviour
    {
        [SerializeField] private RectTransform boardArea;
        private GridShape grid;

        public RectTransform BoardArea => boardArea;
        public bool IsConfigured => boardArea != null && grid != null;

        public void Configure(GridShape value, RectTransform area = null)
        {
            grid = value;
            if (area != null)
                boardArea = area;
        }

        public bool TryGetSnap(
            Vector2 screenPosition,
            Camera eventCamera,
            out GridSnapResult result)
        {
            return BoardGridCoordinateConverter.TryScreenToGrid(
                boardArea, grid, screenPosition, eventCamera, out result);
        }

        public Vector3 ToWorldPosition(Vector2 boardLocalPosition)
        {
            return boardArea == null
                ? transform.position
                : boardArea.TransformPoint(boardLocalPosition);
        }

        public bool TryGetCellLocalPosition(GridPosition position, out Vector2 localPosition)
        {
            localPosition = default;
            if (boardArea == null || grid == null || !grid.IsValid(position))
                return false;
            Rect rect = boardArea.rect;
            if (rect.width <= 0f || rect.height <= 0f)
                return false;
            localPosition = new Vector2(
                rect.xMin + (position.X + 0.5f) * rect.width / grid.Width,
                rect.yMin + (position.Y + 0.5f) * rect.height / grid.Height);
            return true;
        }
    }
}
