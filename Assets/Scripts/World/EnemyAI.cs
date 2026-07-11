using UnityEngine;

namespace MmorpgPrototype
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Health))]
    public sealed class EnemyAI : MonoBehaviour
    {
        public Transform Target;
        public float AggroRange = 9f;
        public float AttackRange = 1.55f;
        public float MoveSpeed = 2.25f;
        public int AttackDamage = 8;
        public float AttackCooldown = 1.2f;

        private CharacterController controller;
        private Health health;
        private Renderer bodyRenderer;
        private float nextAttackTime;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            health = GetComponent<Health>();
            bodyRenderer = GetComponentInChildren<Renderer>();
        }

        private void OnEnable()
        {
            health.Died += HandleDeath;
        }

        private void OnDisable()
        {
            health.Died -= HandleDeath;
        }

        private void Start()
        {
            if (Target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                Target = player != null ? player.transform : null;
            }
        }

        private void Update()
        {
            if (health.IsDead || Target == null)
            {
                return;
            }

            var direction = Target.position - transform.position;
            direction.y = 0f;
            var distance = direction.magnitude;

            if (distance > AggroRange)
            {
                controller.SimpleMove(Vector3.zero);
                return;
            }

            if (distance > AttackRange)
            {
                var move = direction.normalized;
                controller.SimpleMove(move * MoveSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move, Vector3.up), 10f * Time.deltaTime);
                return;
            }

            controller.SimpleMove(Vector3.zero);
            TryAttackPlayer();
        }

        private void TryAttackPlayer()
        {
            if (Time.time < nextAttackTime)
            {
                return;
            }

            nextAttackTime = Time.time + AttackCooldown;

            var playerHealth = Target.GetComponent<Health>();
            if (playerHealth == null || playerHealth.IsDead)
            {
                return;
            }

            playerHealth.TakeDamage(AttackDamage);
            DamagePopup.Spawn(Target.position + Vector3.up * 2.15f, AttackDamage.ToString(), new Color(1f, 0.28f, 0.22f));
        }

        private void HandleDeath(Health _)
        {
            controller.enabled = false;

            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = new Color(0.24f, 0.24f, 0.24f);
            }

            Destroy(gameObject, 1.3f);
        }
    }
}

