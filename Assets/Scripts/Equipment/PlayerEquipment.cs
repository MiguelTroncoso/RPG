using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    public enum EquipResult
    {
        Success,
        UnknownItem,
        NotEquippable,
        LevelTooLow,
        WrongClass
    }

    // Equipamiento por slots basado en instancias. Valida requisitos y
    // expone los bonos totales; EquipmentUpgradeSystem.ApplyBonuses es el
    // unico punto que los aplica al personaje.
    public sealed class PlayerEquipment : MonoBehaviour
    {
        public ItemDatabase Database;
        public InventorySystem Inventory;
        public PlayerProgression Progression;
        public PlayerClassController ClassController;
        public EquipmentUpgradeSystem UpgradeSystem;
        public UpgradeConfig Upgrades;
        public PrototypeHud Hud;

        private readonly Dictionary<EquipSlot, ItemInstance> equipped = new Dictionary<EquipSlot, ItemInstance>();

        public IReadOnlyDictionary<EquipSlot, ItemInstance> Equipped => equipped;

        public int TotalDamageBonus
        {
            get
            {
                var total = 0;
                foreach (var entry in equipped)
                {
                    var definition = DefinitionOf(entry.Value);
                    if (definition != null)
                    {
                        total += definition.DamageBonus + UpgradeDamageOf(definition, entry.Value);
                    }
                }

                return total;
            }
        }

        public int TotalMaxHealthBonus
        {
            get
            {
                var total = 0;
                foreach (var entry in equipped)
                {
                    var definition = DefinitionOf(entry.Value);
                    if (definition != null)
                    {
                        total += definition.MaxHealthBonus + UpgradeHealthOf(definition, entry.Value);
                    }
                }

                return total;
            }
        }

        private int UpgradeDamageOf(EquipmentItemDefinition definition, ItemInstance instance)
        {
            if (Upgrades == null || instance == null || definition.Slot != EquipSlot.Weapon)
            {
                return 0;
            }

            return instance.UpgradeLevel * Upgrades.WeaponDamagePerLevel;
        }

        private int UpgradeHealthOf(EquipmentItemDefinition definition, ItemInstance instance)
        {
            if (Upgrades == null || instance == null || definition.Slot == EquipSlot.Weapon)
            {
                return 0;
            }

            return instance.UpgradeLevel * Upgrades.ArmorHealthPerLevel;
        }

        public float TotalMoveSpeedBonus
        {
            get
            {
                var total = 0f;
                foreach (var entry in equipped)
                {
                    var definition = DefinitionOf(entry.Value);
                    if (definition != null)
                    {
                        total += definition.MoveSpeedBonus;
                    }
                }

                return total;
            }
        }

        public EquipResult TryEquip(ItemInstance instance)
        {
            var definition = DefinitionOf(instance);
            if (definition == null)
            {
                return instance == null || Database == null || Database.Get(instance.ItemId) == null
                    ? EquipResult.UnknownItem
                    : EquipResult.NotEquippable;
            }

            if (Progression != null && Progression.Level < definition.RequiredLevel)
            {
                return EquipResult.LevelTooLow;
            }

            if (ClassController != null && !definition.AllowsClass(ClassController.CurrentClass))
            {
                return EquipResult.WrongClass;
            }

            Inventory?.RemoveInstance(instance);

            if (equipped.TryGetValue(definition.Slot, out var previous) && previous != null)
            {
                Inventory?.AddInstance(previous);
            }

            equipped[definition.Slot] = instance;
            UpgradeSystem?.ApplyBonuses();
            Hud?.RefreshEquipment();
            return EquipResult.Success;
        }

        public ItemInstance GetEquipped(EquipSlot slot)
        {
            return equipped.TryGetValue(slot, out var instance) ? instance : null;
        }

        // Elimina la pieza sin devolverla al inventario (mejora destructiva).
        public bool DestroyEquipped(EquipSlot slot)
        {
            if (!equipped.Remove(slot))
            {
                return false;
            }

            UpgradeSystem?.ApplyBonuses();
            Hud?.RefreshEquipment();
            return true;
        }

        public bool Unequip(EquipSlot slot)
        {
            if (!equipped.TryGetValue(slot, out var instance) || instance == null)
            {
                return false;
            }

            equipped.Remove(slot);
            Inventory?.AddInstance(instance);
            UpgradeSystem?.ApplyBonuses();
            Hud?.RefreshEquipment();
            return true;
        }

        // Equipa lo mejor disponible del inventario slot por slot; reporta el
        // primer motivo de rechazo para que los requisitos sean visibles.
        public void EquipBestFromInventory()
        {
            if (Inventory == null || Database == null)
            {
                return;
            }

            var equippedSomething = false;
            string firstRejection = null;

            var candidates = new List<ItemInstance>(Inventory.Items);
            foreach (var candidate in candidates)
            {
                var definition = DefinitionOf(candidate);
                if (definition == null)
                {
                    continue;
                }

                if (!IsBetterThanEquipped(definition))
                {
                    continue;
                }

                var result = TryEquip(candidate);
                if (result == EquipResult.Success)
                {
                    equippedSomething = true;
                    Hud?.AddFeed(Localization.Tr("equip.feed", definition.DisplayName));
                }
                else if (firstRejection == null)
                {
                    firstRejection = RejectionMessage(result, definition);
                }
            }

            if (equippedSomething)
            {
                Hud?.SetStatus(Localization.Tr("equip.updated"));
            }
            else
            {
                Hud?.SetStatus(firstRejection ?? Localization.Tr("equip.nothing_better"));
            }
        }

        public string Summary()
        {
            if (equipped.Count == 0)
            {
                return Localization.Tr("hud.no_pieces");
            }

            var parts = new List<string>();
            foreach (var entry in equipped)
            {
                var definition = DefinitionOf(entry.Value);
                if (definition != null)
                {
                    var levelSuffix = entry.Value.UpgradeLevel > 0 ? $" +{entry.Value.UpgradeLevel}" : string.Empty;
                    parts.Add(definition.DisplayName + levelSuffix);
                }

                if (parts.Count >= 3)
                {
                    break;
                }
            }

            var extra = equipped.Count > parts.Count ? $" (+{equipped.Count - parts.Count})" : string.Empty;
            return string.Join(", ", parts) + extra;
        }

        public List<SavedEquipmentEntry> ExportEntries()
        {
            var entries = new List<SavedEquipmentEntry>(equipped.Count);
            foreach (var entry in equipped)
            {
                if (entry.Value != null)
                {
                    entries.Add(new SavedEquipmentEntry
                    {
                        Slot = entry.Key.ToString(),
                        ItemId = entry.Value.ItemId,
                        UpgradeLevel = entry.Value.UpgradeLevel
                    });
                }
            }

            return entries;
        }

        public void RestoreEntries(IEnumerable<SavedEquipmentEntry> entries)
        {
            equipped.Clear();

            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    if (!System.Enum.TryParse(entry.Slot, out EquipSlot slot) || slot == EquipSlot.None)
                    {
                        continue;
                    }

                    var definition = Database != null ? Database.Get(entry.ItemId) as EquipmentItemDefinition : null;
                    if (definition == null || definition.Slot != slot)
                    {
                        continue;
                    }

                    var instance = ItemInstance.Create(entry.ItemId);
                    instance.UpgradeLevel = Mathf.Max(0, entry.UpgradeLevel);
                    equipped[slot] = instance;
                }
            }

            UpgradeSystem?.ApplyBonuses();
            Hud?.RefreshEquipment();
        }

        private bool IsBetterThanEquipped(EquipmentItemDefinition candidate)
        {
            if (!equipped.TryGetValue(candidate.Slot, out var current) || current == null)
            {
                return true;
            }

            var currentDefinition = DefinitionOf(current);
            return currentDefinition == null || Score(candidate) > Score(currentDefinition);
        }

        private static float Score(EquipmentItemDefinition definition)
        {
            return definition.DamageBonus * 3f + definition.MaxHealthBonus + definition.MoveSpeedBonus * 10f;
        }

        private string RejectionMessage(EquipResult result, EquipmentItemDefinition definition)
        {
            switch (result)
            {
                case EquipResult.LevelTooLow:
                    return Localization.Tr("equip.need_level", definition.RequiredLevel, definition.DisplayName);
                case EquipResult.WrongClass:
                    return Localization.Tr("equip.wrong_class", definition.DisplayName);
                default:
                    return Localization.Tr("equip.cannot", definition.DisplayName);
            }
        }

        private EquipmentItemDefinition DefinitionOf(ItemInstance instance)
        {
            if (instance == null || Database == null)
            {
                return null;
            }

            return Database.Get(instance.ItemId) as EquipmentItemDefinition;
        }
    }
}
