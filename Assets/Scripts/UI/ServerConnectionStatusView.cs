using UnityEngine;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    public sealed class ServerConnectionStatusView : MonoBehaviour
    {
        public MmorpgNetworkClient Client;
        public Text Label;

        private string lastStatus;

        private void Update()
        {
            if (Client == null || Label == null || lastStatus == Client.CurrentStatus)
            {
                return;
            }

            lastStatus = Client.CurrentStatus;
            Label.text = lastStatus;
            Label.color = lastStatus.StartsWith("Online", System.StringComparison.OrdinalIgnoreCase)
                ? new Color(0.45f, 0.9f, 0.62f)
                : lastStatus.StartsWith("Conectando", System.StringComparison.OrdinalIgnoreCase)
                    ? new Color(1f, 0.82f, 0.38f)
                    : new Color(0.7f, 0.76f, 0.82f);
        }
    }
}
