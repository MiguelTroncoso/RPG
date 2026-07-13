using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class PlayerStatSheet : MonoBehaviour
    {
        public StatSheet Sheet { get; } = new StatSheet();

        public int MaxHealth => Mathf.Max(1, Sheet.GetInt(StatType.MaxHealth));
        public int BaseDamage => Mathf.Max(1, Mathf.RoundToInt(Sheet.GetBase(StatType.Damage)));
        public int Damage => Mathf.Max(1, Sheet.GetInt(StatType.Damage));
        public int BonusDamage => Mathf.Max(0, Damage - BaseDamage);
        public int Defense => Mathf.Max(0, Sheet.GetInt(StatType.Defense));
        public float MoveSpeed => Mathf.Max(0.1f, Sheet.GetFloat(StatType.MoveSpeed));
        public float AttackRange => Mathf.Max(0.1f, Sheet.GetFloat(StatType.AttackRange));
        public float AttackCooldown => Mathf.Max(0.05f, Sheet.GetFloat(StatType.AttackCooldown));
        public float CritChance => Mathf.Clamp01(Sheet.GetFloat(StatType.CritChance));
        public float CritMultiplier => Mathf.Max(1f, Sheet.GetFloat(StatType.CritMultiplier));
        public float Accuracy => Mathf.Clamp01(Sheet.GetFloat(StatType.Accuracy));
        public float Evasion => Mathf.Clamp01(Sheet.GetFloat(StatType.Evasion));

        public void Rebuild(
            ClassDefinition definition,
            PlayerEquipment equipment,
            PlayerAttributes attributes,
            MountService mount,
            PlayerSkills skills,
            PetService pet,
            CosmeticService cosmetics)
        {
            Sheet.Clear();

            if (definition == null)
            {
                return;
            }

            Sheet.SetBase(StatType.MaxHealth, definition.MaxHealth);
            Sheet.SetBase(StatType.Damage, definition.BaseDamage);
            Sheet.SetBase(StatType.Defense, definition.Defense);
            Sheet.SetBase(StatType.MoveSpeed, definition.MoveSpeed);
            Sheet.SetBase(StatType.AttackRange, definition.AttackRange);
            Sheet.SetBase(StatType.AttackCooldown, definition.AttackCooldown);
            Sheet.SetBase(StatType.CritChance, definition.CritChance);
            Sheet.SetBase(StatType.CritMultiplier, definition.CritMultiplier);
            Sheet.SetBase(StatType.Accuracy, definition.Accuracy);
            Sheet.SetBase(StatType.Evasion, definition.Evasion);

            if (equipment != null)
            {
                Sheet.AddModifier(StatType.Damage, StatModifierSource.Equipment, equipment.TotalDamageBonus);
                Sheet.AddModifier(StatType.MaxHealth, StatModifierSource.Equipment, equipment.TotalMaxHealthBonus);
                Sheet.AddModifier(StatType.MoveSpeed, StatModifierSource.Equipment, equipment.TotalMoveSpeedBonus);
            }

            if (attributes != null)
            {
                Sheet.AddModifier(StatType.Damage, StatModifierSource.Attributes, attributes.BonusDamage);
                Sheet.AddModifier(StatType.MaxHealth, StatModifierSource.Attributes, attributes.BonusMaxHealth);
                Sheet.AddModifier(StatType.MoveSpeed, StatModifierSource.Attributes, attributes.BonusMoveSpeed);
                Sheet.AddModifier(StatType.CritChance, StatModifierSource.Attributes, attributes.BonusCritChance);
            }

            if (skills != null)
            {
                Sheet.AddModifier(StatType.Damage, StatModifierSource.Buff, skills.DamageBuff);
            }

            if (pet != null)
            {
                Sheet.AddModifier(StatType.Damage, StatModifierSource.Pet, pet.DamageBonus);
                Sheet.AddModifier(StatType.MaxHealth, StatModifierSource.Pet, pet.MaxHealthBonus);
                Sheet.AddModifier(StatType.CritChance, StatModifierSource.Pet, pet.CritChanceBonus);
            }

            if (cosmetics != null)
            {
                Sheet.AddModifier(StatType.Damage, StatModifierSource.Cosmetic, cosmetics.DamageBonus);
                Sheet.AddModifier(StatType.MaxHealth, StatModifierSource.Cosmetic, cosmetics.MaxHealthBonus);
                Sheet.AddModifier(StatType.CritChance, StatModifierSource.Cosmetic, cosmetics.CritChanceBonus);
            }

            if (mount != null && !Mathf.Approximately(mount.SpeedMultiplier, 1f))
            {
                var speedBeforeMount = Sheet.GetFloat(StatType.MoveSpeed);
                Sheet.AddModifier(StatType.MoveSpeed, StatModifierSource.Mount, speedBeforeMount * (mount.SpeedMultiplier - 1f));
            }

            if (mount != null)
            {
                Sheet.AddModifier(StatType.Damage, StatModifierSource.Mount, mount.DamageBonus);
                Sheet.AddModifier(StatType.MaxHealth, StatModifierSource.Mount, mount.MaxHealthBonus);
                Sheet.AddModifier(StatType.CritChance, StatModifierSource.Mount, mount.CritChanceBonus);
            }
        }

        public string DamageBreakdown()
        {
            return $"base {BaseDamage}, equipo +{Sheet.GetModifierTotal(StatType.Damage, StatModifierSource.Equipment):0}, atributos +{Sheet.GetModifierTotal(StatType.Damage, StatModifierSource.Attributes):0}, mascota +{Sheet.GetModifierTotal(StatType.Damage, StatModifierSource.Pet):0}, cosmeticos +{Sheet.GetModifierTotal(StatType.Damage, StatModifierSource.Cosmetic):0}, buff +{Sheet.GetModifierTotal(StatType.Damage, StatModifierSource.Buff):0}";
        }
    }
}
