using UnityEngine;

namespace MmorpgPrototype
{
    [RequireComponent(typeof(Health))]
    public sealed class EnemyReward : MonoBehaviour
    {
        public int Experience = 35;
        public int GoldMin = 3;
        public int GoldMax = 9;
        public string GuaranteedDrop = string.Empty;
        public bool IsWorldEvent;
        public PlayerProgression Progression;
        public InventorySystem Inventory;
        public PlayerQuestLog QuestLog;
        public PrototypeHud Hud;

        private Health health;
        private bool granted;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            health.Died += Grant;
        }

        private void OnDisable()
        {
            health.Died -= Grant;
        }

        private void Grant(Health _)
        {
            if (granted || Progression == null)
            {
                return;
            }

            granted = true;
            var gold = Random.Range(GoldMin, GoldMax + 1);
            var drop = string.IsNullOrEmpty(GuaranteedDrop) ? LootTable.RollDrop() : GuaranteedDrop;

            Progression.AddExperience(Experience);
            Progression.AddGold(gold);
            QuestLog?.OnEnemyDefeated(IsWorldEvent);

            if (!string.IsNullOrEmpty(drop))
            {
                Inventory?.AddItem(drop);
            }

            var dropName = Inventory != null ? Inventory.DisplayNameOf(drop) : drop;
            var message = string.IsNullOrEmpty(drop)
                ? $"+{Experience} EXP, +{gold} oro"
                : $"+{Experience} EXP, +{gold} oro, loot: {dropName}";
            Hud?.SetStatus(message, 3f);
            Hud?.AddFeed($"+{Experience} EXP  +{gold} oro");
        }
    }
}
