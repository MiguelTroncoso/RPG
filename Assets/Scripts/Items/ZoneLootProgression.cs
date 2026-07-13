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

            switch (tier)
            {
                case EnemyTier.Elite:
                    table.DropChance = 0.76f;
                    entries.Add(new LootTableConfig.LootEntry { ItemId = common, Weight = 18f });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = upgrade, Weight = 34f });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = DefaultGameItems.MinorPotion, Weight = 7f });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = DefaultGameItems.ProtectionRune, Weight = 4f });
                    AddGear(entries, gear, 6f);
                    break;

                case EnemyTier.Boss:
                    table.DropChance = 0.92f;
                    entries.Add(new LootTableConfig.LootEntry { ItemId = common, Weight = 18f });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = upgrade, Weight = 42f });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = DefaultGameItems.MinorPotion, Weight = 8f });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = DefaultGameItems.ProtectionRune, Weight = 8f });
                    AddGear(entries, gear, 5f);
                    break;

                default:
                    table.DropChance = 0.58f;
                    entries.Add(new LootTableConfig.LootEntry { ItemId = common, Weight = 46f });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = upgrade, Weight = 10f });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = DefaultGameItems.MinorPotion, Weight = 18f });
                    entries.Add(new LootTableConfig.LootEntry { ItemId = DefaultGameItems.AncientFragment, Weight = 4f });
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
