using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    [RequireComponent(typeof(PlayerCombat))]
    [RequireComponent(typeof(PlayerClassController))]
    public sealed class PlayerSkills : MonoBehaviour
    {
        public const int SkillSlotCount = 4;
        public const int MaxSkillLevel = 5;

        public float SkillOneCooldown = 4f;
        public float SkillTwoCooldown = 7f;
        public float SkillThreeCooldown = 9f;
        public float SkillFourCooldown = 14f;
        public PrototypeHud Hud;
        public PlayerProgression Progression;
        public InventorySystem Inventory;

        private static readonly int[] UnlockLevels = { 1, 1, 8, 20 };
        private readonly int[] skillLevels = { 1, 1, 0, 0 };
        private readonly float[] nextSkill = new float[SkillSlotCount];
        private PlayerCombat combat;
        private PlayerClassController classController;
        private EquipmentUpgradeSystem upgradeSystem;
        private float buffUntil;
        private int damageBuff;

        public float SkillOneRemaining => Remaining(0);
        public float SkillTwoRemaining => Remaining(1);
        public float SkillThreeRemaining => Remaining(2);
        public float SkillFourRemaining => Remaining(3);
        public int DamageBuff => buffUntil > 0f ? damageBuff : 0;

        private void Awake()
        {
            combat = GetComponent<PlayerCombat>();
            classController = GetComponent<PlayerClassController>();
            upgradeSystem = GetComponent<EquipmentUpgradeSystem>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q)) UseSkillOne();
            if (Input.GetKeyDown(KeyCode.E)) UseSkillTwo();
            if (Input.GetKeyDown(KeyCode.R)) UseSkillThree();
            if (Input.GetKeyDown(KeyCode.F)) UseSkillFour();

            if (buffUntil > 0f && Time.time > buffUntil)
            {
                damageBuff = 0;
                buffUntil = 0f;
                upgradeSystem?.ApplyBonuses();
                Hud?.RefreshClass();
            }
        }

        public void UseSkillOne() => UseSkill(0);
        public void UseSkillTwo() => UseSkill(1);
        public void UseSkillThree() => UseSkill(2);
        public void UseSkillFour() => UseSkill(3);

        public void UpgradeSkill(int slot)
        {
            if (!IsValidSlot(slot))
            {
                return;
            }

            if (!IsSkillUnlocked(slot))
            {
                Hud?.SetStatus(Localization.Tr("skill.locked_level", UnlockLevelFor(slot)));
                return;
            }

            var level = SkillLevelFor(slot);
            if (level >= MaxSkillLevel)
            {
                Hud?.SetStatus(Localization.Tr("skill.upgrade_max", SkillNameFor(slot), MaxSkillLevel));
                return;
            }

            var tomeCost = level;
            if (Inventory == null || Inventory.Count(DefaultGameItems.SkillTome) < tomeCost)
            {
                Hud?.SetStatus(Localization.Tr("skill.upgrade_need_tome", tomeCost));
                return;
            }

            Inventory.TryConsume(DefaultGameItems.SkillTome, tomeCost);
            skillLevels[slot] = Mathf.Clamp(level + 1, 1, MaxSkillLevel);
            Hud?.SetStatus(Localization.Tr("skill.upgraded", SkillNameFor(slot), skillLevels[slot]), 3f);
            Hud?.AddFeed(Localization.Tr("skill.upgrade_feed", SkillNameFor(slot), skillLevels[slot]));
            Hud?.RefreshClass();
            Hud?.RefreshSkillCooldowns(SkillOneRemaining, SkillTwoRemaining, SkillThreeRemaining, SkillFourRemaining);
            GetComponent<PlayerPersistence>()?.SaveNow();
        }

        public bool IsSkillUnlocked(int slot)
        {
            return IsValidSlot(slot)
                && SkillLevelFor(slot) > 0
                && (Progression == null || Progression.Level >= UnlockLevelFor(slot));
        }

        public int SkillLevelFor(int slot)
        {
            return IsValidSlot(slot) ? skillLevels[slot] : 0;
        }

        public int UnlockLevelFor(int slot)
        {
            return IsValidSlot(slot) ? UnlockLevels[slot] : int.MaxValue;
        }

        public string DisplayLabel(int slot, float remaining = 0f)
        {
            if (!IsValidSlot(slot) || classController == null || classController.Definition == null)
            {
                return string.Empty;
            }

            var key = KeyFor(slot);
            var name = SkillNameFor(slot);
            if (!IsSkillUnlocked(slot))
            {
                return Localization.Tr("skill.locked_label", key, name, UnlockLevelFor(slot));
            }

            var cooldown = remaining > 0.1f ? $" {remaining:0.0}s" : string.Empty;
            return $"{key} {name} Nv{SkillLevelFor(slot)}{cooldown}";
        }

        public List<int> ExportLevels()
        {
            return new List<int>(skillLevels);
        }

        public void RestoreLevels(IList<int> savedLevels)
        {
            for (var i = 0; i < SkillSlotCount; i++)
            {
                var fallback = i < 2 ? 1 : 0;
                var saved = savedLevels != null && i < savedLevels.Count ? savedLevels[i] : fallback;
                skillLevels[i] = Mathf.Clamp(saved, 0, MaxSkillLevel);
            }

            // The first two skills are part of the original prototype and stay
            // available in old saves even though they had no skill array.
            skillLevels[0] = Mathf.Max(1, skillLevels[0]);
            skillLevels[1] = Mathf.Max(1, skillLevels[1]);
            Hud?.RefreshClass();
        }

        private void UseSkill(int slot)
        {
            if (!IsValidSlot(slot))
            {
                return;
            }

            if (!IsSkillUnlocked(slot))
            {
                Hud?.SetStatus(Localization.Tr("skill.locked_level", UnlockLevelFor(slot)));
                return;
            }

            if (Time.time < nextSkill[slot])
            {
                Hud?.SetStatus(Localization.Tr("skill.cooldown", SkillNameFor(slot)));
                return;
            }

            nextSkill[slot] = Time.time + CooldownFor(slot);
            PlaySkillMotion();

            switch (slot)
            {
                case 0: UseSkillOneEffect(); break;
                case 1: UseSkillTwoEffect(); break;
                case 2: UseSkillThreeEffect(); break;
                case 3: UseSkillFourEffect(); break;
            }

            Hud?.RefreshSkillCooldowns(SkillOneRemaining, SkillTwoRemaining, SkillThreeRemaining, SkillFourRemaining);
        }

        private void UseSkillOneEffect()
        {
            switch (classController.CurrentClass)
            {
                case CharacterClassType.Ninja:
                    NinjaDash(DamageFor(0, 31), 2.35f, 1);
                    break;
                case CharacterClassType.Chaman:
                    MagicBurst(Localization.Tr("skill.spiritual_bolt"), DamageFor(0, 38), 5.8f, classController.Definition.SkillColor);
                    break;
                case CharacterClassType.Umbra:
                    MagicBurst(Localization.Tr("skill.dark_blade"), DamageFor(0, 44), 3.2f, classController.Definition.SkillColor);
                    break;
                default:
                    Cleave(DamageFor(0, 34), 2.8f, 3);
                    break;
            }
        }

        private void UseSkillTwoEffect()
        {
            switch (classController.CurrentClass)
            {
                case CharacterClassType.Ninja:
                    ApplyDamageBuff(Localization.Tr("skill.shadow_dash"), DamageFor(1, 10), 3.5f);
                    break;
                case CharacterClassType.Chaman:
                    HealSelf(38 + SkillLevelFor(1) * 8);
                    break;
                case CharacterClassType.Umbra:
                    MarkNearbyEnemy(DamageFor(1, 18));
                    break;
                default:
                    ApplyDamageBuff(Localization.Tr("skill.battle_cry"), DamageFor(1, 8), 4.5f);
                    break;
            }
        }

        private void UseSkillThreeEffect()
        {
            switch (classController.CurrentClass)
            {
                case CharacterClassType.Ninja:
                    MagicBurst("Shuriken triple", DamageFor(2, 56), 8.5f, classController.Definition.SkillColor);
                    break;
                case CharacterClassType.Chaman:
                    MagicBurst("Cadena espiritual", DamageFor(2, 52), 7f, classController.Definition.SkillColor);
                    break;
                case CharacterClassType.Umbra:
                    MagicBurst("Eclipse", DamageFor(2, 58), 5f, classController.Definition.SkillColor);
                    break;
                default:
                    MagicBurst("Golpe pesado", DamageFor(2, 62), 2.8f, classController.Definition.SkillColor);
                    break;
            }
        }

        private void UseSkillFourEffect()
        {
            var level = SkillLevelFor(3);
            switch (classController.CurrentClass)
            {
                case CharacterClassType.Ninja:
                    NinjaDash(DamageFor(3, 48), 3.1f, 3);
                    break;
                case CharacterClassType.Chaman:
                    HealSelf(65 + level * 12);
                    Cleave(DamageFor(3, 34), 4.8f, 3);
                    break;
                case CharacterClassType.Umbra:
                    Cleave(DamageFor(3, 60), 4.5f, 4);
                    break;
                default:
                    Cleave(DamageFor(3, 52), 4f, 5);
                    break;
            }
        }

        private void Cleave(int damage, float range, int maxTargets)
        {
            var hits = combat.DamageEnemiesInRange(range, damage, maxTargets);
            Hud?.SetStatus(hits > 0 ? Localization.Tr("skill.cleave_hit", hits) : Localization.Tr("skill.cleave_miss"));
        }

        private void NinjaDash(int damage, float range, int maxTargets)
        {
            var controller = GetComponent<CharacterController>();
            controller?.Move(transform.forward * (maxTargets > 1 ? 2.4f : 2.8f));
            var hits = combat.DamageEnemiesInRange(range, damage, maxTargets);
            Hud?.SetStatus(hits > 0 ? Localization.Tr("skill.ninja_dash_hit") : Localization.Tr("skill.ninja_dash"));
            PrototypePulseAndDestroy.Spawn(transform.position + Vector3.up * 1.1f, classController.Definition.SkillColor);
        }

        private void MagicBurst(string label, int damage, float range, Color color)
        {
            var target = combat.FindNearestEnemy(range);
            if (target == null)
            {
                Hud?.SetStatus(Localization.Tr("skill.no_target", label));
                return;
            }

            combat.DamageEnemy(target, damage, color);
            Hud?.SetStatus(Localization.Tr("skill.hit", label, target.name));
        }

        private void HealSelf(int amount)
        {
            var health = GetComponent<Health>();
            health?.Heal(amount);
            Hud?.SetStatus(Localization.Tr("skill.heal"));
            DamagePopup.Spawn(transform.position + Vector3.up * 2.15f, $"+{amount}", new Color(0.35f, 1f, 0.78f));
        }

        private void MarkNearbyEnemy(int damage)
        {
            var target = combat.FindNearestEnemy(4f);
            if (target == null)
            {
                Hud?.SetStatus(Localization.Tr("skill.mark_no_target"));
                return;
            }

            combat.DamageEnemy(target, damage, classController.Definition.SkillColor);
            target.MoveSpeed *= 0.7f;
            Hud?.SetStatus(Localization.Tr("skill.mark_hit", target.name));
        }

        private void ApplyDamageBuff(string label, int bonusDamage, float duration)
        {
            damageBuff = bonusDamage;
            buffUntil = Time.time + duration;
            upgradeSystem?.ApplyBonuses();
            Hud?.SetStatus(Localization.Tr("skill.buff", label));
            Hud?.RefreshClass();
        }

        private void PlaySkillMotion()
        {
            GetComponent<AvatarMotionAnimator>()?.PlayAttack();
            GetComponent<CombatFeedbackAudio>()?.PlaySkill();
            CombatFeedbackVfx.SpawnSkill(transform.position + Vector3.up * 0.85f, classController.Definition.SkillColor);
        }

        private int DamageFor(int slot, int baseDamage)
        {
            return baseDamage + Mathf.Max(0, SkillLevelFor(slot) - 1) * Mathf.Max(4, baseDamage / 5);
        }

        private float CooldownFor(int slot)
        {
            var baseCooldown = slot == 0 ? SkillOneCooldown
                : slot == 1 ? SkillTwoCooldown
                : slot == 2 ? SkillThreeCooldown
                : SkillFourCooldown;
            return Mathf.Max(1.5f, baseCooldown - Mathf.Max(0, SkillLevelFor(slot) - 1) * 0.45f);
        }

        private float Remaining(int slot)
        {
            return IsValidSlot(slot) ? Mathf.Max(0f, nextSkill[slot] - Time.time) : 0f;
        }

        private string SkillNameFor(int slot)
        {
            var definition = classController != null ? classController.Definition : null;
            if (definition == null) return string.Empty;
            return slot == 0 ? definition.SkillOneName
                : slot == 1 ? definition.SkillTwoName
                : slot == 2 ? definition.SkillThreeName
                : definition.SkillFourName;
        }

        private static string KeyFor(int slot)
        {
            return slot == 0 ? "Q" : slot == 1 ? "E" : slot == 2 ? "R" : "F";
        }

        private static bool IsValidSlot(int slot)
        {
            return slot >= 0 && slot < SkillSlotCount;
        }
    }
}
