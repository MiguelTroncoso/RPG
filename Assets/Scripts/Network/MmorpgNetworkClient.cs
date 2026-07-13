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
        public PlayerCharacterIdentity Identity;
        public PlayerProgression Progression;
        public PlayerPersistence Persistence;
        public Text NetworkStatusText;
        public Text ChatLogText;
        public InputField UrlInput;
        public InputField ChatInput;
        public bool AcceptServerSaveOnConnect = true;
        public float SaveSyncIntervalSeconds = 15f;
        public string CurrentStatus => pendingStatus;

        private readonly Queue<string> incoming = new Queue<string>();
        private readonly object incomingLock = new object();
        private readonly Dictionary<string, NetworkRemotePlayer> remotePlayers = new Dictionary<string, NetworkRemotePlayer>();
        private readonly List<string> chatLines = new List<string>();
        private readonly SemaphoreSlim sendLock = new SemaphoreSlim(1, 1);

        private ClientWebSocket socket;
        private CancellationTokenSource cancellation;
        private string localId = string.Empty;
        private string pendingStatus = Localization.Tr("net.status_offline");
        private string lastClassName = string.Empty;
        private string lastGenderName = string.Empty;
        private string lastPlayerName = string.Empty;
        private int lastLevel = -1;
        private float nextPositionSend;
        private float nextHelloCheck;
        private float nextSaveSync;
        private bool isConnecting;
        private string playerKey = string.Empty;
        private string lastSentSaveJson = string.Empty;
        private bool receivedServerSave;

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
            SendSaveStateTick();
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

            SetStatus(Localization.Tr("net.status_offline"));
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
            SetStatus(Localization.Tr("net.status_connecting"));

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
                SetStatus(Localization.Tr("net.status_online", url));
                await SendHelloAsync();
                _ = ReceiveLoopAsync(cancellation.Token);
            }
            catch (Exception error)
            {
                SetStatus(Localization.Tr("net.status_no_connection", error.Message));
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
                            SetStatus(Localization.Tr("net.status_server_disconnected"));
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
                SetStatus(Localization.Tr("net.status_connection_closed", error.Message));
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
                    case "savedState":
                        HandleSavedState(JsonUtility.FromJson<SavedStateMessage>(json));
                        break;
                    case "activity":
                        HandleActivity(JsonUtility.FromJson<ActivityMessage>(json));
                        break;
                    case "actionRejected":
                        HandleActionRejected(JsonUtility.FromJson<ActionRejectedMessage>(json));
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
            AppendChat(Localization.Tr("net.chat_system"), Localization.Tr("net.connected"));
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

        private void HandleSavedState(SavedStateMessage message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.stateJson) || Persistence == null)
            {
                return;
            }

            receivedServerSave = true;

            try
            {
                var data = JsonUtility.FromJson<PlayerSaveData>(message.stateJson);
                if (data == null || string.IsNullOrWhiteSpace(data.CharacterName))
                {
                    return;
                }

                if (!AcceptServerSaveOnConnect || !ShouldApplyServerSave(data))
                {
                    AppendChat(Localization.Tr("net.chat_server"), "Guardado remoto disponible; se mantiene el progreso local.");
                    SendSaveStateNow();
                    return;
                }

                Persistence.ApplyLoadedData(data);
                PlayerName = data.CharacterName;
                lastSentSaveJson = message.stateJson;
                AppendChat(Localization.Tr("net.chat_server"), $"Guardado remoto aplicado ({data.CharacterName} Nv{data.Level}).");
            }
            catch (Exception error)
            {
                AppendChat(Localization.Tr("net.chat_server"), $"Guardado remoto invalido: {error.Message}");
            }
        }

        private void HandleActivity(ActivityMessage message)
        {
            if (message == null || string.IsNullOrEmpty(message.detail) || message.id == localId)
            {
                return;
            }

            AppendChat(Localization.Tr("net.chat_valley"), $"{message.name} {message.detail}");
        }

        private void HandleActionRejected(ActionRejectedMessage message)
        {
            if (message != null)
            {
                AppendChat(Localization.Tr("net.chat_server"), Localization.Tr("net.rejected", message.action, message.reason));
            }
        }

        // Capa de intenciones: las acciones criticas se reportan con su
        // valor resultante; el servidor valida plausibilidad y ritmo antes
        // de difundirlas como actividad.
        public void SendAction(string action, string detail, int value)
        {
            if (!IsConnected || string.IsNullOrWhiteSpace(action))
            {
                return;
            }

            _ = SendJsonAsync(new ActionPayload { action = action, detail = detail, value = value });
        }

        public void SendTelemetry(string summaryJson)
        {
            if (!IsConnected || string.IsNullOrWhiteSpace(summaryJson))
            {
                return;
            }

            _ = SendJsonAsync(new TelemetryPayload
            {
                playerKey = PlayerKey(),
                summaryJson = summaryJson
            });
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
            var genderName = CurrentGenderName();
            var level = Progression != null ? Progression.Level : 1;
            if (className == lastClassName && genderName == lastGenderName && PlayerName == lastPlayerName && level == lastLevel)
            {
                return;
            }

            _ = SendHelloAsync();
        }

        private void SendSaveStateTick()
        {
            if (!IsConnected || Persistence == null || !Persistence.HasActiveCharacter || Time.unscaledTime < nextSaveSync)
            {
                return;
            }

            nextSaveSync = Time.unscaledTime + Mathf.Max(5f, SaveSyncIntervalSeconds);
            SendSaveStateNow();
        }

        public void SendSaveStateNow()
        {
            if (!IsConnected || Persistence == null || !Persistence.HasActiveCharacter)
            {
                return;
            }

            var stateJson = Persistence.ExportJson();
            if (string.IsNullOrWhiteSpace(stateJson) || stateJson == lastSentSaveJson)
            {
                return;
            }

            lastSentSaveJson = stateJson;
            _ = SendJsonAsync(new SaveStatePayload
            {
                playerKey = PlayerKey(),
                stateJson = stateJson
            });
        }

        private async Task SendHelloAsync()
        {
            var className = ClassController != null && ClassController.Definition != null
                ? ClassController.Definition.DisplayName
                : "Guerrero";
            var genderName = CurrentGenderName();

            var level = Progression != null ? Progression.Level : 1;
            lastClassName = className;
            lastGenderName = genderName;
            lastPlayerName = PlayerName;
            lastLevel = level;
            await SendJsonAsync(new HelloPayload
            {
                playerKey = PlayerKey(),
                name = PlayerName,
                className = className,
                gender = genderName,
                level = level
            });

            if (receivedServerSave)
            {
                SendSaveStateNow();
            }
        }

        private bool ShouldApplyServerSave(PlayerSaveData data)
        {
            if (Progression == null || !Persistence.HasActiveCharacter)
            {
                return true;
            }

            if (data.Level != Progression.Level)
            {
                return data.Level > Progression.Level;
            }

            if (data.Experience != Progression.Experience)
            {
                return data.Experience > Progression.Experience;
            }

            return data.Gold >= Progression.Gold;
        }

        private string CurrentGenderName()
        {
            return Identity != null ? Identity.Gender.ToString() : CharacterGender.Masculino.ToString();
        }

        private string PlayerKey()
        {
            if (!string.IsNullOrEmpty(playerKey))
            {
                return playerKey;
            }

            var stableName = !string.IsNullOrWhiteSpace(PlayerName) ? PlayerName.Trim().ToLowerInvariant() : "hero";
            var device = string.IsNullOrWhiteSpace(SystemInfo.deviceUniqueIdentifier)
                ? Application.productName
                : SystemInfo.deviceUniqueIdentifier;
            playerKey = Hash128.Compute($"{Application.companyName}:{Application.productName}:{device}:{stableName}").ToString();
            return playerKey;
        }

        private async Task SendJsonAsync(object payload)
        {
            if (!IsConnected)
            {
                SetStatus(Localization.Tr("net.status_offline_hint"));
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
                SetStatus(Localization.Tr("net.status_send_failed", error.Message));
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
