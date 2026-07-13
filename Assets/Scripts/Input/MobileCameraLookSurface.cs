using UnityEngine;
using UnityEngine.EventSystems;

namespace MmorpgPrototype
{
    // Receives camera gestures only in free screen space. UI controls are
    // created later and remain above this surface in the canvas hierarchy.
    public sealed class MobileCameraLookSurface : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public OrbitCamera Camera;

        private const int NoPointer = int.MinValue;
        private int activePointerId = NoPointer;
        private Vector2 lastPosition;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Application.isMobilePlatform || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (activePointerId != NoPointer)
            {
                return;
            }

            activePointerId = eventData.pointerId;
            lastPosition = eventData.position;
            eventData.useDragThreshold = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId || Camera == null)
            {
                return;
            }

            var delta = eventData.position - lastPosition;
            lastPosition = eventData.position;
            Camera.RotateByTouchDelta(delta);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == activePointerId)
            {
                activePointerId = NoPointer;
            }
        }

        private void OnDisable()
        {
            activePointerId = NoPointer;
        }
    }
}
