using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class PlayerProgression : MonoBehaviour
    {
        public const int MaxLevel = 105;

        public int Level { get; private set; } = 1;
        public int Experience { get; private set; }
        public int Gold { get; private set; }
        public int NextLevelExperience => Level * 100;

        public PrototypeHud Hud;

        public void AddExperience(int amount)
        {
            if (amount <= 0 || Level >= MaxLevel)
            {
                return;
            }

            Experience += amount;

            while (Level < MaxLevel && Experience >= NextLevelExperience)
            {
                Experience -= NextLevelExperience;
                Level++;
                var health = GetComponent<Health>();
                health?.Heal(health.MaxHealth);
                Hud?.SetStatus($"Subiste a nivel {Level}.");
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

        public void RestoreState(int level, int experience, int gold)
        {
            Level = Mathf.Clamp(level, 1, MaxLevel);
            Experience = Mathf.Clamp(experience, 0, Mathf.Max(0, NextLevelExperience - 1));
            Gold = Mathf.Max(0, gold);
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
