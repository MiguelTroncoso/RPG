using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    public enum ArmorVisualStyle
    {
        Knight,
        Assassin,
        Spirit,
        Void
    }

    [CreateAssetMenu(menuName = "MMORPG/Equipment/Armor Visual Set", fileName = "ArmorVisualSet")]
    public sealed class ArmorVisualSetDefinition : ScriptableObject
    {
        public CharacterClassType ClassType;
        public CharacterGender Gender;
        public ArmorVisualStyle Style;
        public Color ArmorColor = Color.gray;
        public Color TrimColor = Color.white;
        public Color AccentColor = Color.white;
        public bool UseSkirt;
        public float ShoulderScale = 1f;
    }

    // Runtime fallback for prototypes without generated Resources assets. The
    // same definitions can later be generated as ScriptableObjects by editor
    // tooling without changing PlayerAvatarVisual.
    public static class DefaultArmorVisualSets
    {
        private static readonly Dictionary<string, ArmorVisualSetDefinition> Sets = BuildSets();

        public static ArmorVisualSetDefinition Get(CharacterClassType classType, CharacterGender gender)
        {
            var key = Key(classType, gender);
            return Sets.TryGetValue(key, out var definition) ? definition : Sets[Key(CharacterClassType.Guerrero, CharacterGender.Masculino)];
        }

        private static Dictionary<string, ArmorVisualSetDefinition> BuildSets()
        {
            var sets = new Dictionary<string, ArmorVisualSetDefinition>();
            Add(sets, CharacterClassType.Guerrero, CharacterGender.Masculino, ArmorVisualStyle.Knight, false, 1f);
            Add(sets, CharacterClassType.Guerrero, CharacterGender.Femenino, ArmorVisualStyle.Knight, true, 0.92f);
            Add(sets, CharacterClassType.Ninja, CharacterGender.Masculino, ArmorVisualStyle.Assassin, false, 0.84f);
            Add(sets, CharacterClassType.Ninja, CharacterGender.Femenino, ArmorVisualStyle.Assassin, true, 0.76f);
            Add(sets, CharacterClassType.Chaman, CharacterGender.Masculino, ArmorVisualStyle.Spirit, false, 0.9f);
            Add(sets, CharacterClassType.Chaman, CharacterGender.Femenino, ArmorVisualStyle.Spirit, true, 0.82f);
            Add(sets, CharacterClassType.Umbra, CharacterGender.Masculino, ArmorVisualStyle.Void, false, 1.08f);
            Add(sets, CharacterClassType.Umbra, CharacterGender.Femenino, ArmorVisualStyle.Void, true, 0.98f);
            return sets;
        }

        private static void Add(Dictionary<string, ArmorVisualSetDefinition> sets, CharacterClassType classType, CharacterGender gender, ArmorVisualStyle style, bool useSkirt, float shoulderScale)
        {
            var classDefinition = ClassDefinition.Create(classType);
            var set = ScriptableObject.CreateInstance<ArmorVisualSetDefinition>();
            set.name = $"{classType}_{gender}_ArmorVisualSet";
            set.ClassType = classType;
            set.Gender = gender;
            set.Style = style;
            set.UseSkirt = useSkirt;
            set.ShoulderScale = shoulderScale;
            set.ArmorColor = Color.Lerp(classDefinition.BodyColor, Color.black, gender == CharacterGender.Femenino ? 0.08f : 0.22f);
            set.TrimColor = classDefinition.SkillColor;
            set.AccentColor = Color.Lerp(classDefinition.SkillColor, Color.white, gender == CharacterGender.Femenino ? 0.18f : 0.04f);
            sets[Key(classType, gender)] = set;
        }

        private static string Key(CharacterClassType classType, CharacterGender gender)
        {
            return $"{classType}:{gender}";
        }
    }
}
