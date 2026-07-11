using System;
using UnityEngine;

namespace MmorpgPrototype
{
    public enum QuestObjectiveType
    {
        TalkToNpc,
        KillEnemies,
        CollectItems,
        DefeatWorldEvent
    }

    [Serializable]
    public struct QuestObjectiveData
    {
        public QuestObjectiveType Type;

        // NpcId para TalkToNpc, ItemId para CollectItems; vacio = cualquiera
        // (KillEnemies/DefeatWorldEvent normalmente no filtran).
        public string TargetId;
        [Min(1)] public int RequiredCount;
        public string Label;
    }

    [Serializable]
    public struct ItemReward
    {
        public string ItemId;
        [Min(1)] public int Count;
    }

    // Estructura flexible de recompensa: cualquier fuente (mision, nivel,
    // cofre) puede entregar uno o varios tipos a la vez via RewardService.
    [Serializable]
    public class RewardBundle
    {
        public int Experience;
        public int Gold;
        public ItemReward[] Items;
    }

    [CreateAssetMenu(menuName = "MMORPG/Quests/Quest Definition", fileName = "QuestDefinition")]
    public sealed class QuestDefinition : ScriptableObject
    {
        public string QuestId;
        public int SortOrder;
        public string Title;
        [TextArea] public string StartDialog;
        [TextArea] public string CompleteDialog;
        public string GiverNpcId;
        public QuestObjectiveData[] Objectives;
        public RewardBundle Reward;
        public string NextQuestId;

        public bool HasObjectives => Objectives != null && Objectives.Length > 0;
    }
}
