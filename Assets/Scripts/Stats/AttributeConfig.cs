using UnityEngine;

namespace MmorpgPrototype
{
    public enum AttributeType
    {
        Strength,
        Vitality,
        Agility
    }

    // Cuanto aporta cada punto de atributo. Balance como datos, no en logica.
    [CreateAssetMenu(menuName = "MMORPG/Progression/Attribute Config", fileName = "AttributeConfig")]
    public sealed class AttributeConfig : ScriptableObject
    {
        [Min(0)] public int DamagePerStrength = 1;
        [Min(0)] public int HealthPerVitality = 8;
        [Min(0f)] public float MoveSpeedPerAgility = 0.02f;
        [Range(0f, 0.05f)] public float CritChancePerAgility = 0.002f;
    }
}
