using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Mascotas y monturas iniciales. Fuente unica para el generador de
    // assets (MMORPG > Companions > Generate Companions) y el fallback
    // runtime del bootstrap.
    public static class DefaultCompanions
    {
        public const string ValleyFox = "valley_fox";
        public const string EmberFox = "ember_fox";
        public const string BayHorse = "bay_horse";
        public const string StormWolf = "storm_wolf";

        public static List<PetDefinition> CreatePets()
        {
            var fox = ScriptableObject.CreateInstance<PetDefinition>();
            fox.name = ValleyFox;
            fox.PetId = ValleyFox;
            fox.DisplayName = "Zorro del valle";
            fox.Type = PetType.Experience;
            fox.Rarity = Rarity.Uncommon;
            fox.StarterOwned = true;
            fox.ExpBonusPercent = 10f;
            fox.GoldBonusPercent = 5f;
            fox.DamageBonus = 2;
            fox.BodyColor = new Color(0.92f, 0.5f, 0.18f);
            fox.Scale = 0.45f;
            fox.FollowDistance = 1.7f;

            var emberFox = ScriptableObject.CreateInstance<PetDefinition>();
            emberFox.name = EmberFox;
            emberFox.PetId = EmberFox;
            emberFox.DisplayName = "Zorro de brasa";
            emberFox.Type = PetType.Offensive;
            emberFox.Rarity = Rarity.Rare;
            emberFox.StarterOwned = false;
            emberFox.DamageBonus = 8;
            emberFox.GoldBonusPercent = 12f;
            emberFox.BodyColor = new Color(0.9f, 0.18f, 0.08f);
            emberFox.Scale = 0.5f;
            emberFox.FollowDistance = 1.8f;

            return new List<PetDefinition> { fox, emberFox };
        }

        public static List<MountDefinition> CreateMounts()
        {
            var horse = ScriptableObject.CreateInstance<MountDefinition>();
            horse.name = BayHorse;
            horse.MountId = BayHorse;
            horse.DisplayName = "Caballo bayo";
            horse.Category = MountCategory.Horse;
            horse.Rarity = Rarity.Common;
            horse.StarterOwned = true;
            horse.RequiredLevel = 5;
            horse.SpeedMultiplier = 1.6f;
            horse.MaxHealthBonus = 10;
            horse.BodyColor = new Color(0.45f, 0.3f, 0.16f);

            var stormWolf = ScriptableObject.CreateInstance<MountDefinition>();
            stormWolf.name = StormWolf;
            stormWolf.MountId = StormWolf;
            stormWolf.DisplayName = "Lobo de tormenta";
            stormWolf.Category = MountCategory.Wolf;
            stormWolf.Rarity = Rarity.Epic;
            stormWolf.StarterOwned = false;
            stormWolf.RequiredLevel = 35;
            stormWolf.SpeedMultiplier = 1.85f;
            stormWolf.DamageBonus = 15;
            stormWolf.CritChanceBonus = 0.03f;
            stormWolf.BodyColor = new Color(0.16f, 0.24f, 0.36f);

            return new List<MountDefinition> { horse, stormWolf };
        }
    }
}
