using UnityEngine;

namespace MmorpgPrototype
{
    // Delayed, fractional regeneration for the player when incoming damage
    // has stopped. The accumulator keeps healing smooth despite integer HP.
    [RequireComponent(typeof(Health))]
    public sealed class PlayerRegeneration : MonoBehaviour
    {
        public float DelayAfterDamage = 4f;
        public float HealPerSecond = 7f;
        public float MovingMultiplier = 1.15f;
        public float IdleMultiplier = 0.85f;
        public PrototypeHud Hud;

        private Health health;
        private PlayerController movement;
        private float healAccumulator;
        private bool wasRegenerating;

        public bool IsRegenerating { get; private set; }

        private void Awake()
        {
            health = GetComponent<Health>();
            movement = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (movement == null)
            {
                movement = GetComponent<PlayerController>();
            }

            if (health == null || health.IsDead || health.CurrentHealth >= health.MaxHealth || health.TimeSinceDamage < DelayAfterDamage)
            {
                healAccumulator = 0f;
                IsRegenerating = false;
                wasRegenerating = false;
                return;
            }

            var multiplier = movement != null && movement.IsReceivingMovementInput ? MovingMultiplier : IdleMultiplier;
            healAccumulator += HealPerSecond * Mathf.Max(0.1f, multiplier) * Time.deltaTime;

            var amount = Mathf.FloorToInt(healAccumulator);
            if (amount <= 0)
            {
                IsRegenerating = true;
                return;
            }

            healAccumulator -= amount;
            var previousHealth = health.CurrentHealth;
            health.Heal(amount);
            IsRegenerating = health.CurrentHealth > previousHealth;

            if (IsRegenerating && !wasRegenerating)
            {
                Hud?.SetStatus(Localization.Tr("regen.started"), 1.5f);
            }

            wasRegenerating = IsRegenerating;
        }
    }
}
