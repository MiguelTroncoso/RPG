#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MmorpgPrototype.Editor
{
    // Genera/actualiza los assets de mascotas y monturas en Resources a
    // partir de DefaultCompanions. Idempotente.
    public static class CompanionTools
    {
        private const string ResourcesRoot = "Assets/Resources";
        private const string GameFolder = ResourcesRoot + "/Game";
        private const string PetsFolder = GameFolder + "/Pets";
        private const string MountsFolder = GameFolder + "/Mounts";

        [MenuItem("MMORPG/Companions/Generate Companions")]
        public static void GenerateCompanions()
        {
            EnsureFolder("Assets", "Resources");
            EnsureFolder(ResourcesRoot, "Game");
            EnsureFolder(GameFolder, "Pets");
            EnsureFolder(GameFolder, "Mounts");

            var count = 0;
            foreach (var template in DefaultCompanions.CreatePets())
            {
                SaveAsset(template, $"{PetsFolder}/{template.PetId}.asset");
                count++;
            }

            foreach (var template in DefaultCompanions.CreateMounts())
            {
                SaveAsset(template, $"{MountsFolder}/{template.MountId}.asset");
                count++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Companions regenerados: {count} assets en {PetsFolder} y {MountsFolder}.");
        }

        private static void SaveAsset(ScriptableObject template, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(template, path);
                EditorUtility.SetDirty(template);
                return;
            }

            EditorUtility.CopySerialized(template, existing);
            Object.DestroyImmediate(template);
            EditorUtility.SetDirty(existing);
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
