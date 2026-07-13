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
        public const string Zone3 = "ashes_zone3";
        public const string Zone4 = "crystal_zone4";
        public const string Zone5 = "frost_zone5";
        public const string Zone6 = "sunken_zone6";
        public const string Zone7 = "obsidian_zone7";
        public const string Zone8 = "astral_zone8";
        public const string Zone9 = "eclipse_zone9";
        public const string Zone10 = "throne_zone10";

        public const string ValleyEliteId = "valley_elite";
        public const string ValleyBossId = "valley_boss";
        public const string ForestEliteId = "forest_elite";
        public const string ForestBossId = "forest_boss";
        public const string AshCreatureId = "ash_creature";
        public const string AshEliteId = "ash_elite";
        public const string AshBossId = "ash_boss";
        public const string CrystalCreatureId = "crystal_creature";
        public const string CrystalEliteId = "crystal_elite";
        public const string CrystalBossId = "crystal_boss";
        public const string FrostCreatureId = "frost_creature";
        public const string FrostEliteId = "frost_elite";
        public const string FrostBossId = "frost_boss";
        public const string SunkenCreatureId = "sunken_creature";
        public const string SunkenEliteId = "sunken_elite";
        public const string SunkenBossId = "sunken_boss";
        public const string ObsidianCreatureId = "obsidian_creature";
        public const string ObsidianEliteId = "obsidian_elite";
        public const string ObsidianBossId = "obsidian_boss";
        public const string AstralCreatureId = "astral_creature";
        public const string AstralEliteId = "astral_elite";
        public const string AstralBossId = "astral_boss";
        public const string EclipseCreatureId = "eclipse_creature";
        public const string EclipseEliteId = "eclipse_elite";
        public const string EclipseBossId = "eclipse_boss";
        public const string ThroneCreatureId = "throne_creature";
        public const string ThroneEliteId = "throne_elite";
        public const string ThroneBossId = "throne_boss";

        public static List<ZoneDefinition> CreateAll()
        {
            return new List<ZoneDefinition>
            {
                CreateZone1(), CreateZone2(), CreateZone3(), CreateZone4(), CreateZone5(),
                CreateZone6(), CreateZone7(), CreateZone8(), CreateZone9(), CreateZone10()
            };
        }

        public static ZoneDefinition CreateZone1()
        {
            var zone = ScriptableObject.CreateInstance<ZoneDefinition>();
            zone.name = Zone1;
            zone.ZoneId = Zone1;
            zone.DisplayName = "Valle de las Reliquias";
            zone.MinLevel = 1;
            zone.MaxLevel = 10;
            zone.HasSafeZone = true;
            zone.SafeZoneCenter = new Vector3(0f, 0f, -3f);
            zone.SafeZoneRadius = 9.5f;
            zone.NormalAreaCenter = new Vector3(0f, 1f, 14f);
            zone.NormalAreaRadius = 6f;
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

        public static ZoneDefinition CreateZone3()
        {
            var zone = ScriptableObject.CreateInstance<ZoneDefinition>();
            zone.name = Zone3;
            zone.ZoneId = Zone3;
            zone.DisplayName = "Colinas Cenicientas";
            zone.MinLevel = 21;
            zone.MaxLevel = 30;

            zone.HasOwnGround = true;
            zone.GroundCenter = new Vector3(0f, 0f, 140f);
            zone.GroundColor = new Color(0.3f, 0.27f, 0.26f);
            zone.SignPosition = new Vector3(0f, 0f, 108f);

            zone.NormalEnemyId = AshCreatureId;
            zone.NormalName = "Ceniciento errante";
            zone.NormalColor = new Color(0.45f, 0.42f, 0.4f);
            zone.NormalCount = 6;
            zone.NormalAreaCenter = new Vector3(0f, 1f, 132f);
            zone.NormalAreaRadius = 12f;
            zone.NormalHealth = 650;
            zone.NormalDamage = 32;
            zone.NormalDefense = 5;
            zone.NormalExp = 220;
            zone.NormalGoldMin = 25;
            zone.NormalGoldMax = 40;
            zone.NormalMoveSpeed = 2.6f;

            zone.EliteEnemyId = AshEliteId;
            zone.EliteName = "Devorador de ceniza";
            zone.EliteCount = 2;
            zone.EliteAreaCenter = new Vector3(-16f, 1f, 150f);
            zone.EliteAreaRadius = 6f;
            zone.EliteHealth = 1600;
            zone.EliteDamage = 55;
            zone.EliteDefense = 12;
            zone.EliteExp = 600;
            zone.EliteGoldMin = 80;
            zone.EliteGoldMax = 120;
            zone.EliteRespawnSeconds = 35f;

            zone.BossEnemyId = AshBossId;
            zone.BossName = "Corazon de Ceniza";
            zone.BossPosition = new Vector3(18f, 1f, 154f);
            zone.BossHealth = 5200;
            zone.BossDamage = 85;
            zone.BossDefense = 18;
            zone.BossExp = 2200;
            zone.BossGoldMin = 600;
            zone.BossGoldMax = 800;
            zone.BossGuaranteedDrop = DefaultGameItems.ValleyAmulet;
            zone.BossRespawnSeconds = 150f;

            return zone;
        }

        public static ZoneDefinition CreateZone4()
        {
            var zone = ScriptableObject.CreateInstance<ZoneDefinition>();
            zone.name = Zone4;
            zone.ZoneId = Zone4;
            zone.DisplayName = "Cumbres de Cristal";
            zone.MinLevel = 31;
            zone.MaxLevel = 40;

            zone.HasOwnGround = true;
            zone.GroundCenter = new Vector3(0f, 0f, 210f);
            zone.GroundColor = new Color(0.22f, 0.34f, 0.42f);
            zone.SignPosition = new Vector3(0f, 0f, 178f);

            zone.NormalEnemyId = CrystalCreatureId;
            zone.NormalName = "Centinela de cristal";
            zone.NormalColor = new Color(0.45f, 0.78f, 0.92f);
            zone.NormalCount = 7;
            zone.NormalAreaCenter = new Vector3(0f, 1f, 202f);
            zone.NormalAreaRadius = 13f;
            zone.NormalHealth = 1200;
            zone.NormalDamage = 54;
            zone.NormalDefense = 10;
            zone.NormalExp = 420;
            zone.NormalGoldMin = 45;
            zone.NormalGoldMax = 72;
            zone.NormalMoveSpeed = 2.7f;

            zone.EliteEnemyId = CrystalEliteId;
            zone.EliteName = "Custodio prismal";
            zone.EliteCount = 2;
            zone.EliteAreaCenter = new Vector3(16f, 1f, 222f);
            zone.EliteAreaRadius = 7f;
            zone.EliteHealth = 3100;
            zone.EliteDamage = 92;
            zone.EliteDefense = 22;
            zone.EliteExp = 1100;
            zone.EliteGoldMin = 140;
            zone.EliteGoldMax = 210;
            zone.EliteRespawnSeconds = 40f;

            zone.BossEnemyId = CrystalBossId;
            zone.BossName = "Oraculo Fragmentado";
            zone.BossPosition = new Vector3(-18f, 1f, 226f);
            zone.BossHealth = 9800;
            zone.BossDamage = 130;
            zone.BossDefense = 30;
            zone.BossExp = 4200;
            zone.BossGoldMin = 1000;
            zone.BossGoldMax = 1400;
            zone.BossGuaranteedDrop = DefaultGameItems.ProtectionRune;
            zone.BossRespawnSeconds = 180f;

            return zone;
        }

        public static ZoneDefinition CreateZone5()
        {
            return CreateAdvancedZone(
                Zone5, "Paso Glacial", 41, 50, 280f, new Color(0.58f, 0.72f, 0.82f),
                FrostCreatureId, "Lobo de escarcha", new Color(0.72f, 0.88f, 1f),
                FrostEliteId, "Vigia del hielo",
                FrostBossId, "Matriarca Invernal",
                1600, 70, 18, 680, 70, 110,
                4200, 120, 34, 1600, 230, 340,
                12500, 180, 45, 6500, 1500, 2200,
                DefaultGameItems.ValleyAmulet);
        }

        public static ZoneDefinition CreateZone6()
        {
            return CreateAdvancedZone(
                Zone6, "Ruinas Sumergidas", 51, 60, 350f, new Color(0.12f, 0.38f, 0.44f),
                SunkenCreatureId, "Ahogado antiguo", new Color(0.18f, 0.58f, 0.62f),
                SunkenEliteId, "Sirviente abisal",
                SunkenBossId, "Reina de las Mareas Rotas",
                2400, 92, 28, 980, 105, 165,
                6200, 150, 48, 2400, 360, 560,
                19000, 225, 62, 9500, 2500, 3600,
                DefaultGameItems.ProtectionRune);
        }

        public static ZoneDefinition CreateZone7()
        {
            return CreateAdvancedZone(
                Zone7, "Forja Obsidiana", 61, 70, 420f, new Color(0.22f, 0.08f, 0.08f),
                ObsidianCreatureId, "Forjado de obsidiana", new Color(0.42f, 0.12f, 0.1f),
                ObsidianEliteId, "Martillo viviente",
                ObsidianBossId, "Senor del Yunque Negro",
                3400, 120, 42, 1400, 155, 235,
                9000, 195, 70, 3500, 620, 900,
                28000, 300, 88, 14000, 4600, 6200,
                DefaultGameItems.ValleyMedal);
        }

        public static ZoneDefinition CreateZone8()
        {
            return CreateAdvancedZone(
                Zone8, "Jardin Astral", 71, 80, 490f, new Color(0.16f, 0.16f, 0.42f),
                AstralCreatureId, "Semilla estelar", new Color(0.42f, 0.52f, 0.95f),
                AstralEliteId, "Guardia zodiacal",
                AstralBossId, "Arbol de Constelaciones",
                4700, 155, 58, 2000, 250, 370,
                12500, 250, 92, 5000, 980, 1350,
                42000, 390, 118, 21000, 7000, 9800,
                DefaultGameItems.ProtectionRune);
        }

        public static ZoneDefinition CreateZone9()
        {
            return CreateAdvancedZone(
                Zone9, "Santuario del Eclipse", 81, 90, 560f, new Color(0.12f, 0.1f, 0.16f),
                EclipseCreatureId, "Devoto del eclipse", new Color(0.46f, 0.34f, 0.68f),
                EclipseEliteId, "Profeta sin sombra",
                EclipseBossId, "Sol Negro",
                6500, 195, 78, 2900, 360, 560,
                18000, 320, 124, 7200, 1500, 2100,
                65000, 500, 155, 32000, 10500, 14500,
                DefaultGameItems.ValleyAmulet);
        }

        public static ZoneDefinition CreateZone10()
        {
            return CreateAdvancedZone(
                Zone10, "Trono del Vacio", 91, 105, 630f, new Color(0.08f, 0.06f, 0.12f),
                ThroneCreatureId, "Heraldo del vacio", new Color(0.28f, 0.12f, 0.36f),
                ThroneEliteId, "Caballero sin nombre",
                ThroneBossId, "Rey Sin Alba",
                9000, 250, 105, 4300, 600, 850,
                26000, 420, 165, 10500, 2500, 3600,
                100000, 680, 220, 52000, 18000, 26000,
                DefaultGameItems.ProtectionRune,
                15);
        }

        private static ZoneDefinition CreateAdvancedZone(
            string zoneId, string displayName, int minLevel, int maxLevel, float zCenter, Color groundColor,
            string normalId, string normalName, Color normalColor,
            string eliteId, string eliteName,
            string bossId, string bossName,
            int normalHealth, int normalDamage, int normalDefense, int normalExp, int normalGoldMin, int normalGoldMax,
            int eliteHealth, int eliteDamage, int eliteDefense, int eliteExp, int eliteGoldMin, int eliteGoldMax,
            int bossHealth, int bossDamage, int bossDefense, int bossExp, int bossGoldMin, int bossGoldMax,
            string guaranteedDrop,
            int extraLevels = 10)
        {
            var zone = ScriptableObject.CreateInstance<ZoneDefinition>();
            zone.name = zoneId;
            zone.ZoneId = zoneId;
            zone.DisplayName = displayName;
            zone.MinLevel = minLevel;
            zone.MaxLevel = maxLevel;
            zone.HasOwnGround = true;
            zone.GroundCenter = new Vector3(0f, 0f, zCenter);
            zone.GroundColor = groundColor;
            zone.SignPosition = new Vector3(0f, 0f, zCenter - 32f);

            zone.NormalEnemyId = normalId;
            zone.NormalName = normalName;
            zone.NormalColor = normalColor;
            zone.NormalCount = extraLevels > 10 ? 9 : 7;
            zone.NormalAreaCenter = new Vector3(0f, 1f, zCenter - 8f);
            zone.NormalAreaRadius = 13f;
            zone.NormalHealth = normalHealth;
            zone.NormalDamage = normalDamage;
            zone.NormalDefense = normalDefense;
            zone.NormalExp = normalExp;
            zone.NormalGoldMin = normalGoldMin;
            zone.NormalGoldMax = normalGoldMax;
            zone.NormalMoveSpeed = 2.75f;

            zone.EliteEnemyId = eliteId;
            zone.EliteName = eliteName;
            zone.EliteCount = extraLevels > 10 ? 3 : 2;
            zone.EliteAreaCenter = new Vector3(16f, 1f, zCenter + 12f);
            zone.EliteAreaRadius = 7f;
            zone.EliteHealth = eliteHealth;
            zone.EliteDamage = eliteDamage;
            zone.EliteDefense = eliteDefense;
            zone.EliteExp = eliteExp;
            zone.EliteGoldMin = eliteGoldMin;
            zone.EliteGoldMax = eliteGoldMax;
            zone.EliteRespawnSeconds = 45f;

            zone.BossEnemyId = bossId;
            zone.BossName = bossName;
            zone.BossPosition = new Vector3(-18f, 1f, zCenter + 16f);
            zone.BossHealth = bossHealth;
            zone.BossDamage = bossDamage;
            zone.BossDefense = bossDefense;
            zone.BossExp = bossExp;
            zone.BossGoldMin = bossGoldMin;
            zone.BossGoldMax = bossGoldMax;
            zone.BossGuaranteedDrop = guaranteedDrop;
            zone.BossRespawnSeconds = 210f;
            zone.NormalTtkMin = 3f;
            zone.NormalTtkMax = 8f;
            zone.EliteTtkMin = 8f;
            zone.EliteTtkMax = 18f;
            zone.BossTtkMin = 30f;
            zone.BossTtkMax = 75f;

            return zone;
        }
    }
}
