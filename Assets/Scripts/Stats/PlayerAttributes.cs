using UnityEngine;

namespace MmorpgPrototype
{
    // Puntos de atributo gastados por el jugador. Los bonos resultantes se
    // aplican en el recomputo central (EquipmentUpgradeSystem.ApplyBonuses)
    // y en las stats de ataque; aqui solo viven los contadores y la regla.
    public sealed class PlayerAttributes : MonoBehaviour
    {
        public AttributeConfig Config;
        public PlayerProgression Progression;
        public EquipmentUpgradeSystem UpgradeSystem;
        public PrototypeHud Hud;

        public int Strength { get; private set; }
        public int Vitality { get; private set; }
        public int Agility { get; private set; }

        public int BonusDamage => Config != null ? Strength * Config.DamagePerStrength : Strength;
        public int BonusMaxHealth => Config != null ? Vitality * Config.HealthPerVitality : Vitality * 8;
        public float BonusMoveSpeed => Config != null ? Agility * Config.MoveSpeedPerAgility : Agility * 0.02f;
        public float BonusCritChance => Config != null ? Agility * Config.CritChancePerAgility : Agility * 0.002f;

        public bool TrySpend(AttributeType attribute)
        {
            if (Progression == null || !Progression.TrySpendAttributePoint())
            {
                Hud?.SetStatus(Localization.Tr("attr.no_points"));
                return false;
            }

            switch (attribute)
            {
                case AttributeType.Strength:
                    Strength++;
                    Hud?.AddFeed(Localization.Tr("attr.strength_up", Strength, Config != null ? Config.DamagePerStrength : 1));
                    break;
                case AttributeType.Vitality:
                    Vitality++;
                    Hud?.AddFeed(Localization.Tr("attr.vitality_up", Vitality, Config != null ? Config.HealthPerVitality : 8));
                    break;
                default:
                    Agility++;
                    Hud?.AddFeed(Localization.Tr("attr.agility_up", Agility));
                    break;
            }

            UpgradeSystem?.ApplyBonuses();
            return true;
        }

        public void Restore(int strength, int vitality, int agility)
        {
            Strength = Mathf.Max(0, strength);
            Vitality = Mathf.Max(0, vitality);
            Agility = Mathf.Max(0, agility);
        }
    }
}
