using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    public enum StatType
    {
        MaxHealth,
        Damage,
        Defense,
        MoveSpeed,
        AttackRange,
        AttackCooldown,
        CritChance,
        CritMultiplier,
        Accuracy,
        Evasion
    }

    public enum StatModifierSource
    {
        Class,
        Equipment,
        Attributes,
        Mount,
        Pet,
        Cosmetic,
        Buff
    }

    public readonly struct StatModifier
    {
        public readonly StatType Type;
        public readonly StatModifierSource Source;
        public readonly float Value;

        public StatModifier(StatType type, StatModifierSource source, float value)
        {
            Type = type;
            Source = source;
            Value = value;
        }
    }

    // Hoja de stats pequena y explicita: base por clase + modificadores por origen.
    public sealed class StatSheet
    {
        private readonly Dictionary<StatType, float> baseValues = new Dictionary<StatType, float>();
        private readonly Dictionary<StatModifierSource, List<StatModifier>> modifiersBySource = new Dictionary<StatModifierSource, List<StatModifier>>();

        public void Clear()
        {
            baseValues.Clear();
            modifiersBySource.Clear();
        }

        public void SetBase(StatType type, float value)
        {
            baseValues[type] = value;
        }

        public void AddModifier(StatType type, StatModifierSource source, float value)
        {
            if (Mathf.Approximately(value, 0f))
            {
                return;
            }

            if (!modifiersBySource.TryGetValue(source, out var modifiers))
            {
                modifiers = new List<StatModifier>();
                modifiersBySource[source] = modifiers;
            }

            modifiers.Add(new StatModifier(type, source, value));
        }

        public float GetBase(StatType type)
        {
            return baseValues.TryGetValue(type, out var value) ? value : 0f;
        }

        public float GetFloat(StatType type)
        {
            return GetBase(type) + GetModifierTotal(type);
        }

        public int GetInt(StatType type)
        {
            return Mathf.RoundToInt(GetFloat(type));
        }

        public float GetModifierTotal(StatType type)
        {
            var total = 0f;
            foreach (var entry in modifiersBySource)
            {
                foreach (var modifier in entry.Value)
                {
                    if (modifier.Type == type)
                    {
                        total += modifier.Value;
                    }
                }
            }

            return total;
        }

        public float GetModifierTotal(StatType type, StatModifierSource source)
        {
            if (!modifiersBySource.TryGetValue(source, out var modifiers))
            {
                return 0f;
            }

            var total = 0f;
            foreach (var modifier in modifiers)
            {
                if (modifier.Type == type)
                {
                    total += modifier.Value;
                }
            }

            return total;
        }
    }
}
