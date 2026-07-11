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

