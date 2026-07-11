#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MmorpgPrototype.Editor
{
    // Genera/actualiza los assets de progresion en Resources para que el
    // bootstrap runtime los cargue. Regenerar es idempotente: actualiza la
    // tabla existente en lugar de duplicarla.
    public static class ProgressionTableTools
    {
        private const string ResourcesRoot = "Assets/Resources";
        private const string GameFolder = ResourcesRoot + "/Game";
        private const string ConfigAssetPath = GameFolder + "/ExpCurveConfig.asset";
        private const string TableAssetPath = GameFolder + "/LevelProgressionTable.asset";

        [MenuItem("MMORPG/Progression/Generate Level Table")]
        public static void GenerateLevelTable()
        {
            if (!AssetDatabase.IsValidFolder(ResourcesRoot))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(GameFolder))
            {
                AssetDatabase.CreateFolder(ResourcesRoot, "Game");
            }

            var config = AssetDatabase.LoadAssetAtPath<ExpCurveConfig>(ConfigAssetPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ExpCurveConfig>();
                AssetDatabase.CreateAsset(config, ConfigAssetPath);
            }

            var table = AssetDatabase.LoadAssetAtPath<LevelProgressionTable>(TableAssetPath);
            if (table == null)
            {
                table = ScriptableObject.CreateInstance<LevelProgressionTable>();
                AssetDatabase.CreateAsset(table, TableAssetPath);
            }

            table.GenerateFrom(config);
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();

            Debug.Log($"Tabla de niveles regenerada ({table.MaxLevel} niveles) desde {ConfigAssetPath}. " +
                      $"EXP nivel 1: {table.GetExpToNext(1)}, nivel {table.MaxLevel - 1}: {table.GetExpToNext(table.MaxLevel - 1)}.");
        }
    }
}
#endif
