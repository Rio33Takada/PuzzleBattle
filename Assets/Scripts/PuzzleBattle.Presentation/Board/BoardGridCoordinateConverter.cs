using System;
using PuzzleBattle.Domain.Grid;
using UnityEngine;

namespace PuzzleBattle.Presentation.Board
{
    public readonly struct GridSnapResult
    {
        public GridSnapResult(GridPosition gridPosition, Vector2 snappedLocalPosition)
        {
            GridPosition = gridPosition;
            SnappedLocalPosition = snappedLocalPosition;
        }

        public GridPosition GridPosition { get; }
        public Vector2 SnappedLocalPosition { get; }
    }

    public static class BoardGridCoordinateConverter
    {
        public static bool TryScreenToGrid(
            RectTransform boardArea,
            GridShape grid,
            Vector2 screenPosition,
            Camera eventCamera,
            out GridSnapResult result)
        {
            result = default;
            if (boardArea == null || grid == null)
                return false;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    boardArea, screenPosition, eventCamera, out Vector2 localPosition))
            {
                return false;
            }

            return TryLocalToGrid(boardArea.rect, grid, localPosition, out result);
        }

        public static bool TryLocalToGrid(
            Rect boardRect,
            GridShape grid,
            Vector2 localPosition,
            out GridSnapResult result)
        {
            result = default;
            if (grid == null || boardRect.width <= 0f || boardRect.height <= 0f)
                return false;
            if (localPosition.x < boardRect.xMin || localPosition.x >= boardRect.xMax ||
                localPosition.y < boardRect.yMin || localPosition.y >= boardRect.yMax)
            {
                return false;
            }

            float cellWidth = boardRect.width / grid.Width;
            float cellHeight = boardRect.height / grid.Height;
            int x = Mathf.FloorToInt((localPosition.x - boardRect.xMin) / cellWidth);
            int y = Mathf.FloorToInt((localPosition.y - boardRect.yMin) / cellHeight);
            var gridPosition = new GridPosition(x, y);
            if (!grid.IsValid(gridPosition))
                return false;

            var snapped = new Vector2(
                boardRect.xMin + (x + 0.5f) * cellWidth,
                boardRect.yMin + (y + 0.5f) * cellHeight);
            result = new GridSnapResult(gridPosition, snapped);
            return true;
        }
    }
}
