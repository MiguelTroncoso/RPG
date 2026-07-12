using UnityEngine;

namespace MmorpgPrototype
{
    [RequireComponent(typeof(PlayerCombat))]
    [RequireComponent(typeof(PlayerClassController))]
    public sealed class PlayerSkills : MonoBehaviour
    {
        public float SkillOneCooldown = 4f;
        public float SkillTwoCooldown = 7f;
        public PrototypeHud Hud;

        private PlayerCombat combat;
        private PlayerClassController classController;
        private EquipmentUpgradeSystem upgradeSystem;
        private float nextSkillOne;
        private float nextSkillTwo;
        private float buffUntil;
        private int damageBuff;

        public float SkillOneRemaining => Mathf.Max(0f, nextSkillOne - Time.time);
        public float SkillTwoRemaining => Mathf.Max(0f, nextSkillTwo - Time.time);
        public int DamageBuff => buffUntil > 0f ? damageBuff : 0;

        private void Awake()
        {
            combat = GetComponent<PlayerCombat>();
            classController = GetComponent<PlayerClassController>();
            upgradeSystem = GetComponent<EquipmentUpgradeSystem>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                UseSkillOne();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                UseSkillTwo();
            }

            if (buffUntil > 0f && Time.time > buffUntil)
            {
                damageBuff = 0;
                buffUntil = 0f;
                upgradeSystem?.ApplyBonuses();
                Hud?.RefreshClass();
            }
        }

        public void UseSkillOne()
        {
            if (Time.time < nextSkillOne)
            {
                Hud?.SetStatus(Localization.Tr("skill.one_cooldown"));
                return;
            }

            nextSkillOne = Time.time + SkillOneCooldown;
            PlaySkillMotion();

            switch (classController.CurrentClass)
            {
                case CharacterClassType.Ninja:
                    NinjaDash();
                    break;
                case CharacterClassType.Chaman:
                    MagicBurst(Localization.Tr("skill.spiritual_bolt"), 38, 5.8f, classController.Definition.SkillColor);
                    break;
                case CharacterClassType.Umbra:
                    MagicBurst(Localization.Tr("skill.dark_blade"), 44, 3.2f, classController.Definition.SkillColor);
                    break;
                default:
                    Cleave();
                    break;
            }

            Hud?.RefreshSkillCooldowns(nextSkillOne - Time.time, nextSkillTwo - Time.time);
        }

        public void UseSkillTwo()
        {
            if (Time.time < nextSkillTwo)
            {
                Hud?.SetStatus(Localization.Tr("skill.two_cooldown"));
                return;
            }

            nextSkillTwo = Time.time + SkillTwoCooldown;
            PlaySkillMotion();

            switch (classController.CurrentClass)
            {
                case CharacterClassType.Ninja:
                    ApplyDamageBuff(Localization.Tr("skill.shadow_dash"), 10, 3.5f);
                    break;
                case CharacterClassType.Chaman:
                    HealSelf();
                    break;
                case CharacterClassType.Umbra:
                    MarkNearbyEnemy();
                    break;
                default:
                    ApplyDamageBuff(Localization.Tr("skill.battle_cry"), 8, 4.5f);
                    break;
            }

            Hud?.RefreshSkillCooldowns(nextSkillOne - Time.time, nextSkillTwo - Time.time);
        }

        private void Cleave()
        {
            var hits = combat.DamageEnemiesInRange(2.8f, 34, 3);
            Hud?.SetStatus(hits > 0 ? Localization.Tr("skill.cleave_hit", hits) : Localization.Tr("skill.cleave_miss"));
        }

        private void NinjaDash()
        {
            var controller = GetComponent<CharacterController>();
            var dash = transform.forward * 2.8f;
            controller.Move(dash);

            var hits = combat.DamageEnemiesInRange(2.35f, 31, 1);
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

        private void HealSelf()
        {
            var health = GetComponent<Health>();
            health.Heal(38);
            Hud?.SetStatus(Localization.Tr("skill.heal"));
            DamagePopup.Spawn(transform.position + Vector3.up * 2.15f, "+38", new Color(0.35f, 1f, 0.78f));
        }

        private void MarkNearbyEnemy()
        {
            var target = combat.FindNearestEnemy(4f);
            if (target == null)
            {
                Hud?.SetStatus(Localization.Tr("skill.mark_no_target"));
                return;
            }

            combat.DamageEnemy(target, 18, classController.Definition.SkillColor);
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
    }
}
