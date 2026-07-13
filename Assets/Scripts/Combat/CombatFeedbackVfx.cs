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
        }

        public static void SpawnSkill(Vector3 position, Color color)
        {
            SpawnBurst("Skill VFX", position, color, 12, 1.7f, 0.2f);
        }

        public static void SpawnLevelUp(Vector3 position, Color color)
        {
            SpawnBurst("Level Up VFX", position, color, 20, 2.4f, 0.34f);
        }

        public static void SpawnEnemyTelegraph(Vector3 position, Color color)
        {
            SpawnBurst("Enemy Telegraph VFX", position, color, 8, 0.55f, 0.32f);
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
    }
}
