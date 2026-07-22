using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.DropAttack;
using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Field;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;
using PuzzleBattle.Domain.Placement;
using PuzzleBattle.Domain.Residual;
using PuzzleBattle.Presentation.Battle;
using PuzzleBattle.Presentation.Board;
using UnityEngine;
using UnityEngine.TestTools;
using BattleBoard = PuzzleBattle.Domain.Board.Board;
using BattleCharacter = PuzzleBattle.Domain.Character.Character;
using BattleField = PuzzleBattle.Domain.Field.Field;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.PlayMode.Tests
{
    public sealed class BattlePresentationPlayModeTests
    {
        [Test]
        public void ForecastView_UpdatesAndKeepsPlannedMarkerVisibleUntilNextForecast()
        {
            var canvasObject = new GameObject("canvas", typeof(RectTransform), typeof(Canvas));
            var boardObject = new GameObject("board", typeof(RectTransform), typeof(BoardGridView));
            var markerObject = new GameObject("marker", typeof(RectTransform));
            var forecastObject = new GameObject("forecast", typeof(EnemyMovementForecastView));
            try
            {
                canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                RectTransform canvas = canvasObject.GetComponent<RectTransform>();
                RectTransform boardRect = boardObject.GetComponent<RectTransform>();
                boardRect.SetParent(canvas, false);
                boardRect.sizeDelta = new Vector2(200f, 100f);
                RectTransform marker = markerObject.GetComponent<RectTransform>();
                marker.SetParent(boardRect, false);
                GridShape range = GridShape.CreateRectangle(2, 1);
                BoardGridView boardView = boardObject.GetComponent<BoardGridView>();
                boardView.Configure(range, boardRect);
                EnemyUnit enemy = CreateEnemy("enemy", 0);
                var movement = new EnemyMovementService();
                var residuals = new ResidualPieceField(range);
                var priority = new FixedDirectionPriority(new[] { GridDirection.Right });
                var view = forecastObject.GetComponent<EnemyMovementForecastView>();
                view.SetBoardView(boardView);
                view.SetMarkers(new[] { new EnemyForecastMarkerBinding("enemy", marker) });

                view.Show(movement.Forecast(new[] { enemy }, range, residuals, priority));
                Assert.That(marker.gameObject.activeSelf, Is.True);
                Assert.That(marker.anchoredPosition.x, Is.EqualTo(50f).Within(0.01f));

                enemy.BattleState.ReceiveDamage(enemy.BattleState.CurrentHitPoints);
                view.Show(movement.Forecast(new[] { enemy }, range, residuals, priority));
                Assert.That(marker.gameObject.activeSelf, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(canvasObject);
                Object.DestroyImmediate(boardObject);
                Object.DestroyImmediate(markerObject);
                Object.DestroyImmediate(forecastObject);
            }
        }

        [UnityTest]
        public IEnumerator DropSequence_UsesPlacementOrderShowsPerEnemyPieceTotalsAndDoesNotChangeBattleResult()
        {
            PieceDropAttackResult result = CreateDropResult(out EnemyUnit firstEnemy, out EnemyUnit secondEnemy);
            int firstHpAfterResolution = firstEnemy.BattleState.CurrentHitPoints;
            int secondHpAfterResolution = secondEnemy.BattleState.CurrentHitPoints;
            var visual = new RecordingDropVisual();
            var damage = new RecordingDamageDisplay();
            var viewObject = new GameObject("drop-sequence", typeof(PieceDropSequenceView));
            try
            {
                var view = viewObject.GetComponent<PieceDropSequenceView>();
                view.Bind(visual, damage);
                view.SetSecondsPerPiece(0f);

                yield return view.PlaySequence(result);

                Assert.That(visual.Orders, Is.EqualTo(new[] { 0 }));
                Assert.That(damage.Values, Is.EqualTo(new[] { "a:6", "b:6" }));
                Assert.That(firstEnemy.BattleState.CurrentHitPoints, Is.EqualTo(firstHpAfterResolution));
                Assert.That(secondEnemy.BattleState.CurrentHitPoints, Is.EqualTo(secondHpAfterResolution));
                Assert.That(result.TotalAppliedDamage, Is.EqualTo(12));
            }
            finally
            {
                Object.DestroyImmediate(viewObject);
            }
        }

        [Test]
        public void PresentationViews_HandleMissingReferencesAndNullResults()
        {
            var forecastObject = new GameObject("forecast", typeof(EnemyMovementForecastView));
            var dropObject = new GameObject("drop", typeof(PieceDropSequenceView));
            try
            {
                Assert.DoesNotThrow(() => forecastObject.GetComponent<EnemyMovementForecastView>().Show(null));
                Assert.That(dropObject.GetComponent<PieceDropSequenceView>().Play(null), Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(forecastObject);
                Object.DestroyImmediate(dropObject);
            }
        }

        private static PieceDropAttackResult CreateDropResult(
            out EnemyUnit firstEnemy,
            out EnemyUnit secondEnemy)
        {
            GridShape range = GridShape.CreateRectangle(2, 1);
            firstEnemy = CreateEnemy("a", 0);
            secondEnemy = CreateEnemy("b", 1);
            var field = new BattleField(range, new FieldObject[] { firstEnemy.Body, secondEnemy.Body });
            var board = new BattleBoard(range, field);
            var owner = new CharacterId("hero");
            GridPosition[] cells = { new GridPosition(0, 0), new GridPosition(1, 0) };
            var piece = new BattlePiece(
                new PieceId("piece"), owner,
                cells.ToDictionary(cell => cell, _ => 2),
                cells.ToDictionary(cell => cell, _ => new ElementAttribute("fire")));
            PlacedPiece placed = new PiecePlacementService().TryPlace(
                board, new PiecePlacementRequest(piece, new GridPosition(0, 0))).PlacedPiece;
            var levels = new CharacterLevelTable(new[]
            {
                new KeyValuePair<int, CharacterLevelStats>(1, new CharacterLevelStats(10, 3))
            });
            var character = new BattleCharacter(owner, 1, levels, new[] { piece });
            return new PieceDropAttackService().Resolve(
                board, new[] { placed }, new[] { character }, new[] { firstEnemy, secondEnemy });
        }

        private static EnemyUnit CreateEnemy(string id, int x)
        {
            var enemyId = new EnemyId(id);
            return new EnemyUnit(
                new EnemyBattleState(enemyId, new EnemyStats(20, 1, 0)),
                new EnemyFieldObject(enemyId, new[] { new GridPosition(x, 0) }));
        }

        private sealed class RecordingDropVisual : IPieceDropVisual
        {
            public List<int> Orders { get; } = new List<int>();
            public void ShowDrop(PieceDropPresentationItem piece) => Orders.Add(piece.Source.PlacementOrder);
        }

        private sealed class RecordingDamageDisplay : IEnemyDamageDisplay
        {
            public List<string> Values { get; } = new List<string>();
            public void ShowDamage(EnemyId enemyId, int totalDamage) =>
                Values.Add(enemyId.Value + ":" + totalDamage);
        }
    }
}
