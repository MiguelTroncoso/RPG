using UnityEngine;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    // Panel de lectura de telemetria: no modifica reglas de combate ni balance.
    public sealed class TelemetryWindowController : MonoBehaviour
    {
        public GameObject Panel;
        public Text BodyText;
        public CombatTelemetry Telemetry;

        private void OnEnable()
        {
            if (Telemetry != null)
            {
                Telemetry.Changed += Refresh;
                Refresh();
            }
        }

        private void OnDisable()
        {
            if (Telemetry != null)
            {
                Telemetry.Changed -= Refresh;
            }
        }

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
            if (BodyText != null && Telemetry != null)
            {
                BodyText.text = Telemetry.BuildDisplayText();
            }
        }

        public void SaveNow()
        {
            Telemetry?.SaveNow();
            Refresh();
        }
    }
}
