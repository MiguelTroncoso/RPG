using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        public Transform Target;
        public PlayerProgression Progression;
        public InventorySystem Inventory;
        public PlayerQuestLog QuestLog;
        public PrototypeHud Hud;
        public LootTableConfig Loot;
        public ZoneDefinition Zone;
        public float RespawnDelay = 4.5f;

        private readonly System.Collections.Generic.List<GameObject> elites = new System.Collections.Generic.List<GameObject>();
        private GameObject boss;
        private bool bossAlive;
        private float nextEliteSpawn;
        private float nextBossSpawn;
        private int lastEliteCount;

        private readonly Vector3[] positions =
        {
            new Vector3(4f, 1f, 5f),
            new Vector3(-6f, 1f, 3f),
            new Vector3(7f, 1f, -6f),
            new Vector3(-5f, 1f, -7f),
            new Vector3(0f, 1f, 9f)
        };

        private float nextSpawnCheck;

        private void Start()
        {
            SpawnAll();

            if (Zone != null)
            {
                for (var i = 0; i < Zone.EliteCount; i++)
                {
                    SpawnElite();
                }

                SpawnBoss();
                lastEliteCount = elites.Count;
            }
        }

        private void Update()
        {
            if (Zone != null)
            {
                EliteTick();
                BossTick();
            }

            if (Time.time < nextSpawnCheck)
            {
                return;
            }

            nextSpawnCheck = Time.time + RespawnDelay;

            if (CountSpawnedEnemies() < positions.Length)
            {
                SpawnOne(Random.Range(0, positions.Length));
            }
        }

        private void EliteTick()
        {
            elites.RemoveAll(elite => elite == null);

            if (elites.Count < lastEliteCount)
            {
                nextEliteSpawn = Time.time + Zone.EliteRespawnSeconds;
            }

            lastEliteCount = elites.Count;

            if (elites.Count < Zone.EliteCount && Time.time >= nextEliteSpawn)
            {
                SpawnElite();
                lastEliteCount = elites.Count;
            }
        }

        private void BossTick()
        {
            if (boss == null && bossAlive)
            {
                bossAlive = false;
                nextBossSpawn = Time.time + Zone.BossRespawnSeconds;
                Hud?.AddFeed($"{Zone.BossName} reaparecera pronto");
                return;
            }

            if (boss == null && Time.time >= nextBossSpawn)
            {
                SpawnBoss();
            }
        }

        private void SpawnElite()
        {
            var offset = Random.insideUnitCircle * Zone.EliteAreaRadius;
            var position = Zone.EliteAreaCenter + new Vector3(offset.x, 0f, offset.y);
            var elite = SpawnCustomEnemy(
                $"{Zone.EliteName} (Elite)", position, 1.25f, new Color(0.5f, 0.2f, 0.62f),
                Zone.EliteHealth, Zone.EliteDamage, Zone.EliteDefense, 2.45f,
                Zone.EliteExp, Zone.EliteGoldMin, Zone.EliteGoldMax, EnemyTier.Elite, string.Empty);
            CreateEnemyLabel(elite.transform, "Elite", new Color(0.85f, 0.55f, 1f), 1.7f);
            elites.Add(elite);
        }

        private void SpawnBoss()
        {
            boss = SpawnCustomEnemy(
                $"{Zone.BossName} (Jefe)", Zone.BossPosition, 1.7f, new Color(0.55f, 0.12f, 0.1f),
                Zone.BossHealth, Zone.BossDamage, Zone.BossDefense, 1.9f,
                Zone.BossExp, Zone.BossGoldMin, Zone.BossGoldMax, EnemyTier.Boss, Zone.BossGuaranteedDrop);
            CreateEnemyLabel(boss.transform, "Jefe de zona", new Color(1f, 0.45f, 0.35f), 1.95f);
            bossAlive = true;
        }

        private GameObject SpawnCustomEnemy(string enemyName, Vector3 position, float scale, Color color,
            int health, int damage, int defense, float moveSpeed,
            int exp, int goldMin, int goldMax, EnemyTier tier, string guaranteedDrop)
        {
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = enemyName;
            enemy.transform.position = position;
            enemy.transform.localScale = Vector3.one * scale;

            var material = new Material(Shader.Find("Standard"));
            material.color = color;
            enemy.GetComponent<Renderer>().sharedMaterial = material;

            var capsule = enemy.GetComponent<CapsuleCollider>();
            if (capsule != null)
            {
                Destroy(capsule);
            }

            var controller = enemy.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.4f * scale;

            var enemyHealth = enemy.AddComponent<Health>();
            enemyHealth.ResetHealth(health);

            var ai = enemy.AddComponent<EnemyAI>();
            ai.Target = Target;
            ai.MoveSpeed = moveSpeed;
            ai.AttackDamage = damage;
            ai.Defense = defense;
            ai.Evasion = 0.05f;
            ai.AggroRange = 10f;

            var reward = enemy.AddComponent<EnemyReward>();
            reward.Progression = Progression;
            reward.Inventory = Inventory;
            reward.QuestLog = QuestLog;
            reward.Hud = Hud;
            reward.Loot = Loot;
            reward.Experience = exp;
            reward.GoldMin = goldMin;
            reward.GoldMax = goldMax;
            reward.Tier = tier;
            reward.GuaranteedDrop = guaranteedDrop ?? string.Empty;

            return enemy;
        }

        private static void CreateEnemyLabel(Transform parent, string text, Color color, float height)
        {
            var labelObject = new GameObject("Tier Label");
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = new Vector3(0f, height, 0f);

            var label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.fontSize = 34;
            label.characterSize = 0.05f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = color;
        }

        private void SpawnAll()
        {
            for (var i = 0; i < positions.Length; i++)
            {
                SpawnOne(i);
            }
        }

        private void SpawnOne(int index)
        {
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = index % 2 == 0 ? $"Lobo corrupto {index + 1}" : $"Bandido errante {index + 1}";
            enemy.transform.position = positions[index] + new Vector3(Random.Range(-1.2f, 1.2f), 0f, Random.Range(-1.2f, 1.2f));

            var material = new Material(Shader.Find("Standard"));
            material.color = index % 2 == 0 ? new Color(0.78f, 0.18f, 0.16f) : new Color(0.58f, 0.24f, 0.1f);
            enemy.GetComponent<Renderer>().sharedMaterial = material;

            var capsule = enemy.GetComponent<CapsuleCollider>();
            if (capsule != null)
            {
                Destroy(capsule);
            }

            var controller = enemy.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.4f;

            var health = enemy.AddComponent<Health>();
            health.ResetHealth(74 + index * 12);

            var ai = enemy.AddComponent<EnemyAI>();
            ai.Target = Target;
            ai.MoveSpeed = 2.05f + index * 0.11f;
            ai.AttackDamage = 7 + index;
            ai.Defense = index / 2;
            ai.Evasion = 0.03f + index * 0.005f;

            var reward = enemy.AddComponent<EnemyReward>();
            reward.Loot = Loot;
            reward.Progression = Progression;
            reward.Inventory = Inventory;
            reward.QuestLog = QuestLog;
            reward.Hud = Hud;
            reward.Experience = 30 + index * 8;
            reward.GoldMin = 3 + index;
            reward.GoldMax = 8 + index * 2;
        }

        private static int CountSpawnedEnemies()
        {
            var enemies = FindObjectsByType<EnemyAI>();
            var count = 0;

            foreach (var enemy in enemies)
            {
                if (enemy.name.StartsWith("Lobo corrupto") || enemy.name.StartsWith("Bandido errante"))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
