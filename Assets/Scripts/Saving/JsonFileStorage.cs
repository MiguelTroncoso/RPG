using System;
using System.IO;
using UnityEngine;

namespace MmorpgPrototype
{
    // Guardado local en archivos JSON con escritura atomica (tmp + swap) y
    // respaldo del ultimo guardado valido, pensado para mobile donde la app
    // puede morir en medio de una escritura.
    public sealed class JsonFileStorage : ISaveStorage
    {
        private readonly string directory;

        public JsonFileStorage(string directory)
        {
            this.directory = directory;
        }

        public bool Exists(string slot)
        {
            return File.Exists(MainPath(slot)) || File.Exists(BackupPath(slot));
        }

        public void Save(string slot, string json)
        {
            try
            {
                Directory.CreateDirectory(directory);

                var main = MainPath(slot);
                var backup = BackupPath(slot);
                var temp = main + ".tmp";

                File.WriteAllText(temp, json);

                if (File.Exists(main))
                {
                    DeleteIfExists(backup);
                    File.Move(main, backup);
                }

                File.Move(temp, main);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"JsonFileStorage: no se pudo guardar '{slot}': {exception.Message}");
            }
        }

        public string Load(string slot)
        {
            var content = ReadOrNull(MainPath(slot));
            return string.IsNullOrEmpty(content) ? ReadOrNull(BackupPath(slot)) : content;
        }

        public void Delete(string slot)
        {
            DeleteIfExists(MainPath(slot) + ".tmp");
            DeleteIfExists(MainPath(slot));
            DeleteIfExists(BackupPath(slot));
        }

        private string MainPath(string slot)
        {
            return Path.Combine(directory, slot + ".json");
        }

        private string BackupPath(string slot)
        {
            return Path.Combine(directory, slot + ".json.bak");
        }

        private static string ReadOrNull(string path)
        {
            try
            {
                return File.Exists(path) ? File.ReadAllText(path) : null;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"JsonFileStorage: no se pudo leer '{path}': {exception.Message}");
                return null;
            }
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
