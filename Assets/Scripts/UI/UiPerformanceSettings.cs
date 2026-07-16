using UnityEngine;

namespace MmorpgPrototype
{
    public enum UiPerformanceProfile
    {
        Performance,
        Quality
    }

    // Device-level preference. It is intentionally separate from the player
    // save so changing phones or characters does not change game progress.
    public static class UiPerformanceSettings
    {
        private const string PreferenceKey = "mmorpg.ui.performance_profile";

        public static UiPerformanceProfile Current { get; private set; } = UiPerformanceProfile.Performance;

        public static void LoadAndApply()
        {
            var fallback = Application.platform == RuntimePlatform.Android
                ? UiPerformanceProfile.Performance
                : UiPerformanceProfile.Quality;
            var saved = PlayerPrefs.GetInt(PreferenceKey, (int)fallback);
            SetProfile(saved == (int)UiPerformanceProfile.Quality
                ? UiPerformanceProfile.Quality
                : UiPerformanceProfile.Performance, save: false);
        }

        public static void SetProfile(UiPerformanceProfile profile)
        {
            SetProfile(profile, save: true);
        }

        public static string Label => Current == UiPerformanceProfile.Quality ? "CALIDAD" : "RENDIMIENTO";

        private static void SetProfile(UiPerformanceProfile profile, bool save)
        {
            Current = profile;
            if (save)
            {
                PlayerPrefs.SetInt(PreferenceKey, (int)profile);
                PlayerPrefs.Save();
            }

            Application.targetFrameRate = profile == UiPerformanceProfile.Quality ? 60 : 45;
            QualitySettings.shadows = profile == UiPerformanceProfile.Quality
                ? ShadowQuality.HardOnly
                : ShadowQuality.Disable;
            QualitySettings.shadowDistance = profile == UiPerformanceProfile.Quality ? 34f : 18f;
            QualitySettings.pixelLightCount = profile == UiPerformanceProfile.Quality ? 2 : 1;
            QualitySettings.antiAliasing = profile == UiPerformanceProfile.Quality ? 2 : 0;
            QualitySettings.anisotropicFiltering = profile == UiPerformanceProfile.Quality
                ? AnisotropicFiltering.Enable
                : AnisotropicFiltering.Disable;
            QualitySettings.realtimeReflectionProbes = profile == UiPerformanceProfile.Quality;
        }
    }
}
