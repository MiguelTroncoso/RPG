using UnityEngine;

namespace MmorpgPrototype
{
    public enum CosmeticSlot
    {
        Outfit,
        Wings
    }

    [CreateAssetMenu(menuName = "MMORPG/Cosmetics/Cosmetic Definition", fileName = "CosmeticDefinition")]
    public sealed class CosmeticDefinition : ScriptableObject
    {
        public string CosmeticId;
        public string DisplayName;
        public CosmeticSlot Slot = CosmeticSlot.Outfit;
        public Rarity Rarity = Rarity.Common;
        public bool IsShopItem;
        [Min(0)] public int ShopPrice;
        [Min(1)] public int RequiredLevel = 1;
        [Min(0)] public int DamageBonus;
        [Min(0)] public int MaxHealthBonus;
        [Range(0f, 1f)] public float CritChanceBonus;
        public Color PrimaryColor = Color.white;
        public Color SecondaryColor = Color.gray;
    }
}
