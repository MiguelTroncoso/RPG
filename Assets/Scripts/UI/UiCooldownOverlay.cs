using UnityEngine;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    // Shared radial cooldown presentation for combat buttons. Gameplay owns
    // the timer; this component only renders the remaining duration.
    public sealed class UiCooldownOverlay : MonoBehaviour
    {
        public Image Mask;
        public Text Countdown;

        public void SetRemaining(float remaining, float duration)
        {
            var active = remaining > 0.05f;
            if (Mask != null)
            {
                Mask.gameObject.SetActive(active);
                Mask.fillAmount = active ? Mathf.Clamp01(remaining / Mathf.Max(0.01f, duration)) : 0f;
            }

            if (Countdown != null)
            {
                Countdown.gameObject.SetActive(active);
                Countdown.text = active ? Format(remaining) : string.Empty;
            }
        }

        private static string Format(float seconds)
        {
            if (seconds >= 60f)
            {
                var minutes = Mathf.FloorToInt(seconds / 60f);
                var remainder = Mathf.FloorToInt(seconds % 60f);
                return $"{minutes}:{remainder:00}";
            }

            return seconds >= 10f ? Mathf.CeilToInt(seconds).ToString() : seconds.ToString("0.0");
        }
    }
}
