#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MmorpgPrototype.Editor
{
    public static class MonsterAnimatorAssetGenerator
    {
        private const string ModelRoot = "Assets/Resources/ThirdParty/Quaternius/UltimateMonsters/FBX";
        private const string ControllerRoot = "Assets/Resources/ThirdParty/Quaternius/UltimateMonsters/Controllers";

        [MenuItem("MMORPG/Monsters/Generate Ultimate Monsters Controllers")]
        public static void GenerateFromMenu()
        {
            Generate();
        }

        public static void Generate()
        {
            EnsureFolder("Assets/Resources/ThirdParty/Quaternius", "UltimateMonsters");
            EnsureFolder("Assets/Resources/ThirdParty/Quaternius/UltimateMonsters", "Controllers");

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
            Debug.Log($"MonsterAnimatorAssetGenerator: generated {modelPaths.Count} controllers");
        }

        private static void GenerateController(string modelPath)
        {
            var clips = AssetDatabase.LoadAllAssetsAtPath(modelPath)
                .OfType<AnimationClip>()
                .Where(clip => !clip.name.StartsWith("__preview__"))
                .ToList();
            if (clips.Count == 0)
            {
                Debug.LogWarning($"MonsterAnimatorAssetGenerator: no clips found in {modelPath}");
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
            var idle = AddState(stateMachine, "Idle", PickClip(clips, "idle", "stand") ?? clips[0]);
            var run = AddState(stateMachine, "Run", PickClip(clips, "run", "walk", "fly") ?? clips[0]);
            var attack = AddState(stateMachine, "Attack", PickClip(clips, "attack", "punch", "bite", "headbutt", "claw") ?? clips[0]);
            var hit = AddState(stateMachine, "Hit", PickClip(clips, "hit", "hurt", "damage") ?? clips[0]);
            var death = AddState(stateMachine, "Death", PickClip(clips, "death", "die", "defeat") ?? clips[0]);
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

            var anyToHit = stateMachine.AddAnyStateTransition(hit);
            anyToHit.hasExitTime = false;
            anyToHit.duration = 0.04f;
            anyToHit.AddCondition(AnimatorConditionMode.If, 0f, "Hit");

            var anyToDeath = stateMachine.AddAnyStateTransition(death);
            anyToDeath.hasExitTime = false;
            anyToDeath.duration = 0.04f;
            anyToDeath.AddCondition(AnimatorConditionMode.If, 0f, "Death");

            var attackToIdle = attack.AddTransition(idle);
            attackToIdle.hasExitTime = true;
            attackToIdle.exitTime = 0.9f;
            attackToIdle.duration = 0.1f;

            var hitToIdle = hit.AddTransition(idle);
            hitToIdle.hasExitTime = true;
            hitToIdle.exitTime = 0.9f;
            hitToIdle.duration = 0.12f;

            EditorUtility.SetDirty(controller);
            Debug.Log($"MonsterAnimatorAssetGenerator: {modelName} uses {clips.Count} clips");
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
