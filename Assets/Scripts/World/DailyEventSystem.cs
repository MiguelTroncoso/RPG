using System;
using System.Globalization;
using UnityEngine;

namespace MmorpgPrototype
{
    // Evento diario local reproducible: se activa al entrar al juego cada dia
    // y queda listo para pasar a autoridad del servidor en la fase online.
    public sealed class DailyEventSystem : MonoBehaviour
    {
        public PlayerProgression Progression;
        public CosmeticService Cosmetics;
        public PlayerPersistence Persistence;
        public PrototypeHud Hud;

        public const int TargetDefeats = 5;
        public const string EventId = "daily_relic_hunt";

        public string CurrentDate { get; private set; } = string.Empty;
        public int Progress { get; private set; }
        public bool Completed { get; private set; }

        public void Initialize()
        {
            EnsureToday();
            Hud?.SetStatus(Localization.Tr("event.daily_started"), 4f);
        }

        public void RestoreState(string date, int progress, bool completed)
        {
            CurrentDate = date;
            Progress = Mathf.Clamp(progress, 0, TargetDefeats);
            Completed = completed;
            EnsureToday();
        }

        public void RecordDefeat(EnemyTier tier, bool isWorldEvent)
        {
            EnsureToday();
            if (Completed)
            {
                return;
            }

            Progress = Mathf.Min(TargetDefeats, Progress + 1);
            if (Progress >= TargetDefeats)
            {
                Completed = true;
                Progression?.AddExperience(100);
                Progression?.AddGold(120);
                Cosmetics?.UnlockCosmetic(DefaultCosmetics.EmberWings);
                Hud?.SetStatus(Localization.Tr("event.daily_completed"), 5f);
                Hud?.AddFeed(Localization.Tr("event.daily_reward"));
            }
            else
            {
                Hud?.SetStatus(Localization.Tr("event.daily_progress", Progress, TargetDefeats), 2.5f);
            }

            Persistence?.SaveNow();
        }

        public string Summary()
        {
            return Localization.Tr(Completed ? "event.daily_done_summary" : "event.daily_summary", Progress, TargetDefeats);
        }

        private void EnsureToday()
        {
            var today = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (string.IsNullOrEmpty(CurrentDate))
            {
                CurrentDate = today;
                return;
            }

            if (!string.Equals(CurrentDate, today, StringComparison.Ordinal))
            {
                CurrentDate = today;
                Progress = 0;
                Completed = false;
            }
        }
    }
}
