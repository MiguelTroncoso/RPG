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
        public PlayerEquipment Gear;
        public PlayerQuestLog QuestLog;
        public PetService Pets;
        public MountService Mounts;
        public CosmeticService Cosmetics;
        public DailyEventSystem DailyEvents;
        public StorageService Storage;
        public PlayerAttributes Attributes;
        public PlayerSkills Skills;
        public RepeatableContractSystem Contracts;
        public WeeklyEventSystem WeeklyEvent;
        public SeasonProgressionSystem Season;

        // Evita sobreescribir un guardado real con los valores por defecto
        // mientras el panel de creacion sigue abierto.
        public bool HasActiveCharacter { get; private set; }

        private ISaveStorage storage;
        private float autoSaveTimer;
        private readonly HashSet<string> claimedPointOfInterestIds = new HashSet<string>();

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

        public string ExportJson()
        {
            var data = Capture();
            return data != null ? JsonUtility.ToJson(data) : string.Empty;
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
            Progression?.RestoreRebirthState(data.RebirthCount, data.Renown);
            Skills?.RestoreLevels(data.SkillLevels);
            Skills?.RestoreUltimateCooldown(data.UltimateReadyAtUtcTicks);
            Attributes?.Restore(data.SpentStrength, data.SpentVitality, data.SpentAgility);
            Inventory?.RestoreEntries(data.Items);
            Gear?.RestoreEntries(data.Equipment);
            Equipment?.RestoreUpgrades(data.WeaponLevel, data.ArmorLevel);
            QuestLog?.Restore(data.Quests);
            Contracts?.Restore(data.Contracts);
            WeeklyEvent?.Restore(data.WeeklyEvent);
            Season?.Restore(data.Season);
            Storage?.RestoreEntries(data.Storage);

            Cosmetics?.RestoreOwned(data.OwnedCosmeticIds);
            Cosmetics?.RestoreActive(data.ActiveOutfitId, data.ActiveWingsId);
            Pets?.RestoreOwned(data.OwnedPetIds);
            Mounts?.RestoreOwned(data.OwnedMountIds);

            claimedPointOfInterestIds.Clear();
            if (data.ClaimedPointOfInterestIds != null)
            {
                foreach (var pointOfInterestId in data.ClaimedPointOfInterestIds)
                {
                    if (!string.IsNullOrWhiteSpace(pointOfInterestId))
                    {
                        claimedPointOfInterestIds.Add(pointOfInterestId);
                    }
                }
            }

            if (Mounts != null && !string.IsNullOrEmpty(data.SelectedMountId))
            {
                Mounts.SelectMount(data.SelectedMountId);
            }

            if (Pets != null && !string.IsNullOrEmpty(data.ActivePetId))
            {
                Pets.Summon(data.ActivePetId);
            }

            DailyEvents?.RestoreState(data.DailyEventDate, data.DailyEventProgress, data.DailyEventCompleted);

            if (data.HasPosition)
            {
                RestorePosition(new Vector3(data.PosX, data.PosY, data.PosZ), data.Yaw);
            }

            HasActiveCharacter = true;
            autoSaveTimer = 0f;
            SaveNow();
        }

        // El CharacterController pisa los cambios directos de transform:
        // hay que apagarlo para teletransportar.
        private void RestorePosition(Vector3 position, float yaw)
        {
            if (float.IsNaN(position.x) || float.IsInfinity(position.x) ||
                float.IsNaN(position.y) || float.IsInfinity(position.y) ||
                float.IsNaN(position.z) || float.IsInfinity(position.z))
            {
                position = new Vector3(0f, 1f, 0f);
            }

            // A previous build could save while the player was falling below
            // the training field. Clamp legacy saves to the spawn floor.
            position.y = Mathf.Max(1f, position.y);

            var controller = GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            transform.position = position;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            if (controller != null)
            {
                controller.enabled = true;
            }
        }

        public void DeleteSave()
        {
            storage?.Delete(SlotName);
            claimedPointOfInterestIds.Clear();
            HasActiveCharacter = false;
        }

        public bool HasClaimedPointOfInterest(string claimId)
        {
            return !string.IsNullOrWhiteSpace(claimId) && claimedPointOfInterestIds.Contains(claimId);
        }

        public void MarkPointOfInterestClaimed(string claimId)
        {
            if (string.IsNullOrWhiteSpace(claimId))
            {
                return;
            }

            if (claimedPointOfInterestIds.Add(claimId))
            {
                SaveNow();
            }
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
                RebirthCount = Progression.RebirthCount,
                Renown = Progression.Renown,
                SkillLevels = Skills != null ? Skills.ExportLevels() : new List<int>(),
                UltimateReadyAtUtcTicks = Skills != null ? Skills.ExportUltimateReadyAtUtcTicks() : 0L,
                SpentStrength = Attributes != null ? Attributes.Strength : 0,
                SpentVitality = Attributes != null ? Attributes.Vitality : 0,
                SpentAgility = Attributes != null ? Attributes.Agility : 0,
                WeaponLevel = Equipment != null ? Equipment.WeaponLevel : 0,
                ArmorLevel = Equipment != null ? Equipment.ArmorLevel : 0,
                Items = Inventory != null ? Inventory.ExportEntries() : new List<SavedItemEntry>(),
                Equipment = Gear != null ? Gear.ExportEntries() : new List<SavedEquipmentEntry>(),
                Quests = QuestLog != null ? QuestLog.Export() : new QuestSaveData(),
                Contracts = Contracts != null ? Contracts.Export() : new RepeatableContractSaveData(),
                WeeklyEvent = WeeklyEvent != null ? WeeklyEvent.Export() : new WeeklyEventSaveData(),
                Season = Season != null ? Season.Export() : new SeasonSaveData(),
                ActivePetId = Pets != null ? Pets.ActivePetId : string.Empty,
                SelectedMountId = Mounts != null ? Mounts.SelectedMountId : string.Empty,
                ActiveOutfitId = Cosmetics != null ? Cosmetics.ActiveOutfitId : string.Empty,
                ActiveWingsId = Cosmetics != null ? Cosmetics.ActiveWingsId : string.Empty,
                OwnedCosmeticIds = Cosmetics != null ? Cosmetics.ExportOwnedIds() : new List<string>(),
                OwnedPetIds = Pets != null ? Pets.ExportOwnedIds() : new List<string>(),
                OwnedMountIds = Mounts != null ? Mounts.ExportOwnedIds() : new List<string>(),
                DailyEventDate = DailyEvents != null ? DailyEvents.CurrentDate : string.Empty,
                DailyEventProgress = DailyEvents != null ? DailyEvents.Progress : 0,
                DailyEventCompleted = DailyEvents != null && DailyEvents.Completed,
                Storage = Storage != null ? Storage.ExportEntries() : new List<SavedItemEntry>(),
                HasPosition = true,
                PosX = transform.position.x,
                PosY = transform.position.y,
                PosZ = transform.position.z,
                Yaw = transform.eulerAngles.y,
                ClaimedPointOfInterestIds = new List<string>(claimedPointOfInterestIds)
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
