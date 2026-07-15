using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace MmorpgPrototype
{
    public enum RepeatableContractType
    {
        KillNormal,
        KillElite,
        CollectUpgradeMaterial
    }

    [Serializable]
    public sealed class RepeatableContractEntry
    {
        public string Id = string.Empty;
        public string ZoneId = string.Empty;
        public string Title = string.Empty;
        public string TargetId = string.Empty;
        public RepeatableContractType Type;
        public int Required;
        public int Progress;
        public int Experience;
        public int Gold;
        public string RewardItemId = string.Empty;
        public int RewardItemCount;
        public bool Completed;
    }

    [Serializable]
    public sealed class RepeatableContractSaveData
    {
        public string DateUtc = string.Empty;
        public string ZoneId = string.Empty;
        public List<RepeatableContractEntry> Entries = new List<RepeatableContractEntry>();
    }

    // Contratos diarios que mantienen actividad despues de completar la
    // campana. Se generan de forma determinista y estan listos para pasar a
    // autoridad del servidor junto con el resto de recompensas.
    public sealed class RepeatableContractSystem : MonoBehaviour
    {
        public const int ContractsPerDay = 3;

        public PlayerProgression Progression;
        public InventorySystem Inventory;
        public PrototypeHud Hud;
        public PlayerPersistence Persistence;

        public string CurrentDateUtc { get; private set; } = string.Empty;
        public int CompletedCount
        {
            get
            {
                var count = 0;
                foreach (var contract in contracts)
                {
                    if (contract != null && contract.Completed)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        private readonly List<ZoneDefinition> zones = new List<ZoneDefinition>();
        private readonly List<RepeatableContractEntry> contracts = new List<RepeatableContractEntry>();
        private string generatedZoneId = string.Empty;

        public void Initialize(IList<ZoneDefinition> sourceZones)
        {
            zones.Clear();
            if (sourceZones != null)
            {
                foreach (var zone in sourceZones)
                {
                    if (zone != null)
                    {
                        zones.Add(zone);
                    }
                }
            }

            EnsureContracts();
        }

        public void Restore(RepeatableContractSaveData data)
        {
            contracts.Clear();
            CurrentDateUtc = data != null ? data.DateUtc : string.Empty;
            generatedZoneId = data != null ? data.ZoneId : string.Empty;

            if (data != null && data.Entries != null)
            {
                foreach (var saved in data.Entries)
                {
                    if (saved != null && !string.IsNullOrWhiteSpace(saved.Id))
                    {
                        saved.Progress = Mathf.Clamp(saved.Progress, 0, Mathf.Max(1, saved.Required));
                        saved.Required = Mathf.Max(1, saved.Required);
                        contracts.Add(saved);
                    }
                }
            }

            EnsureContracts();
            Hud?.RefreshQuest();
        }

        public RepeatableContractSaveData Export()
        {
            var data = new RepeatableContractSaveData
            {
                DateUtc = CurrentDateUtc,
                ZoneId = generatedZoneId
            };

            foreach (var contract in contracts)
            {
                if (contract == null)
                {
                    continue;
                }

                data.Entries.Add(new RepeatableContractEntry
                {
                    Id = contract.Id,
                    ZoneId = contract.ZoneId,
                    Title = contract.Title,
                    TargetId = contract.TargetId,
                    Type = contract.Type,
                    Required = contract.Required,
                    Progress = contract.Progress,
                    Experience = contract.Experience,
                    Gold = contract.Gold,
                    RewardItemId = contract.RewardItemId,
                    RewardItemCount = contract.RewardItemCount,
                    Completed = contract.Completed
                });
            }

            return data;
        }

        public void OnEnemyDefeated(EnemyTier tier, string enemyId)
        {
            EnsureContracts();
            foreach (var contract in contracts)
            {
                if (contract == null || contract.Completed || contract.TargetId != enemyId)
                {
                    continue;
                }

                var matches = contract.Type == RepeatableContractType.KillNormal && tier == EnemyTier.Normal
                    || contract.Type == RepeatableContractType.KillElite && tier == EnemyTier.Elite;
                if (matches)
                {
                    Advance(contract, 1);
                }
            }
        }

        public void OnItemAdded(string itemId, int amount)
        {
            EnsureContracts();
            if (amount <= 0)
            {
                return;
            }

            foreach (var contract in contracts)
            {
                if (contract != null && !contract.Completed
                    && contract.Type == RepeatableContractType.CollectUpgradeMaterial
                    && contract.TargetId == itemId)
                {
                    Advance(contract, amount);
                }
            }
        }

        public void OnRebirth()
        {
            CurrentDateUtc = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            GenerateContracts();
            Hud?.RefreshQuest();
            Persistence?.SaveNow();
        }

        public string Summary()
        {
            EnsureContracts();
            var builder = new StringBuilder();
            builder.Append(Localization.Tr("contract.summary", CompletedCount, contracts.Count));
            foreach (var contract in contracts)
            {
                if (contract == null)
                {
                    continue;
                }

                builder.Append("\n");
                builder.Append(contract.Completed
                    ? Localization.Tr("contract.ok", contract.Title)
                    : Localization.Tr("contract.pending", contract.Title, contract.Progress, contract.Required));
            }

            return builder.ToString();
        }

        private void EnsureContracts()
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var zoneId = CurrentZone()?.ZoneId ?? string.Empty;
            if (contracts.Count != ContractsPerDay
                || CurrentDateUtc != today
                || generatedZoneId != zoneId)
            {
                CurrentDateUtc = today;
                GenerateContracts();
            }
        }

        private void GenerateContracts()
        {
            contracts.Clear();
            var zone = CurrentZone();
            if (zone == null)
            {
                return;
            }

            generatedZoneId = zone.ZoneId;
            var rebirth = Progression != null ? Progression.RebirthCount : 0;
            var prefix = $"contract-{CurrentDateUtc}-{zone.ZoneId}-r{rebirth}";
            var rewardMultiplier = Mathf.Max(1, zone.MinLevel);

            contracts.Add(new RepeatableContractEntry
            {
                Id = $"{prefix}-normal",
                ZoneId = zone.ZoneId,
                Title = Localization.Tr("contract.normal_title", zone.NormalName),
                TargetId = zone.NormalEnemyId,
                Type = RepeatableContractType.KillNormal,
                Required = 8 + Mathf.Min(6, zone.MinLevel / 20),
                Experience = 100 + rewardMultiplier * 35,
                Gold = 50 + rewardMultiplier * 12,
                RewardItemId = ProgressionItemCatalog.CommonMaterialFor(zone.ZoneId),
                RewardItemCount = 2
            });
            contracts.Add(new RepeatableContractEntry
            {
                Id = $"{prefix}-elite",
                ZoneId = zone.ZoneId,
                Title = Localization.Tr("contract.elite_title", zone.EliteName),
                TargetId = zone.EliteEnemyId,
                Type = RepeatableContractType.KillElite,
                Required = 2,
                Experience = 180 + rewardMultiplier * 55,
                Gold = 100 + rewardMultiplier * 20,
                RewardItemId = DefaultGameItems.ProtectionRune,
                RewardItemCount = 1
            });
            contracts.Add(new RepeatableContractEntry
            {
                Id = $"{prefix}-material",
                ZoneId = zone.ZoneId,
                Title = Localization.Tr("contract.material_title", zone.DisplayName),
                TargetId = ProgressionItemCatalog.UpgradeMaterialFor(zone.ZoneId),
                Type = RepeatableContractType.CollectUpgradeMaterial,
                Required = 3,
                Experience = 140 + rewardMultiplier * 45,
                Gold = 80 + rewardMultiplier * 16,
                RewardItemId = DefaultGameItems.SkillTome,
                RewardItemCount = 1
            });
        }

        private ZoneDefinition CurrentZone()
        {
            if (zones.Count == 0)
            {
                return null;
            }

            var level = Progression != null ? Progression.Level : 1;
            foreach (var zone in zones)
            {
                if (level >= zone.MinLevel && level <= zone.MaxLevel)
                {
                    return zone;
                }
            }

            return level < zones[0].MinLevel ? zones[0] : zones[zones.Count - 1];
        }

        private void Advance(RepeatableContractEntry contract, int amount)
        {
            contract.Progress = Mathf.Min(contract.Required, contract.Progress + amount);
            Hud?.AddFeed(Localization.Tr("contract.progress", contract.Title, contract.Progress, contract.Required));
            if (contract.Progress < contract.Required)
            {
                Hud?.RefreshQuest();
                return;
            }

            contract.Completed = true;
            Progression?.AddExperience(contract.Experience);
            Progression?.AddGold(contract.Gold);
            if (!string.IsNullOrWhiteSpace(contract.RewardItemId) && contract.RewardItemCount > 0)
            {
                Inventory?.AddItem(contract.RewardItemId, contract.RewardItemCount);
            }

            Hud?.SetStatus(Localization.Tr("contract.complete", contract.Title), 4f);
            Hud?.AddFeed(Localization.Tr("contract.complete", contract.Title));
            Hud?.RefreshQuest();
            Persistence?.SaveNow();
        }
    }
}
