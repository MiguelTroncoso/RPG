using UnityEngine;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    public enum NetworkConnectionState
    {
        Offline,
        Connecting,
        Online,
        Error
    }

    public sealed class ConnectionStatusIndicator : MonoBehaviour
    {
        public MmorpgNetworkClient Client;
        public Image Dot;
        public Text Label;
        public string ServerLabel = "S-01";

        private void OnEnable()
        {
            if (Client != null)
            {
                Client.ConnectionStateChanged += Refresh;
            }

            Refresh(Client != null ? Client.ConnectionState : NetworkConnectionState.Offline);
        }

        private void OnDisable()
        {
            if (Client != null)
            {
                Client.ConnectionStateChanged -= Refresh;
            }
        }

        public void RefreshNow()
        {
            Refresh(Client != null ? Client.ConnectionState : NetworkConnectionState.Offline);
        }

        private void Refresh(NetworkConnectionState state)
        {
            if (Dot != null)
            {
                Dot.color = state == NetworkConnectionState.Online
                    ? new Color(0.2f, 0.9f, 0.48f)
                    : state == NetworkConnectionState.Connecting
                        ? new Color(0.96f, 0.72f, 0.18f)
                        : state == NetworkConnectionState.Error
                            ? new Color(0.96f, 0.26f, 0.22f)
                            : new Color(0.46f, 0.54f, 0.62f);
            }

            if (Label != null)
            {
                var stateLabel = state == NetworkConnectionState.Online ? Localization.Tr("net.connected_short") :
                    state == NetworkConnectionState.Connecting ? Localization.Tr("net.connecting_short") :
                    state == NetworkConnectionState.Error ? Localization.Tr("net.error_short") : Localization.Tr("net.disconnected_short");
                Label.text = $"{ServerLabel}  {stateLabel}";
            }
        }
    }
}
