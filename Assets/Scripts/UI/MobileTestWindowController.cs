using UnityEngine;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    public sealed class MobileTestWindowController : MonoBehaviour
    {
        public GameObject Panel;
        public Text BodyText;
        public MobileRuntimeDiagnostics Diagnostics;
        public CombatFeedbackAudio Audio;

        public void Toggle()
        {
            if (Panel == null)
            {
                return;
            }

            Panel.SetActive(!Panel.activeSelf);
            if (Panel.activeSelf)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            if (BodyText == null)
            {
                return;
            }

            var summary = Diagnostics != null ? Diagnostics.LastSummary : Localization.Tr("mobile.unavailable");
            var music = Audio != null && Audio.MusicEnabled ? "ON" : "OFF";
            var sfx = Audio != null ? Audio.SfxVolume.ToString("0.00") : "--";
            var musicVolume = Audio != null ? Audio.MusicVolume.ToString("0.00") : "--";
            BodyText.text = $"{summary}\n{Localization.Tr("mobile.audio", music, sfx, musicVolume)}";
        }

        public void ToggleMusic()
        {
            Audio?.ToggleMusic();
            Refresh();
        }

        public void AdjustSfx(float amount)
        {
            if (Audio != null)
            {
                Audio.SetSfxVolume(Audio.SfxVolume + amount);
            }

            Refresh();
        }

        public void AdjustMusic(float amount)
        {
            if (Audio != null)
            {
                Audio.SetMusicVolume(Audio.MusicVolume + amount);
            }

            Refresh();
        }
    }
}
