using UnityEngine;

namespace MmorpgPrototype
{
    // Offline presentation layer: atmosphere, sky and subtle mobile-friendly
    // ambience that makes the ten authored zones feel like one world.
    public sealed class OfflinePresentationDirector : MonoBehaviour
    {
        public Transform Target;
        public Camera WorldCamera;

        private ParticleSystem atmosphere;

        public void Initialize(Transform target, Camera worldCamera)
        {
            Target = target;
            WorldCamera = worldCamera;
            ConfigureSky();
            CreateAtmosphere();
        }

        private void Update()
        {
            if (Target == null || atmosphere == null)
            {
                return;
            }

            var position = Target.position;
            position.y = 1.6f;
            atmosphere.transform.position = position;
        }

        private void ConfigureSky()
        {
            if (WorldCamera != null)
            {
                WorldCamera.clearFlags = CameraClearFlags.Skybox;
                WorldCamera.backgroundColor = new Color(0.07f, 0.11f, 0.16f);
            }

            var skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader == null)
            {
                return;
            }

            var sky = new Material(skyShader);
            if (sky.HasProperty("_SkyTint"))
            {
                sky.SetColor("_SkyTint", new Color(0.34f, 0.48f, 0.62f));
            }

            if (sky.HasProperty("_GroundColor"))
            {
                sky.SetColor("_GroundColor", new Color(0.07f, 0.12f, 0.14f));
            }

            if (sky.HasProperty("_SunSize"))
            {
                sky.SetFloat("_SunSize", 0.035f);
            }

            if (sky.HasProperty("_AtmosphereThickness"))
            {
                sky.SetFloat("_AtmosphereThickness", 0.72f);
            }

            RenderSettings.skybox = sky;
            DynamicGI.UpdateEnvironment();
        }

        private void CreateAtmosphere()
        {
            var objectRoot = new GameObject("Offline Valley Atmosphere");
            objectRoot.transform.SetParent(transform, false);
            atmosphere = objectRoot.AddComponent<ParticleSystem>();

            var main = atmosphere.main;
            main.loop = true;
            main.playOnAwake = true;
            main.duration = 8f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.015f, 0.05f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.075f);
            main.startColor = new Color(0.52f, 0.88f, 0.76f, 0.34f);
            main.maxParticles = Application.platform == RuntimePlatform.Android ? 28 : 44;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = atmosphere.emission;
            emission.enabled = true;
            emission.rateOverTime = Application.platform == RuntimePlatform.Android ? 5f : 8f;

            var shape = atmosphere.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(32f, 5f, 32f);

            var renderer = objectRoot.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = VisualMaterialUtility.Create(
                new Color(0.45f, 0.92f, 0.78f), true, 0f, 0.08f);
            atmosphere.Play();
        }
    }
}
