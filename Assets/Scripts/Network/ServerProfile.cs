using System;

namespace MmorpgPrototype
{
    [Serializable]
    public sealed class ServerProfile
    {
        public string Id;
        public string DisplayName;
        public string Url;
        public bool Enabled;

        public ServerProfile(string id, string displayName, string url, bool enabled)
        {
            Id = id;
            DisplayName = displayName;
            Url = url;
            Enabled = enabled;
        }

        public string Label => $"{Id}\n{DisplayName}";
    }

    public static class DefaultServerProfiles
    {
        public static ServerProfile[] Create()
        {
            return new[]
            {
                new ServerProfile("S-01", "Valle Central", "ws://localhost:7777", true),
                new ServerProfile("S-02", "Bosque de los Susurros", "ws://localhost:7778", false),
                new ServerProfile("S-03", "Reino de Pruebas", "ws://localhost:7779", false)
            };
        }
    }
}
