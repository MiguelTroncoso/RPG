using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class WorldEventSystem : MonoBehaviour
    {
        public Transform Target;
        public PlayerProgression Progression;
        public InventorySystem Inventory;
        public PlayerQuestLog QuestLog;
        public PrototypeHud Hud;
        public float RespawnDelay = 55f;

        private readonly Vector3 spawnPosition = new Vector3(0f, 1f, -12f);
        private GameObject activeMonolith;
        private float nextSpawnTime;

        private void Start()
        {
            SpawnMonolith();
        }

        private void Update()
        {
            if (activeMonolith == null && Time.time >= nextSpawnTime)
            {
                SpawnMonolith();
            }
        }

        private void SpawnMonolith()
        {
            activeMonolith = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            activeMonolith.name = Localization.Tr("event.monolith_name");
            activeMonolith.transform.position = spawnPosition;
            activeMonolith.transform.localScale = new Vector3(1.2f, 2.2f, 1.2f);

            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.38f, 0.08f, 0.52f);
            activeMonolith.GetComponent<Renderer>().sharedMaterial = material;

            var collider = activeMonolith.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var controller = activeMonolith.AddComponent<CharacterController>();
            controller.height = 3.2f;
            controller.radius = 0.75f;

            var health = activeMonolith.AddComponent<Health>();
            health.ResetHealth(260);
            health.Died += HandleMonolithDestroyed;
            activeMonolith.AddComponent<HitFlashOnDamage>();

            var ai = activeMonolith.AddComponent<EnemyAI>();
            ai.Target = Target;
            ai.AggroRange = 0f;
            ai.AttackRange = 0f;
            ai.MoveSpeed = 0f;
            ai.AttackDamage = 0;

            var reward = activeMonolith.AddComponent<EnemyReward>();
            reward.Progression = Progression;
            reward.Inventory = Inventory;
            reward.QuestLog = QuestLog;
            reward.Hud = Hud;
            reward.Experience = 120;
            reward.GoldMin = 35;
            reward.GoldMax = 55;
            reward.GuaranteedDrop = DefaultGameItems.AncientFragment;
            reward.IsWorldEvent = true;

            CreateFloatingLabel(activeMonolith.transform, Localization.Tr("event.monolith_label"));
            Hud?.SetStatus(Localization.Tr("event.monolith_spawned"), 4f);
        }

        private void HandleMonolithDestroyed(Health _)
        {
            nextSpawnTime = Time.time + RespawnDelay;
            activeMonolith = null;
            Hud?.SetStatus(Localization.Tr("event.monolith_completed"), 4f);
        }

        private static void CreateFloatingLabel(Transform parent, string text)
        {
            var labelObject = new GameObject("Event Label");
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = new Vector3(0f, 1.4f, 0f);

            var label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.fontSize = 42;
            label.characterSize = 0.045f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = new Color(1f, 0.84f, 1f);
        }
    }
}
