using System.Text;
using System.Collections.Generic;
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
        public readonly List<Text> NodeTexts = new List<Text>();
        public readonly List<Image> NodeBackgrounds = new List<Image>();

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

            for (var i = 0; i < NodeTexts.Count && i < PlayerSkills.SkillSlotCount; i++)
            {
                var unlocked = Skills.IsSkillUnlocked(i);
                var state = unlocked
                    ? Localization.Tr("ui.skill_level", Skills.SkillLevelFor(i), Skills.MaxSkillLevelFor(i))
                    : Localization.Tr("ui.skill_unlock", Skills.UnlockLevelFor(i));
                NodeTexts[i].text = $"{KeyFor(i)}\n{Skills.SkillNameForDisplay(i)}\n{state}";
                NodeTexts[i].color = unlocked ? Color.white : new Color(0.58f, 0.64f, 0.7f);
                if (i < NodeBackgrounds.Count && NodeBackgrounds[i] != null)
                {
                    NodeBackgrounds[i].color = unlocked
                        ? Color.Lerp(new Color(0.06f, 0.11f, 0.14f), new Color(0.16f, 0.42f, 0.46f), 0.38f)
                        : new Color(0.045f, 0.055f, 0.065f, 0.96f);
                }
            }
        }

        public void RegisterNode(Text text, Image background)
        {
            if (text != null)
            {
                NodeTexts.Add(text);
            }

            NodeBackgrounds.Add(background);
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
