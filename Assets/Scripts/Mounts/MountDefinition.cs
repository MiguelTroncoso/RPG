using UnityEngine;

namespace MmorpgPrototype
{
    // Flying existe desde ya en el enum, pero la locomocion voladora se
    // implementa mas adelante como estrategia aparte (IMountLocomotion).
    public enum MountCategory
    {
        Horse,
        Wolf,
        Tiger,
        MagicCreature,
        GroundDragon,
        Flying
    }

    [CreateAssetMenu(menuName = "MMORPG/Companions/Mount Definition", fileName = "MountDefinition")]
    public sealed class MountDefinition : ScriptableObject
    {
        public string MountId;
        public string DisplayName;
        public MountCategory Category = MountCategory.Horse;
        public Rarity Rarity = Rarity.Common;
        public bool StarterOwned = true;
        [Min(1)] public int RequiredLevel = 5;
        [Min(1f)] public float SpeedMultiplier = 1.6f;
        [Min(0)] public int DamageBonus;
        [Min(0)] public int MaxHealthBonus;
        [Range(0f, 1f)] public float CritChanceBonus;

        // Visual procedural temporal.
        public Color BodyColor = new Color(0.45f, 0.3f, 0.16f);
    }
}
