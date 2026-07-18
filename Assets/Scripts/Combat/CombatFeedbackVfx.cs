using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Small particle bursts for hit, skill and level-up feedback. The effect
    // uses one cached unlit material per color and remains mobile-friendly.
    public static class CombatFeedbackVfx
    {
        private static readonly Dictionary<int, Material> Materials = new Dictionary<int, Material>();

        public static void SpawnHit(Vector3 position, Color color, bool critical)
        {
            SpawnBurst("Hit VFX", position, color, (short)(critical ? 16 : 8), critical ? 2.8f : 2.1f, critical ? 0.22f : 0.16f);
            SpawnRing(position + Vector3.down * 0.78f, color, critical ? 0.3f : 0.2f, critical ? 1.15f : 0.78f, critical ? 0.32f : 0.22f);
        }

        public static void SpawnSkill(Vector3 position, Color color)
        {
            SpawnBurst("Skill VFX", position, color, 12, 1.7f, 0.2f);
            SpawnRing(position + Vector3.down * 0.76f, color, 0.35f, 1.45f, 0.42f);
        }

        public static void SpawnUltimate(Vector3 position, Color color)
        {
            SpawnBurst("Ultimate VFX", position, color, 26, 3.4f, 0.65f);
            SpawnRing(position + Vector3.down * 0.78f, color, 0.45f, 2.4f, 0.72f);
            SpawnRing(position + Vector3.down * 0.76f, Color.Lerp(color, Color.white, 0.45f), 0.25f, 1.55f, 0.48f);
        }

        public static void SpawnLevelUp(Vector3 position, Color color)
        {
            SpawnBurst("Level Up VFX", position, color, 20, 2.4f, 0.34f);
            SpawnRing(position + Vector3.down * 0.7f, color, 0.35f, 1.8f, 0.6f);
        }

        public static void SpawnEnemyTelegraph(Vector3 position, Color color)
        {
            SpawnBurst("Enemy Telegraph VFX", position, color, 8, 0.55f, 0.32f);
            SpawnRing(position + Vector3.down * 0.08f, color, 0.3f, 1.1f, 0.4f);
        }

        private static void SpawnRing(Vector3 position, Color color, float startRadius, float endRadius, float lifetime)
        {
            var effect = new GameObject("Combat Ring VFX");
            effect.transform.position = position;

            var line = effect.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 32;
            line.startWidth = 0.045f;
            line.endWidth = 0.008f;
            line.material = MaterialFor(Color.Lerp(color, Color.white, 0.16f));
            for (var i = 0; i < line.positionCount; i++)
            {
                var angle = Mathf.PI * 2f * i / line.positionCount;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * startRadius, 0f, Mathf.Sin(angle) * startRadius));
            }

            var animation = effect.AddComponent<RingAnimation>();
            animation.Line = line;
            animation.StartRadius = startRadius;
            animation.EndRadius = endRadius;
            animation.Lifetime = lifetime;
            Object.Destroy(effect, lifetime + 0.08f);
        }

        private static void SpawnBurst(string name, Vector3 position, Color color, short count, float speed, float lifetime)
        {
            var effect = new GameObject(name);
            effect.transform.position = position;
            var particleSystem = effect.AddComponent<ParticleSystem>();
            var main = particleSystem.main;
            main.duration = lifetime;
            main.loop = false;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = 0.07f;
            main.startColor = color;
            main.maxParticles = count;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = particleSystem.emission;
            emission.enabled = true;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.08f;

            var renderer = effect.GetComponent<ParticleSystemRenderer>();
            renderer.material = MaterialFor(color);
            particleSystem.Play();
            Object.Destroy(effect, lifetime + 0.25f);
        }

        private static Material MaterialFor(Color color)
        {
            var colorKey = ((Color32)color).r << 24 | ((Color32)color).g << 16 | ((Color32)color).b << 8 | ((Color32)color).a;
            if (Materials.TryGetValue(colorKey, out var material) && material != null)
            {
                return material;
            }

            var shader = Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Standard");
            material = VisualMaterialUtility.Create(color, true, 0f, 0.1f);
            Materials[colorKey] = material;
            return material;
        }

        private sealed class RingAnimation : MonoBehaviour
        {
            public LineRenderer Line;
            public float StartRadius;
            public float EndRadius;
            public float Lifetime;

            private float elapsed;

            private void Update()
            {
                elapsed += Time.deltaTime;
                var normalized = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, Lifetime));
                var radius = Mathf.Lerp(StartRadius, EndRadius, normalized);
                for (var i = 0; i < Line.positionCount; i++)
                {
                    var angle = Mathf.PI * 2f * i / Line.positionCount;
                    Line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
                }

                Line.startWidth = Mathf.Lerp(0.05f, 0.008f, normalized);
                Line.endWidth = Mathf.Lerp(0.012f, 0.002f, normalized);
            }
        }
    }
}
