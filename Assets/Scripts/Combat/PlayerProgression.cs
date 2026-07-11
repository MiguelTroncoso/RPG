using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class PlayerProgression : MonoBehaviour
    {
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; }
        public int Gold { get; private set; }
        public int NextLevelExperience => Level * 100;

        public PrototypeHud Hud;

        public void AddExperience(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Experience += amount;

            while (Experience >= NextLevelExperience)
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
