using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class ZonePointOfInterest : MonoBehaviour
    {
        public string DisplayName;
        public string InteractionMessage;
        public float InteractDistance = 8f;

        public static void InteractNearest(Transform actor)
        {
            var hud = FindFirstObjectByType<PrototypeHud>();
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

            hud?.SetStatus(InteractionMessage, 4f);
            hud?.AddFeed(Localization.Tr("poi.interacted", DisplayName));
        }
    }
}
