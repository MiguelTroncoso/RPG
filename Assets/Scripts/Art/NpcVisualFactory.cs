using UnityEngine;

namespace MmorpgPrototype
{
    // Role-based NPC presentation for the offline beta. The KayKit humanoids
    // provide a readable body while the attached props make each service
    // identifiable before the final bespoke NPC meshes are authored.
    public static class NpcVisualFactory
    {
        public static void BuildMerchant(Transform parent)
        {
            var warmGold = new Color(0.82f, 0.43f, 0.12f);
            var cloth = new Color(0.12f, 0.28f, 0.34f);
            BuildHumanoid(parent, "ThirdParty/KayKit/Adventurers/Characters/Mage", "Merchant Body", warmGold, 0.92f);
            CreateCone(parent, "Merchant Hood Ornament", new Vector3(0f, 2.12f, 0f), 0.16f, 0.035f, 0.3f, warmGold, true);
            CreateEllipsoid(parent, "Merchant Satchel", new Vector3(-0.58f, 0.68f, -0.08f), new Vector3(0.34f, 0.42f, 0.22f), cloth);
            CreateEllipsoid(parent, "Merchant Coin Seal", new Vector3(-0.58f, 0.72f, -0.2f), new Vector3(0.12f, 0.12f, 0.04f), new Color(1f, 0.78f, 0.18f), true);
            CreateCone(parent, "Merchant Lantern", new Vector3(0.54f, 0.72f, 0.03f), 0.13f, 0.08f, 0.3f, new Color(0.95f, 0.54f, 0.12f), true);
            CreateStaff(parent, "Merchant Staff", new Vector3(0.64f, 0.84f, 0.04f), warmGold, new Color(0.3f, 0.92f, 0.8f));
        }

        public static void BuildBlacksmith(Transform parent)
        {
            var iron = new Color(0.22f, 0.27f, 0.34f);
            var leather = new Color(0.28f, 0.1f, 0.055f);
            var ember = new Color(1f, 0.25f, 0.06f);
            BuildHumanoid(parent, "ThirdParty/KayKit/Adventurers/Characters/Knight", "Blacksmith Body", iron, 0.98f);
            CreateEllipsoid(parent, "Blacksmith Apron", new Vector3(0f, 0.75f, -0.42f), new Vector3(0.58f, 0.62f, 0.08f), leather);
            CreatePart(parent, "Blacksmith Hammer Head", PrimitiveType.Cube, new Vector3(0.64f, 0.86f, -0.04f), new Vector3(0.28f, 0.16f, 0.2f), iron, Quaternion.Euler(0f, 0f, -18f));
            CreatePart(parent, "Blacksmith Hammer Grip", PrimitiveType.Cylinder, new Vector3(0.56f, 0.54f, -0.02f), new Vector3(0.06f, 0.42f, 0.06f), leather, Quaternion.Euler(0f, 0f, -18f));
            CreateCone(parent, "Blacksmith Ember", new Vector3(0f, 0.34f, -0.62f), 0.2f, 0.03f, 0.38f, ember, true);
            CreatePart(parent, "Blacksmith Anvil", PrimitiveType.Cube, new Vector3(0.95f, 0.22f, 0.02f), new Vector3(0.62f, 0.28f, 0.44f), iron, Quaternion.identity);
            CreatePart(parent, "Blacksmith Anvil Horn", PrimitiveType.Cylinder, new Vector3(1.3f, 0.3f, 0.02f), new Vector3(0.18f, 0.3f, 0.18f), iron, Quaternion.Euler(0f, 0f, 90f));
        }

        public static void BuildStorageKeeper(Transform parent)
        {
            var wood = new Color(0.32f, 0.16f, 0.07f);
            var brass = new Color(0.9f, 0.62f, 0.18f);
            var cloth = new Color(0.12f, 0.2f, 0.25f);
            BuildHumanoid(parent, "ThirdParty/KayKit/Adventurers/Characters/Barbarian", "Storage Keeper Body", cloth, 0.9f);
            CreatePart(parent, "Storage Chest", PrimitiveType.Cube, new Vector3(0.72f, 0.42f, 0.12f), new Vector3(0.82f, 0.5f, 0.58f), wood, Quaternion.Euler(0f, -8f, 0f));
            CreatePart(parent, "Storage Chest Lid", PrimitiveType.Cube, new Vector3(0.72f, 0.7f, 0.12f), new Vector3(0.86f, 0.1f, 0.62f), wood, Quaternion.Euler(0f, -8f, 0f));
            CreateEllipsoid(parent, "Storage Lock", new Vector3(0.72f, 0.49f, -0.19f), new Vector3(0.12f, 0.16f, 0.05f), brass, true);
            CreatePart(parent, "Storage Key", PrimitiveType.Cylinder, new Vector3(-0.6f, 0.86f, -0.28f), new Vector3(0.05f, 0.28f, 0.05f), brass, Quaternion.Euler(70f, 0f, 12f));
        }

        private static GameObject BuildHumanoid(Transform parent, string resource, string name, Color tint, float scale)
        {
            var prefab = Resources.Load<GameObject>(resource);
            if (prefab == null)
            {
                return null;
            }

            var model = Object.Instantiate(prefab, parent);
            model.name = name;
            RemoveImportArtifacts(model);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one * scale;
            foreach (var collider in model.GetComponentsInChildren<Collider>())
            {
                Object.Destroy(collider);
            }

            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                // Keep the authored KayKit texture. Applying a broad runtime
                // tint made every role look like the same silver mannequin.
                renderer.receiveShadows = true;
            }

            return model;
        }

        private static void RemoveImportArtifacts(GameObject model)
        {
            if (model == null)
            {
                return;
            }

            foreach (var child in model.GetComponentsInChildren<Transform>(true))
            {
                if (child == model.transform)
                {
                    continue;
                }

                var name = child.name;
                if (name == "Cube" || name.StartsWith("Cube."))
                {
                    Object.Destroy(child.gameObject);
                }
            }
        }

        private static void CreateStaff(Transform parent, string name, Vector3 position, Color wood, Color crystal)
        {
            CreatePart(parent, name + " Shaft", PrimitiveType.Cylinder, position, new Vector3(0.06f, 0.58f, 0.06f), wood, Quaternion.Euler(0f, 0f, -12f));
            CreateEllipsoid(parent, name + " Crystal", position + new Vector3(0.12f, 0.42f, 0f), new Vector3(0.22f, 0.22f, 0.22f), crystal, true);
        }

        private static GameObject CreateCone(Transform parent, string name, Vector3 position, float bottomRadius, float topRadius, float height, Color color, bool emissive = false)
        {
            return RuntimeArtMeshFactory.CreateCone(parent, name, position, bottomRadius, topRadius, height, color, 8, 0f, emissive, 0.12f, 0.34f);
        }

        private static GameObject CreateEllipsoid(Transform parent, string name, Vector3 position, Vector3 size, Color color, bool emissive = false)
        {
            return RuntimeArtMeshFactory.CreateEllipsoid(parent, name, position, size, color, 9, 4, emissive, 0.1f, 0.34f);
        }

        private static GameObject CreatePart(Transform parent, string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color, Quaternion rotation)
        {
            var part = GameObject.CreatePrimitive(primitive);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.transform.localScale = scale;
            var collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }
            part.GetComponent<Renderer>().sharedMaterial = VisualMaterialUtility.Create(color, VisualMaterialUtility.ShouldGlow(name), 0.18f, 0.38f);
            return part;
        }
    }
}
