using System;
using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Catalogo determinista de la progresion 1-105. Cada banda corresponde a
    // una zona, tiene materiales propios, un set de equipo y una reliquia de
    // jefe. Los ItemId son estables para inventario, loot y guardados.
    public static class ProgressionItemCatalog
    {
        private struct ZoneBand
        {
            public string ZoneId;
            public string Key;
            public string Name;
            public int MinLevel;
            public int MaxLevel;

            public ZoneBand(string zoneId, string key, string name, int minLevel, int maxLevel)
            {
                ZoneId = zoneId;
                Key = key;
                Name = name;
                MinLevel = minLevel;
                MaxLevel = maxLevel;
            }
        }

        private static readonly ZoneBand[] Bands =
        {
            new ZoneBand(DefaultZones.Zone1, "valley", "Valle", 1, 10),
            new ZoneBand(DefaultZones.Zone2, "forest", "Bosque", 11, 20),
            new ZoneBand(DefaultZones.Zone3, "ash", "Ceniza", 21, 30),
            new ZoneBand(DefaultZones.Zone4, "crystal", "Cristal", 31, 40),
            new ZoneBand(DefaultZones.Zone5, "frost", "Escarcha", 41, 50),
            new ZoneBand(DefaultZones.Zone6, "sunken", "Abismo", 51, 60),
            new ZoneBand(DefaultZones.Zone7, "obsidian", "Obsidiana", 61, 70),
            new ZoneBand(DefaultZones.Zone8, "astral", "Astral", 71, 80),
            new ZoneBand(DefaultZones.Zone9, "eclipse", "Eclipse", 81, 90),
            new ZoneBand(DefaultZones.Zone10, "throne", "Vacio", 91, 105)
        };

        private static readonly EquipSlot[] SetSlots =
        {
            EquipSlot.Weapon,
            EquipSlot.Helmet,
            EquipSlot.Chest,
            EquipSlot.Gloves,
            EquipSlot.Pants,
            EquipSlot.Boots,
            EquipSlot.Necklace
        };

        public static string CommonMaterialFor(string zoneId)
        {
            return IdFor(zoneId, "material_common");
        }

        public static string UpgradeMaterialFor(string zoneId)
        {
            return IdFor(zoneId, "material_upgrade");
        }

        public static string GearFor(string zoneId, EquipSlot slot)
        {
            return IdFor(zoneId, "gear_" + slot.ToString().ToLowerInvariant());
        }

        public static string BossRelicFor(string zoneId)
        {
            return IdFor(zoneId, "boss_relic");
        }

        public static string[] GearIdsFor(string zoneId)
        {
            var ids = new string[SetSlots.Length];
            for (var i = 0; i < SetSlots.Length; i++)
            {
                ids[i] = GearFor(zoneId, SetSlots[i]);
            }

            return ids;
        }

        public static List<ItemDefinition> CreateAll()
        {
            var items = new List<ItemDefinition>(Bands.Length * 10);
            for (var index = 0; index < Bands.Length; index++)
            {
                var band = Bands[index];
                var rarity = GearRarity(index);
                var materialRarity = index >= 5 ? Rarity.Uncommon : Rarity.Common;
                var upgradeRarity = index >= 7 ? Rarity.Rare : Rarity.Uncommon;

                items.Add(CreateMaterial(
                    CommonMaterialFor(band.ZoneId),
                    $"Fragmento de {band.Name.ToLowerInvariant()}",
                    $"Material comun de la zona {band.MinLevel}-{band.MaxLevel}.",
                    materialRarity, 3 + index * 2));
                items.Add(CreateMaterial(
                    UpgradeMaterialFor(band.ZoneId),
                    $"Nucleo de {band.Name.ToLowerInvariant()}",
                    $"Material de mejora de la zona {band.MinLevel}-{band.MaxLevel}.",
                    upgradeRarity, 8 + index * 4));

                foreach (var slot in SetSlots)
                {
                    items.Add(CreateGear(band, index, slot, rarity));
                }

                items.Add(CreateBossRelic(band, index, rarity));
            }

            return items;
        }

        private static ItemDefinition CreateMaterial(string id, string displayName, string description, Rarity rarity, int sellPrice)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.name = id;
            item.ItemId = id;
            item.DisplayName = displayName;
            item.Description = description;
            item.Category = ItemCategory.Material;
            item.Rarity = rarity;
            item.MaxStack = 99;
            item.SellPrice = sellPrice;
            return item;
        }

        private static EquipmentItemDefinition CreateGear(ZoneBand band, int index, EquipSlot slot, Rarity rarity)
        {
            var item = ScriptableObject.CreateInstance<EquipmentItemDefinition>();
            var id = GearFor(band.ZoneId, slot);
            var power = Growth(8 + index * 3, band.MinLevel, 0.86f);

            item.name = id;
            item.ItemId = id;
            item.DisplayName = $"{band.Name} {SlotName(slot)}";
            item.Description = $"Pieza del conjunto de {band.Name.ToLowerInvariant()} para niveles {band.MinLevel}-{band.MaxLevel}.";
            item.Slot = slot;
            item.RequiredLevel = band.MinLevel;
            item.Rarity = rarity;
            item.DamageBonus = slot == EquipSlot.Weapon ? power : slot == EquipSlot.Necklace ? Math.Max(1, power / 5) : 0;
            item.MaxHealthBonus = HealthFor(slot, power);
            item.MoveSpeedBonus = slot == EquipSlot.Boots ? 0.08f + index * 0.015f : slot == EquipSlot.Necklace ? 0.03f + index * 0.01f : 0f;
            item.BuyPrice = Growth(70 + index * 20, band.MinLevel, 1.05f);
            item.SellPrice = Math.Max(1, item.BuyPrice / 3);
            item.VisualId = slot == EquipSlot.Weapon ? "sword" : string.Empty;
            item.UpgradeMaterialId = UpgradeMaterialFor(band.ZoneId);
            item.ProgressionTier = index + 1;
            return item;
        }

        private static EquipmentItemDefinition CreateBossRelic(ZoneBand band, int index, Rarity setRarity)
        {
            var item = ScriptableObject.CreateInstance<EquipmentItemDefinition>();
            var id = BossRelicFor(band.ZoneId);
            var power = Growth(12 + index * 4, band.MinLevel, 0.9f);

            item.name = id;
            item.ItemId = id;
            item.DisplayName = $"Reliquia del jefe de {band.Name.ToLowerInvariant()}";
            item.Description = $"Recompensa exclusiva del jefe de la zona {band.MinLevel}-{band.MaxLevel}.";
            item.Slot = EquipSlot.Talisman;
            item.RequiredLevel = band.MinLevel;
            item.Rarity = BossRarity(setRarity);
            item.DamageBonus = Math.Max(2, power / 3);
            item.MaxHealthBonus = power * 2;
            item.MoveSpeedBonus = 0.08f + index * 0.015f;
            item.BuyPrice = 0;
            item.SellPrice = Math.Max(1, power * 2);
            item.UpgradeMaterialId = UpgradeMaterialFor(band.ZoneId);
            item.ProgressionTier = index + 1;
            item.BossExclusive = true;
            return item;
        }

        private static int HealthFor(EquipSlot slot, int power)
        {
            switch (slot)
            {
                case EquipSlot.Helmet: return power * 2;
                case EquipSlot.Chest: return power * 4;
                case EquipSlot.Gloves: return power;
                case EquipSlot.Pants: return power * 2;
                case EquipSlot.Boots: return power;
                case EquipSlot.Necklace: return power * 2;
                default: return 0;
            }
        }

        private static Rarity GearRarity(int index)
        {
            var rarities = new[]
            {
                Rarity.Common, Rarity.Uncommon, Rarity.Uncommon, Rarity.Rare,
                Rarity.Rare, Rarity.Epic, Rarity.Epic, Rarity.Legendary,
                Rarity.Legendary, Rarity.Mythic
            };
            return rarities[Math.Max(0, Math.Min(index, rarities.Length - 1))];
        }

        private static Rarity BossRarity(Rarity setRarity)
        {
            return (int)setRarity >= (int)Rarity.Epic ? Rarity.Legendary : setRarity == Rarity.Rare ? Rarity.Epic : Rarity.Rare;
        }

        private static int Growth(int baseValue, int level, float exponent)
        {
            return Math.Max(baseValue, (int)Math.Round(baseValue * Math.Pow(Math.Max(1, level), exponent)));
        }

        private static string IdFor(string zoneId, string suffix)
        {
            var key = "zone";
            for (var i = 0; i < Bands.Length; i++)
            {
                if (Bands[i].ZoneId == zoneId)
                {
                    key = Bands[i].Key;
                    break;
                }
            }

            return $"{key}_{suffix}";
        }

        private static string SlotName(EquipSlot slot)
        {
            switch (slot)
            {
                case EquipSlot.Weapon: return "arma";
                case EquipSlot.Helmet: return "casco";
                case EquipSlot.Chest: return "pechera";
                case EquipSlot.Gloves: return "guantes";
                case EquipSlot.Pants: return "pantalones";
                case EquipSlot.Boots: return "botas";
                case EquipSlot.Necklace: return "collar";
                default: return slot.ToString().ToLowerInvariant();
            }
        }
    }
}
