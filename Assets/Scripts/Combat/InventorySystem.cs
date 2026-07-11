using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class InventorySystem : MonoBehaviour
    {
        private readonly Dictionary<string, int> items = new Dictionary<string, int>();

        public PrototypeHud Hud;
        public PlayerQuestLog QuestLog;

        public void AddItem(string itemName, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(itemName) || amount <= 0)
            {
                return;
            }

            if (!items.ContainsKey(itemName))
            {
                items[itemName] = 0;
            }

            items[itemName] += amount;
            QuestLog?.OnItemAdded(itemName, amount);
            Hud?.RefreshInventory();
            Hud?.AddFeed($"+{amount} {itemName}");
        }

        public bool TryConsume(string itemName, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(itemName) || amount <= 0 || Count(itemName) < amount)
            {
                return false;
            }

            items[itemName] -= amount;
            if (items[itemName] <= 0)
            {
                items.Remove(itemName);
            }

            Hud?.RefreshInventory();
            return true;
        }

        public int Count(string itemName)
        {
            return !string.IsNullOrWhiteSpace(itemName) && items.TryGetValue(itemName, out var count) ? count : 0;
        }

        public string Summary()
        {
            if (items.Count == 0)
            {
                return "Inventario: vacio";
            }

            var parts = new List<string>();
            foreach (var entry in items)
            {
                parts.Add($"{entry.Key} x{entry.Value}");
                if (parts.Count >= 4)
                {
                    break;
                }
            }

            return $"Inventario: {string.Join(" | ", parts)}";
        }
    }
}
