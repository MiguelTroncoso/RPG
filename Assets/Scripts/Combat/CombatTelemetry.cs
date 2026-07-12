using System;
using System.Collections.Generic;
using System.IO;
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
                summary.TotalTimeToKill += Mathf.Max(0f, Time.unscaledTime - sample.StartedAt);
                activeCombats.Remove(enemyHealth);
            }

            MarkDirty();
        }

        public string ToJson(bool pretty)
        {
            return JsonUtility.ToJson(BuildSnapshot(), pretty);
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
                AverageTimeToKill = AverageTimeToKill
            };
        }
    }
}
