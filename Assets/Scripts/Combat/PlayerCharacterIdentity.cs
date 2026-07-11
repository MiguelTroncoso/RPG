using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class PlayerCharacterIdentity : MonoBehaviour
    {
        public string CharacterName { get; private set; } = "Heroe";
        public CharacterGender Gender { get; private set; } = CharacterGender.Masculino;
        public string GenderLabel => Gender == CharacterGender.Femenino ? Localization.Tr("identity.gender_female") : Localization.Tr("identity.gender_male");
        public string DisplayLabel => Localization.Tr("identity.display", CharacterName, GenderLabel);

        private Renderer bodyRenderer;
        private PlayerAvatarVisual avatarVisual;
        private TextMesh nameLabel;
        private ClassDefinition currentClass;

        private void Awake()
        {
            bodyRenderer = GetComponentInChildren<Renderer>();
            avatarVisual = GetComponent<PlayerAvatarVisual>();
            CreateNameLabel();
        }

        private void LateUpdate()
        {
            if (nameLabel == null)
            {
                return;
            }

            var camera = Camera.main;
            if (camera != null)
            {
                nameLabel.transform.rotation = Quaternion.LookRotation(nameLabel.transform.position - camera.transform.position);
            }
        }

        public void ApplySelection(string characterName, CharacterGender gender)
        {
            CharacterName = SanitizeName(characterName);
            Gender = gender;
            gameObject.name = $"Player - {CharacterName}";

            var network = GetComponent<MmorpgNetworkClient>();
            if (network != null)
            {
                network.PlayerName = CharacterName;
            }

            ApplyClassVisual(currentClass);
        }

        public void ApplyClassVisual(ClassDefinition definition)
        {
            currentClass = definition;
            avatarVisual ??= GetComponent<PlayerAvatarVisual>();

            if (avatarVisual != null)
            {
                avatarVisual.Apply(definition, Gender);
                RefreshNameLabel();
                return;
            }

            if (bodyRenderer != null && definition != null)
            {
                transform.localScale = Gender == CharacterGender.Femenino
                    ? new Vector3(0.92f, 1f, 0.92f)
                    : new Vector3(1.04f, 1.03f, 1.04f);

                var tint = Gender == CharacterGender.Femenino
                    ? new Color(1f, 0.82f, 0.92f)
                    : new Color(0.82f, 0.9f, 1f);
                bodyRenderer.material.color = Color.Lerp(definition.BodyColor, tint, 0.18f);
            }

            RefreshNameLabel();
        }

        private void CreateNameLabel()
        {
            var labelObject = new GameObject("Player Name Label");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 1.55f, 0f);

            nameLabel = labelObject.AddComponent<TextMesh>();
            nameLabel.fontSize = 42;
            nameLabel.characterSize = 0.04f;
            nameLabel.anchor = TextAnchor.MiddleCenter;
            nameLabel.alignment = TextAlignment.Center;
            nameLabel.color = new Color(0.94f, 0.98f, 1f);
            RefreshNameLabel();
        }

        private void RefreshNameLabel()
        {
            if (nameLabel == null)
            {
                return;
            }

            var className = currentClass != null ? currentClass.DisplayName : Localization.Tr("class.guerrero");
            nameLabel.text = Localization.Tr("identity.name_label", CharacterName, className, GenderLabel);
        }

        private static string SanitizeName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                return Localization.Tr("identity.default_name");
            }

            var trimmed = rawName.Trim();
            return trimmed.Length > 14 ? trimmed.Substring(0, 14) : trimmed;
        }
    }
}
