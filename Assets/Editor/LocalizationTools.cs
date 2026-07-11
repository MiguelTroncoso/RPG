#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MmorpgPrototype.Editor
{
    // Genera/actualiza la tabla de textos (es) en Resources a partir de
    // DefaultLocalization. Idempotente.
    public static class LocalizationTools
    {
        private const string ResourcesRoot = "Assets/Resources";
        private const string GameFolder = ResourcesRoot + "/Game";
        private const string TableAssetPath = GameFolder + "/LocalizationTable.asset";

        [MenuItem("MMORPG/Localization/Generate Localization")]
        public static void GenerateLocalization()
        {
            if (!AssetDatabase.IsValidFolder(ResourcesRoot))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(GameFolder))
            {
                AssetDatabase.CreateFolder(ResourcesRoot, "Game");
            }

            var table = AssetDatabase.LoadAssetAtPath<LocalizationTable>(TableAssetPath);
            if (table == null)
            {
                table = ScriptableObject.CreateInstance<LocalizationTable>();
                AssetDatabase.CreateAsset(table, TableAssetPath);
            }

            table.LanguageCode = "es";
            table.FillFrom(DefaultLocalization.CreateSpanish());
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
            Debug.Log($"LocalizationTable regenerada: {table.Entries.Count} claves ({table.LanguageCode}) en {TableAssetPath}.");
        }
    }
}
#endif
