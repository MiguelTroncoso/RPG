#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MmorpgPrototype.Editor
{
    // Genera/actualiza los assets de misiones en Resources a partir de
    // DefaultQuests. Idempotente: regenerar actualiza en lugar de duplicar.
    public static class QuestTools
    {
        private const string ResourcesRoot = "Assets/Resources";
        private const string GameFolder = ResourcesRoot + "/Game";
        private const string QuestsFolder = GameFolder + "/Quests";

        [MenuItem("MMORPG/Quests/Generate Quests")]
        public static void GenerateQuests()
        {
            EnsureFolder("Assets", "Resources");
            EnsureFolder(ResourcesRoot, "Game");
            EnsureFolder(GameFolder, "Quests");

            var count = 0;
            foreach (var template in DefaultQuests.CreateAll())
            {
                var path = $"{QuestsFolder}/{template.QuestId}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<QuestDefinition>(path);

                if (existing == null)
                {
                    AssetDatabase.CreateAsset(template, path);
                    existing = template;
                }
                else
                {
                    EditorUtility.CopySerialized(template, existing);
                    Object.DestroyImmediate(template);
                }

                EditorUtility.SetDirty(existing);
                count++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Misiones regeneradas: {count} assets en {QuestsFolder}.");
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
