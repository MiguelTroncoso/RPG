using UnityEngine;

namespace MmorpgPrototype
{
    // Keeps modal windows inside the safe area on landscape, portrait and
    // small test resolutions without changing their readable layout.
    public sealed class ResponsivePanelScaler : MonoBehaviour
    {
        public Vector2 ReferenceSize;
        public float Padding = 24f;

        private RectTransform panel;
        private RectTransform parent;
        private Vector2Int lastScreenSize;

        private void Awake()
        {
            panel = GetComponent<RectTransform>();
            parent = transform.parent as RectTransform;
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Update()
        {
            if (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            if (panel == null || parent == null)
            {
                return;
            }

            var targetSize = ReferenceSize.sqrMagnitude > 0.01f ? ReferenceSize : panel.sizeDelta;
            var available = parent.rect.size - new Vector2(Padding, Padding);
            if (targetSize.x <= 0f || targetSize.y <= 0f || available.x <= 0f || available.y <= 0f)
            {
                return;
            }

            var scale = Mathf.Min(1f, available.x / targetSize.x, available.y / targetSize.y);
            panel.localScale = Vector3.one * Mathf.Clamp(scale, 0.55f, 1f);
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
