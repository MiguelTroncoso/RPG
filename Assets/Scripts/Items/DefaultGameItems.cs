using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Set inicial de items. Es la fuente unica que usan el generador de
    // assets del editor (MMORPG > Items > Generate Item Database) y el
    // fallback runtime del bootstrap cuando los assets no existen.
    public static class DefaultGameItems
    {
        public const string MinorPotion = "minor_potion";
        public const string AncientFragment = "ancient_fragment";
        public const string WornRing = "worn_ring";
        public const string DullOre = "dull_ore";
        public const string TornScroll = "torn_scroll";
        public const string ValleyMedal = "valley_medal";
        public const string ProtectionRune = "protection_rune";
        public const string RecruitSword = "recruit_sword";
        public const string LeatherHelmet = "leather_helmet";
        public const string GuardChestplate = "guard_chestplate";
        public const string ValleyAmulet = "valley_amulet";

        // Nombres visibles usados como clave en guardados de esquema <= 2.
        public static readonly Dictionary<string, string> LegacyNameToId = new Dictionary<string, string>
        {
            { "Pocion menor", MinorPotion },
            { "Fragmento antiguo", AncientFragment },
            { "Anillo gastado", WornRing },
            { "Mineral opaco", DullOre },
            { "Pergamino roto", TornScroll },
            { "Medalla del valle", ValleyMedal }
        };

        public static List<ItemDefinition> CreateAll()
        {
            var items = new List<ItemDefinition>
            {
                Consumable(MinorPotion, "Pocion menor", "Restaura algo de vida.", Rarity.Common, healAmount: 45, buyPrice: 25, sellPrice: 8),
                Basic(AncientFragment, "Fragmento antiguo", "Resto de energia del valle. Objeto de mision.", ItemCategory.QuestItem, Rarity.Uncommon, sellPrice: 0),
                Basic(WornRing, "Anillo gastado", "Material para reforzar armadura.", ItemCategory.Material, Rarity.Common, sellPrice: 5),
                Basic(DullOre, "Mineral opaco", "Material para mejorar armas.", ItemCategory.Material, Rarity.Common, sellPrice: 5),
                Basic(TornScroll, "Pergamino roto", "Podria interesarle a alguien.", ItemCategory.Material, Rarity.Common, sellPrice: 4),
                Basic(ValleyMedal, "Medalla del valle", "Reconocimiento por estabilizar el valle.", ItemCategory.QuestItem, Rarity.Rare, sellPrice: 0),
                Basic(ProtectionRune, "Runa de proteccion", "Evita que un objeto baje de nivel o se destruya al fallar una mejora.", ItemCategory.Material, Rarity.Rare, sellPrice: 40, buyPrice: 120),

                Equipment(RecruitSword, "Espada de recluta", "Una hoja simple pero confiable.", Rarity.Common,
                    EquipSlot.Weapon, requiredLevel: 1, damage: 6, health: 0, speed: 0f, buyPrice: 60, sellPrice: 18, visualId: "sword"),
                Equipment(LeatherHelmet, "Casco de cuero", "Proteccion basica para la cabeza.", Rarity.Common,
                    EquipSlot.Helmet, requiredLevel: 2, damage: 0, health: 20, speed: 0f, buyPrice: 70, sellPrice: 20, visualId: "leather_helmet"),
                Equipment(GuardChestplate, "Pechera del guardia", "Armadura ligera de la guardia del valle.", Rarity.Uncommon,
                    EquipSlot.Chest, requiredLevel: 3, damage: 0, health: 40, speed: 0f, buyPrice: 140, sellPrice: 45, visualId: "guard_chest"),
                Equipment(ValleyAmulet, "Amuleto del valle", "Reliquia menor que vibra con energia antigua.", Rarity.Rare,
                    EquipSlot.Necklace, requiredLevel: 4, damage: 4, health: 25, speed: 0.3f, buyPrice: 260, sellPrice: 90, visualId: "valley_amulet")
            };

            items.AddRange(ProgressionItemCatalog.CreateAll());
            return items;
        }

        public static string[] MaterialDropIds()
        {
            return new[] { MinorPotion, AncientFragment, WornRing, DullOre, TornScroll, ProtectionRune };
        }

        public static string[] EquipmentDropIds()
        {
            return new[] { RecruitSword, LeatherHelmet, GuardChestplate, ValleyAmulet };
        }

        private static ItemDefinition Basic(string id, string displayName, string description, ItemCategory category, Rarity rarity, int sellPrice, int buyPrice = 0)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.name = id;
            item.ItemId = id;
            item.DisplayName = displayName;
            item.Description = description;
            item.Category = category;
            item.Rarity = rarity;
            item.MaxStack = 99;
            item.BuyPrice = buyPrice;
            item.SellPrice = sellPrice;
            return item;
        }

        private static ItemDefinition Consumable(string id, string displayName, string description, Rarity rarity, int healAmount, int buyPrice, int sellPrice)
        {
            var item = ScriptableObject.CreateInstance<ConsumableItemDefinition>();
            item.name = id;
            item.ItemId = id;
            item.DisplayName = displayName;
            item.Description = description;
            item.Rarity = rarity;
            item.MaxStack = 99;
            item.BuyPrice = buyPrice;
            item.SellPrice = sellPrice;
            item.HealAmount = healAmount;
            return item;
        }

        private static ItemDefinition Equipment(string id, string displayName, string description, Rarity rarity,
            EquipSlot slot, int requiredLevel, int damage, int health, float speed, int buyPrice, int sellPrice, string visualId = null)
        {
            var item = ScriptableObject.CreateInstance<EquipmentItemDefinition>();
            item.name = id;
            item.ItemId = id;
            item.DisplayName = displayName;
            item.Description = description;
            item.Rarity = rarity;
            item.Slot = slot;
            item.RequiredLevel = requiredLevel;
            item.DamageBonus = damage;
            item.MaxHealthBonus = health;
            item.MoveSpeedBonus = speed;
            item.VisualId = visualId;
            item.BuyPrice = buyPrice;
            item.SellPrice = sellPrice;
            return item;
        }

        public static ItemDatabase CreateRuntimeDatabase(RarityTable rarities)
        {
            var database = ScriptableObject.CreateInstance<ItemDatabase>();
            database.Items = CreateAll();
            database.Rarities = rarities;
            database.RebuildLookup();
            return database;
        }
    }
}
