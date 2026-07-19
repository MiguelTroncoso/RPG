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
        private static readonly int MetallicId = Shader.PropertyToID("_Metallic");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private GameObject visualRoot;
        private GameObject statusRoot;
        private Transform healthFill;
        private Health health;
        private float healthBarWidth = 1.25f;
        private Vector3 visualBaseScale;
        private Coroutine deathRoutine;
        private bool usingOriginalArt;

        // The generated FBX pass is not ready for player-facing use. The beta
        // therefore uses the readable Quaternius silhouettes and keeps the
        // generated pack available only for a later authored-content pass.
        private static bool PreferExperimentalOriginalArt => false;

        public void Initialize(string enemyId, string displayName, EnemyTier tier, Color baseColor, float scale, Health enemyHealth)
        {
            health = enemyHealth;
            if (health != null)
            {
                health.Damaged -= HandleDamaged;
                health.Damaged += HandleDamaged;
            }
            usingOriginalArt = false;
            visualRoot = new GameObject("Enemy Visual");
            visualRoot.transform.SetParent(transform, false);
            visualRoot.transform.localScale = Vector3.one * Mathf.Max(0.7f, scale);
            visualBaseScale = visualRoot.transform.localScale;

            var id = (enemyId ?? string.Empty).ToLowerInvariant();
            var accent = AccentFor(tier, baseColor);

            var builtRealModel = TryBuildRealModel(id, tier, baseColor, accent);
            if (builtRealModel)
            {
                // Imported Quaternius models already have their own silhouette.
                // Do not attach the old primitive zone kit on top of them.
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

            var anchorY = usingOriginalArt ? -0.02f : 0.01f;
            var anchorRadius = tier == EnemyTier.Boss ? 1.22f : tier == EnemyTier.Elite ? 1.02f : 0.82f;
            ArtPresentationUtility.AttachGroundAnchor(visualRoot.transform, anchorY, anchorRadius, accent, tier != EnemyTier.Normal);
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

            GetComponent<AvatarMotionAnimator>()?.PlayDeath();
            deathRoutine = StartCoroutine(DeathAnimation());
        }

        private void HandleDamaged(Health damagedHealth, int amount)
        {
            if (damagedHealth == null || damagedHealth.IsDead)
            {
                return;
            }

            GetComponent<AvatarMotionAnimator>()?.PlayHit();
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Damaged -= HandleDamaged;
            }
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
            var authoredResource = AuthoredMobResourceFor(enemyId, tier);
            var authoredPrefab = PreferExperimentalOriginalArt
                ? Resources.Load<GameObject>(authoredResource)
                : null;
            var model = authoredPrefab != null
                ? Instantiate(authoredPrefab, visualRoot.transform)
                : null;
            usingOriginalArt = authoredPrefab != null;
            Animator modelAnimator = null;

            if (model == null)
            {
                // Keep the authored runtime silhouette as a fallback only for
                // legacy content that predates the exported FBX pack.
                var originalModelRequested = PreferExperimentalOriginalArt
                    && tier == EnemyTier.Normal
                    && enemyId == "valley_creature";
                model = originalModelRequested
                    ? OriginalArtVisualFactory.BuildValleyMob(visualRoot.transform, baseColor, accent)
                    : null;
                usingOriginalArt = model != null;
            }

            var modelResource = authoredPrefab != null ? authoredResource : string.Empty;
            if (model == null)
            {
                modelResource = ModelResourceFor(enemyId, tier);
                var prefab = Resources.Load<GameObject>(modelResource);
                if (prefab == null)
                {
                    return false;
                }

                model = Instantiate(prefab, visualRoot.transform);
            }

            if (model == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(modelResource))
            {
                model.name = authoredPrefab != null ? "Authored Mob 3D Model" : "Enemy 3D Model";
                RemoveImportArtifacts(model);
                model.transform.localPosition = ModelOffsetFor(modelResource);
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one * ModelScaleFor(modelResource, tier);

                ApplyModelArtTreatment(model, enemyId, baseColor, accent, tier);
                if (authoredPrefab != null)
                {
                    ConfigureImportedLodGroup(model);
                }

                foreach (var modelCollider in model.GetComponentsInChildren<Collider>())
                {
                    Destroy(modelCollider);
                }

                modelAnimator = model.GetComponentInChildren<Animator>();
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
            }

            var motion = GetComponent<AvatarMotionAnimator>() ?? gameObject.AddComponent<AvatarMotionAnimator>();
            motion.SetVisualRoot(model.transform, modelAnimator);
            return true;
        }

        private static void ConfigureImportedLodGroup(GameObject model)
        {
            if (model == null)
            {
                return;
            }

            var lod0 = new List<Renderer>();
            var lod1 = new List<Renderer>();
            var lod2 = new List<Renderer>();
            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                var rendererName = renderer.gameObject.name;
                if (rendererName.Contains("_LOD2") || rendererName.Contains(" LOD2"))
                {
                    lod2.Add(renderer);
                }
                else if (rendererName.Contains("_LOD1") || rendererName.Contains(" LOD1"))
                {
                    lod1.Add(renderer);
                }
                else
                {
                    lod0.Add(renderer);
                }
            }

            if (lod0.Count == 0)
            {
                lod0.AddRange(model.GetComponentsInChildren<Renderer>(true));
            }

            if (lod1.Count == 0)
            {
                lod1.AddRange(lod0);
            }

            if (lod2.Count == 0)
            {
                lod2.AddRange(lod1);
            }

            var lodGroup = model.GetComponent<LODGroup>() ?? model.AddComponent<LODGroup>();
            lodGroup.SetLODs(new[]
            {
                new LOD(0.52f, lod0.ToArray()),
                new LOD(0.22f, lod1.ToArray()),
                new LOD(0.08f, lod2.ToArray())
            });
            lodGroup.RecalculateBounds();
        }

        private GameObject BuildValleyAuthoredModel(EnemyTier tier, Color baseColor, Color accent)
        {
            var root = new GameObject(tier == EnemyTier.Boss ? "Valley Relic Guardian" : tier == EnemyTier.Elite ? "Valley Corrupted Wolf Elite" : "Valley Corrupted Wolf");
            root.transform.SetParent(visualRoot.transform, false);

            var fur = tier == EnemyTier.Boss
                ? new Color(0.2f, 0.12f, 0.1f)
                : Color.Lerp(new Color(0.12f, 0.07f, 0.06f), baseColor, 0.3f);
            var furDark = Color.Lerp(fur, Color.black, 0.38f);
            var relicMetal = tier == EnemyTier.Boss
                ? new Color(0.72f, 0.46f, 0.16f)
                : new Color(0.38f, 0.28f, 0.18f);
            var eyeColor = tier == EnemyTier.Boss ? new Color(1f, 0.72f, 0.16f) : new Color(1f, 0.18f, 0.08f);

            var large = tier == EnemyTier.Boss;
            var bodySize = large ? new Vector3(1.18f, 0.82f, 1.5f) : new Vector3(0.82f, 0.62f, 1.14f);
            var bodyPosition = large ? new Vector3(0f, 0.42f, 0f) : new Vector3(0f, 0.26f, 0f);
            RuntimeArtMeshFactory.CreateEllipsoid(root.transform, "Wolf Faceted Body", bodyPosition, bodySize, fur, 10, 5, false, 0.04f, 0.28f);

            var shoulderSize = large ? new Vector3(0.82f, 0.58f, 0.74f) : new Vector3(0.58f, 0.46f, 0.62f);
            RuntimeArtMeshFactory.CreateEllipsoid(root.transform, "Wolf Neck", large ? new Vector3(0f, 0.72f, 0.5f) : new Vector3(0f, 0.53f, 0.38f), shoulderSize, furDark, 9, 4, false, 0.04f, 0.28f);

            var headPosition = large ? new Vector3(0f, 1.02f, 0.88f) : new Vector3(0f, 0.7f, 0.62f);
            var headSize = large ? new Vector3(0.74f, 0.62f, 0.76f) : new Vector3(0.56f, 0.46f, 0.58f);
            RuntimeArtMeshFactory.CreateEllipsoid(root.transform, "Wolf Faceted Head", headPosition, headSize, fur, 9, 4, false, 0.04f, 0.28f);

            var muzzlePosition = large ? new Vector3(0f, 0.84f, 1.2f) : new Vector3(0f, 0.55f, 0.93f);
            var muzzleSize = large ? new Vector3(0.4f, 0.26f, 0.42f) : new Vector3(0.28f, 0.2f, 0.3f);
            RuntimeArtMeshFactory.CreateEllipsoid(root.transform, "Wolf Muzzle", muzzlePosition, muzzleSize, furDark, 8, 3, false, 0.04f, 0.25f);

            var earY = large ? 1.43f : 1.02f;
            var earZ = large ? 0.76f : 0.56f;
            var earHeight = large ? 0.58f : 0.38f;
            RuntimeArtMeshFactory.CreateCone(root.transform, "Wolf Ear L", new Vector3(large ? -0.3f : -0.22f, earY, earZ), large ? 0.17f : 0.13f, 0.015f, earHeight, relicMetal, 7, 15f, false, 0.28f, 0.42f);
            RuntimeArtMeshFactory.CreateCone(root.transform, "Wolf Ear R", new Vector3(large ? 0.3f : 0.22f, earY, earZ), large ? 0.17f : 0.13f, 0.015f, earHeight, relicMetal, 7, 42f, false, 0.28f, 0.42f);

            var legHeight = large ? 0.8f : 0.58f;
            var legY = large ? -0.02f : -0.12f;
            var legRadius = large ? 0.16f : 0.11f;
            var legTopRadius = large ? 0.2f : 0.14f;
            var legX = large ? 0.42f : 0.27f;
            var legZ = large ? 0.52f : 0.35f;
            CreateValleyLeg(root.transform, "Wolf Leg Front L", new Vector3(-legX, legY, legZ), legRadius, legTopRadius, legHeight, furDark);
            CreateValleyLeg(root.transform, "Wolf Leg Front R", new Vector3(legX, legY, legZ), legRadius, legTopRadius, legHeight, furDark);
            CreateValleyLeg(root.transform, "Wolf Leg Back L", new Vector3(-legX, legY, -legZ), legRadius, legTopRadius, legHeight, furDark);
            CreateValleyLeg(root.transform, "Wolf Leg Back R", new Vector3(legX, legY, -legZ), legRadius, legTopRadius, legHeight, furDark);

            var pawY = large ? -0.4f : -0.42f;
            var pawSize = large ? new Vector3(0.32f, 0.16f, 0.38f) : new Vector3(0.22f, 0.12f, 0.26f);
            RuntimeArtMeshFactory.CreateEllipsoid(root.transform, "Wolf Paw Front L", new Vector3(-legX, pawY, legZ + 0.04f), pawSize, furDark, 8, 3, false, 0.02f, 0.24f);
            RuntimeArtMeshFactory.CreateEllipsoid(root.transform, "Wolf Paw Front R", new Vector3(legX, pawY, legZ + 0.04f), pawSize, furDark, 8, 3, false, 0.02f, 0.24f);
            RuntimeArtMeshFactory.CreateEllipsoid(root.transform, "Wolf Paw Back L", new Vector3(-legX, pawY, -legZ - 0.02f), pawSize, furDark, 8, 3, false, 0.02f, 0.24f);
            RuntimeArtMeshFactory.CreateEllipsoid(root.transform, "Wolf Paw Back R", new Vector3(legX, pawY, -legZ - 0.02f), pawSize, furDark, 8, 3, false, 0.02f, 0.24f);

            var eyeY = large ? 1.04f : 0.72f;
            var eyeZ = large ? 1.25f : 0.9f;
            var eyeSize = large ? new Vector3(0.09f, 0.08f, 0.05f) : new Vector3(0.065f, 0.06f, 0.04f);
            RuntimeArtMeshFactory.CreateEllipsoid(root.transform, "Wolf Eye L", new Vector3(large ? -0.14f : -0.1f, eyeY, eyeZ), eyeSize, eyeColor, 7, 3, true, 0.05f, 0.36f);
            RuntimeArtMeshFactory.CreateEllipsoid(root.transform, "Wolf Eye R", new Vector3(large ? 0.14f : 0.1f, eyeY, eyeZ), eyeSize, eyeColor, 7, 3, true, 0.05f, 0.36f);

            var tailPosition = large ? new Vector3(0f, 0.62f, -0.75f) : new Vector3(0f, 0.42f, -0.58f);
            RuntimeArtMeshFactory.CreateCone(root.transform, "Wolf Relic Tail", tailPosition, large ? 0.2f : 0.14f, 0.035f, large ? 0.95f : 0.72f, furDark, 8, 0f, false, 0.04f, 0.25f).transform.localRotation = Quaternion.Euler(-38f, 0f, 0f);

            var platePosition = large ? new Vector3(0f, 0.55f, 1.18f) : new Vector3(0f, 0.4f, 0.78f);
            var plateSize = large ? new Vector3(0.92f, 0.42f, 0.12f) : new Vector3(0.56f, 0.26f, 0.08f);
            RuntimeArtMeshFactory.CreateEllipsoid(root.transform, "Wolf Relic Chest Plate", platePosition, plateSize, relicMetal, 8, 3, false, 0.58f, 0.46f);

            if (tier == EnemyTier.Elite || tier == EnemyTier.Boss)
            {
                var guardSize = large ? new Vector3(1.04f, 0.24f, 0.5f) : new Vector3(0.68f, 0.18f, 0.34f);
                RuntimeArtMeshFactory.CreateEllipsoid(root.transform, "Wolf Elite Mantle", large ? new Vector3(0f, 0.78f, 0.34f) : new Vector3(0f, 0.55f, 0.24f), guardSize, accent, 8, 3, false, 0.38f, 0.44f);
                RuntimeArtMeshFactory.CreateCone(root.transform, "Wolf Relic Spike L", new Vector3(large ? -0.48f : -0.34f, large ? 0.82f : 0.58f, 0.18f), large ? 0.14f : 0.1f, 0.01f, large ? 0.5f : 0.34f, accent, 6, 22f, true, 0.16f, 0.42f);
                RuntimeArtMeshFactory.CreateCone(root.transform, "Wolf Relic Spike R", new Vector3(large ? 0.48f : 0.34f, large ? 0.82f : 0.58f, 0.18f), large ? 0.14f : 0.1f, 0.01f, large ? 0.5f : 0.34f, accent, 6, 52f, true, 0.16f, 0.42f);
            }

            if (tier == EnemyTier.Boss)
            {
                RuntimeArtMeshFactory.CreateCone(root.transform, "Guardian Relic Crown", new Vector3(0f, 1.65f, 0.74f), 0.22f, 0.02f, 0.56f, accent, 8, 22f, true, 0.3f, 0.46f);
                RuntimeArtMeshFactory.CreateEllipsoid(root.transform, "Guardian Relic Core", new Vector3(0f, 0.74f, 1.26f), new Vector3(0.3f, 0.3f, 0.1f), eyeColor, 8, 3, true, 0.18f, 0.46f);
            }

            return root;
        }

        private static void CreateValleyLeg(Transform parent, string name, Vector3 position, float bottomRadius, float topRadius, float height, Color color)
        {
            RuntimeArtMeshFactory.CreateCone(parent, name, position, bottomRadius, topRadius, height, color, 7, 18f, false, 0.03f, 0.24f);
        }

        private static void ApplyModelArtTreatment(GameObject model, string enemyId, Color baseColor, Color accent, EnemyTier tier)
        {
            var block = new MaterialPropertyBlock();
            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                renderer.GetPropertyBlock(block);
                // Preserve the authored monster atlas. Tinting every renderer
                // white was flattening the creatures into pale metal objects.
                block.SetFloat(MetallicId, tier == EnemyTier.Boss ? 0.18f : 0.04f);
                block.SetFloat(SmoothnessId, tier == EnemyTier.Boss ? 0.42f : 0.28f);
                block.SetColor(EmissionColorId, Color.Lerp(Color.black, accent, tier == EnemyTier.Boss ? 0.12f : 0.035f));
                renderer.SetPropertyBlock(block);

                renderer.receiveShadows = true;
                if (renderer is SkinnedMeshRenderer skinned)
                {
                    skinned.quality = SkinQuality.Auto;
                    skinned.updateWhenOffscreen = false;
                }
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
            var scale = enemyId.StartsWith("originalart/mobs/") ? 0.82f : 0.85f;
            if (enemyId.Contains("tribal"))
            {
                scale = 0.92f;
            }
            else if (enemyId.Contains("glub") || enemyId.Contains("goleling"))
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

        private static string AuthoredMobResourceFor(string enemyId, EnemyTier tier)
        {
            var zone = ZoneKeyFor(enemyId);
            if (string.IsNullOrWhiteSpace(zone))
            {
                return string.Empty;
            }

            return $"OriginalArt/Mobs/{zone}_{tier.ToString().ToLowerInvariant()}";
        }

        private static string AuthoredMobControllerResourceFor(string enemyId, EnemyTier tier)
        {
            var zone = ZoneKeyFor(enemyId);
            if (string.IsNullOrWhiteSpace(zone))
            {
                return string.Empty;
            }

            return $"OriginalArt/Mobs/Controllers/{zone}_{tier.ToString().ToLowerInvariant()}";
        }

        private static string ZoneKeyFor(string enemyId)
        {
            enemyId = (enemyId ?? string.Empty).ToLowerInvariant();
            if (enemyId.Contains("valley")) return "valley";
            if (enemyId.Contains("forest")) return "forest";
            if (enemyId.Contains("ash")) return "ash";
            if (enemyId.Contains("crystal")) return "crystal";
            if (enemyId.Contains("frost")) return "frost";
            if (enemyId.Contains("sunken")) return "sunken";
            if (enemyId.Contains("obsidian")) return "obsidian";
            if (enemyId.Contains("astral")) return "astral";
            if (enemyId.Contains("eclipse")) return "eclipse";
            if (enemyId.Contains("throne")) return "throne";
            return string.Empty;
        }

        private static string ModelResourceFor(string enemyId, EnemyTier tier)
        {
            if (enemyId.Contains("valley"))
            {
                return tier == EnemyTier.Boss ? MonsterResource("Orc_Skull") : tier == EnemyTier.Elite ? MonsterResource("Orc") : MonsterResource("Tribal");
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
            var authoredController = AuthoredMobControllerResourceFor(enemyId, tier);
            if (!string.IsNullOrWhiteSpace(authoredController)
                && Resources.Load<RuntimeAnimatorController>(authoredController) != null)
            {
                return authoredController;
            }

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
                if (tier == EnemyTier.Normal)
                {
                    if (!usingOriginalArt)
                    {
                        CreatePart("Relic Scar", PrimitiveType.Cube, new Vector3(0f, 0.34f, -0.46f), new Vector3(0.24f, 0.06f, 0.05f), new Color(1f, 0.3f, 0.12f), Quaternion.Euler(0f, 0f, -24f));
                    }
                    CreatePart("Relic Shard", PrimitiveType.Cylinder, new Vector3(0.26f, 0.64f, 0.08f), new Vector3(0.06f, 0.24f, 0.06f), accent, Quaternion.Euler(0f, 0f, -22f));
                }
                else if (tier == EnemyTier.Elite)
                {
                    CreatePart("Relic Shoulder L", PrimitiveType.Cube, new Vector3(-0.48f, 0.38f, 0f), new Vector3(0.24f, 0.24f, 0.44f), accent, Quaternion.Euler(0f, 0f, -18f));
                    CreatePart("Relic Shoulder R", PrimitiveType.Cube, new Vector3(0.48f, 0.38f, 0f), new Vector3(0.24f, 0.24f, 0.44f), accent, Quaternion.Euler(0f, 0f, 18f));
                    CreatePart("Relic Elite Sigil", PrimitiveType.Sphere, new Vector3(0f, 0.42f, -0.5f), new Vector3(0.14f, 0.14f, 0.08f), new Color(1f, 0.42f, 0.14f));
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
            label.fontSize = tier == EnemyTier.Boss ? 28 : 22;
            label.characterSize = tier == EnemyTier.Boss ? 0.036f : 0.028f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontStyle = FontStyle.Bold;
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
            part.GetComponent<Renderer>().sharedMaterial = VisualMaterialUtility.Create(color, VisualMaterialUtility.ShouldGlow(name), 0.06f, 0.28f);
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

        private static void RemoveImportArtifacts(GameObject model)
        {
            if (model == null)
            {
                return;
            }

            foreach (var child in model.GetComponentsInChildren<Transform>(true))
            {
                if (child == model.transform)
                {
                    continue;
                }

                var name = child.name;
                if (name == "Cube" || name.StartsWith("Cube."))
                {
                    Object.Destroy(child.gameObject);
                }
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
            var material = VisualMaterialUtility.Create(color, false, 0.06f, 0.28f);
            Materials[key] = material;
            return material;
        }
    }
}
