using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Invocacion de mascotas: una activa a la vez. Aplica los bonos pasivos
    // como multiplicadores en PlayerProgression y crea el visual procedural.
    public sealed class PetService : MonoBehaviour
    {
        public PrototypeHud Hud;
        public PlayerProgression Progression;

        private readonly List<PetDefinition> pets = new List<PetDefinition>();
        private PetDefinition active;
        private GameObject petVisual;

        public bool HasActive => active != null;
        public string ActivePetId => active != null ? active.PetId : string.Empty;

        public void Initialize(List<PetDefinition> definitions)
        {
            pets.Clear();
            if (definitions != null)
            {
                foreach (var pet in definitions)
                {
                    if (pet != null && !string.IsNullOrWhiteSpace(pet.PetId))
                    {
                        pets.Add(pet);
                    }
                }
            }
        }

        public void ToggleDefault()
        {
            if (HasActive)
            {
                Dismiss();
                return;
            }

            if (pets.Count == 0)
            {
                Hud?.SetStatus("No tienes mascotas.");
                return;
            }

            Summon(pets[0].PetId);
        }

        public void Summon(string petId)
        {
            var pet = Find(petId);
            if (pet == null)
            {
                return;
            }

            Dismiss(silent: true);
            active = pet;
            CreateVisual(pet);
            ApplyBonuses();
            Hud?.SetStatus($"{pet.DisplayName} te acompana (+{pet.ExpBonusPercent:0}% EXP, +{pet.GoldBonusPercent:0}% oro).", 4f);
            Hud?.AddFeed($"Mascota invocada: {pet.DisplayName}");
        }

        public void Dismiss(bool silent = false)
        {
            if (!HasActive)
            {
                return;
            }

            var name = active.DisplayName;
            active = null;

            if (petVisual != null)
            {
                Destroy(petVisual);
                petVisual = null;
            }

            ApplyBonuses();

            if (!silent)
            {
                Hud?.SetStatus($"{name} descansa.");
                Hud?.AddFeed($"Mascota guardada: {name}");
            }
        }

        private void ApplyBonuses()
        {
            if (Progression == null)
            {
                return;
            }

            Progression.ExperienceMultiplier = active != null ? 1f + active.ExpBonusPercent / 100f : 1f;
            Progression.GoldMultiplier = active != null ? 1f + active.GoldBonusPercent / 100f : 1f;
        }

        private void CreateVisual(PetDefinition pet)
        {
            petVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            petVisual.name = $"Pet - {pet.DisplayName}";
            petVisual.transform.localScale = Vector3.one * pet.Scale;
            petVisual.transform.position = transform.position + new Vector3(-1.2f, 0.45f, -0.8f);

            var collider = petVisual.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var material = new Material(Shader.Find("Standard"));
            material.color = pet.BodyColor;
            petVisual.GetComponent<Renderer>().sharedMaterial = material;

            var controller = petVisual.AddComponent<PetController>();
            controller.Target = transform;
            controller.FollowDistance = pet.FollowDistance;

            var labelObject = new GameObject("Pet Label");
            labelObject.transform.SetParent(petVisual.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            var label = labelObject.AddComponent<TextMesh>();
            label.text = pet.DisplayName;
            label.fontSize = 34;
            label.characterSize = 0.05f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = new Color(1f, 0.9f, 0.7f);
        }

        private PetDefinition Find(string petId)
        {
            foreach (var pet in pets)
            {
                if (pet.PetId == petId)
                {
                    return pet;
                }
            }

            return null;
        }
    }
}
