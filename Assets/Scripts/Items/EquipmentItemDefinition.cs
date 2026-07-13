using UnityEngine;

namespace MmorpgPrototype
{
    [CreateAssetMenu(menuName = "MMORPG/Items/Equipment Definition", fileName = "EquipmentDefinition")]
    public sealed class EquipmentItemDefinition : ItemDefinition
    {
        public EquipSlot Slot = EquipSlot.Weapon;
        [Min(1)] public int RequiredLevel = 1;

        // Vacio = todas las clases pueden equiparlo.
        public CharacterClassType[] AllowedClasses;

        [Min(0)] public int DamageBonus;
        [Min(0)] public int MaxHealthBonus;
        [Min(0f)] public float MoveSpeedBonus;

        // Identificador visual consumido por EquipmentVisualController. Vacio
        // permite que una pieza aporte estadisticas sin imponer una apariencia.
        public string VisualId;

        // Material recomendado para mejorar esta pieza. Los equipos antiguos
        // quedan compatibles porque el sistema usa sus materiales legacy si
        // este campo esta vacio.
        public string UpgradeMaterialId;
        public int ProgressionTier;
        public bool BossExclusive;

        private void OnEnable()
        {
            Category = ItemCategory.Equipment;
            MaxStack = 1;
        }

        public bool AllowsClass(CharacterClassType classType)
        {
            if (AllowedClasses == null || AllowedClasses.Length == 0)
            {
                return true;
            }

            foreach (var allowed in AllowedClasses)
            {
                if (allowed == classType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
