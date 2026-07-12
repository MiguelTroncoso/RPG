using UnityEngine;

namespace MmorpgPrototype
{
    // Bridges data-driven equipment changes to the avatar presentation. The
    // stat calculation remains inside PlayerEquipment/EquipmentUpgradeSystem.
    public sealed class EquipmentVisualController : MonoBehaviour
    {
        public PlayerEquipment Equipment;
        public PlayerAvatarVisual Avatar;

        private bool subscribed;

        public void Initialize()
        {
            Unsubscribe();
            if (Equipment == null)
            {
                Equipment = GetComponent<PlayerEquipment>();
            }

            if (Avatar == null)
            {
                Avatar = GetComponent<PlayerAvatarVisual>();
            }

            if (Equipment != null)
            {
                Equipment.Changed += Refresh;
                subscribed = true;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (subscribed && Equipment != null)
            {
                Equipment.Changed -= Refresh;
            }

            subscribed = false;
        }

        private void Refresh()
        {
            Avatar?.RefreshEquipmentVisuals(Equipment);
        }
    }
}
