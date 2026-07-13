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

            for (var i = 0; i < 12; i++)
            {
                var angle = (float)(Math.PI * 2d * i / 12d) + (float)random.NextDouble() * 0.34f;
                var radius = 10f + (float)random.NextDouble() * 14f;
                var position = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                BuildDecoration(root.transform, zone, i, position, random);
            }

            BuildEntryMarker(root.transform, zone);
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
        }

        private static GameObject CreatePart(Transform parent, string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color)
        {
            return CreatePart(parent, name, primitive, position, scale, color, Quaternion.identity);
        }

        private static GameObject CreatePart(Transform parent, string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color, Quaternion rotation)
        {
            var part = GameObject.CreatePrimitive(primitive);
            part.name = name;
            part.transform.SetParent(parent, true);
            part.transform.position = position;
            part.transform.localScale = scale;
            part.transform.rotation = rotation;
            var collider = part.GetComponent<Collider>();
            if (collider != null)
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
