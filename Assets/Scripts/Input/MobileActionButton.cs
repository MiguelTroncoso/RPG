using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MmorpgPrototype
{
    // Fires gameplay actions on pointer down so a second finger can keep the
    // movement joystick active while the action button is being used.
    public sealed class MobileActionButton : MonoBehaviour, IPointerDownHandler
    {
        public UnityEvent Pressed = new UnityEvent();

        public void OnPointerDown(PointerEventData eventData)
        {
            Pressed?.Invoke();
        }
    }
}
