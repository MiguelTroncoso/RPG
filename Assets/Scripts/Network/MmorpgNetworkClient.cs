using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    public sealed class MmorpgNetworkClient : MonoBehaviour
    {
        public string ServerUrl = "ws://localhost:7777";
        public string PlayerName = "Heroe";
        public Transform PlayerTransform;
        public PlayerClassController ClassController;
        public Text NetworkStatusText;
        public Text ChatLogText;
        public InputField UrlInput;
        public InputField ChatInput;

        private readonly Queue<string> incoming = new Queue<string>();
        private readonly object incomingLock = new object();
        private readonly Dictionary<string, NetworkRemotePlayer> remotePlayers = new Dictionary<string, NetworkRemotePlayer>();
        private readonly List<string> chatLines = new List<string>();
        private readonly SemaphoreSlim sendLock = new SemaphoreSlim(1, 1);

        private ClientWebSocket socket;
        private CancellationTokenSource cancellation;
        private string localId = string.Empty;
        private string pendingStatus = "Offline";
        private string lastClassName = string.Empty;
        private float nextPositionSend;
        private float nextHelloCheck;
        private bool isConnecting;

        private bool IsConnected => socket != null && socket.State == WebSocketState.Open;

        private void Awake()
        {
            Application.runInBackground = true;
        }

        private void Update()
        {
            ApplyPendingStatus();
            ProcessIncoming();
            SendPositionTick();
            SendClassChangeTick();
        }

        private void OnDestroy()
        {
            _ = DisconnectAsync();
        }

        public void Connect()
        {
            if (isConnecting || IsConnected)
            {
                return;
            }

            _ = ConnectAsync();
        }

        public async Task DisconnectAsync()
        {
            cancellation?.Cancel();

            if (socket != null)
            {
                try
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
                    }
                }
                catch
                {
                    // Closing can fail if the server is already gone; the client state is still disposable.
                }

                socket.Dispose();
                socket = null;
            }

            SetStatus("Offline");
        }

        public void SendChatFromInput()
        {
            if (ChatInput == null)
            {
                return;
            }

            var text = ChatInput.text.Trim();
            if (text.Length == 0)
            {
                return;
            }

            ChatInput.text = string.Empty;
            _ = SendJsonAsync(new ChatPayload { text = text });
        }

        private async Task ConnectAsync()
        {
            isConnecting = true;
            SetStatus("Conectando...");

            try
            {
                cancellation?.Cancel();
                cancellation = new CancellationTokenSource();
                socket?.Dispose();
                socket = new ClientWebSocket();

                var url = UrlInput != null && !string.IsNullOrWhiteSpace(UrlInput.text)
                    ? UrlInput.text.Trim()
                    : ServerUrl;

                await socket.ConnectAsync(new Uri(url), cancellation.Token);
                SetStatus($"Online: {url}");
                await SendHelloAsync();
                _ = ReceiveLoopAsync(cancellation.Token);
            }
            catch (Exception error)
            {
                SetStatus($"Sin conexion: {error.Message}");
                socket?.Dispose();
                socket = null;
            }
            finally
            {
                isConnecting = false;
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            var buffer = new byte[8192];

            try
            {
                while (!token.IsCancellationRequested && socket != null && socket.State == WebSocketState.Open)
                {
                    var builder = new StringBuilder();
                    WebSocketReceiveResult result;

                    do
                    {
                        result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            SetStatus("Servidor desconectado.");
                            return;
                        }

                        builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }
                    while (!result.EndOfMessage);

                    lock (incomingLock)
                    {
                        incoming.Enqueue(builder.ToString());
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception error)
            {
                SetStatus($"Conexion cerrada: {error.Message}");
            }
        }

        private void ProcessIncoming()
        {
            while (true)
            {
                string json;

                lock (incomingLock)
                {
                    if (incoming.Count == 0)
                    {
                        break;
                    }

                    json = incoming.Dequeue();
                }

                var envelope = JsonUtility.FromJson<NetworkEnvelope>(json);
                if (envelope == null || string.IsNullOrEmpty(envelope.type))
                {
                    continue;
                }

                switch (envelope.type)
                {
                    case "welcome":
                        HandleWelcome(JsonUtility.FromJson<WelcomeMessage>(json));
                        break;
                    case "snapshot":
                        HandleSnapshot(JsonUtility.FromJson<SnapshotMessage>(json));
                        break;
                    case "playerJoined":
                    case "playerUpdated":
                        HandlePlayerState(JsonUtility.FromJson<PlayerStateMessage>(json));
                        break;
                    case "playerLeft":
                        HandlePlayerLeft(JsonUtility.FromJson<PlayerLeftMessage>(json));
                        break;
                    case "chat":
                        HandleChat(JsonUtility.FromJson<NetworkChatMessage>(json));
                        break;
                    case "error":
                        SetStatus(json);
                        break;
                }
            }
        }

        private void HandleWelcome(WelcomeMessage message)
        {
            localId = message.id;
            AppendChat("Sistema", "Conectado al servidor local.");
        }

        private void HandleSnapshot(SnapshotMessage message)
        {
            if (message.players == null)
            {
                return;
            }

            foreach (var player in message.players)
            {
                ApplyRemotePlayer(player);
            }
        }

        private void HandlePlayerState(PlayerStateMessage message)
        {
            if (message.player != null)
            {
                ApplyRemotePlayer(message.player);
            }
        }

        private void HandlePlayerLeft(PlayerLeftMessage message)
        {
            if (message == null || string.IsNullOrEmpty(message.id))
            {
                return;
            }

            if (remotePlayers.TryGetValue(message.id, out var remote))
            {
                Destroy(remote.gameObject);
                remotePlayers.Remove(message.id);
            }
        }

        private void HandleChat(NetworkChatMessage message)
        {
            if (message != null)
            {
                AppendChat(message.name, message.text);
            }
        }

        private void ApplyRemotePlayer(RemotePlayerState state)
        {
            if (state == null || string.IsNullOrEmpty(state.id) || state.id == localId)
            {
                return;
            }

            if (!remotePlayers.TryGetValue(state.id, out var remote))
            {
                remote = NetworkRemotePlayer.Create(state);
                remotePlayers[state.id] = remote;
                return;
            }

            remote.ApplyState(state);
        }

        private void SendPositionTick()
        {
            if (!IsConnected || PlayerTransform == null || Time.time < nextPositionSend)
            {
                return;
            }

            nextPositionSend = Time.time + 0.1f;
            var position = PlayerTransform.position;
            _ = SendJsonAsync(new PositionPayload
            {
                x = position.x,
                y = position.y,
                z = position.z,
                yaw = PlayerTransform.eulerAngles.y
            });
        }

        private void SendClassChangeTick()
        {
            if (!IsConnected || ClassController == null || ClassController.Definition == null || Time.time < nextHelloCheck)
            {
                return;
            }

            nextHelloCheck = Time.time + 0.5f;
            var className = ClassController.Definition.DisplayName;
            if (className == lastClassName)
            {
                return;
            }

            _ = SendHelloAsync();
        }

        private async Task SendHelloAsync()
        {
            var className = ClassController != null && ClassController.Definition != null
                ? ClassController.Definition.DisplayName
                : "Guerrero";

            lastClassName = className;
            await SendJsonAsync(new HelloPayload
            {
                name = PlayerName,
                className = className
            });
        }

        private async Task SendJsonAsync(object payload)
        {
            if (!IsConnected)
            {
                SetStatus("Offline: inicia el server y presiona ONLINE.");
                return;
            }

            var json = JsonUtility.ToJson(payload);
            var bytes = Encoding.UTF8.GetBytes(json);

            await sendLock.WaitAsync();

            try
            {
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellation.Token);
            }
            catch (Exception error)
            {
                SetStatus($"Envio fallido: {error.Message}");
            }
            finally
            {
                sendLock.Release();
            }
        }

        private void AppendChat(string name, string text)
        {
            chatLines.Add($"{name}: {text}");

            while (chatLines.Count > 6)
            {
                chatLines.RemoveAt(0);
            }

            if (ChatLogText != null)
            {
                ChatLogText.text = string.Join("\n", chatLines);
            }
        }

        private void SetStatus(string status)
        {
            pendingStatus = status;
        }

        private void ApplyPendingStatus()
        {
            if (NetworkStatusText != null)
            {
                NetworkStatusText.text = pendingStatus;
            }
        }
    }
}
