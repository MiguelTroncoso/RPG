using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Original modular low-poly art. It is deliberately generated at runtime
    // so the first art pass stays editable, deterministic and light on Android.
    public static class OriginalArtVisualFactory
    {
        private enum TexturePattern
        {
            Solid,
            Plate,
            Fabric,
            Leather,
            Scale,
            Rune,
            Stone,
            Bone
        }

        private static readonly Dictionary<int, Material> Materials = new Dictionary<int, Material>();
        private static readonly Dictionary<int, TextureAtlasEntry> TextureEntries = new Dictionary<int, TextureAtlasEntry>();
        private const int AtlasSize = 512;
        private const int AtlasTileSize = 32;
        private const int AtlasTilesPerSide = AtlasSize / AtlasTileSize;
        private static Texture2D albedoAtlas;
        private static Texture2D normalAtlas;
        private static int nextAtlasSlot;

        private struct TextureAtlasEntry
        {
            public Vector2 Scale;
            public Vector2 Offset;
        }

        public static GameObject BuildCharacter(Transform parent, CharacterArtProfile profile)
        {
            if (parent == null || profile == null)
            {
                return null;
            }

            var root = new GameObject($"Original {profile.ClassType} {profile.Gender}");
            root.transform.SetParent(parent, false);

            switch (profile.Silhouette)
            {
                case CharacterArtSilhouette.Veil:
                    BuildVeil(root.transform, profile);
                    break;
                case CharacterArtSilhouette.Spirit:
                    BuildSpirit(root.transform, profile);
                    break;
                case CharacterArtSilhouette.Void:
                    BuildVoid(root.transform, profile);
                    break;
                default:
                    BuildVanguard(root.transform, profile);
                    break;
            }

            BuildRigMarkers(root.transform, profile);
            BuildSkinnedRepresentation(root.transform);
            return root;
        }

        public static GameObject BuildWarriorMale(Transform parent, CharacterArtProfile profile)
        {
            if (profile == null || profile.ClassType != CharacterClassType.Guerrero || profile.Gender != CharacterGender.Masculino)
            {
                return null;
            }

            return BuildCharacter(parent, profile);
        }

        public static GameObject BuildValleyMob(Transform parent, Color baseColor, Color accent)
        {
            if (parent == null)
            {
                return null;
            }

            var root = new GameObject("Original Relic Hound");
            root.transform.SetParent(parent, false);

            var hide = Color.Lerp(baseColor, new Color(0.08f, 0.035f, 0.025f), 0.34f);
            var bone = new Color(0.62f, 0.42f, 0.2f);
            var corruption = new Color(1f, 0.24f, 0.08f);

            CreatePart(root.transform, "Hound Body", LowPolySphere(0.52f, 10, 5), new Vector3(0f, 0.06f, 0f), new Vector3(1.1f, 0.72f, 1.55f), Quaternion.Euler(90f, 0f, 0f), hide, false, TexturePattern.Scale);
            CreatePart(root.transform, "Hound Head", LowPolySphere(0.38f, 10, 5), new Vector3(0f, 0.22f, 0.64f), new Vector3(1.05f, 0.92f, 1.08f), Quaternion.identity, hide, false, TexturePattern.Scale);
            CreatePart(root.transform, "Hound Muzzle", Frustum(0.22f, 0.12f, 0.32f, 6), new Vector3(0f, 0.12f, 0.94f), new Vector3(1f, 0.8f, 1f), Quaternion.Euler(90f, 0f, 0f), bone, false, TexturePattern.Bone);
            CreatePart(root.transform, "Hound Ear Left", Frustum(0.14f, 0.035f, 0.42f, 5), new Vector3(-0.25f, 0.58f, 0.56f), Vector3.one, Quaternion.Euler(0f, 0f, -22f), bone, false, TexturePattern.Bone);
            CreatePart(root.transform, "Hound Ear Right", Frustum(0.14f, 0.035f, 0.42f, 5), new Vector3(0.25f, 0.58f, 0.56f), Vector3.one, Quaternion.Euler(0f, 0f, 22f), bone, false, TexturePattern.Bone);
            CreatePart(root.transform, "Hound Leg Front Left", Frustum(0.16f, 0.11f, 0.62f, 6), new Vector3(-0.3f, -0.42f, 0.38f), Vector3.one, Quaternion.identity, hide, false, TexturePattern.Scale);
            CreatePart(root.transform, "Hound Leg Front Right", Frustum(0.16f, 0.11f, 0.62f, 6), new Vector3(0.3f, -0.42f, 0.38f), Vector3.one, Quaternion.identity, hide, false, TexturePattern.Scale);
            CreatePart(root.transform, "Hound Leg Back Left", Frustum(0.16f, 0.11f, 0.62f, 6), new Vector3(-0.3f, -0.42f, -0.38f), Vector3.one, Quaternion.identity, hide, false, TexturePattern.Scale);
            CreatePart(root.transform, "Hound Leg Back Right", Frustum(0.16f, 0.11f, 0.62f, 6), new Vector3(0.3f, -0.42f, -0.38f), Vector3.one, Quaternion.identity, hide, false, TexturePattern.Scale);
            CreatePart(root.transform, "Hound Tail", Frustum(0.13f, 0.035f, 0.78f, 6), new Vector3(0f, 0.22f, -0.76f), Vector3.one, Quaternion.Euler(-48f, 0f, 0f), bone, false, TexturePattern.Bone);
            CreatePart(root.transform, "Relic Scar", Frustum(0.12f, 0.035f, 0.48f, 5), new Vector3(0f, 0.38f, 0.86f), Vector3.one, Quaternion.Euler(90f, 0f, 0f), corruption, true, TexturePattern.Rune);
            CreatePart(root.transform, "Relic Spine", Frustum(0.11f, 0.025f, 0.48f, 5), new Vector3(0f, 0.48f, 0.02f), Vector3.one, Quaternion.Euler(0f, 0f, 90f), accent, true, TexturePattern.Rune);
            CreatePart(root.transform, "Eye Left", LowPolySphere(0.055f, 6, 3), new Vector3(-0.13f, 0.28f, 0.91f), Vector3.one, Quaternion.identity, corruption, true, TexturePattern.Rune);
            CreatePart(root.transform, "Eye Right", LowPolySphere(0.055f, 6, 3), new Vector3(0.13f, 0.28f, 0.91f), Vector3.one, Quaternion.identity, corruption, true, TexturePattern.Rune);
            BuildCullLod(root.transform);
            return root;
        }

        public static void SetStarterWeaponVisible(Transform root, bool visible)
        {
            var weapon = FindDeepChild(root, "Starter Weapon");
            if (weapon != null)
            {
                weapon.gameObject.SetActive(visible);
            }
        }

        private static void BuildVanguard(Transform root, CharacterArtProfile profile)
        {
            var female = profile.Gender == CharacterGender.Femenino;
            var armor = profile.MetalColor;
            var fabric = profile.SecondaryColor;
            var skin = female ? new Color(0.9f, 0.66f, 0.5f) : new Color(0.72f, 0.46f, 0.31f);
            BuildHumanoidBase(root, profile, armor, fabric, skin);

            CreatePart(root, "Vanguard Chest Plate", Frustum(0.38f, 0.32f, 0.42f, 8), new Vector3(0f, 0.12f, -0.32f), new Vector3(1f, 0.7f, 0.24f), Quaternion.Euler(90f, 0f, 0f), armor, false, TexturePattern.Plate);
            CreatePart(root, "Vanguard Chest Rune", Frustum(0.11f, 0.035f, 0.28f, 4), new Vector3(0f, 0.18f, -0.49f), Vector3.one, Quaternion.Euler(90f, 0f, 45f), profile.GlowColor, true, TexturePattern.Rune);
            CreatePart(root, "Vanguard Shoulder Left", LowPolySphere(0.27f, 8, 4), new Vector3(-0.5f, 0.42f, 0f), new Vector3(1.25f, 0.72f, 1.1f), Quaternion.identity, armor, false, TexturePattern.Plate);
            CreatePart(root, "Vanguard Shoulder Right", LowPolySphere(0.27f, 8, 4), new Vector3(0.5f, 0.42f, 0f), new Vector3(1.25f, 0.72f, 1.1f), Quaternion.identity, armor, false, TexturePattern.Plate);
            CreatePart(root, "Vanguard Belt", Frustum(0.46f, 0.42f, 0.12f, 8), new Vector3(0f, -0.28f, 0f), Vector3.one, Quaternion.identity, profile.GlowColor, true, TexturePattern.Rune);

            if (female)
            {
                CreatePart(root, "Vanguard Hair", LowPolySphere(0.35f, 8, 4), new Vector3(0f, 0.96f, 0.04f), new Vector3(1.04f, 0.9f, 1.04f), Quaternion.identity, new Color(0.12f, 0.055f, 0.035f), false, TexturePattern.Leather);
                CreatePart(root, "Vanguard Tiara", Frustum(0.34f, 0.21f, 0.08f, 6), new Vector3(0f, 1.18f, -0.04f), Vector3.one, Quaternion.identity, profile.GlowColor, true, TexturePattern.Rune);
            }
            else
            {
                CreatePart(root, "Vanguard Helmet", Frustum(0.38f, 0.27f, 0.24f, 8), new Vector3(0f, 1.12f, 0f), Vector3.one, Quaternion.identity, armor, false, TexturePattern.Plate);
                CreatePart(root, "Vanguard Crest", Frustum(0.12f, 0.035f, 0.36f, 4), new Vector3(0f, 1.38f, -0.02f), Vector3.one, Quaternion.Euler(0f, 0f, 45f), profile.GlowColor, true, TexturePattern.Rune);
            }

            CreatePart(root, "Vanguard Cape", CapeMesh(), new Vector3(0f, 0.02f, 0.34f), Vector3.one * profile.CloakScale, Quaternion.Euler(8f, 0f, 0f), fabric, false, TexturePattern.Fabric);
            BuildSword(root, armor, profile.GlowColor);
        }

        private static void BuildVeil(Transform root, CharacterArtProfile profile)
        {
            var female = profile.Gender == CharacterGender.Femenino;
            var fabric = profile.SecondaryColor;
            var leather = Color.Lerp(fabric, Color.black, 0.24f);
            var skin = female ? new Color(0.9f, 0.66f, 0.5f) : new Color(0.68f, 0.42f, 0.28f);
            BuildHumanoidBase(root, profile, leather, fabric, skin);

            CreatePart(root, "Veil Hood", LowPolySphere(0.37f, 8, 4), new Vector3(0f, 0.96f, 0.02f), new Vector3(1.12f, 0.96f, 1.1f), Quaternion.identity, fabric, false, TexturePattern.Fabric);
            CreatePart(root, "Veil Mask", Frustum(0.24f, 0.12f, 0.11f, 6), new Vector3(0f, 0.87f, -0.36f), new Vector3(1.2f, 0.8f, 0.5f), Quaternion.Euler(90f, 0f, 0f), profile.GlowColor, true, TexturePattern.Rune);
            CreatePart(root, "Veil Shoulder Left", Frustum(0.2f, 0.08f, 0.34f, 5), new Vector3(-0.46f, 0.43f, 0f), Vector3.one, Quaternion.Euler(0f, 0f, 20f), profile.MetalColor, false, TexturePattern.Plate);
            CreatePart(root, "Veil Shoulder Right", Frustum(0.2f, 0.08f, 0.34f, 5), new Vector3(0.46f, 0.43f, 0f), Vector3.one, Quaternion.Euler(0f, 0f, -20f), profile.MetalColor, false, TexturePattern.Plate);
            CreatePart(root, "Veil Sash", Frustum(0.45f, 0.4f, 0.1f, 8), new Vector3(0f, -0.02f, -0.42f), Vector3.one, Quaternion.identity, profile.MetalColor, false, TexturePattern.Leather);
            CreatePart(root, "Veil Cloak", CapeMesh(), new Vector3(0f, 0.04f, 0.34f), Vector3.one * profile.CloakScale, Quaternion.Euler(10f, 0f, 0f), fabric, false, TexturePattern.Fabric);

            if (female)
            {
                CreatePart(root, "Veil Hair Ribbon", Frustum(0.05f, 0.025f, 0.52f, 5), new Vector3(0.28f, 0.62f, 0.04f), Vector3.one, Quaternion.Euler(0f, 0f, -16f), profile.GlowColor, true, TexturePattern.Rune);
            }

            var weaponRoot = CreateEmpty(root, "Starter Weapon");
            CreatePart(weaponRoot.transform, "Veil Dagger Left", Frustum(0.07f, 0.018f, 0.58f, 4), new Vector3(-0.48f, 0.02f, -0.28f), Vector3.one, Quaternion.Euler(0f, 0f, 26f), profile.MetalColor, false, TexturePattern.Plate);
            CreatePart(weaponRoot.transform, "Veil Dagger Right", Frustum(0.07f, 0.018f, 0.58f, 4), new Vector3(0.48f, 0.02f, -0.28f), Vector3.one, Quaternion.Euler(0f, 0f, -26f), profile.MetalColor, false, TexturePattern.Plate);
        }

        private static void BuildSpirit(Transform root, CharacterArtProfile profile)
        {
            var female = profile.Gender == CharacterGender.Femenino;
            var robe = Color.Lerp(profile.PrimaryColor, Color.white, 0.08f);
            var fabric = profile.SecondaryColor;
            var skin = female ? new Color(0.9f, 0.66f, 0.5f) : new Color(0.7f, 0.44f, 0.3f);
            BuildHumanoidBase(root, profile, robe, fabric, skin);

            CreatePart(root, "Spirit Robe", Frustum(0.52f, 0.34f, 0.46f, 8), new Vector3(0f, -0.48f, 0f), new Vector3(1.08f, 1f, 0.9f), Quaternion.identity, fabric, false, TexturePattern.Fabric);
            CreatePart(root, "Spirit Hood", LowPolySphere(0.38f, 8, 4), new Vector3(0f, 0.96f, 0.02f), new Vector3(1.14f, 0.96f, 1.08f), Quaternion.identity, fabric, false, TexturePattern.Fabric);
            CreatePart(root, "Spirit Face Rune", Frustum(0.2f, 0.03f, 0.12f, 5), new Vector3(0f, 0.86f, -0.37f), new Vector3(1.25f, 0.7f, 0.45f), Quaternion.Euler(90f, 0f, 0f), profile.GlowColor, true, TexturePattern.Rune);
            CreatePart(root, "Spirit Collar", Frustum(0.25f, 0.18f, 0.1f, 8), new Vector3(0f, 0.58f, -0.08f), Vector3.one, Quaternion.identity, profile.MetalColor, false, TexturePattern.Plate);
            CreatePart(root, "Spirit Orb", LowPolySphere(0.14f, 8, 4), new Vector3(0f, 0.34f, -0.5f), Vector3.one, Quaternion.identity, profile.GlowColor, true, TexturePattern.Rune);

            if (female)
            {
                CreatePart(root, "Spirit Headdress", Frustum(0.27f, 0.08f, 0.3f, 6), new Vector3(0f, 1.24f, 0f), Vector3.one, Quaternion.Euler(0f, 0f, 14f), profile.MetalColor, false, TexturePattern.Plate);
            }
            else
            {
                CreatePart(root, "Spirit Crown", Frustum(0.16f, 0.035f, 0.34f, 5), new Vector3(0f, 1.27f, 0f), Vector3.one, Quaternion.Euler(0f, 0f, 18f), profile.GlowColor, true, TexturePattern.Rune);
            }

            var weaponRoot = CreateEmpty(root, "Starter Weapon");
            CreatePart(weaponRoot.transform, "Spirit Staff", Frustum(0.07f, 0.05f, 1.12f, 6), new Vector3(0.56f, 0.26f, 0.04f), Vector3.one, Quaternion.Euler(0f, 0f, -10f), profile.MetalColor, false, TexturePattern.Leather);
            CreatePart(weaponRoot.transform, "Spirit Staff Crystal", LowPolySphere(0.16f, 8, 4), new Vector3(0.66f, 0.8f, 0.04f), Vector3.one, Quaternion.identity, profile.GlowColor, true, TexturePattern.Rune);
        }

        private static void BuildVoid(Transform root, CharacterArtProfile profile)
        {
            var female = profile.Gender == CharacterGender.Femenino;
            var armor = profile.MetalColor;
            var shadow = profile.SecondaryColor;
            var skin = female ? new Color(0.9f, 0.66f, 0.5f) : new Color(0.66f, 0.4f, 0.28f);
            BuildHumanoidBase(root, profile, armor, shadow, skin);

            CreatePart(root, "Void Chest Plate", Frustum(0.44f, 0.28f, 0.5f, 8), new Vector3(0f, 0.08f, -0.28f), new Vector3(1f, 0.8f, 0.28f), Quaternion.Euler(90f, 0f, 0f), armor, false, TexturePattern.Plate);
            CreatePart(root, "Void Core", LowPolySphere(0.14f, 8, 4), new Vector3(0f, 0.26f, -0.5f), Vector3.one, Quaternion.identity, profile.GlowColor, true, TexturePattern.Rune);
            CreatePart(root, "Void Shoulder Left", Frustum(0.24f, 0.06f, 0.48f, 5), new Vector3(-0.5f, 0.48f, 0f), Vector3.one, Quaternion.Euler(0f, 0f, 22f), armor, false, TexturePattern.Plate);
            CreatePart(root, "Void Shoulder Right", Frustum(0.24f, 0.06f, 0.48f, 5), new Vector3(0.5f, 0.48f, 0f), Vector3.one, Quaternion.Euler(0f, 0f, -22f), armor, false, TexturePattern.Plate);
            CreatePart(root, "Void Cloak", CapeMesh(), new Vector3(0f, 0.04f, 0.38f), Vector3.one * profile.CloakScale, Quaternion.Euler(12f, 0f, 0f), shadow, false, TexturePattern.Fabric);

            if (female)
            {
                CreatePart(root, "Void Veil", LowPolySphere(0.36f, 8, 4), new Vector3(0f, 0.98f, 0.02f), new Vector3(1.08f, 0.9f, 1.06f), Quaternion.identity, shadow, false, TexturePattern.Fabric);
                CreatePart(root, "Void Tiara", Frustum(0.3f, 0.04f, 0.28f, 5), new Vector3(0f, 1.25f, 0f), Vector3.one, Quaternion.Euler(0f, 0f, 16f), profile.GlowColor, true, TexturePattern.Rune);
            }
            else
            {
                CreatePart(root, "Void Horn Left", Frustum(0.12f, 0.025f, 0.4f, 5), new Vector3(-0.18f, 1.25f, 0f), Vector3.one, Quaternion.Euler(0f, 0f, -28f), profile.GlowColor, true, TexturePattern.Rune);
                CreatePart(root, "Void Horn Right", Frustum(0.12f, 0.025f, 0.4f, 5), new Vector3(0.18f, 1.25f, 0f), Vector3.one, Quaternion.Euler(0f, 0f, 28f), profile.GlowColor, true, TexturePattern.Rune);
            }

            var weaponRoot = CreateEmpty(root, "Starter Weapon");
            CreatePart(weaponRoot.transform, "Void Blade", Frustum(0.12f, 0.03f, 1.0f, 4), new Vector3(0.52f, 0.14f, 0.06f), Vector3.one, Quaternion.Euler(0f, 0f, -24f), profile.MetalColor, false, TexturePattern.Plate);
            CreatePart(weaponRoot.transform, "Void Blade Rune", Frustum(0.06f, 0.02f, 0.28f, 4), new Vector3(0.3f, 0.42f, 0.06f), Vector3.one, Quaternion.Euler(0f, 0f, -24f), profile.GlowColor, true, TexturePattern.Rune);
        }

        private static void BuildHumanoidBase(Transform root, CharacterArtProfile profile, Color torsoColor, Color fabricColor, Color skinColor)
        {
            var female = profile.Gender == CharacterGender.Femenino;
            var shoulderWidth = female ? 0.44f : 0.5f;
            var hipWidth = female ? 0.24f : 0.2f;
            CreatePart(root, "Torso", Frustum(0.4f, female ? 0.34f : 0.38f, 0.78f, 8), new Vector3(0f, 0.04f, 0f), Vector3.one, Quaternion.identity, torsoColor, false, TexturePattern.Plate);
            CreatePart(root, "Neck", Frustum(0.13f, 0.11f, 0.14f, 6), new Vector3(0f, 0.48f, 0f), Vector3.one, Quaternion.identity, skinColor, false, TexturePattern.Solid);
            CreatePart(root, "Head", LowPolySphere(0.34f, 8, 5), new Vector3(0f, 0.84f, 0f), Vector3.one * (female ? 0.98f : 1f), Quaternion.identity, skinColor, false, TexturePattern.Solid);
            CreateLimb(root, "Left Arm", new Vector3(-shoulderWidth, 0.06f, 0f), new Vector3(0.17f, 0.4f, 0.17f), Quaternion.Euler(0f, 0f, 16f), fabricColor, TexturePattern.Leather);
            CreateLimb(root, "Right Arm", new Vector3(shoulderWidth, 0.06f, 0f), new Vector3(0.17f, 0.4f, 0.17f), Quaternion.Euler(0f, 0f, -16f), fabricColor, TexturePattern.Leather);
            CreateLimb(root, "Left Leg", new Vector3(-hipWidth, -0.58f, 0f), new Vector3(0.2f, 0.56f, 0.2f), Quaternion.identity, fabricColor, TexturePattern.Leather);
            CreateLimb(root, "Right Leg", new Vector3(hipWidth, -0.58f, 0f), new Vector3(0.2f, 0.56f, 0.2f), Quaternion.identity, fabricColor, TexturePattern.Leather);
            CreatePart(root, "Left Boot", Frustum(0.23f, 0.18f, 0.22f, 6), new Vector3(-hipWidth, -0.91f, -0.08f), Vector3.one, Quaternion.identity, torsoColor, false, TexturePattern.Plate);
            CreatePart(root, "Right Boot", Frustum(0.23f, 0.18f, 0.22f, 6), new Vector3(hipWidth, -0.91f, -0.08f), Vector3.one, Quaternion.identity, torsoColor, false, TexturePattern.Plate);
            CreatePart(root, "Hip Sash", Frustum(0.46f, 0.4f, 0.1f, 8), new Vector3(0f, -0.3f, 0f), Vector3.one, Quaternion.identity, profile.GlowColor, true, TexturePattern.Rune);
        }

        private static void BuildSword(Transform root, Color metal, Color glow)
        {
            var weaponRoot = CreateEmpty(root, "Starter Weapon");
            CreatePart(weaponRoot.transform, "Sword Blade", Frustum(0.11f, 0.035f, 0.92f, 4), new Vector3(0.52f, 0.12f, -0.18f), Vector3.one, Quaternion.Euler(0f, 0f, -26f), metal, false, TexturePattern.Plate);
            CreatePart(weaponRoot.transform, "Sword Guard", Frustum(0.12f, 0.12f, 0.08f, 6), new Vector3(0.4f, -0.28f, -0.18f), new Vector3(1.5f, 1f, 0.7f), Quaternion.Euler(0f, 0f, -26f), glow, true, TexturePattern.Rune);
            CreatePart(weaponRoot.transform, "Sword Grip", Frustum(0.055f, 0.055f, 0.28f, 6), new Vector3(0.3f, -0.39f, -0.18f), Vector3.one, Quaternion.Euler(0f, 0f, -26f), new Color(0.12f, 0.07f, 0.045f), false, TexturePattern.Leather);
        }

        private static void BuildRigMarkers(Transform root, CharacterArtProfile profile)
        {
            var rig = CreateEmpty(root, "Rig Root").transform;
            var hips = CreateEmpty(rig, "Hips").transform;
            var spine = CreateEmpty(hips, "Spine").transform;
            var chest = CreateEmpty(spine, "Chest").transform;
            var neck = CreateEmpty(chest, "Neck").transform;
            CreateEmpty(neck, "Head");

            var leftShoulder = CreateEmpty(chest, "Shoulder.L").transform;
            var leftUpperArm = CreateEmpty(leftShoulder, "UpperArm.L").transform;
            var leftLowerArm = CreateEmpty(leftUpperArm, "LowerArm.L").transform;
            CreateEmpty(leftLowerArm, "Hand.L");

            var rightShoulder = CreateEmpty(chest, "Shoulder.R").transform;
            var rightUpperArm = CreateEmpty(rightShoulder, "UpperArm.R").transform;
            var rightLowerArm = CreateEmpty(rightUpperArm, "LowerArm.R").transform;
            CreateEmpty(rightLowerArm, "Hand.R");

            var leftHip = CreateEmpty(hips, "Thigh.L").transform;
            CreateEmpty(leftHip, "Foot.L");
            var rightHip = CreateEmpty(hips, "Thigh.R").transform;
            CreateEmpty(rightHip, "Foot.R");

            var grip = CreateEmpty(rightLowerArm, "WeaponGrip").transform;
            grip.localPosition = new Vector3(0f, -0.22f, -0.08f);
            var spell = CreateEmpty(leftLowerArm, "SpellHand").transform;
            spell.localPosition = new Vector3(0f, -0.2f, -0.08f);

            var descriptor = root.gameObject.AddComponent<OriginalRigDescriptor>();
            descriptor.ClassType = profile.ClassType;
            descriptor.Gender = profile.Gender;
        }

        private static void BuildSkinnedRepresentation(Transform root)
        {
            var filters = root.GetComponentsInChildren<MeshFilter>(true);
            var parts = new List<MeshFilter>();
            foreach (var filter in filters)
            {
                if (filter.sharedMesh == null || filter.GetComponent<MeshRenderer>() == null || IsUnderNamedRoot(filter.transform, "Starter Weapon"))
                {
                    continue;
                }

                parts.Add(filter);
            }

            if (parts.Count == 0)
            {
                return;
            }

            var combines = new CombineInstance[parts.Count];
            var bones = new Transform[parts.Count];
            var bindposes = new Matrix4x4[parts.Count];
            var weights = new List<BoneWeight>();
            var materials = new List<Material>();
            for (var index = 0; index < parts.Count; index++)
            {
                var filter = parts[index];
                var renderer = filter.GetComponent<MeshRenderer>();
                combines[index] = new CombineInstance
                {
                    mesh = filter.sharedMesh,
                    transform = root.worldToLocalMatrix * filter.transform.localToWorldMatrix
                };
                bones[index] = filter.transform;
                bindposes[index] = filter.transform.worldToLocalMatrix * root.localToWorldMatrix;
                materials.Add(renderer.sharedMaterial);
                renderer.enabled = false;

                for (var vertex = 0; vertex < filter.sharedMesh.vertexCount; vertex++)
                {
                    weights.Add(new BoneWeight
                    {
                        boneIndex0 = index,
                        weight0 = 1f
                    });
                }
            }

            var skinnedMesh = new Mesh
            {
                name = "Original Character Skinned Mesh"
            };
            skinnedMesh.CombineMeshes(combines, false, true, false);
            skinnedMesh.bindposes = bindposes;
            skinnedMesh.boneWeights = weights.ToArray();
            skinnedMesh.RecalculateBounds();

            var skinnedObject = new GameObject("Skinned Avatar Body");
            skinnedObject.transform.SetParent(root, false);
            var skinnedRenderer = skinnedObject.AddComponent<SkinnedMeshRenderer>();
            skinnedRenderer.sharedMesh = skinnedMesh;
            skinnedRenderer.sharedMaterials = materials.ToArray();
            skinnedRenderer.bones = bones;
            skinnedRenderer.rootBone = root;
            skinnedRenderer.quality = SkinQuality.Bone1;
            skinnedRenderer.updateWhenOffscreen = false;

            var renderers = new List<Renderer> { skinnedRenderer };
            var starterWeapon = FindDeepChild(root, "Starter Weapon");
            if (starterWeapon != null)
            {
                renderers.AddRange(starterWeapon.GetComponentsInChildren<Renderer>(true));
            }

            var lodGroup = root.gameObject.AddComponent<LODGroup>();
            lodGroup.SetLODs(new[]
            {
                new LOD(0.42f, renderers.ToArray()),
                new LOD(0.12f, new Renderer[0])
            });
            lodGroup.RecalculateBounds();
        }

        private static void BuildCullLod(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            var lodGroup = root.gameObject.AddComponent<LODGroup>();
            lodGroup.SetLODs(new[]
            {
                new LOD(0.32f, renderers),
                new LOD(0.08f, new Renderer[0])
            });
            lodGroup.RecalculateBounds();
        }

        private static bool IsUnderNamedRoot(Transform child, string rootName)
        {
            var current = child;
            while (current != null)
            {
                if (current.name == rootName)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static GameObject CreateLimb(Transform parent, string name, Vector3 position, Vector3 scale, Quaternion rotation, Color color, TexturePattern pattern)
        {
            return CreatePart(parent, name, Frustum(0.16f, 0.12f, 0.9f, 6), position, scale, rotation, color, false, pattern);
        }

        private static GameObject CreateEmpty(Transform parent, string name)
        {
            var empty = new GameObject(name);
            empty.transform.SetParent(parent, false);
            return empty;
        }

        private static GameObject CreatePart(Transform parent, string name, Mesh mesh, Vector3 position, Vector3 scale, Quaternion rotation, Color color, bool glow, TexturePattern pattern)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.transform.localScale = scale;
            var filter = part.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = MaterialFor(color, glow, pattern);
            return part;
        }

        private static Mesh LowPolySphere(float radius, int segments, int rings)
        {
            var vertices = new Vector3[(rings + 1) * segments];
            var triangles = new List<int>(rings * segments * 6);
            for (var ring = 0; ring <= rings; ring++)
            {
                var phi = Mathf.PI * ring / rings;
                var y = Mathf.Cos(phi) * radius;
                var ringRadius = Mathf.Sin(phi) * radius;
                for (var segment = 0; segment < segments; segment++)
                {
                    var theta = Mathf.PI * 2f * segment / segments;
                    vertices[ring * segments + segment] = new Vector3(Mathf.Cos(theta) * ringRadius, y, Mathf.Sin(theta) * ringRadius);
                }
            }

            for (var ring = 0; ring < rings; ring++)
            {
                for (var segment = 0; segment < segments; segment++)
                {
                    var next = (segment + 1) % segments;
                    var current = ring * segments + segment;
                    var currentNext = ring * segments + next;
                    var upper = (ring + 1) * segments + segment;
                    var upperNext = (ring + 1) * segments + next;
                    triangles.Add(current);
                    triangles.Add(upper);
                    triangles.Add(currentNext);
                    triangles.Add(currentNext);
                    triangles.Add(upper);
                    triangles.Add(upperNext);
                }
            }

            return CreateMesh(vertices, triangles.ToArray());
        }

        private static Mesh Frustum(float bottomRadius, float topRadius, float height, int segments)
        {
            var vertices = new Vector3[segments * 2 + 2];
            var triangles = new List<int>(segments * 12);
            for (var segment = 0; segment < segments; segment++)
            {
                var angle = Mathf.PI * 2f * segment / segments;
                var x = Mathf.Cos(angle);
                var z = Mathf.Sin(angle);
                vertices[segment] = new Vector3(x * bottomRadius, -height * 0.5f, z * bottomRadius);
                vertices[segments + segment] = new Vector3(x * topRadius, height * 0.5f, z * topRadius);
            }

            var bottomCenter = segments * 2;
            var topCenter = bottomCenter + 1;
            vertices[bottomCenter] = new Vector3(0f, -height * 0.5f, 0f);
            vertices[topCenter] = new Vector3(0f, height * 0.5f, 0f);
            for (var segment = 0; segment < segments; segment++)
            {
                var next = (segment + 1) % segments;
                triangles.Add(segment);
                triangles.Add(segments + segment);
                triangles.Add(next);
                triangles.Add(next);
                triangles.Add(segments + segment);
                triangles.Add(segments + next);
                triangles.Add(bottomCenter);
                triangles.Add(next);
                triangles.Add(segment);
                triangles.Add(topCenter);
                triangles.Add(segments + segment);
                triangles.Add(segments + next);
            }

            return CreateMesh(vertices, triangles.ToArray());
        }

        private static Mesh CapeMesh()
        {
            var vertices = new[]
            {
                new Vector3(-0.45f, 0.42f, 0f), new Vector3(0.45f, 0.42f, 0f),
                new Vector3(0.32f, -0.5f, 0.05f), new Vector3(-0.32f, -0.5f, 0.05f),
                new Vector3(-0.45f, 0.42f, 0.08f), new Vector3(0.45f, 0.42f, 0.08f),
                new Vector3(0.32f, -0.5f, 0.13f), new Vector3(-0.32f, -0.5f, 0.13f)
            };
            var triangles = new[]
            {
                0, 1, 2, 0, 2, 3, 4, 6, 5, 4, 7, 6,
                0, 4, 5, 0, 5, 1, 3, 2, 6, 3, 6, 7,
                0, 3, 7, 0, 7, 4, 1, 5, 6, 1, 6, 2
            };
            return CreateMesh(vertices, triangles);
        }

        private static Mesh CreateMesh(Vector3[] vertices, int[] triangles)
        {
            var mesh = new Mesh();
            mesh.name = "Original Low Poly Mesh";
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            var uv = new Vector2[vertices.Length];
            for (var index = 0; index < vertices.Length; index++)
            {
                uv[index] = new Vector2(vertices[index].x * 1.8f + vertices[index].z * 0.6f, vertices[index].y * 1.8f + vertices[index].z * 0.4f);
            }

            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Material MaterialFor(Color color, bool glow, TexturePattern pattern)
        {
            var rgba = (Color32)color;
            var key = rgba.r << 25 | rgba.g << 17 | rgba.b << 9 | rgba.a << 1 | (glow ? 1 : 0);
            key = key * 31 + (int)pattern;
            if (Materials.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var atlasEntry = GetTextureAtlasEntry(color, pattern);
            var material = VisualMaterialUtility.CreateTextured(color, albedoAtlas, normalAtlas, atlasEntry.Scale, atlasEntry.Offset, glow, glow ? 0.16f : 0.08f, glow ? 0.5f : 0.38f);
            Materials[key] = material;
            return material;
        }

        private static TextureAtlasEntry GetTextureAtlasEntry(Color color, TexturePattern pattern)
        {
            var rgba = (Color32)color;
            var key = rgba.r << 24 | rgba.g << 16 | rgba.b << 8 | (int)pattern;
            if (TextureEntries.TryGetValue(key, out var cached))
            {
                return cached;
            }

            EnsureTextureAtlases();
            if (nextAtlasSlot >= AtlasTilesPerSide * AtlasTilesPerSide)
            {
                nextAtlasSlot = 0;
            }

            var slot = nextAtlasSlot++;
            var tileX = (slot % AtlasTilesPerSide) * AtlasTileSize;
            var tileY = (slot / AtlasTilesPerSide) * AtlasTileSize;
            for (var y = 0; y < AtlasTileSize; y++)
            {
                for (var x = 0; x < AtlasTileSize; x++)
                {
                    var wave = Mathf.Sin((x * 1.7f + y * 2.3f) * Mathf.PI * 0.18f) * 0.035f;
                    var pixel = color * (0.94f + wave);
                    var normalX = 0.5f + wave * 1.8f;
                    var normalY = 0.5f + Mathf.Cos((x + y) * 0.23f) * 0.025f;
                    if (pattern == TexturePattern.Plate && (x == 0 || x == 16))
                    {
                        pixel = Color.Lerp(pixel, Color.white, 0.12f);
                        normalX += 0.06f;
                    }
                    else if (pattern == TexturePattern.Fabric && (x + y) % 7 == 0)
                    {
                        pixel = Color.Lerp(pixel, Color.black, 0.08f);
                        normalY += 0.05f;
                    }
                    else if (pattern == TexturePattern.Leather && (x * 3 + y) % 11 == 0)
                    {
                        pixel = Color.Lerp(pixel, Color.black, 0.15f);
                        normalX -= 0.04f;
                    }
                    else if (pattern == TexturePattern.Scale && (x + y) % 4 == 0)
                    {
                        pixel = Color.Lerp(pixel, Color.white, 0.07f);
                        normalY += 0.06f;
                    }
                    else if (pattern == TexturePattern.Rune && (x == y || x + y == AtlasTileSize - 1))
                    {
                        pixel = Color.Lerp(pixel, Color.white, 0.2f);
                        normalX += 0.05f;
                    }
                    else if (pattern == TexturePattern.Stone && (x * 5 + y * 3) % 13 == 0)
                    {
                        pixel = Color.Lerp(pixel, Color.black, 0.2f);
                        normalY -= 0.05f;
                    }
                    else if (pattern == TexturePattern.Bone && (x * 2 + y) % 9 == 0)
                    {
                        pixel = Color.Lerp(pixel, Color.white, 0.1f);
                        normalX += 0.03f;
                    }

                    albedoAtlas.SetPixel(tileX + x, tileY + y, pixel);
                    normalAtlas.SetPixel(tileX + x, tileY + y, new Color(normalX, normalY, 1f, 1f));
                }
            }

            albedoAtlas.Apply(false, false);
            normalAtlas.Apply(false, false);
            var entry = new TextureAtlasEntry
            {
                Scale = new Vector2(1f / AtlasTilesPerSide, 1f / AtlasTilesPerSide),
                Offset = new Vector2(tileX / (float)AtlasSize, tileY / (float)AtlasSize)
            };
            TextureEntries[key] = entry;
            return entry;
        }

        private static void EnsureTextureAtlases()
        {
            if (albedoAtlas != null && normalAtlas != null)
            {
                return;
            }

            albedoAtlas = new Texture2D(AtlasSize, AtlasSize, TextureFormat.RGBA32, false, true)
            {
                name = "Original Character Albedo Atlas 512",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            normalAtlas = new Texture2D(AtlasSize, AtlasSize, TextureFormat.RGBA32, false, true)
            {
                name = "Original Character Normal Atlas 512",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        private static Transform FindDeepChild(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }

                var nested = FindDeepChild(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }
    }

    // Small runtime descriptor so tools and future export code can identify
    // the class/gender behind a generated modular rig.
    public sealed class OriginalRigDescriptor : MonoBehaviour
    {
        public CharacterClassType ClassType;
        public CharacterGender Gender;
    }
}
