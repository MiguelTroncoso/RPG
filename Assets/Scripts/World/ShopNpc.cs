using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class ShopNpc : MonoBehaviour
    {
        public Transform Player;
        public PlayerProgression Progression;
        public InventorySystem Inventory;
        public EquipmentUpgradeSystem Equipment;
        public PrototypeHud Hud;
        public float InteractionRange = 4f;

        public void BuyPotion()
        {
            if (!IsPlayerNear())
            {
                return;
            }

            const int cost = 25;
            if (!Progression.SpendGold(cost))
            {
                Hud?.SetStatus($"Necesitas {cost} oro para comprar pocion.");
                return;
            }

            Inventory.AddItem(DefaultGameItems.MinorPotion);
            Hud?.SetStatus("Compraste Pocion menor.");
            Hud?.AddFeed("Mercader: pocion comprada");
        }

        public void UpgradeWeapon()
        {
            if (IsPlayerNear())
            {
                Equipment.TryUpgradeWeapon();
            }
        }

        public void UpgradeArmor()
        {
            if (IsPlayerNear())
            {
                Equipment.TryUpgradeArmor();
            }
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

            Hud?.SetStatus("Acercate al Mercader del Valle.");
            return false;
        }
    }
}
