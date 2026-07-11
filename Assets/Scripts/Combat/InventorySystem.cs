using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Inventario basado en instancias de item resueltas contra ItemDatabase.
    // La API publica trabaja con ItemId (ej. "minor_potion").
    public sealed class InventorySystem : MonoBehaviour
    {
        private readonly List<ItemInstance> items = new List<ItemInstance>();

        public ItemDatabase Database;
        public PrototypeHud Hud;
        public PlayerQuestLog QuestLog;

        public IReadOnlyList<ItemInstance> Items => items;

        public void AddItem(string itemId, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return;
            }

            var definition = Database != null ? Database.Get(itemId) : null;
            if (definition == null)
            {
                Debug.LogWarning($"InventorySystem: item desconocido '{itemId}'.");
                return;
            }

            if (definition.IsStackable)
            {
                var stack = FindFirst(itemId);
                if (stack != null)
                {
                    stack.Quantity += amount;
                }
                else
                {
                    items.Add(ItemInstance.Create(itemId, amount));
                }
            }
            else
            {
                for (var i = 0; i < amount; i++)
                {
                    items.Add(ItemInstance.Create(itemId));
                }
            }

            QuestLog?.OnItemAdded(itemId, amount);
            Hud?.RefreshInventory();
            Hud?.AddFeed($"+{amount} {definition.DisplayName}");
        }

        public bool TryConsume(string itemId, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0 || Count(itemId) < amount)
            {
                return false;
            }

            var remaining = amount;
            for (var i = items.Count - 1; i >= 0 && remaining > 0; i--)
            {
                if (items[i].ItemId != itemId)
                {
                    continue;
                }

                var take = Mathf.Min(items[i].Quantity, remaining);
                items[i].Quantity -= take;
                remaining -= take;

                if (items[i].Quantity <= 0)
                {
                    items.RemoveAt(i);
                }
            }

            Hud?.RefreshInventory();
            return true;
        }

        public int Count(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return 0;
            }

            var total = 0;
            foreach (var item in items)
            {
                if (item.ItemId == itemId)
                {
                    total += item.Quantity;
                }
            }

            return total;
        }

        public ItemInstance FindFirst(string itemId)
        {
            foreach (var item in items)
            {
                if (item.ItemId == itemId)
                {
                    return item;
                }
            }

            return null;
        }

        public bool RemoveInstance(ItemInstance instance)
        {
            if (instance == null || !items.Remove(instance))
            {
                return false;
            }

            Hud?.RefreshInventory();
            return true;
        }

        public void AddInstance(ItemInstance instance)
        {
            if (instance == null)
            {
                return;
            }

            items.Add(instance);
            Hud?.RefreshInventory();
        }

        public string DisplayNameOf(string itemId)
        {
            return Database != null ? Database.DisplayNameOf(itemId) : itemId;
        }

        public List<SavedItemEntry> ExportEntries()
        {
            var totals = new Dictionary<string, int>();
            foreach (var item in items)
            {
                totals.TryGetValue(item.ItemId, out var current);
                totals[item.ItemId] = current + item.Quantity;
            }

            var entries = new List<SavedItemEntry>(totals.Count);
            foreach (var entry in totals)
            {
                entries.Add(new SavedItemEntry { Name = entry.Key, Count = entry.Value });
            }

            return entries;
        }

        public void RestoreEntries(IEnumerable<SavedItemEntry> entries)
        {
            items.Clear();

            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    var itemId = ResolveId(entry.Name);
                    if (string.IsNullOrWhiteSpace(itemId) || entry.Count <= 0)
                    {
                        continue;
                    }

                    var definition = Database != null ? Database.Get(itemId) : null;
                    if (definition == null)
                    {
                        continue;
                    }

                    if (definition.IsStackable)
                    {
                        items.Add(ItemInstance.Create(itemId, entry.Count));
                    }
                    else
                    {
                        for (var i = 0; i < entry.Count; i++)
                        {
                            items.Add(ItemInstance.Create(itemId));
                        }
                    }
                }
            }

            Hud?.RefreshInventory();
        }

        // Guardados antiguos (esquema <= 2) usaban el nombre visible como clave.
        private static string ResolveId(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            return DefaultGameItems.LegacyNameToId.TryGetValue(key, out var mapped) ? mapped : key;
        }

        public string Summary()
        {
            if (items.Count == 0)
            {
                return "Inventario: vacio";
            }

            var totals = ExportEntries();
            var parts = new List<string>();
            foreach (var entry in totals)
            {
                var label = $"{DisplayNameOf(entry.Name)} x{entry.Count}";
                if (Database != null)
                {
                    var hex = ColorUtility.ToHtmlStringRGB(Database.ColorOf(entry.Name));
                    label = $"<color=#{hex}>{label}</color>";
                }

                parts.Add(label);
                if (parts.Count >= 4)
                {
                    break;
                }
            }

            return $"Inventario: {string.Join(" | ", parts)}";
        }
    }
}
