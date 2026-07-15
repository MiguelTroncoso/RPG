using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        public Image PlayerHealthFill;
        public Text PlayerHealthText;
        public Image EnemyHealthFill;
        public Text EnemyHealthText;
        public Text StatusText;
        public Text ClassText;
        public Text ProgressionText;
        public Text SkillOneText;
        public Text SkillTwoText;
        public Text SkillThreeText;
        public Text SkillFourText;
        public Text UltimateSkillText;
        public Text InventoryText;
        public Text QuestText;
        public Text EquipmentText;
        public Text FeedText;
        public event Action SummaryChanged;

        private readonly List<string> feedLines = new List<string>();
        private Health playerHealth;
        private PlayerClassController classController;
        private PlayerCharacterIdentity identity;
        private PlayerProgression progression;
        private PlayerSkills skills;
        private InventorySystem inventory;
        private PlayerQuestLog questLog;
        private EquipmentUpgradeSystem equipment;
        private PlayerCombat combat;
        private Health watchedEnemy;
        private float statusUntil;
        private float feedUntil;

        public void Bind(
            Health player,
            PlayerClassController playerClass,
            PlayerCharacterIdentity playerIdentity,
            PlayerProgression playerProgression,
            PlayerSkills playerSkills,
            InventorySystem playerInventory,
            PlayerQuestLog playerQuestLog,
            EquipmentUpgradeSystem playerEquipment,
            PlayerCombat playerCombat)
        {
            playerHealth = player;
            classController = playerClass;
            identity = playerIdentity;
            progression = playerProgression;
            skills = playerSkills;
            inventory = playerInventory;
            questLog = playerQuestLog;
            equipment = playerEquipment;
            combat = playerCombat;
            UpdatePlayerHealth();
            UpdateEnemyHealth();
            RefreshClass();
            RefreshProgression();
            RefreshInventory();
            RefreshQuest();
            RefreshEquipment();
        }

        public void WatchEnemy(Health enemy)
        {
            watchedEnemy = enemy;
            UpdateEnemyHealth();
        }

        public void SetStatus(string message, float seconds = 2.25f)
        {
            if (StatusText == null)
            {
                return;
            }

            StatusText.text = message;
            statusUntil = Time.time + seconds;
        }

        private void Update()
        {
            UpdatePlayerHealth();
            UpdateEnemyHealth();
            RefreshSkillCooldowns(
                skills != null ? skills.SkillOneRemaining : 0f,
                skills != null ? skills.SkillTwoRemaining : 0f,
                skills != null ? skills.SkillThreeRemaining : 0f,
                skills != null ? skills.SkillFourRemaining : 0f,
                skills != null ? skills.UltimateRemaining : 0f);

            if (StatusText != null && statusUntil > 0f && Time.time > statusUntil)
            {
                StatusText.text = Localization.Tr("hud.hint");
                statusUntil = 0f;
            }

            if (FeedText != null && feedUntil > 0f && Time.time > feedUntil)
            {
                feedLines.Clear();
                FeedText.text = string.Empty;
                feedUntil = 0f;
            }
        }

        public void AddFeed(string message)
        {
            if (FeedText == null || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            feedLines.Add(message);
            while (feedLines.Count > 5)
            {
                feedLines.RemoveAt(0);
            }

            FeedText.text = string.Join("\n", feedLines);
            feedUntil = Time.time + 7f;
        }

        public void RefreshClass()
        {
            if (ClassText == null || classController == null || classController.Definition == null)
            {
                return;
            }

            var definition = classController.Definition;
            var damage = combat != null ? combat.TotalAttackDamage : definition.BaseDamage;
            var maxHealth = playerHealth != null ? playerHealth.MaxHealth : definition.MaxHealth;
            var identityLabel = identity != null ? $"{identity.CharacterName}  {identity.GenderLabel}" : definition.DisplayName;
            ClassText.text = $"{identityLabel}  {definition.DisplayName}  DMG {damage}  HP {maxHealth}";

            if (SkillOneText != null)
            {
                SkillOneText.text = skills != null ? skills.DisplayLabel(0) : $"Q {definition.SkillOneName}";
            }

            if (SkillTwoText != null)
            {
                SkillTwoText.text = skills != null ? skills.DisplayLabel(1) : $"E {definition.SkillTwoName}";
            }

            if (SkillThreeText != null)
            {
                SkillThreeText.text = skills != null ? skills.DisplayLabel(2) : $"R {definition.SkillThreeName}";
            }

            if (SkillFourText != null)
            {
                SkillFourText.text = skills != null ? skills.DisplayLabel(3) : $"F {definition.SkillFourName}";
            }

            if (UltimateSkillText != null)
            {
                UltimateSkillText.text = skills != null ? skills.DisplayLabel(4) : $"G {definition.UltimateSkillName}";
            }
        }

        public void RefreshProgression()
        {
            if (ProgressionText == null || progression == null)
            {
                return;
            }

            var bonusLabel = progression.ExperienceMultiplier > 1.001f
                ? Localization.Tr("hud.exp_bonus_suffix", ((progression.ExperienceMultiplier - 1f) * 100f).ToString("0"))
                : string.Empty;
            var rebirthLabel = progression.RebirthCount > 0
                ? Localization.Tr("hud.rebirth", progression.RebirthCount, progression.Renown)
                : string.Empty;
            var expLabel = progression.IsMaxLevel
                ? Localization.Tr("hud.exp_max")
                : Localization.Tr("hud.exp", progression.Experience, progression.NextLevelExperience);
            var pointsLabel = progression.AttributePoints > 0 ? Localization.Tr("hud.points_suffix", progression.AttributePoints) : string.Empty;
            ProgressionText.text = Localization.Tr("hud.progression", progression.Level, expLabel, progression.Gold, pointsLabel, rebirthLabel + bonusLabel);
        }

        public void RefreshInventory()
        {
            if (InventoryText != null)
            {
                InventoryText.text = inventory != null ? inventory.Summary() : Localization.Tr("inv.empty");
            }

            SummaryChanged?.Invoke();
        }

        public void RefreshQuest()
        {
            if (QuestText != null)
            {
                QuestText.text = questLog != null ? questLog.Summary() : Localization.Tr("quest.none");
            }

            SummaryChanged?.Invoke();
        }

        public void RefreshEquipment()
        {
            if (EquipmentText != null)
            {
                EquipmentText.text = equipment != null ? equipment.Summary() : Localization.Tr("hud.equipment", "-");
            }

            SummaryChanged?.Invoke();
        }

        public void RefreshSkillCooldowns(float skillOneRemaining, float skillTwoRemaining, float skillThreeRemaining, float skillFourRemaining, float ultimateRemaining)
        {
            if (classController == null || classController.Definition == null || skills == null)
            {
                return;
            }

            if (SkillOneText != null)
            {
                SkillOneText.text = skills.DisplayLabel(0, skillOneRemaining);
            }

            if (SkillTwoText != null)
            {
                SkillTwoText.text = skills.DisplayLabel(1, skillTwoRemaining);
            }

            if (SkillThreeText != null)
            {
                SkillThreeText.text = skills.DisplayLabel(2, skillThreeRemaining);
            }

            if (SkillFourText != null)
            {
                SkillFourText.text = skills.DisplayLabel(3, skillFourRemaining);
            }

            if (UltimateSkillText != null)
            {
                UltimateSkillText.text = skills.DisplayLabel(4, ultimateRemaining);
            }
        }

        private void UpdatePlayerHealth()
        {
            if (playerHealth == null)
            {
                return;
            }

            if (PlayerHealthFill != null)
            {
                PlayerHealthFill.fillAmount = playerHealth.Normalized;
            }

            if (PlayerHealthText != null)
            {
                PlayerHealthText.text = Localization.Tr("hud.player_hp", playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }

        private void UpdateEnemyHealth()
        {
            var hasEnemy = watchedEnemy != null && !watchedEnemy.IsDead;

            if (EnemyHealthFill != null)
            {
                EnemyHealthFill.fillAmount = hasEnemy ? watchedEnemy.Normalized : 0f;
            }

            if (EnemyHealthText != null)
            {
                EnemyHealthText.text = hasEnemy ? Localization.Tr("hud.target_hp", watchedEnemy.CurrentHealth, watchedEnemy.MaxHealth) : Localization.Tr("hud.no_target");
            }
        }
    }
}
