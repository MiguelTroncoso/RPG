using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class PlayerAvatarVisual : MonoBehaviour
    {
        private GameObject visualRoot;
        private Renderer baseRenderer;

        private void Awake()
        {
            baseRenderer = GetComponent<Renderer>();
            if (baseRenderer != null)
            {
                baseRenderer.enabled = false;
            }
        }

        public void Apply(ClassDefinition definition, CharacterGender gender)
        {
            if (definition == null)
            {
                return;
            }

            if (visualRoot != null)
            {
                Destroy(visualRoot);
            }

            visualRoot = new GameObject("Procedural Avatar Visual");
            visualRoot.transform.SetParent(transform, false);
            visualRoot.transform.localPosition = Vector3.zero;
            visualRoot.transform.localRotation = Quaternion.identity;
            visualRoot.transform.localScale = gender == CharacterGender.Femenino
                ? new Vector3(0.88f, 0.98f, 0.88f)
                : new Vector3(1.02f, 1.04f, 1.02f);

            var baseColor = definition.BodyColor;
            var accentColor = definition.SkillColor;
            var genderTint = gender == CharacterGender.Femenino
                ? new Color(1f, 0.78f, 0.9f)
                : new Color(0.76f, 0.88f, 1f);
            var bodyColor = Color.Lerp(baseColor, genderTint, 0.16f);

            CreatePart("Torso", PrimitiveType.Capsule, new Vector3(0f, 0f, 0f), new Vector3(0.55f, 0.82f, 0.42f), bodyColor);
            CreatePart("Head", PrimitiveType.Sphere, new Vector3(0f, 0.92f, 0f), new Vector3(0.42f, 0.42f, 0.42f), Color.Lerp(bodyColor, Color.white, 0.16f));
            CreatePart("Chest Accent", PrimitiveType.Cube, new Vector3(0f, 0.18f, -0.36f), new Vector3(0.5f, 0.12f, 0.08f), accentColor);
            CreatePart("Left Arm", PrimitiveType.Capsule, new Vector3(-0.42f, 0.18f, 0f), new Vector3(0.18f, 0.42f, 0.18f), Color.Lerp(bodyColor, Color.black, 0.08f), Quaternion.Euler(0f, 0f, 18f));
            CreatePart("Right Arm", PrimitiveType.Capsule, new Vector3(0.42f, 0.18f, 0f), new Vector3(0.18f, 0.42f, 0.18f), Color.Lerp(bodyColor, Color.black, 0.08f), Quaternion.Euler(0f, 0f, -18f));

            switch (definition.Type)
            {
                case CharacterClassType.Ninja:
                    BuildNinja(accentColor);
                    break;
                case CharacterClassType.Chaman:
                    BuildChaman(accentColor);
                    break;
                case CharacterClassType.Umbra:
                    BuildUmbra(accentColor);
                    break;
                default:
                    BuildGuerrero(accentColor);
                    break;
            }
        }

        private void BuildGuerrero(Color accentColor)
        {
            CreatePart("Left Shoulder", PrimitiveType.Cube, new Vector3(-0.52f, 0.52f, 0f), new Vector3(0.34f, 0.18f, 0.36f), Color.Lerp(accentColor, Color.white, 0.16f));
            CreatePart("Right Shoulder", PrimitiveType.Cube, new Vector3(0.52f, 0.52f, 0f), new Vector3(0.34f, 0.18f, 0.36f), Color.Lerp(accentColor, Color.white, 0.16f));
            CreatePart("Back Blade", PrimitiveType.Cube, new Vector3(0.32f, 0.42f, 0.42f), new Vector3(0.08f, 0.92f, 0.12f), new Color(0.85f, 0.9f, 0.92f), Quaternion.Euler(0f, 0f, -34f));
        }

        private void BuildNinja(Color accentColor)
        {
            CreatePart("Mask", PrimitiveType.Cube, new Vector3(0f, 0.92f, -0.22f), new Vector3(0.38f, 0.12f, 0.06f), Color.Lerp(Color.black, accentColor, 0.22f));
            CreatePart("Left Dagger", PrimitiveType.Cube, new Vector3(-0.52f, -0.08f, -0.22f), new Vector3(0.06f, 0.48f, 0.08f), new Color(0.76f, 0.84f, 0.88f), Quaternion.Euler(0f, 0f, 22f));
            CreatePart("Right Dagger", PrimitiveType.Cube, new Vector3(0.52f, -0.08f, -0.22f), new Vector3(0.06f, 0.48f, 0.08f), new Color(0.76f, 0.84f, 0.88f), Quaternion.Euler(0f, 0f, -22f));
        }

        private void BuildChaman(Color accentColor)
        {
            CreatePart("Crown Orb", PrimitiveType.Sphere, new Vector3(0f, 1.28f, 0f), new Vector3(0.18f, 0.18f, 0.18f), Color.Lerp(accentColor, Color.white, 0.28f));
            CreatePart("Spirit Staff", PrimitiveType.Cylinder, new Vector3(0.58f, 0.28f, 0.05f), new Vector3(0.07f, 0.82f, 0.07f), new Color(0.72f, 0.54f, 0.24f), Quaternion.Euler(0f, 0f, -12f));
            CreatePart("Staff Crystal", PrimitiveType.Sphere, new Vector3(0.68f, 0.98f, 0.05f), new Vector3(0.2f, 0.2f, 0.2f), accentColor);
        }

        private void BuildUmbra(Color accentColor)
        {
            CreatePart("Void Crest", PrimitiveType.Cube, new Vector3(0f, 1.24f, 0f), new Vector3(0.16f, 0.34f, 0.16f), accentColor, Quaternion.Euler(0f, 0f, 45f));
            CreatePart("Left Shadow Guard", PrimitiveType.Cube, new Vector3(-0.54f, 0.45f, 0f), new Vector3(0.22f, 0.42f, 0.2f), Color.Lerp(accentColor, Color.black, 0.22f), Quaternion.Euler(0f, 0f, 18f));
            CreatePart("Right Shadow Guard", PrimitiveType.Cube, new Vector3(0.54f, 0.45f, 0f), new Vector3(0.22f, 0.42f, 0.2f), Color.Lerp(accentColor, Color.black, 0.22f), Quaternion.Euler(0f, 0f, -18f));
            CreatePart("Void Core", PrimitiveType.Sphere, new Vector3(0f, 0.22f, -0.42f), new Vector3(0.22f, 0.22f, 0.22f), Color.Lerp(accentColor, Color.white, 0.12f));
        }

        private GameObject CreatePart(string partName, PrimitiveType primitive, Vector3 localPosition, Vector3 localScale, Color color)
        {
            return CreatePart(partName, primitive, localPosition, localScale, color, Quaternion.identity);
        }

        private GameObject CreatePart(string partName, PrimitiveType primitive, Vector3 localPosition, Vector3 localScale, Color color, Quaternion localRotation)
        {
            var part = GameObject.CreatePrimitive(primitive);
            part.name = partName;
            part.transform.SetParent(visualRoot.transform, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;

            var collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Standard"));
                material.color = color;
                renderer.sharedMaterial = material;
            }

            return part;
        }
    }
}
