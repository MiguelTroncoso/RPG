using UnityEngine;
using UnityEngine.EventSystems;

namespace MmorpgPrototype
{
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform Knob;
        public float Radius = 80f;

        public Vector2 Value { get; private set; }

        private const int NoPointer = int.MinValue;
        private int activePointerId = NoPointer;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activePointerId != NoPointer && activePointerId != eventData.pointerId)
            {
                return;
            }

            activePointerId = eventData.pointerId;
            UpdateDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            UpdateDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            activePointerId = NoPointer;
            Value = Vector2.zero;

            if (Knob != null)
            {
                Knob.anchoredPosition = Vector2.zero;
            }
        }

        private void OnDisable()
        {
            activePointerId = NoPointer;
            Value = Vector2.zero;

            if (Knob != null)
            {
                Knob.anchoredPosition = Vector2.zero;
            }
        }

        private void UpdateDrag(PointerEventData eventData)
        {
            var rectTransform = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint))
            {
                return;
            }

            var clamped = Vector2.ClampMagnitude(localPoint, Radius);
            Value = clamped / Radius;

            if (Knob != null)
            {
                Knob.anchoredPosition = clamped;
            }
        }
    }
}
