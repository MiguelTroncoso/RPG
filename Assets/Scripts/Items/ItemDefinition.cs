using UnityEngine;

namespace MmorpgPrototype
{
    // Definicion inmutable de un item (nunca muta en runtime). El estado del
    // jugador vive en ItemInstance.
    [CreateAssetMenu(menuName = "MMORPG/Items/Item Definition", fileName = "ItemDefinition")]
    public class ItemDefinition : ScriptableObject
    {
        public string ItemId;
        public string DisplayName;
        [TextArea] public string Description;
        public ItemCategory Category = ItemCategory.Material;
        public Rarity Rarity = Rarity.Common;
        [Min(1)] public int MaxStack = 99;
        [Min(0)] public int BuyPrice;
        [Min(0)] public int SellPrice;

        public bool IsStackable => MaxStack > 1;
    }
}
