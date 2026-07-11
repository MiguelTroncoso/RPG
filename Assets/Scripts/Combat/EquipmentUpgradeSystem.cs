using UnityEngine;

namespace MmorpgPrototype
{
    // Mejora +0..+15 de las piezas equipadas segun UpgradeConfig (costos,
    // materiales, probabilidades y politica de fallo como datos). La regla
    // se resuelve en UpgradeResolver (puro); aqui solo hay orquestacion y
    // presentacion. Tambien es el punto unico de recomputo de stats.
    [RequireComponent(typeof(PlayerCombat))]
    [RequireComponent(typeof(Health))]
    public sealed class EquipmentUpgradeSystem : MonoBehaviour
    {
        public UpgradeConfig Config;
        public PrototypeHud Hud;
        public PlayerProgression Progression;
        public InventorySystem Inventory;

        private PlayerCombat combat;
        private Health health;

        // Derivados de las piezas equipadas; se conservan para el guardado
        // legacy y como informacion rapida.
        public int WeaponLevel => LevelOf(EquipSlot.Weapon);
        public int ArmorLevel => LevelOf(FindArmorSlot());

        private void Awake()
        {
            combat = GetComponent<PlayerCombat>();
            health = GetComponent<Health>();
        }

        public void TryUpgradeWeapon()
        {
            var gear = GetComponent<PlayerEquipment>();
            var instance = gear != null ? gear.GetEquipped(EquipSlot.Weapon) : null;
            if (instance == null)
            {
                Hud?.SetStatus("Equipa un arma primero (boton EQUIPAR).");
                return;
            }

            TryUpgradeInstance(gear, instance, EquipSlot.Weapon, DefaultGameItems.DullOre);
        }

        public void TryUpgradeArmor()
        {
            var gear = GetComponent<PlayerEquipment>();
            var slot = FindArmorSlot();
            var instance = gear != null && slot != EquipSlot.None ? gear.GetEquipped(slot) : null;
            if (instance == null)
            {
                Hud?.SetStatus("Equipa una pieza de armadura primero (boton EQUIPAR).");
                return;
            }

            TryUpgradeInstance(gear, instance, slot, DefaultGameItems.WornRing);
        }

        private void TryUpgradeInstance(PlayerEquipment gear, ItemInstance instance, EquipSlot slot, string materialId)
        {
            if (Progression == null || Inventory == null)
            {
                Hud?.SetStatus("Sistema de mejora no inicializado.");
                return;
            }

            var config = EnsureConfig();
            var itemName = Inventory.DisplayNameOf(instance.ItemId);
            var maxLevel = MaxUpgradeFor(instance, config);

            if (instance.UpgradeLevel >= maxLevel)
            {
                Hud?.SetStatus($"{itemName} ya esta al maximo (+{instance.UpgradeLevel}).");
                return;
            }

            var step = config.GetStep(instance.UpgradeLevel + 1);

            if (Progression.Gold < step.GoldCost)
            {
                Hud?.SetStatus($"Oro insuficiente: necesitas {step.GoldCost}.");
                return;
            }

            if (Inventory.Count(materialId) < step.MaterialCost)
            {
                Hud?.SetStatus($"Necesitas {step.MaterialCost} {Inventory.DisplayNameOf(materialId)}.");
                return;
            }

            var useProtection = step.OnFailure != FailurePolicy.KeepLevel
                && Inventory.Count(DefaultGameItems.ProtectionRune) > 0;

            Progression.SpendGold(step.GoldCost);
            Inventory.TryConsume(materialId, step.MaterialCost);
            if (useProtection)
            {
                Inventory.TryConsume(DefaultGameItems.ProtectionRune);
                Hud?.AddFeed("Runa de proteccion aplicada");
            }

            var outcome = UpgradeResolver.Resolve(step, Random.value, useProtection);

            switch (outcome)
            {
                case UpgradeOutcome.Success:
                    instance.UpgradeLevel++;
                    Hud?.SetStatus($"{itemName} mejorado a +{instance.UpgradeLevel}.");
                    Hud?.AddFeed($"{itemName} +{instance.UpgradeLevel}");
                    DamagePopup.Spawn(transform.position + Vector3.up * 2.15f, $"+{instance.UpgradeLevel}", new Color(1f, 0.85f, 0.3f));
                    break;

                case UpgradeOutcome.FailKept:
                    Hud?.SetStatus(useProtection
                        ? $"La mejora fallo, pero la runa protegio {itemName}."
                        : $"La mejora de {itemName} fallo. El objeto se mantiene.");
                    break;

                case UpgradeOutcome.FailDowngraded:
                    instance.UpgradeLevel = Mathf.Max(0, instance.UpgradeLevel - 1);
                    Hud?.SetStatus($"La mejora fallo: {itemName} bajo a +{instance.UpgradeLevel}.");
                    Hud?.AddFeed($"{itemName} bajo a +{instance.UpgradeLevel}");
                    break;

                case UpgradeOutcome.Destroyed:
                    gear.DestroyEquipped(slot);
                    Hud?.SetStatus($"La mejora fallo: {itemName} se destruyo.", 4f);
                    Hud?.AddFeed($"{itemName} destruido en la mejora");
                    break;
            }

            ApplyBonuses();
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

        // Migracion de guardados anteriores al esquema 4, donde la mejora era
        // un contador global de arma/armadura.
        public void RestoreUpgrades(int weaponLevel, int armorLevel)
        {
            var gear = GetComponent<PlayerEquipment>();
            if (gear != null)
            {
                var weapon = gear.GetEquipped(EquipSlot.Weapon);
                if (weapon != null && weapon.UpgradeLevel == 0 && weaponLevel > 0)
                {
                    weapon.UpgradeLevel = weaponLevel;
                }

                var armorSlot = FindArmorSlot();
                var armor = armorSlot != EquipSlot.None ? gear.GetEquipped(armorSlot) : null;
                if (armor != null && armor.UpgradeLevel == 0 && armorLevel > 0)
                {
                    armor.UpgradeLevel = armorLevel;
                }
            }

            ApplyBonuses();
        }

        public string Summary()
        {
            var equipment = GetComponent<PlayerEquipment>();
            var pieces = equipment != null ? equipment.Summary() : "sin piezas";
            return $"Equipo: {pieces}";
        }

        // Punto unico de recomputo de stats derivados del equipamiento.
        public void ApplyBonuses()
        {
            var equipment = GetComponent<PlayerEquipment>();
            var equipDamage = equipment != null ? equipment.TotalDamageBonus : 0;
            var equipHealth = equipment != null ? equipment.TotalMaxHealthBonus : 0;
            var equipSpeed = equipment != null ? equipment.TotalMoveSpeedBonus : 0f;

            combat.EquipmentDamageBonus = equipDamage;

            var classController = GetComponent<PlayerClassController>();
            if (classController != null && classController.Definition != null)
            {
                var definition = classController.Definition;
                if (equipHealth > 0 || health.MaxHealth != definition.MaxHealth)
                {
                    health.ResetHealth(definition.MaxHealth + equipHealth);
                }

                var movement = GetComponent<PlayerController>();
                if (movement != null)
                {
                    var mount = GetComponent<MountService>();
                    var mountMultiplier = mount != null ? mount.SpeedMultiplier : 1f;
                    movement.MoveSpeed = (definition.MoveSpeed + equipSpeed) * mountMultiplier;
                }
            }
            else if (equipHealth > 0)
            {
                health.ResetHealth(health.MaxHealth + equipHealth);
            }

            Hud?.RefreshEquipment();
            Hud?.RefreshClass();
        }

        private UpgradeConfig EnsureConfig()
        {
            if (Config == null)
            {
                Config = ScriptableObject.CreateInstance<UpgradeConfig>();
                Config.FillWithDefaults();
            }

            return Config;
        }

        private int MaxUpgradeFor(ItemInstance instance, UpgradeConfig config)
        {
            var rarityMax = Inventory != null && Inventory.Database != null
                ? Inventory.Database.RarityRowOf(instance.ItemId).MaxUpgradeLevel
                : config.MaxUpgradeLevel;

            return Mathf.Min(config.MaxUpgradeLevel, Mathf.Max(1, rarityMax));
        }

        private int LevelOf(EquipSlot slot)
        {
            if (slot == EquipSlot.None)
            {
                return 0;
            }

            var gear = GetComponent<PlayerEquipment>();
            var instance = gear != null ? gear.GetEquipped(slot) : null;
            return instance != null ? instance.UpgradeLevel : 0;
        }

        private EquipSlot FindArmorSlot()
        {
            var gear = GetComponent<PlayerEquipment>();
            if (gear == null)
            {
                return EquipSlot.None;
            }

            if (gear.GetEquipped(EquipSlot.Chest) != null)
            {
                return EquipSlot.Chest;
            }

            if (gear.GetEquipped(EquipSlot.Helmet) != null)
            {
                return EquipSlot.Helmet;
            }

            foreach (var entry in gear.Equipped)
            {
                if (entry.Key != EquipSlot.Weapon && entry.Value != null)
                {
                    return entry.Key;
                }
            }

            return EquipSlot.None;
        }
    }
}
