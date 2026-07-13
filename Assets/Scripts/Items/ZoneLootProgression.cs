using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Construye tablas de loot por banda de nivel y tier. Se crean en runtime
    // para conservar el fallback de LootTableConfig y evitar duplicar diez
    // assets casi identicos mientras el balance sigue iterando.
    public static class ZoneLootProgression
    {
        public static LootTableConfig CreateFor(string zoneId, EnemyTier tier, LootTableConfig fallback)
        {
            if (string.IsNullOrWhiteSpace(ProgressionItemCatalog.CommonMaterialFor(zoneId))
                || ProgressionItemCatalog.CommonMaterialFor(zoneId).StartsWith("zone_"))
            {
                return fallback;
            }

            var table = ScriptableObject.CreateInstance<LootTableConfig>();
            var entries = new List<LootTableConfig.LootEntry>();
            var common = ProgressionItemCatalog.CommonMaterialFor(zoneId);
            var upgrade = ProgressionItemCatalog.UpgradeMaterialFor(zoneId);
            var gear = ProgressionItemCatalog.GearIdsFor(zoneId);
            var bandIndex = Mathf.Max(0, ProgressionItemCatalog.BandIndexFor(zoneId));

            switch (tier)
            {
                case EnemyTier.Elite:
                    table.DropChance = Mathf.Min(0.92f, 0.68f + bandIndex * 0.025f);
                    entries.Add(new LootTableConfig.LootEntry { ItemId = common, Weight = Mathf.Max(10f, 16f - bandIndex * 0.4f) });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = upgrade, Weight = 28f + bandIndex * 2f });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = DefaultGameItems.MinorPotion, Weight = Mathf.Max(4f, 6f - bandIndex * 0.2f) });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = DefaultGameItems.ProtectionRune, Weight = 3f + bandIndex * 0.4f });
                    AddGear(entries, gear, 5f + bandIndex * 0.4f);
                    break;

                case EnemyTier.Boss:
                    table.DropChance = Mathf.Min(0.96f, 0.88f + bandIndex * 0.012f);
                    entries.Add(new LootTableConfig.LootEntry { ItemId = common, Weight = Mathf.Max(10f, 14f - bandIndex * 0.3f) });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = upgrade, Weight = 34f + bandIndex * 2.4f });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = DefaultGameItems.MinorPotion, Weight = 6f });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = DefaultGameItems.ProtectionRune, Weight = 7f + bandIndex * 0.6f });
                    AddGear(entries, gear, 4f + bandIndex * 0.5f);
                    break;

                default:
                    table.DropChance = Mathf.Min(0.68f, 0.52f + bandIndex * 0.015f);
                    entries.Add(new LootTableConfig.LootEntry { ItemId = common, Weight = Mathf.Max(34f, 48f - bandIndex * 1.2f) });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = upgrade, Weight = 7f + bandIndex * 1.5f });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = DefaultGameItems.MinorPotion, Weight = Mathf.Max(12f, 17f - bandIndex * 0.3f) });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = DefaultGameItems.AncientFragment, Weight = 3f });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = DefaultGameItems.TornScroll, Weight = 4f });
                    break;
            }

            table.Entries = entries.ToArray();
            return table;
        }

        private static void AddGear(List<LootTableConfig.LootEntry> entries, string[] gear, float weight)
        {
            if (gear == null)
            {
                return;
            }

            foreach (var itemId in gear)
            {
                if (!string.IsNullOrWhiteSpace(itemId))
                {
                    entries.Add(new LootTableConfig.LootEntry { ItemId = itemId, Weight = weight });
                }
            }
        }
    }
}
