using System;

namespace MmorpgPrototype
{
    // Instancia concreta de un item en manos del jugador. El InstanceId
    // (GUID) es la base anti-duplicacion de cara al servidor.
    [Serializable]
    public sealed class ItemInstance
    {
        public string InstanceId;
        public string ItemId;
        public int Quantity = 1;
        public int UpgradeLevel;

        public static ItemInstance Create(string itemId, int quantity = 1)
        {
            return new ItemInstance
            {
                InstanceId = Guid.NewGuid().ToString("N"),
                ItemId = itemId,
                Quantity = Math.Max(1, quantity)
            };
        }
    }
}
