#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MmorpgPrototype.Editor
{
    // Ventana para generar variantes de equipo por nivel desde un
    // ItemArchetype. Los IDs son deterministas (archetype_lvNNN): regenerar
    // actualiza los assets existentes en lugar de duplicarlos, y las
    // variantes se registran en el ItemDatabase asset si existe.
    public sealed class ItemVariantGeneratorWindow : EditorWindow
    {
        private const string ResourcesRoot = "Assets/Resources";
        private const string GameFolder = ResourcesRoot + "/Game";
        private const string ItemsFolder = GameFolder + "/Items";
        private const string GeneratedFolder = ItemsFolder + "/Generated";
        private const string ArchetypesFolder = GameFolder + "/Archetypes";
        private const string DatabaseAssetPath = GameFolder + "/ItemDatabase.asset";

        private ItemArchetype archetype;
        private int fromLevel = 1;
        private int toLevel = 105;
        private int levelStep = 5;
        private string lastReport = string.Empty;

        [MenuItem("MMORPG/Items/Item Variant Generator")]
        public static void Open()
        {
            GetWindow<ItemVariantGeneratorWindow>("Item Variants");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Generador de variantes por nivel", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            archetype = (ItemArchetype)EditorGUILayout.ObjectField("Arquetipo", archetype, typeof(ItemArchetype), false);
            fromLevel = Mathf.Clamp(EditorGUILayout.IntField("Nivel desde", fromLevel), 1, 105);
            toLevel = Mathf.Clamp(EditorGUILayout.IntField("Nivel hasta", toLevel), fromLevel, 105);
            levelStep = Mathf.Clamp(EditorGUILayout.IntField("Cada N niveles", levelStep), 1, 105);

            EditorGUILayout.Space(8f);

            if (GUILayout.Button("Crear arquetipo de ejemplo (espada)"))
            {
                CreateSampleArchetype();
            }

            using (new EditorGUI.DisabledScope(archetype == null))
            {
                if (GUILayout.Button($"Generar variantes ({CountLevels()} items)"))
                {
                    Generate();
                }
            }

            if (!string.IsNullOrEmpty(lastReport))
            {
                EditorGUILayout.Space(8f);
                EditorGUILayout.HelpBox(lastReport, MessageType.Info);
            }
        }

        private int CountLevels()
        {
            return (toLevel - fromLevel) / levelStep + 1;
        }

        private void CreateSampleArchetype()
        {
            EnsureFolder("Assets", "Resources");
            EnsureFolder(ResourcesRoot, "Game");
            EnsureFolder(GameFolder, "Archetypes");

            var path = $"{ArchetypesFolder}/valley_sword.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ItemArchetype>(path);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<ItemArchetype>();
                AssetDatabase.CreateAsset(existing, path);
                AssetDatabase.SaveAssets();
            }

            archetype = existing;
            lastReport = $"Arquetipo de ejemplo en {path}. Ajusta stats/curva en el inspector y genera.";
        }

        private void Generate()
        {
            EnsureFolder("Assets", "Resources");
            EnsureFolder(ResourcesRoot, "Game");
            EnsureFolder(GameFolder, "Items");
            EnsureFolder(ItemsFolder, "Generated");

            var generated = new List<ItemDefinition>();
            var created = 0;
            var updated = 0;

            for (var level = fromLevel; level <= toLevel; level += levelStep)
            {
                var template = archetype.CreateDefinition(level);
                var path = $"{GeneratedFolder}/{template.ItemId}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<EquipmentItemDefinition>(path);

                if (existing == null)
                {
                    AssetDatabase.CreateAsset(template, path);
                    existing = template;
                    created++;
                }
                else
                {
                    EditorUtility.CopySerialized(template, existing);
                    Object.DestroyImmediate(template);
                    updated++;
                }

                EditorUtility.SetDirty(existing);
                generated.Add(existing);
            }

            var registered = RegisterInDatabase(generated);
            AssetDatabase.SaveAssets();

            lastReport = $"{created} creados, {updated} actualizados en {GeneratedFolder}. {registered}";
            Debug.Log($"ItemVariantGenerator: {lastReport}");
        }

        private static string RegisterInDatabase(List<ItemDefinition> items)
        {
            var database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(DatabaseAssetPath);
            if (database == null)
            {
                return "ItemDatabase.asset no existe (usa MMORPG > Items > Generate Item Database); las variantes no quedaron registradas.";
            }

            var added = 0;
            foreach (var item in items)
            {
                if (!database.Items.Contains(item))
                {
                    database.Items.Add(item);
                    added++;
                }
            }

            database.RebuildLookup();
            EditorUtility.SetDirty(database);

            var problems = database.Validate();
            if (problems.Count > 0)
            {
                foreach (var problem in problems)
                {
                    Debug.LogWarning($"ItemDatabase: {problem}");
                }

                return $"{added} registrados en ItemDatabase, con {problems.Count} problemas de validacion (ver consola).";
            }

            return $"{added} registrados en ItemDatabase, validacion sin problemas.";
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
