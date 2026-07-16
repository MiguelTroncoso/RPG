#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MmorpgPrototype.Editor
{
    public static class OriginalArtAnimatorAssetGenerator
    {
        private const string CharacterRoot = "Assets/Resources/OriginalArt/Characters";
        private const string ControllerRoot = "Assets/Resources/OriginalArt/Controllers";

        [MenuItem("MMORPG/Characters/Generate Original Art Controllers")]
        public static void GenerateFromMenu()
        {
            Generate();
        }

        public static void Generate()
        {
            EnsureFolder("Assets/Resources/OriginalArt", "Controllers");
            ConfigureAtlasImportSettings();

            foreach (var className in new[] { "Guerrero", "Ninja", "Chaman", "Umbra" })
            {
                foreach (var gender in new[] { "Masculino", "Femenino" })
                {
                    var modelPath = $"{CharacterRoot}/{className}_{gender}.fbx";
                    var allClips = AssetDatabase.LoadAllAssetsAtPath(modelPath)
                        .OfType<AnimationClip>()
                        .Where(clip => !clip.name.StartsWith("__preview__"))
                        .ToList();
                    var clips = allClips
                        .Where(clip => clip.name.Contains($"Original_{className}_{gender}_"))
                        .ToList();
                    if (clips.Count == 0)
                    {
                        Debug.LogWarning($"OriginalArtAnimatorAssetGenerator: no clips found in {modelPath}");
                        continue;
                    }

                    var controllerPath = $"{ControllerRoot}/{className}_{gender}.controller";
                    if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
                    {
                        AssetDatabase.DeleteAsset(controllerPath);
                    }

                    var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                    controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
                    controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

                    var stateMachine = controller.layers[0].stateMachine;
                    var idle = AddState(stateMachine, "Idle", PickClip(clips, "idle") ?? clips[0]);
                    var run = AddState(stateMachine, "Run", PickClip(clips, "run", "walk") ?? clips[0]);
                    var attack = AddState(stateMachine, "Attack", PickClip(clips, "attack") ?? clips[0]);
                    stateMachine.defaultState = idle;

                    var idleToRun = idle.AddTransition(run);
                    idleToRun.hasExitTime = false;
                    idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.08f, "Speed");

                    var runToIdle = run.AddTransition(idle);
                    runToIdle.hasExitTime = false;
                    runToIdle.AddCondition(AnimatorConditionMode.Less, 0.05f, "Speed");

                    var anyToAttack = stateMachine.AddAnyStateTransition(attack);
                    anyToAttack.hasExitTime = false;
                    anyToAttack.duration = 0.05f;
                    anyToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

                    var attackToIdle = attack.AddTransition(idle);
                    attackToIdle.hasExitTime = true;
                    attackToIdle.exitTime = 0.9f;
                    attackToIdle.duration = 0.1f;

                    EditorUtility.SetDirty(controller);
                    Debug.Log($"OriginalArtAnimatorAssetGenerator: {className}_{gender} uses {clips.Count} authored clips");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ConfigureAtlasImportSettings()
        {
            foreach (var className in new[] { "Guerrero", "Ninja", "Chaman", "Umbra" })
            {
                ConfigureTextureImport($"Assets/Resources/OriginalArt/Textures/OriginalArt_{className}_AlbedoAtlas_2K.png", false);
                ConfigureTextureImport($"Assets/Resources/OriginalArt/Textures/OriginalArt_{className}_NormalAtlas_2K.png", true);
            }
        }

        private static void ConfigureTextureImport(string path, bool normalMap)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !normalMap;
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 2;

            var android = importer.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.maxTextureSize = 2048;
            android.format = TextureImporterFormat.ETC2_RGBA8;
            importer.SetPlatformTextureSettings(android);
            importer.SaveAndReimport();
        }

        private static AnimatorState AddState(AnimatorStateMachine stateMachine, string name, AnimationClip clip)
        {
            var state = stateMachine.AddState(name);
            state.motion = clip;
            return state;
        }

        private static AnimationClip PickClip(List<AnimationClip> clips, params string[] terms)
        {
            return clips.FirstOrDefault(clip => terms.Any(term => clip.name.ToLowerInvariant().Contains(term)));
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
#endif
