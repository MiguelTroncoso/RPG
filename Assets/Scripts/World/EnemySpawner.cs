using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Spawner por zona: enemigos normales, pack de elites y jefe salen de
    // los datos de ZoneDefinition. Una instancia por zona.
    public sealed class EnemySpawner : MonoBehaviour
    {
        public Transform Target;
        public PlayerProgression Progression;
        public InventorySystem Inventory;
        public PlayerQuestLog QuestLog;
        public PrototypeHud Hud;
        public LootTableConfig Loot;
        public ZoneDefinition Zone;
        public CombatTelemetry Telemetry;
        public float RespawnDelay = 4.5f;
        public float ActivationDistance = 112f;

        private readonly List<GameObject> normals = new List<GameObject>();
        private readonly List<GameObject> elites = new List<GameObject>();
        private GameObject boss;
        private bool bossAlive;
        private float nextNormalSpawn;
        private float nextEliteSpawn;
        private float nextBossSpawn;
        private int lastEliteCount;
        private bool zoneActive;

        private void Start()
        {
            if (Zone == null)
            {
                return;
            }

            RefreshZoneState(true);
        }

        private void SpawnZone()
        {
            var mobile = Application.platform == RuntimePlatform.Android;
            var normalCount = ZoneBalanceResolver.NormalCountFor(Zone, mobile);
            var eliteCount = ZoneBalanceResolver.EliteCountFor(Zone, mobile);

            for (var i = 0; i < normalCount; i++)
            {
                SpawnNormal();
            }

            for (var i = 0; i < eliteCount; i++)
            {
                SpawnElite();
            }

            SpawnBoss();
            lastEliteCount = elites.Count;
        }

        private void Update()
        {
            if (Zone == null)
            {
                return;
            }

            RefreshZoneState(false);
            if (!zoneActive)
            {
                return;
            }

            NormalTick();
            EliteTick();
            BossTick();
        }

        private void RefreshZoneState(bool force)
        {
            var shouldBeActive = Target == null || Vector3.Distance(Target.position, Zone.GroundCenter) <= ActivationDistance;
            if (!force && shouldBeActive == zoneActive)
            {
                return;
            }

            zoneActive = shouldBeActive;
            if (zoneActive)
            {
                SpawnZone();
                return;
            }

            ClearZone();
        }

        private void ClearZone()
        {
            foreach (var enemy in normals)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }

            foreach (var elite in elites)
            {
                if (elite != null)
                {
                    Destroy(elite);
                }
            }

            if (boss != null)
            {
                Destroy(boss);
            }

            normals.Clear();
            elites.Clear();
            boss = null;
            bossAlive = false;
            lastEliteCount = 0;
            nextNormalSpawn = Time.time + RespawnDelay;
            nextEliteSpawn = Time.time + Zone.EliteRespawnSeconds;
            nextBossSpawn = Time.time + Zone.BossRespawnSeconds;
        }

        private void NormalTick()
        {
            normals.RemoveAll(enemy => enemy == null);

            var normalTarget = ZoneBalanceResolver.NormalCountFor(Zone, Application.platform == RuntimePlatform.Android);
            if (normals.Count >= normalTarget || Time.time < nextNormalSpawn)
            {
                return;
            }

            nextNormalSpawn = Time.time + RespawnDelay;
            SpawnNormal();
        }

        private void EliteTick()
        {
            elites.RemoveAll(elite => elite == null);

            if (elites.Count < lastEliteCount)
            {
                nextEliteSpawn = Time.time + Zone.EliteRespawnSeconds;
            }

            lastEliteCount = elites.Count;

            var eliteTarget = ZoneBalanceResolver.EliteCountFor(Zone, Application.platform == RuntimePlatform.Android);
            if (elites.Count < eliteTarget && Time.time >= nextEliteSpawn)
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
                Hud?.AddFeed(Localization.Tr("zone.boss_respawn", Zone.BossName));
                return;
            }

            if (boss == null && Time.time >= nextBossSpawn)
            {
                SpawnBoss();
            }
        }

        private void SpawnNormal()
        {
            var position = RandomInArea(Zone.NormalAreaCenter, Zone.NormalAreaRadius);
            var enemy = SpawnCustomEnemy(
                $"{Zone.NormalName} {normals.Count + 1}", position, 1f, Zone.NormalColor,
                Zone.NormalHealth, Zone.NormalDamage, Zone.NormalDefense, Zone.NormalMoveSpeed,
                Zone.NormalExp, Zone.NormalGoldMin, Zone.NormalGoldMax,
                EnemyTier.Normal, Zone.NormalEnemyId, string.Empty);
            normals.Add(enemy);
        }

        private void SpawnElite()
        {
            var position = RandomInArea(Zone.EliteAreaCenter, Zone.EliteAreaRadius);
            var elite = SpawnCustomEnemy(
                $"{Zone.EliteName} (Elite)", position, 1.25f, new Color(0.5f, 0.2f, 0.62f),
                Zone.EliteHealth, Zone.EliteDamage, Zone.EliteDefense, 2.45f,
                Zone.EliteExp, Zone.EliteGoldMin, Zone.EliteGoldMax,
                EnemyTier.Elite, Zone.EliteEnemyId, string.Empty);
            CreateEnemyLabel(elite.transform, Localization.Tr("zone.elite_label"), new Color(0.85f, 0.55f, 1f), 1.7f);
            elites.Add(elite);
        }

        private void SpawnBoss()
        {
            boss = SpawnCustomEnemy(
                $"{Zone.BossName} (Jefe)", Zone.BossPosition, 1.7f, new Color(0.55f, 0.12f, 0.1f),
                Zone.BossHealth, Zone.BossDamage, Zone.BossDefense, 1.9f,
                Zone.BossExp, Zone.BossGoldMin, Zone.BossGoldMax,
                EnemyTier.Boss, Zone.BossEnemyId, Zone.BossGuaranteedDrop);
            CreateEnemyLabel(boss.transform, Localization.Tr("zone.boss_label"), new Color(1f, 0.45f, 0.35f), 1.95f);
            bossAlive = true;
        }

        private static Vector3 RandomInArea(Vector3 center, float radius)
        {
            var offset = Random.insideUnitCircle * radius;
            return center + new Vector3(offset.x, 0f, offset.y);
        }

        private GameObject SpawnCustomEnemy(string enemyName, Vector3 position, float scale, Color color,
            int health, int damage, int defense, float moveSpeed,
            int exp, int goldMin, int goldMax, EnemyTier tier, string enemyId, string guaranteedDrop)
        {
            var enemy = new GameObject(enemyName);
            enemy.name = enemyName;
            enemy.transform.position = position;

            var controller = enemy.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.4f * scale;

            var enemyHealth = enemy.AddComponent<Health>();
            enemyHealth.ResetHealth(health);

            var visuals = enemy.AddComponent<EnemyVisualController>();
            visuals.Initialize(enemyId, enemyName, tier, color, scale, enemyHealth);

            var flash = enemy.AddComponent<HitFlashOnDamage>();
            flash.FlashColor = tier == EnemyTier.Boss
                ? new Color(1f, 0.72f, 0.28f)
                : new Color(1f, 0.36f, 0.28f);

            var ai = enemy.AddComponent<EnemyAI>();
            ai.Target = Target;
            ai.MoveSpeed = moveSpeed;
            ai.AttackDamage = damage;
            ai.Defense = defense;
            ai.Evasion = tier == EnemyTier.Normal ? 0.03f : 0.05f;
            ai.AggroRange = tier == EnemyTier.Normal ? 9f : 10f;
            ai.AttackCooldown = tier == EnemyTier.Boss ? 1.55f : tier == EnemyTier.Elite ? 1.35f : 1.65f;
            ai.AttackWindup = tier == EnemyTier.Boss ? 0.48f : tier == EnemyTier.Elite ? 0.36f : 0.28f;

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
            reward.EnemyId = enemyId ?? string.Empty;
            reward.ZoneId = Zone != null ? Zone.DisplayName : string.Empty;
            reward.GuaranteedDrop = guaranteedDrop ?? string.Empty;
            reward.Telemetry = Telemetry;

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
    }
}
