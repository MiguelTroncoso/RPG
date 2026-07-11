using UnityEngine;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    // Ventana de atributos: solo presentacion. Las reglas de gasto viven en
    // PlayerAttributes/PlayerProgression.
    public sealed class StatsWindowController : MonoBehaviour
    {
        public GameObject Panel;
        public Text PointsText;
        public Text StrengthText;
        public Text VitalityText;
        public Text AgilityText;
        public PlayerAttributes Attributes;
        public PlayerProgression Progression;

        public void Toggle()
        {
            if (Panel == null)
            {
                return;
            }

            Panel.SetActive(!Panel.activeSelf);
            if (Panel.activeSelf)
            {
                Refresh();
            }
        }

        public void SpendStrength()
        {
            Spend(AttributeType.Strength);
        }

        public void SpendVitality()
        {
            Spend(AttributeType.Vitality);
        }

        public void SpendAgility()
        {
            Spend(AttributeType.Agility);
        }

        private void Spend(AttributeType attribute)
        {
            if (Attributes != null && Attributes.TrySpend(attribute))
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            if (Attributes == null)
            {
                return;
            }

            if (PointsText != null && Progression != null)
            {
                PointsText.text = $"Puntos disponibles: {Progression.AttributePoints}";
            }

            var config = Attributes.Config;

            if (StrengthText != null)
            {
                StrengthText.text = $"Fuerza {Attributes.Strength}  (+{Attributes.BonusDamage} dano, +{config?.DamagePerStrength ?? 1}/punto)";
            }

            if (VitalityText != null)
            {
                VitalityText.text = $"Vitalidad {Attributes.Vitality}  (+{Attributes.BonusMaxHealth} vida, +{config?.HealthPerVitality ?? 8}/punto)";
            }

            if (AgilityText != null)
            {
                AgilityText.text = $"Agilidad {Attributes.Agility}  (+{Attributes.BonusMoveSpeed:0.00} vel, +{Attributes.BonusCritChance * 100f:0.0}% crit)";
            }
        }
    }
}
