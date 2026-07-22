using System.Collections.Generic;
using NUnit.Framework;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;
using PuzzleBattle.Presentation.Board;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.PlayMode.Tests
{
    public sealed class BoardDragPlayModeTests
    {
        [TestCase(320f, 180f)]
        [TestCase(1920f, 1080f)]
        public void CoordinateConversion_IsResolutionIndependent(float width, float height)
        {
            var rect = new Rect(-width * 0.5f, -height * 0.5f, width, height);
            var local = new Vector2(rect.xMin + width * 0.625f, rect.yMin + height * 0.375f);

            bool succeeded = BoardGridCoordinateConverter.TryLocalToGrid(
                rect, GridShape.CreateRectangle(4, 4), local, out GridSnapResult result);

            Assert.That(succeeded, Is.True);
            Assert.That(result.GridPosition, Is.EqualTo(new GridPosition(2, 1)));
            Assert.That(result.SnappedLocalPosition.x, Is.EqualTo(rect.xMin + width * 0.625f).Within(0.001f));
            Assert.That(result.SnappedLocalPosition.y, Is.EqualTo(rect.yMin + height * 0.375f).Within(0.001f));
        }

        [Test]
        public void CoordinateConversion_SnapsCellsAndRejectsOutsideOrInvalidCells()
        {
            var rect = new Rect(0f, 0f, 300f, 200f);
            var validCells = new[]
            {
                new GridPosition(0, 0), new GridPosition(1, 0),
                new GridPosition(0, 1)
            };
            var shape = new GridShape(3, 2, validCells);

            Assert.That(BoardGridCoordinateConverter.TryLocalToGrid(
                rect, shape, new Vector2(149f, 50f), out GridSnapResult snap), Is.True);
            Assert.That(snap.GridPosition, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(snap.SnappedLocalPosition, Is.EqualTo(new Vector2(150f, 50f)));
            Assert.That(BoardGridCoordinateConverter.TryLocalToGrid(
                rect, shape, new Vector2(250f, 150f), out _), Is.False);
            Assert.That(BoardGridCoordinateConverter.TryLocalToGrid(
                rect, shape, new Vector2(300f, 100f), out _), Is.False);
        }

        [Test]
        public void CameraController_ConfiguresOrthographicProjectionAndHandlesMissingReference()
        {
            var controllerObject = new GameObject("camera-controller");
            var cameraObject = new GameObject("board-camera");
            try
            {
                var controller = controllerObject.AddComponent<OrthographicBoardCameraController>();
                var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = false;

                Assert.That(controller.ConfigureProjection(), Is.False);
                controller.SetCamera(camera);
                Assert.That(controller.ConfigureProjection(), Is.True);
                Assert.That(camera.orthographic, Is.True);
                Assert.That(camera.orthographicSize, Is.GreaterThan(0f));
            }
            finally
            {
                Object.DestroyImmediate(controllerObject);
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void BoardAndDragViews_HandleMissingSceneReferences()
        {
            var gameObject = new GameObject("unconfigured-view");
            var eventSystemObject = new GameObject("event-system");
            try
            {
                var board = gameObject.AddComponent<BoardGridView>();
                var drag = gameObject.AddComponent<PieceDragView>();
                var eventSystem = eventSystemObject.AddComponent<EventSystem>();
                var pointer = new PointerEventData(eventSystem) { position = Vector2.zero };

                Assert.That(board.TryGetSnap(Vector2.zero, null, out _), Is.False);
                Assert.DoesNotThrow(() =>
                {
                    drag.OnBeginDrag(pointer);
                    drag.OnDrag(pointer);
                    drag.OnEndDrag(pointer);
                });
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
                Object.DestroyImmediate(eventSystemObject);
            }
        }

        [Test]
        public void DragEnd_ForwardsSnappedGridPositionWithoutPlacementRulesInView()
        {
            var canvasObject = new GameObject("canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var boardObject = new GameObject("board", typeof(RectTransform), typeof(BoardGridView));
            var visualObject = new GameObject("piece", typeof(RectTransform), typeof(PieceDragView));
            var eventSystemObject = new GameObject("event-system", typeof(EventSystem));
            try
            {
                Canvas canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
                RectTransform boardRect = boardObject.GetComponent<RectTransform>();
                boardRect.SetParent(canvasRect, false);
                boardRect.sizeDelta = new Vector2(200f, 200f);
                RectTransform visual = visualObject.GetComponent<RectTransform>();
                visual.SetParent(canvasRect, false);
                var boardView = boardObject.GetComponent<BoardGridView>();
                boardView.Configure(GridShape.CreateRectangle(2, 2), boardRect);
                var input = new RecordingPlacementInput();
                var drag = visualObject.GetComponent<PieceDragView>();
                drag.SetSceneReferences(visual, canvasRect, boardView);
                drag.Bind(CreatePiece(), input);
                Canvas.ForceUpdateCanvases();

                Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, boardRect.TransformPoint(new Vector2(50f, 50f)));
                var pointer = new PointerEventData(eventSystemObject.GetComponent<EventSystem>()) { position = screen };
                drag.OnBeginDrag(pointer);
                drag.OnEndDrag(pointer);

                Assert.That(input.CallCount, Is.EqualTo(1));
                Assert.That(input.LastTranslation, Is.EqualTo(new GridPosition(1, 1)));
            }
            finally
            {
                Object.DestroyImmediate(canvasObject);
                Object.DestroyImmediate(boardObject);
                Object.DestroyImmediate(visualObject);
                Object.DestroyImmediate(eventSystemObject);
            }
        }

        private static BattlePiece CreatePiece()
        {
            var cell = new GridPosition(0, 0);
            return new BattlePiece(
                new PieceId("piece"), new CharacterId("hero"),
                new Dictionary<GridPosition, int> { [cell] = 1 },
                new Dictionary<GridPosition, ElementAttribute>
                {
                    [cell] = new ElementAttribute("fire")
                });
        }

        private sealed class RecordingPlacementInput : IPiecePlacementInput
        {
            public int CallCount { get; private set; }
            public GridPosition LastTranslation { get; private set; }
            public bool TryPlace(BattlePiece piece, GridPosition translation)
            {
                CallCount++;
                LastTranslation = translation;
                return true;
            }
        }
    }
}
