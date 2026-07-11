using System;
using UnityEngine;

namespace MmorpgPrototype
{
    // Plantilla para generar variantes de equipo por nivel (1..105) sin
    // crear items a mano: stats base en nivel 1 + curva de crecimiento +
    // rareza por conjunto visual (tier). La usa la ventana de editor
    // MMORPG > Items > Item Variant Generator.
    [CreateAssetMenu(menuName = "MMORPG/Items/Item Archetype", fileName = "ItemArchetype")]
    public sealed class ItemArchetype : ScriptableObject
    {
        public string ArchetypeId = "valley_sword";
        public string DisplayNameFormat = "Espada del valle Nv{0}";
        public EquipSlot Slot = EquipSlot.Weapon;

        [Header("Conjuntos visuales por rango de nivel")]
        [Min(1)] public int TierSize = 10;
        public Rarity[] TierRarities =
        {
            Rarity.Common, Rarity.Common, Rarity.Common,
            Rarity.Uncommon, Rarity.Uncommon,
            Rarity.Rare, Rarity.Rare,
            Rarity.Epic, Rarity.Epic,
            Rarity.Legendary,
            Rarity.Mythic
        };

        [Header("Stats en nivel 1 y crecimiento")]
        [Min(0)] public int BaseDamage = 6;
        [Min(0)] public int BaseHealth;
        [Min(0f)] public float BaseSpeed;
        [Range(0.1f, 2f)] public float GrowthExponent = 0.9f;

        [Header("Economia")]
        [Min(0)] public int BasePrice = 60;
        [Range(0.5f, 2f)] public float PriceExponent = 1.1f;

        public int TierOf(int level)
        {
            return 1 + (Math.Max(1, level) - 1) / Math.Max(1, TierSize);
        }

        public Rarity RarityOf(int level)
        {
            if (TierRarities == null || TierRarities.Length == 0)
            {
                return Rarity.Common;
            }

            var index = Math.Clamp(TierOf(level) - 1, 0, TierRarities.Length - 1);
            return TierRarities[index];
        }

        public string ItemIdFor(int level)
        {
            return $"{ArchetypeId}_lv{level:D3}";
        }

        public EquipmentItemDefinition CreateDefinition(int level)
        {
            var safeLevel = Math.Max(1, level);
            var item = ScriptableObject.CreateInstance<EquipmentItemDefinition>();
            item.name = ItemIdFor(safeLevel);
            item.ItemId = item.name;
            item.DisplayName = string.Format(DisplayNameFormat, safeLevel);
            item.Description = $"Equipo del conjunto {TierOf(safeLevel)} (niveles {FirstLevelOfTier(safeLevel)}-{LastLevelOfTier(safeLevel)}).";
            item.Slot = Slot;
            item.RequiredLevel = safeLevel;
            item.Rarity = RarityOf(safeLevel);
            item.DamageBonus = Scale(BaseDamage, safeLevel, GrowthExponent);
            item.MaxHealthBonus = Scale(BaseHealth, safeLevel, GrowthExponent);
            item.MoveSpeedBonus = BaseSpeed;
            item.BuyPrice = Scale(BasePrice, safeLevel, PriceExponent);
            item.SellPrice = Math.Max(1, item.BuyPrice / 3);
            return item;
        }

        private int FirstLevelOfTier(int level)
        {
            return (TierOf(level) - 1) * Math.Max(1, TierSize) + 1;
        }

        private int LastLevelOfTier(int level)
        {
            return TierOf(level) * Math.Max(1, TierSize);
        }

        private static int Scale(int baseValue, int level, float exponent)
        {
            if (baseValue <= 0)
            {
                return 0;
            }

            return Math.Max(baseValue, (int)Math.Round(baseValue * Math.Pow(level, exponent)));
        }
    }
}
