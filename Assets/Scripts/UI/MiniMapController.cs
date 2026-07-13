using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    // Lightweight world radar. It samples active objects instead of rendering
    // a second camera, keeping the Android cost predictable.
    public sealed class MiniMapController : MonoBehaviour
    {
        public Transform Target;
        public RectTransform MapRect;
        public RectTransform PlayerMarker;
        public float WorldRadius = 48f;
        public float RefreshInterval = 0.16f;

        private readonly List<Image> enemyMarkers = new List<Image>();
        private readonly List<Image> poiMarkers = new List<Image>();
        private float nextRefresh;

        private void Update()
        {
            if (Target == null || MapRect == null || Time.unscaledTime < nextRefresh)
            {
                return;
            }

            nextRefresh = Time.unscaledTime + Mathf.Max(0.05f, RefreshInterval);
            if (PlayerMarker != null)
            {
                PlayerMarker.anchoredPosition = Vector2.zero;
                PlayerMarker.SetAsLastSibling();
            }

            var enemyIndex = 0;
            var enemies = FindObjectsByType<EnemyAI>();
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.GetComponent<Health>()?.IsDead == true || !IsWithinRange(enemy.transform.position))
                {
                    continue;
                }

                var reward = enemy.GetComponent<EnemyReward>();
                var tier = reward != null ? reward.Tier : EnemyTier.Normal;
                var color = tier == EnemyTier.Boss
                    ? new Color(1f, 0.36f, 0.12f)
                    : tier == EnemyTier.Elite
                        ? new Color(0.72f, 0.32f, 1f)
                        : new Color(0.95f, 0.24f, 0.2f);
                var marker = MarkerFor(enemyMarkers, ref enemyIndex, color, 12f);
                marker.rectTransform.anchoredPosition = PositionFor(enemy.transform.position);
            }

            HideMarkers(enemyMarkers, enemyIndex);

            var poiIndex = 0;
            var points = FindObjectsByType<ZonePointOfInterest>();
            foreach (var point in points)
            {
                if (point == null || !IsWithinRange(point.transform.position))
                {
                    continue;
                }

                var color = point.Type == ZonePointOfInterestType.SafeCommerce
                    ? new Color(0.2f, 0.85f, 0.55f)
                    : point.Type == ZonePointOfInterestType.Boss
                        ? new Color(1f, 0.68f, 0.18f)
                        : new Color(0.95f, 0.85f, 0.3f);
                var marker = MarkerFor(poiMarkers, ref poiIndex, color, 8f);
                marker.rectTransform.anchoredPosition = PositionFor(point.transform.position);
            }

            HideMarkers(poiMarkers, poiIndex);
        }

        private Vector2 PositionFor(Vector3 worldPosition)
        {
            var offset = new Vector2(worldPosition.x - Target.position.x, worldPosition.z - Target.position.z);
            var halfSize = Mathf.Min(MapRect.rect.width, MapRect.rect.height) * 0.5f - 8f;
            return Vector2.ClampMagnitude(offset / Mathf.Max(1f, WorldRadius) * halfSize, halfSize);
        }

        private bool IsWithinRange(Vector3 worldPosition)
        {
            var offset = worldPosition - Target.position;
            offset.y = 0f;
            return offset.sqrMagnitude <= WorldRadius * WorldRadius;
        }

        private Image MarkerFor(List<Image> pool, ref int index, Color color, float size)
        {
            if (index >= pool.Count)
            {
                var markerObject = new GameObject("Minimap Marker", typeof(RectTransform));
                markerObject.transform.SetParent(MapRect, false);
                var markerImage = markerObject.AddComponent<Image>();
                markerImage.raycastTarget = false;
                pool.Add(markerImage);
            }

            var marker = pool[index++];
            marker.gameObject.SetActive(true);
            marker.color = color;
            marker.rectTransform.sizeDelta = new Vector2(size, size);
            return marker;
        }

        private static void HideMarkers(List<Image> pool, int used)
        {
            for (var i = used; i < pool.Count; i++)
            {
                pool[i].gameObject.SetActive(false);
            }
        }
    }
}
