using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    // Converts the old text summaries into compact, readable cards without
    // changing inventory, equipment or quest rules.
    public sealed class PlayerMenuVisualContent : MonoBehaviour
    {
        public RectTransform Root;
        public PlayerMenuWindowController Menu;
        public InventorySystem Inventory;
        public PlayerEquipment Equipment;
        public PlayerQuestLog QuestLog;
        public ItemDatabase Database;

        private readonly List<GameObject> generated = new List<GameObject>();

        private void OnEnable()
        {
            if (Menu != null)
            {
                Menu.VisualRefreshRequested += Refresh;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (Menu != null)
            {
                Menu.VisualRefreshRequested -= Refresh;
            }
        }

        public void Refresh()
        {
            if (Root == null || Menu == null)
            {
                return;
            }

            Clear();
            switch (Menu.ActiveTab)
            {
                case PlayerMenuWindowController.MenuTab.Equipment:
                    BuildEquipment();
                    break;
                case PlayerMenuWindowController.MenuTab.Quest:
                    BuildQuest();
                    break;
                default:
                    BuildInventory();
                    break;
            }
        }

        private void BuildInventory()
        {
            CreateHeader("OBJETOS", "Inventario del aventurero");
            var grid = CreateGrid("Inventory Grid", 4);
            if (Inventory == null || Inventory.Items.Count == 0)
            {
                CreateCard(grid.transform, "VACIO", "Aun no tienes objetos", UiThemeConfig.Runtime.TextMuted, 180f, 110f);
                return;
            }

            foreach (var instance in Inventory.Items)
            {
                var definition = Database != null ? Database.Get(instance.ItemId) : null;
                var accent = Database != null ? Database.ColorOf(instance.ItemId) : UiThemeConfig.Runtime.Accent;
                var title = definition != null ? definition.DisplayName : instance.ItemId;
                var details = $"{CategoryCode(definition)}\nCantidad x{Mathf.Max(1, instance.Quantity)}";
                CreateCard(grid.transform, title, details, accent, 180f, 110f);
            }
        }

        private void BuildEquipment()
        {
            CreateHeader("EQUIPO", "Ranuras, rareza y mejoras");
            var grid = CreateGrid("Equipment Grid", 4);
            var slots = new[]
            {
                EquipSlot.Weapon, EquipSlot.Helmet, EquipSlot.Chest, EquipSlot.Gloves,
                EquipSlot.Pants, EquipSlot.Boots, EquipSlot.Cape, EquipSlot.Necklace,
                EquipSlot.RingLeft, EquipSlot.RingRight, EquipSlot.Bracelet, EquipSlot.Belt,
                EquipSlot.Talisman
            };

            foreach (var slot in slots)
            {
                var instance = Equipment != null ? Equipment.GetEquipped(slot) : null;
                var definition = instance != null && Database != null
                    ? Database.Get(instance.ItemId) as EquipmentItemDefinition
                    : null;
                var accent = definition != null && Database != null
                    ? Database.ColorOf(definition.ItemId)
                    : UiThemeConfig.Runtime.TextMuted;
                var title = definition != null ? definition.DisplayName : Localization.Tr("menu.empty_slot");
                var upgrade = instance != null && instance.UpgradeLevel > 0 ? $" +{instance.UpgradeLevel}" : string.Empty;
                CreateCard(grid.transform, SlotCode(slot), $"{title}{upgrade}", accent, 180f, 94f);
            }
        }

        private void BuildQuest()
        {
            CreateHeader("MISIONES Y EVENTOS", "Campana, contratos y actividad");
            var questText = QuestLog != null ? QuestLog.DetailedSummary() : Localization.Tr("quest.none");
            var missionCard = CreateCard(Root, "MISION ACTIVA", questText, UiThemeConfig.Runtime.AccentGold, 730f, 220f);
            SetRect(missionCard.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(730f, 220f), new Vector2(24f, -68f));
            var activityCard = CreateCard(Root, "ACTIVIDAD", "Contratos diarios\nEventos semanales\nTemporada y jefe mundial", UiThemeConfig.Runtime.Accent, 730f, 110f);
            SetRect(activityCard.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(730f, 110f), new Vector2(24f, -300f));
        }

        private void CreateHeader(string title, string subtitle)
        {
            var titleText = CreateText("Menu Section Title", title, 22, TextAnchor.MiddleLeft);
            SetRect(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(720f, 30f), new Vector2(24f, -8f));
            titleText.fontStyle = FontStyle.Bold;

            var subtitleText = CreateText("Menu Section Subtitle", subtitle, 14, TextAnchor.MiddleLeft);
            subtitleText.color = UiThemeConfig.Runtime.TextMuted;
            SetRect(subtitleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(720f, 24f), new Vector2(24f, -38f));
        }

        private GridLayoutGroup CreateGrid(string name, int columns)
        {
            var objectRoot = CreateObject(name);
            var rect = objectRoot.GetComponent<RectTransform>();
            SetRect(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(760f, 260f), new Vector2(24f, -68f));
            var grid = objectRoot.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(180f, 104f);
            grid.spacing = new Vector2(10f, 10f);
            grid.padding = new RectOffset(4, 4, 4, 4);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            return grid;
        }

        private GameObject CreateCard(Transform parent, string title, string details, Color accent, float width, float height)
        {
            var card = CreateObject("Menu Card");
            card.transform.SetParent(parent, false);
            var rect = card.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);
            var image = card.AddComponent<Image>();
            UiThemeConfig.Runtime.StyleCard(image, accent);

            var badgeObject = CreateObject("Card Badge");
            badgeObject.transform.SetParent(card.transform, false);
            var badgeImage = badgeObject.AddComponent<Image>();
            badgeImage.color = new Color(accent.r, accent.g, accent.b, 0.22f);
            badgeImage.raycastTarget = false;
            SetRect(badgeObject.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, 44f), new Vector2(12f, -12f));

            var code = CreateText(badgeObject.transform, "Badge Code", CodeFor(title), 11, TextAnchor.MiddleCenter);
            code.transform.SetParent(badgeObject.transform, false);
            SetRect(code.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(42f, 42f), Vector2.zero);
            code.fontStyle = FontStyle.Bold;

            var titleText = CreateText(card.transform, "Card Title", title, 14, TextAnchor.MiddleLeft);
            titleText.fontStyle = FontStyle.Bold;
            SetRect(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(width - 70f, 28f), new Vector2(64f, -14f));

            var detailsText = CreateText(card.transform, "Card Details", details, 12, TextAnchor.UpperLeft);
            detailsText.color = UiThemeConfig.Runtime.TextMuted;
            detailsText.horizontalOverflow = HorizontalWrapMode.Wrap;
            detailsText.verticalOverflow = VerticalWrapMode.Overflow;
            SetRect(detailsText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(width - 28f, height - 52f), new Vector2(14f, -48f));
            return card;
        }

        private Text CreateText(string name, string value, int fontSize, TextAnchor alignment)
        {
            return CreateText(Root, name, value, fontSize, alignment);
        }

        private Text CreateText(Transform parent, string name, string value, int fontSize, TextAnchor alignment)
        {
            var objectRoot = CreateObject(name);
            objectRoot.transform.SetParent(parent, false);
            var text = objectRoot.AddComponent<Text>();
            text.text = value;
            text.font = UiThemeConfig.Runtime.Font;
            text.fontSize = fontSize;
            text.color = UiThemeConfig.Runtime.TextPrimary;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.supportRichText = false;
            return text;
        }

        private GameObject CreateObject(string name)
        {
            var objectRoot = new GameObject(name, typeof(RectTransform));
            generated.Add(objectRoot);
            return objectRoot;
        }

        private void Clear()
        {
            for (var i = 0; i < generated.Count; i++)
            {
                if (generated[i] != null)
                {
                    Destroy(generated[i]);
                }
            }

            generated.Clear();
        }

        private static string CategoryCode(ItemDefinition definition)
        {
            if (definition == null)
            {
                return "ITEM";
            }

            switch (definition.Category)
            {
                case ItemCategory.Equipment: return "EQP";
                case ItemCategory.Consumable: return "POT";
                case ItemCategory.Material: return "MAT";
                case ItemCategory.Cosmetic: return "COS";
                case ItemCategory.Pet: return "PET";
                case ItemCategory.Mount: return "MNT";
                default: return "KEY";
            }
        }

        private static string CodeFor(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return "--";
            }

            var clean = title.Trim().ToUpperInvariant();
            return clean.Length <= 3 ? clean : clean.Substring(0, 3);
        }

        private static string SlotCode(EquipSlot slot)
        {
            switch (slot)
            {
                case EquipSlot.Weapon: return "ARM";
                case EquipSlot.Helmet: return "CAS";
                case EquipSlot.Chest: return "PECH";
                case EquipSlot.Gloves: return "MAN";
                case EquipSlot.Pants: return "PAN";
                case EquipSlot.Boots: return "BOT";
                case EquipSlot.Cape: return "CAP";
                case EquipSlot.Necklace: return "COL";
                case EquipSlot.RingLeft: return "AN1";
                case EquipSlot.RingRight: return "AN2";
                case EquipSlot.Bracelet: return "BRA";
                case EquipSlot.Belt: return "CIN";
                case EquipSlot.Talisman: return "TAL";
                default: return "--";
            }
        }

        private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 position)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }
    }
}
