using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class DamagePopup : MonoBehaviour
    {
        private TextMesh textMesh;
        private Color baseColor;
        private float lifetime = 0.8f;
        private float age;

        public static void Spawn(Vector3 position, string text, Color color)
        {
            var popup = new GameObject("Damage Popup");
            popup.transform.position = position;

            var textMesh = popup.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = 54;
            textMesh.characterSize = 0.035f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = color;

            var component = popup.AddComponent<DamagePopup>();
            component.textMesh = textMesh;
            component.baseColor = color;
        }

        private void LateUpdate()
        {
            age += Time.deltaTime;
            transform.position += Vector3.up * (1.15f * Time.deltaTime);

            var camera = Camera.main;
            if (camera != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - camera.transform.position);
            }

            var t = Mathf.Clamp01(age / lifetime);
            textMesh.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - t);

            if (age >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}

