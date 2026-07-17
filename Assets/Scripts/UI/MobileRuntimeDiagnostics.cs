using System;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

namespace MmorpgPrototype
{
    public sealed class MobileRuntimeDiagnostics : MonoBehaviour
    {
        public RectTransform SafeAreaRoot;
        public CombatFeedbackAudio Audio;
        public bool LogChanges = true;
        public bool ApplyRuntimeTuning = true;

        public string LastSummary { get; private set; } = string.Empty;
        public float CurrentFps { get; private set; }
        public float AverageFps { get; private set; }
        public float MinimumFps { get; private set; }
        public int ActiveEnemyCount { get; private set; }
        public int ActivePointOfInterestCount { get; private set; }
        public string LastExportPath { get; private set; } = string.Empty;

        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;
        private float sampleElapsed;
        private float frameSum;
        private float frameMinimum = float.MaxValue;
        private int frameCount;
        private float nextWorldSample;
        private float sessionStartedAt;

        private void Awake()
        {
            sessionStartedAt = Time.realtimeSinceStartup;
        }

        private void Start()
        {
            if (ApplyRuntimeTuning && Application.platform == RuntimePlatform.Android)
            {
                Application.targetFrameRate = 60;
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            Report();
        }

        private void Update()
        {
            SampleFrame(Time.unscaledDeltaTime);

            if (Time.unscaledTime >= nextWorldSample)
            {
                nextWorldSample = Time.unscaledTime + 0.5f;
                SampleWorldObjects();
            }

            if (lastSafeArea != Screen.safeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
            {
                Report();
            }
        }

        private void SampleFrame(float delta)
        {
            if (delta <= 0.0001f)
            {
                return;
            }

            CurrentFps = 1f / delta;
            frameSum += CurrentFps;
            frameMinimum = Mathf.Min(frameMinimum, CurrentFps);
            frameCount++;
            sampleElapsed += delta;

            if (sampleElapsed < 0.5f || frameCount <= 0)
            {
                return;
            }

            AverageFps = frameSum / frameCount;
            MinimumFps = frameMinimum;
            frameSum = 0f;
            frameMinimum = float.MaxValue;
            frameCount = 0;
            sampleElapsed = 0f;
            Report(false);
        }

        private void SampleWorldObjects()
        {
            ActiveEnemyCount = FindObjectsByType<EnemyAI>().Length;
            ActivePointOfInterestCount = FindObjectsByType<ZonePointOfInterest>().Length;
        }

        private void Report(bool log = true)
        {
            lastSafeArea = Screen.safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            var orientation = Screen.width >= Screen.height ? "landscape" : "portrait";
            var audioLabel = Audio != null
                ? $"audio sfx {Audio.SfxVolume:0.00} music {Audio.MusicVolume:0.00}"
                : "audio unavailable";
            var deviceLabel = Localization.Tr("mobile.device", SystemInfo.deviceModel, Screen.dpi, Application.targetFrameRate);
            var performanceLabel = Localization.Tr("mobile.performance",
                Time.realtimeSinceStartup - sessionStartedAt,
                CurrentFps,
                AverageFps,
                MinimumFps,
                Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f),
                ActiveEnemyCount,
                ActivePointOfInterestCount);
            LastSummary = $"{Screen.width}x{Screen.height} {orientation} safe {Screen.safeArea}\n{deviceLabel}\n{performanceLabel}\n{audioLabel}";

            if (LogChanges && log)
            {
                Debug.Log($"MobileRuntimeDiagnostics: {LastSummary}");
            }
        }

        public string ExportReport()
        {
            var exportDirectory = Path.Combine(Application.persistentDataPath, "qa");
            Directory.CreateDirectory(exportDirectory);

            var snapshot = new MobileDiagnosticsSnapshot
            {
                utc = DateTime.UtcNow.ToString("O"),
                device = SystemInfo.deviceModel,
                operatingSystem = SystemInfo.operatingSystem,
                screenWidth = Screen.width,
                screenHeight = Screen.height,
                orientation = Screen.width >= Screen.height ? "landscape" : "portrait",
                safeAreaX = Screen.safeArea.x,
                safeAreaY = Screen.safeArea.y,
                safeAreaWidth = Screen.safeArea.width,
                safeAreaHeight = Screen.safeArea.height,
                dpi = Screen.dpi,
                performanceProfile = UiPerformanceSettings.Label,
                targetFps = Application.targetFrameRate,
                sessionSeconds = Time.realtimeSinceStartup - sessionStartedAt,
                currentFps = CurrentFps,
                averageFps = AverageFps,
                minimumFps = MinimumFps,
                allocatedMemoryMb = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f),
                activeEnemies = ActiveEnemyCount,
                activePointOfInterest = ActivePointOfInterestCount,
                audioAvailable = Audio != null
            };

            var fileName = $"android-qa-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
            LastExportPath = Path.Combine(exportDirectory, fileName);
            File.WriteAllText(LastExportPath, JsonUtility.ToJson(snapshot, true));
            Debug.Log($"MobileRuntimeDiagnostics: QA report exported to {LastExportPath}");
            return LastExportPath;
        }

        [Serializable]
        private sealed class MobileDiagnosticsSnapshot
        {
            public string utc;
            public string device;
            public string operatingSystem;
            public int screenWidth;
            public int screenHeight;
            public string orientation;
            public float safeAreaX;
            public float safeAreaY;
            public float safeAreaWidth;
            public float safeAreaHeight;
            public float dpi;
            public string performanceProfile;
            public int targetFps;
            public float sessionSeconds;
            public float currentFps;
            public float averageFps;
            public float minimumFps;
            public float allocatedMemoryMb;
            public int activeEnemies;
            public int activePointOfInterest;
            public bool audioAvailable;
        }
    }
}
