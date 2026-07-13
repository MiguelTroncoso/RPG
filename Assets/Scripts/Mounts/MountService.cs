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
        private readonly HashSet<string> ownedMountIds = new HashSet<string>();
        private MountDefinition selected;
        private GameObject mountVisual;

        public bool IsMounted { get; private set; }
        public string SelectedMountId => selected != null ? selected.MountId : string.Empty;
        public float SpeedMultiplier => IsMounted && selected != null ? selected.SpeedMultiplier : 1f;
        public int DamageBonus => IsMounted && selected != null ? selected.DamageBonus : 0;
        public int MaxHealthBonus => IsMounted && selected != null ? selected.MaxHealthBonus : 0;
        public float CritChanceBonus => IsMounted && selected != null ? selected.CritChanceBonus : 0f;

        public void Initialize(List<MountDefinition> definitions)
        {
            mounts.Clear();
            ownedMountIds.Clear();
            if (definitions != null)
            {
                foreach (var mount in definitions)
                {
                    if (mount != null && !string.IsNullOrWhiteSpace(mount.MountId))
                    {
                        mounts.Add(mount);
                        if (mount.StarterOwned)
                        {
                            ownedMountIds.Add(mount.MountId);
                        }
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
            if (mount != null && IsOwned(mountId))
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

            if (!IsOwned(selected.MountId))
            {
                Hud?.SetStatus(Localization.Tr("mount.locked"));
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

        public bool IsOwned(string mountId)
        {
            return !string.IsNullOrWhiteSpace(mountId) && ownedMountIds.Contains(mountId);
        }

        public List<string> ExportOwnedIds()
        {
            return new List<string>(ownedMountIds);
        }

        public void RestoreOwned(List<string> savedIds)
        {
            if (savedIds == null || savedIds.Count == 0)
            {
                return;
            }

            ownedMountIds.Clear();
            foreach (var id in savedIds)
            {
                if (Find(id) != null)
                {
                    ownedMountIds.Add(id);
                }
            }
        }

        public void UnlockMount(string mountId)
        {
            if (Find(mountId) != null)
            {
                ownedMountIds.Add(mountId);
            }
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

            var material = VisualMaterialUtility.Create(mount.BodyColor, false, 0.08f, 0.3f);
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
