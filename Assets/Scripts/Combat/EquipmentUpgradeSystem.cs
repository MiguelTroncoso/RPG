using UnityEngine;

namespace MmorpgPrototype
{
    [RequireComponent(typeof(PlayerCombat))]
    [RequireComponent(typeof(Health))]
    public sealed class EquipmentUpgradeSystem : MonoBehaviour
    {
        public int WeaponLevel { get; private set; }
        public int ArmorLevel { get; private set; }

        public PrototypeHud Hud;
        public PlayerProgression Progression;
        public InventorySystem Inventory;

        private PlayerCombat combat;
        private Health health;

        private void Awake()
        {
            combat = GetComponent<PlayerCombat>();
            health = GetComponent<Health>();
        }

        public void TryUpgradeWeapon()
        {
            var cost = 40 + WeaponLevel * 25;
            if (!CanUseShop(cost))
            {
                return;
            }

            if (!Inventory.TryConsume("Mineral opaco"))
            {
                Hud?.SetStatus("Necesitas Mineral opaco para mejorar arma.");
                return;
            }

            if (!Progression.SpendGold(cost))
            {
                Hud?.SetStatus($"Necesitas {cost} oro para mejorar arma.");
                return;
            }

            WeaponLevel++;
            combat.EquipmentDamageBonus = WeaponLevel * 4;
            Hud?.SetStatus($"Arma mejorada a +{WeaponLevel}.");
            Hud?.AddFeed($"Arma +{WeaponLevel}: dano aumentado");
            Hud?.RefreshEquipment();
            Hud?.RefreshClass();
        }

        public void TryUpgradeArmor()
        {
            var cost = 35 + ArmorLevel * 20;
            if (!CanUseShop(cost))
            {
                return;
            }

            if (!Inventory.TryConsume("Anillo gastado"))
            {
                Hud?.SetStatus("Necesitas Anillo gastado para reforzar armadura.");
                return;
            }

            if (!Progression.SpendGold(cost))
            {
                Hud?.SetStatus($"Necesitas {cost} oro para reforzar armadura.");
                return;
            }

            ArmorLevel++;
            ApplyBonuses();
            Hud?.SetStatus($"Armadura reforzada a +{ArmorLevel}.");
            Hud?.AddFeed($"Armadura +{ArmorLevel}: vida maxima aumentada");
            Hud?.RefreshEquipment();
        }

        public void TryUsePotion()
        {
            if (!Inventory.TryConsume("Pocion menor"))
            {
                Hud?.SetStatus("No tienes pociones.");
                return;
            }

            health.Heal(45);
            Hud?.SetStatus("Usaste Pocion menor.");
            Hud?.AddFeed("Pocion usada: +45 vida");
            DamagePopup.Spawn(transform.position + Vector3.up * 2.15f, "+45", new Color(0.35f, 1f, 0.78f));
        }

        private bool CanUseShop(int cost)
        {
            if (Progression == null || Inventory == null)
            {
                Hud?.SetStatus("Sistema de tienda no inicializado.");
                return false;
            }

            if (Progression.Gold < cost)
            {
                Hud?.SetStatus($"Oro insuficiente: necesitas {cost}.");
                return false;
            }

            return true;
        }

        public string Summary()
        {
            return $"Equipo: arma +{WeaponLevel} (+{combat.EquipmentDamageBonus} dano) | armadura +{ArmorLevel}";
        }

        public void ApplyBonuses()
        {
            combat.EquipmentDamageBonus = WeaponLevel * 4;

            if (ArmorLevel > 0)
            {
                var classController = GetComponent<PlayerClassController>();
                var baseHealth = classController != null && classController.Definition != null
                    ? classController.Definition.MaxHealth
                    : health.MaxHealth;
                health.ResetHealth(baseHealth + ArmorLevel * 15);
            }

            Hud?.RefreshEquipment();
            Hud?.RefreshClass();
        }
    }
}
