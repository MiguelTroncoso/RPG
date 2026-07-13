using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class PrototypePulseAndDestroy : MonoBehaviour
    {
        private float age;
        private float lifetime = 0.28f;
        private Vector3 startScale;

        public static void Spawn(Vector3 position, Color color)
        {
            var pulse = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pulse.name = "Hit Pulse";
            pulse.transform.position = position;
            pulse.transform.localScale = Vector3.one * 0.2f;

            var collider = pulse.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var material = VisualMaterialUtility.Create(color, true, 0.02f, 0.18f);
            pulse.GetComponent<Renderer>().sharedMaterial = material;
            pulse.AddComponent<PrototypePulseAndDestroy>();
        }

        private void Awake()
        {
            startScale = transform.localScale;
        }

        private void Update()
        {
            age += Time.deltaTime;
            var t = Mathf.Clamp01(age / lifetime);
            transform.localScale = Vector3.Lerp(startScale, Vector3.one * 1.15f, t);

            if (age >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
