#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MmorpgPrototype.Editor
{
    public static class OriginalMobAnimatorAssetGenerator
    {
        private const string ModelRoot = "Assets/Resources/OriginalArt/Mobs";
        private const string ControllerRoot = "Assets/Resources/OriginalArt/Mobs/Controllers";

        [MenuItem("MMORPG/Mobs/Generate Original Art Mob Controllers")]
        public static void GenerateFromMenu()
        {
            Generate();
        }

        public static void Generate()
        {
            EnsureFolder("Assets/Resources", "OriginalArt");
            EnsureFolder("Assets/Resources/OriginalArt", "Mobs");
            EnsureFolder(ModelRoot, "Controllers");

            var modelPaths = Directory.Exists(ModelRoot)
                ? Directory.GetFiles(ModelRoot, "*.fbx", SearchOption.TopDirectoryOnly)
                    .Select(path => path.Replace('\\', '/'))
                    .OrderBy(path => path)
                    .ToList()
                : new List<string>();

            foreach (var modelPath in modelPaths)
            {
                GenerateController(modelPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"OriginalMobAnimatorAssetGenerator: generated {modelPaths.Count} controllers");
        }

        private static void GenerateController(string modelPath)
        {
            var clips = AssetDatabase.LoadAllAssetsAtPath(modelPath)
                .OfType<AnimationClip>()
                .Where(clip => !clip.name.StartsWith("__preview__"))
                .ToList();
            if (clips.Count == 0)
            {
                Debug.LogWarning($"OriginalMobAnimatorAssetGenerator: no clips found in {modelPath}");
                return;
            }

            var modelName = Path.GetFileNameWithoutExtension(modelPath);
            var controllerPath = $"{ControllerRoot}/{modelName}.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                AssetDatabase.DeleteAsset(controllerPath);
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

            var stateMachine = controller.layers[0].stateMachine;
            var idle = AddState(stateMachine, "Idle", PickClip(clips, "_Idle", "idle") ?? clips[0]);
            var run = AddState(stateMachine, "Run", PickClip(clips, "_Run", "run") ?? clips[0]);
            var attack = AddState(stateMachine, "Attack", PickClip(clips, "_Attack", "attack") ?? clips[0]);
            var hit = AddState(stateMachine, "Hit", PickClip(clips, "_Hit", "hit") ?? clips[0]);
            var death = AddState(stateMachine, "Death", PickClip(clips, "_Death", "death") ?? clips[0]);
            stateMachine.defaultState = idle;

            var idleToRun = idle.AddTransition(run);
            idleToRun.hasExitTime = false;
            idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.08f, "Speed");
            var runToIdle = run.AddTransition(idle);
            runToIdle.hasExitTime = false;
            runToIdle.AddCondition(AnimatorConditionMode.Less, 0.05f, "Speed");

            AddTriggerTransition(stateMachine, attack, "Attack");
            AddTriggerTransition(stateMachine, hit, "Hit");
            AddTriggerTransition(stateMachine, death, "Death");
            AddExitTransition(attack, idle, 0.9f, 0.1f);
            AddExitTransition(hit, idle, 0.9f, 0.12f);

            EditorUtility.SetDirty(controller);
            Debug.Log($"OriginalMobAnimatorAssetGenerator: {modelName} uses {clips.Count} clips");
        }

        private static void AddTriggerTransition(AnimatorStateMachine stateMachine, AnimatorState destination, string parameter)
        {
            var transition = stateMachine.AddAnyStateTransition(destination);
            transition.hasExitTime = false;
            transition.duration = 0.04f;
            transition.AddCondition(AnimatorConditionMode.If, 0f, parameter);
        }

        private static void AddExitTransition(AnimatorState source, AnimatorState destination, float exitTime, float duration)
        {
            var transition = source.AddTransition(destination);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = duration;
        }

        private static AnimatorState AddState(AnimatorStateMachine stateMachine, string name, AnimationClip clip)
        {
            var state = stateMachine.AddState(name);
            state.motion = clip;
            return state;
        }

        private static AnimationClip PickClip(List<AnimationClip> clips, params string[] terms)
        {
            return clips.FirstOrDefault(clip => terms.Any(term => clip.name.ToLowerInvariant().Contains(term.ToLowerInvariant())));
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
