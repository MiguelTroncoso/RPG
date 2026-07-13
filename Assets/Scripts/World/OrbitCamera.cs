using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class OrbitCamera : MonoBehaviour
    {
        public Transform Target;
        public float Distance = 7.2f;
        public float Height = 1.35f;
        public float Pitch = 38f;
        public float Yaw = 42f;
        public float MouseSensitivity = 160f;
        public float TouchSensitivity = 0.18f;
        public float MinimumPitch = 24f;
        public float MaximumPitch = 62f;
        public float FollowSharpness = 12f;

        private Vector3 lastMousePosition;
        private bool mouseLookActive;

        public void RotateByTouchDelta(Vector2 screenDelta)
        {
            Yaw += screenDelta.x * TouchSensitivity;
            Pitch = Mathf.Clamp(Pitch - screenDelta.y * TouchSensitivity * 0.75f, MinimumPitch, MaximumPitch);
        }

        private void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            if (Input.GetMouseButtonDown(1))
            {
                mouseLookActive = true;
                lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(1))
            {
                mouseLookActive = false;
            }

            if (mouseLookActive)
            {
                var delta = Input.mousePosition - lastMousePosition;
                Yaw += delta.x * MouseSensitivity * 0.01f * Time.deltaTime;
                Pitch = Mathf.Clamp(Pitch - delta.y * MouseSensitivity * 0.006f * Time.deltaTime, MinimumPitch, MaximumPitch);
            }

            lastMousePosition = Input.mousePosition;

            var scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                Distance = Mathf.Clamp(Distance - scroll * 0.6f, 4.2f, 11f);
            }

            var focus = Target.position + Vector3.up * Height;
            var rotation = Quaternion.Euler(Pitch, Yaw, 0f);
            var desiredPosition = focus + rotation * new Vector3(0f, 0f, -Distance);

            transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-FollowSharpness * Time.deltaTime));
            transform.rotation = rotation;
        }
    }
}
