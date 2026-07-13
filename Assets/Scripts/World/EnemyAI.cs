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
        public float Evasion = 0.03f;
        public int Defense;
        public float CritChance = 0.05f;
        public float Accuracy = 0.92f;
        public float AttackWindup = 0.28f;

        public CombatStats DefenderStats => CombatStats.Defender(Evasion, Defense);

        private CharacterController controller;
        private Health health;
        private Renderer bodyRenderer;
        private AvatarMotionAnimator motionAnimator;
        private float nextAttackTime;
        private bool isWindingUp;
        private float windupEndsAt;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            health = GetComponent<Health>();
            bodyRenderer = GetComponentInChildren<Renderer>();
            motionAnimator = GetComponent<AvatarMotionAnimator>();
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

            if (isWindingUp)
            {
                controller.SimpleMove(Vector3.zero);
                FaceTarget(direction);

                if (distance > AttackRange * 1.35f)
                {
                    isWindingUp = false;
                    nextAttackTime = Time.time + AttackCooldown * 0.35f;
                    return;
                }

                if (Time.time >= windupEndsAt)
                {
                    isWindingUp = false;
                    ResolveAttack();
                }

                return;
            }

            if (distance > AggroRange)
            {
                controller.SimpleMove(Vector3.zero);
                return;
            }

            if (distance > AttackRange)
            {
                var move = direction.normalized;
                controller.SimpleMove(move * MoveSpeed);
                FaceTarget(direction);
                return;
            }

            controller.SimpleMove(Vector3.zero);
            StartAttackWindup();
        }

        private void StartAttackWindup()
        {
            if (Time.time < nextAttackTime)
            {
                return;
            }

            var playerHealth = Target.GetComponent<Health>();
            if (playerHealth == null || playerHealth.IsDead)
            {
                return;
            }

            isWindingUp = true;
            windupEndsAt = Time.time + AttackWindup;
            nextAttackTime = windupEndsAt + AttackCooldown;
            motionAnimator?.PlayAttack();
            DamagePopup.Spawn(transform.position + Vector3.up * 2.35f, "!", new Color(1f, 0.58f, 0.18f), 1.35f);
            PrototypePulseAndDestroy.Spawn(Target.position + Vector3.up * 1.05f, new Color(1f, 0.28f, 0.16f));
            CombatFeedbackVfx.SpawnEnemyTelegraph(transform.position + Vector3.up * 0.1f, new Color(1f, 0.42f, 0.12f));
        }

        private void ResolveAttack()
        {
            var playerHealth = Target.GetComponent<Health>();
            if (playerHealth == null || playerHealth.IsDead)
            {
                return;
            }

            var playerStats = Target.GetComponent<PlayerStatSheet>();
            var playerDefinition = Target.GetComponent<PlayerClassController>()?.Definition;
            var defender = playerStats != null
                ? CombatStats.Defender(playerStats.Evasion, playerStats.Defense)
                : playerDefinition != null
                    ? CombatStats.Defender(playerDefinition.Evasion, playerDefinition.Defense)
                    : CombatStats.Defender(0f, 0);

            var attacker = new CombatStats(AttackDamage, CritChance, 1.5f, Accuracy, Evasion, Defense);
            var result = DamageCalculator.Resolve(attacker, defender, Random.value, Random.value, Random.value);

            if (result.IsMiss)
            {
                DamagePopup.Spawn(Target.position + Vector3.up * 2.15f, Localization.Tr("combat.dodge_popup"), new Color(0.6f, 0.85f, 1f));
                return;
            }

            playerHealth.TakeDamage(result.Amount);
            Target.GetComponent<CombatFeedbackAudio>()?.PlayPlayerDamage();
            var text = result.IsCritical ? $"{result.Amount}!" : result.Amount.ToString();
            DamagePopup.Spawn(Target.position + Vector3.up * 2.15f, text, new Color(1f, 0.28f, 0.22f), result.IsCritical ? 1.25f : 1f);
            CombatFeedbackVfx.SpawnHit(Target.position + Vector3.up * 1.05f, new Color(1f, 0.28f, 0.22f), result.IsCritical);
        }

        private void FaceTarget(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            var lookRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
        }

        private void HandleDeath(Health _)
        {
            controller.enabled = false;
            GetComponent<EnemyVisualController>()?.PlayDeath();

            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = new Color(0.24f, 0.24f, 0.24f);
            }

            Destroy(gameObject, 1.3f);
        }
    }
}
