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
        public float FollowSharpness = 12f;

        private Vector3 lastMousePosition;

        private void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            if (Input.GetMouseButton(1))
            {
                var delta = Input.mousePosition - lastMousePosition;
                Yaw += delta.x * MouseSensitivity * 0.01f * Time.deltaTime;
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
