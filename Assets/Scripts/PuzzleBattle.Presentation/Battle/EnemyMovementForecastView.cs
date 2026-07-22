using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Presentation.Board;
using UnityEngine;

namespace PuzzleBattle.Presentation.Battle
{
    [Serializable]
    public sealed class EnemyForecastMarkerBinding
    {
        [SerializeField] private string enemyId;
        [SerializeField] private RectTransform marker;
        public string EnemyId => enemyId;
        public RectTransform Marker => marker;

        public EnemyForecastMarkerBinding() { }

        public EnemyForecastMarkerBinding(string enemyId, RectTransform marker)
        {
            this.enemyId = enemyId;
            this.marker = marker;
        }
    }

    public sealed class EnemyMovementForecastView : MonoBehaviour
    {
        [SerializeField] private BoardGridView boardView;
        [SerializeField] private List<EnemyForecastMarkerBinding> markers = new List<EnemyForecastMarkerBinding>();

        public void SetBoardView(BoardGridView value) => boardView = value;
        public void SetMarkers(IEnumerable<EnemyForecastMarkerBinding> value) =>
            markers = value == null ? new List<EnemyForecastMarkerBinding>() : value.ToList();

        public void Show(EnemyMovementForecastBatch batch)
        {
            foreach (EnemyForecastMarkerBinding binding in markers)
            {
                if (binding?.Marker != null)
                    binding.Marker.gameObject.SetActive(false);
            }
            if (batch == null || boardView == null || boardView.BoardArea == null)
                return;

            foreach (EnemyMovementForecast forecast in batch.Forecasts)
            {
                if (!forecast.Destination.HasValue)
                    continue;
                EnemyForecastMarkerBinding binding = markers.FirstOrDefault(item =>
                    item != null && string.Equals(item.EnemyId, forecast.Enemy.Id.Value, StringComparison.Ordinal));
                if (binding?.Marker == null ||
                    !boardView.TryGetCellLocalPosition(forecast.Destination.Value, out Vector2 local))
                {
                    continue;
                }

                RectTransform marker = binding.Marker;
                if (marker.parent is RectTransform parent)
                    marker.anchoredPosition = parent.InverseTransformPoint(boardView.ToWorldPosition(local));
                marker.gameObject.SetActive(true);
            }
        }
    }
}
