using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class CombatTelemetry : MonoBehaviour
    {
        private const float SaveIntervalSeconds = 15f;
        private const float FeedIntervalSeconds = 45f;
        private const float NetworkIntervalSeconds = 45f;

        public Health PlayerHealth;
        public PrototypeHud Hud;
        public MmorpgNetworkClient Network;
        public event Action Changed;

        private readonly Dictionary<string, ZoneTelemetrySummary> zones = new Dictionary<string, ZoneTelemetrySummary>();
        private readonly Dictionary<Health, EnemyCombatSample> activeCombats = new Dictionary<Health, EnemyCombatSample>();
        private readonly List<ZoneDefinition> zoneDefinitions = new List<ZoneDefinition>();

        private string telemetryPath;
        private float nextSave;
        private float nextFeed;
        private float nextNetwork;
        private bool dirty;

        private void Awake()
        {
            telemetryPath = Path.Combine(Application.persistentDataPath, "telemetry", "combat-telemetry.json");
        }

        private void OnEnable()
        {
            if (PlayerHealth != null)
            {
                PlayerHealth.Damaged += HandlePlayerDamaged;
                PlayerHealth.Died += HandlePlayerDied;
            }
        }

        private void OnDisable()
        {
            if (PlayerHealth != null)
            {
                PlayerHealth.Damaged -= HandlePlayerDamaged;
                PlayerHealth.Died -= HandlePlayerDied;
            }
        }

        private void Update()
        {
            if (!dirty)
            {
                return;
            }

            if (Time.unscaledTime >= nextFeed)
            {
                nextFeed = Time.unscaledTime + FeedIntervalSeconds;
                Hud?.AddFeed(SummaryLine());
            }

            if (Time.unscaledTime >= nextNetwork)
            {
                nextNetwork = Time.unscaledTime + NetworkIntervalSeconds;
                Network?.SendTelemetry(ToJson(pretty: false));
            }

            if (Time.unscaledTime >= nextSave)
            {
                SaveNow();
            }
        }

        public void Initialize(Health playerHealth, IEnumerable<ZoneDefinition> definitions)
        {
            if (PlayerHealth != null)
            {
                PlayerHealth.Damaged -= HandlePlayerDamaged;
                PlayerHealth.Died -= HandlePlayerDied;
            }

            PlayerHealth = playerHealth;
            zoneDefinitions.Clear();

            if (definitions != null)
            {
                zoneDefinitions.AddRange(definitions);
            }

            if (PlayerHealth != null && isActiveAndEnabled)
            {
                PlayerHealth.Damaged += HandlePlayerDamaged;
                PlayerHealth.Died += HandlePlayerDied;
            }
        }

        public void RecordEnemyDamaged(Health enemyHealth, EnemyTier tier, string enemyId, string zoneId, int amount)
        {
            if (enemyHealth == null || amount <= 0)
            {
                return;
            }

            var key = ZoneKey(zoneId);
            var summary = SummaryFor(key);
            summary.DamageDealt += amount;

            if (!activeCombats.ContainsKey(enemyHealth))
            {
                activeCombats[enemyHealth] = new EnemyCombatSample
                {
                    StartedAt = Time.unscaledTime,
                    Tier = tier,
                    EnemyId = enemyId ?? string.Empty,
                    ZoneId = key
                };
            }

            MarkDirty();
        }

        public void RecordEnemyDefeated(Health enemyHealth, EnemyTier tier, string enemyId, string zoneId)
        {
            var key = ZoneKey(zoneId);
            var summary = SummaryFor(key);
            summary.Kills++;

            switch (tier)
            {
                case EnemyTier.Elite:
                    summary.EliteKills++;
                    break;
                case EnemyTier.Boss:
                    summary.BossKills++;
                    break;
                default:
                    summary.NormalKills++;
                    break;
            }

            if (enemyHealth != null && activeCombats.TryGetValue(enemyHealth, out var sample))
            {
                var timeToKill = Mathf.Max(0f, Time.unscaledTime - sample.StartedAt);
                summary.TotalTimeToKill += timeToKill;
                switch (sample.Tier)
                {
                    case EnemyTier.Elite:
                        summary.EliteTotalTimeToKill += timeToKill;
                        break;
                    case EnemyTier.Boss:
                        summary.BossTotalTimeToKill += timeToKill;
                        break;
                    default:
                        summary.NormalTotalTimeToKill += timeToKill;
                        break;
                }

                activeCombats.Remove(enemyHealth);
            }

            MarkDirty();
        }

        public string ToJson(bool pretty)
        {
            return JsonUtility.ToJson(BuildSnapshot(), pretty);
        }

        public string BuildDisplayText()
        {
            var snapshot = BuildSnapshot();
            var builder = new StringBuilder();
            builder.AppendLine(Localization.Tr("telemetry.summary", snapshot.TotalKills, snapshot.TotalDeaths, snapshot.TotalDamageDealt, snapshot.TotalDamageTaken));
            var targetZone = zoneDefinitions.Count > 0 ? zoneDefinitions[0] : null;
            builder.AppendLine(Localization.Tr("telemetry.targets",
                targetZone != null ? targetZone.NormalTtkMin : 3f,
                targetZone != null ? targetZone.NormalTtkMax : 8f,
                targetZone != null ? targetZone.EliteTtkMin : 8f,
                targetZone != null ? targetZone.EliteTtkMax : 18f,
                targetZone != null ? targetZone.BossTtkMin : 30f,
                targetZone != null ? targetZone.BossTtkMax : 75f));

            if (zoneDefinitions.Count > 0)
            {
                foreach (var zone in zoneDefinitions)
                {
                    if (zone == null)
                    {
                        continue;
                    }

                    var summary = zones.TryGetValue(ZoneKey(zone.DisplayName), out var stored) ? stored : null;
                    builder.AppendLine(BuildZoneLine(zone, summary));
                }
            }
            else
            {
                foreach (var summary in snapshot.Zones)
                {
                    builder.AppendLine($"{summary.ZoneId}: {summary.Kills} kills");
                }
            }

            builder.AppendLine();
            builder.Append(Localization.Tr("telemetry.path", telemetryPath));
            return builder.ToString();
        }

        public void SaveNow()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(telemetryPath));
                File.WriteAllText(telemetryPath, ToJson(pretty: true));
                dirty = false;
                nextSave = Time.unscaledTime + SaveIntervalSeconds;
            }
            catch (Exception error)
            {
                Debug.LogWarning($"CombatTelemetry: no se pudo guardar telemetria: {error.Message}");
            }
        }

        private void HandlePlayerDamaged(Health _, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            SummaryFor(CurrentZoneKey()).DamageTaken += amount;
            MarkDirty();
        }

        private void HandlePlayerDied(Health _)
        {
            SummaryFor(CurrentZoneKey()).Deaths++;
            MarkDirty();
            SaveNow();
            Network?.SendTelemetry(ToJson(pretty: false));
        }

        private CombatTelemetrySnapshot BuildSnapshot()
        {
            var snapshot = new CombatTelemetrySnapshot
            {
                GeneratedAt = DateTime.UtcNow.ToString("O")
            };

            foreach (var entry in zones.Values)
            {
                var copy = entry.Copy();
                copy.AverageTimeToKill = copy.Kills > 0 ? copy.TotalTimeToKill / copy.Kills : 0f;
                copy.NormalAverageTimeToKill = copy.NormalKills > 0 ? copy.NormalTotalTimeToKill / copy.NormalKills : 0f;
                copy.EliteAverageTimeToKill = copy.EliteKills > 0 ? copy.EliteTotalTimeToKill / copy.EliteKills : 0f;
                copy.BossAverageTimeToKill = copy.BossKills > 0 ? copy.BossTotalTimeToKill / copy.BossKills : 0f;
                snapshot.Zones.Add(copy);
                snapshot.TotalKills += copy.Kills;
                snapshot.TotalDeaths += copy.Deaths;
                snapshot.TotalDamageDealt += copy.DamageDealt;
                snapshot.TotalDamageTaken += copy.DamageTaken;
            }

            return snapshot;
        }

        private string SummaryLine()
        {
            var snapshot = BuildSnapshot();
            return $"TEL kills {snapshot.TotalKills} | muertes {snapshot.TotalDeaths} | dano {snapshot.TotalDamageDealt}/{snapshot.TotalDamageTaken}";
        }

        private ZoneTelemetrySummary SummaryFor(string zoneId)
        {
            if (!zones.TryGetValue(zoneId, out var summary))
            {
                summary = new ZoneTelemetrySummary { ZoneId = zoneId };
                zones[zoneId] = summary;
            }

            return summary;
        }

        private string CurrentZoneKey()
        {
            if (zoneDefinitions.Count == 0)
            {
                return "training";
            }

            var position = transform.position;
            var best = zoneDefinitions[0];
            var bestDistance = float.MaxValue;

            foreach (var zone in zoneDefinitions)
            {
                if (zone == null)
                {
                    continue;
                }

                var distance = (zone.GroundCenter - position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    best = zone;
                    bestDistance = distance;
                }
            }

            return ZoneKey(best != null ? best.DisplayName : "training");
        }

        private static string ZoneKey(string zoneId)
        {
            return string.IsNullOrWhiteSpace(zoneId) ? "training" : zoneId.Trim();
        }

        private void MarkDirty()
        {
            dirty = true;
            Changed?.Invoke();

            if (nextSave <= 0f)
            {
                nextSave = Time.unscaledTime + SaveIntervalSeconds;
                nextFeed = Time.unscaledTime + 8f;
                nextNetwork = Time.unscaledTime + 12f;
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && dirty)
            {
                SaveNow();
            }
        }

        private void OnApplicationQuit()
        {
            if (dirty)
            {
                SaveNow();
            }
        }

        private static string BuildZoneLine(ZoneDefinition zone, ZoneTelemetrySummary summary)
        {
            if (summary == null || summary.Kills == 0)
            {
                return Localization.Tr("telemetry.zone_no_data", zone.DisplayName, zone.MinLevel, zone.MaxLevel);
            }

            var normal = FormatTier(summary.NormalKills, summary.NormalAverageTimeToKill, zone.NormalTtkMin, zone.NormalTtkMax);
            var elite = FormatTier(summary.EliteKills, summary.EliteAverageTimeToKill, zone.EliteTtkMin, zone.EliteTtkMax);
            var boss = FormatTier(summary.BossKills, summary.BossAverageTimeToKill, zone.BossTtkMin, zone.BossTtkMax);
            return Localization.Tr("telemetry.zone_line", zone.DisplayName, zone.MinLevel, zone.MaxLevel,
                summary.Deaths, normal, elite, boss);
        }

        private static string FormatTier(int kills, float averageTimeToKill, float targetMin, float targetMax)
        {
            if (kills == 0)
            {
                return Localization.Tr("telemetry.no_kills");
            }

            var status = averageTimeToKill < targetMin
                ? Localization.Tr("telemetry.fast")
                : averageTimeToKill > targetMax
                    ? Localization.Tr("telemetry.slow")
                    : Localization.Tr("telemetry.ok");
            return Localization.Tr("telemetry.tier", kills, averageTimeToKill, status);
        }

        private struct EnemyCombatSample
        {
            public float StartedAt;
            public EnemyTier Tier;
            public string EnemyId;
            public string ZoneId;
        }
    }

    [Serializable]
    public sealed class CombatTelemetrySnapshot
    {
        public string GeneratedAt;
        public int TotalKills;
        public int TotalDeaths;
        public int TotalDamageDealt;
        public int TotalDamageTaken;
        public List<ZoneTelemetrySummary> Zones = new List<ZoneTelemetrySummary>();
    }

    [Serializable]
    public sealed class ZoneTelemetrySummary
    {
        public string ZoneId;
        public int Kills;
        public int NormalKills;
        public int EliteKills;
        public int BossKills;
        public int Deaths;
        public int DamageDealt;
        public int DamageTaken;
        public float TotalTimeToKill;
        public float AverageTimeToKill;
        public float NormalTotalTimeToKill;
        public float EliteTotalTimeToKill;
        public float BossTotalTimeToKill;
        public float NormalAverageTimeToKill;
        public float EliteAverageTimeToKill;
        public float BossAverageTimeToKill;

        public ZoneTelemetrySummary Copy()
        {
            return new ZoneTelemetrySummary
            {
                ZoneId = ZoneId,
                Kills = Kills,
                NormalKills = NormalKills,
                EliteKills = EliteKills,
                BossKills = BossKills,
                Deaths = Deaths,
                DamageDealt = DamageDealt,
                DamageTaken = DamageTaken,
                TotalTimeToKill = TotalTimeToKill,
                AverageTimeToKill = AverageTimeToKill,
                NormalTotalTimeToKill = NormalTotalTimeToKill,
                EliteTotalTimeToKill = EliteTotalTimeToKill,
                BossTotalTimeToKill = BossTotalTimeToKill,
                NormalAverageTimeToKill = NormalAverageTimeToKill,
                EliteAverageTimeToKill = EliteAverageTimeToKill,
                BossAverageTimeToKill = BossAverageTimeToKill
            };
        }
    }
}
