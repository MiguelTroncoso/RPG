using System;
using System.Collections.Generic;

namespace MmorpgPrototype
{
    [Serializable]
    public sealed class PlayerSaveData
    {
        public int SchemaVersion = 2;
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
    }

    [Serializable]
    public struct SavedItemEntry
    {
        public string Name;
        public int Count;
    }
}
