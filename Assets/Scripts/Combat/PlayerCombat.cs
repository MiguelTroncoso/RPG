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

            var damage = Mathf.Max(1, AttackDamage + EquipmentDamageBonus + Random.Range(-4, 5));
            DamageEnemy(enemy, damage, new Color(1f, 0.9f, 0.28f));
            Hud?.SetStatus(health.IsDead ? $"{enemy.name} derrotado." : $"Golpeaste a {enemy.name} por {damage}.");
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

                DamageEnemy(enemy, damage, SkillTint);
                hits++;
            }

            return hits;
        }

        public void DamageEnemy(EnemyAI enemy, int damage, Color color)
        {
            if (enemy == null)
            {
                return;
            }

            var health = enemy.GetComponent<Health>();
            if (health == null || health.IsDead)
            {
                return;
            }

            FaceTarget(enemy.transform.position);
            health.TakeDamage(Mathf.Max(1, damage));
            Hud?.WatchEnemy(health);
            DamagePopup.Spawn(enemy.transform.position + Vector3.up * 2.15f, damage.ToString(), color);
            PrototypePulseAndDestroy.Spawn(enemy.transform.position + Vector3.up * 1.1f, color);
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
