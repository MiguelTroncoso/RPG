using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Shared grounding and tier accents keep imported authored meshes readable
    // on the flat prototype terrain without adding gameplay colliders.
    public static class ArtPresentationUtility
    {
        private static readonly Dictionary<int, Material> GroundMaterials = new Dictionary<int, Material>();

        public static void AttachGroundAnchor(Transform parent, float y, float radius, Color accent, bool tierAura)
        {
            if (parent == null)
            {
                return;
            }

            var shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shadow.name = tierAura ? "Tier Aura Anchor" : "Soft Ground Shadow";
            shadow.transform.SetParent(parent, false);
            shadow.transform.localPosition = new Vector3(0f, y, 0f);
            shadow.transform.localScale = new Vector3(radius, 0.008f, radius);

            var collider = shadow.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }

            var material = CreateTransparentMaterial(tierAura
                ? new Color(accent.r, accent.g, accent.b, 0.22f)
                : new Color(0.015f, 0.02f, 0.025f, 0.34f));
            shadow.GetComponent<Renderer>().sharedMaterial = material;

            var pulse = shadow.AddComponent<PresentationPulse>();
            pulse.BaseScale = shadow.transform.localScale;
            pulse.Amplitude = tierAura ? 0.08f : 0.025f;
            pulse.Speed = tierAura ? 2.4f : 1.2f;
            pulse.Tint = tierAura ? accent : new Color(0.02f, 0.025f, 0.03f);
            pulse.UseEmission = tierAura;
        }

        private static Material CreateTransparentMaterial(Color color)
        {
            var color32 = (Color32)color;
            var key = color32.r << 24 | color32.g << 16 | color32.b << 8 | color32.a;
            if (GroundMaterials.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var shader = Shader.Find("Legacy Shaders/Transparent/Diffuse")
                ?? Shader.Find("Unlit/Transparent")
                ?? Shader.Find("Standard");
            var material = new Material(shader)
            {
                color = color,
                renderQueue = 3000
            };

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            GroundMaterials[key] = material;
            return material;
        }

        private sealed class PresentationPulse : MonoBehaviour
        {
            public Vector3 BaseScale;
            public float Amplitude;
            public float Speed;
            public Color Tint;
            public bool UseEmission;

            private Renderer targetRenderer;
            private MaterialPropertyBlock block;

            private void Awake()
            {
                targetRenderer = GetComponent<Renderer>();
                block = new MaterialPropertyBlock();
            }

            private void Update()
            {
                var pulse = 1f + Mathf.Sin(Time.time * Speed) * Amplitude;
                transform.localScale = BaseScale * pulse;

                if (!UseEmission || targetRenderer == null)
                {
                    return;
                }

                targetRenderer.GetPropertyBlock(block);
                block.SetColor("_EmissionColor", Tint * (0.65f + pulse * 0.35f));
                targetRenderer.SetPropertyBlock(block);
            }
        }
    }
}
