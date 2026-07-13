using UnityEngine;

namespace MmorpgPrototype
{
    // Mobile-friendly enemy presentation built from primitives until dedicated
    // creature assets are imported. The family is selected from data ids.
    public sealed class EnemyVisualController : MonoBehaviour
    {
        private GameObject visualRoot;
        private GameObject statusRoot;
        private Transform healthFill;
        private Health health;
        private float healthBarWidth = 1.25f;

        public void Initialize(string enemyId, string displayName, EnemyTier tier, Color baseColor, float scale, Health enemyHealth)
        {
            health = enemyHealth;
            visualRoot = new GameObject("Enemy Visual");
            visualRoot.transform.SetParent(transform, false);
            visualRoot.transform.localScale = Vector3.one * Mathf.Max(0.7f, scale);

            var id = (enemyId ?? string.Empty).ToLowerInvariant();
            var accent = AccentFor(tier, baseColor);

            if (id.Contains("forest") || id.Contains("valley"))
            {
                BuildBeast(baseColor, accent);
            }
            else if (id.Contains("crystal") || id.Contains("frost") || id.Contains("obsidian"))
            {
                BuildGuardian(baseColor, accent);
            }
            else if (id.Contains("sunken") || id.Contains("astral"))
            {
                BuildSpirit(baseColor, accent);
            }
            else
            {
                BuildShadow(baseColor, accent);
            }

            BuildTierAdornment(tier, accent);
            BuildStatusDisplay(displayName, tier, accent);
        }

        private void Update()
        {
            if (health == null || healthFill == null)
            {
                return;
            }

            var normalized = health.Normalized;
            healthFill.localScale = new Vector3(Mathf.Max(0.01f, normalized), 1f, 1f);
            healthFill.localPosition = new Vector3(-healthBarWidth * 0.5f * (1f - normalized), 0f, -0.035f);

            var camera = Camera.main;
            if (camera != null && statusRoot != null)
            {
                statusRoot.transform.LookAt(camera.transform);
            }
        }

        private void BuildBeast(Color bodyColor, Color accent)
        {
            var dark = Color.Lerp(bodyColor, Color.black, 0.32f);
            CreatePart("Beast Body", PrimitiveType.Capsule, new Vector3(0f, 0f, 0f), new Vector3(0.58f, 0.5f, 0.88f), bodyColor, Quaternion.Euler(90f, 0f, 0f));
            CreatePart("Beast Head", PrimitiveType.Sphere, new Vector3(0f, 0.12f, 0.72f), new Vector3(0.5f, 0.46f, 0.5f), bodyColor);
            CreatePart("Beast Snout", PrimitiveType.Cube, new Vector3(0f, 0.02f, 0.98f), new Vector3(0.28f, 0.2f, 0.28f), dark);
            CreatePart("Beast Leg L", PrimitiveType.Capsule, new Vector3(-0.32f, -0.43f, 0.38f), new Vector3(0.17f, 0.38f, 0.17f), dark);
            CreatePart("Beast Leg R", PrimitiveType.Capsule, new Vector3(0.32f, -0.43f, 0.38f), new Vector3(0.17f, 0.38f, 0.17f), dark);
            CreatePart("Beast Leg Back L", PrimitiveType.Capsule, new Vector3(-0.32f, -0.43f, -0.38f), new Vector3(0.17f, 0.38f, 0.17f), dark);
            CreatePart("Beast Leg Back R", PrimitiveType.Capsule, new Vector3(0.32f, -0.43f, -0.38f), new Vector3(0.17f, 0.38f, 0.17f), dark);
            CreatePart("Beast Ear L", PrimitiveType.Cylinder, new Vector3(-0.22f, 0.48f, 0.68f), new Vector3(0.12f, 0.24f, 0.12f), accent, Quaternion.Euler(0f, 0f, -24f));
            CreatePart("Beast Ear R", PrimitiveType.Cylinder, new Vector3(0.22f, 0.48f, 0.68f), new Vector3(0.12f, 0.24f, 0.12f), accent, Quaternion.Euler(0f, 0f, 24f));
            CreatePart("Beast Tail", PrimitiveType.Cylinder, new Vector3(0f, 0.12f, -0.85f), new Vector3(0.12f, 0.42f, 0.12f), accent, Quaternion.Euler(-42f, 0f, 0f));
            CreateEyes(new Vector3(0f, 0.18f, 0.98f), new Color(1f, 0.82f, 0.2f));
        }

        private void BuildGuardian(Color bodyColor, Color accent)
        {
            var dark = Color.Lerp(bodyColor, Color.black, 0.3f);
            CreatePart("Guardian Core", PrimitiveType.Cube, new Vector3(0f, 0.02f, 0f), new Vector3(0.72f, 1.05f, 0.58f), bodyColor);
            CreatePart("Guardian Head", PrimitiveType.Cube, new Vector3(0f, 0.76f, 0f), new Vector3(0.52f, 0.44f, 0.5f), dark);
            CreatePart("Guardian Visor", PrimitiveType.Cube, new Vector3(0f, 0.78f, -0.27f), new Vector3(0.38f, 0.1f, 0.06f), accent);
            CreatePart("Guardian Shoulder L", PrimitiveType.Cube, new Vector3(-0.52f, 0.36f, 0f), new Vector3(0.28f, 0.3f, 0.62f), accent, Quaternion.Euler(0f, 0f, -12f));
            CreatePart("Guardian Shoulder R", PrimitiveType.Cube, new Vector3(0.52f, 0.36f, 0f), new Vector3(0.28f, 0.3f, 0.62f), accent, Quaternion.Euler(0f, 0f, 12f));
            CreatePart("Guardian Leg L", PrimitiveType.Cube, new Vector3(-0.22f, -0.68f, 0f), new Vector3(0.25f, 0.42f, 0.3f), dark);
            CreatePart("Guardian Leg R", PrimitiveType.Cube, new Vector3(0.22f, -0.68f, 0f), new Vector3(0.25f, 0.42f, 0.3f), dark);
            CreatePart("Guardian Core Gem", PrimitiveType.Sphere, new Vector3(0f, 0.05f, -0.34f), new Vector3(0.16f, 0.16f, 0.1f), Color.Lerp(accent, Color.white, 0.25f));
        }

        private void BuildSpirit(Color bodyColor, Color accent)
        {
            var dark = Color.Lerp(bodyColor, Color.black, 0.42f);
            CreatePart("Spirit Body", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0f), new Vector3(0.62f, 0.86f, 0.46f), bodyColor);
            CreatePart("Spirit Hood", PrimitiveType.Sphere, new Vector3(0f, 0.82f, 0f), new Vector3(0.48f, 0.42f, 0.48f), dark);
            CreatePart("Spirit Face", PrimitiveType.Cube, new Vector3(0f, 0.78f, -0.26f), new Vector3(0.3f, 0.1f, 0.06f), accent);
            CreatePart("Spirit Sash", PrimitiveType.Cube, new Vector3(0f, 0.05f, -0.38f), new Vector3(0.68f, 0.12f, 0.08f), accent);
            CreatePart("Spirit Orb", PrimitiveType.Sphere, new Vector3(0f, 0.36f, -0.5f), new Vector3(0.14f, 0.14f, 0.14f), Color.Lerp(accent, Color.white, 0.2f));
            CreatePart("Spirit Skirt", PrimitiveType.Cube, new Vector3(0f, -0.55f, 0f), new Vector3(0.72f, 0.3f, 0.52f), dark);
        }

        private void BuildShadow(Color bodyColor, Color accent)
        {
            var dark = Color.Lerp(bodyColor, Color.black, 0.5f);
            CreatePart("Shadow Body", PrimitiveType.Capsule, new Vector3(0f, 0f, 0f), new Vector3(0.58f, 0.86f, 0.44f), bodyColor);
            CreatePart("Shadow Hood", PrimitiveType.Sphere, new Vector3(0f, 0.88f, 0f), new Vector3(0.46f, 0.46f, 0.46f), dark);
            CreatePart("Shadow Cloak", PrimitiveType.Cube, new Vector3(0f, 0.02f, 0.28f), new Vector3(0.76f, 0.96f, 0.12f), dark, Quaternion.Euler(10f, 0f, 0f));
            CreatePart("Shadow Blade", PrimitiveType.Cube, new Vector3(0.48f, 0.1f, -0.08f), new Vector3(0.08f, 0.68f, 0.08f), accent, Quaternion.Euler(0f, 0f, -18f));
            CreatePart("Shadow Eye Line", PrimitiveType.Cube, new Vector3(0f, 0.9f, -0.36f), new Vector3(0.3f, 0.08f, 0.06f), accent);
        }

        private void BuildTierAdornment(EnemyTier tier, Color accent)
        {
            if (tier == EnemyTier.Normal)
            {
                return;
            }

            CreatePart("Tier Crest", PrimitiveType.Cylinder, new Vector3(0f, 1.05f, 0f), tier == EnemyTier.Boss ? new Vector3(0.36f, 0.08f, 0.36f) : new Vector3(0.28f, 0.06f, 0.28f), accent);

            if (tier == EnemyTier.Boss)
            {
                CreatePart("Boss Core", PrimitiveType.Sphere, new Vector3(0f, 1.34f, 0f), new Vector3(0.2f, 0.2f, 0.2f), Color.Lerp(accent, Color.white, 0.3f));
                CreatePart("Boss Horn L", PrimitiveType.Cylinder, new Vector3(-0.3f, 0.98f, 0f), new Vector3(0.1f, 0.3f, 0.1f), accent, Quaternion.Euler(0f, 0f, -28f));
                CreatePart("Boss Horn R", PrimitiveType.Cylinder, new Vector3(0.3f, 0.98f, 0f), new Vector3(0.1f, 0.3f, 0.1f), accent, Quaternion.Euler(0f, 0f, 28f));
            }
        }

        private void CreateEyes(Vector3 center, Color color)
        {
            CreatePart("Eye L", PrimitiveType.Sphere, center + new Vector3(-0.1f, 0f, 0.02f), new Vector3(0.06f, 0.06f, 0.04f), color);
            CreatePart("Eye R", PrimitiveType.Sphere, center + new Vector3(0.1f, 0f, 0.02f), new Vector3(0.06f, 0.06f, 0.04f), color);
        }

        private void BuildStatusDisplay(string displayName, EnemyTier tier, Color accent)
        {
            statusRoot = new GameObject("Enemy Status");
            statusRoot.transform.SetParent(transform, false);
            statusRoot.transform.localPosition = new Vector3(0f, 1.75f, 0f);

            var labelObject = new GameObject("Enemy Name");
            labelObject.transform.SetParent(statusRoot.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.18f, 0f);
            var label = labelObject.AddComponent<TextMesh>();
            label.text = displayName;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = tier == EnemyTier.Boss ? 34 : 28;
            label.characterSize = tier == EnemyTier.Boss ? 0.045f : 0.038f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = tier == EnemyTier.Normal ? Color.white : accent;

            var background = CreateStatusPart("Health Background", new Vector3(healthBarWidth, 0.075f, 0.035f), new Color(0.03f, 0.04f, 0.05f));
            background.transform.SetParent(statusRoot.transform, false);
            background.transform.localPosition = new Vector3(0f, -0.08f, 0f);

            var fill = CreateStatusPart("Health Fill", new Vector3(healthBarWidth, 0.055f, 0.045f), tier == EnemyTier.Boss ? new Color(1f, 0.38f, 0.12f) : tier == EnemyTier.Elite ? new Color(0.82f, 0.36f, 1f) : new Color(0.25f, 0.9f, 0.35f));
            fill.transform.SetParent(statusRoot.transform, false);
            fill.transform.localPosition = new Vector3(0f, -0.08f, -0.035f);
            healthFill = fill.transform;
        }

        private GameObject CreateStatusPart(string name, Vector3 size, Color color)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.localScale = size;
            RemoveCollider(part);
            part.GetComponent<Renderer>().sharedMaterial = MaterialFor(color);
            return part;
        }

        private GameObject CreatePart(string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color)
        {
            return CreatePart(name, primitive, position, scale, color, Quaternion.identity);
        }

        private GameObject CreatePart(string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color, Quaternion rotation)
        {
            var part = GameObject.CreatePrimitive(primitive);
            part.name = name;
            part.transform.SetParent(visualRoot.transform, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.transform.localScale = scale;
            RemoveCollider(part);
            part.GetComponent<Renderer>().sharedMaterial = MaterialFor(color);
            return part;
        }

        private static void RemoveCollider(GameObject part)
        {
            var collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }
        }

        private static Color AccentFor(EnemyTier tier, Color baseColor)
        {
            if (tier == EnemyTier.Boss)
            {
                return new Color(1f, 0.58f, 0.14f);
            }

            if (tier == EnemyTier.Elite)
            {
                return new Color(0.82f, 0.36f, 1f);
            }

            return Color.Lerp(baseColor, Color.white, 0.42f);
        }

        private static Material MaterialFor(Color color)
        {
            var shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
            return new Material(shader) { color = color };
        }
    }
}
