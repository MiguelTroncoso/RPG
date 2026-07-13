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
            CreatePointOfInterest(parent, "Elite Landmark", zone.EliteAreaCenter, Localization.Tr("poi.elite", zone.DisplayName));
            CreatePointOfInterest(parent, "Boss Landmark", zone.BossPosition, Localization.Tr("poi.boss", zone.DisplayName));

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
            var markerColor = new Color(0.16f, 0.72f, 0.48f);
            CreatePart(parent, "Safe Commerce Zone", PrimitiveType.Cylinder, center, new Vector3(zone.SafeZoneRadius * 2f, 0.018f, zone.SafeZoneRadius * 2f), Color.Lerp(zone.GroundColor, markerColor, 0.42f));
            CreatePart(parent, "Safe Zone Marker L", PrimitiveType.Cube, center + new Vector3(-zone.SafeZoneRadius, 0.6f, 0f), new Vector3(0.18f, 1.2f, 0.18f), markerColor);
            CreatePart(parent, "Safe Zone Marker R", PrimitiveType.Cube, center + new Vector3(zone.SafeZoneRadius, 0.6f, 0f), new Vector3(0.18f, 1.2f, 0.18f), markerColor);
            CreatePointOfInterest(parent, "Safe Commerce Zone", zone.SafeZoneCenter, Localization.Tr("poi.safe", zone.DisplayName));
        }

        private static void CreatePointOfInterest(Transform parent, string name, Vector3 position, string message)
        {
            var pointObject = new GameObject($"{name} POI");
            pointObject.transform.SetParent(parent, true);
            pointObject.transform.position = position;
            var point = pointObject.AddComponent<ZonePointOfInterest>();
            point.DisplayName = name;
            point.InteractionMessage = message;
            point.InteractDistance = 8f;

            var labelObject = new GameObject("POI Label");
            labelObject.transform.SetParent(pointObject.transform, false);
            labelObject.transform.localPosition = Vector3.up * 3.1f;
            var label = labelObject.AddComponent<TextMesh>();
            label.text = name;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 26;
            label.characterSize = 0.035f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = Color.Lerp(Color.white, Color.yellow, 0.25f);
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
            var trunk = CreatePart(parent, "Tree Trunk", PrimitiveType.Cylinder, position + Vector3.up * 1.1f, new Vector3(0.32f, 1.1f, 0.32f), Color.Lerp(groundColor, new Color(0.25f, 0.12f, 0.06f), 0.65f));
            trunk.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            var canopyColor = Color.Lerp(groundColor, new Color(0.14f, 0.5f, 0.2f), 0.58f);
            CreatePart(parent, "Tree Canopy", PrimitiveType.Sphere, position + Vector3.up * 2.45f, new Vector3(1.35f, 1.15f, 1.35f), canopyColor);

            if (flowering)
            {
                CreatePart(parent, "Tree Flower", PrimitiveType.Sphere, position + Vector3.up * 2.72f + Vector3.forward * 0.58f, new Vector3(0.22f, 0.22f, 0.22f), new Color(0.92f, 0.55f, 0.2f));
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
            CreatePointOfInterest(parent, "Zone Entry", position, Localization.Tr("poi.entry", zone.DisplayName));
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

            part.GetComponent<Renderer>().sharedMaterial = MaterialFor(color);
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
            var material = new Material(shader) { color = color };
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
