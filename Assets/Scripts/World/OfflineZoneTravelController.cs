using UnityEngine;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    // The offline atlas is both a player-facing world map and a practical way
    // to validate every zone, boss and authored mob on a physical device.
    public sealed class OfflineZoneTravelController : MonoBehaviour
    {
        private const string ReviewModeKey = "mmorpg.offline.atlas.review";

        public GameObject Panel;
        public Transform Player;
        public PlayerProgression Progression;
        public PlayerCombat Combat;
        public PrototypeHud Hud;
        public ZoneDefinition[] Zones;
        public Text TitleText;
        public Text DetailsText;
        public Text ModeText;
        public Text TravelLabel;
        public Button TravelButton;
        public Button[] ZoneButtons;
        public Image[] ZoneImages;
        public Text[] ZoneLabels;

        public bool ReviewMode { get; private set; }
        public int SelectedZoneIndex { get; private set; }

        private void Awake()
        {
            ReviewMode = PlayerPrefs.GetInt(ReviewModeKey, 1) == 1;
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void Toggle()
        {
            if (Panel == null)
            {
                return;
            }

            Panel.SetActive(!Panel.activeSelf);
        }

        public void SelectZone(int index)
        {
            if (Zones == null || index < 0 || index >= Zones.Length || Zones[index] == null)
            {
                return;
            }

            SelectedZoneIndex = index;
            Refresh();
        }

        public void ToggleReviewMode()
        {
            ReviewMode = !ReviewMode;
            PlayerPrefs.SetInt(ReviewModeKey, ReviewMode ? 1 : 0);
            PlayerPrefs.Save();
            Refresh();
        }

        public void TravelToSelected()
        {
            if (Player == null || Zones == null || SelectedZoneIndex < 0 || SelectedZoneIndex >= Zones.Length)
            {
                return;
            }

            var zone = Zones[SelectedZoneIndex];
            if (zone == null)
            {
                return;
            }

            var level = Progression != null ? Progression.Level : 1;
            if (!ReviewMode && level < zone.MinLevel)
            {
                Hud?.SetStatus($"{zone.DisplayName} requiere nivel {zone.MinLevel}.", 3f);
                return;
            }

            var destination = zone.HasSafeZone
                ? zone.SafeZoneCenter + Vector3.up
                : zone.GroundCenter + new Vector3(0f, 1f, -18f);
            var characterController = Player.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            Player.position = destination;
            Player.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

            if (characterController != null)
            {
                characterController.enabled = true;
            }

            if (Combat != null)
            {
                Combat.SafeZoneCenter = zone.SafeZoneCenter;
                Combat.SafeZoneRadius = zone.HasSafeZone ? zone.SafeZoneRadius : 0f;
            }

            Hud?.WatchEnemy(null);
            Hud?.SetStatus($"Ruta abierta: {zone.DisplayName} ({zone.MinLevel}-{zone.MaxLevel}).", 4f);
            Hud?.AddFeed($"Explorando {zone.DisplayName}: {zone.NormalName}, elite y {zone.BossName}.");
            Panel?.SetActive(false);
        }

        public void Refresh()
        {
            if (Zones == null || Zones.Length == 0)
            {
                return;
            }

            SelectedZoneIndex = Mathf.Clamp(SelectedZoneIndex, 0, Zones.Length - 1);
            var level = Progression != null ? Progression.Level : 1;

            for (var i = 0; i < Zones.Length; i++)
            {
                var zone = Zones[i];
                if (zone == null)
                {
                    continue;
                }

                var selected = i == SelectedZoneIndex;
                var unlocked = level >= zone.MinLevel;
                var color = selected
                    ? Color.Lerp(zone.GroundColor, Color.white, 0.25f)
                    : Color.Lerp(zone.GroundColor, new Color(0.04f, 0.055f, 0.07f), 0.52f);

                if (!unlocked && !ReviewMode)
                {
                    color = Color.Lerp(color, Color.black, 0.45f);
                }

                if (ZoneImages != null && i < ZoneImages.Length && ZoneImages[i] != null)
                {
                    ZoneImages[i].color = color;
                }

                if (ZoneLabels != null && i < ZoneLabels.Length && ZoneLabels[i] != null)
                {
                    var state = unlocked ? "DISPONIBLE" : ReviewMode ? "PRUEBA" : $"NV {zone.MinLevel}";
                    ZoneLabels[i].text = $"{i + 1:00}  {zone.DisplayName}\nNv {zone.MinLevel}-{zone.MaxLevel}  {state}";
                }
            }

            var selectedZone = Zones[SelectedZoneIndex];
            if (selectedZone == null)
            {
                return;
            }

            if (TitleText != null)
            {
                TitleText.text = $"Atlas del Valle  |  {selectedZone.DisplayName}";
            }

            if (DetailsText != null)
            {
                var access = level >= selectedZone.MinLevel ? "Ruta de campaña disponible" : ReviewMode ? "Acceso de prueba disponible" : $"Requiere nivel {selectedZone.MinLevel}";
                DetailsText.text =
                    $"Nivel {selectedZone.MinLevel}-{selectedZone.MaxLevel}  •  {access}\n" +
                    $"Mobs: {selectedZone.NormalName}  |  Elite: {selectedZone.EliteName}\n" +
                    $"Jefe: {selectedZone.BossName}  •  Reliquia: {selectedZone.BossGuaranteedDrop}";
            }

            if (ModeText != null)
            {
                ModeText.text = ReviewMode
                    ? "MODO PRUEBA ACTIVO: puedes recorrer las 10 zonas sin requisito de nivel."
                    : "MODO CAMPAÑA: las zonas se desbloquean al alcanzar el nivel indicado.";
            }

            if (TravelLabel != null)
            {
                TravelLabel.text = ReviewMode || level >= selectedZone.MinLevel ? "VIAJAR" : $"NIVEL {selectedZone.MinLevel}";
            }

            if (TravelButton != null)
            {
                TravelButton.interactable = ReviewMode || level >= selectedZone.MinLevel;
            }
        }
    }
}
