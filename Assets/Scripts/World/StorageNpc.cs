using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class StorageNpc : MonoBehaviour, IInteractable
    {
        public Transform Player;
        public StorageService Storage;
        public PrototypeHud Hud;
        public float InteractionRange = 4f;

        public void Interact()
        {
            ToggleStorage();
        }

        public void ToggleStorage()
        {
            if (Player == null || Storage == null)
            {
                return;
            }

            if (Vector3.Distance(transform.position, Player.position) > InteractionRange)
            {
                Hud?.SetStatus(Localization.Tr("storage.too_far"));
                return;
            }

            Storage.Toggle();
        }
    }
}
