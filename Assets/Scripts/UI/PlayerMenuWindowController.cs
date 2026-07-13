using UnityEngine;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    public sealed class PlayerMenuWindowController : MonoBehaviour
    {
        private enum MenuTab
        {
            Inventory,
            Equipment,
            Quest
        }

        public GameObject Panel;
        public Text TitleText;
        public Text ContentText;
        public PrototypeHud Hud;
        public InventorySystem Inventory;
        public PlayerEquipment Equipment;
        public PlayerQuestLog QuestLog;
        public StatsWindowController StatsWindow;
        public TelemetryWindowController TelemetryWindow;

        private MenuTab activeTab = MenuTab.Inventory;

        private void OnEnable()
        {
            if (Hud != null)
            {
                Hud.SummaryChanged += Refresh;
                Refresh();
            }
        }

        private void OnDisable()
        {
            if (Hud != null)
            {
                Hud.SummaryChanged -= Refresh;
            }
        }

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

        public void ShowInventory()
        {
            activeTab = MenuTab.Inventory;
            OpenAndRefresh();
        }

        public void ShowEquipment()
        {
            activeTab = MenuTab.Equipment;
            OpenAndRefresh();
        }

        public void ShowQuest()
        {
            activeTab = MenuTab.Quest;
            OpenAndRefresh();
        }

        public void ShowStats()
        {
            Panel?.SetActive(false);
            StatsWindow?.Toggle();
        }

        public void ShowTelemetry()
        {
            Panel?.SetActive(false);
            TelemetryWindow?.Toggle();
        }

        public void Refresh()
        {
            if (ContentText == null)
            {
                return;
            }

            switch (activeTab)
            {
                case MenuTab.Equipment:
                    TitleText.text = Localization.Tr("ui.menu_equipment");
                    ContentText.text = Equipment != null ? Equipment.DetailedSummary() : Localization.Tr("menu.empty");
                    break;
                case MenuTab.Quest:
                    TitleText.text = Localization.Tr("ui.menu_quest");
                    ContentText.text = QuestLog != null ? QuestLog.DetailedSummary() : Localization.Tr("quest.none");
                    break;
                default:
                    TitleText.text = Localization.Tr("ui.menu_inventory");
                    ContentText.text = Inventory != null ? Inventory.DetailedSummary() : Localization.Tr("inv.empty");
                    break;
            }
        }

        private void OpenAndRefresh()
        {
            if (Panel != null && !Panel.activeSelf)
            {
                Panel.SetActive(true);
            }

            Refresh();
        }
    }
}
