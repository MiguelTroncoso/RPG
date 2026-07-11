using System;
using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class PlayerProgression : MonoBehaviour
    {
        // Fallback si no hay tabla asignada; el limite real vive en la tabla.
        public const int MaxLevel = 105;

        public int Level { get; private set; } = 1;
        public int Experience { get; private set; }
        public int Gold { get; private set; }
        public int AttributePoints { get; private set; }

        public LevelProgressionTable Table;
        public PrototypeHud Hud;

        public int EffectiveMaxLevel => ExperienceResolver.MaxLevelOf(Table);
        public bool IsMaxLevel => Level >= EffectiveMaxLevel;

        public int NextLevelExperience => Table != null
            ? (int)Math.Min(int.MaxValue, Table.GetExpToNext(Level))
            : Level * LevelProgressionTable.FallbackExpPerLevel;

        public void AddExperience(int amount)
        {
            if (amount <= 0 || IsMaxLevel)
            {
                return;
            }

            var result = ExperienceResolver.AddExperience(Table, Level, Experience, amount);
            Level = result.Level;
            Experience = result.Experience;
            AttributePoints += result.AttributePointsGained;

            if (result.LevelsGained > 0)
            {
                var health = GetComponent<Health>();
                health?.Heal(health.MaxHealth);

                Hud?.SetStatus(IsMaxLevel
                    ? $"Alcanzaste el nivel maximo {Level}."
                    : $"Subiste a nivel {Level}.");

                if (result.AttributePointsGained > 0)
                {
                    Hud?.AddFeed($"+{result.AttributePointsGained} puntos de atributo");
                }
            }

            Hud?.RefreshProgression();
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Gold += amount;
            Hud?.RefreshProgression();
        }

        public void RestoreState(int level, int experience, int gold, int attributePoints)
        {
            Level = Mathf.Clamp(level, 1, EffectiveMaxLevel);
            Experience = Mathf.Clamp(experience, 0, Mathf.Max(0, NextLevelExperience - 1));
            Gold = Mathf.Max(0, gold);
            AttributePoints = Mathf.Max(0, attributePoints);
            Hud?.RefreshProgression();
        }

        public bool SpendGold(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (Gold < amount)
            {
                return false;
            }

            Gold -= amount;
            Hud?.RefreshProgression();
            return true;
        }
    }
}
