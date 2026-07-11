using System;
using System.Collections.Generic;

namespace MmorpgPrototype
{
    [Serializable]
    public sealed class PlayerSaveData
    {
        public int SchemaVersion = 4;
        public string CharacterName;
        public string ClassName;
        public string GenderName;
        public int Level = 1;
        public int Experience;
        public int Gold;
        public int AttributePoints;
        public int WeaponLevel;
        public int ArmorLevel;
        public List<SavedItemEntry> Items = new List<SavedItemEntry>();
        public List<SavedEquipmentEntry> Equipment = new List<SavedEquipmentEntry>();
    }

    [Serializable]
    public struct SavedItemEntry
    {
        // Contiene el ItemId; en guardados de esquema <= 2 era el nombre
        // visible (InventorySystem migra al cargar).
        public string Name;
        public int Count;
    }

    [Serializable]
    public struct SavedEquipmentEntry
    {
        public string Slot;
        public string ItemId;
        public int UpgradeLevel;
    }
}
