using System;
using System.Collections.Generic;

namespace MmorpgPrototype
{
    [Serializable]
    public sealed class PlayerSaveData
    {
        public int SchemaVersion = 8;
        public string CharacterName;
        public string ClassName;
        public string GenderName;
        public int Level = 1;
        public int Experience;
        public int Gold;
        public int AttributePoints;
        public int SpentStrength;
        public int SpentVitality;
        public int SpentAgility;
        public int WeaponLevel;
        public int ArmorLevel;
        public List<SavedItemEntry> Items = new List<SavedItemEntry>();
        public List<SavedEquipmentEntry> Equipment = new List<SavedEquipmentEntry>();
        public QuestSaveData Quests = new QuestSaveData();
        public string ActivePetId = string.Empty;
        public string SelectedMountId = string.Empty;
        public List<SavedItemEntry> Storage = new List<SavedItemEntry>();
    }

    [Serializable]
    public sealed class QuestSaveData
    {
        public string ActiveQuestId = string.Empty;
        public List<int> Counters = new List<int>();
        public List<string> CompletedQuestIds = new List<string>();
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
