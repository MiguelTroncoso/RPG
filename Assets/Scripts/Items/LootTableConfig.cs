using System;
using UnityEngine;

namespace MmorpgPrototype
{
    // Tabla de botin como datos: probabilidad global de drop y entradas con
    // peso. Reemplaza a la antigua LootTable estatica con listas en codigo.
    [CreateAssetMenu(menuName = "MMORPG/Items/Loot Table", fileName = "LootTable")]
    public sealed class LootTableConfig : ScriptableObject
    {
        [Serializable]
        public struct LootEntry
        {
            public string ItemId;
            [Min(0f)] public float Weight;
        }

        [Range(0f, 1f)] public float DropChance = 0.42f;
        public LootEntry[] Entries;

        public bool HasEntries => Entries != null && Entries.Length > 0;

        // Rolls inyectados (0..1) para mantener la regla testeable.
        public string Roll(float chanceRoll, float weightRoll)
        {
            if (!HasEntries || chanceRoll > DropChance)
            {
                return string.Empty;
            }

            var totalWeight = 0f;
            foreach (var entry in Entries)
            {
                totalWeight += Mathf.Max(0f, entry.Weight);
            }

            if (totalWeight <= 0f)
            {
                return string.Empty;
            }

            var pick = weightRoll * totalWeight;
            foreach (var entry in Entries)
            {
                pick -= Mathf.Max(0f, entry.Weight);
                if (pick <= 0f)
                {
                    return entry.ItemId;
                }
            }

            return Entries[Entries.Length - 1].ItemId;
        }

        public void FillWithDefaults()
        {
            DropChance = 0.42f;
            Entries = new[]
            {
                new LootEntry { ItemId = DefaultGameItems.MinorPotion, Weight = 20f },
                new LootEntry { ItemId = DefaultGameItems.AncientFragment, Weight = 16f },
                new LootEntry { ItemId = DefaultGameItems.WornRing, Weight = 16f },
                new LootEntry { ItemId = DefaultGameItems.DullOre, Weight = 16f },
                new LootEntry { ItemId = DefaultGameItems.TornScroll, Weight = 12f },
                new LootEntry { ItemId = DefaultGameItems.ProtectionRune, Weight = 4f },
                new LootEntry { ItemId = DefaultGameItems.SkillTome, Weight = 2f },
                new LootEntry { ItemId = DefaultGameItems.RecruitSword, Weight = 4f },
                new LootEntry { ItemId = DefaultGameItems.LeatherHelmet, Weight = 4f },
                new LootEntry { ItemId = DefaultGameItems.GuardChestplate, Weight = 3f },
                new LootEntry { ItemId = DefaultGameItems.ValleyAmulet, Weight = 2f }
            };
        }
    }
}
