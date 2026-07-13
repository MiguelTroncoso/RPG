using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class ZoneBalanceReport
    {
        public string ZoneId;
        public int ExpectedPlayerDamage;
        public float NormalTtk;
        public float EliteTtk;
        public float BossTtk;
        public bool NormalWithinTarget;
        public bool EliteWithinTarget;
        public bool BossWithinTarget;

        public bool IsWithinTargets => NormalWithinTarget && EliteWithinTarget && BossWithinTarget;
    }

    // Read-only balance pass. It reports TTK against the configured targets
    // without mutating ScriptableObjects or hiding balance problems.
    public static class ZoneBalanceResolver
    {
        private const float BasePlayerDamage = 26f;
        private const float BaseAttackInterval = 0.65f;

        public static ZoneBalanceReport Evaluate(ZoneDefinition zone)
        {
            if (zone == null)
            {
                return new ZoneBalanceReport();
            }

            var levelOffset = Mathf.Max(0, zone.MinLevel - 1) / 10f;
            var expectedDamage = Mathf.Max(1, Mathf.RoundToInt(BasePlayerDamage * Mathf.Pow(1.075f, levelOffset)));
            var report = new ZoneBalanceReport
            {
                ZoneId = zone.ZoneId,
                ExpectedPlayerDamage = expectedDamage,
                NormalTtk = Ttk(zone.NormalHealth, expectedDamage),
                EliteTtk = Ttk(zone.EliteHealth, expectedDamage),
                BossTtk = Ttk(zone.BossHealth, expectedDamage)
            };

            report.NormalWithinTarget = Within(report.NormalTtk, zone.NormalTtkMin, zone.NormalTtkMax);
            report.EliteWithinTarget = Within(report.EliteTtk, zone.EliteTtkMin, zone.EliteTtkMax);
            report.BossWithinTarget = Within(report.BossTtk, zone.BossTtkMin, zone.BossTtkMax);
            return report;
        }

        public static void LogReports(List<ZoneDefinition> zones)
        {
            if (zones == null)
            {
                return;
            }

            foreach (var zone in zones)
            {
                var report = Evaluate(zone);
                Debug.Log($"ZoneBalance {report.ZoneId}: DMG {report.ExpectedPlayerDamage} | TTK N {report.NormalTtk:0.0}s E {report.EliteTtk:0.0}s B {report.BossTtk:0.0}s | targets {(report.IsWithinTargets ? "OK" : "REVIEW")}");
            }
        }

        public static int NormalCountFor(ZoneDefinition zone, bool mobile)
        {
            if (zone == null)
            {
                return 0;
            }

            return mobile ? Mathf.Min(zone.NormalCount, 6) : zone.NormalCount;
        }

        public static int EliteCountFor(ZoneDefinition zone, bool mobile)
        {
            if (zone == null)
            {
                return 0;
            }

            return mobile ? Mathf.Min(zone.EliteCount, 2) : zone.EliteCount;
        }

        private static float Ttk(int health, int damage)
        {
            return Mathf.Max(1, health) / (float)Mathf.Max(1, damage) * BaseAttackInterval;
        }

        private static bool Within(float value, float min, float max)
        {
            return value >= min * 0.75f && value <= max * 1.35f;
        }
    }
}
