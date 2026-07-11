using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Registro de misiones data-driven: recorre la cadena de QuestDefinition
    // en orden, avanza objetivos cuando los demas sistemas notifican
    // (enemigo derrotado, item obtenido, NPC hablado) y entrega recompensas
    // via RewardService.
    public sealed class PlayerQuestLog : MonoBehaviour
    {
        public PrototypeHud Hud;
        public PlayerProgression Progression;
        public InventorySystem Inventory;

        private readonly List<QuestDefinition> questLine = new List<QuestDefinition>();
        private readonly List<string> completedIds = new List<string>();
        private QuestDefinition activeQuest;
        private int[] counters;

        public string ActiveQuestId => activeQuest != null ? activeQuest.QuestId : string.Empty;

        public void Initialize(List<QuestDefinition> quests)
        {
            questLine.Clear();
            if (quests != null)
            {
                foreach (var quest in quests)
                {
                    if (quest != null && quest.HasObjectives && !string.IsNullOrWhiteSpace(quest.QuestId))
                    {
                        questLine.Add(quest);
                    }
                }
            }

            if (activeQuest == null)
            {
                ActivateNext(announce: false);
            }
        }

        public void OnEnemyDefeated(EnemyTier tier, bool isWorldEvent)
        {
            if (isWorldEvent)
            {
                Progress(QuestObjectiveType.DefeatWorldEvent, string.Empty, 1);
                return;
            }

            // El TargetId del objetivo filtra por tier ("Elite", "Boss");
            // vacio = cualquier enemigo.
            Progress(QuestObjectiveType.KillEnemies, tier.ToString(), 1);
        }

        public void OnItemUpgraded()
        {
            Progress(QuestObjectiveType.UpgradeItem, string.Empty, 1);
        }

        public void OnItemAdded(string itemId, int amount)
        {
            Progress(QuestObjectiveType.CollectItems, itemId, amount);
        }

        public void OnNpcTalked(string npcId)
        {
            Progress(QuestObjectiveType.TalkToNpc, npcId, 1);
        }

        // Dialogo del NPC segun la mision activa: instruccion si la mision es
        // suya, null si no tiene nada que decir.
        public string DialogFor(string npcId)
        {
            if (activeQuest == null || activeQuest.GiverNpcId != npcId)
            {
                return null;
            }

            return activeQuest.StartDialog;
        }

        public string Summary()
        {
            if (activeQuest == null)
            {
                return completedIds.Count > 0
                    ? Localization.Tr("quest.all_done")
                    : Localization.Tr("quest.none");
            }

            var parts = new List<string>();
            for (var i = 0; i < activeQuest.Objectives.Length; i++)
            {
                var objective = activeQuest.Objectives[i];
                var current = counters != null && i < counters.Length ? counters[i] : 0;
                parts.Add($"{objective.Label} {current}/{objective.RequiredCount}");
            }

            return Localization.Tr("quest.summary", activeQuest.Title, string.Join(" | ", parts));
        }

        public QuestSaveData Export()
        {
            var data = new QuestSaveData { ActiveQuestId = ActiveQuestId };
            data.CompletedQuestIds.AddRange(completedIds);
            if (counters != null)
            {
                data.Counters.AddRange(counters);
            }

            return data;
        }

        public void Restore(QuestSaveData data)
        {
            if (data == null)
            {
                return;
            }

            completedIds.Clear();
            if (data.CompletedQuestIds != null)
            {
                completedIds.AddRange(data.CompletedQuestIds);
            }

            activeQuest = null;
            counters = null;

            if (!string.IsNullOrWhiteSpace(data.ActiveQuestId))
            {
                var quest = FindQuest(data.ActiveQuestId);
                if (quest != null)
                {
                    activeQuest = quest;
                    counters = new int[quest.Objectives.Length];
                    for (var i = 0; i < counters.Length; i++)
                    {
                        var saved = data.Counters != null && i < data.Counters.Count ? data.Counters[i] : 0;
                        counters[i] = Mathf.Clamp(saved, 0, quest.Objectives[i].RequiredCount);
                    }
                }
            }

            if (activeQuest == null)
            {
                ActivateNext(announce: false);
            }

            Hud?.RefreshQuest();
        }

        private void Progress(QuestObjectiveType type, string targetId, int amount)
        {
            if (activeQuest == null || counters == null || amount <= 0)
            {
                return;
            }

            var changed = false;
            for (var i = 0; i < activeQuest.Objectives.Length; i++)
            {
                var objective = activeQuest.Objectives[i];
                if (objective.Type != type)
                {
                    continue;
                }

                var matchesTarget = string.IsNullOrEmpty(objective.TargetId) || objective.TargetId == targetId;
                if (!matchesTarget || counters[i] >= objective.RequiredCount)
                {
                    continue;
                }

                counters[i] = Mathf.Min(objective.RequiredCount, counters[i] + amount);
                changed = true;
                Hud?.AddFeed(Localization.Tr("quest.feed_progress", objective.Label, counters[i], objective.RequiredCount));
            }

            if (!changed)
            {
                return;
            }

            if (AllObjectivesComplete())
            {
                CompleteActiveQuest();
            }

            Hud?.RefreshQuest();
        }

        private bool AllObjectivesComplete()
        {
            for (var i = 0; i < activeQuest.Objectives.Length; i++)
            {
                if (counters[i] < activeQuest.Objectives[i].RequiredCount)
                {
                    return false;
                }
            }

            return true;
        }

        private void CompleteActiveQuest()
        {
            var quest = activeQuest;
            completedIds.Add(quest.QuestId);
            activeQuest = null;
            counters = null;

            RewardService.Grant(quest.Reward, Progression, Inventory, Hud, $"Mision '{quest.Title}'");
            Hud?.SetStatus(Localization.Tr("quest.completed", quest.Title, quest.CompleteDialog), 5f);

            if (!string.IsNullOrWhiteSpace(quest.NextQuestId))
            {
                Activate(FindQuest(quest.NextQuestId), announce: true);
            }
            else
            {
                ActivateNext(announce: true);
            }
        }

        private void ActivateNext(bool announce)
        {
            foreach (var quest in questLine)
            {
                if (!completedIds.Contains(quest.QuestId))
                {
                    Activate(quest, announce);
                    return;
                }
            }

            Hud?.RefreshQuest();
        }

        private void Activate(QuestDefinition quest, bool announce)
        {
            if (quest == null || completedIds.Contains(quest.QuestId))
            {
                ActivateNext(announce);
                return;
            }

            activeQuest = quest;
            counters = new int[quest.Objectives.Length];

            // Los objetivos de recoleccion cuentan lo que ya hay en el
            // inventario al activarse.
            for (var i = 0; i < quest.Objectives.Length; i++)
            {
                var objective = quest.Objectives[i];
                if (objective.Type == QuestObjectiveType.CollectItems && Inventory != null && !string.IsNullOrEmpty(objective.TargetId))
                {
                    counters[i] = Mathf.Min(objective.RequiredCount, Inventory.Count(objective.TargetId));
                }
            }

            if (announce)
            {
                Hud?.AddFeed(Localization.Tr("quest.new", quest.Title));
            }

            if (AllObjectivesComplete())
            {
                CompleteActiveQuest();
                return;
            }

            Hud?.RefreshQuest();
        }

        private QuestDefinition FindQuest(string questId)
        {
            foreach (var quest in questLine)
            {
                if (quest.QuestId == questId)
                {
                    return quest;
                }
            }

            return null;
        }
    }
}
