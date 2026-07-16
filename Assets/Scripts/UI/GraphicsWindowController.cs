using UnityEngine;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    public sealed class GraphicsWindowController : MonoBehaviour
    {
        public GameObject Panel;
        public Text TitleText;
        public Text ProfileText;

        private void OnEnable()
        {
            Refresh();
        }

        public void Toggle()
        {
            if (Panel != null)
            {
                Panel.SetActive(!Panel.activeSelf);
            }
        }

        public void UseQuality()
        {
            UiPerformanceSettings.SetProfile(UiPerformanceProfile.Quality);
            Refresh();
        }

        public void UsePerformance()
        {
            UiPerformanceSettings.SetProfile(UiPerformanceProfile.Performance);
            Refresh();
        }

        private void Refresh()
        {
            if (ProfileText != null)
            {
                ProfileText.text = Localization.Tr("ui.profile", UiPerformanceSettings.Label);
            }
        }
    }
}
