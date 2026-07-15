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
        public int RebirthCount { get; private set; }
        public int Renown { get; private set; }

        public LevelProgressionTable Table;
        public PrototypeHud Hud;
        public MmorpgNetworkClient Network;

        // Multiplicadores pasivos (mascotas, eventos); 1 = sin bono.
        public float ExperienceMultiplier = 1f;
        public float GoldMultiplier = 1f;

        public int EffectiveMaxLevel => ExperienceResolver.MaxLevelOf(Table);
        public bool IsMaxLevel => Level >= EffectiveMaxLevel;
        public bool CanRebirth => IsMaxLevel;
        public float RebirthExperienceMultiplier => 1f + RebirthCount * 0.02f;

        public int NextLevelExperience => Table != null
            ? (int)Math.Min(int.MaxValue, Table.GetExpToNext(Level))
            : Level * LevelProgressionTable.FallbackExpPerLevel;

        public void AddExperience(int amount)
        {
            if (amount <= 0 || IsMaxLevel)
            {
                return;
            }

            amount = Mathf.Max(1, Mathf.RoundToInt(amount * ExperienceMultiplier * RebirthExperienceMultiplier));
            var result = ExperienceResolver.AddExperience(Table, Level, Experience, amount);
            Level = result.Level;
            Experience = result.Experience;
            AttributePoints += result.AttributePointsGained;

            if (result.LevelsGained > 0)
            {
                var health = GetComponent<Health>();
                health?.Heal(health.MaxHealth);

                Hud?.SetStatus(IsMaxLevel
                    ? Localization.Tr("status.max_level", Level)
                    : Localization.Tr("status.level_up", Level));

                if (result.AttributePointsGained > 0)
                {
                    Hud?.AddFeed(Localization.Tr("feed.attribute_points", result.AttributePointsGained));
                }

                GetComponent<CombatFeedbackAudio>()?.PlayLevelUp();
                CombatFeedbackVfx.SpawnLevelUp(transform.position + Vector3.up * 0.6f, new Color(1f, 0.82f, 0.22f));

                Network?.SendAction("level_up", Localization.Tr("net.level_up", Level), Level);
            }

            Hud?.RefreshProgression();
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Gold += Mathf.Max(1, Mathf.RoundToInt(amount * GoldMultiplier));
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

        public void RestoreRebirthState(int rebirthCount, int renown)
        {
            RebirthCount = Mathf.Max(0, rebirthCount);
            Renown = Mathf.Max(0, renown);
            Hud?.RefreshProgression();
        }

        public bool TryRebirth()
        {
            if (!CanRebirth)
            {
                Hud?.SetStatus(Localization.Tr("rebirth.required", EffectiveMaxLevel), 3f);
                return false;
            }

            RebirthCount++;
            Renown++;
            Level = 1;
            Experience = 0;
            AttributePoints = 0;

            var health = GetComponent<Health>();
            health?.Heal(health.MaxHealth);
            Hud?.SetStatus(Localization.Tr("rebirth.success", RebirthCount, Renown), 6f);
            Hud?.AddFeed(Localization.Tr("rebirth.renown", Renown, RebirthExperienceMultiplier));
            Hud?.RefreshProgression();
            return true;
        }

        public bool TrySpendAttributePoint()
        {
            if (AttributePoints <= 0)
            {
                return false;
            }

            AttributePoints--;
            Hud?.RefreshProgression();
            return true;
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
