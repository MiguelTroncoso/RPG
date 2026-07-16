using System;
using System.Globalization;
using UnityEngine;

namespace MmorpgPrototype
{
    [Serializable]
    public sealed class EventCalendarSaveData
    {
        public string DateUtc = string.Empty;
        public string EventId = string.Empty;
        public int Progress;
        public bool Completed;
        public string WorldBossDateUtc = string.Empty;
        public int WorldBossHourUtc;
        public int WorldBossMinuteUtc;
        public bool WorldBossDefeated;
    }

    // Rotacion diaria de actividades. Las horas del jefe son UTC y se calculan
    // de forma determinista para que cliente y servidor puedan coincidir.
    public sealed class EventCalendarSystem : MonoBehaviour
    {
        public const int WorldBossWindowMinutes = 60;
        public const string DailyWorldBossId = "daily_world_boss";

        private static readonly int[] bossHours = { 18, 20, 17, 21, 19, 16, 20 };
        private static readonly int[] bossMinutes = { 0, 30, 30, 0, 30, 0, 30 };

        public PlayerProgression Progression;
        public InventorySystem Inventory;
        public PlayerPersistence Persistence;
        public PrototypeHud Hud;
        public GuildEventSystem GuildEvents;

        public string CurrentDateUtc { get; private set; } = string.Empty;
        public string ActiveEventId { get; private set; } = string.Empty;
        public int Progress { get; private set; }
        public int Target { get; private set; }
        public bool Completed { get; private set; }
        public string WorldBossDateUtc { get; private set; } = string.Empty;
        public int WorldBossHourUtc { get; private set; }
        public int WorldBossMinuteUtc { get; private set; }
        public bool WorldBossDefeated { get; private set; }

        public void Initialize()
        {
            EnsureToday();
            Hud?.SetStatus(Localization.Tr("event.calendar_started"), 4f);
        }

        public void Restore(EventCalendarSaveData data)
        {
            CurrentDateUtc = data != null ? data.DateUtc : string.Empty;
            ActiveEventId = data != null ? data.EventId : string.Empty;
            Progress = data != null ? Mathf.Max(0, data.Progress) : 0;
            Completed = data != null && data.Completed;
            WorldBossDateUtc = data != null ? data.WorldBossDateUtc : string.Empty;
            WorldBossHourUtc = data != null ? data.WorldBossHourUtc : 0;
            WorldBossMinuteUtc = data != null ? data.WorldBossMinuteUtc : 0;
            WorldBossDefeated = data != null && data.WorldBossDefeated;
            Target = TargetFor(ActiveEventId);
            EnsureToday();
            Hud?.RefreshQuest();
        }

        public EventCalendarSaveData Export()
        {
            return new EventCalendarSaveData
            {
                DateUtc = CurrentDateUtc,
                EventId = ActiveEventId,
                Progress = Progress,
                Completed = Completed,
                WorldBossDateUtc = WorldBossDateUtc,
                WorldBossHourUtc = WorldBossHourUtc,
                WorldBossMinuteUtc = WorldBossMinuteUtc,
                WorldBossDefeated = WorldBossDefeated
            };
        }

        public void RecordDefeat(EnemyTier tier, bool isWorldEvent)
        {
            EnsureToday();
            GuildEvents?.RecordContribution(isWorldEvent ? 10 : tier == EnemyTier.Normal ? 1 : 3);

            if (isWorldEvent && IsWorldBossWindow())
            {
                CompleteWorldBoss();
            }

            if (Completed || ActiveEventId == "weekly_valley_conquest" ||
                ActiveEventId == "free_for_all" || ActiveEventId == "guild_war" ||
                ActiveEventId == "forge_festival")
            {
                return;
            }

            var counts = ActiveEventId == "elite_surge"
                ? tier == EnemyTier.Elite || tier == EnemyTier.Boss
                : ActiveEventId == "relic_rush" || ActiveEventId == "gold_rush";
            if (counts)
            {
                Advance(1);
            }
        }

        public void RecordUpgrade()
        {
            EnsureToday();
            if (!Completed && ActiveEventId == "forge_festival")
            {
                Advance(1);
            }
        }

        public bool IsWorldBossWindow()
        {
            EnsureToday();
            var start = WorldBossStartUtc();
            var end = start.AddMinutes(WorldBossWindowMinutes);
            var now = DateTime.UtcNow;
            return now >= start && now < end;
        }

        public string Summary()
        {
            EnsureToday();
            var activity = ActiveEventId == "weekly_valley_conquest"
                ? Localization.Tr("event.calendar_weekly")
                : Localization.Tr("event.calendar_activity", ActivityName(), Progress, Target);
            var bossState = WorldBossDefeated
                ? Localization.Tr("event.calendar_boss_done")
                : Localization.Tr("event.calendar_boss", WorldBossHourUtc, WorldBossMinuteUtc);
            return activity + "\n" + bossState;
        }

        private void CompleteWorldBoss()
        {
            if (WorldBossDefeated)
            {
                return;
            }

            WorldBossDefeated = true;
            Progression?.AddExperience(500);
            Progression?.AddGold(300);
            Inventory?.AddItem(DefaultGameItems.ProtectionRune, 1);
            Hud?.SetStatus(Localization.Tr("event.calendar_boss_completed"), 5f);
            Hud?.AddFeed(Localization.Tr("event.calendar_boss_reward"));
            Persistence?.SaveNow();
        }

        private void Advance(int amount)
        {
            Progress = Mathf.Min(Target, Progress + amount);
            if (Progress >= Target)
            {
                Completed = true;
                Progression?.AddExperience(250);
                Progression?.AddGold(220);
                Inventory?.AddItem(DefaultGameItems.DullOre, 3);
                Hud?.SetStatus(Localization.Tr("event.calendar_completed", ActivityName()), 5f);
                Hud?.AddFeed(Localization.Tr("event.calendar_reward"));
            }
            else
            {
                Hud?.SetStatus(Localization.Tr("event.calendar_progress", ActivityName(), Progress, Target), 2.5f);
            }

            Hud?.RefreshQuest();
            Persistence?.SaveNow();
        }

        private void EnsureToday()
        {
            var today = DateTime.UtcNow.Date;
            var date = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (string.Equals(CurrentDateUtc, date, StringComparison.Ordinal))
            {
                return;
            }

            CurrentDateUtc = date;
            ActiveEventId = EventForDay(today.DayOfWeek);
            Target = TargetFor(ActiveEventId);
            Progress = 0;
            Completed = false;
            WorldBossDateUtc = date;
            var index = (today.DayOfYear - 1) % bossHours.Length;
            WorldBossHourUtc = bossHours[index];
            WorldBossMinuteUtc = bossMinutes[index];
            WorldBossDefeated = false;
        }

        private string ActivityName()
        {
            return Localization.Tr("event.calendar_name_" + ActiveEventId);
        }

        private static string EventForDay(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return "weekly_valley_conquest";
                case DayOfWeek.Tuesday: return "elite_surge";
                case DayOfWeek.Wednesday: return "forge_festival";
                case DayOfWeek.Thursday: return "relic_rush";
                case DayOfWeek.Friday: return "gold_rush";
                case DayOfWeek.Saturday: return "guild_war";
                default: return "free_for_all";
            }
        }

        private static int TargetFor(string eventId)
        {
            switch (eventId)
            {
                case "elite_surge": return 8;
                case "forge_festival": return 3;
                case "relic_rush": return 15;
                case "gold_rush": return 20;
                default: return 1;
            }
        }

        private DateTime WorldBossStartUtc()
        {
            var date = DateTime.ParseExact(WorldBossDateUtc, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return date.AddHours(WorldBossHourUtc).AddMinutes(WorldBossMinuteUtc);
        }
    }
}
