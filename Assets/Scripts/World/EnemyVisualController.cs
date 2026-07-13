using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Mobile-friendly enemy presentation. Real Quaternius monsters are selected
    // by zone/tier and procedural parts remain as a safe fallback.
    public sealed class EnemyVisualController : MonoBehaviour
    {
        private static readonly Dictionary<int, Material> Materials = new Dictionary<int, Material>();
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyColorId = Shader.PropertyToID("_Color");
        private GameObject visualRoot;
        private GameObject statusRoot;
        private Transform healthFill;
        private Health health;
        private float healthBarWidth = 1.25f;
        private Vector3 visualBaseScale;
        private Coroutine deathRoutine;

        public void Initialize(string enemyId, string displayName, EnemyTier tier, Color baseColor, float scale, Health enemyHealth)
        {
            health = enemyHealth;
            visualRoot = new GameObject("Enemy Visual");
            visualRoot.transform.SetParent(transform, false);
            visualRoot.transform.localScale = Vector3.one * Mathf.Max(0.7f, scale);
            visualBaseScale = visualRoot.transform.localScale;

            var id = (enemyId ?? string.Empty).ToLowerInvariant();
            var accent = AccentFor(tier, baseColor);

            var builtRealModel = TryBuildRealModel(id, tier, baseColor, accent);
            if (builtRealModel)
            {
                ApplyZoneVariant(id, baseColor, accent, tier);
                BuildTierAdornment(tier, accent, id);
            }
            else if (id.Contains("forest") || id.Contains("valley"))
            {
                BuildBeast(baseColor, accent);
                ApplyZoneVariant(id, baseColor, accent, tier);
                BuildTierAdornment(tier, accent, id);
            }
            else if (id.Contains("crystal") || id.Contains("frost") || id.Contains("obsidian"))
            {
                BuildGuardian(baseColor, accent);
                ApplyZoneVariant(id, baseColor, accent, tier);
                BuildTierAdornment(tier, accent, id);
            }
            else if (id.Contains("sunken") || id.Contains("astral"))
            {
                BuildSpirit(baseColor, accent);
                ApplyZoneVariant(id, baseColor, accent, tier);
                BuildTierAdornment(tier, accent, id);
            }
            else
            {
                BuildShadow(baseColor, accent);
                ApplyZoneVariant(id, baseColor, accent, tier);
                BuildTierAdornment(tier, accent, id);
            }

            if (tier == EnemyTier.Boss)
            {
                BuildBossIdentity(id, baseColor, accent);
            }

            BuildStatusDisplay(displayName, tier, accent);
        }

        public void PlayDeath()
        {
            if (deathRoutine != null)
            {
                StopCoroutine(deathRoutine);
            }

            if (statusRoot != null)
            {
                statusRoot.SetActive(false);
            }

            deathRoutine = StartCoroutine(DeathAnimation());
        }

        private void Update()
        {
            if (health == null || healthFill == null)
            {
                return;
            }

            var normalized = health.Normalized;
            healthFill.localScale = new Vector3(Mathf.Max(0.01f, normalized), 1f, 1f);
            healthFill.localPosition = new Vector3(-healthBarWidth * 0.5f * (1f - normalized), 0f, -0.035f);

            var camera = Camera.main;
            if (camera != null && statusRoot != null)
            {
                statusRoot.transform.LookAt(camera.transform);
            }
        }

        private IEnumerator DeathAnimation()
        {
            var elapsed = 0f;
            const float duration = 1.2f;
            while (elapsed < duration && visualRoot != null)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                visualRoot.transform.localScale = Vector3.Lerp(visualBaseScale, Vector3.one * 0.08f, t);
                visualRoot.transform.localRotation = Quaternion.Euler(0f, t * 18f, t * 12f);
                yield return null;
            }
        }

        private bool TryBuildRealModel(string enemyId, EnemyTier tier, Color baseColor, Color accent)
        {
            var modelResource = ModelResourceFor(enemyId, tier);
            var prefab = Resources.Load<GameObject>(modelResource);
            if (prefab == null)
            {
                return false;
            }

            var model = Instantiate(prefab, visualRoot.transform);
            model.name = "Enemy 3D Model";
            model.transform.localPosition = ModelOffsetFor(modelResource);
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one * ModelScaleFor(modelResource, tier);

            if (tier == EnemyTier.Boss)
            {
                ApplyBossPalette(model, enemyId, baseColor);
            }

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

            var controller = Resources.Load<RuntimeAnimatorController>(AnimatorResourceFor(enemyId, tier));
            if (controller != null)
            {
                modelAnimator.runtimeAnimatorController = controller;
            }

            var motion = GetComponent<AvatarMotionAnimator>() ?? gameObject.AddComponent<AvatarMotionAnimator>();
            motion.SetVisualRoot(model.transform, modelAnimator);
            return true;
        }

        private static void ApplyBossPalette(GameObject model, string enemyId, Color fallback)
        {
            var palette = BossPaletteFor(enemyId, fallback);
            var block = new MaterialPropertyBlock();
            foreach (var renderer in model.GetComponentsInChildren<Renderer>())
            {
                renderer.GetPropertyBlock(block);
                var tint = Color.Lerp(Color.white, palette, 0.22f);
                block.SetColor(BaseColorId, tint);
                block.SetColor(LegacyColorId, tint);
                renderer.SetPropertyBlock(block);
            }
        }

        private static Color BossPaletteFor(string enemyId, Color fallback)
        {
            if (enemyId.Contains("valley"))
            {
                return new Color(0.76f, 0.5f, 0.18f);
            }

            if (enemyId.Contains("forest"))
            {
                return new Color(0.18f, 0.55f, 0.25f);
            }

            if (enemyId.Contains("ash"))
            {
                return new Color(0.75f, 0.25f, 0.1f);
            }

            if (enemyId.Contains("crystal"))
            {
                return new Color(0.28f, 0.75f, 0.95f);
            }

            if (enemyId.Contains("frost"))
            {
                return new Color(0.55f, 0.85f, 1f);
            }

            if (enemyId.Contains("sunken"))
            {
                return new Color(0.1f, 0.72f, 0.68f);
            }

            if (enemyId.Contains("obsidian"))
            {
                return new Color(0.95f, 0.18f, 0.06f);
            }

            if (enemyId.Contains("astral"))
            {
                return new Color(0.58f, 0.32f, 1f);
            }

            if (enemyId.Contains("eclipse"))
            {
                return new Color(0.42f, 0.08f, 0.65f);
            }

            if (enemyId.Contains("throne"))
            {
                return new Color(0.72f, 0.12f, 1f);
            }

            return fallback;
        }

        private static Vector3 ModelOffsetFor(string enemyId)
        {
            // Quaternius monsters use a ground-centered root. The old KayKit
            // characters need the legacy offset, so only those stay lowered.
            return (enemyId ?? string.Empty).ToLowerInvariant().Contains("kaykit")
                ? new Vector3(0f, -1f, 0f)
                : Vector3.zero;
        }

        private static float ModelScaleFor(string enemyId, EnemyTier tier)
        {
            enemyId = (enemyId ?? string.Empty).ToLowerInvariant();
            var scale = 0.85f;
            if (enemyId.Contains("glub") || enemyId.Contains("goleling"))
            {
                scale = 1.05f;
            }
            else if (enemyId.Contains("dragon") || enemyId.Contains("orc") || enemyId.Contains("demon"))
            {
                scale = 0.78f;
            }
            else if (enemyId.Contains("fish") || enemyId.Contains("yeti"))
            {
                scale = 0.92f;
            }
            else if (enemyId.Contains("ghost"))
            {
                scale = 0.88f;
            }

            if (tier == EnemyTier.Elite)
            {
                scale *= 1.08f;
            }
            else if (tier == EnemyTier.Boss)
            {
                scale *= 1.16f;
            }

            return scale;
        }

        private static string ModelResourceFor(string enemyId, EnemyTier tier)
        {
            if (enemyId.Contains("valley"))
            {
                return tier == EnemyTier.Boss ? MonsterResource("Orc_Skull") : tier == EnemyTier.Elite ? MonsterResource("Orc") : MonsterResource("MushroomKing");
            }

            if (enemyId.Contains("forest"))
            {
                return tier == EnemyTier.Boss ? MonsterResource("Yeti") : tier == EnemyTier.Elite ? MonsterResource("Orc") : MonsterResource("Tribal");
            }

            if (enemyId.Contains("ash"))
            {
                return tier == EnemyTier.Boss ? MonsterResource("BlueDemon") : tier == EnemyTier.Elite ? MonsterResource("Demon") : MonsterResource("MushroomKing");
            }

            if (enemyId.Contains("crystal"))
            {
                return tier == EnemyTier.Boss ? MonsterResource("Dragon") : tier == EnemyTier.Elite ? MonsterResource("Goleling_Evolved") : MonsterResource("Goleling");
            }

            if (enemyId.Contains("frost"))
            {
                return tier == EnemyTier.Boss ? MonsterResource("Goleling_Evolved") : tier == EnemyTier.Elite ? MonsterResource("Yeti") : MonsterResource("Goleling_Evolved");
            }

            if (enemyId.Contains("sunken"))
            {
                return tier == EnemyTier.Boss ? MonsterResource("Fish") : tier == EnemyTier.Elite ? MonsterResource("Glub_Evolved") : MonsterResource("Glub");
            }

            if (enemyId.Contains("obsidian"))
            {
                return tier == EnemyTier.Boss ? MonsterResource("Dragon_Evolved") : tier == EnemyTier.Elite ? MonsterResource("Demon") : MonsterResource("BlueDemon");
            }

            if (enemyId.Contains("astral"))
            {
                return tier == EnemyTier.Boss ? MonsterResource("Ghost_Skull") : tier == EnemyTier.Elite ? MonsterResource("Ghost_Skull") : MonsterResource("Ghost");
            }

            if (enemyId.Contains("eclipse"))
            {
                return tier == EnemyTier.Boss ? MonsterResource("Demon") : tier == EnemyTier.Elite ? MonsterResource("BlueDemon") : MonsterResource("Ghost");
            }

            if (enemyId.Contains("throne"))
            {
                return tier == EnemyTier.Boss ? MonsterResource("Orc") : tier == EnemyTier.Elite ? MonsterResource("Orc_Skull") : MonsterResource("Dragon");
            }

            if (tier == EnemyTier.Boss)
            {
                return "ThirdParty/KayKit/Adventurers/Characters/Barbarian";
            }

            if (enemyId.Contains("crystal") || enemyId.Contains("frost"))
            {
                return "ThirdParty/KayKit/Adventurers/Characters/Knight";
            }

            if (enemyId.Contains("sunken") || enemyId.Contains("astral"))
            {
                return "ThirdParty/KayKit/Adventurers/Characters/Mage";
            }

            return "ThirdParty/KayKit/Adventurers/Characters/RogueHooded";
        }

        private static string MonsterResource(string modelName)
        {
            return $"ThirdParty/Quaternius/UltimateMonsters/FBX/{modelName}";
        }

        private static string AnimatorResourceFor(string enemyId, EnemyTier tier)
        {
            var modelResource = ModelResourceFor(enemyId, tier);
            if (modelResource.StartsWith("ThirdParty/Quaternius/UltimateMonsters/FBX/"))
            {
                var modelName = modelResource.Substring(modelResource.LastIndexOf('/') + 1);
                return $"ThirdParty/Quaternius/UltimateMonsters/Controllers/{modelName}";
            }

            if (tier == EnemyTier.Boss || enemyId.Contains("obsidian") || enemyId.Contains("throne"))
            {
                return "ThirdParty/KayKit/Adventurers/Controllers/Barbarian";
            }

            if (enemyId.Contains("crystal") || enemyId.Contains("frost"))
            {
                return "ThirdParty/KayKit/Adventurers/Controllers/Knight";
            }

            if (enemyId.Contains("sunken") || enemyId.Contains("astral"))
            {
                return "ThirdParty/KayKit/Adventurers/Controllers/Mage";
            }

            return "ThirdParty/KayKit/Adventurers/Controllers/Rogue";
        }

        private void BuildBeast(Color bodyColor, Color accent)
        {
            var dark = Color.Lerp(bodyColor, Color.black, 0.32f);
            CreatePart("Beast Body", PrimitiveType.Capsule, new Vector3(0f, 0f, 0f), new Vector3(0.58f, 0.5f, 0.88f), bodyColor, Quaternion.Euler(90f, 0f, 0f));
            CreatePart("Beast Head", PrimitiveType.Sphere, new Vector3(0f, 0.12f, 0.72f), new Vector3(0.5f, 0.46f, 0.5f), bodyColor);
            CreatePart("Beast Snout", PrimitiveType.Cube, new Vector3(0f, 0.02f, 0.98f), new Vector3(0.28f, 0.2f, 0.28f), dark);
            CreatePart("Beast Leg L", PrimitiveType.Capsule, new Vector3(-0.32f, -0.43f, 0.38f), new Vector3(0.17f, 0.38f, 0.17f), dark);
            CreatePart("Beast Leg R", PrimitiveType.Capsule, new Vector3(0.32f, -0.43f, 0.38f), new Vector3(0.17f, 0.38f, 0.17f), dark);
            CreatePart("Beast Leg Back L", PrimitiveType.Capsule, new Vector3(-0.32f, -0.43f, -0.38f), new Vector3(0.17f, 0.38f, 0.17f), dark);
            CreatePart("Beast Leg Back R", PrimitiveType.Capsule, new Vector3(0.32f, -0.43f, -0.38f), new Vector3(0.17f, 0.38f, 0.17f), dark);
            CreatePart("Beast Ear L", PrimitiveType.Cylinder, new Vector3(-0.22f, 0.48f, 0.68f), new Vector3(0.12f, 0.24f, 0.12f), accent, Quaternion.Euler(0f, 0f, -24f));
            CreatePart("Beast Ear R", PrimitiveType.Cylinder, new Vector3(0.22f, 0.48f, 0.68f), new Vector3(0.12f, 0.24f, 0.12f), accent, Quaternion.Euler(0f, 0f, 24f));
            CreatePart("Beast Tail", PrimitiveType.Cylinder, new Vector3(0f, 0.12f, -0.85f), new Vector3(0.12f, 0.42f, 0.12f), accent, Quaternion.Euler(-42f, 0f, 0f));
            CreateEyes(new Vector3(0f, 0.18f, 0.98f), new Color(1f, 0.82f, 0.2f));
        }

        private void BuildGuardian(Color bodyColor, Color accent)
        {
            var dark = Color.Lerp(bodyColor, Color.black, 0.3f);
            CreatePart("Guardian Core", PrimitiveType.Cube, new Vector3(0f, 0.02f, 0f), new Vector3(0.72f, 1.05f, 0.58f), bodyColor);
            CreatePart("Guardian Head", PrimitiveType.Cube, new Vector3(0f, 0.76f, 0f), new Vector3(0.52f, 0.44f, 0.5f), dark);
            CreatePart("Guardian Visor", PrimitiveType.Cube, new Vector3(0f, 0.78f, -0.27f), new Vector3(0.38f, 0.1f, 0.06f), accent);
            CreatePart("Guardian Shoulder L", PrimitiveType.Cube, new Vector3(-0.52f, 0.36f, 0f), new Vector3(0.28f, 0.3f, 0.62f), accent, Quaternion.Euler(0f, 0f, -12f));
            CreatePart("Guardian Shoulder R", PrimitiveType.Cube, new Vector3(0.52f, 0.36f, 0f), new Vector3(0.28f, 0.3f, 0.62f), accent, Quaternion.Euler(0f, 0f, 12f));
            CreatePart("Guardian Leg L", PrimitiveType.Cube, new Vector3(-0.22f, -0.68f, 0f), new Vector3(0.25f, 0.42f, 0.3f), dark);
            CreatePart("Guardian Leg R", PrimitiveType.Cube, new Vector3(0.22f, -0.68f, 0f), new Vector3(0.25f, 0.42f, 0.3f), dark);
            CreatePart("Guardian Core Gem", PrimitiveType.Sphere, new Vector3(0f, 0.05f, -0.34f), new Vector3(0.16f, 0.16f, 0.1f), Color.Lerp(accent, Color.white, 0.25f));
        }

        private void BuildSpirit(Color bodyColor, Color accent)
        {
            var dark = Color.Lerp(bodyColor, Color.black, 0.42f);
            CreatePart("Spirit Body", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0f), new Vector3(0.62f, 0.86f, 0.46f), bodyColor);
            CreatePart("Spirit Hood", PrimitiveType.Sphere, new Vector3(0f, 0.82f, 0f), new Vector3(0.48f, 0.42f, 0.48f), dark);
            CreatePart("Spirit Face", PrimitiveType.Cube, new Vector3(0f, 0.78f, -0.26f), new Vector3(0.3f, 0.1f, 0.06f), accent);
            CreatePart("Spirit Sash", PrimitiveType.Cube, new Vector3(0f, 0.05f, -0.38f), new Vector3(0.68f, 0.12f, 0.08f), accent);
            CreatePart("Spirit Orb", PrimitiveType.Sphere, new Vector3(0f, 0.36f, -0.5f), new Vector3(0.14f, 0.14f, 0.14f), Color.Lerp(accent, Color.white, 0.2f));
            CreatePart("Spirit Skirt", PrimitiveType.Cube, new Vector3(0f, -0.55f, 0f), new Vector3(0.72f, 0.3f, 0.52f), dark);
        }

        private void BuildShadow(Color bodyColor, Color accent)
        {
            var dark = Color.Lerp(bodyColor, Color.black, 0.5f);
            CreatePart("Shadow Body", PrimitiveType.Capsule, new Vector3(0f, 0f, 0f), new Vector3(0.58f, 0.86f, 0.44f), bodyColor);
            CreatePart("Shadow Hood", PrimitiveType.Sphere, new Vector3(0f, 0.88f, 0f), new Vector3(0.46f, 0.46f, 0.46f), dark);
            CreatePart("Shadow Cloak", PrimitiveType.Cube, new Vector3(0f, 0.02f, 0.28f), new Vector3(0.76f, 0.96f, 0.12f), dark, Quaternion.Euler(10f, 0f, 0f));
            CreatePart("Shadow Blade", PrimitiveType.Cube, new Vector3(0.48f, 0.1f, -0.08f), new Vector3(0.08f, 0.68f, 0.08f), accent, Quaternion.Euler(0f, 0f, -18f));
            CreatePart("Shadow Eye Line", PrimitiveType.Cube, new Vector3(0f, 0.9f, -0.36f), new Vector3(0.3f, 0.08f, 0.06f), accent);
        }

        private void ApplyZoneVariant(string enemyId, Color bodyColor, Color accent, EnemyTier tier)
        {
            if (enemyId.Contains("valley"))
            {
                CreatePart("Relic Collar", PrimitiveType.Cylinder, new Vector3(0f, 0.58f, 0f), new Vector3(0.34f, 0.08f, 0.34f), new Color(0.76f, 0.5f, 0.18f));
                if (tier == EnemyTier.Elite)
                {
                    CreatePart("Relic Shoulder L", PrimitiveType.Cube, new Vector3(-0.48f, 0.38f, 0f), new Vector3(0.24f, 0.24f, 0.44f), accent, Quaternion.Euler(0f, 0f, -18f));
                    CreatePart("Relic Shoulder R", PrimitiveType.Cube, new Vector3(0.48f, 0.38f, 0f), new Vector3(0.24f, 0.24f, 0.44f), accent, Quaternion.Euler(0f, 0f, 18f));
                }
                else if (tier == EnemyTier.Boss)
                {
                    CreatePart("Relic Boss Plate", PrimitiveType.Cube, new Vector3(0f, 0.18f, -0.46f), new Vector3(0.58f, 0.36f, 0.1f), accent);
                    CreatePart("Relic Boss Horn L", PrimitiveType.Cylinder, new Vector3(-0.34f, 0.52f, 0.62f), new Vector3(0.12f, 0.42f, 0.12f), new Color(0.9f, 0.7f, 0.28f), Quaternion.Euler(0f, 0f, -30f));
                    CreatePart("Relic Boss Horn R", PrimitiveType.Cylinder, new Vector3(0.34f, 0.52f, 0.62f), new Vector3(0.12f, 0.42f, 0.12f), new Color(0.9f, 0.7f, 0.28f), Quaternion.Euler(0f, 0f, 30f));
                }
                return;
            }

            if (enemyId.Contains("forest"))
            {
                CreatePart("Forest Vines", PrimitiveType.Cylinder, new Vector3(0f, 0.15f, -0.36f), new Vector3(0.09f, 0.44f, 0.09f), new Color(0.12f, 0.55f, 0.2f), Quaternion.Euler(0f, 0f, 28f));
                CreatePart("Forest Thorn", PrimitiveType.Cylinder, new Vector3(0.28f, 0.62f, 0f), new Vector3(0.08f, 0.3f, 0.08f), Color.Lerp(bodyColor, Color.black, 0.2f), Quaternion.Euler(0f, 0f, 34f));
                return;
            }

            if (enemyId.Contains("ash") || enemyId.Contains("eclipse"))
            {
                CreatePart("Ash Core", PrimitiveType.Sphere, new Vector3(0f, 0.18f, -0.4f), new Vector3(0.19f, 0.19f, 0.12f), new Color(1f, 0.28f, 0.08f));
                CreatePart("Ash Smoke", PrimitiveType.Sphere, new Vector3(0.25f, 0.7f, 0.1f), new Vector3(0.13f, 0.2f, 0.13f), Color.Lerp(bodyColor, Color.black, 0.2f));
                return;
            }

            if (enemyId.Contains("crystal"))
            {
                CreatePart("Prism Core", PrimitiveType.Sphere, new Vector3(0f, 0.26f, -0.38f), new Vector3(0.2f, 0.2f, 0.12f), Color.Lerp(accent, Color.white, 0.35f));
                return;
            }

            if (enemyId.Contains("frost"))
            {
                CreatePart("Frost Crest", PrimitiveType.Cylinder, new Vector3(0f, 1.1f, 0f), new Vector3(0.18f, 0.34f, 0.18f), new Color(0.76f, 0.94f, 1f), Quaternion.Euler(0f, 0f, 18f));
                return;
            }

            if (enemyId.Contains("sunken"))
            {
                CreatePart("Abyssal Pearl", PrimitiveType.Sphere, new Vector3(0f, 0.45f, -0.4f), new Vector3(0.18f, 0.18f, 0.12f), new Color(0.14f, 0.9f, 0.86f));
                CreatePart("Abyssal Fin", PrimitiveType.Cube, new Vector3(0f, 0.2f, 0.42f), new Vector3(0.12f, 0.48f, 0.08f), accent, Quaternion.Euler(24f, 0f, 0f));
                return;
            }

            if (enemyId.Contains("obsidian"))
            {
                CreatePart("Magma Core", PrimitiveType.Sphere, new Vector3(0f, 0.2f, -0.4f), new Vector3(0.22f, 0.22f, 0.12f), new Color(1f, 0.22f, 0.08f));
                CreatePart("Forge Hammer", PrimitiveType.Cube, new Vector3(0.45f, 0.24f, 0f), new Vector3(0.16f, 0.55f, 0.16f), accent, Quaternion.Euler(0f, 0f, -28f));
                return;
            }

            if (enemyId.Contains("astral"))
            {
                CreatePart("Astral Star", PrimitiveType.Sphere, new Vector3(0f, 1.18f, -0.12f), new Vector3(0.22f, 0.22f, 0.22f), new Color(0.84f, 0.72f, 1f));
                CreatePart("Astral Ring", PrimitiveType.Cylinder, new Vector3(0f, 0.54f, 0f), new Vector3(0.45f, 0.025f, 0.45f), accent, Quaternion.Euler(90f, 0f, 0f));
                return;
            }

            if (enemyId.Contains("throne"))
            {
                CreatePart("Void Crown", PrimitiveType.Cylinder, new Vector3(0f, 1.18f, 0f), new Vector3(0.42f, 0.1f, 0.42f), new Color(0.76f, 0.22f, 1f));
                CreatePart("Void Core", PrimitiveType.Sphere, new Vector3(0f, 0.25f, -0.4f), new Vector3(0.23f, 0.23f, 0.12f), Color.Lerp(accent, Color.black, 0.2f));
            }
        }

        private void BuildBossIdentity(string enemyId, Color bodyColor, Color accent)
        {
            var palette = BossPaletteFor(enemyId, bodyColor);
            var shadow = Color.Lerp(palette, Color.black, 0.34f);
            var highlight = Color.Lerp(palette, Color.white, 0.38f);

            if (enemyId.Contains("valley"))
            {
                CreatePart("Valley Relic Crown", PrimitiveType.Cylinder, new Vector3(0f, 1.42f, 0f), new Vector3(0.48f, 0.08f, 0.48f), palette);
                CreatePart("Valley Relic Rune", PrimitiveType.Sphere, new Vector3(0f, 0.62f, -0.5f), new Vector3(0.2f, 0.2f, 0.1f), highlight);
                CreatePart("Valley Relic Mantle", PrimitiveType.Cube, new Vector3(0f, 0.42f, 0.3f), new Vector3(0.9f, 0.16f, 0.12f), shadow, Quaternion.Euler(0f, 0f, 8f));
                return;
            }

            if (enemyId.Contains("forest"))
            {
                CreatePart("Forest Root Mantle", PrimitiveType.Cube, new Vector3(0f, 0.36f, 0.35f), new Vector3(1.02f, 0.42f, 0.12f), shadow, Quaternion.Euler(12f, 0f, 0f));
                CreatePart("Forest Antler L", PrimitiveType.Cylinder, new Vector3(-0.38f, 1.3f, 0f), new Vector3(0.1f, 0.56f, 0.1f), palette, Quaternion.Euler(0f, 0f, -28f));
                CreatePart("Forest Antler R", PrimitiveType.Cylinder, new Vector3(0.38f, 1.3f, 0f), new Vector3(0.1f, 0.56f, 0.1f), palette, Quaternion.Euler(0f, 0f, 28f));
                return;
            }

            if (enemyId.Contains("ash"))
            {
                CreatePart("Ash Ember Halo", PrimitiveType.Cylinder, new Vector3(0f, 0.78f, 0f), new Vector3(0.74f, 0.035f, 0.74f), palette, Quaternion.Euler(90f, 0f, 0f));
                CreatePart("Ash Ember Core", PrimitiveType.Sphere, new Vector3(0f, 0.42f, -0.48f), new Vector3(0.24f, 0.24f, 0.12f), highlight);
                CreatePart("Ash Smoke Crest", PrimitiveType.Capsule, new Vector3(0.34f, 1.18f, 0.04f), new Vector3(0.14f, 0.36f, 0.14f), shadow, Quaternion.Euler(0f, 0f, 18f));
                return;
            }

            if (enemyId.Contains("crystal"))
            {
                CreatePart("Crystal Crown L", PrimitiveType.Cube, new Vector3(-0.36f, 1.34f, 0f), new Vector3(0.16f, 0.62f, 0.16f), highlight, Quaternion.Euler(0f, 0f, -18f));
                CreatePart("Crystal Crown Core", PrimitiveType.Cube, new Vector3(0f, 1.48f, 0f), new Vector3(0.18f, 0.7f, 0.18f), palette);
                CreatePart("Crystal Crown R", PrimitiveType.Cube, new Vector3(0.36f, 1.34f, 0f), new Vector3(0.16f, 0.62f, 0.16f), highlight, Quaternion.Euler(0f, 0f, 18f));
                return;
            }

            if (enemyId.Contains("frost"))
            {
                CreatePart("Frost Antler L", PrimitiveType.Cylinder, new Vector3(-0.36f, 1.3f, 0f), new Vector3(0.1f, 0.62f, 0.1f), highlight, Quaternion.Euler(0f, 0f, -24f));
                CreatePart("Frost Antler R", PrimitiveType.Cylinder, new Vector3(0.36f, 1.3f, 0f), new Vector3(0.1f, 0.62f, 0.1f), highlight, Quaternion.Euler(0f, 0f, 24f));
                CreatePart("Frost Plate", PrimitiveType.Cube, new Vector3(0f, 0.48f, -0.43f), new Vector3(0.72f, 0.3f, 0.1f), palette, Quaternion.Euler(-8f, 0f, 0f));
                return;
            }

            if (enemyId.Contains("sunken"))
            {
                CreatePart("Sunken Abyss Pearl", PrimitiveType.Sphere, new Vector3(0f, 0.62f, -0.5f), new Vector3(0.26f, 0.26f, 0.14f), highlight);
                CreatePart("Sunken Fin L", PrimitiveType.Cube, new Vector3(-0.42f, 0.36f, 0.18f), new Vector3(0.12f, 0.62f, 0.1f), palette, Quaternion.Euler(0f, 0f, -28f));
                CreatePart("Sunken Fin R", PrimitiveType.Cube, new Vector3(0.42f, 0.36f, 0.18f), new Vector3(0.12f, 0.62f, 0.1f), palette, Quaternion.Euler(0f, 0f, 28f));
                return;
            }

            if (enemyId.Contains("obsidian"))
            {
                CreatePart("Obsidian Forge Ring", PrimitiveType.Cylinder, new Vector3(0f, 0.7f, 0f), new Vector3(0.78f, 0.035f, 0.78f), palette, Quaternion.Euler(90f, 0f, 0f));
                CreatePart("Obsidian Magma Core", PrimitiveType.Sphere, new Vector3(0f, 0.42f, -0.5f), new Vector3(0.25f, 0.25f, 0.12f), highlight);
                CreatePart("Obsidian Hammer", PrimitiveType.Cube, new Vector3(0.48f, 0.38f, 0f), new Vector3(0.18f, 0.7f, 0.18f), shadow, Quaternion.Euler(0f, 0f, -30f));
                return;
            }

            if (enemyId.Contains("astral"))
            {
                CreatePart("Astral Star Orb", PrimitiveType.Sphere, new Vector3(0f, 1.5f, -0.04f), new Vector3(0.24f, 0.24f, 0.24f), highlight);
                CreatePart("Astral Grand Ring", PrimitiveType.Cylinder, new Vector3(0f, 0.78f, 0f), new Vector3(0.88f, 0.035f, 0.88f), palette, Quaternion.Euler(90f, 0f, 0f));
                CreatePart("Astral Satellite", PrimitiveType.Sphere, new Vector3(0.58f, 1.18f, 0f), new Vector3(0.12f, 0.12f, 0.12f), shadow);
                return;
            }

            if (enemyId.Contains("eclipse"))
            {
                CreatePart("Eclipse Disc", PrimitiveType.Cylinder, new Vector3(0f, 1.22f, 0.02f), new Vector3(0.58f, 0.08f, 0.58f), shadow, Quaternion.Euler(90f, 0f, 0f));
                CreatePart("Eclipse Halo", PrimitiveType.Cylinder, new Vector3(0f, 1.22f, 0f), new Vector3(0.78f, 0.035f, 0.78f), palette, Quaternion.Euler(90f, 0f, 0f));
                CreatePart("Eclipse Core", PrimitiveType.Sphere, new Vector3(0f, 0.48f, -0.48f), new Vector3(0.24f, 0.24f, 0.12f), highlight);
                return;
            }

            if (enemyId.Contains("throne"))
            {
                CreatePart("Throne Void Crown", PrimitiveType.Cylinder, new Vector3(0f, 1.48f, 0f), new Vector3(0.62f, 0.1f, 0.62f), palette);
                CreatePart("Throne Mantle", PrimitiveType.Cube, new Vector3(0f, 0.46f, 0.36f), new Vector3(1.08f, 0.46f, 0.12f), shadow, Quaternion.Euler(10f, 0f, 0f));
                CreatePart("Throne Void Core", PrimitiveType.Sphere, new Vector3(0f, 0.54f, -0.52f), new Vector3(0.28f, 0.28f, 0.14f), highlight);
            }
        }

        private void BuildTierAdornment(EnemyTier tier, Color accent, string enemyId)
        {
            if (tier == EnemyTier.Normal)
            {
                return;
            }

            CreatePart("Tier Crest", PrimitiveType.Cylinder, new Vector3(0f, 1.05f, 0f), tier == EnemyTier.Boss ? new Vector3(0.36f, 0.08f, 0.36f) : new Vector3(0.28f, 0.06f, 0.28f), accent);

            BuildFamilyTierAdornment(enemyId, tier, accent);

            if (tier == EnemyTier.Boss)
            {
                CreatePart("Boss Core", PrimitiveType.Sphere, new Vector3(0f, 1.34f, 0f), new Vector3(0.2f, 0.2f, 0.2f), Color.Lerp(accent, Color.white, 0.3f));
                CreatePart("Boss Horn L", PrimitiveType.Cylinder, new Vector3(-0.3f, 0.98f, 0f), new Vector3(0.1f, 0.3f, 0.1f), accent, Quaternion.Euler(0f, 0f, -28f));
                CreatePart("Boss Horn R", PrimitiveType.Cylinder, new Vector3(0.3f, 0.98f, 0f), new Vector3(0.1f, 0.3f, 0.1f), accent, Quaternion.Euler(0f, 0f, 28f));
            }
        }

        private void BuildFamilyTierAdornment(string enemyId, EnemyTier tier, Color accent)
        {
            var family = enemyId ?? string.Empty;
            if (family.Contains("forest"))
            {
                CreatePart("Forest Tier Thorn L", PrimitiveType.Cylinder, new Vector3(-0.34f, 0.72f, 0.08f), new Vector3(0.07f, tier == EnemyTier.Boss ? 0.42f : 0.26f, 0.07f), new Color(0.16f, 0.72f, 0.24f), Quaternion.Euler(0f, 0f, -28f));
                CreatePart("Forest Tier Thorn R", PrimitiveType.Cylinder, new Vector3(0.34f, 0.72f, 0.08f), new Vector3(0.07f, tier == EnemyTier.Boss ? 0.42f : 0.26f, 0.07f), new Color(0.16f, 0.72f, 0.24f), Quaternion.Euler(0f, 0f, 28f));
                return;
            }

            if (family.Contains("ash") || family.Contains("eclipse"))
            {
                CreatePart("Ember Tier Ring", PrimitiveType.Cylinder, new Vector3(0f, 0.64f, 0f), new Vector3(tier == EnemyTier.Boss ? 0.62f : 0.46f, 0.025f, tier == EnemyTier.Boss ? 0.62f : 0.46f), new Color(1f, 0.24f, 0.06f), Quaternion.Euler(90f, 0f, 0f));
                return;
            }

            if (family.Contains("crystal") || family.Contains("frost"))
            {
                CreatePart("Prism Tier L", PrimitiveType.Cube, new Vector3(-0.44f, 0.54f, -0.12f), new Vector3(0.16f, tier == EnemyTier.Boss ? 0.62f : 0.36f, 0.16f), Color.Lerp(accent, Color.white, 0.3f), Quaternion.Euler(0f, 0f, -18f));
                CreatePart("Prism Tier R", PrimitiveType.Cube, new Vector3(0.44f, 0.54f, -0.12f), new Vector3(0.16f, tier == EnemyTier.Boss ? 0.62f : 0.36f, 0.16f), Color.Lerp(accent, Color.white, 0.3f), Quaternion.Euler(0f, 0f, 18f));
                return;
            }

            if (family.Contains("sunken"))
            {
                CreatePart("Abyss Tier Fin", PrimitiveType.Cube, new Vector3(0f, 0.72f, 0.32f), new Vector3(0.12f, tier == EnemyTier.Boss ? 0.64f : 0.38f, 0.08f), new Color(0.12f, 0.84f, 0.8f), Quaternion.Euler(22f, 0f, 0f));
                return;
            }

            if (family.Contains("obsidian"))
            {
                CreatePart("Forge Tier Guard", PrimitiveType.Cube, new Vector3(0f, 0.48f, -0.4f), new Vector3(tier == EnemyTier.Boss ? 0.78f : 0.56f, 0.18f, 0.08f), new Color(1f, 0.3f, 0.08f));
                return;
            }

            if (family.Contains("astral"))
            {
                CreatePart("Astral Tier Halo", PrimitiveType.Cylinder, new Vector3(0f, 1.28f, 0f), new Vector3(tier == EnemyTier.Boss ? 0.62f : 0.42f, 0.025f, tier == EnemyTier.Boss ? 0.62f : 0.42f), Color.Lerp(accent, Color.white, 0.34f), Quaternion.Euler(90f, 0f, 0f));
                return;
            }

            if (family.Contains("throne"))
            {
                CreatePart("Void Tier Mantle", PrimitiveType.Cube, new Vector3(0f, 0.38f, 0.38f), new Vector3(tier == EnemyTier.Boss ? 0.94f : 0.7f, 0.38f, 0.1f), Color.Lerp(accent, Color.black, 0.3f), Quaternion.Euler(12f, 0f, 0f));
            }
        }

        private void CreateEyes(Vector3 center, Color color)
        {
            CreatePart("Eye L", PrimitiveType.Sphere, center + new Vector3(-0.1f, 0f, 0.02f), new Vector3(0.06f, 0.06f, 0.04f), color);
            CreatePart("Eye R", PrimitiveType.Sphere, center + new Vector3(0.1f, 0f, 0.02f), new Vector3(0.06f, 0.06f, 0.04f), color);
        }

        private void BuildStatusDisplay(string displayName, EnemyTier tier, Color accent)
        {
            statusRoot = new GameObject("Enemy Status");
            statusRoot.transform.SetParent(transform, false);
            statusRoot.transform.localPosition = new Vector3(0f, 1.75f, 0f);

            var labelObject = new GameObject("Enemy Name");
            labelObject.transform.SetParent(statusRoot.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.18f, 0f);
            var label = labelObject.AddComponent<TextMesh>();
            label.text = displayName;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = tier == EnemyTier.Boss ? 34 : 28;
            label.characterSize = tier == EnemyTier.Boss ? 0.045f : 0.038f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = tier == EnemyTier.Normal ? Color.white : accent;

            var background = CreateStatusPart("Health Background", new Vector3(healthBarWidth, 0.075f, 0.035f), new Color(0.03f, 0.04f, 0.05f));
            background.transform.SetParent(statusRoot.transform, false);
            background.transform.localPosition = new Vector3(0f, -0.08f, 0f);

            var fill = CreateStatusPart("Health Fill", new Vector3(healthBarWidth, 0.055f, 0.045f), tier == EnemyTier.Boss ? new Color(1f, 0.38f, 0.12f) : tier == EnemyTier.Elite ? new Color(0.82f, 0.36f, 1f) : new Color(0.25f, 0.9f, 0.35f));
            fill.transform.SetParent(statusRoot.transform, false);
            fill.transform.localPosition = new Vector3(0f, -0.08f, -0.035f);
            healthFill = fill.transform;
        }

        private GameObject CreateStatusPart(string name, Vector3 size, Color color)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.localScale = size;
            RemoveCollider(part);
            part.GetComponent<Renderer>().sharedMaterial = MaterialFor(color);
            return part;
        }

        private GameObject CreatePart(string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color)
        {
            return CreatePart(name, primitive, position, scale, color, Quaternion.identity);
        }

        private GameObject CreatePart(string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color, Quaternion rotation)
        {
            var part = GameObject.CreatePrimitive(primitive);
            part.name = name;
            part.transform.SetParent(visualRoot.transform, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.transform.localScale = scale;
            RemoveCollider(part);
            part.GetComponent<Renderer>().sharedMaterial = MaterialFor(color);
            return part;
        }

        private static void RemoveCollider(GameObject part)
        {
            var collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }
        }

        private static Color AccentFor(EnemyTier tier, Color baseColor)
        {
            if (tier == EnemyTier.Boss)
            {
                return new Color(1f, 0.58f, 0.14f);
            }

            if (tier == EnemyTier.Elite)
            {
                return new Color(0.82f, 0.36f, 1f);
            }

            return Color.Lerp(baseColor, Color.white, 0.42f);
        }

        private static Material MaterialFor(Color color)
        {
            var key = ((Color32)color).r << 24 | ((Color32)color).g << 16 | ((Color32)color).b << 8 | ((Color32)color).a;
            if (Materials.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader) { color = color };
            Materials[key] = material;
            return material;
        }
    }
}
