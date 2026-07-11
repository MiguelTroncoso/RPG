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
            var enemy = FindNearestEnemy(AttackRange);

            if (enemy == null)
            {
                Hud?.SetStatus("No hay enemigos en rango.");
                return;
            }

            var health = enemy.GetComponent<Health>();
            if (health == null || health.IsDead)
            {
                Hud?.SetStatus("El objetivo ya fue derrotado.");
                return;
            }

            FaceTarget(enemy.transform.position);

            var result = DamageEnemy(enemy, AttackDamage + EquipmentDamageBonus, new Color(1f, 0.9f, 0.28f));
            if (result.IsMiss)
            {
                Hud?.SetStatus($"{enemy.name} esquivo tu ataque.");
            }
            else if (health.IsDead)
            {
                Hud?.SetStatus($"{enemy.name} derrotado.");
            }
            else
            {
                Hud?.SetStatus(result.IsCritical
                    ? $"Golpe critico a {enemy.name} por {result.Amount}."
                    : $"Golpeaste a {enemy.name} por {result.Amount}.");
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
                DamagePopup.Spawn(enemy.transform.position + Vector3.up * 2.15f, "Fallo", new Color(0.72f, 0.76f, 0.8f));
                return result;
            }

            health.TakeDamage(result.Amount);
            var popupText = result.IsCritical ? $"{result.Amount}!" : result.Amount.ToString();
            var popupColor = result.IsCritical ? new Color(1f, 0.55f, 0.12f) : color;
            DamagePopup.Spawn(enemy.transform.position + Vector3.up * 2.15f, popupText, popupColor);
            PrototypePulseAndDestroy.Spawn(enemy.transform.position + Vector3.up * 1.1f, popupColor);
            return result;
        }

        private CombatStats BuildAttackerStats(int baseDamage)
        {
            var attributes = GetComponent<PlayerAttributes>();
            var critBonus = attributes != null ? attributes.BonusCritChance : 0f;

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
