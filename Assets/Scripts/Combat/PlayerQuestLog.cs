using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class PlayerQuestLog : MonoBehaviour
    {
        private const int KillGoal = 6;
        private const int FragmentGoal = 2;
        private const int MonolithGoal = 1;

        public PrototypeHud Hud;
        public PlayerProgression Progression;
        public InventorySystem Inventory;

        private int kills;
        private int fragments;
        private int monoliths;
        private bool completed;

        public string Summary()
        {
            if (completed)
            {
                return "Mision: Valle estabilizado. Esperando nueva tarea.";
            }

            return $"Mision: elimina {kills}/{KillGoal}, fragmentos {fragments}/{FragmentGoal}, monolito {monoliths}/{MonolithGoal}";
        }

        public void OnEnemyDefeated(bool isWorldEvent)
        {
            if (!isWorldEvent)
            {
                kills = Mathf.Min(KillGoal, kills + 1);
            }
            else
            {
                monoliths = Mathf.Min(MonolithGoal, monoliths + 1);
            }

            CheckCompletion();
            Hud?.RefreshQuest();
            Hud?.AddFeed(isWorldEvent ? "Evento: monolito destruido" : $"Mision: enemigo {kills}/{KillGoal}");
        }

        public void OnItemAdded(string itemId, int amount)
        {
            if (itemId == DefaultGameItems.AncientFragment)
            {
                fragments = Mathf.Min(FragmentGoal, fragments + amount);
                CheckCompletion();
                Hud?.RefreshQuest();
                Hud?.AddFeed($"Mision: fragmentos {fragments}/{FragmentGoal}");
            }
        }

        private void CheckCompletion()
        {
            if (completed || kills < KillGoal || fragments < FragmentGoal || monoliths < MonolithGoal)
            {
                return;
            }

            completed = true;
            Progression?.AddExperience(150);
            Progression?.AddGold(75);
            Inventory?.AddItem(DefaultGameItems.ValleyMedal);
            Hud?.SetStatus("Mision completada: recibiste EXP, oro y una medalla.", 4f);
            Hud?.AddFeed("Mision completada: +150 EXP, +75 oro");
        }
    }
}
