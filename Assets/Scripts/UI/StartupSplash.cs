using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class StartupSplash : MonoBehaviour
    {
        public CanvasGroup Group;
        public float HoldSeconds = 1.15f;
        public float FadeSeconds = 0.85f;

        private float startedAt;

        private void Awake()
        {
            startedAt = Time.unscaledTime;
        }

        private void Update()
        {
            if (Group == null)
            {
                Destroy(gameObject);
                return;
            }

            var elapsed = Time.unscaledTime - startedAt;
            if (elapsed < HoldSeconds)
            {
                return;
            }

            var fade = Mathf.Clamp01((elapsed - HoldSeconds) / FadeSeconds);
            Group.alpha = 1f - fade;

            if (fade >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
