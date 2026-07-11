using UnityEngine;

namespace MmorpgPrototype
{
    public static class LootTable
    {
        private const float DropChance = 0.42f;
        private const float EquipmentShare = 0.15f;

        public static string RollDrop()
        {
            if (Random.value > DropChance)
            {
                return string.Empty;
            }

            var pool = Random.value < EquipmentShare
                ? DefaultGameItems.EquipmentDropIds()
                : DefaultGameItems.MaterialDropIds();

            return pool[Random.Range(0, pool.Length)];
        }
    }
}
