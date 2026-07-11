#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MmorpgPrototype.Editor
{
    public static class AndroidBuildTools
    {
        private const string ProductName = "Valle de las Reliquias";
        private const string CompanyName = "Miguel Troncoso";
        private const string BundleIdentifier = "com.migueltroncoso.valledelasreliquias";
        private const string VersionName = "0.1.0";
        private const int VersionCode = 1;
        private const string BuildFolder = "Builds/Android";

        private static readonly string[] Scenes =
        {
            "Assets/Scenes/Prototype.unity"
        };

        [MenuItem("MMORPG/Android/Apply Android Settings")]
        public static void ApplyAndroidSettingsMenu()
        {
            ApplyAndroidSettings(true);
        }

        public static void ApplyAndroidSettingsBatch()
        {
            ApplyAndroidSettings(false);
        }

        [MenuItem("MMORPG/Android/Build Debug APK")]
        public static void BuildDebugApk()
        {
            BuildAndroid(Path.Combine(BuildFolder, "valle-reliquias-debug.apk"), false, BuildOptions.Development);
        }

        [MenuItem("MMORPG/Android/Build Google Play AAB")]
        public static void BuildGooglePlayBundle()
        {
            BuildAndroid(Path.Combine(BuildFolder, "valle-reliquias-0.1.0.aab"), true, BuildOptions.None);
        }

        private static void ApplyAndroidSettings(bool showDialog)
        {
            Directory.CreateDirectory(BuildFolder);
            BrandAssetGenerator.EnsurePlaceholderBrandAssets(false);

            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = VersionName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BundleIdentifier);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.bundleVersionCode = VersionCode;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)26;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)35;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(Scenes[0], true)
            };

            AssetDatabase.SaveAssets();

            if (showDialog)
            {
                EditorUtility.DisplayDialog("Android listo", "Configuracion Android aplicada para el prototipo.", "OK");
            }
        }

        private static void BuildAndroid(string outputPath, bool appBundle, BuildOptions options)
        {
            ApplyAndroidSettings(false);

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                throw new BuildFailedException("No se pudo cambiar a Android. Instala Android Build Support desde Unity Hub.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            EditorUserBuildSettings.buildAppBundle = appBundle;

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = options
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"Build Android fallida: {report.summary.result}");
            }

            EditorUtility.RevealInFinder(outputPath);
        }
    }
}
#endif
