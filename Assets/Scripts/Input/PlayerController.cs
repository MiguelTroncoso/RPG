using UnityEngine;

namespace MmorpgPrototype
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        public VirtualJoystick MovementJoystick;
        public float MoveSpeed = 5.2f;
        public float TurnSpeed = 14f;
        public float Gravity = -18f;
        public float FallRecoveryY = -2f;
        public Vector3 SafeSpawnPosition = new Vector3(0f, 1f, 0f);

        public bool IsReceivingMovementInput { get; private set; }

        private CharacterController controller;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (transform.position.y < FallRecoveryY)
            {
                RecoverFromFall();
            }

            var input = ReadMovementInput();
            var moveDirection = CameraRelativeDirection(input);
            IsReceivingMovementInput = moveDirection.sqrMagnitude > 0.001f;

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -1f;
            }

            verticalVelocity += Gravity * Time.deltaTime;

            var velocity = moveDirection * MoveSpeed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);

            if (IsReceivingMovementInput)
            {
                var targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, TurnSpeed * Time.deltaTime);
            }
        }

        private void RecoverFromFall()
        {
            controller.enabled = false;
            transform.position = SafeSpawnPosition;
            verticalVelocity = -1f;
            controller.enabled = true;
        }

        private Vector2 ReadMovementInput()
        {
            var input = Vector2.zero;

            if (MovementJoystick != null)
            {
                input += MovementJoystick.Value;
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                input.y += 1f;
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                input.y -= 1f;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                input.x += 1f;
            }

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                input.x -= 1f;
            }

            return Vector2.ClampMagnitude(input, 1f);
        }

        private static Vector3 CameraRelativeDirection(Vector2 input)
        {
            if (input.sqrMagnitude <= 0.001f)
            {
                return Vector3.zero;
            }

            var camera = Camera.main;
            var forward = camera != null ? camera.transform.forward : Vector3.forward;
            var right = camera != null ? camera.transform.right : Vector3.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (forward * input.y + right * input.x).normalized;
        }
    }
}
