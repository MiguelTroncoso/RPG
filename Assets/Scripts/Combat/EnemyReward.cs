using UnityEngine;

namespace MmorpgPrototype
{
    [RequireComponent(typeof(Health))]
    public sealed class EnemyReward : MonoBehaviour, ILootSource
    {
        public int Experience = 35;
        public int GoldMin = 3;
        public int GoldMax = 9;
        public string GuaranteedDrop = string.Empty;
        public LootTableConfig Loot;
        public EnemyTier Tier = EnemyTier.Normal;
        public string EnemyId = string.Empty;
        public string ZoneId = string.Empty;
        public bool IsWorldEvent;
        public PlayerProgression Progression;
        public InventorySystem Inventory;
        public PlayerQuestLog QuestLog;
        public PrototypeHud Hud;
        public CombatTelemetry Telemetry;

        private Health health;
        private bool granted;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            health.Died += Grant;
            health.Damaged += TrackDamage;
        }

        private void OnDisable()
        {
            health.Died -= Grant;
            health.Damaged -= TrackDamage;
        }

        private void TrackDamage(Health source, int amount)
        {
            Telemetry?.RecordEnemyDamaged(source, Tier, EnemyId, ZoneId, amount);
        }

        private void Grant(Health _)
        {
            if (granted || Progression == null)
            {
                return;
            }

            granted = true;
            var gold = Random.Range(GoldMin, GoldMax + 1);
            var drop = RollLoot();

            Progression.AddExperience(Experience);
            Progression.AddGold(gold);
            QuestLog?.OnEnemyDefeated(Tier, EnemyId, IsWorldEvent);
            Telemetry?.RecordEnemyDefeated(health, Tier, EnemyId, ZoneId);

            if (!string.IsNullOrEmpty(drop))
            {
                Inventory?.AddItem(drop);
            }

            var dropName = Inventory != null ? Inventory.DisplayNameOf(drop) : drop;
            var message = string.IsNullOrEmpty(drop)
                ? Localization.Tr("reward.kill", Experience, gold)
                : Localization.Tr("reward.kill_loot", Experience, gold, dropName);
            Hud?.SetStatus(message, 3f);
            Hud?.AddFeed(Localization.Tr("reward.kill_feed", Experience, gold));
        }

        public string RollLoot()
        {
            if (!string.IsNullOrEmpty(GuaranteedDrop))
            {
                return GuaranteedDrop;
            }

            return Loot != null ? Loot.Roll(Random.value, Random.value) : string.Empty;
        }
    }
}
