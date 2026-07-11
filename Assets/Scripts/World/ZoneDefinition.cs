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

        [Header("Area de elites")]
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
