using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class PlayerAvatarVisual : MonoBehaviour
    {
        private GameObject visualRoot;
        private GameObject armorRoot;
        private GameObject cosmeticRoot;
        private Renderer baseRenderer;
        private CharacterClassType currentClass;
        private CharacterGender currentGender;
        private ClassDefinition currentDefinition;
        private PlayerEquipment currentEquipment;
        private bool hasVisual;

        private void Awake()
        {
            baseRenderer = GetComponent<Renderer>();
            if (baseRenderer != null)
            {
                baseRenderer.enabled = false;
            }
        }

        public void Apply(ClassDefinition definition, CharacterGender gender)
        {
            if (definition == null)
            {
                return;
            }

            if (hasVisual && currentClass == definition.Type && currentGender == gender)
            {
                RefreshEquipmentVisuals(currentEquipment);
                ApplyCosmetics(GetComponent<CosmeticService>());
                return;
            }

            currentClass = definition.Type;
            currentGender = gender;
            currentDefinition = definition;
            hasVisual = true;

            if (visualRoot != null)
            {
                Destroy(visualRoot);
            }

            visualRoot = new GameObject("Avatar Visual");
            visualRoot.transform.SetParent(transform, false);
            visualRoot.transform.localPosition = Vector3.zero;
            visualRoot.transform.localRotation = Quaternion.identity;
            visualRoot.transform.localScale = gender == CharacterGender.Femenino
                ? new Vector3(0.88f, 0.98f, 0.88f)
                : new Vector3(1.02f, 1.04f, 1.02f);

            // Modelo real si el asset existe en Resources; procedural si no.
            if (TryBuildCharacterModel(definition, gender))
            {
                BuildArmorOverlay(definition, gender);
                ApplyCosmetics(GetComponent<CosmeticService>());
                return;
            }

            var baseColor = definition.BodyColor;
            var accentColor = definition.SkillColor;
            var genderTint = gender == CharacterGender.Femenino
                ? new Color(1f, 0.78f, 0.9f)
                : new Color(0.76f, 0.88f, 1f);
            var bodyColor = Color.Lerp(baseColor, genderTint, 0.16f);

            CreatePart("Torso", PrimitiveType.Capsule, new Vector3(0f, 0f, 0f), new Vector3(0.55f, 0.82f, 0.42f), bodyColor);
            CreatePart("Head", PrimitiveType.Sphere, new Vector3(0f, 0.92f, 0f), new Vector3(0.42f, 0.42f, 0.42f), Color.Lerp(bodyColor, Color.white, 0.16f));
            CreatePart("Chest Accent", PrimitiveType.Cube, new Vector3(0f, 0.18f, -0.36f), new Vector3(0.5f, 0.12f, 0.08f), accentColor);
            CreatePart("Left Arm", PrimitiveType.Capsule, new Vector3(-0.42f, 0.18f, 0f), new Vector3(0.18f, 0.42f, 0.18f), Color.Lerp(bodyColor, Color.black, 0.08f), Quaternion.Euler(0f, 0f, 18f));
            CreatePart("Right Arm", PrimitiveType.Capsule, new Vector3(0.42f, 0.18f, 0f), new Vector3(0.18f, 0.42f, 0.18f), Color.Lerp(bodyColor, Color.black, 0.08f), Quaternion.Euler(0f, 0f, -18f));

            switch (definition.Type)
            {
                case CharacterClassType.Ninja:
                    BuildNinja(accentColor);
                    break;
                case CharacterClassType.Chaman:
                    BuildChaman(accentColor);
                    break;
                case CharacterClassType.Umbra:
                    BuildUmbra(accentColor);
                    break;
                default:
                    BuildGuerrero(accentColor);
                    break;
            }

            BuildArmorOverlay(definition, gender);
            BindMotionAnimator();
            ApplyCosmetics(GetComponent<CosmeticService>());
        }

        private bool TryBuildCharacterModel(ClassDefinition definition, CharacterGender gender)
        {
            var modelResource = definition.ModelResourceFor(gender);
            if (string.IsNullOrEmpty(modelResource))
            {
                return false;
            }

            var prefab = Resources.Load<GameObject>(modelResource);
            if (prefab == null)
            {
                return false;
            }

            var model = Instantiate(prefab, visualRoot.transform);
            model.name = "Character Model";
            // El pivote de los personajes KayKit esta en los pies; el del
            // jugador (capsula) en el centro, a 1 unidad del suelo.
            model.transform.localPosition = new Vector3(0f, -1f, 0f);
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;

            foreach (var modelCollider in model.GetComponentsInChildren<Collider>())
            {
                Destroy(modelCollider);
            }

            var modelAnimator = model.GetComponentInChildren<Animator>();
            if (modelAnimator == null)
            {
                modelAnimator = model.AddComponent<Animator>();
            }

            if (modelAnimator.avatar == null)
            {
                foreach (var importedAvatar in Resources.LoadAll<Avatar>(modelResource))
                {
                    if (importedAvatar != null)
                    {
                        modelAnimator.avatar = importedAvatar;
                        break;
                    }
                }
            }

            var controller = Resources.Load<RuntimeAnimatorController>(definition.AnimatorResourceFor(gender));
            if (controller != null)
            {
                modelAnimator.runtimeAnimatorController = controller;
            }

            BindMotionAnimator(modelAnimator);

            return true;
        }

        public void RefreshEquipmentVisuals(PlayerEquipment equipment)
        {
            currentEquipment = equipment;
            if (!hasVisual || visualRoot == null || currentDefinition == null)
            {
                return;
            }

            BuildArmorOverlay(currentDefinition, currentGender);
        }

        public void ApplyCosmetics(CosmeticService cosmetics)
        {
            if (!hasVisual || visualRoot == null)
            {
                return;
            }

            if (cosmeticRoot != null)
            {
                Destroy(cosmeticRoot);
            }

            cosmeticRoot = new GameObject("Cosmetic Visuals");
            cosmeticRoot.transform.SetParent(visualRoot.transform, false);

            var outfit = cosmetics != null ? cosmetics.GetActive(CosmeticSlot.Outfit) : null;
            var wings = cosmetics != null ? cosmetics.GetActive(CosmeticSlot.Wings) : null;
            if (outfit != null)
            {
                CreatePart(cosmeticRoot.transform, "Outfit Mantle", PrimitiveType.Cube, new Vector3(0f, 0.08f, 0.24f), new Vector3(0.68f, 0.65f, 0.08f), outfit.PrimaryColor);
                CreatePart(cosmeticRoot.transform, "Outfit Trim", PrimitiveType.Cube, new Vector3(0f, -0.2f, -0.44f), new Vector3(0.7f, 0.08f, 0.08f), outfit.SecondaryColor);
            }

            if (wings != null)
            {
                CreatePart(cosmeticRoot.transform, "Wing Left", PrimitiveType.Cube, new Vector3(-0.52f, 0.38f, 0.2f), new Vector3(0.12f, 0.72f, 0.28f), wings.PrimaryColor, Quaternion.Euler(0f, -24f, -22f));
                CreatePart(cosmeticRoot.transform, "Wing Right", PrimitiveType.Cube, new Vector3(0.52f, 0.38f, 0.2f), new Vector3(0.12f, 0.72f, 0.28f), wings.PrimaryColor, Quaternion.Euler(0f, 24f, 22f));
                CreatePart(cosmeticRoot.transform, "Wing Core", PrimitiveType.Sphere, new Vector3(0f, 0.32f, 0.28f), new Vector3(0.16f, 0.16f, 0.16f), wings.SecondaryColor);
            }
        }

        private void BuildArmorOverlay(ClassDefinition definition, CharacterGender gender)
        {
            if (armorRoot != null)
            {
                Destroy(armorRoot);
            }

            armorRoot = new GameObject("Equipment Armor Visual");
            armorRoot.transform.SetParent(visualRoot.transform, false);

            var profile = DefaultArmorVisualSets.Get(definition.Type, gender);
            var armorColor = profile.ArmorColor;
            var trimColor = profile.TrimColor;
            var accentColor = profile.AccentColor;
            CreatePart(armorRoot.transform, "Armor Chest", PrimitiveType.Cube, new Vector3(0f, 0.18f, -0.39f), new Vector3(0.56f, 0.24f, 0.1f), armorColor);
            CreatePart(armorRoot.transform, "Armor Belt", PrimitiveType.Cube, new Vector3(0f, -0.17f, -0.36f), new Vector3(0.6f, 0.1f, 0.08f), trimColor);

            if (profile.UseSkirt)
            {
                CreatePart(armorRoot.transform, "Armor Skirt", PrimitiveType.Cube, new Vector3(0f, -0.38f, 0f), new Vector3(0.58f, 0.22f, 0.42f), armorColor);
            }

            var shoulderWidth = profile.Style == ArmorVisualStyle.Assassin ? 0.42f : 0.5f;
            var shoulderDepth = profile.Style == ArmorVisualStyle.Void ? 0.38f : 0.32f;
            var shoulderSize = new Vector3(0.3f, 0.16f, shoulderDepth) * profile.ShoulderScale;
            var shoulderPrimitive = profile.Style == ArmorVisualStyle.Assassin || profile.Style == ArmorVisualStyle.Spirit
                ? PrimitiveType.Sphere
                : PrimitiveType.Cube;
            CreatePart(armorRoot.transform, "Armor Shoulder Left", shoulderPrimitive, new Vector3(-shoulderWidth, 0.5f, 0f), shoulderSize, accentColor);
            CreatePart(armorRoot.transform, "Armor Shoulder Right", shoulderPrimitive, new Vector3(shoulderWidth, 0.5f, 0f), shoulderSize, accentColor);

            switch (profile.Style)
            {
                case ArmorVisualStyle.Assassin:
                    CreatePart(armorRoot.transform, "Armor Sash", PrimitiveType.Cube, new Vector3(0f, -0.04f, -0.45f), new Vector3(0.64f, 0.08f, 0.06f), new Color(0.08f, 0.08f, 0.12f));
                    CreatePart(armorRoot.transform, "Armor Shoulder Strap", PrimitiveType.Cube, new Vector3(0f, 0.34f, -0.42f), new Vector3(0.08f, 0.42f, 0.06f), trimColor, Quaternion.Euler(0f, 0f, -22f));
                    break;
                case ArmorVisualStyle.Spirit:
                    CreatePart(armorRoot.transform, "Armor Spirit Charm", PrimitiveType.Sphere, new Vector3(0f, 0.42f, -0.46f), new Vector3(0.12f, 0.12f, 0.12f), trimColor);
                    CreatePart(armorRoot.transform, "Armor Spirit Collar", PrimitiveType.Cube, new Vector3(0f, 0.58f, -0.22f), new Vector3(0.34f, 0.07f, 0.06f), accentColor);
                    break;
                case ArmorVisualStyle.Void:
                    CreatePart(armorRoot.transform, "Armor Void Plate", PrimitiveType.Cube, new Vector3(0f, 0.45f, -0.42f), new Vector3(0.18f, 0.28f, 0.08f) * profile.ShoulderScale, trimColor);
                    CreatePart(armorRoot.transform, "Armor Void Spine", PrimitiveType.Cube, new Vector3(0f, 0.12f, 0.38f), new Vector3(0.12f, 0.5f, 0.08f), new Color(0.08f, 0.04f, 0.14f));
                    break;
            }

            BuildClassEmblem(armorRoot.transform, definition.Type, trimColor, accentColor);
            BuildEquippedVisuals(trimColor);
        }

        private void BuildClassEmblem(Transform parent, CharacterClassType classType, Color trimColor, Color accentColor)
        {
            switch (classType)
            {
                case CharacterClassType.Ninja:
                    CreatePart(parent, "Ninja Emblem", PrimitiveType.Cube, new Vector3(0f, 0.27f, -0.51f), new Vector3(0.24f, 0.08f, 0.04f), trimColor, Quaternion.Euler(0f, 0f, 45f));
                    break;
                case CharacterClassType.Chaman:
                    CreatePart(parent, "Chaman Emblem", PrimitiveType.Sphere, new Vector3(0f, 0.28f, -0.51f), new Vector3(0.14f, 0.14f, 0.06f), accentColor);
                    CreatePart(parent, "Chaman Emblem Ring", PrimitiveType.Cylinder, new Vector3(0f, 0.28f, -0.54f), new Vector3(0.22f, 0.018f, 0.22f), trimColor, Quaternion.Euler(90f, 0f, 0f));
                    break;
                case CharacterClassType.Umbra:
                    CreatePart(parent, "Umbra Emblem", PrimitiveType.Cube, new Vector3(0f, 0.27f, -0.51f), new Vector3(0.16f, 0.16f, 0.04f), accentColor, Quaternion.Euler(0f, 0f, 45f));
                    break;
                default:
                    CreatePart(parent, "Guerrero Emblem", PrimitiveType.Cube, new Vector3(0f, 0.27f, -0.51f), new Vector3(0.22f, 0.16f, 0.04f), trimColor);
                    break;
            }
        }

        private void BuildEquippedVisuals(Color trimColor)
        {
            if (currentEquipment == null)
            {
                return;
            }

            BuildEquippedPiece(EquipSlot.Weapon, new Vector3(0.44f, 0.08f, -0.3f), new Vector3(0.12f, 0.68f, 0.08f), Quaternion.Euler(0f, 0f, -24f));
            BuildEquippedPiece(EquipSlot.Helmet, new Vector3(0f, 1.08f, 0f), new Vector3(0.48f, 0.18f, 0.48f), Quaternion.identity);
            BuildEquippedPiece(EquipSlot.Chest, new Vector3(0f, 0.2f, -0.45f), new Vector3(0.62f, 0.32f, 0.08f), Quaternion.identity);
            BuildEquippedPiece(EquipSlot.Gloves, new Vector3(-0.48f, 0.12f, -0.08f), new Vector3(0.16f, 0.28f, 0.16f), Quaternion.identity);
            BuildEquippedPiece(EquipSlot.Pants, new Vector3(-0.2f, -0.38f, -0.02f), new Vector3(0.2f, 0.34f, 0.22f), Quaternion.identity);
            BuildEquippedPiece(EquipSlot.Boots, new Vector3(-0.22f, -0.72f, -0.18f), new Vector3(0.24f, 0.18f, 0.36f), Quaternion.identity);
            BuildEquippedPiece(EquipSlot.Necklace, new Vector3(0f, 0.36f, -0.52f), new Vector3(0.16f, 0.16f, 0.08f), Quaternion.identity);
            BuildEquippedPiece(EquipSlot.Talisman, new Vector3(0f, 0.62f, -0.56f), new Vector3(0.2f, 0.2f, 0.08f), Quaternion.identity);
            BuildEquippedPiece(EquipSlot.Cape, new Vector3(0f, 0.18f, 0.42f), new Vector3(0.74f, 0.82f, 0.06f), Quaternion.Euler(8f, 0f, 0f));
            BuildEquippedPiece(EquipSlot.RingLeft, new Vector3(-0.45f, 0.03f, -0.25f), new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity);
            BuildEquippedPiece(EquipSlot.RingRight, new Vector3(0.45f, 0.03f, -0.25f), new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity);
            BuildEquippedPiece(EquipSlot.Bracelet, new Vector3(-0.49f, 0.28f, -0.16f), new Vector3(0.14f, 0.14f, 0.08f), Quaternion.Euler(0f, 0f, 90f));
            BuildEquippedPiece(EquipSlot.Belt, new Vector3(0f, -0.17f, -0.43f), new Vector3(0.62f, 0.08f, 0.08f), Quaternion.identity);
        }

        private void BuildEquippedPiece(EquipSlot slot, Vector3 position, Vector3 size, Quaternion rotation)
        {
            var item = currentEquipment.GetDefinition(currentEquipment.GetEquipped(slot));
            if (item == null)
            {
                return;
            }

            var tier = Mathf.Clamp(item.ProgressionTier, 1, 10);
            var tierColor = Color.Lerp(new Color(0.34f, 0.42f, 0.52f), currentDefinition.SkillColor, tier / 10f);
            var rarityColor = RarityColor(item.Rarity);
            var metalColor = Color.Lerp(tierColor, rarityColor, 0.48f);
            CreatePart(armorRoot.transform, $"Equipped {slot} Tier {tier}", PrimitiveType.Cube, position, size, metalColor, rotation);

            if (slot == EquipSlot.Weapon)
            {
                CreatePart(armorRoot.transform, "Weapon Guard", PrimitiveType.Cube, position + new Vector3(0f, -0.32f, 0f), new Vector3(0.28f, 0.07f, 0.1f), rarityColor, rotation);
            }
            else if (slot == EquipSlot.Chest)
            {
                CreatePart(armorRoot.transform, "Armor Set Emblem", PrimitiveType.Cylinder, position + new Vector3(0f, 0f, -0.08f), new Vector3(0.12f, 0.025f, 0.12f), rarityColor, Quaternion.Euler(90f, 0f, 0f));
            }
            else if (slot == EquipSlot.Necklace || slot == EquipSlot.Talisman)
            {
                CreatePart(armorRoot.transform, "Relic Gem", PrimitiveType.Sphere, position + new Vector3(0f, -0.02f, -0.06f), new Vector3(0.1f, 0.1f, 0.1f), rarityColor);
            }
        }

        private static Color RarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Uncommon: return new Color(0.24f, 0.9f, 0.36f);
                case Rarity.Rare: return new Color(0.28f, 0.56f, 1f);
                case Rarity.Epic: return new Color(0.68f, 0.3f, 1f);
                case Rarity.Legendary: return new Color(1f, 0.58f, 0.12f);
                case Rarity.Mythic: return new Color(1f, 0.2f, 0.42f);
                default: return new Color(0.78f, 0.82f, 0.88f);
            }
        }

        private void BindMotionAnimator(Animator modelAnimator = null)
        {
            var animator = GetComponent<AvatarMotionAnimator>();
            if (animator == null)
            {
                animator = gameObject.AddComponent<AvatarMotionAnimator>();
            }

            animator.SetVisualRoot(visualRoot.transform, modelAnimator);
        }

        private void BuildGuerrero(Color accentColor)
        {
            CreatePart("Left Shoulder", PrimitiveType.Cube, new Vector3(-0.52f, 0.52f, 0f), new Vector3(0.34f, 0.18f, 0.36f), Color.Lerp(accentColor, Color.white, 0.16f));
            CreatePart("Right Shoulder", PrimitiveType.Cube, new Vector3(0.52f, 0.52f, 0f), new Vector3(0.34f, 0.18f, 0.36f), Color.Lerp(accentColor, Color.white, 0.16f));
            CreatePart("Back Blade", PrimitiveType.Cube, new Vector3(0.32f, 0.42f, 0.42f), new Vector3(0.08f, 0.92f, 0.12f), new Color(0.85f, 0.9f, 0.92f), Quaternion.Euler(0f, 0f, -34f));
        }

        private void BuildNinja(Color accentColor)
        {
            CreatePart("Mask", PrimitiveType.Cube, new Vector3(0f, 0.92f, -0.22f), new Vector3(0.38f, 0.12f, 0.06f), Color.Lerp(Color.black, accentColor, 0.22f));
            CreatePart("Left Dagger", PrimitiveType.Cube, new Vector3(-0.52f, -0.08f, -0.22f), new Vector3(0.06f, 0.48f, 0.08f), new Color(0.76f, 0.84f, 0.88f), Quaternion.Euler(0f, 0f, 22f));
            CreatePart("Right Dagger", PrimitiveType.Cube, new Vector3(0.52f, -0.08f, -0.22f), new Vector3(0.06f, 0.48f, 0.08f), new Color(0.76f, 0.84f, 0.88f), Quaternion.Euler(0f, 0f, -22f));
        }

        private void BuildChaman(Color accentColor)
        {
            CreatePart("Crown Orb", PrimitiveType.Sphere, new Vector3(0f, 1.28f, 0f), new Vector3(0.18f, 0.18f, 0.18f), Color.Lerp(accentColor, Color.white, 0.28f));
            CreatePart("Spirit Staff", PrimitiveType.Cylinder, new Vector3(0.58f, 0.28f, 0.05f), new Vector3(0.07f, 0.82f, 0.07f), new Color(0.72f, 0.54f, 0.24f), Quaternion.Euler(0f, 0f, -12f));
            CreatePart("Staff Crystal", PrimitiveType.Sphere, new Vector3(0.68f, 0.98f, 0.05f), new Vector3(0.2f, 0.2f, 0.2f), accentColor);
        }

        private void BuildUmbra(Color accentColor)
        {
            CreatePart("Void Crest", PrimitiveType.Cube, new Vector3(0f, 1.24f, 0f), new Vector3(0.16f, 0.34f, 0.16f), accentColor, Quaternion.Euler(0f, 0f, 45f));
            CreatePart("Left Shadow Guard", PrimitiveType.Cube, new Vector3(-0.54f, 0.45f, 0f), new Vector3(0.22f, 0.42f, 0.2f), Color.Lerp(accentColor, Color.black, 0.22f), Quaternion.Euler(0f, 0f, 18f));
            CreatePart("Right Shadow Guard", PrimitiveType.Cube, new Vector3(0.54f, 0.45f, 0f), new Vector3(0.22f, 0.42f, 0.2f), Color.Lerp(accentColor, Color.black, 0.22f), Quaternion.Euler(0f, 0f, -18f));
            CreatePart("Void Core", PrimitiveType.Sphere, new Vector3(0f, 0.22f, -0.42f), new Vector3(0.22f, 0.22f, 0.22f), Color.Lerp(accentColor, Color.white, 0.12f));
        }

        private GameObject CreatePart(string partName, PrimitiveType primitive, Vector3 localPosition, Vector3 localScale, Color color)
        {
            return CreatePart(partName, primitive, localPosition, localScale, color, Quaternion.identity);
        }

        private GameObject CreatePart(string partName, PrimitiveType primitive, Vector3 localPosition, Vector3 localScale, Color color, Quaternion localRotation)
        {
            return CreatePart(visualRoot.transform, partName, primitive, localPosition, localScale, color, localRotation);
        }

        private GameObject CreatePart(Transform parent, string partName, PrimitiveType primitive, Vector3 localPosition, Vector3 localScale, Color color)
        {
            return CreatePart(parent, partName, primitive, localPosition, localScale, color, Quaternion.identity);
        }

        private GameObject CreatePart(Transform parent, string partName, PrimitiveType primitive, Vector3 localPosition, Vector3 localScale, Color color, Quaternion localRotation)
        {
            var part = GameObject.CreatePrimitive(primitive);
            part.name = partName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;

            var collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = VisualMaterialUtility.Create(color, VisualMaterialUtility.ShouldGlow(partName), 0.08f, 0.3f);
            }

            return part;
        }
    }
}
