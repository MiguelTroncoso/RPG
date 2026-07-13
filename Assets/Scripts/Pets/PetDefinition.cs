using UnityEngine;

namespace MmorpgPrototype
{
    public enum PetType
    {
        Support,
        Collector,
        Offensive,
        Defensive,
        Experience
    }

    [CreateAssetMenu(menuName = "MMORPG/Companions/Pet Definition", fileName = "PetDefinition")]
    public sealed class PetDefinition : ScriptableObject
    {
        public string PetId;
        public string DisplayName;
        public PetType Type = PetType.Experience;
        public Rarity Rarity = Rarity.Common;
        public bool StarterOwned = true;

        // Bonos pasivos mientras la mascota esta invocada.
        [Min(0f)] public float ExpBonusPercent;
        [Min(0f)] public float GoldBonusPercent;
        [Min(0)] public int DamageBonus;
        [Min(0)] public int MaxHealthBonus;
        [Range(0f, 1f)] public float CritChanceBonus;

        // Visual procedural temporal, como el avatar del jugador.
        public Color BodyColor = new Color(0.9f, 0.55f, 0.2f);
        [Min(0.1f)] public float Scale = 0.45f;
        [Min(0.5f)] public float FollowDistance = 1.7f;
    }
}
