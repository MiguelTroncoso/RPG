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
        private float nextSkillOne;
        private float nextSkillTwo;
        private float buffUntil;
        private int baseDamageBeforeBuff;

        public float SkillOneRemaining => Mathf.Max(0f, nextSkillOne - Time.time);
        public float SkillTwoRemaining => Mathf.Max(0f, nextSkillTwo - Time.time);

        private void Awake()
        {
            combat = GetComponent<PlayerCombat>();
            classController = GetComponent<PlayerClassController>();
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
                combat.AttackDamage = baseDamageBeforeBuff;
                buffUntil = 0f;
                Hud?.RefreshClass();
            }
        }

        public void UseSkillOne()
        {
            if (Time.time < nextSkillOne)
            {
                Hud?.SetStatus("Habilidad 1 en recarga.");
                return;
            }

            nextSkillOne = Time.time + SkillOneCooldown;

            switch (classController.CurrentClass)
            {
                case CharacterClassType.Ninja:
                    NinjaDash();
                    break;
                case CharacterClassType.Chaman:
                    MagicBurst("Rayo espiritual", 38, 5.8f, classController.Definition.SkillColor);
                    break;
                case CharacterClassType.Umbra:
                    MagicBurst("Hoja oscura", 44, 3.2f, classController.Definition.SkillColor);
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
                Hud?.SetStatus("Habilidad 2 en recarga.");
                return;
            }

            nextSkillTwo = Time.time + SkillTwoCooldown;

            switch (classController.CurrentClass)
            {
                case CharacterClassType.Ninja:
                    ApplyDamageBuff("Sombra fugaz", 10, 3.5f);
                    break;
                case CharacterClassType.Chaman:
                    HealSelf();
                    break;
                case CharacterClassType.Umbra:
                    MarkNearbyEnemy();
                    break;
                default:
                    ApplyDamageBuff("Grito de batalla", 8, 4.5f);
                    break;
            }

            Hud?.RefreshSkillCooldowns(nextSkillOne - Time.time, nextSkillTwo - Time.time);
        }

        private void Cleave()
        {
            var hits = combat.DamageEnemiesInRange(2.8f, 34, 3);
            Hud?.SetStatus(hits > 0 ? $"Corte pesado golpeo {hits} enemigo(s)." : "Corte pesado no encontro objetivo.");
        }

        private void NinjaDash()
        {
            var controller = GetComponent<CharacterController>();
            var dash = transform.forward * 2.8f;
            controller.Move(dash);

            var hits = combat.DamageEnemiesInRange(2.35f, 31, 1);
            Hud?.SetStatus(hits > 0 ? "Estocada veloz conecto." : "Estocada veloz.");
            PrototypePulseAndDestroy.Spawn(transform.position + Vector3.up * 1.1f, classController.Definition.SkillColor);
        }

        private void MagicBurst(string label, int damage, float range, Color color)
        {
            var target = combat.FindNearestEnemy(range);
            if (target == null)
            {
                Hud?.SetStatus($"{label}: sin objetivo.");
                return;
            }

            combat.DamageEnemy(target, damage, color);
            Hud?.SetStatus($"{label} impacto a {target.name}.");
        }

        private void HealSelf()
        {
            var health = GetComponent<Health>();
            health.Heal(38);
            Hud?.SetStatus("Bendicion espiritual: curacion.");
            DamagePopup.Spawn(transform.position + Vector3.up * 2.15f, "+38", new Color(0.35f, 1f, 0.78f));
        }

        private void MarkNearbyEnemy()
        {
            var target = combat.FindNearestEnemy(4f);
            if (target == null)
            {
                Hud?.SetStatus("Marca del vacio: sin objetivo.");
                return;
            }

            combat.DamageEnemy(target, 18, classController.Definition.SkillColor);
            target.MoveSpeed *= 0.7f;
            Hud?.SetStatus($"Marca del vacio debilito a {target.name}.");
        }

        private void ApplyDamageBuff(string label, int bonusDamage, float duration)
        {
            if (buffUntil <= 0f)
            {
                baseDamageBeforeBuff = combat.AttackDamage;
            }

            combat.AttackDamage = baseDamageBeforeBuff + bonusDamage;
            buffUntil = Time.time + duration;
            Hud?.SetStatus($"{label}: dano aumentado.");
            Hud?.RefreshClass();
        }
    }
}
