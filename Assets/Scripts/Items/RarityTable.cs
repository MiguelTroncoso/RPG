using System;
using UnityEngine;

namespace MmorpgPrototype
{
    // Configuracion centralizada de rarezas: colores de UI y multiplicadores.
    // Nunca hardcodear colores de rareza en logica o UI.
    [CreateAssetMenu(menuName = "MMORPG/Items/Rarity Table", fileName = "RarityTable")]
    public sealed class RarityTable : ScriptableObject
    {
        [Serializable]
        public struct RarityRow
        {
            public Rarity Tier;
            public Color UiColor;
            public float PowerMultiplier;
            public int MaxUpgradeLevel;
            public float DropWeight;
        }

        public RarityRow[] Rows;

        public RarityRow GetRow(Rarity tier)
        {
            if (Rows != null)
            {
                foreach (var row in Rows)
                {
                    if (row.Tier == tier)
                    {
                        return row;
                    }
                }
            }

            return DefaultRow(tier);
        }

        public Color GetColor(Rarity tier)
        {
            return GetRow(tier).UiColor;
        }

        public string GetColorHex(Rarity tier)
        {
            return ColorUtility.ToHtmlStringRGB(GetColor(tier));
        }

        public void FillWithDefaults()
        {
            var tiers = (Rarity[])Enum.GetValues(typeof(Rarity));
            Rows = new RarityRow[tiers.Length];
            for (var i = 0; i < tiers.Length; i++)
            {
                Rows[i] = DefaultRow(tiers[i]);
            }
        }

        public static RarityRow DefaultRow(Rarity tier)
        {
            switch (tier)
            {
                case Rarity.Uncommon:
                    return new RarityRow { Tier = tier, UiColor = new Color(0.42f, 0.85f, 0.38f), PowerMultiplier = 1.15f, MaxUpgradeLevel = 15, DropWeight = 30f };
                case Rarity.Rare:
                    return new RarityRow { Tier = tier, UiColor = new Color(0.32f, 0.62f, 1f), PowerMultiplier = 1.35f, MaxUpgradeLevel = 15, DropWeight = 12f };
                case Rarity.Epic:
                    return new RarityRow { Tier = tier, UiColor = new Color(0.72f, 0.4f, 0.95f), PowerMultiplier = 1.6f, MaxUpgradeLevel = 15, DropWeight = 4f };
                case Rarity.Legendary:
                    return new RarityRow { Tier = tier, UiColor = new Color(1f, 0.72f, 0.2f), PowerMultiplier = 2f, MaxUpgradeLevel = 15, DropWeight = 1f };
                case Rarity.Mythic:
                    return new RarityRow { Tier = tier, UiColor = new Color(1f, 0.32f, 0.38f), PowerMultiplier = 2.6f, MaxUpgradeLevel = 15, DropWeight = 0.2f };
                default:
                    return new RarityRow { Tier = Rarity.Common, UiColor = new Color(0.86f, 0.88f, 0.9f), PowerMultiplier = 1f, MaxUpgradeLevel = 9, DropWeight = 53f };
            }
        }
    }
}
