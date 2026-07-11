using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class NetworkRemotePlayer : MonoBehaviour
    {
        private TextMesh label;
        private Renderer bodyRenderer;
        private PlayerAvatarVisual avatarVisual;
        private Vector3 targetPosition;
        private Quaternion targetRotation;

        public string PlayerId { get; private set; }

        public static NetworkRemotePlayer Create(RemotePlayerState state)
        {
            var remote = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            remote.name = $"Remote Player - {state.name}";

            var collider = remote.GetComponent<CapsuleCollider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var component = remote.AddComponent<NetworkRemotePlayer>();
            component.avatarVisual = remote.AddComponent<PlayerAvatarVisual>();
            component.PlayerId = state.id;
            component.bodyRenderer = remote.GetComponent<Renderer>();
            component.CreateLabel();
            component.ApplyState(state, true);
            return component;
        }

        public void ApplyState(RemotePlayerState state, bool immediate = false)
        {
            PlayerId = state.id;
            gameObject.name = $"Remote Player - {state.name}";
            targetPosition = new Vector3(state.x, state.y, state.z);
            targetRotation = Quaternion.Euler(0f, state.yaw, 0f);

            var gender = ParseGender(state.gender);

            if (label != null)
            {
                var genderLabel = gender == CharacterGender.Femenino ? Localization.Tr("identity.gender_female") : Localization.Tr("identity.gender_male");
                var levelLabel = state.level > 0 ? $" Nv{state.level}" : string.Empty;
                label.text = $"{state.name}\n{state.className}{levelLabel} - {genderLabel}";
            }

            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = ClassColor(state.className);
            }

            avatarVisual ??= GetComponent<PlayerAvatarVisual>();
            if (avatarVisual != null)
            {
                avatarVisual.Apply(ClassDefinition.Create(ParseClass(state.className)), gender);
            }

            if (immediate)
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation;
            }
        }

        private void Update()
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, 1f - Mathf.Exp(-14f * Time.deltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-14f * Time.deltaTime));
        }

        private void LateUpdate()
        {
            if (label == null)
            {
                return;
            }

            var camera = Camera.main;
            if (camera != null)
            {
                label.transform.rotation = Quaternion.LookRotation(label.transform.position - camera.transform.position);
            }
        }

        private void CreateLabel()
        {
            var labelObject = new GameObject("Name Label");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 1.45f, 0f);

            label = labelObject.AddComponent<TextMesh>();
            label.text = "Player";
            label.fontSize = 38;
            label.characterSize = 0.04f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = Color.white;
        }

        private static Color ClassColor(string className)
        {
            switch (className)
            {
                case "Ninja":
                    return new Color(0.18f, 0.18f, 0.24f);
                case "Chaman":
                    return new Color(0.24f, 0.62f, 0.88f);
                case "Umbra":
                    return new Color(0.34f, 0.14f, 0.52f);
                default:
                    return new Color(0.18f, 0.42f, 0.9f);
            }
        }

        private static CharacterGender ParseGender(string genderName)
        {
            return System.Enum.TryParse(genderName, out CharacterGender gender)
                ? gender
                : CharacterGender.Masculino;
        }

        private static CharacterClassType ParseClass(string className)
        {
            switch (className)
            {
                case "Ninja":
                    return CharacterClassType.Ninja;
                case "Chaman":
                    return CharacterClassType.Chaman;
                case "Umbra":
                    return CharacterClassType.Umbra;
                default:
                    return CharacterClassType.Guerrero;
            }
        }
    }
}
