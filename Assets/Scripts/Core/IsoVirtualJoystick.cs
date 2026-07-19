using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MmorpgPrototype
{
    public sealed class IsoVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform Knob;
        public float Radius = 58f;
        public event Action<Vector2> ValueChanged;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UpdateValue(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateValue(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Knob != null)
            {
                Knob.anchoredPosition = Vector2.zero;
            }
            ValueChanged?.Invoke(Vector2.zero);
        }

        private void UpdateValue(PointerEventData eventData)
        {
            if (rectTransform == null || Knob == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint))
            {
                return;
            }

            var value = Vector2.ClampMagnitude(localPoint, Radius);
            Knob.anchoredPosition = value;
            ValueChanged?.Invoke(value / Radius);
        }
    }
}
