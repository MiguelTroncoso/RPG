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
        public Text InventoryText;
        public Text QuestText;
        public Text EquipmentText;
        public Text FeedText;

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
            RefreshSkillCooldowns(skills != null ? skills.SkillOneRemaining : 0f, skills != null ? skills.SkillTwoRemaining : 0f);

            if (StatusText != null && statusUntil > 0f && Time.time > statusUntil)
            {
                StatusText.text = "Cambia clase, completa la mision, mejora equipo y busca el monolito.";
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
            var damage = combat != null ? combat.AttackDamage + combat.EquipmentDamageBonus : definition.BaseDamage;
            var maxHealth = playerHealth != null ? playerHealth.MaxHealth : definition.MaxHealth;
            var identityLabel = identity != null ? $"{identity.CharacterName}  {identity.GenderLabel}" : definition.DisplayName;
            ClassText.text = $"{identityLabel}  {definition.DisplayName}  DMG {damage}  HP {maxHealth}";

            if (SkillOneText != null)
            {
                SkillOneText.text = $"Q {definition.SkillOneName}";
            }

            if (SkillTwoText != null)
            {
                SkillTwoText.text = $"E {definition.SkillTwoName}";
            }
        }

        public void RefreshProgression()
        {
            if (ProgressionText == null || progression == null)
            {
                return;
            }

            ProgressionText.text = $"Nivel {progression.Level}  EXP {progression.Experience}/{progression.NextLevelExperience}  Oro {progression.Gold}";
        }

        public void RefreshInventory()
        {
            if (InventoryText != null)
            {
                InventoryText.text = inventory != null ? inventory.Summary() : "Inventario: -";
            }
        }

        public void RefreshQuest()
        {
            if (QuestText != null)
            {
                QuestText.text = questLog != null ? questLog.Summary() : "Mision: -";
            }
        }

        public void RefreshEquipment()
        {
            if (EquipmentText != null)
            {
                EquipmentText.text = equipment != null ? equipment.Summary() : "Equipo: -";
            }
        }

        public void RefreshSkillCooldowns(float skillOneRemaining, float skillTwoRemaining)
        {
            if (classController == null || classController.Definition == null)
            {
                return;
            }

            if (SkillOneText != null)
            {
                SkillOneText.text = skillOneRemaining > 0.1f
                    ? $"Q {classController.Definition.SkillOneName} {skillOneRemaining:0.0}s"
                    : $"Q {classController.Definition.SkillOneName}";
            }

            if (SkillTwoText != null)
            {
                SkillTwoText.text = skillTwoRemaining > 0.1f
                    ? $"E {classController.Definition.SkillTwoName} {skillTwoRemaining:0.0}s"
                    : $"E {classController.Definition.SkillTwoName}";
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
                PlayerHealthText.text = $"Jugador {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}";
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
                EnemyHealthText.text = hasEnemy ? $"Objetivo {watchedEnemy.CurrentHealth}/{watchedEnemy.MaxHealth}" : "Sin objetivo";
            }
        }
    }
}
