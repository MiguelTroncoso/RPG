using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class PlayerCombat : MonoBehaviour
    {
        public int AttackDamage = 25;
        public float AttackRange = 2.2f;
        public float AttackCooldown = 0.7f;
        public PrototypeHud Hud;
        public Color SkillTint = new Color(1f, 0.72f, 0.18f);
        public int EquipmentDamageBonus;

        private float nextAttackTime;
        private PlayerStatSheet statSheet;
        private CombatFeedbackAudio feedback;

        private PlayerStatSheet Stats => statSheet != null ? statSheet : statSheet = GetComponent<PlayerStatSheet>();

        public int TotalAttackDamage => Stats != null ? Stats.Damage : AttackDamage + EquipmentDamageBonus;

        private void Awake()
        {
            statSheet = GetComponent<PlayerStatSheet>();
            feedback = GetComponent<CombatFeedbackAudio>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                TryAttack();
            }
        }

        public void TryAttack()
        {
            if (Time.time < nextAttackTime)
            {
                return;
            }

            nextAttackTime = Time.time + AttackCooldown;
            feedback?.PlayAttack();
            GetComponent<AvatarMotionAnimator>()?.PlayAttack();
            var enemy = FindNearestEnemy(AttackRange);

            if (enemy == null)
            {
                Hud?.SetStatus(Localization.Tr("combat.no_target"));
                return;
            }

            var health = enemy.GetComponent<Health>();
            if (health == null || health.IsDead)
            {
                Hud?.SetStatus(Localization.Tr("combat.already_dead"));
                return;
            }

            FaceTarget(enemy.transform.position);

            var result = DamageEnemy(enemy, TotalAttackDamage, new Color(1f, 0.9f, 0.28f));
            if (result.IsMiss)
            {
                Hud?.SetStatus(Localization.Tr("combat.dodged", enemy.name));
            }
            else if (health.IsDead)
            {
                Hud?.SetStatus(Localization.Tr("combat.defeated", enemy.name));
            }
            else
            {
                Hud?.SetStatus(result.IsCritical
                    ? Localization.Tr("combat.crit", enemy.name, result.Amount)
                    : Localization.Tr("combat.hit", enemy.name, result.Amount));
            }
        }

        public EnemyAI FindNearestEnemy(float range)
        {
            var enemies = FindObjectsByType<EnemyAI>();
            EnemyAI nearest = null;
            var nearestDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                var health = enemy.GetComponent<Health>();
                if (health == null || health.IsDead)
                {
                    continue;
                }

                var distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= range && distance < nearestDistance)
                {
                    nearest = enemy;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        public int DamageEnemiesInRange(float range, int damage, int maxTargets)
        {
            var enemies = FindObjectsByType<EnemyAI>();
            var hits = 0;

            foreach (var enemy in enemies)
            {
                if (hits >= maxTargets)
                {
                    break;
                }

                var health = enemy.GetComponent<Health>();
                if (health == null || health.IsDead)
                {
                    continue;
                }

                var distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance > range)
                {
                    continue;
                }

                var result = DamageEnemy(enemy, damage, SkillTint);
                if (!result.IsMiss)
                {
                    hits++;
                }
            }

            return hits;
        }

        // Resuelve el golpe con DamageCalculator (critico/evasion) y aplica
        // el resultado. baseDamage ya debe incluir los bonos que correspondan.
        public DamageResult DamageEnemy(EnemyAI enemy, int baseDamage, Color color)
        {
            if (enemy == null)
            {
                return new DamageResult(0, false, true);
            }

            var health = enemy.GetComponent<Health>();
            if (health == null || health.IsDead)
            {
                return new DamageResult(0, false, true);
            }

            FaceTarget(enemy.transform.position);

            var result = DamageCalculator.Resolve(
                BuildAttackerStats(baseDamage),
                enemy.DefenderStats,
                Random.value, Random.value, Random.value);

            Hud?.WatchEnemy(health);

            if (result.IsMiss)
            {
                feedback?.PlayMiss();
                DamagePopup.Spawn(enemy.transform.position + Vector3.up * 2.15f, Localization.Tr("combat.miss_popup"), new Color(0.72f, 0.76f, 0.8f));
                return result;
            }

            health.TakeDamage(result.Amount);
            feedback?.PlayHit(result.IsCritical);
            var popupText = result.IsCritical ? $"{result.Amount}!" : result.Amount.ToString();
            var popupColor = result.IsCritical ? new Color(1f, 0.55f, 0.12f) : color;
            DamagePopup.Spawn(enemy.transform.position + Vector3.up * 2.15f, popupText, popupColor, result.IsCritical ? 1.25f : 1f);
            PrototypePulseAndDestroy.Spawn(enemy.transform.position + Vector3.up * 1.1f, popupColor);
            CombatFeedbackVfx.SpawnHit(enemy.transform.position + Vector3.up * 1.1f, popupColor, result.IsCritical);
            return result;
        }

        private CombatStats BuildAttackerStats(int baseDamage)
        {
            var attributes = GetComponent<PlayerAttributes>();
            var critBonus = attributes != null ? attributes.BonusCritChance : 0f;

            var stats = Stats;
            if (stats != null)
            {
                return new CombatStats(
                    baseDamage,
                    stats.CritChance,
                    stats.CritMultiplier,
                    stats.Accuracy,
                    stats.Evasion,
                    stats.Defense);
            }

            var definition = GetComponent<PlayerClassController>()?.Definition;
            if (definition == null)
            {
                return new CombatStats(baseDamage, 0.08f + critBonus, 1.5f, 0.95f, 0f, 0);
            }

            return new CombatStats(
                baseDamage,
                definition.CritChance + critBonus,
                definition.CritMultiplier,
                definition.Accuracy,
                definition.Evasion,
                definition.Defense);
        }

        private void FaceTarget(Vector3 targetPosition)
        {
            var direction = targetPosition - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }
    }
}
