using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class ShopNpc : MonoBehaviour, IInteractable
    {
        public string NpcId = DefaultQuests.MerchantNpcId;
        public Transform Player;
        public PlayerProgression Progression;
        public InventorySystem Inventory;
        public PlayerQuestLog QuestLog;
        public PrototypeHud Hud;
        public float InteractionRange = 4f;

        public void Interact()
        {
            Talk();
        }

        public void Talk()
        {
            if (!IsPlayerNear())
            {
                return;
            }

            var dialog = QuestLog != null ? QuestLog.DialogFor(NpcId) : null;
            Hud?.SetStatus(dialog ?? Localization.Tr("shop.dialog"), 5f);
            QuestLog?.OnNpcTalked(NpcId);
        }

        public void BuyPotion()
        {
            if (!IsPlayerNear())
            {
                return;
            }

            const int cost = 25;
            if (!Progression.SpendGold(cost))
            {
                Hud?.SetStatus(Localization.Tr("shop.need_gold", cost));
                return;
            }

            Inventory.AddItem(DefaultGameItems.MinorPotion);
            Hud?.SetStatus(Localization.Tr("shop.bought_potion"));
            Hud?.AddFeed(Localization.Tr("shop.feed_potion"));
        }

        private bool IsPlayerNear()
        {
            if (Player == null)
            {
                return false;
            }

            if (Vector3.Distance(transform.position, Player.position) <= InteractionRange)
            {
                return true;
            }

            Hud?.SetStatus(Localization.Tr("shop.too_far"));
            return false;
        }
    }
}
