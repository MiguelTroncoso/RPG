using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Registro ItemId -> ItemDefinition. Unica fuente de verdad para
    // resolver items en runtime.
    [CreateAssetMenu(menuName = "MMORPG/Items/Item Database", fileName = "ItemDatabase")]
    public sealed class ItemDatabase : ScriptableObject
    {
        public List<ItemDefinition> Items = new List<ItemDefinition>();
        public RarityTable Rarities;

        private Dictionary<string, ItemDefinition> lookup;

        public ItemDefinition Get(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            EnsureLookup();
            return lookup.TryGetValue(itemId, out var definition) ? definition : null;
        }

        public string DisplayNameOf(string itemId)
        {
            var definition = Get(itemId);
            return definition != null ? definition.DisplayName : itemId;
        }

        public Color ColorOf(string itemId)
        {
            var definition = Get(itemId);
            if (definition == null)
            {
                return Color.white;
            }

            return Rarities != null ? Rarities.GetColor(definition.Rarity) : RarityTable.DefaultRow(definition.Rarity).UiColor;
        }

        public RarityTable.RarityRow RarityRowOf(string itemId)
        {
            var definition = Get(itemId);
            var tier = definition != null ? definition.Rarity : Rarity.Common;
            return Rarities != null ? Rarities.GetRow(tier) : RarityTable.DefaultRow(tier);
        }

        public List<string> Validate()
        {
            var problems = new List<string>();
            var seen = new HashSet<string>();

            foreach (var item in Items)
            {
                if (item == null)
                {
                    problems.Add("Entrada nula en la lista de items.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.ItemId))
                {
                    problems.Add($"'{item.name}' no tiene ItemId.");
                }
                else if (!seen.Add(item.ItemId))
                {
                    problems.Add($"ItemId duplicado: '{item.ItemId}'.");
                }
            }

            return problems;
        }

        public void RebuildLookup()
        {
            lookup = null;
            EnsureLookup();
        }

        private void EnsureLookup()
        {
            if (lookup != null)
            {
                return;
            }

            lookup = new Dictionary<string, ItemDefinition>();
            foreach (var item in Items)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.ItemId) && !lookup.ContainsKey(item.ItemId))
                {
                    lookup[item.ItemId] = item;
                }
            }
        }
    }
}
