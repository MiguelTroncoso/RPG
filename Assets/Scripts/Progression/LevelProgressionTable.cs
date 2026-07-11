using System;
using UnityEngine;

namespace MmorpgPrototype
{
    // Tabla de niveles 1..MaxLevel generada desde ExpCurveConfig. No se
    // rellena a mano: usar el menu MMORPG > Progression > Generate Level
    // Table, o el fallback runtime del bootstrap si el asset no existe.
    [CreateAssetMenu(menuName = "MMORPG/Progression/Level Progression Table", fileName = "LevelProgressionTable")]
    public sealed class LevelProgressionTable : ScriptableObject
    {
        [Serializable]
        public struct LevelRow
        {
            public int Level;
            public long ExpToNext;
            public int AttributePoints;
        }

        public const int FallbackExpPerLevel = 100;
        public const int FallbackAttributePoints = 5;

        [Min(2)] public int MaxLevel = 105;
        public LevelRow[] Rows;

        public bool HasRows => Rows != null && Rows.Length > 0;

        public long GetExpToNext(int level)
        {
            var index = level - 1;
            if (!HasRows || index < 0 || index >= Rows.Length)
            {
                return Math.Max(1, level) * (long)FallbackExpPerLevel;
            }

            return Math.Max(1L, Rows[index].ExpToNext);
        }

        public int GetAttributePoints(int level)
        {
            var index = level - 1;
            if (!HasRows || index < 0 || index >= Rows.Length)
            {
                return FallbackAttributePoints;
            }

            return Math.Max(0, Rows[index].AttributePoints);
        }

        public void GenerateFrom(ExpCurveConfig config)
        {
            if (config == null)
            {
                return;
            }

            MaxLevel = Mathf.Max(2, config.MaxLevel);
            Rows = new LevelRow[MaxLevel];

            for (var i = 0; i < MaxLevel; i++)
            {
                var level = i + 1;
                Rows[i] = new LevelRow
                {
                    Level = level,
                    ExpToNext = config.ExpToNext(level),
                    AttributePoints = config.AttributePointsPerLevel
                };
            }
        }
    }
}
