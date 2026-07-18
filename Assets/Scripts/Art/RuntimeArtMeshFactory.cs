using System;
using UnityEngine;

namespace MmorpgPrototype
{
    // Small authored meshes for the mobile beta. They keep the silhouette and
    // material language coherent without requiring a runtime model importer.
    public static class RuntimeArtMeshFactory
    {
        public static GameObject CreateCone(
            Transform parent,
            string name,
            Vector3 localPosition,
            float bottomRadius,
            float topRadius,
            float height,
            Color color,
            int sides = 8,
            float rotationDegrees = 0f,
            bool emissive = false,
            float metallic = 0.12f,
            float smoothness = 0.32f)
        {
            var mesh = new Mesh
            {
                name = name + " Mesh"
            };

            var sideCount = Mathf.Max(5, sides);
            var vertices = new Vector3[(sideCount + 1) * 2];
            var triangles = new int[sideCount * 6];
            var rotation = rotationDegrees * Mathf.Deg2Rad;

            for (var ring = 0; ring < 2; ring++)
            {
                var radius = ring == 0 ? bottomRadius : topRadius;
                var y = ring == 0 ? -height * 0.5f : height * 0.5f;
                for (var side = 0; side <= sideCount; side++)
                {
                    var angle = rotation + Mathf.PI * 2f * side / sideCount;
                    vertices[ring * (sideCount + 1) + side] = new Vector3(
                        Mathf.Cos(angle) * radius,
                        y,
                        Mathf.Sin(angle) * radius);
                }
            }

            var triangleIndex = 0;
            for (var side = 0; side < sideCount; side++)
            {
                var next = side + 1;
                triangles[triangleIndex++] = side;
                triangles[triangleIndex++] = sideCount + 1 + side;
                triangles[triangleIndex++] = next;
                triangles[triangleIndex++] = next;
                triangles[triangleIndex++] = sideCount + 1 + side;
                triangles[triangleIndex++] = sideCount + 1 + next;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return CreateMeshObject(parent, name, localPosition, Quaternion.identity, mesh, color, emissive, metallic, smoothness);
        }

        public static GameObject CreateEllipsoid(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 size,
            Color color,
            int sides = 10,
            int rings = 5,
            bool emissive = false,
            float metallic = 0.08f,
            float smoothness = 0.3f)
        {
            var mesh = new Mesh
            {
                name = name + " Mesh"
            };

            var sideCount = Mathf.Max(6, sides);
            var ringCount = Mathf.Max(3, rings);
            var vertices = new Vector3[(ringCount + 1) * (sideCount + 1)];
            var triangles = new int[ringCount * sideCount * 6];
            for (var ring = 0; ring <= ringCount; ring++)
            {
                var vertical = ring / (float)ringCount;
                var latitude = -Mathf.PI * 0.5f + Mathf.PI * vertical;
                var ringRadius = Mathf.Cos(latitude);
                var y = Mathf.Sin(latitude);
                for (var side = 0; side <= sideCount; side++)
                {
                    var longitude = Mathf.PI * 2f * side / sideCount;
                    vertices[ring * (sideCount + 1) + side] = new Vector3(
                        Mathf.Cos(longitude) * ringRadius * size.x * 0.5f,
                        y * size.y * 0.5f,
                        Mathf.Sin(longitude) * ringRadius * size.z * 0.5f);
                }
            }

            var triangleIndex = 0;
            for (var ring = 0; ring < ringCount; ring++)
            {
                for (var side = 0; side < sideCount; side++)
                {
                    var current = ring * (sideCount + 1) + side;
                    var next = current + 1;
                    var upper = current + sideCount + 1;
                    var upperNext = upper + 1;
                    triangles[triangleIndex++] = current;
                    triangles[triangleIndex++] = upper;
                    triangles[triangleIndex++] = next;
                    triangles[triangleIndex++] = next;
                    triangles[triangleIndex++] = upper;
                    triangles[triangleIndex++] = upperNext;
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return CreateMeshObject(parent, name, localPosition, Quaternion.identity, mesh, color, emissive, metallic, smoothness);
        }

        public static GameObject CreateBlade(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 size,
            Color color,
            Quaternion localRotation,
            bool emissive = false,
            float metallic = 0.7f,
            float smoothness = 0.58f)
        {
            var width = Mathf.Max(0.02f, size.x);
            var height = Mathf.Max(0.04f, size.y);
            var depth = Mathf.Max(0.012f, size.z);
            var points = new[]
            {
                new Vector2(-width * 0.34f, -height * 0.5f),
                new Vector2(width * 0.34f, -height * 0.5f),
                new Vector2(width * 0.5f, height * 0.18f),
                new Vector2(0f, height * 0.5f),
                new Vector2(-width * 0.5f, height * 0.18f)
            };

            var vertices = new Vector3[points.Length * 2];
            for (var i = 0; i < points.Length; i++)
            {
                vertices[i] = new Vector3(points[i].x, points[i].y, -depth * 0.5f);
                vertices[i + points.Length] = new Vector3(points[i].x, points[i].y, depth * 0.5f);
            }

            var triangles = new int[(points.Length - 2) * 6 + points.Length * 6];
            var triangleIndex = 0;
            for (var i = 1; i < points.Length - 1; i++)
            {
                triangles[triangleIndex++] = 0;
                triangles[triangleIndex++] = i;
                triangles[triangleIndex++] = i + 1;
                triangles[triangleIndex++] = points.Length;
                triangles[triangleIndex++] = points.Length + i + 1;
                triangles[triangleIndex++] = points.Length + i;
            }

            for (var i = 0; i < points.Length; i++)
            {
                var next = (i + 1) % points.Length;
                triangles[triangleIndex++] = i;
                triangles[triangleIndex++] = next;
                triangles[triangleIndex++] = points.Length + i;
                triangles[triangleIndex++] = next;
                triangles[triangleIndex++] = points.Length + next;
                triangles[triangleIndex++] = points.Length + i;
            }

            var mesh = new Mesh
            {
                name = name + " Mesh",
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return CreateMeshObject(parent, name, localPosition, localRotation, mesh, color, emissive, metallic, smoothness);
        }

        private static GameObject CreateMeshObject(
            Transform parent,
            string name,
            Vector3 localPosition,
            Quaternion localRotation,
            Mesh mesh,
            Color color,
            bool emissive,
            float metallic,
            float smoothness)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;

            var filter = part.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = VisualMaterialUtility.Create(color, emissive, metallic, smoothness);
            renderer.receiveShadows = true;
            return part;
        }
    }
}
