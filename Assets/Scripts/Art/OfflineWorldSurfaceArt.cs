using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Lightweight authored-looking terrain surfaces. Keeping these generated
    // makes the offline beta self-contained while final scanned terrain assets
    // remain an optional production upgrade.
    public static class OfflineWorldSurfaceArt
    {
        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        public static Material MaterialFor(ZoneDefinition zone)
        {
            var zoneId = zone != null && !string.IsNullOrWhiteSpace(zone.ZoneId) ? zone.ZoneId : "valley";
            if (Materials.TryGetValue(zoneId, out var cached) && cached != null)
            {
                return cached;
            }

            var texture = BuildTexture(zoneId, zone != null ? zone.GroundColor : new Color(0.2f, 0.34f, 0.28f));
            var material = VisualMaterialUtility.CreateTextured(Color.white, texture, false, 0.015f, 0.18f);
            if (material.HasProperty("_MainTex"))
            {
                material.SetTextureScale("_MainTex", new Vector2(5.5f, 5.5f));
            }

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTextureScale("_BaseMap", new Vector2(5.5f, 5.5f));
            }

            Materials[zoneId] = material;
            return material;
        }

        private static Texture2D BuildTexture(string zoneId, Color baseColor)
        {
            const int size = 256;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                name = $"Offline Surface {zoneId}",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 2
            };

            var seed = StableSeed(zoneId);
            var pixels = new Color[size * size];
            var dark = Color.Lerp(baseColor, new Color(0.025f, 0.035f, 0.04f), 0.34f);
            var light = Color.Lerp(baseColor, Color.white, 0.15f);
            var accent = AccentFor(zoneId, baseColor);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var u = x / (float)size;
                    var v = y / (float)size;
                    var macro = Mathf.PerlinNoise(u * 3.8f + seed * 0.0007f, v * 3.8f + seed * 0.0011f);
                    var detail = Mathf.PerlinNoise(u * 19f + seed * 0.0019f, v * 17f + seed * 0.0013f);
                    var grain = Mathf.Sin((u + v) * 48f + seed * 0.008f) * 0.035f;
                    var value = Mathf.Clamp01(macro * 0.72f + detail * 0.22f + grain);
                    var color = Color.Lerp(dark, light, value);

                    var vein = Mathf.Abs(Mathf.Sin((u * 8f - v * 5.4f + seed * 0.0003f) * Mathf.PI));
                    if (vein > 0.975f)
                    {
                        color = Color.Lerp(color, accent, 0.18f);
                    }

                    pixels[y * size + x] = color;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(true, true);
            return texture;
        }

        private static Color AccentFor(string zoneId, Color fallback)
        {
            var id = (zoneId ?? string.Empty).ToLowerInvariant();
            if (id.Contains("ash") || id.Contains("obsidian")) return new Color(0.8f, 0.16f, 0.05f);
            if (id.Contains("crystal") || id.Contains("frost")) return new Color(0.22f, 0.78f, 1f);
            if (id.Contains("sunken")) return new Color(0.1f, 0.72f, 0.68f);
            if (id.Contains("astral") || id.Contains("eclipse") || id.Contains("throne")) return new Color(0.64f, 0.26f, 0.92f);
            return Color.Lerp(fallback, new Color(0.35f, 0.82f, 0.34f), 0.62f);
        }

        private static int StableSeed(string value)
        {
            unchecked
            {
                var hash = 23;
                foreach (var character in value ?? string.Empty)
                {
                    hash = hash * 31 + character;
                }

                return Mathf.Abs(hash);
            }
        }
    }
}
