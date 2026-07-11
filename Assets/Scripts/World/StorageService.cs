using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Almacen simple: deposita todos los materiales del inventario y los
    // devuelve al retirar. El contenido persiste en el guardado.
    public sealed class StorageService : MonoBehaviour
    {
        public InventorySystem Inventory;
        public PrototypeHud Hud;

        private readonly List<SavedItemEntry> stash = new List<SavedItemEntry>();

        public bool HasStoredItems => stash.Count > 0;

        public void Toggle()
        {
            if (HasStoredItems)
            {
                WithdrawAll();
            }
            else
            {
                DepositMaterials();
            }
        }

        public void DepositMaterials()
        {
            if (Inventory == null)
            {
                return;
            }

            var taken = Inventory.TakeAllOfCategory(ItemCategory.Material);
            if (taken.Count == 0)
            {
                Hud?.SetStatus(Localization.Tr("storage.nothing"));
                return;
            }

            var total = 0;
            foreach (var entry in taken)
            {
                Merge(entry);
                total += entry.Count;
            }

            Hud?.SetStatus(Localization.Tr("storage.deposited", total));
            Hud?.AddFeed(Localization.Tr("storage.feed", total));
        }

        public void WithdrawAll()
        {
            if (Inventory == null || stash.Count == 0)
            {
                return;
            }

            foreach (var entry in stash)
            {
                Inventory.AddItem(entry.Name, entry.Count);
            }

            stash.Clear();
            Hud?.SetStatus(Localization.Tr("storage.withdrawn"));
        }

        public List<SavedItemEntry> ExportEntries()
        {
            return new List<SavedItemEntry>(stash);
        }

        public void RestoreEntries(IEnumerable<SavedItemEntry> entries)
        {
            stash.Clear();
            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    if (!string.IsNullOrWhiteSpace(entry.Name) && entry.Count > 0)
                    {
                        Merge(entry);
                    }
                }
            }
        }

        private void Merge(SavedItemEntry entry)
        {
            for (var i = 0; i < stash.Count; i++)
            {
                if (stash[i].Name == entry.Name)
                {
                    stash[i] = new SavedItemEntry { Name = entry.Name, Count = stash[i].Count + entry.Count };
                    return;
                }
            }

            stash.Add(entry);
        }
    }
}
