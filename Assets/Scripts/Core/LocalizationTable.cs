using System;
using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Tabla de textos visibles al jugador. El idioma base es espanol; los
    // sistemas de juego solo conocen claves.
    [CreateAssetMenu(menuName = "MMORPG/Localization/Localization Table", fileName = "LocalizationTable")]
    public sealed class LocalizationTable : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string Key;
            [TextArea] public string Value;
        }

        public string LanguageCode = "es";
        public List<Entry> Entries = new List<Entry>();

        public bool HasEntries => Entries != null && Entries.Count > 0;

        public Dictionary<string, string> ToDictionary()
        {
            var table = new Dictionary<string, string>();
            if (Entries != null)
            {
                foreach (var entry in Entries)
                {
                    if (!string.IsNullOrWhiteSpace(entry.Key) && !table.ContainsKey(entry.Key))
                    {
                        table[entry.Key] = entry.Value;
                    }
                }
            }

            return table;
        }

        public void FillFrom(Dictionary<string, string> source)
        {
            Entries = new List<Entry>();
            foreach (var pair in source)
            {
                Entries.Add(new Entry { Key = pair.Key, Value = pair.Value });
            }

            Entries.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
        }
    }
}
