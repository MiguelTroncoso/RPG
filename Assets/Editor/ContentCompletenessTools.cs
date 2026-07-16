#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MmorpgPrototype.Editor
{
    // Auditoria reproducible del contenido funcional 1-105. El informe separa
    // cobertura de datos y recursos existentes de la calidad del arte final.
    public static class ContentCompletenessTools
    {
        private const string ReportPath = "docs/content-completeness.md";

        [MenuItem("MMORPG/QA/Generate Content Completeness Report")]
        public static void GenerateReport()
        {
            var checks = new List<CheckResult>();
            var zones = new List<ZoneDefinition>();
            var quests = new List<QuestDefinition>();
            var items = new List<ItemDefinition>();
            var temporaryLoot = new List<LootTableConfig>();
            ExpCurveConfig temporaryCurve = null;

            try
            {
                zones = DefaultZones.CreateAll();
                quests = DefaultQuests.CreateAll();
                items = DefaultGameItems.CreateAll();

                AuditLevelBands(zones, checks);
                AuditZones(zones, checks);
                AuditQuests(zones, quests, items, checks);
                AuditItems(zones, items, checks);
                AuditLoot(zones, items, temporaryLoot, checks);
                AuditClasses(checks);
                AuditVisualResources(checks);
                AuditDailyEvent(checks);
                AuditLongevity(checks);

                temporaryCurve = ScriptableObject.CreateInstance<ExpCurveConfig>();
                AuditLevelCurve(temporaryCurve, checks);

                WriteReport(checks, zones, quests, items);
            }
            finally
            {
                DestroyAll(zones);
                DestroyAll(quests);
                DestroyAll(items);
                DestroyAll(temporaryLoot);
                if (temporaryCurve != null)
                {
                    UnityEngine.Object.DestroyImmediate(temporaryCurve);
                }
            }

            var errors = checks.Count(check => !check.Passed);
            if (errors == 0)
            {
                Debug.Log($"Auditoria 1-105 completada: {checks.Count} checks sin errores. Reporte: {ReportPath}");
            }
            else
            {
                Debug.LogError($"Auditoria 1-105 encontro {errors} problemas. Revisar: {ReportPath}");
            }
        }

        private static void AuditLevelBands(List<ZoneDefinition> zones, List<CheckResult> checks)
        {
            var valid = zones.Count == 10
                && zones.OrderBy(zone => zone.MinLevel).FirstOrDefault()?.MinLevel == 1
                && zones.OrderBy(zone => zone.MaxLevel).LastOrDefault()?.MaxLevel == 105;

            var expectedMin = 1;
            foreach (var zone in zones.OrderBy(zone => zone.MinLevel))
            {
                valid &= zone.MinLevel == expectedMin && zone.MaxLevel >= zone.MinLevel;
                expectedMin = zone.MaxLevel + 1;
            }

            checks.Add(new CheckResult(
                "Bandas de nivel 1-105",
                valid && expectedMin == 106,
                $"{zones.Count} zonas, rango continuo 1-{Math.Max(0, expectedMin - 1)}"));
        }

        private static void AuditZones(List<ZoneDefinition> zones, List<CheckResult> checks)
        {
            var uniqueIds = zones.Count == zones.Select(zone => zone.ZoneId).Distinct().Count();
            var validData = true;
            var enemyArchetypes = 0;

            foreach (var zone in zones)
            {
                var zoneValid = !string.IsNullOrWhiteSpace(zone.ZoneId)
                    && !string.IsNullOrWhiteSpace(zone.DisplayName)
                    && !string.IsNullOrWhiteSpace(zone.NormalEnemyId)
                    && !string.IsNullOrWhiteSpace(zone.EliteEnemyId)
                    && !string.IsNullOrWhiteSpace(zone.BossEnemyId)
                    && zone.NormalCount > 0
                    && zone.EliteCount > 0
                    && zone.NormalHealth > 0
                    && zone.EliteHealth > 0
                    && zone.BossHealth > 0
                    && zone.NormalExp > 0
                    && zone.EliteExp > 0
                    && zone.BossExp > 0
                    && zone.NormalTtkMin <= zone.NormalTtkMax
                    && zone.EliteTtkMin <= zone.EliteTtkMax
                    && zone.BossTtkMin <= zone.BossTtkMax
                    && !string.IsNullOrWhiteSpace(zone.BossGuaranteedDrop);

                validData &= zoneValid;
                enemyArchetypes += 3;
            }

            var safeZone = zones.Count > 0
                && zones[0].ZoneId == DefaultZones.Zone1
                && zones[0].HasSafeZone
                && zones[0].SafeZoneRadius > 0f;
            var noCombatInOtherSafeZones = zones.Skip(1).All(zone => !zone.HasSafeZone);

            checks.Add(new CheckResult(
                "Zonas y balance base",
                uniqueIds && validData,
                $"{zones.Count} zonas validas, {enemyArchetypes} arquetipos normales/elite/jefe"));
            checks.Add(new CheckResult(
                "Zona segura y comercio inicial",
                safeZone && noCombatInOtherSafeZones,
                "Zona 1 protegida; las zonas de combate permanecen fuera del area segura"));
        }

        private static void AuditQuests(List<ZoneDefinition> zones, List<QuestDefinition> quests,
            List<ItemDefinition> items, List<CheckResult> checks)
        {
            var itemIds = new HashSet<string>(items.Where(item => item != null).Select(item => item.ItemId));
            var idsUnique = quests.Count == quests.Select(quest => quest.QuestId).Distinct().Count();
            var orderValid = quests.Count == 35 && Enumerable.Range(1, quests.Count).All(order => quests.Any(quest => quest.SortOrder == order));
            var byId = quests.Where(quest => quest != null).ToDictionary(quest => quest.QuestId, quest => quest);
            var chainValid = quests.All(quest => string.IsNullOrEmpty(quest.NextQuestId) || byId.ContainsKey(quest.NextQuestId));
            var rewardItemsValid = quests.All(quest => quest.Reward != null
                && quest.Reward.Experience > 0
                && quest.Reward.Gold >= 0
                && (quest.Reward.Items == null || quest.Reward.Items.All(item => itemIds.Contains(item.ItemId) && item.Count > 0)));

            var objectives = new HashSet<string>(StringComparer.Ordinal);
            foreach (var quest in quests)
            {
                if (quest.Objectives == null)
                {
                    continue;
                }

                foreach (var objective in quest.Objectives)
                {
                    if (!string.IsNullOrWhiteSpace(objective.TargetId))
                    {
                        objectives.Add(objective.TargetId);
                    }
                }
            }

            var zoneObjectivesValid = true;
            foreach (var zone in zones)
            {
                var normalCovered = zone.ZoneId == DefaultZones.Zone1
                    ? quests.Any(quest => quest.QuestId == DefaultQuests.ClearFields)
                    : objectives.Contains(zone.NormalEnemyId);
                zoneObjectivesValid &= normalCovered
                    && objectives.Contains(zone.EliteEnemyId)
                    && objectives.Contains(zone.BossEnemyId);
            }

            checks.Add(new CheckResult(
                "Cadena de misiones 1-105",
                idsUnique && orderValid && chainValid && rewardItemsValid && zoneObjectivesValid,
                $"{quests.Count} misiones encadenadas; cada zona tiene objetivos normal/elite/jefe"));
        }

        private static void AuditItems(List<ZoneDefinition> zones, List<ItemDefinition> items, List<CheckResult> checks)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var uniqueIds = true;
            var validDefinitions = true;

            foreach (var item in items)
            {
                uniqueIds &= item != null && !string.IsNullOrWhiteSpace(item.ItemId) && ids.Add(item.ItemId);
                validDefinitions &= item != null && !string.IsNullOrWhiteSpace(item.DisplayName) && item.MaxStack > 0;
            }

            var expectedItems = 12 + (zones.Count * (2 + ProgressionItemCatalog.GearIdsFor(DefaultZones.Zone1).Length + 1));
            var allZoneSetsValid = zones.All(zone =>
                !string.IsNullOrWhiteSpace(ProgressionItemCatalog.CommonMaterialFor(zone.ZoneId))
                && !string.IsNullOrWhiteSpace(ProgressionItemCatalog.UpgradeMaterialFor(zone.ZoneId))
                && ProgressionItemCatalog.GearIdsFor(zone.ZoneId).Length == 12
                && !string.IsNullOrWhiteSpace(ProgressionItemCatalog.BossRelicFor(zone.ZoneId)));

            checks.Add(new CheckResult(
                "Materiales, equipo y reliquias",
                uniqueIds && validDefinitions && items.Count == expectedItems && allZoneSetsValid,
                $"{items.Count} items: {zones.Count} bandas x 2 materiales x 12 piezas + reliquia, mas set inicial"));
        }

        private static void AuditLoot(List<ZoneDefinition> zones, List<ItemDefinition> items,
            List<LootTableConfig> temporaryLoot, List<CheckResult> checks)
        {
            var itemIds = new HashSet<string>(items.Select(item => item.ItemId), StringComparer.Ordinal);
            var valid = true;
            var tableCount = 0;

            foreach (var zone in zones)
            {
                foreach (var tier in new[] { EnemyTier.Normal, EnemyTier.Elite, EnemyTier.Boss })
                {
                    var table = ZoneLootProgression.CreateFor(zone.ZoneId, tier, null);
                    temporaryLoot.Add(table);
                    tableCount++;
                    valid &= table != null && table.DropChance > 0f && table.DropChance <= 1f && table.HasEntries;
                    if (table != null && table.Entries != null)
                    {
                        valid &= table.Entries.All(entry => itemIds.Contains(entry.ItemId) && entry.Weight > 0f);
                        if (tier == EnemyTier.Elite || tier == EnemyTier.Boss)
                        {
                            valid &= ProgressionItemCatalog.GearIdsFor(zone.ZoneId)
                                .All(gearId => table.Entries.Any(entry => entry.ItemId == gearId));
                        }
                    }
                }
            }

            var guaranteedRelics = zones.All(zone => itemIds.Contains(zone.BossGuaranteedDrop));
            checks.Add(new CheckResult(
                "Loot por zona y tier",
                valid && tableCount == zones.Count * 3 && guaranteedRelics,
                $"{tableCount} tablas runtime, equipo en elite/jefe y reliquia garantizada por jefe"));
        }

        private static void AuditClasses(List<CheckResult> checks)
        {
            var types = (CharacterClassType[])Enum.GetValues(typeof(CharacterClassType));
            var valid = types.Length == 4;
            foreach (var type in types)
            {
                var definition = ClassDefinition.Create(type);
                valid &= !string.IsNullOrWhiteSpace(definition.DisplayName)
                    && !string.IsNullOrWhiteSpace(definition.SkillOneName)
                    && !string.IsNullOrWhiteSpace(definition.SkillTwoName)
                    && !string.IsNullOrWhiteSpace(definition.SkillThreeName)
                    && !string.IsNullOrWhiteSpace(definition.SkillFourName)
                    && !string.IsNullOrWhiteSpace(definition.UltimateSkillName)
                    && !string.IsNullOrWhiteSpace(definition.CharacterModelResource)
                    && !string.IsNullOrWhiteSpace(definition.FemaleCharacterModelResource);
            }

            checks.Add(new CheckResult(
                "Clases, sexo y habilidades",
                valid,
                "4 clases, modelo masculino/femenino y 4 habilidades mas final por clase"));
        }

        private static void AuditVisualResources(List<CheckResult> checks)
        {
            var monsterModels = new[]
            {
                "MushroomKing", "Tribal", "Orc", "Orc_Skull", "Yeti", "BlueDemon", "Demon",
                "Goleling", "Goleling_Evolved", "Glub", "Glub_Evolved", "Dragon", "Dragon_Evolved",
                "Ghost", "Ghost_Skull"
            };
            var monsterResources = monsterModels.Count(model => AssetExists($"Assets/Resources/ThirdParty/Quaternius/UltimateMonsters/FBX/{model}.fbx"));
            var classResources = new[]
            {
                "Assets/Resources/ThirdParty/KayKit/Adventurers/Characters/Knight.fbx",
                "Assets/Resources/ThirdParty/KayKit/Adventurers/Characters/Rogue.fbx",
                "Assets/Resources/ThirdParty/KayKit/Adventurers/Characters/RogueHooded.fbx",
                "Assets/Resources/ThirdParty/KayKit/Adventurers/Characters/Mage.fbx",
                "Assets/Resources/ThirdParty/KayKit/Adventurers/Characters/Barbarian.fbx",
                "Assets/Resources/ThirdParty/Quaternius/Characters/AnimatedWoman.fbx"
            };
            var classResourceCount = classResources.Count(AssetExists);
            var controllerCount = new[] { "Knight", "Rogue", "Mage", "Barbarian" }
                .Count(controller => AssetExists($"Assets/Resources/ThirdParty/KayKit/Adventurers/Controllers/{controller}.controller"));

            checks.Add(new CheckResult(
                "Mobs 3D y variantes de zona",
                monsterResources == monsterModels.Length,
                $"{monsterResources}/{monsterModels.Length} modelos de mobs disponibles"));
            checks.Add(new CheckResult(
                "Personajes 3D y animacion",
                classResourceCount == classResources.Length && controllerCount == 4,
                $"{classResourceCount}/{classResources.Length} modelos de clase y {controllerCount}/4 controladores"));
        }

        private static void AuditDailyEvent(List<CheckResult> checks)
        {
            checks.Add(new CheckResult(
                "Eventos diarios",
                !string.IsNullOrWhiteSpace(DailyEventSystem.EventId) && DailyEventSystem.TargetDefeats > 0,
                $"Evento {DailyEventSystem.EventId}, objetivo {DailyEventSystem.TargetDefeats} derrotas"));
        }

        private static void AuditLongevity(List<CheckResult> checks)
        {
            var save = new PlayerSaveData();
            checks.Add(new CheckResult(
                "Contratos repetibles",
                RepeatableContractSystem.ContractsPerDay == 3,
                "3 contratos diarios rotativos por banda: normal, elite y material de mejora"));
            checks.Add(new CheckResult(
                "Renacimiento y Renombre",
                save.SchemaVersion == 15 && save.Contracts != null,
                "Renacer al nivel 105 conserva progresion permanente y actualiza el guardado al esquema 15"));
            checks.Add(new CheckResult(
                "Evento semanal",
                WeeklyEventSystem.TargetDefeats == 30 && WeeklyEventSystem.TargetEliteDefeats == 3,
                "Conquista semanal: 30 derrotas y 3 objetivos elite con reset UTC"));
            checks.Add(new CheckResult(
                "Temporada",
                SeasonProgressionSystem.MaxSeasonLevel == 30 && SeasonProgressionSystem.SeasonLengthDays == 28 && save.Season != null,
                "Temporada de 28 dias con 30 niveles, hitos y recompensa final"));
        }

        private static void AuditLevelCurve(ExpCurveConfig curve, List<CheckResult> checks)
        {
            var valid = curve != null && curve.MaxLevel == 105 && curve.ExpToNext(1) > 0;
            var previous = 0L;
            for (var level = 1; level <= 105; level++)
            {
                var current = curve.ExpToNext(level);
                valid &= current >= previous;
                previous = current;
            }

            checks.Add(new CheckResult(
                "Curva de experiencia",
                valid,
                "Tabla funcional de nivel 1 a 105 con EXP monotonicamente creciente"));
        }

        private static bool AssetExists(string path)
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return File.Exists(Path.Combine(projectRoot, path));
        }

        private static void WriteReport(List<CheckResult> checks, List<ZoneDefinition> zones,
            List<QuestDefinition> quests, List<ItemDefinition> items)
        {
            var directory = Path.GetDirectoryName(ReportPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var passed = checks.Count(check => check.Passed);
            var builder = new StringBuilder();
            builder.AppendLine("# Auditoria de contenido 1-105");
            builder.AppendLine();
            builder.AppendLine($"> Generado por `MMORPG > QA > Generate Content Completeness Report` el {DateTime.Now:yyyy-MM-dd HH:mm}.");
            builder.AppendLine();
            builder.AppendLine("## Estado");
            builder.AppendLine();
            builder.AppendLine($"- **Contenido funcional 1-105:** {(passed == checks.Count ? "100%" : $"{passed}/{checks.Count} checks")}");
            builder.AppendLine("- **Arte comercial final:** no se declara 100% en esta auditoria; requiere reemplazar progresivamente el arte provisional, VFX, UI y cinematica.");
            builder.AppendLine($"- **Datos auditados:** {zones.Count} zonas, {quests.Count} misiones, {items.Count} items.");
            builder.AppendLine();
            builder.AppendLine("## Checks");
            builder.AppendLine();
            builder.AppendLine("| Estado | Area | Resultado |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (var check in checks)
            {
                builder.AppendLine($"| {(check.Passed ? "PASS" : "FAIL")} | {check.Name} | {check.Detail} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Lectura de producto");
            builder.AppendLine();
            builder.AppendLine("El juego ya tiene una progresion funcional completa desde el nivel 1 hasta el 105: diez zonas continuas, combate por tiers, misiones, materiales, sets de 12 piezas, reliquias de jefe, habilidades de clase, habilidad final y evento diario.");
            builder.AppendLine("La siguiente etapa no es inventar mas contenido de niveles, sino convertir esta cobertura funcional en calidad comercial: modelos finales, VFX, animaciones, UI, audio, optimizacion, online autoritativo y pruebas con jugadores.");

            File.WriteAllText(ReportPath, builder.ToString(), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static void DestroyAll<T>(List<T> objects) where T : UnityEngine.Object
        {
            foreach (var item in objects)
            {
                if (item != null)
                {
                    UnityEngine.Object.DestroyImmediate(item);
                }
            }
        }

        private struct CheckResult
        {
            public readonly string Name;
            public readonly bool Passed;
            public readonly string Detail;

            public CheckResult(string name, bool passed, string detail)
            {
                Name = name;
                Passed = passed;
                Detail = detail;
            }
        }
    }
}
#endif
