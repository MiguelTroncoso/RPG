using UnityEngine;

namespace MmorpgPrototype
{
    public enum CharacterArtSilhouette
    {
        Vanguard,
        Veil,
        Spirit,
        Void
    }

    // Shared visual direction for the eight playable combinations. The
    // values keep the runtime fallback and imported FBX characters on the
    // same silhouette, palette and Android-friendly visual budget.
    public sealed class CharacterArtProfile
    {
        public CharacterClassType ClassType;
        public CharacterGender Gender;
        public CharacterArtSilhouette Silhouette;
        public Color PrimaryColor;
        public Color SecondaryColor;
        public Color MetalColor;
        public Color GlowColor;
        public float ModelScale;
        public float ShoulderScale;
        public float CloakScale;
        public bool UseHood;
        public bool UseMask;
        public bool UseCloak;
        public bool UseCrown;
        public bool UseSpikes;
    }

    public static class CharacterArtProfiles
    {
        public static CharacterArtProfile Get(CharacterClassType classType, CharacterGender gender)
        {
            var definition = ClassDefinition.Create(classType);
            var female = gender == CharacterGender.Femenino;
            var profile = new CharacterArtProfile
            {
                ClassType = classType,
                Gender = gender,
                PrimaryColor = definition.BodyColor,
                SecondaryColor = Color.Lerp(definition.BodyColor, Color.black, female ? 0.08f : 0.2f),
                MetalColor = Color.Lerp(definition.BodyColor, Color.white, 0.34f),
                GlowColor = definition.SkillColor,
                ModelScale = female ? 0.96f : 1.02f,
                ShoulderScale = female ? 0.9f : 1f,
                CloakScale = female ? 0.92f : 1f
            };

            switch (classType)
            {
                case CharacterClassType.Ninja:
                    profile.Silhouette = CharacterArtSilhouette.Veil;
                    profile.UseHood = true;
                    profile.UseMask = true;
                    profile.UseCloak = true;
                    profile.SecondaryColor = new Color(0.055f, 0.065f, 0.1f);
                    profile.MetalColor = new Color(0.42f, 0.52f, 0.68f);
                    break;
                case CharacterClassType.Chaman:
                    profile.Silhouette = CharacterArtSilhouette.Spirit;
                    profile.UseHood = true;
                    profile.UseCrown = true;
                    profile.UseCloak = true;
                    profile.MetalColor = new Color(0.78f, 0.62f, 0.3f);
                    break;
                case CharacterClassType.Umbra:
                    profile.Silhouette = CharacterArtSilhouette.Void;
                    profile.UseCrown = true;
                    profile.UseSpikes = true;
                    profile.UseCloak = true;
                    profile.SecondaryColor = new Color(0.07f, 0.025f, 0.12f);
                    profile.MetalColor = new Color(0.34f, 0.2f, 0.5f);
                    break;
                default:
                    profile.Silhouette = CharacterArtSilhouette.Vanguard;
                    profile.UseCrown = true;
                    profile.UseCloak = true;
                    profile.MetalColor = new Color(0.62f, 0.68f, 0.76f);
                    break;
            }

            return profile;
        }
    }
}
