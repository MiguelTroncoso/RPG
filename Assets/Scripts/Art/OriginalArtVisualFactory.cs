using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Original low-poly reference art for the first production visual pass.
    // The meshes are generated once per spawned reference and use no external
    // geometry, textures or colliders.
    public static class OriginalArtVisualFactory
    {
        private static readonly Dictionary<int, Material> Materials = new Dictionary<int, Material>();

        public static GameObject BuildWarriorMale(Transform parent, CharacterArtProfile profile)
        {
            if (parent == null || profile == null || profile.ClassType != CharacterClassType.Guerrero || profile.Gender != CharacterGender.Masculino)
            {
                return null;
            }

            var root = new GameObject("Original Warrior Male");
            root.transform.SetParent(parent, false);

            var armor = profile.MetalColor;
            var leather = Color.Lerp(profile.SecondaryColor, Color.black, 0.12f);
            var body = Color.Lerp(profile.PrimaryColor, Color.white, 0.14f);
            var glow = profile.GlowColor;

            CreatePart(root.transform, "Torso Armor", Frustum(0.52f, 0.38f, 0.78f, 8), new Vector3(0f, 0.02f, 0f), Vector3.one, Quaternion.identity, armor, false);
            CreatePart(root.transform, "Chest Inlay", Frustum(0.18f, 0.1f, 0.38f, 6), new Vector3(0f, 0.12f, -0.38f), new Vector3(1f, 0.9f, 0.3f), Quaternion.Euler(90f, 0f, 0f), glow, true);
            CreatePart(root.transform, "Head", LowPolySphere(0.34f, 8, 5), new Vector3(0f, 0.86f, 0f), Vector3.one, Quaternion.identity, body, false);
            CreatePart(root.transform, "Helmet Crown", Frustum(0.38f, 0.26f, 0.24f, 8), new Vector3(0f, 1.12f, 0f), Vector3.one, Quaternion.identity, armor, false);
            CreatePart(root.transform, "Helmet Crest", Frustum(0.12f, 0.035f, 0.36f, 4), new Vector3(0f, 1.38f, -0.02f), Vector3.one, Quaternion.Euler(0f, 0f, 45f), glow, true);

            CreatePart(root.transform, "Left Shoulder", LowPolySphere(0.28f, 8, 4), new Vector3(-0.54f, 0.42f, 0f), new Vector3(1.25f, 0.7f, 1.1f), Quaternion.identity, armor, false);
            CreatePart(root.transform, "Right Shoulder", LowPolySphere(0.28f, 8, 4), new Vector3(0.54f, 0.42f, 0f), new Vector3(1.25f, 0.7f, 1.1f), Quaternion.identity, armor, false);
            CreatePart(root.transform, "Left Arm", Frustum(0.16f, 0.12f, 0.62f, 6), new Vector3(-0.45f, 0.08f, 0f), Vector3.one, Quaternion.Euler(0f, 0f, 16f), leather, false);
            CreatePart(root.transform, "Right Arm", Frustum(0.16f, 0.12f, 0.62f, 6), new Vector3(0.45f, 0.08f, 0f), Vector3.one, Quaternion.Euler(0f, 0f, -16f), leather, false);
            CreatePart(root.transform, "Left Leg", Frustum(0.2f, 0.15f, 0.62f, 6), new Vector3(-0.2f, -0.57f, 0f), Vector3.one, Quaternion.identity, leather, false);
            CreatePart(root.transform, "Right Leg", Frustum(0.2f, 0.15f, 0.62f, 6), new Vector3(0.2f, -0.57f, 0f), Vector3.one, Quaternion.identity, leather, false);
            CreatePart(root.transform, "Left Boot", Frustum(0.24f, 0.18f, 0.24f, 6), new Vector3(-0.2f, -0.89f, -0.08f), Vector3.one, Quaternion.identity, armor, false);
            CreatePart(root.transform, "Right Boot", Frustum(0.24f, 0.18f, 0.24f, 6), new Vector3(0.2f, -0.89f, -0.08f), Vector3.one, Quaternion.identity, armor, false);
            CreatePart(root.transform, "Belt", Frustum(0.48f, 0.44f, 0.12f, 8), new Vector3(0f, -0.28f, 0f), Vector3.one, Quaternion.identity, glow, true);
            CreatePart(root.transform, "Back Mantle", CapeMesh(), new Vector3(0f, 0.02f, 0.34f), Vector3.one, Quaternion.Euler(8f, 0f, 0f), leather, false);

            BuildOriginalSword(root.transform, armor, glow);
            return root;
        }

        public static GameObject BuildValleyMob(Transform parent, Color baseColor, Color accent)
        {
            if (parent == null)
            {
                return null;
            }

            var root = new GameObject("Original Relic Hound");
            root.transform.SetParent(parent, false);

            var hide = Color.Lerp(baseColor, new Color(0.08f, 0.035f, 0.025f), 0.34f);
            var bone = new Color(0.62f, 0.42f, 0.2f);
            var corruption = new Color(1f, 0.24f, 0.08f);

            CreatePart(root.transform, "Hound Body", LowPolySphere(0.52f, 10, 5), new Vector3(0f, 0.06f, 0f), new Vector3(1.1f, 0.72f, 1.55f), Quaternion.Euler(90f, 0f, 0f), hide, false);
            CreatePart(root.transform, "Hound Head", LowPolySphere(0.38f, 10, 5), new Vector3(0f, 0.22f, 0.64f), new Vector3(1.05f, 0.92f, 1.08f), Quaternion.identity, hide, false);
            CreatePart(root.transform, "Hound Muzzle", Frustum(0.22f, 0.12f, 0.32f, 6), new Vector3(0f, 0.12f, 0.94f), new Vector3(1f, 0.8f, 1f), Quaternion.Euler(90f, 0f, 0f), bone, false);
            CreatePart(root.transform, "Hound Ear Left", Frustum(0.14f, 0.035f, 0.42f, 5), new Vector3(-0.25f, 0.58f, 0.56f), Vector3.one, Quaternion.Euler(0f, 0f, -22f), bone, false);
            CreatePart(root.transform, "Hound Ear Right", Frustum(0.14f, 0.035f, 0.42f, 5), new Vector3(0.25f, 0.58f, 0.56f), Vector3.one, Quaternion.Euler(0f, 0f, 22f), bone, false);
            CreatePart(root.transform, "Hound Leg Front Left", Frustum(0.16f, 0.11f, 0.62f, 6), new Vector3(-0.3f, -0.42f, 0.38f), Vector3.one, Quaternion.identity, hide, false);
            CreatePart(root.transform, "Hound Leg Front Right", Frustum(0.16f, 0.11f, 0.62f, 6), new Vector3(0.3f, -0.42f, 0.38f), Vector3.one, Quaternion.identity, hide, false);
            CreatePart(root.transform, "Hound Leg Back Left", Frustum(0.16f, 0.11f, 0.62f, 6), new Vector3(-0.3f, -0.42f, -0.38f), Vector3.one, Quaternion.identity, hide, false);
            CreatePart(root.transform, "Hound Leg Back Right", Frustum(0.16f, 0.11f, 0.62f, 6), new Vector3(0.3f, -0.42f, -0.38f), Vector3.one, Quaternion.identity, hide, false);
            CreatePart(root.transform, "Hound Tail", Frustum(0.13f, 0.035f, 0.78f, 6), new Vector3(0f, 0.22f, -0.76f), Vector3.one, Quaternion.Euler(-48f, 0f, 0f), bone, false);
            CreatePart(root.transform, "Relic Scar", Frustum(0.12f, 0.035f, 0.48f, 5), new Vector3(0f, 0.38f, 0.86f), Vector3.one, Quaternion.Euler(90f, 0f, 0f), corruption, true);
            CreatePart(root.transform, "Relic Spine", Frustum(0.11f, 0.025f, 0.48f, 5), new Vector3(0f, 0.48f, 0.02f), Vector3.one, Quaternion.Euler(0f, 0f, 90f), accent, true);
            CreatePart(root.transform, "Eye Left", LowPolySphere(0.055f, 6, 3), new Vector3(-0.13f, 0.28f, 0.91f), Vector3.one, Quaternion.identity, corruption, true);
            CreatePart(root.transform, "Eye Right", LowPolySphere(0.055f, 6, 3), new Vector3(0.13f, 0.28f, 0.91f), Vector3.one, Quaternion.identity, corruption, true);
            return root;
        }

        private static void BuildOriginalSword(Transform parent, Color metal, Color glow)
        {
            CreatePart(parent, "Original Sword Blade", Frustum(0.11f, 0.035f, 0.92f, 4), new Vector3(0.52f, 0.12f, -0.18f), Vector3.one, Quaternion.Euler(0f, 0f, -26f), metal, false);
            CreatePart(parent, "Original Sword Guard", Frustum(0.12f, 0.12f, 0.08f, 6), new Vector3(0.4f, -0.28f, -0.18f), new Vector3(1.5f, 1f, 0.7f), Quaternion.Euler(0f, 0f, -26f), glow, true);
            CreatePart(parent, "Original Sword Grip", Frustum(0.055f, 0.055f, 0.28f, 6), new Vector3(0.3f, -0.39f, -0.18f), Vector3.one, Quaternion.Euler(0f, 0f, -26f), new Color(0.12f, 0.07f, 0.045f), false);
        }

        private static void CreatePart(Transform parent, string name, Mesh mesh, Vector3 position, Vector3 scale, Quaternion rotation, Color color, bool glow)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.transform.localScale = scale;
            var filter = part.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = MaterialFor(color, glow);
        }

        private static Mesh LowPolySphere(float radius, int segments, int rings)
        {
            var vertices = new Vector3[(rings + 1) * segments];
            var triangles = new List<int>(rings * segments * 6);
            for (var ring = 0; ring <= rings; ring++)
            {
                var phi = Mathf.PI * ring / rings;
                var y = Mathf.Cos(phi) * radius;
                var ringRadius = Mathf.Sin(phi) * radius;
                for (var segment = 0; segment < segments; segment++)
                {
                    var theta = Mathf.PI * 2f * segment / segments;
                    vertices[ring * segments + segment] = new Vector3(Mathf.Cos(theta) * ringRadius, y, Mathf.Sin(theta) * ringRadius);
                }
            }

            for (var ring = 0; ring < rings; ring++)
            {
                for (var segment = 0; segment < segments; segment++)
                {
                    var next = (segment + 1) % segments;
                    var current = ring * segments + segment;
                    var currentNext = ring * segments + next;
                    var upper = (ring + 1) * segments + segment;
                    var upperNext = (ring + 1) * segments + next;
                    triangles.Add(current);
                    triangles.Add(upper);
                    triangles.Add(currentNext);
                    triangles.Add(currentNext);
                    triangles.Add(upper);
                    triangles.Add(upperNext);
                }
            }

            return CreateMesh(vertices, triangles.ToArray());
        }

        private static Mesh Frustum(float bottomRadius, float topRadius, float height, int segments)
        {
            var vertices = new Vector3[segments * 2 + 2];
            var triangles = new List<int>(segments * 12);
            for (var segment = 0; segment < segments; segment++)
            {
                var angle = Mathf.PI * 2f * segment / segments;
                var x = Mathf.Cos(angle);
                var z = Mathf.Sin(angle);
                vertices[segment] = new Vector3(x * bottomRadius, -height * 0.5f, z * bottomRadius);
                vertices[segments + segment] = new Vector3(x * topRadius, height * 0.5f, z * topRadius);
            }

            var bottomCenter = segments * 2;
            var topCenter = bottomCenter + 1;
            vertices[bottomCenter] = new Vector3(0f, -height * 0.5f, 0f);
            vertices[topCenter] = new Vector3(0f, height * 0.5f, 0f);
            for (var segment = 0; segment < segments; segment++)
            {
                var next = (segment + 1) % segments;
                triangles.Add(segment);
                triangles.Add(segments + segment);
                triangles.Add(next);
                triangles.Add(next);
                triangles.Add(segments + segment);
                triangles.Add(segments + next);
                triangles.Add(bottomCenter);
                triangles.Add(next);
                triangles.Add(segment);
                triangles.Add(topCenter);
                triangles.Add(segments + segment);
                triangles.Add(segments + next);
            }

            return CreateMesh(vertices, triangles.ToArray());
        }

        private static Mesh CapeMesh()
        {
            var vertices = new[]
            {
                new Vector3(-0.45f, 0.42f, 0f), new Vector3(0.45f, 0.42f, 0f),
                new Vector3(0.32f, -0.5f, 0.05f), new Vector3(-0.32f, -0.5f, 0.05f),
                new Vector3(-0.45f, 0.42f, 0.08f), new Vector3(0.45f, 0.42f, 0.08f),
                new Vector3(0.32f, -0.5f, 0.13f), new Vector3(-0.32f, -0.5f, 0.13f)
            };
            var triangles = new[]
            {
                0, 1, 2, 0, 2, 3, 4, 6, 5, 4, 7, 6,
                0, 4, 5, 0, 5, 1, 3, 2, 6, 3, 6, 7,
                0, 3, 7, 0, 7, 4, 1, 5, 6, 1, 6, 2
            };
            return CreateMesh(vertices, triangles);
        }

        private static Mesh CreateMesh(Vector3[] vertices, int[] triangles)
        {
            var mesh = new Mesh();
            mesh.name = "Original Low Poly Mesh";
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Material MaterialFor(Color color, bool glow)
        {
            var rgba = (Color32)color;
            var key = rgba.r << 25 | rgba.g << 17 | rgba.b << 9 | rgba.a << 1 | (glow ? 1 : 0);
            if (Materials.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var material = VisualMaterialUtility.Create(color, glow, glow ? 0.16f : 0.08f, glow ? 0.5f : 0.38f);
            Materials[key] = material;
            return material;
        }
    }
}
