using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Materiales compartidos para una respuesta de luz consistente y ligera.
    public static class VisualMaterialUtility
    {
        private static readonly Dictionary<int, Material> Cache = new Dictionary<int, Material>();

        public static Material Create(Color color, bool emissive = false, float metallic = 0.05f, float smoothness = 0.24f)
        {
            return CreateInternal(color, null, emissive, metallic, smoothness);
        }

        public static Material CreateTextured(Color color, Texture2D texture, bool emissive = false, float metallic = 0.05f, float smoothness = 0.24f)
        {
            return CreateInternal(color, texture, emissive, metallic, smoothness);
        }

        private static Material CreateInternal(Color color, Texture2D texture, bool emissive, float metallic, float smoothness)
        {
            var color32 = (Color32)color;
            var key = color32.r << 24 | color32.g << 16 | color32.b << 8 | color32.a;
            key = key * 31 + (emissive ? 1 : 0);
            key = key * 31 + Mathf.RoundToInt(metallic * 100f);
            key = key * 31 + Mathf.RoundToInt(smoothness * 100f);
            key = key * 31 + (texture != null ? texture.name.GetHashCode() : 0);
            if (Cache.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader);
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (texture != null)
            {
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", texture);
                }

                if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", texture);
                }
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (emissive)
            {
                if (material.HasProperty("_EmissionColor"))
                {
                    material.SetColor("_EmissionColor", color * 1.65f);
                }

                material.EnableKeyword("_EMISSION");
            }

            Cache[key] = material;
            return material;
        }

        public static bool ShouldGlow(string objectName)
        {
            var name = objectName ?? string.Empty;
            return name.Contains("Beacon") || name.Contains("Rune") || name.Contains("Core")
                || name.Contains("Crystal") || name.Contains("Orb") || name.Contains("Halo")
                || name.Contains("Ring") || name.Contains("Gem") || name.Contains("Pearl")
                || name.Contains("Star") || name.Contains("Magma") || name.Contains("Prism");
        }
    }
}
