using System;

namespace MmorpgPrototype
{
    public readonly struct ExperienceResult
    {
        public readonly int Level;
        public readonly int Experience;
        public readonly int LevelsGained;
        public readonly int AttributePointsGained;

        public ExperienceResult(int level, int experience, int levelsGained, int attributePointsGained)
        {
            Level = level;
            Experience = experience;
            LevelsGained = levelsGained;
            AttributePointsGained = attributePointsGained;
        }
    }

    // Resolucion pura de EXP y subida de nivel: sin estado, sin escena, sin
    // efectos. Portable al servidor como especificacion de la regla.
    public static class ExperienceResolver
    {
        public static ExperienceResult AddExperience(LevelProgressionTable table, int level, int experience, int amount)
        {
            var maxLevel = MaxLevelOf(table);
            var currentLevel = Math.Clamp(level, 1, maxLevel);
            long currentExp = Math.Max(0, experience);

            if (amount <= 0 || currentLevel >= maxLevel)
            {
                return new ExperienceResult(currentLevel, (int)Math.Min(int.MaxValue, currentExp), 0, 0);
            }

            currentExp += amount;
            var levelsGained = 0;
            var pointsGained = 0;

            while (currentLevel < maxLevel && currentExp >= ExpToNext(table, currentLevel))
            {
                currentExp -= ExpToNext(table, currentLevel);
                currentLevel++;
                levelsGained++;
                pointsGained += AttributePointsAt(table, currentLevel);
            }

            if (currentLevel >= maxLevel)
            {
                currentExp = 0;
            }

            return new ExperienceResult(currentLevel, (int)Math.Min(int.MaxValue, currentExp), levelsGained, pointsGained);
        }

        public static int MaxLevelOf(LevelProgressionTable table)
        {
            return table != null ? Math.Max(2, table.MaxLevel) : PlayerProgression.MaxLevel;
        }

        private static long ExpToNext(LevelProgressionTable table, int level)
        {
            return table != null
                ? table.GetExpToNext(level)
                : Math.Max(1, level) * (long)LevelProgressionTable.FallbackExpPerLevel;
        }

        private static int AttributePointsAt(LevelProgressionTable table, int level)
        {
            return table != null ? table.GetAttributePoints(level) : LevelProgressionTable.FallbackAttributePoints;
        }
    }
}
