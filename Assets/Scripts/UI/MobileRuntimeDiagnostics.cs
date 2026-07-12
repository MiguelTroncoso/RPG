using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class MobileRuntimeDiagnostics : MonoBehaviour
    {
        public RectTransform SafeAreaRoot;
        public CombatFeedbackAudio Audio;
        public bool LogChanges = true;

        public string LastSummary { get; private set; } = string.Empty;

        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void Start()
        {
            Report();
        }

        private void Update()
        {
            if (lastSafeArea != Screen.safeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
            {
                Report();
            }
        }

        private void Report()
        {
            lastSafeArea = Screen.safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            var orientation = Screen.width >= Screen.height ? "landscape" : "portrait";
            var audioLabel = Audio != null
                ? $"audio sfx {Audio.SfxVolume:0.00} music {Audio.MusicVolume:0.00}"
                : "audio unavailable";
            LastSummary = $"{Screen.width}x{Screen.height} {orientation} safe {Screen.safeArea} {audioLabel}";

            if (LogChanges)
            {
                Debug.Log($"MobileRuntimeDiagnostics: {LastSummary}");
            }
        }
    }
}
