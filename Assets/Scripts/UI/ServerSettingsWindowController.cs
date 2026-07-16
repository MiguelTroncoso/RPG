using UnityEngine;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    public sealed class ServerSettingsWindowController : MonoBehaviour
    {
        public GameObject Panel;
        public MmorpgNetworkClient Client;
        public InputField UrlInput;
        public Text TitleText;
        public Text StatusText;

        private float nextRefresh;

        private void OnEnable()
        {
            if (Client != null)
            {
                Client.ConnectionStateChanged += OnConnectionStateChanged;
            }

            RefreshStatus();
        }

        private void OnDisable()
        {
            if (Client != null)
            {
                Client.ConnectionStateChanged -= OnConnectionStateChanged;
            }
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh)
            {
                return;
            }

            nextRefresh = Time.unscaledTime + 0.25f;
            RefreshStatus();
        }

        public void Toggle()
        {
            if (Panel != null)
            {
                Panel.SetActive(!Panel.activeSelf);
            }
        }

        public void Connect()
        {
            if (Client == null)
            {
                return;
            }

            if (UrlInput != null && !string.IsNullOrWhiteSpace(UrlInput.text))
            {
                Client.ServerUrl = UrlInput.text.Trim();
                PlayerPrefs.SetString("mmorpg.server.url", Client.ServerUrl);
                PlayerPrefs.Save();
            }

            Client.Connect();
            RefreshStatus();
        }

        private void OnConnectionStateChanged(NetworkConnectionState state)
        {
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (StatusText != null && Client != null)
            {
                StatusText.text = Client.CurrentStatus;
            }
        }
    }
}
