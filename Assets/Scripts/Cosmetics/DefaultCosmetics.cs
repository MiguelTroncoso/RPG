using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    public static class DefaultCosmetics
    {
        public const string TravelerOutfit = "traveler_outfit";
        public const string EmberWings = "ember_wings";
        public const string VoidOutfit = "void_outfit";

        public static List<CosmeticDefinition> CreateAll()
        {
            var traveler = Create(TravelerOutfit, "Atuendo del viajero", CosmeticSlot.Outfit, Rarity.Common, false, 0,
                1, 0, 12, 0f, new Color(0.18f, 0.42f, 0.75f), new Color(0.9f, 0.72f, 0.28f));
            var wings = Create(EmberWings, "Alas de brasa", CosmeticSlot.Wings, Rarity.Rare, true, 750,
                12, 3, 20, 0.02f, new Color(0.95f, 0.2f, 0.08f), new Color(1f, 0.72f, 0.18f));
            var voidOutfit = Create(VoidOutfit, "Atuendo del eclipse", CosmeticSlot.Outfit, Rarity.Epic, true, 1200,
                25, 8, 30, 0.04f, new Color(0.16f, 0.06f, 0.3f), new Color(0.65f, 0.18f, 0.95f));
            return new List<CosmeticDefinition> { traveler, wings, voidOutfit };
        }

        private static CosmeticDefinition Create(string id, string name, CosmeticSlot slot, Rarity rarity,
            bool shopItem, int price, int requiredLevel, int damage, int health, float crit, Color primary, Color secondary)
        {
            var definition = ScriptableObject.CreateInstance<CosmeticDefinition>();
            definition.name = id;
            definition.CosmeticId = id;
            definition.DisplayName = name;
            definition.Slot = slot;
            definition.Rarity = rarity;
            definition.IsShopItem = shopItem;
            definition.ShopPrice = price;
            definition.RequiredLevel = requiredLevel;
            definition.DamageBonus = damage;
            definition.MaxHealthBonus = health;
            definition.CritChanceBonus = crit;
            definition.PrimaryColor = primary;
            definition.SecondaryColor = secondary;
            return definition;
        }
    }
}
