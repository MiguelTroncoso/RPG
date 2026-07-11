using UnityEngine;

namespace MmorpgPrototype
{
    // Herrero del campamento: las mejoras de equipo se hacen cerca de el.
    public sealed class BlacksmithNpc : MonoBehaviour, IInteractable
    {
        public string NpcId = DefaultQuests.BlacksmithNpcId;
        public Transform Player;
        public EquipmentUpgradeSystem Equipment;
        public PlayerQuestLog QuestLog;
        public PrototypeHud Hud;
        public float InteractionRange = 4f;

        public void Interact()
        {
            Talk();
        }

        public void Talk()
        {
            if (!IsPlayerNear())
            {
                return;
            }

            var dialog = QuestLog != null ? QuestLog.DialogFor(NpcId) : null;
            Hud?.SetStatus(dialog ?? Localization.Tr("smith.dialog"), 5f);
            QuestLog?.OnNpcTalked(NpcId);
        }

        public void UpgradeWeapon()
        {
            if (IsPlayerNear())
            {
                Equipment?.TryUpgradeWeapon();
            }
        }

        public void UpgradeArmor()
        {
            if (IsPlayerNear())
            {
                Equipment?.TryUpgradeArmor();
            }
        }

        private bool IsPlayerNear()
        {
            if (Player == null)
            {
                return false;
            }

            if (Vector3.Distance(transform.position, Player.position) <= InteractionRange)
            {
                return true;
            }

            Hud?.SetStatus(Localization.Tr("smith.too_far"));
            return false;
        }
    }
}
