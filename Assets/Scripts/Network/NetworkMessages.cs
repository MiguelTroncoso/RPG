using System;

namespace MmorpgPrototype
{
    [Serializable]
    public sealed class NetworkEnvelope
    {
        public string type;
    }

    [Serializable]
    public sealed class WelcomeMessage
    {
        public string type;
        public string id;
        public long serverTime;
    }

    [Serializable]
    public sealed class SnapshotMessage
    {
        public string type;
        public RemotePlayerState[] players;
    }

    [Serializable]
    public sealed class PlayerStateMessage
    {
        public string type;
        public RemotePlayerState player;
    }

    [Serializable]
    public sealed class PlayerLeftMessage
    {
        public string type;
        public string id;
    }

    [Serializable]
    public sealed class NetworkChatMessage
    {
        public string type;
        public string id;
        public string name;
        public string text;
        public long serverTime;
    }

    [Serializable]
    public sealed class RemotePlayerState
    {
        public string id;
        public string name;
        public string className;
        public string gender;
        public float x;
        public float y;
        public float z;
        public float yaw;
    }

    [Serializable]
    public sealed class HelloPayload
    {
        public string type = "hello";
        public string name;
        public string className;
        public string gender;
    }

    // Intencion de accion critica: hoy el servidor solo la difunde como
    // actividad; manana la validara antes de aplicar el resultado.
    [Serializable]
    public sealed class ActionPayload
    {
        public string type = "action";
        public string action;
        public string detail;
    }

    [Serializable]
    public sealed class ActivityMessage
    {
        public string type;
        public string id;
        public string name;
        public string action;
        public string detail;
        public long serverTime;
    }

    [Serializable]
    public sealed class PositionPayload
    {
        public string type = "position";
        public float x;
        public float y;
        public float z;
        public float yaw;
    }

    [Serializable]
    public sealed class ChatPayload
    {
        public string type = "chat";
        public string text;
    }
}

