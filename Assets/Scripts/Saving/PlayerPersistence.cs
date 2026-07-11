using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MmorpgPrototype
{
    // Persistencia local MVP: nombre, clase, sexo, nivel, EXP, oro, inventario
    // y niveles de mejora. Autosave periodico + al pausar/cerrar la app.
    // El almacenamiento es intercambiable via ISaveStorage para migrar a
    // servidor mas adelante.
    public sealed class PlayerPersistence : MonoBehaviour
    {
        private const string SlotName = "player";
        private const float AutoSaveIntervalSeconds = 30f;

        public PlayerCharacterIdentity Identity;
        public PlayerClassController ClassController;
        public PlayerProgression Progression;
        public InventorySystem Inventory;
        public EquipmentUpgradeSystem Equipment;

        // Evita sobreescribir un guardado real con los valores por defecto
        // mientras el panel de creacion sigue abierto.
        public bool HasActiveCharacter { get; private set; }

        private ISaveStorage storage;
        private float autoSaveTimer;

        private void Awake()
        {
            storage = new JsonFileStorage(Path.Combine(Application.persistentDataPath, "saves"));
        }

        private void Update()
        {
            if (!HasActiveCharacter)
            {
                return;
            }

            autoSaveTimer += Time.unscaledDeltaTime;
            if (autoSaveTimer >= AutoSaveIntervalSeconds)
            {
                autoSaveTimer = 0f;
                SaveNow();
            }
        }

        public PlayerSaveData LoadOrNull()
        {
            if (storage == null || !storage.Exists(SlotName))
            {
                return null;
            }

            var json = storage.Load(SlotName);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                var data = JsonUtility.FromJson<PlayerSaveData>(json);
                return data != null && !string.IsNullOrWhiteSpace(data.CharacterName) ? data : null;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"PlayerPersistence: guardado ilegible, se ignora: {exception.Message}");
                return null;
            }
        }

        public void MarkCharacterActive()
        {
            HasActiveCharacter = true;
            autoSaveTimer = 0f;
            SaveNow();
        }

        public void SaveNow()
        {
            if (!HasActiveCharacter || storage == null)
            {
                return;
            }

            var data = Capture();
            if (data != null)
            {
                storage.Save(SlotName, JsonUtility.ToJson(data, true));
            }
        }

        public void ApplyLoadedData(PlayerSaveData data)
        {
            if (data == null)
            {
                return;
            }

            var gender = Enum.TryParse(data.GenderName, out CharacterGender parsedGender)
                ? parsedGender
                : CharacterGender.Masculino;
            var classType = Enum.TryParse(data.ClassName, out CharacterClassType parsedClass)
                ? parsedClass
                : CharacterClassType.Guerrero;

            Identity?.ApplySelection(data.CharacterName, gender);
            ClassController?.ApplyClass(classType);
            Progression?.RestoreState(data.Level, data.Experience, data.Gold, data.AttributePoints);
            Inventory?.RestoreEntries(data.Items);
            Equipment?.RestoreUpgrades(data.WeaponLevel, data.ArmorLevel);

            HasActiveCharacter = true;
            autoSaveTimer = 0f;
        }

        public void DeleteSave()
        {
            storage?.Delete(SlotName);
        }

        private PlayerSaveData Capture()
        {
            if (Identity == null || ClassController == null || Progression == null)
            {
                return null;
            }

            return new PlayerSaveData
            {
                CharacterName = Identity.CharacterName,
                ClassName = ClassController.CurrentClass.ToString(),
                GenderName = Identity.Gender.ToString(),
                Level = Progression.Level,
                Experience = Progression.Experience,
                Gold = Progression.Gold,
                AttributePoints = Progression.AttributePoints,
                WeaponLevel = Equipment != null ? Equipment.WeaponLevel : 0,
                ArmorLevel = Equipment != null ? Equipment.ArmorLevel : 0,
                Items = Inventory != null ? Inventory.ExportEntries() : new List<SavedItemEntry>()
            };
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                SaveNow();
            }
        }

        private void OnApplicationQuit()
        {
            SaveNow();
        }
    }
}
