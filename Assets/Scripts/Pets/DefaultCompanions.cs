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
        public const string BayHorse = "bay_horse";

        public static List<PetDefinition> CreatePets()
        {
            var fox = ScriptableObject.CreateInstance<PetDefinition>();
            fox.name = ValleyFox;
            fox.PetId = ValleyFox;
            fox.DisplayName = "Zorro del valle";
            fox.Type = PetType.Experience;
            fox.Rarity = Rarity.Uncommon;
            fox.ExpBonusPercent = 10f;
            fox.GoldBonusPercent = 5f;
            fox.BodyColor = new Color(0.92f, 0.5f, 0.18f);
            fox.Scale = 0.45f;
            fox.FollowDistance = 1.7f;

            return new List<PetDefinition> { fox };
        }

        public static List<MountDefinition> CreateMounts()
        {
            var horse = ScriptableObject.CreateInstance<MountDefinition>();
            horse.name = BayHorse;
            horse.MountId = BayHorse;
            horse.DisplayName = "Caballo bayo";
            horse.Category = MountCategory.Horse;
            horse.Rarity = Rarity.Common;
            horse.RequiredLevel = 5;
            horse.SpeedMultiplier = 1.6f;
            horse.BodyColor = new Color(0.45f, 0.3f, 0.16f);

            return new List<MountDefinition> { horse };
        }
    }
}
