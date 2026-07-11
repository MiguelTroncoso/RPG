using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Montar/desmontar con requisito de nivel. El multiplicador de velocidad
    // se aplica en el recomputo central de stats (ApplyBonuses), no aqui.
    public sealed class MountService : MonoBehaviour
    {
        public PrototypeHud Hud;
        public PlayerProgression Progression;
        public EquipmentUpgradeSystem UpgradeSystem;

        private readonly List<MountDefinition> mounts = new List<MountDefinition>();
        private MountDefinition selected;
        private GameObject mountVisual;

        public bool IsMounted { get; private set; }
        public string SelectedMountId => selected != null ? selected.MountId : string.Empty;
        public float SpeedMultiplier => IsMounted && selected != null ? selected.SpeedMultiplier : 1f;

        public void Initialize(List<MountDefinition> definitions)
        {
            mounts.Clear();
            if (definitions != null)
            {
                foreach (var mount in definitions)
                {
                    if (mount != null && !string.IsNullOrWhiteSpace(mount.MountId))
                    {
                        mounts.Add(mount);
                    }
                }
            }

            if (selected == null && mounts.Count > 0)
            {
                selected = mounts[0];
            }
        }

        public void SelectMount(string mountId)
        {
            var mount = Find(mountId);
            if (mount != null)
            {
                selected = mount;
            }
        }

        public void ToggleMount()
        {
            if (IsMounted)
            {
                Dismount();
                return;
            }

            if (selected == null)
            {
                Hud?.SetStatus(Localization.Tr("mount.none"));
                return;
            }

            if (Progression != null && Progression.Level < selected.RequiredLevel)
            {
                Hud?.SetStatus(Localization.Tr("mount.need_level", selected.RequiredLevel, selected.DisplayName));
                return;
            }

            IsMounted = true;
            CreateVisual(selected);
            UpgradeSystem?.ApplyBonuses();
            Hud?.SetStatus(Localization.Tr("mount.mounted", selected.DisplayName, selected.SpeedMultiplier.ToString("0.0")), 3.5f);
            Hud?.AddFeed(Localization.Tr("mount.feed", selected.DisplayName));
        }

        public void Dismount(bool silent = false)
        {
            if (!IsMounted)
            {
                return;
            }

            IsMounted = false;

            if (mountVisual != null)
            {
                Destroy(mountVisual);
                mountVisual = null;
            }

            UpgradeSystem?.ApplyBonuses();

            if (!silent && selected != null)
            {
                Hud?.SetStatus(Localization.Tr("mount.dismounted", selected.DisplayName));
            }
        }

        private void CreateVisual(MountDefinition mount)
        {
            mountVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mountVisual.name = $"Mount - {mount.DisplayName}";
            mountVisual.transform.SetParent(transform, false);
            mountVisual.transform.localPosition = new Vector3(0f, -0.55f, 0.1f);
            mountVisual.transform.localScale = new Vector3(0.55f, 0.5f, 1.7f);

            var collider = mountVisual.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var material = new Material(Shader.Find("Standard"));
            material.color = mount.BodyColor;
            mountVisual.GetComponent<Renderer>().sharedMaterial = material;
        }

        private MountDefinition Find(string mountId)
        {
            foreach (var mount in mounts)
            {
                if (mount.MountId == mountId)
                {
                    return mount;
                }
            }

            return null;
        }
    }
}
