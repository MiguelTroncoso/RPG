using System;
using System.Collections.Generic;

namespace MmorpgPrototype
{
    // Punto unico de resolucion de textos visibles. Si falta una clave se
    // devuelve la clave misma, para que el hueco sea evidente en pantalla.
    public static class Localization
    {
        private static Dictionary<string, string> table;

        public static void Initialize(LocalizationTable source)
        {
            table = DefaultLocalization.CreateSpanish();
            if (source == null || !source.HasEntries)
            {
                return;
            }

            foreach (var entry in source.ToDictionary())
            {
                table[entry.Key] = entry.Value;
            }
        }

        public static string Tr(string key)
        {
            if (table == null || table.Count == 0)
            {
                table = DefaultLocalization.CreateSpanish();
            }

            return !string.IsNullOrEmpty(key) && table.TryGetValue(key, out var value) ? value : key;
        }

        public static string Tr(string key, params object[] args)
        {
            var format = Tr(key);

            try
            {
                return string.Format(format, args);
            }
            catch (FormatException)
            {
                return format;
            }
        }
    }
}
