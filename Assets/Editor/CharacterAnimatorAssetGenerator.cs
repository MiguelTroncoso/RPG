#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MmorpgPrototype.Editor
{
    public static class CharacterAnimatorAssetGenerator
    {
        private const string CharacterRoot = "Assets/Resources/ThirdParty/KayKit/Adventurers/Characters";
        private const string ControllerRoot = "Assets/Resources/ThirdParty/KayKit/Adventurers/Controllers";

        [MenuItem("MMORPG/Characters/Generate KayKit Controllers")]
        public static void GenerateFromMenu()
        {
            Generate();
        }

        public static void Generate()
        {
            EnsureFolder("Assets/Resources/ThirdParty/KayKit/Adventurers", "Controllers");

            foreach (var character in new[] { "Knight", "Rogue", "Mage", "Barbarian" })
            {
                var modelPath = $"{CharacterRoot}/{character}.fbx";
                var clips = AssetDatabase.LoadAllAssetsAtPath(modelPath)
                    .OfType<AnimationClip>()
                    .Where(clip => !clip.name.StartsWith("__preview__"))
                    .ToList();
                if (clips.Count == 0)
                {
                    Debug.LogWarning($"CharacterAnimatorAssetGenerator: no clips found in {modelPath}");
                    continue;
                }

                var controllerPath = $"{ControllerRoot}/{character}.controller";
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
                var attack = AddState(stateMachine, "Attack", PickClip(clips, "attack", "slash", "sword") ?? clips[0]);
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
                Debug.Log($"CharacterAnimatorAssetGenerator: {character} uses {clips.Count} clips");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
