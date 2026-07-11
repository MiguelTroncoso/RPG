using UnityEngine;

namespace MmorpgPrototype
{
    [CreateAssetMenu(menuName = "MMORPG/Items/Consumable Definition", fileName = "ConsumableDefinition")]
    public sealed class ConsumableItemDefinition : ItemDefinition
    {
        [Min(0)] public int HealAmount = 45;

        private void OnEnable()
        {
            Category = ItemCategory.Consumable;
        }
    }
}
