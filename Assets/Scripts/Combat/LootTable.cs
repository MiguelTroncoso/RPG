using UnityEngine;

namespace MmorpgPrototype
{
    public static class LootTable
    {
        private static readonly string[] Drops =
        {
            "Pocion menor",
            "Fragmento antiguo",
            "Anillo gastado",
            "Mineral opaco",
            "Pergamino roto"
        };

        public static string RollDrop()
        {
            if (Random.value > 0.58f)
            {
                return string.Empty;
            }

            return Drops[Random.Range(0, Drops.Length)];
        }
    }
}

