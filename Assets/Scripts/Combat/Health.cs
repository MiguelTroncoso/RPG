using System;
using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth = 100;

        public event Action<Health> Changed;
        public event Action<Health> Died;
        public event Action<Health, int> Damaged;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public bool IsDead => currentHealth <= 0;
        public float Normalized => maxHealth <= 0 ? 0f : Mathf.Clamp01((float)currentHealth / maxHealth);

        private void Awake()
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        public void ResetHealth(int newMaxHealth)
        {
            maxHealth = Mathf.Max(1, newMaxHealth);
            currentHealth = maxHealth;
            Changed?.Invoke(this);
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - amount);
            Damaged?.Invoke(this, amount);
            Changed?.Invoke(this);

            if (currentHealth == 0)
            {
                Died?.Invoke(this);
            }
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            Changed?.Invoke(this);
        }
    }
}
