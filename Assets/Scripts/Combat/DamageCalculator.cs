using System;

namespace MmorpgPrototype
{
    public readonly struct CombatStats
    {
        public readonly int Damage;
        public readonly float CritChance;
        public readonly float CritMultiplier;
        public readonly float Accuracy;
        public readonly float Evasion;
        public readonly int Defense;

        public CombatStats(int damage, float critChance, float critMultiplier, float accuracy, float evasion, int defense)
        {
            Damage = damage;
            CritChance = critChance;
            CritMultiplier = critMultiplier;
            Accuracy = accuracy;
            Evasion = evasion;
            Defense = defense;
        }

        public static CombatStats Defender(float evasion, int defense)
        {
            return new CombatStats(0, 0f, 1f, 1f, evasion, defense);
        }
    }

    public readonly struct DamageResult
    {
        public readonly int Amount;
        public readonly bool IsCritical;
        public readonly bool IsMiss;

        public DamageResult(int amount, bool isCritical, bool isMiss)
        {
            Amount = amount;
            IsCritical = isCritical;
            IsMiss = isMiss;
        }
    }

    // Calculo de dano centralizado y puro: los rolls se inyectan (0..1) para
    // poder testearlo y ejecutarlo en el servidor. Nada de Unity aqui.
    public static class DamageCalculator
    {
        public const float MinHitChance = 0.2f;
        public const float DamageVariance = 0.15f;

        public static DamageResult Resolve(CombatStats attacker, CombatStats defender, float hitRoll, float critRoll, float varianceRoll)
        {
            var hitChance = Clamp(attacker.Accuracy - defender.Evasion, MinHitChance, 1f);
            if (hitRoll >= hitChance)
            {
                return new DamageResult(0, false, true);
            }

            var variance = 1f + (varianceRoll * 2f - 1f) * DamageVariance;
            var raw = attacker.Damage * variance;

            var isCritical = critRoll < attacker.CritChance;
            if (isCritical)
            {
                raw *= attacker.CritMultiplier;
            }

            var amount = Math.Max(1, (int)Math.Round(raw) - defender.Defense);
            return new DamageResult(amount, isCritical, false);
        }

        private static float Clamp(float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }
    }
}
