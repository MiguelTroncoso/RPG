using UnityEngine;

namespace MmorpgPrototype
{
    public enum ZonePointOfInterestType
    {
        Entry,
        SafeCommerce,
        Elite,
        Boss
    }

    public sealed class ZonePointOfInterest : MonoBehaviour
    {
        public string DisplayName;
        public string InteractionMessage;
        public float InteractDistance = 8f;
        public ZonePointOfInterestType Type;
        public RewardBundle Reward;
        public string ClaimId;

        private bool rewardClaimed;

        public static void InteractNearest(Transform actor)
        {
            var hud = FindAnyObjectByType<PrototypeHud>();
            if (actor == null)
            {
                hud?.SetStatus(Localization.Tr("poi.no_target"));
                return;
            }

            var nearest = FindNearest(actor.position);
            if (nearest == null)
            {
                hud?.SetStatus(Localization.Tr("poi.no_target"));
                return;
            }

            nearest.Interact(actor, hud);
        }

        private static ZonePointOfInterest FindNearest(Vector3 position)
        {
            var points = FindObjectsByType<ZonePointOfInterest>();
            ZonePointOfInterest nearest = null;
            var nearestDistance = float.MaxValue;

            foreach (var point in points)
            {
                if (point == null)
                {
                    continue;
                }

                var distance = (point.transform.position - position).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearest = point;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private void Interact(Transform actor, PrototypeHud hud)
        {
            var distance = Vector3.Distance(actor.position, transform.position);
            if (distance > InteractDistance)
            {
                hud?.SetStatus(Localization.Tr("poi.too_far", DisplayName), 2.5f);
                return;
            }

            var persistence = actor.GetComponent<PlayerPersistence>();
            if (rewardClaimed || (persistence != null && persistence.HasClaimedPointOfInterest(ClaimId)))
            {
                hud?.SetStatus(Localization.Tr("poi.already_explored", DisplayName), 2.5f);
                return;
            }

            var progression = actor.GetComponent<PlayerProgression>();
            var inventory = actor.GetComponent<InventorySystem>();
            RewardService.Grant(Reward, progression, inventory, hud, DisplayName);
            rewardClaimed = true;
            persistence?.MarkPointOfInterestClaimed(ClaimId);
            hud?.SetStatus(InteractionMessage, 4f);
            hud?.AddFeed(Localization.Tr("poi.interacted", DisplayName));

            if (Reward != null && (Reward.Experience > 0 || Reward.Gold > 0))
            {
                hud?.AddFeed(Localization.Tr("poi.reward", Reward.Experience, Reward.Gold));
            }
        }
    }
}
