using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    [RequireComponent(typeof(Health))]
    public sealed class HitFlashOnDamage : MonoBehaviour
    {
        public Color FlashColor = new Color(1f, 0.32f, 0.22f);
        public float FlashSeconds = 0.12f;

        private Health health;
        private Renderer[] renderers;
        private Color[] originalColors;
        private float flashUntil;

        private void Awake()
        {
            health = GetComponent<Health>();
            CaptureRenderers();
        }

        private void OnEnable()
        {
            health.Damaged += HandleDamaged;
        }

        private void OnDisable()
        {
            health.Damaged -= HandleDamaged;
        }

        private void Update()
        {
            if (health.IsDead || flashUntil <= 0f || Time.time < flashUntil)
            {
                return;
            }

            RestoreColors();
            flashUntil = 0f;
        }

        private void HandleDamaged(Health _, int __)
        {
            var currentRenderers = CollectFlashRenderers();
            if (renderers == null || currentRenderers.Count != renderers.Length)
            {
                CaptureRenderers(currentRenderers);
            }

            flashUntil = Time.time + FlashSeconds;
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].material.color = FlashColor;
                }
            }
        }

        private void CaptureRenderers()
        {
            CaptureRenderers(CollectFlashRenderers());
        }

        private void CaptureRenderers(List<Renderer> captured)
        {
            renderers = captured.ToArray();
            originalColors = new Color[renderers.Length];

            for (var i = 0; i < renderers.Length; i++)
            {
                originalColors[i] = renderers[i].material.color;
            }
        }

        private List<Renderer> CollectFlashRenderers()
        {
            var captured = new List<Renderer>();
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                if (renderer.GetComponent<TextMesh>() == null)
                {
                    captured.Add(renderer);
                }
            }

            return captured;
        }

        private void RestoreColors()
        {
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].material.color = originalColors[i];
                }
            }
        }
    }
}
