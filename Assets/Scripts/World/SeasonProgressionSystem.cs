using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace MmorpgPrototype
{
    [Serializable]
    public sealed class SeasonSaveData
    {
        public string SeasonId = string.Empty;
        public int Experience;
        public int Level = 1;
        public List<int> ClaimedRewardLevels = new List<int>();
    }

    // A compact 28-day reward track. It is intentionally local for now and
    // exposes a single API for a future server-authoritative season service.
    public sealed class SeasonProgressionSystem : MonoBehaviour
    {
        public const int MaxSeasonLevel = 30;
        public const int SeasonLengthDays = 28;
        private static readonly DateTime AnchorUtc = new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc);

        public PlayerProgression Progression;
        public InventorySystem Inventory;
        public CosmeticService Cosmetics;
        public PlayerPersistence Persistence;
        public PrototypeHud Hud;

        public string SeasonId { get; private set; } = string.Empty;
        public int SeasonNumber { get; private set; }
        public int Experience { get; private set; }
        public int Level { get; private set; } = 1;
        public int DaysRemaining { get; private set; }
        public string SeasonName => Localization.Tr("season.name", SeasonNumber);
        public int ExperienceToNext => Level >= MaxSeasonLevel ? 0 : ExperienceToNextLevel(Level);

        private readonly List<int> claimedRewardLevels = new List<int>();

        public void Initialize()
        {
            EnsureCurrentSeason();
            Hud?.SetStatus(Localization.Tr("season.started", SeasonName, DaysRemaining), 4f);
        }

        public void Restore(SeasonSaveData data)
        {
            SeasonId = data != null ? data.SeasonId : string.Empty;
            Experience = data != null ? Mathf.Max(0, data.Experience) : 0;
            Level = data != null ? Mathf.Clamp(data.Level, 1, MaxSeasonLevel) : 1;
            claimedRewardLevels.Clear();
            if (data != null && data.ClaimedRewardLevels != null)
            {
                claimedRewardLevels.AddRange(data.ClaimedRewardLevels);
            }

            EnsureCurrentSeason();
            Hud?.RefreshQuest();
        }

        public SeasonSaveData Export()
        {
            return new SeasonSaveData
            {
                SeasonId = SeasonId,
                Experience = Experience,
                Level = Level,
                ClaimedRewardLevels = new List<int>(claimedRewardLevels)
            };
        }

        public void RecordDefeat(EnemyTier tier, bool isWorldEvent)
        {
            var amount = isWorldEvent
                ? 50
                : tier == EnemyTier.Boss ? 50 : tier == EnemyTier.Elite ? 25 : 10;
            AddExperience(amount);
        }

        public void RecordQuestCompletion()
        {
            AddExperience(75);
        }

        public void RecordContractCompletion()
        {
            AddExperience(40);
        }

        public void RecordWeeklyCompletion()
        {
            AddExperience(250);
        }

        public string Summary()
        {
            EnsureCurrentSeason();
            return Localization.Tr("season.summary", SeasonName, Level, MaxSeasonLevel, Experience, ExperienceToNext, DaysRemaining);
        }

        public void AddExperience(int amount)
        {
            EnsureCurrentSeason();
            if (amount <= 0 || Level >= MaxSeasonLevel)
            {
                return;
            }

            Experience += amount;
            var leveledUp = false;
            while (Level < MaxSeasonLevel && Experience >= ExperienceToNextLevel(Level))
            {
                Experience -= ExperienceToNextLevel(Level);
                Level++;
                leveledUp = true;
                GrantMilestoneReward(Level);
            }

            if (leveledUp)
            {
                Hud?.SetStatus(Localization.Tr("season.level_up", SeasonName, Level), 4f);
                Hud?.RefreshQuest();
                Persistence?.SaveNow();
            }
        }

        private void GrantMilestoneReward(int level)
        {
            if (level % 5 != 0 || claimedRewardLevels.Contains(level))
            {
                return;
            }

            claimedRewardLevels.Add(level);
            var gold = level * 120;
            Progression?.AddGold(gold);
            var rewardId = level % 10 == 0 ? DefaultGameItems.SkillTome : DefaultGameItems.ProtectionRune;
            var rewardCount = level >= 20 ? 2 : 1;
            Inventory?.AddItem(rewardId, rewardCount);

            var rewardText = Localization.Tr("season.reward", level, rewardCount, Inventory != null ? Inventory.DisplayNameOf(rewardId) : rewardId, gold);
            Hud?.AddFeed(rewardText);
            if (level == MaxSeasonLevel)
            {
                var alreadyOwned = Cosmetics != null && Cosmetics.IsOwned(DefaultCosmetics.VoidOutfit);
                Cosmetics?.UnlockCosmetic(DefaultCosmetics.VoidOutfit, false);
                if (!alreadyOwned)
                {
                    Hud?.AddFeed(Localization.Tr("season.final_reward"));
                }
            }
        }

        private void EnsureCurrentSeason()
        {
            var now = DateTime.UtcNow;
            var elapsedDays = Mathf.Max(0, (int)(now.Date - AnchorUtc.Date).TotalDays);
            var seasonIndex = elapsedDays / SeasonLengthDays;
            var currentSeasonId = $"season_{seasonIndex + 1:00}";
            SeasonNumber = seasonIndex + 1;
            DaysRemaining = SeasonLengthDays - (elapsedDays % SeasonLengthDays);

            if (string.IsNullOrEmpty(SeasonId))
            {
                SeasonId = currentSeasonId;
                return;
            }

            if (!string.Equals(SeasonId, currentSeasonId, StringComparison.Ordinal))
            {
                SeasonId = currentSeasonId;
                Experience = 0;
                Level = 1;
                claimedRewardLevels.Clear();
            }
        }

        private static int ExperienceToNextLevel(int level)
        {
            return 80 + level * 20;
        }
    }
}
