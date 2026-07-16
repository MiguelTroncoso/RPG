using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class PlayerEnergySystem : MonoBehaviour
    {
        public float MaxEnergy = 100f;
        public float RegenerationPerSecond = 16f;
        public float CurrentEnergy { get; private set; }
        public float Normalized => MaxEnergy <= 0f ? 0f : Mathf.Clamp01(CurrentEnergy / MaxEnergy);

        private void Awake()
        {
            CurrentEnergy = MaxEnergy;
        }

        private void Update()
        {
            if (CurrentEnergy < MaxEnergy)
            {
                CurrentEnergy = Mathf.Min(MaxEnergy, CurrentEnergy + RegenerationPerSecond * Time.deltaTime);
            }
        }

        public bool TrySpend(float amount)
        {
            if (amount <= 0f)
            {
                return true;
            }

            if (CurrentEnergy + 0.01f < amount)
            {
                return false;
            }

            CurrentEnergy = Mathf.Max(0f, CurrentEnergy - amount);
            return true;
        }

        public void RestoreFull()
        {
            CurrentEnergy = MaxEnergy;
        }
    }
}
