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

            if (!Inventory.TryConsume(DefaultGameItems.DullOre))
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

            if (!Inventory.TryConsume(DefaultGameItems.WornRing))
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
            if (!Inventory.TryConsume(DefaultGameItems.MinorPotion))
            {
                Hud?.SetStatus("No tienes pociones.");
                return;
            }

            var definition = Inventory.Database != null
                ? Inventory.Database.Get(DefaultGameItems.MinorPotion) as ConsumableItemDefinition
                : null;
            var healAmount = definition != null ? definition.HealAmount : 45;

            health.Heal(healAmount);
            Hud?.SetStatus("Usaste Pocion menor.");
            Hud?.AddFeed($"Pocion usada: +{healAmount} vida");
            DamagePopup.Spawn(transform.position + Vector3.up * 2.15f, $"+{healAmount}", new Color(0.35f, 1f, 0.78f));
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

        public void RestoreUpgrades(int weaponLevel, int armorLevel)
        {
            WeaponLevel = Mathf.Max(0, weaponLevel);
            ArmorLevel = Mathf.Max(0, armorLevel);
            ApplyBonuses();
        }

        public string Summary()
        {
            var equipment = GetComponent<PlayerEquipment>();
            var pieces = equipment != null ? equipment.Summary() : "sin piezas";
            return $"Equipo: arma +{WeaponLevel} | armadura +{ArmorLevel} | {pieces}";
        }

        // Punto unico de recomputo de stats: mejoras +N y piezas equipadas.
        public void ApplyBonuses()
        {
            var equipment = GetComponent<PlayerEquipment>();
            var equipDamage = equipment != null ? equipment.TotalDamageBonus : 0;
            var equipHealth = equipment != null ? equipment.TotalMaxHealthBonus : 0;
            var equipSpeed = equipment != null ? equipment.TotalMoveSpeedBonus : 0f;

            combat.EquipmentDamageBonus = WeaponLevel * 4 + equipDamage;

            var classController = GetComponent<PlayerClassController>();
            if (classController != null && classController.Definition != null)
            {
                var definition = classController.Definition;
                var totalHealthBonus = ArmorLevel * 15 + equipHealth;
                if (totalHealthBonus > 0 || health.MaxHealth != definition.MaxHealth)
                {
                    health.ResetHealth(definition.MaxHealth + totalHealthBonus);
                }

                var movement = GetComponent<PlayerController>();
                if (movement != null)
                {
                    movement.MoveSpeed = definition.MoveSpeed + equipSpeed;
                }
            }
            else if (ArmorLevel > 0 || equipHealth > 0)
            {
                health.ResetHealth(health.MaxHealth + ArmorLevel * 15 + equipHealth);
            }

            Hud?.RefreshEquipment();
            Hud?.RefreshClass();
        }
    }
}
