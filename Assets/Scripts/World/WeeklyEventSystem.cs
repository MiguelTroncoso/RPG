using System;
using System.Globalization;
using UnityEngine;

namespace MmorpgPrototype
{
    [Serializable]
    public sealed class WeeklyEventSaveData
    {
        public string WeekStartUtc = string.Empty;
        public int Defeats;
        public int EliteDefeats;
        public bool Completed;
    }

    // Weekly activity with a stable UTC reset. The server can later validate
    // the same week key and reward claims without changing the client API.
    public sealed class WeeklyEventSystem : MonoBehaviour
    {
        public const int TargetDefeats = 30;
        public const int TargetEliteDefeats = 3;
        public const string EventId = "weekly_valley_conquest";

        public PlayerProgression Progression;
        public InventorySystem Inventory;
        public SeasonProgressionSystem Season;
        public PlayerPersistence Persistence;
        public PrototypeHud Hud;

        public string WeekStartUtc { get; private set; } = string.Empty;
        public int Defeats { get; private set; }
        public int EliteDefeats { get; private set; }
        public bool Completed { get; private set; }

        public void Initialize()
        {
            EnsureCurrentWeek();
            Hud?.SetStatus(Localization.Tr("event.weekly_started"), 4f);
        }

        public void Restore(WeeklyEventSaveData data)
        {
            WeekStartUtc = data != null ? data.WeekStartUtc : string.Empty;
            Defeats = data != null ? Mathf.Clamp(data.Defeats, 0, TargetDefeats) : 0;
            EliteDefeats = data != null ? Mathf.Clamp(data.EliteDefeats, 0, TargetEliteDefeats) : 0;
            Completed = data != null && data.Completed;
            EnsureCurrentWeek();
            Hud?.RefreshQuest();
        }

        public WeeklyEventSaveData Export()
        {
            return new WeeklyEventSaveData
            {
                WeekStartUtc = WeekStartUtc,
                Defeats = Defeats,
                EliteDefeats = EliteDefeats,
                Completed = Completed
            };
        }

        public void RecordDefeat(EnemyTier tier, bool isWorldEvent)
        {
            EnsureCurrentWeek();
            if (Completed)
            {
                return;
            }

            Defeats = Mathf.Min(TargetDefeats, Defeats + 1);
            if (tier == EnemyTier.Elite || tier == EnemyTier.Boss || isWorldEvent)
            {
                EliteDefeats = Mathf.Min(TargetEliteDefeats, EliteDefeats + 1);
            }

            if (Defeats >= TargetDefeats && EliteDefeats >= TargetEliteDefeats)
            {
                CompleteEvent();
                return;
            }

            Hud?.SetStatus(Localization.Tr("event.weekly_progress", Defeats, TargetDefeats, EliteDefeats, TargetEliteDefeats), 2.5f);
            Hud?.RefreshQuest();
            Persistence?.SaveNow();
        }

        public string Summary()
        {
            EnsureCurrentWeek();
            return Localization.Tr(
                Completed ? "event.weekly_done_summary" : "event.weekly_summary",
                Defeats,
                TargetDefeats,
                EliteDefeats,
                TargetEliteDefeats);
        }

        private void CompleteEvent()
        {
            Completed = true;
            Progression?.AddExperience(600);
            Progression?.AddGold(500);
            Inventory?.AddItem(DefaultGameItems.ProtectionRune, 2);
            Inventory?.AddItem(DefaultGameItems.SkillTome, 1);
            Season?.RecordWeeklyCompletion();
            Hud?.SetStatus(Localization.Tr("event.weekly_completed"), 5f);
            Hud?.AddFeed(Localization.Tr("event.weekly_reward"));
            Hud?.RefreshQuest();
            Persistence?.SaveNow();
        }

        private void EnsureCurrentWeek()
        {
            var currentWeek = CurrentWeekStartUtc();
            if (string.IsNullOrEmpty(WeekStartUtc))
            {
                WeekStartUtc = currentWeek;
                return;
            }

            if (!string.Equals(WeekStartUtc, currentWeek, StringComparison.Ordinal))
            {
                WeekStartUtc = currentWeek;
                Defeats = 0;
                EliteDefeats = 0;
                Completed = false;
            }
        }

        private static string CurrentWeekStartUtc()
        {
            var today = DateTime.UtcNow.Date;
            var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
            return today.AddDays(-daysSinceMonday).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
