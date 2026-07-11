#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MmorpgPrototype.Editor
{
    // Genera/actualiza los assets de items en Resources a partir de
    // DefaultGameItems. Idempotente: los IDs son deterministas, regenerar
    // actualiza los assets existentes en lugar de duplicarlos.
    public static class ItemDatabaseTools
    {
        private const string ResourcesRoot = "Assets/Resources";
        private const string GameFolder = ResourcesRoot + "/Game";
        private const string ItemsFolder = GameFolder + "/Items";
        private const string RarityAssetPath = GameFolder + "/RarityTable.asset";
        private const string DatabaseAssetPath = GameFolder + "/ItemDatabase.asset";

        [MenuItem("MMORPG/Items/Generate Item Database")]
        public static void GenerateItemDatabase()
        {
            EnsureFolder("Assets", "Resources");
            EnsureFolder(ResourcesRoot, "Game");
            EnsureFolder(GameFolder, "Items");

            var rarities = AssetDatabase.LoadAssetAtPath<RarityTable>(RarityAssetPath);
            if (rarities == null)
            {
                rarities = ScriptableObject.CreateInstance<RarityTable>();
                rarities.FillWithDefaults();
                AssetDatabase.CreateAsset(rarities, RarityAssetPath);
            }

            var database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(DatabaseAssetPath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<ItemDatabase>();
                AssetDatabase.CreateAsset(database, DatabaseAssetPath);
            }

            database.Rarities = rarities;
            database.Items.Clear();

            foreach (var template in DefaultGameItems.CreateAll())
            {
                var path = $"{ItemsFolder}/{template.ItemId}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);

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
                database.Items.Add(existing);
            }

            database.RebuildLookup();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            var problems = database.Validate();
            if (problems.Count > 0)
            {
                foreach (var problem in problems)
                {
                    Debug.LogWarning($"ItemDatabase: {problem}");
                }
            }
            else
            {
                Debug.Log($"ItemDatabase regenerada: {database.Items.Count} items en {ItemsFolder}, sin problemas de validacion.");
            }
        }

        private const string UpgradeAssetPath = GameFolder + "/UpgradeConfig.asset";

        [MenuItem("MMORPG/Items/Generate Upgrade Config")]
        public static void GenerateUpgradeConfig()
        {
            EnsureFolder("Assets", "Resources");
            EnsureFolder(ResourcesRoot, "Game");

            var config = AssetDatabase.LoadAssetAtPath<UpgradeConfig>(UpgradeAssetPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<UpgradeConfig>();
                AssetDatabase.CreateAsset(config, UpgradeAssetPath);
            }

            config.FillWithDefaults();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            var first = config.GetStep(1);
            var last = config.GetStep(config.MaxUpgradeLevel);
            Debug.Log($"UpgradeConfig regenerado: +1 ({first.SuccessChance:P0}) .. +{config.MaxUpgradeLevel} ({last.SuccessChance:P0}, {last.OnFailure}).");
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
