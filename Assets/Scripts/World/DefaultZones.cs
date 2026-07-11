using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Zonas del juego. Fuente unica para el generador de assets
    // (MMORPG > World > Generate Zones) y el fallback runtime. Solo se
    // ajustan datos: el molde es siempre ZoneDefinition.
    public static class DefaultZones
    {
        public const string Zone1 = "valley_zone1";
        public const string Zone2 = "forest_zone2";

        public const string ValleyEliteId = "valley_elite";
        public const string ValleyBossId = "valley_boss";
        public const string ForestEliteId = "forest_elite";
        public const string ForestBossId = "forest_boss";

        public static List<ZoneDefinition> CreateAll()
        {
            return new List<ZoneDefinition> { CreateZone1(), CreateZone2() };
        }

        public static ZoneDefinition CreateZone1()
        {
            var zone = ScriptableObject.CreateInstance<ZoneDefinition>();
            zone.name = Zone1;
            zone.ZoneId = Zone1;
            zone.DisplayName = "Valle de las Reliquias";
            zone.MinLevel = 1;
            zone.MaxLevel = 10;
            zone.EliteEnemyId = ValleyEliteId;
            zone.BossEnemyId = ValleyBossId;
            return zone;
        }

        public static ZoneDefinition CreateZone2()
        {
            var zone = ScriptableObject.CreateInstance<ZoneDefinition>();
            zone.name = Zone2;
            zone.ZoneId = Zone2;
            zone.DisplayName = "Bosque de los Susurros";
            zone.MinLevel = 11;
            zone.MaxLevel = 20;

            zone.HasOwnGround = true;
            zone.GroundCenter = new Vector3(0f, 0f, 70f);
            zone.GroundColor = new Color(0.13f, 0.24f, 0.15f);
            zone.SignPosition = new Vector3(0f, 0f, 38f);

            zone.NormalEnemyId = "forest_creature";
            zone.NormalName = "Espinoso del bosque";
            zone.NormalColor = new Color(0.2f, 0.42f, 0.2f);
            zone.NormalCount = 6;
            zone.NormalAreaCenter = new Vector3(0f, 1f, 62f);
            zone.NormalAreaRadius = 12f;
            zone.NormalHealth = 260;
            zone.NormalDamage = 18;
            zone.NormalDefense = 2;
            zone.NormalExp = 90;
            zone.NormalGoldMin = 12;
            zone.NormalGoldMax = 20;
            zone.NormalMoveSpeed = 2.5f;

            zone.EliteEnemyId = ForestEliteId;
            zone.EliteName = "Sombra del bosque";
            zone.EliteCount = 2;
            zone.EliteAreaCenter = new Vector3(16f, 1f, 80f);
            zone.EliteAreaRadius = 6f;
            zone.EliteHealth = 620;
            zone.EliteDamage = 30;
            zone.EliteDefense = 6;
            zone.EliteExp = 260;
            zone.EliteGoldMin = 35;
            zone.EliteGoldMax = 55;
            zone.EliteRespawnSeconds = 30f;

            zone.BossEnemyId = ForestBossId;
            zone.BossName = "Anciano de Espinas";
            zone.BossPosition = new Vector3(-18f, 1f, 84f);
            zone.BossHealth = 2200;
            zone.BossDamage = 45;
            zone.BossDefense = 10;
            zone.BossExp = 900;
            zone.BossGoldMin = 250;
            zone.BossGoldMax = 350;
            zone.BossGuaranteedDrop = DefaultGameItems.ProtectionRune;
            zone.BossRespawnSeconds = 120f;

            return zone;
        }
    }
}
