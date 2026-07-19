using System;
using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Deterministic, low-cost decoration for every level band. It gives each
    // zone a readable silhouette while the final art pass is still pending.
    public static class ZoneEnvironmentBuilder
    {
        private static readonly Dictionary<int, Material> Materials = new Dictionary<int, Material>();

        public static void Build(ZoneDefinition zone)
        {
            if (zone == null)
            {
                return;
            }

            var root = new GameObject($"Environment - {zone.DisplayName}");
            var random = new System.Random(StableSeed(zone.ZoneId));
            var center = zone.GroundCenter;

            var decorationCount = Application.platform == RuntimePlatform.Android ? 8 : 12;
            for (var i = 0; i < decorationCount; i++)
            {
                var angle = (float)(Math.PI * 2d * i / decorationCount) + (float)random.NextDouble() * 0.34f;
                var radius = 10f + (float)random.NextDouble() * 14f;
                var position = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                BuildDecoration(root.transform, zone, i, position, random);
            }

            BuildNavigation(root.transform, zone);
            BuildEntryMarker(root.transform, zone);
            BuildObstacleField(root.transform, zone);
            BuildSafeZone(root.transform, zone);
            BuildCombatAreaMarker(root.transform, zone);
            BuildGroundAccents(root.transform, zone, random);
            BuildZoneSignature(root.transform, zone);
            BuildBiomeFrame(root.transform, zone);
            if ((zone.ZoneId ?? string.Empty).ToLowerInvariant().Contains("valley"))
            {
                BuildValleyArtPass(root.transform, zone, random);
                BuildValleyCamp(root.transform, zone);
            }
        }

        // Zone 1 gets a more authored silhouette pass while the final
        // production environment meshes are being created in Blender. These
        // meshes are intentionally small and material-light for Android.
        private static void BuildValleyArtPass(Transform parent, ZoneDefinition zone, System.Random random)
        {
            var center = zone.GroundCenter;
            var forestColor = new Color(0.11f, 0.24f, 0.16f);
            var barkColor = new Color(0.18f, 0.1f, 0.055f);
            var relicStone = new Color(0.24f, 0.28f, 0.31f);
            var relicTrim = new Color(0.52f, 0.36f, 0.18f);
            var spirit = new Color(0.22f, 0.78f, 0.7f);

            var pinePositions = new[]
            {
                center + new Vector3(-25f, 0f, -1f),
                center + new Vector3(24f, 0f, 3f),
                center + new Vector3(-27f, 0f, 15f),
                center + new Vector3(27f, 0f, 24f),
                center + new Vector3(11f, 0f, 30f),
                center + new Vector3(-10f, 0f, 31f)
            };

            for (var i = 0; i < pinePositions.Length; i++)
            {
                var scale = 0.82f + (float)random.NextDouble() * 0.3f;
                BuildValleyPine(parent, pinePositions[i], scale, forestColor, barkColor, i % 3 == 0);
            }

            BuildRelicMonolith(parent, center + new Vector3(-8f, 0f, 9f), 1.05f, relicStone, relicTrim, spirit);
            BuildRelicMonolith(parent, center + new Vector3(8f, 0f, 11f), 0.8f, relicStone, relicTrim, spirit);
            BuildRelicRuin(parent, center + new Vector3(-18f, 0f, 7f), relicStone, relicTrim);
            BuildRelicRuin(parent, center + new Vector3(19f, 0f, 12f), relicStone, relicTrim);
        }

        private static void BuildValleyPine(Transform parent, Vector3 position, float scale, Color foliage, Color bark, bool spiritTree)
        {
            CreateCone(parent, "Valley Pine Trunk", position + Vector3.up * (0.7f * scale), 0.22f * scale, 0.14f * scale, 1.4f * scale, bark);
            CreateCone(parent, "Valley Pine Lower Crown", position + Vector3.up * (1.25f * scale), 0.86f * scale, 0.18f * scale, 1.25f * scale, foliage);
            CreateCone(parent, "Valley Pine Upper Crown", position + Vector3.up * (2.05f * scale), 0.64f * scale, 0.08f * scale, 1.1f * scale, Color.Lerp(foliage, Color.black, 0.12f));
            if (spiritTree)
            {
                CreateCone(parent, "Valley Pine Spirit Tip", position + Vector3.up * (2.82f * scale), 0.18f * scale, 0.02f, 0.42f * scale, new Color(0.24f, 0.72f, 0.62f));
            }
        }

        private static void BuildRelicMonolith(Transform parent, Vector3 position, float scale, Color stone, Color trim, Color spirit)
        {
            CreateCone(parent, "Relic Monolith", position + Vector3.up * (1.25f * scale), 0.62f * scale, 0.34f * scale, 2.5f * scale, stone, 7, 0.12f);
            CreateCone(parent, "Relic Monolith Crown", position + Vector3.up * (2.75f * scale), 0.43f * scale, 0.05f, 0.4f * scale, trim, 6, -0.08f);
            CreatePart(parent, "Relic Monolith Rune", PrimitiveType.Cube, position + new Vector3(0f, 1.55f * scale, -0.38f * scale), new Vector3(0.12f, 0.56f, 0.035f) * scale, spirit, Quaternion.Euler(0f, 0f, 18f));
        }

        private static void BuildRelicRuin(Transform parent, Vector3 position, Color stone, Color trim)
        {
            CreateCone(parent, "Broken Relic Pillar", position + Vector3.up * 0.82f, 0.44f, 0.25f, 1.64f, stone, 7, 0.2f);
            CreateCone(parent, "Broken Relic Cap", position + Vector3.up * 1.7f, 0.46f, 0.18f, 0.28f, trim, 6, 0.08f);
            var shard = CreateCone(parent, "Fallen Relic Shard", position + new Vector3(0.72f, 0.18f, 0.35f), 0.2f, 0.03f, 0.75f, stone, 6, 0.18f);
            shard.transform.rotation = Quaternion.Euler(22f, 28f, -38f);
        }

        private static void BuildNavigation(Transform parent, ZoneDefinition zone)
        {
            var pathStart = zone.SignPosition + Vector3.up * 0.04f;
            var pathEnd = zone.NormalAreaCenter + Vector3.up * 0.04f;
            var direction = pathEnd - pathStart;
            direction.y = 0f;
            var length = Mathf.Max(4f, direction.magnitude);
            var pathColor = Color.Lerp(zone.GroundColor, new Color(0.72f, 0.62f, 0.42f), 0.34f);
            CreatePart(parent, "Main Zone Path", PrimitiveType.Cube, (pathStart + pathEnd) * 0.5f, new Vector3(3.2f, 0.05f, length * 0.5f), pathColor, Quaternion.LookRotation(direction.normalized, Vector3.up));

            BuildLandmark(parent, "Elite Landmark", zone.EliteAreaCenter, new Color(0.72f, 0.32f, 1f), 2.2f);
            BuildLandmark(parent, "Boss Landmark", zone.BossPosition, new Color(1f, 0.36f, 0.12f), 2.8f);
            CreatePointOfInterest(parent, "Elite Landmark", zone.EliteAreaCenter, Localization.Tr("poi.elite", zone.DisplayName), ZonePointOfInterestType.Elite, RewardFor(zone, ZonePointOfInterestType.Elite), ClaimIdFor(zone, ZonePointOfInterestType.Elite));
            CreatePointOfInterest(parent, "Boss Landmark", zone.BossPosition, Localization.Tr("poi.boss", zone.DisplayName), ZonePointOfInterestType.Boss, RewardFor(zone, ZonePointOfInterestType.Boss), ClaimIdFor(zone, ZonePointOfInterestType.Boss));

            var center = zone.GroundCenter;
            var edge = 29f;
            BuildBoundaryMarker(parent, center + new Vector3(-edge, 0f, -edge), zone.GroundColor);
            BuildBoundaryMarker(parent, center + new Vector3(edge, 0f, -edge), zone.GroundColor);
            BuildBoundaryMarker(parent, center + new Vector3(-edge, 0f, edge), zone.GroundColor);
            BuildBoundaryMarker(parent, center + new Vector3(edge, 0f, edge), zone.GroundColor);
        }

        private static void BuildObstacleField(Transform parent, ZoneDefinition zone)
        {
            var center = zone.GroundCenter;
            var obstacleColor = Color.Lerp(zone.GroundColor, Color.black, 0.28f);
            var positions = new[]
            {
                center + new Vector3(-22f, 0f, -12f),
                center + new Vector3(22f, 0f, -12f),
                center + new Vector3(-24f, 0f, 20f),
                center + new Vector3(24f, 0f, 20f)
            };

            for (var i = 0; i < positions.Length; i++)
            {
                var size = new Vector3(1.8f + (i % 2) * 0.6f, 0.8f + (i % 3) * 0.24f, 1.5f + (i % 2) * 0.5f);
                CreateSolidPart(parent, "Solid Zone Obstacle", PrimitiveType.Cube, positions[i] + Vector3.up * size.y * 0.5f, size, obstacleColor, Quaternion.Euler(0f, i * 17f, 0f));
            }
        }

        private static void BuildSafeZone(Transform parent, ZoneDefinition zone)
        {
            if (!zone.HasSafeZone || zone.SafeZoneRadius <= 0f)
            {
                return;
            }

            var center = zone.SafeZoneCenter + Vector3.up * 0.035f;
            var markerColor = new Color(0.18f, 0.72f, 0.5f);
            var ringColor = Color.Lerp(zone.GroundColor, markerColor, 0.58f);
            CreateRing(parent, "Safe Commerce Zone Ring", center, zone.SafeZoneRadius - 0.18f, zone.SafeZoneRadius, 0.035f, ringColor);

            var postRadius = zone.SafeZoneRadius * 0.82f;
            for (var i = 0; i < 4; i++)
            {
                var angle = i * 90f * Mathf.Deg2Rad;
                var postPosition = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * postRadius;
                CreateCone(parent, "Safe Zone Waystone", postPosition, 0.16f, 0.06f, 0.85f, Color.Lerp(markerColor, Color.black, 0.18f), 6, 18f);
                CreatePart(parent, "Safe Zone Waystone Light", PrimitiveType.Sphere, postPosition + Vector3.up * 0.98f, new Vector3(0.18f, 0.18f, 0.18f), markerColor);
            }
            CreatePointOfInterest(parent, "Safe Commerce Zone", zone.SafeZoneCenter, Localization.Tr("poi.safe", zone.DisplayName), ZonePointOfInterestType.SafeCommerce, RewardFor(zone, ZonePointOfInterestType.SafeCommerce), ClaimIdFor(zone, ZonePointOfInterestType.SafeCommerce));
        }

        private static void BuildCombatAreaMarker(Transform parent, ZoneDefinition zone)
        {
            if (zone == null || zone.NormalAreaRadius <= 0f)
            {
                return;
            }

            var center = new Vector3(zone.NormalAreaCenter.x, 0.028f, zone.NormalAreaCenter.z);
            var areaColor = new Color(0.72f, 0.28f, 0.18f);
            CreateRing(parent, "Combat Area Boundary", center, zone.NormalAreaRadius - 0.14f, zone.NormalAreaRadius + 0.06f, 0.04f,
                Color.Lerp(zone.GroundColor, areaColor, 0.6f));

            var markerRadius = zone.NormalAreaRadius + 0.7f;
            for (var i = 0; i < 4; i++)
            {
                var angle = i * 90f * Mathf.Deg2Rad;
                var markerPosition = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * markerRadius;
                CreateCone(parent, "Combat Area Marker Post", markerPosition, 0.15f, 0.04f, 0.78f, Color.Lerp(areaColor, Color.black, 0.18f), 6, 18f);
                CreatePart(parent, "Combat Area Marker Cloth", PrimitiveType.Cube, markerPosition + Vector3.up * 0.74f,
                    new Vector3(0.42f, 0.24f, 0.04f), areaColor, Quaternion.Euler(0f, i * 90f, 0f));
            }
        }

        private static void BuildGroundAccents(Transform parent, ZoneDefinition zone, System.Random random)
        {
            var accent = AccentFor(zone);
            var center = zone.GroundCenter;
            var count = Application.platform == RuntimePlatform.Android ? 5 : 8;
            for (var i = 0; i < count; i++)
            {
                var angle = (float)(Math.PI * 2d * i / count) + (float)random.NextDouble() * 0.5f;
                var radius = 5f + (float)random.NextDouble() * 20f;
                var position = center + new Vector3(Mathf.Cos(angle) * radius, 0.025f, Mathf.Sin(angle) * radius);
                var size = 0.45f + (float)random.NextDouble() * 0.45f;
                if ((zone.ZoneId ?? string.Empty).ToLowerInvariant().Contains("valley"))
                {
                    CreateRing(parent, "Zone Accent Rune", position, size * 0.28f, size * 0.5f, 0.025f, Color.Lerp(zone.GroundColor, accent, 0.62f), 7);
                }
                else
                {
                    CreatePart(parent, "Zone Accent Rune", PrimitiveType.Cylinder, position, new Vector3(size, 0.025f, size), Color.Lerp(zone.GroundColor, accent, 0.6f), Quaternion.Euler(0f, i * 21f, 0f));
                }

                if (i % 2 == 0)
                {
                    CreateCone(parent, "Zone Accent Shard", position + Vector3.up * 0.03f, 0.08f, 0.015f, 0.42f, accent, 6, i * 33f);
                }
            }
        }

        private static void BuildZoneSignature(Transform parent, ZoneDefinition zone)
        {
            var id = (zone.ZoneId ?? string.Empty).ToLowerInvariant();
            var center = zone.GroundCenter + new Vector3(0f, 0f, 8f);
            var accent = AccentFor(zone);

            if (id.Contains("valley"))
            {
                CreatePart(parent, "Valley Relic Pillar", PrimitiveType.Cylinder, center + Vector3.up * 1.4f, new Vector3(0.42f, 1.4f, 0.42f), new Color(0.74f, 0.52f, 0.2f));
                CreatePart(parent, "Valley Relic Orb", PrimitiveType.Sphere, center + Vector3.up * 3.1f, new Vector3(0.42f, 0.42f, 0.42f), accent);
                return;
            }

            if (id.Contains("forest"))
            {
                CreatePart(parent, "Forest Elder Root", PrimitiveType.Cylinder, center + Vector3.up * 1.8f, new Vector3(0.5f, 1.8f, 0.5f), new Color(0.2f, 0.36f, 0.14f));
                CreatePart(parent, "Forest Elder Canopy", PrimitiveType.Sphere, center + Vector3.up * 3.6f, new Vector3(2.1f, 1.35f, 2.1f), accent);
                CreatePart(parent, "Forest Elder Rune", PrimitiveType.Sphere, center + new Vector3(0f, 2.1f, -0.56f), new Vector3(0.28f, 0.28f, 0.16f), Color.Lerp(accent, Color.white, 0.35f));
                return;
            }

            if (id.Contains("ash"))
            {
                CreatePart(parent, "Ash Obelisk", PrimitiveType.Cube, center + Vector3.up * 1.7f, new Vector3(0.72f, 3.4f, 0.72f), new Color(0.18f, 0.14f, 0.14f), Quaternion.Euler(0f, 18f, 0f));
                CreatePart(parent, "Ash Ember", PrimitiveType.Sphere, center + Vector3.up * 3.5f, new Vector3(0.42f, 0.42f, 0.42f), accent);
                return;
            }

            if (id.Contains("crystal"))
            {
                BuildCrystalCluster(parent, center + new Vector3(-1.2f, 0f, 0f), zone.GroundColor, 1.8f);
                BuildCrystalCluster(parent, center + new Vector3(1.2f, 0f, 0.2f), accent, 1.35f);
                return;
            }

            if (id.Contains("frost"))
            {
                CreatePart(parent, "Frost Gate L", PrimitiveType.Cylinder, center + new Vector3(-1.7f, 1.4f, 0f), new Vector3(0.36f, 1.4f, 0.36f), accent, Quaternion.Euler(0f, 0f, -14f));
                CreatePart(parent, "Frost Gate R", PrimitiveType.Cylinder, center + new Vector3(1.7f, 1.4f, 0f), new Vector3(0.36f, 1.4f, 0.36f), accent, Quaternion.Euler(0f, 0f, 14f));
                CreatePart(parent, "Frost Gate Crown", PrimitiveType.Cube, center + Vector3.up * 2.8f, new Vector3(3.5f, 0.3f, 0.34f), Color.Lerp(accent, Color.white, 0.25f));
                return;
            }

            if (id.Contains("sunken"))
            {
                CreatePart(parent, "Sunken Broken Arch L", PrimitiveType.Cube, center + new Vector3(-1.5f, 1.2f, 0f), new Vector3(0.5f, 2.4f, 0.5f), accent, Quaternion.Euler(0f, 0f, -12f));
                CreatePart(parent, "Sunken Broken Arch R", PrimitiveType.Cube, center + new Vector3(1.5f, 0.8f, 0f), new Vector3(0.5f, 1.6f, 0.5f), accent, Quaternion.Euler(0f, 0f, 16f));
                CreatePart(parent, "Sunken Pearl", PrimitiveType.Sphere, center + Vector3.up * 2.7f, new Vector3(0.5f, 0.5f, 0.5f), Color.Lerp(accent, Color.white, 0.32f));
                return;
            }

            if (id.Contains("obsidian"))
            {
                CreatePart(parent, "Forge Anvil", PrimitiveType.Cube, center + Vector3.up * 0.7f, new Vector3(1.5f, 1.4f, 0.8f), new Color(0.12f, 0.08f, 0.1f));
                CreatePart(parent, "Forge Flame", PrimitiveType.Sphere, center + Vector3.up * 1.8f, new Vector3(0.5f, 0.7f, 0.5f), accent);
                CreatePart(parent, "Forge Ring", PrimitiveType.Cylinder, center + Vector3.up * 1.1f, new Vector3(2.4f, 0.04f, 2.4f), accent, Quaternion.Euler(90f, 0f, 0f));
                return;
            }

            if (id.Contains("astral"))
            {
                CreatePart(parent, "Astral Spire", PrimitiveType.Cylinder, center + Vector3.up * 1.8f, new Vector3(0.28f, 1.8f, 0.28f), accent);
                CreatePart(parent, "Astral Planet", PrimitiveType.Sphere, center + Vector3.up * 3.5f, new Vector3(0.7f, 0.7f, 0.7f), Color.Lerp(accent, Color.white, 0.3f));
                CreatePart(parent, "Astral Orbit", PrimitiveType.Cylinder, center + Vector3.up * 3.5f, new Vector3(1.9f, 0.035f, 1.9f), accent, Quaternion.Euler(72f, 0f, 18f));
                return;
            }

            if (id.Contains("eclipse"))
            {
                CreatePart(parent, "Eclipse Altar", PrimitiveType.Cylinder, center + Vector3.up * 0.25f, new Vector3(2.4f, 0.5f, 2.4f), new Color(0.08f, 0.06f, 0.12f));
                CreatePart(parent, "Eclipse Sun", PrimitiveType.Sphere, center + Vector3.up * 2.5f, new Vector3(1.1f, 1.1f, 1.1f), new Color(0.08f, 0.02f, 0.12f));
                CreatePart(parent, "Eclipse Halo", PrimitiveType.Cylinder, center + Vector3.up * 2.5f, new Vector3(1.7f, 0.05f, 1.7f), accent, Quaternion.Euler(90f, 0f, 0f));
                return;
            }

            CreatePart(parent, "Throne Shard L", PrimitiveType.Cube, center + new Vector3(-1.2f, 1.4f, 0f), new Vector3(0.5f, 2.8f, 0.5f), accent, Quaternion.Euler(0f, 0f, -12f));
            CreatePart(parent, "Throne Shard R", PrimitiveType.Cube, center + new Vector3(1.2f, 1.8f, 0.2f), new Vector3(0.5f, 3.6f, 0.5f), accent, Quaternion.Euler(0f, 0f, 14f));
            CreatePart(parent, "Throne Crown", PrimitiveType.Cube, center + Vector3.up * 3.5f, new Vector3(3.2f, 0.3f, 0.5f), Color.Lerp(accent, Color.white, 0.2f));
        }

        private static void BuildBiomeFrame(Transform parent, ZoneDefinition zone)
        {
            var accent = AccentFor(zone);
            var center = zone.GroundCenter;
            var edge = 33f;
            for (var i = 0; i < 12; i++)
            {
                var angle = i / 12f * Mathf.PI * 2f;
                var position = center + new Vector3(Mathf.Cos(angle) * edge, 0f, Mathf.Sin(angle) * edge);
                var scale = 1.2f + (i % 3) * 0.34f;
                if ((zone.ZoneId ?? string.Empty).ToLowerInvariant().Contains("forest") || (zone.ZoneId ?? string.Empty).ToLowerInvariant().Contains("valley"))
                {
                    BuildTree(parent, position, zone.GroundColor, i % 4 == 0);
                }
                else
                {
                    CreatePart(parent, "Biome Border Rock", PrimitiveType.Sphere, position + Vector3.up * scale * 0.45f,
                        new Vector3(scale * 1.1f, scale * 0.9f, scale), Color.Lerp(zone.GroundColor, Color.black, 0.2f));
                    if (i % 3 == 0)
                    {
                        CreatePart(parent, "Biome Border Rune", PrimitiveType.Cylinder, position + Vector3.up * (scale + 0.08f),
                            new Vector3(0.16f, 0.34f, 0.16f), accent);
                    }
                }
            }
        }

        private static void BuildValleyCamp(Transform parent, ZoneDefinition zone)
        {
            var center = zone.SafeZoneCenter;
            var wood = new Color(0.28f, 0.16f, 0.07f);
            var cloth = new Color(0.12f, 0.42f, 0.32f);
            var stone = new Color(0.18f, 0.22f, 0.25f);
            var positions = new[]
            {
                center + new Vector3(-5.2f, 0f, 2.2f),
                center + new Vector3(5.2f, 0f, 2.2f)
            };

            for (var i = 0; i < positions.Length; i++)
            {
                var stall = positions[i];
                CreatePart(parent, "Camp Stall Counter", PrimitiveType.Cube, stall + new Vector3(0f, 0.62f, 0f), new Vector3(2.35f, 0.72f, 0.7f), wood);
                CreateCone(parent, "Camp Stall Awning", stall + Vector3.up * 1.55f, 1.62f, 0.08f, 0.82f, cloth, 6, i == 0 ? 30f : -30f);
                CreateCone(parent, "Camp Stall Post L", stall + new Vector3(-1.18f, 0f, 0.56f), 0.1f, 0.055f, 1.65f, wood, 6, 12f);
                CreateCone(parent, "Camp Stall Post R", stall + new Vector3(1.18f, 0f, 0.56f), 0.1f, 0.055f, 1.65f, wood, 6, 12f);
                CreatePart(parent, "Camp Stall Banner", PrimitiveType.Cube, stall + new Vector3(0f, 1.72f, 0.84f), new Vector3(0.66f, 0.62f, 0.035f), Color.Lerp(cloth, Color.white, 0.12f), Quaternion.Euler(0f, 0f, i == 0 ? 3f : -3f));
                CreatePart(parent, "Camp Crate", PrimitiveType.Cube, stall + new Vector3(i == 0 ? -1.55f : 1.55f, 0.32f, -0.54f), new Vector3(0.58f, 0.58f, 0.58f), Color.Lerp(wood, Color.white, 0.08f), Quaternion.Euler(0f, i * 17f, 0f));
            }

            var firePosition = center + new Vector3(0f, 0.04f, -2.6f);
            CreateRing(parent, "Camp Fire Ring", firePosition, 0.86f, 1.2f, 0.16f, stone, 8);
            CreateCone(parent, "Camp Fire", firePosition, 0.46f, 0.08f, 1.18f, new Color(1f, 0.25f, 0.04f), 7, 15f);
            CreateCone(parent, "Camp Fire Core", firePosition + Vector3.up * 0.08f, 0.24f, 0.02f, 0.76f, new Color(1f, 0.82f, 0.18f), 6, 0f);
        }

        private static Color AccentFor(ZoneDefinition zone)
        {
            var id = (zone != null ? zone.ZoneId : string.Empty).ToLowerInvariant();
            if (id.Contains("forest") || id.Contains("valley")) return new Color(0.32f, 0.82f, 0.42f);
            if (id.Contains("ash") || id.Contains("obsidian")) return new Color(1f, 0.3f, 0.08f);
            if (id.Contains("crystal") || id.Contains("frost")) return new Color(0.35f, 0.82f, 1f);
            if (id.Contains("sunken")) return new Color(0.12f, 0.82f, 0.76f);
            if (id.Contains("astral")) return new Color(0.7f, 0.42f, 1f);
            return new Color(0.5f, 0.28f, 0.86f);
        }

        private static string ClaimIdFor(ZoneDefinition zone, ZonePointOfInterestType type)
        {
            return $"{zone.ZoneId}.{type.ToString().ToLowerInvariant()}";
        }

        private static RewardBundle RewardFor(ZoneDefinition zone, ZonePointOfInterestType type)
        {
            if (zone == null)
            {
                return null;
            }

            switch (type)
            {
                case ZonePointOfInterestType.Entry:
                    return new RewardBundle { Experience = Mathf.Max(1, zone.MinLevel * 12), Gold = Mathf.Max(1, zone.MinLevel * 3) };
                case ZonePointOfInterestType.Elite:
                    return new RewardBundle { Experience = Mathf.Max(1, zone.EliteExp / 5), Gold = Mathf.Max(1, zone.EliteGoldMin / 2) };
                case ZonePointOfInterestType.Boss:
                    return new RewardBundle { Experience = Mathf.Max(1, zone.BossExp / 4), Gold = Mathf.Max(1, zone.BossGoldMin) };
                default:
                    return null;
            }
        }

        private static void CreatePointOfInterest(Transform parent, string name, Vector3 position, string message, ZonePointOfInterestType type, RewardBundle reward, string claimId)
        {
            var pointObject = new GameObject($"{name} POI");
            pointObject.transform.SetParent(parent, true);
            pointObject.transform.position = position;
            var point = pointObject.AddComponent<ZonePointOfInterest>();
            point.DisplayName = name;
            point.InteractionMessage = message;
            point.InteractDistance = 8f;
            point.Type = type;
            point.Reward = reward;
            point.ClaimId = claimId;

            // POI names are shown through the minimap, mission feed and the
            // interaction prompt. Keeping them out of world-space avoids text
            // crossing the camera at arbitrary angles on mobile.
        }

        private static void BuildLandmark(Transform parent, string name, Vector3 position, Color accent, float scale)
        {
            CreatePart(parent, $"{name} Platform", PrimitiveType.Cylinder, position + Vector3.up * 0.08f, new Vector3(scale, 0.08f, scale), Color.Lerp(accent, Color.black, 0.54f));
            CreatePart(parent, $"{name} Pillar L", PrimitiveType.Cube, position + new Vector3(-scale * 0.72f, scale * 0.55f, 0f), new Vector3(0.28f, scale * 1.1f, 0.28f), accent);
            CreatePart(parent, $"{name} Pillar R", PrimitiveType.Cube, position + new Vector3(scale * 0.72f, scale * 0.55f, 0f), new Vector3(0.28f, scale * 1.1f, 0.28f), accent);
            CreatePart(parent, $"{name} Beacon", PrimitiveType.Sphere, position + Vector3.up * scale * 1.35f, new Vector3(0.34f, 0.34f, 0.34f), Color.Lerp(accent, Color.white, 0.3f));
        }

        private static void BuildBoundaryMarker(Transform parent, Vector3 position, Color groundColor)
        {
            CreatePart(parent, "Zone Boundary Marker", PrimitiveType.Cube, position + Vector3.up * 0.42f, new Vector3(0.26f, 0.84f, 0.26f), Color.Lerp(groundColor, Color.white, 0.26f));
        }

        private static void BuildDecoration(Transform parent, ZoneDefinition zone, int index, Vector3 position, System.Random random)
        {
            var id = (zone.ZoneId ?? string.Empty).ToLowerInvariant();
            if (id.Contains("forest") || id.Contains("valley"))
            {
                BuildTree(parent, position, zone.GroundColor, index % 3 == 0);
                return;
            }

            if (id.Contains("crystal") || id.Contains("frost"))
            {
                BuildCrystalCluster(parent, position, zone.GroundColor, 1f + (float)random.NextDouble() * 0.7f);
                return;
            }

            if (id.Contains("sunken"))
            {
                BuildRuins(parent, position, zone.GroundColor, index % 2 == 0);
                return;
            }

            if (id.Contains("astral"))
            {
                BuildAstralGarden(parent, position, zone.GroundColor, index % 3 == 0);
                return;
            }

            BuildObelisk(parent, position, zone.GroundColor, id.Contains("obsidian") || id.Contains("throne"));
        }

        private static void BuildTree(Transform parent, Vector3 position, Color groundColor, bool flowering)
        {
            var foliage = Color.Lerp(groundColor, new Color(0.05f, 0.24f, 0.14f), 0.62f);
            var bark = Color.Lerp(groundColor, new Color(0.22f, 0.1f, 0.045f), 0.72f);
            BuildValleyPine(parent, position, 0.72f + (flowering ? 0.12f : 0f), foliage, bark, flowering);

            if (flowering)
            {
                CreatePart(parent, "Tree Spirit Bloom", PrimitiveType.Sphere, position + Vector3.up * 2.15f + Vector3.forward * 0.28f, new Vector3(0.16f, 0.16f, 0.16f), new Color(0.92f, 0.55f, 0.2f));
            }
        }

        private static void BuildCrystalCluster(Transform parent, Vector3 position, Color groundColor, float scale)
        {
            var crystalColor = Color.Lerp(groundColor, Color.white, 0.5f);
            CreatePart(parent, "Crystal A", PrimitiveType.Cylinder, position + Vector3.up * (0.9f * scale), new Vector3(0.34f * scale, 0.9f * scale, 0.34f * scale), crystalColor, Quaternion.Euler(0f, 22f, -10f));
            CreatePart(parent, "Crystal B", PrimitiveType.Cylinder, position + new Vector3(0.55f, 0.65f, 0.2f) * scale, new Vector3(0.25f * scale, 0.65f * scale, 0.25f * scale), Color.Lerp(crystalColor, Color.white, 0.22f), Quaternion.Euler(0f, -18f, 13f));
            CreatePart(parent, "Crystal C", PrimitiveType.Cylinder, position + new Vector3(-0.46f, 0.48f, -0.16f) * scale, new Vector3(0.2f * scale, 0.48f * scale, 0.2f * scale), Color.Lerp(crystalColor, Color.black, 0.16f), Quaternion.Euler(0f, 32f, -18f));
        }

        private static void BuildRuins(Transform parent, Vector3 position, Color groundColor, bool hasArch)
        {
            var stone = Color.Lerp(groundColor, new Color(0.42f, 0.48f, 0.5f), 0.45f);
            CreatePart(parent, "Ruin Pillar", PrimitiveType.Cube, position + Vector3.up * 0.9f, new Vector3(0.72f, 1.8f, 0.72f), stone, Quaternion.Euler(0f, 18f, 0f));
            CreatePart(parent, "Ruin Cap", PrimitiveType.Cube, position + Vector3.up * 1.88f, new Vector3(0.95f, 0.2f, 0.95f), Color.Lerp(stone, Color.black, 0.18f), Quaternion.Euler(0f, -10f, 0f));

            if (hasArch)
            {
                CreatePart(parent, "Ruin Arch", PrimitiveType.Cube, position + new Vector3(0f, 1.35f, 0.48f), new Vector3(1.55f, 0.24f, 0.24f), stone, Quaternion.Euler(0f, 0f, 0f));
            }
        }

        private static void BuildAstralGarden(Transform parent, Vector3 position, Color groundColor, bool hasOrb)
        {
            var stem = Color.Lerp(groundColor, new Color(0.25f, 0.8f, 0.55f), 0.5f);
            CreatePart(parent, "Astral Stem", PrimitiveType.Cylinder, position + Vector3.up * 0.75f, new Vector3(0.14f, 0.75f, 0.14f), stem);
            CreatePart(parent, "Astral Bloom", PrimitiveType.Sphere, position + Vector3.up * 1.62f, new Vector3(0.75f, 0.35f, 0.75f), new Color(0.38f, 0.55f, 1f));
            if (hasOrb)
            {
                CreatePart(parent, "Astral Orb", PrimitiveType.Sphere, position + Vector3.up * 2.15f, new Vector3(0.22f, 0.22f, 0.22f), new Color(0.9f, 0.78f, 1f));
            }
        }

        private static void BuildObelisk(Transform parent, Vector3 position, Color groundColor, bool dark)
        {
            var stone = dark ? Color.Lerp(groundColor, Color.black, 0.35f) : Color.Lerp(groundColor, new Color(0.4f, 0.34f, 0.5f), 0.38f);
            var accent = dark ? new Color(1f, 0.25f, 0.12f) : new Color(0.65f, 0.42f, 1f);
            CreatePart(parent, "Zone Obelisk", PrimitiveType.Cube, position + Vector3.up * 1.25f, new Vector3(0.56f, 2.5f, 0.56f), stone, Quaternion.Euler(0f, 22f, 0f));
            CreatePart(parent, "Obelisk Rune", PrimitiveType.Cube, position + new Vector3(0f, 1.28f, -0.32f), new Vector3(0.2f, 0.58f, 0.05f), accent);
            CreatePart(parent, "Obelisk Base", PrimitiveType.Cube, position + Vector3.up * 0.12f, new Vector3(0.95f, 0.24f, 0.95f), Color.Lerp(stone, Color.black, 0.22f));
        }

        private static void BuildEntryMarker(Transform parent, ZoneDefinition zone)
        {
            var position = zone.SignPosition + new Vector3(0f, 0f, 2.8f);
            CreatePart(parent, "Entry Marker", PrimitiveType.Cylinder, position + Vector3.up * 0.08f, new Vector3(1.2f, 0.08f, 1.2f), Color.Lerp(zone.GroundColor, Color.white, 0.28f));
            CreatePart(parent, "Entry Pillar L", PrimitiveType.Cube, position + new Vector3(-2.2f, 1.2f, 0f), new Vector3(0.34f, 2.4f, 0.34f), Color.Lerp(zone.GroundColor, Color.black, 0.2f));
            CreatePart(parent, "Entry Pillar R", PrimitiveType.Cube, position + new Vector3(2.2f, 1.2f, 0f), new Vector3(0.34f, 2.4f, 0.34f), Color.Lerp(zone.GroundColor, Color.black, 0.2f));
            CreatePart(parent, "Entry Arch", PrimitiveType.Cube, position + Vector3.up * 2.35f, new Vector3(4.7f, 0.28f, 0.34f), Color.Lerp(zone.GroundColor, Color.white, 0.2f));
            CreatePointOfInterest(parent, "Zone Entry", position, Localization.Tr("poi.entry", zone.DisplayName), ZonePointOfInterestType.Entry, RewardFor(zone, ZonePointOfInterestType.Entry), ClaimIdFor(zone, ZonePointOfInterestType.Entry));
        }

        private static GameObject CreateCone(Transform parent, string name, Vector3 position, float bottomRadius, float topRadius, float height, Color color)
        {
            return CreateCone(parent, name, position, bottomRadius, topRadius, height, color, 8, 0f);
        }

        private static GameObject CreateCone(Transform parent, string name, Vector3 position, float bottomRadius, float topRadius, float height, Color color, int sides, float rotationDegrees)
        {
            sides = Mathf.Clamp(sides, 5, 12);
            var mesh = new Mesh { name = $"{name} Mesh" };
            var vertices = new Vector3[sides * 2 + 2];
            var triangles = new int[sides * 12];
            var rotation = rotationDegrees * Mathf.Deg2Rad;
            var bottomCenter = sides * 2;
            var topCenter = bottomCenter + 1;

            for (var i = 0; i < sides; i++)
            {
                var angle = Mathf.PI * 2f * i / sides + rotation;
                var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                vertices[i] = direction * bottomRadius;
                vertices[sides + i] = new Vector3(direction.x * topRadius, height, direction.z * topRadius);
            }

            vertices[bottomCenter] = Vector3.zero;
            vertices[topCenter] = Vector3.up * height;
            var index = 0;
            for (var i = 0; i < sides; i++)
            {
                var next = (i + 1) % sides;
                triangles[index++] = i;
                triangles[index++] = next;
                triangles[index++] = sides + i;
                triangles[index++] = sides + i;
                triangles[index++] = next;
                triangles[index++] = sides + next;
                triangles[index++] = bottomCenter;
                triangles[index++] = next;
                triangles[index++] = i;
                triangles[index++] = topCenter;
                triangles[index++] = sides + i;
                triangles[index++] = sides + next;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var part = new GameObject(name);
            part.transform.SetParent(parent, true);
            part.transform.position = position;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            part.AddComponent<MeshRenderer>().sharedMaterial = VisualMaterialUtility.Create(
                color,
                VisualMaterialUtility.ShouldGlow(name),
                color.r < 0.35f ? 0.08f : 0.02f,
                0.28f);
            return part;
        }

        private static GameObject CreateRing(Transform parent, string name, Vector3 position, float innerRadius, float outerRadius, float height, Color color, int sides = 48)
        {
            sides = Mathf.Clamp(sides, 8, 64);
            innerRadius = Mathf.Max(0.02f, innerRadius);
            outerRadius = Mathf.Max(innerRadius + 0.01f, outerRadius);
            height = Mathf.Max(0.008f, height);

            var mesh = new Mesh { name = $"{name} Mesh" };
            var vertices = new Vector3[sides * 4];
            var triangles = new int[sides * 24];
            for (var i = 0; i < sides; i++)
            {
                var angle = Mathf.PI * 2f * i / sides;
                var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                vertices[i] = direction * outerRadius;
                vertices[sides + i] = direction * innerRadius;
                vertices[sides * 2 + i] = direction * outerRadius + Vector3.down * height;
                vertices[sides * 3 + i] = direction * innerRadius + Vector3.down * height;
            }

            var triangleIndex = 0;
            for (var i = 0; i < sides; i++)
            {
                var next = (i + 1) % sides;
                var outer = i;
                var inner = sides + i;
                var outerNext = next;
                var innerNext = sides + next;
                triangles[triangleIndex++] = outer;
                triangles[triangleIndex++] = inner;
                triangles[triangleIndex++] = outerNext;
                triangles[triangleIndex++] = outerNext;
                triangles[triangleIndex++] = inner;
                triangles[triangleIndex++] = innerNext;

                var bottomOuter = sides * 2 + i;
                var bottomInner = sides * 3 + i;
                var bottomOuterNext = sides * 2 + next;
                var bottomInnerNext = sides * 3 + next;
                triangles[triangleIndex++] = bottomOuter;
                triangles[triangleIndex++] = bottomOuterNext;
                triangles[triangleIndex++] = bottomInner;
                triangles[triangleIndex++] = bottomOuterNext;
                triangles[triangleIndex++] = bottomInnerNext;
                triangles[triangleIndex++] = bottomInner;

                triangles[triangleIndex++] = outer;
                triangles[triangleIndex++] = outerNext;
                triangles[triangleIndex++] = bottomOuter;
                triangles[triangleIndex++] = outerNext;
                triangles[triangleIndex++] = bottomOuterNext;
                triangles[triangleIndex++] = bottomOuter;

                triangles[triangleIndex++] = inner;
                triangles[triangleIndex++] = bottomInner;
                triangles[triangleIndex++] = innerNext;
                triangles[triangleIndex++] = innerNext;
                triangles[triangleIndex++] = bottomInner;
                triangles[triangleIndex++] = bottomInnerNext;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var part = new GameObject(name);
            part.transform.SetParent(parent, true);
            part.transform.position = position;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            part.AddComponent<MeshRenderer>().sharedMaterial = VisualMaterialUtility.Create(
                color,
                VisualMaterialUtility.ShouldGlow(name),
                0.04f,
                0.28f);
            return part;
        }

        private static GameObject CreatePart(Transform parent, string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color)
        {
            return CreatePart(parent, name, primitive, position, scale, color, Quaternion.identity);
        }

        private static GameObject CreatePart(Transform parent, string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color, Quaternion rotation)
        {
            return CreatePart(parent, name, primitive, position, scale, color, rotation, false);
        }

        private static GameObject CreateSolidPart(Transform parent, string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color, Quaternion rotation)
        {
            return CreatePart(parent, name, primitive, position, scale, color, rotation, true);
        }

        private static GameObject CreatePart(Transform parent, string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color, Quaternion rotation, bool solid)
        {
            var part = GameObject.CreatePrimitive(primitive);
            part.name = name;
            part.transform.SetParent(parent, true);
            part.transform.position = position;
            part.transform.localScale = scale;
            part.transform.rotation = rotation;
            var collider = part.GetComponent<Collider>();
            if (collider != null && !solid)
            {
                UnityEngine.Object.Destroy(collider);
            }

            part.GetComponent<Renderer>().sharedMaterial = VisualMaterialUtility.Create(color, VisualMaterialUtility.ShouldGlow(name), 0.04f, 0.22f);
            return part;
        }

        private static Material MaterialFor(Color color)
        {
            var key = ((Color32)color).r << 24 | ((Color32)color).g << 16 | ((Color32)color).b << 8 | ((Color32)color).a;
            if (Materials.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
            var material = VisualMaterialUtility.Create(color, false, 0.04f, 0.22f);
            Materials[key] = material;
            return material;
        }

        private static int StableSeed(string value)
        {
            unchecked
            {
                var hash = 17;
                foreach (var character in value ?? string.Empty)
                {
                    hash = hash * 31 + character;
                }

                return hash;
            }
        }
    }
}
