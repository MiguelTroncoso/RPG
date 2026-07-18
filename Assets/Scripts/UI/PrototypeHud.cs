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
        public Image PlayerEnergyFill;
        public Text PlayerEnergyText;
        public Image PlayerExperienceFill;
        public Text PlayerNameText;
        public Text PlayerLevelText;
        public Image EnemyHealthFill;
        public Text EnemyHealthText;
        public Text EnemyNameText;
        public GameObject TargetPanel;
        public GameObject[] TargetElements;
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
        public UiCooldownOverlay[] SkillCooldowns;
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
            var energy = player.GetComponent<PlayerEnergySystem>();
            UpdatePlayerEnergy(energy);
            UpdatePlayerHealth();
            UpdatePlayerExperience();
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
            UpdatePlayerEnergy(playerHealth != null ? playerHealth.GetComponent<PlayerEnergySystem>() : null);
            UpdatePlayerExperience();
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
            ClassText.text = $"{definition.DisplayName}  DMG {damage}";

            if (SkillOneText != null)
            {
                SkillOneText.text = CompactSkillLabel(0, 0f);
            }

            if (SkillTwoText != null)
            {
                SkillTwoText.text = CompactSkillLabel(1, 0f);
            }

            if (SkillThreeText != null)
            {
                SkillThreeText.text = CompactSkillLabel(2, 0f);
            }

            if (SkillFourText != null)
            {
                SkillFourText.text = CompactSkillLabel(3, 0f);
            }

            if (UltimateSkillText != null)
            {
                UltimateSkillText.text = CompactSkillLabel(4, 0f);
            }
        }

        public void RefreshProgression()
        {
            if (ProgressionText == null || progression == null)
            {
                return;
            }

            var expLabel = progression.IsMaxLevel
                ? "MAX"
                : $"{progression.Experience}/{progression.NextLevelExperience}";
            var pointsLabel = progression.AttributePoints > 0 ? $"  P {progression.AttributePoints}" : string.Empty;
            ProgressionText.text = $"Nv {progression.Level}  EXP {expLabel}  Oro {progression.Gold}{pointsLabel}";
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
                SkillOneText.text = CompactSkillLabel(0, skillOneRemaining);
            }

            if (SkillTwoText != null)
            {
                SkillTwoText.text = CompactSkillLabel(1, skillTwoRemaining);
            }

            if (SkillThreeText != null)
            {
                SkillThreeText.text = CompactSkillLabel(2, skillThreeRemaining);
            }

            if (SkillFourText != null)
            {
                SkillFourText.text = CompactSkillLabel(3, skillFourRemaining);
            }

            if (UltimateSkillText != null)
            {
                UltimateSkillText.text = CompactSkillLabel(4, ultimateRemaining);
            }

            if (SkillCooldowns != null)
            {
                var remaining = new[]
                {
                    skillOneRemaining,
                    skillTwoRemaining,
                    skillThreeRemaining,
                    skillFourRemaining,
                    ultimateRemaining
                };

                for (var i = 0; i < SkillCooldowns.Length && i < remaining.Length; i++)
                {
                    SkillCooldowns[i]?.SetRemaining(remaining[i], skills.CooldownDurationFor(i));
                }
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

        private string CompactSkillLabel(int slot, float remaining)
        {
            var keys = new[] { "Q", "E", "R", "F", "G" };
            var key = slot >= 0 && slot < keys.Length ? keys[slot] : "?";
            if (skills == null)
            {
                return key;
            }

            if (!skills.IsSkillUnlocked(slot))
            {
                return $"{key}\nNv{skills.UnlockLevelFor(slot)}";
            }

            return remaining > 0.05f
                ? $"{key}\n{Mathf.CeilToInt(remaining)}s"
                : $"{key}\nNv{skills.SkillLevelFor(slot)}";
        }

        private void UpdatePlayerExperience()
        {
            if (progression == null)
            {
                return;
            }

            if (PlayerExperienceFill != null)
            {
                PlayerExperienceFill.fillAmount = progression.IsMaxLevel
                    ? 1f
                    : Mathf.Clamp01(progression.Experience / (float)Mathf.Max(1, progression.NextLevelExperience));
            }

            if (PlayerNameText != null && identity != null)
            {
                PlayerNameText.text = identity.CharacterName;
            }

            if (PlayerLevelText != null)
            {
                PlayerLevelText.text = progression.IsMaxLevel
                    ? Localization.Tr("hud.level_max", progression.Level)
                    : Localization.Tr("hud.level_short", progression.Level);
            }
        }

        private void UpdateEnemyHealth()
        {
            var hasEnemy = watchedEnemy != null && !watchedEnemy.IsDead;

            if (TargetElements != null && TargetElements.Length > 0)
            {
                for (var i = 0; i < TargetElements.Length; i++)
                {
                    TargetElements[i]?.SetActive(hasEnemy);
                }
            }
            else if (TargetPanel != null)
            {
                TargetPanel.SetActive(hasEnemy);
            }

            if (EnemyHealthFill != null)
            {
                EnemyHealthFill.fillAmount = hasEnemy ? watchedEnemy.Normalized : 0f;
            }

            if (EnemyHealthText != null)
            {
                EnemyHealthText.text = hasEnemy ? Localization.Tr("hud.target_hp", watchedEnemy.CurrentHealth, watchedEnemy.MaxHealth) : Localization.Tr("hud.no_target");
            }

            if (EnemyNameText != null)
            {
                if (!hasEnemy)
                {
                    EnemyNameText.text = Localization.Tr("hud.no_target");
                }
                else
                {
                    var reward = watchedEnemy.GetComponent<EnemyReward>();
                    var tier = reward != null ? reward.Tier.ToString().ToUpperInvariant() : "NORMAL";
                    EnemyNameText.text = $"{tier}  {watchedEnemy.gameObject.name}";
                }
            }
        }

        private void UpdatePlayerEnergy(PlayerEnergySystem energy)
        {
            if (energy == null)
            {
                return;
            }

            if (PlayerEnergyFill != null)
            {
                PlayerEnergyFill.fillAmount = energy.Normalized;
            }

            if (PlayerEnergyText != null)
            {
                PlayerEnergyText.text = $"{Mathf.RoundToInt(energy.CurrentEnergy)}/{Mathf.RoundToInt(energy.MaxEnergy)}";
            }
        }
    }
}
