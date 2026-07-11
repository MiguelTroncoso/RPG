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
        public float RespawnDelay = 4.5f;

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
        }

        private void Update()
        {
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
