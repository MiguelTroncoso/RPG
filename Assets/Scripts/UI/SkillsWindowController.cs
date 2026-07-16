using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    public sealed class SkillsWindowController : MonoBehaviour
    {
        public GameObject Panel;
        public PlayerSkills Skills;
        public Text TitleText;
        public Text BodyText;

        private float nextRefresh;

        private void OnEnable()
        {
            Refresh();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh)
            {
                return;
            }

            nextRefresh = Time.unscaledTime + 0.5f;
            Refresh();
        }

        public void Toggle()
        {
            if (Panel != null)
            {
                Panel.SetActive(!Panel.activeSelf);
            }
        }

        public void Upgrade(int slot)
        {
            Skills?.UpgradeSkill(slot);
            Refresh();
        }

        public void Refresh()
        {
            if (BodyText == null || Skills == null)
            {
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine(Localization.Tr("ui.skills_summary"));
            builder.AppendLine(Localization.Tr("ui.skills_material_hint"));
            builder.AppendLine();
            for (var slot = 0; slot < PlayerSkills.SkillSlotCount; slot++)
            {
                var unlocked = Skills.IsSkillUnlocked(slot);
                var level = Skills.SkillLevelFor(slot);
                var max = Skills.MaxSkillLevelFor(slot);
                var state = unlocked
                    ? Localization.Tr("ui.skill_level", level, max)
                    : Localization.Tr("ui.skill_unlock", Skills.UnlockLevelFor(slot));
                builder.AppendLine(Localization.Tr("ui.skill_row", KeyFor(slot), state, Skills.EnergyCostFor(slot).ToString("0")));
            }

            BodyText.text = builder.ToString();
        }

        private static string KeyFor(int slot)
        {
            switch (slot)
            {
                case 0: return "Q";
                case 1: return "E";
                case 2: return "R";
                case 3: return "F";
                default: return "G";
            }
        }
    }
}
