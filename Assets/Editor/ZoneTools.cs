#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MmorpgPrototype.Editor
{
    // Genera/actualiza los assets de zonas en Resources a partir de
    // DefaultZones. Idempotente.
    public static class ZoneTools
    {
        private const string ResourcesRoot = "Assets/Resources";
        private const string GameFolder = ResourcesRoot + "/Game";
        private const string ZonesFolder = GameFolder + "/Zones";

        [MenuItem("MMORPG/World/Generate Zones")]
        public static void GenerateZones()
        {
            EnsureFolder("Assets", "Resources");
            EnsureFolder(ResourcesRoot, "Game");
            EnsureFolder(GameFolder, "Zones");

            var count = 0;
            foreach (var template in DefaultZones.CreateAll())
            {
                var path = $"{ZonesFolder}/{template.ZoneId}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<ZoneDefinition>(path);

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
            Debug.Log($"Zonas regeneradas: {count} assets en {ZonesFolder}.");
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
