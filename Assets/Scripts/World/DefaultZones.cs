using UnityEngine;

namespace MmorpgPrototype
{
    // Zona inicial. Fuente unica para el generador de assets
    // (MMORPG > World > Generate Zones) y el fallback runtime.
    public static class DefaultZones
    {
        public const string Zone1 = "valley_zone1";

        public static ZoneDefinition CreateZone1()
        {
            var zone = ScriptableObject.CreateInstance<ZoneDefinition>();
            zone.name = Zone1;
            zone.ZoneId = Zone1;
            zone.DisplayName = "Valle de las Reliquias";
            zone.MinLevel = 1;
            zone.MaxLevel = 10;
            return zone;
        }
    }
}
