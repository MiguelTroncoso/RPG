using UnityEngine;

namespace MmorpgPrototype
{
    // Zona por rango de nivel: nombre, elites y jefe como datos. La Zona 1
    // es la unica construida; las siguientes se crean con este mismo molde.
    [CreateAssetMenu(menuName = "MMORPG/World/Zone Definition", fileName = "ZoneDefinition")]
    public sealed class ZoneDefinition : ScriptableObject
    {
        public string ZoneId = "valley_zone1";
        public string DisplayName = "Valle de las Reliquias";
        [Min(1)] public int MinLevel = 1;
        [Min(1)] public int MaxLevel = 10;
        public bool MountsAllowed = true;

        [Header("Terreno y cartel")]
        public bool HasOwnGround;
        public Vector3 GroundCenter = Vector3.zero;
        public Color GroundColor = new Color(0.22f, 0.34f, 0.24f);
        public Vector3 SignPosition = new Vector3(0f, 0f, -6.5f);

        [Header("Enemigos normales")]
        public string NormalEnemyId = "valley_creature";
        public string NormalName = "Lobo corrupto";
        public Color NormalColor = new Color(0.78f, 0.18f, 0.16f);
        [Min(0)] public int NormalCount = 5;
        public Vector3 NormalAreaCenter = new Vector3(0f, 1f, 4f);
        [Min(1f)] public float NormalAreaRadius = 9f;
        [Min(1)] public int NormalHealth = 90;
        [Min(0)] public int NormalDamage = 8;
        [Min(0)] public int NormalDefense;
        [Min(0)] public int NormalExp = 34;
        [Min(0)] public int NormalGoldMin = 3;
        [Min(0)] public int NormalGoldMax = 9;
        [Min(0.5f)] public float NormalMoveSpeed = 2.2f;

        [Header("Area de elites")]
        public string EliteEnemyId = "valley_elite";
        public string EliteName = "Acechador del claro";
        [Min(0)] public int EliteCount = 2;
        public Vector3 EliteAreaCenter = new Vector3(18f, 1f, 14f);
        [Min(1f)] public float EliteAreaRadius = 5f;
        [Min(1)] public int EliteHealth = 240;
        [Min(0)] public int EliteDamage = 15;
        [Min(0)] public int EliteDefense = 3;
        [Min(0)] public int EliteExp = 110;
        [Min(0)] public int EliteGoldMin = 18;
        [Min(0)] public int EliteGoldMax = 30;
        [Min(1f)] public float EliteRespawnSeconds = 25f;

        [Header("Jefe de zona")]
        public string BossEnemyId = "valley_boss";
        public string BossName = "Coloso de las Reliquias";
        public Vector3 BossPosition = new Vector3(-22f, 1f, 20f);
        [Min(1)] public int BossHealth = 900;
        [Min(0)] public int BossDamage = 22;
        [Min(0)] public int BossDefense = 5;
        [Min(0)] public int BossExp = 400;
        [Min(0)] public int BossGoldMin = 110;
        [Min(0)] public int BossGoldMax = 160;
        public string BossGuaranteedDrop = DefaultGameItems.ValleyAmulet;
        [Min(1f)] public float BossRespawnSeconds = 90f;
    }
}
