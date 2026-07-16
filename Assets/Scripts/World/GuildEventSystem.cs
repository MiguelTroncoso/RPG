using System;
using System.Globalization;
using UnityEngine;

namespace MmorpgPrototype
{
    [Serializable]
    public sealed class GuildEventSaveData
    {
        public string WeekStartUtc = string.Empty;
        public string GuildId = string.Empty;
        public string GuildName = string.Empty;
        public int Contribution;
        public bool Completed;
    }

    // Base local para mostrar reglas y contribucion. La pertenencia, miembros,
    // ranking y recompensa final deben validarse en el servidor.
    public sealed class GuildEventSystem : MonoBehaviour
    {
        public const int TargetContribution = 50;

        public PrototypeHud Hud;
        public PlayerPersistence Persistence;
        public string GuildId { get; private set; } = string.Empty;
        public string GuildName { get; private set; } = string.Empty;
        public int Contribution { get; private set; }
        public bool Completed { get; private set; }
        public string WeekStartUtc { get; private set; } = string.Empty;
        public bool HasGuild => !string.IsNullOrEmpty(GuildId);

        public void Initialize()
        {
            EnsureCurrentWeek();
        }

        public void Restore(GuildEventSaveData data)
        {
            WeekStartUtc = data != null ? data.WeekStartUtc : string.Empty;
            GuildId = data != null ? data.GuildId : string.Empty;
            GuildName = data != null ? data.GuildName : string.Empty;
            Contribution = data != null ? Mathf.Clamp(data.Contribution, 0, TargetContribution) : 0;
            Completed = data != null && data.Completed;
            EnsureCurrentWeek();
        }

        public GuildEventSaveData Export()
        {
            return new GuildEventSaveData
            {
                WeekStartUtc = WeekStartUtc,
                GuildId = GuildId,
                GuildName = GuildName,
                Contribution = Contribution,
                Completed = Completed
            };
        }

        public void ConfigureGuild(string guildId, string guildName)
        {
            GuildId = guildId ?? string.Empty;
            GuildName = guildName ?? string.Empty;
            EnsureCurrentWeek();
            Persistence?.SaveNow();
        }

        public void RecordContribution(int points)
        {
            EnsureCurrentWeek();
            if (!HasGuild || Completed || points <= 0)
            {
                return;
            }

            Contribution = Mathf.Min(TargetContribution, Contribution + points);
            if (Contribution >= TargetContribution)
            {
                Completed = true;
                Hud?.SetStatus(Localization.Tr("event.guild_completed"), 5f);
                Hud?.AddFeed(Localization.Tr("event.guild_reward"));
            }

            Hud?.RefreshQuest();
            Persistence?.SaveNow();
        }

        public string Summary()
        {
            EnsureCurrentWeek();
            if (!HasGuild)
            {
                return Localization.Tr("event.guild_unassigned");
            }

            return Localization.Tr(Completed ? "event.guild_done" : "event.guild_summary", GuildName, Contribution, TargetContribution);
        }

        private void EnsureCurrentWeek()
        {
            var today = DateTime.UtcNow.Date;
            var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
            var current = today.AddDays(-daysSinceMonday).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (string.IsNullOrEmpty(WeekStartUtc))
            {
                WeekStartUtc = current;
                return;
            }

            if (!string.Equals(WeekStartUtc, current, StringComparison.Ordinal))
            {
                WeekStartUtc = current;
                Contribution = 0;
                Completed = false;
            }
        }
    }
}
