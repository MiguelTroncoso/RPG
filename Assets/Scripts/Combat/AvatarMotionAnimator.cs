using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class AvatarMotionAnimator : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        private Transform visualRoot;
        private Transform leftArm;
        private Transform rightArm;
        private Animator modelAnimator;
        private Vector3 baseLocalPosition;
        private Vector3 lastWorldPosition;
        private float attackUntil;
        private float moveAmount;
        private bool hasSpeedParameter;
        private bool hasAttackParameter;

        private void Awake()
        {
            lastWorldPosition = transform.position;
        }

        public void SetVisualRoot(Transform root, Animator animator)
        {
            visualRoot = root;
            modelAnimator = animator;
            baseLocalPosition = visualRoot != null ? visualRoot.localPosition : Vector3.zero;
            leftArm = visualRoot != null ? FindDeepChild(visualRoot, "Left Arm") : null;
            rightArm = visualRoot != null ? FindDeepChild(visualRoot, "Right Arm") : null;
            CacheAnimatorParameters();
        }

        public void PlayAttack()
        {
            attackUntil = Time.time + 0.28f;

            if (modelAnimator != null && modelAnimator.runtimeAnimatorController != null && hasAttackParameter)
            {
                modelAnimator.SetTrigger(AttackHash);
            }
        }

        private void Update()
        {
            if (visualRoot == null)
            {
                lastWorldPosition = transform.position;
                return;
            }

            var frameSpeed = Time.deltaTime > 0f
                ? (transform.position - lastWorldPosition).magnitude / Time.deltaTime
                : 0f;
            lastWorldPosition = transform.position;

            moveAmount = Mathf.Lerp(moveAmount, Mathf.Clamp01(frameSpeed / 4.5f), 1f - Mathf.Exp(-12f * Time.deltaTime));
            var phase = Time.time * Mathf.Lerp(2.4f, 8.5f, moveAmount);
            var idleBob = Mathf.Sin(Time.time * 2.1f) * 0.018f;
            var runBob = Mathf.Abs(Mathf.Sin(phase)) * 0.055f * moveAmount;
            var attackAmount = Mathf.Clamp01((attackUntil - Time.time) / 0.28f);
            var attackCurve = Mathf.Sin(attackAmount * Mathf.PI);

            visualRoot.localPosition = baseLocalPosition + Vector3.up * (idleBob + runBob) + Vector3.forward * (attackCurve * 0.08f);
            visualRoot.localRotation = Quaternion.Euler(attackCurve * 3f, 0f, Mathf.Sin(phase) * moveAmount * 2.5f);

            AnimateProceduralArms(phase, moveAmount, attackCurve);
            AnimateModel(moveAmount);
        }

        private void AnimateProceduralArms(float phase, float movement, float attack)
        {
            if (leftArm == null || rightArm == null)
            {
                return;
            }

            var swing = Mathf.Sin(phase) * movement * 28f;
            var attackSwing = attack * 62f;
            leftArm.localRotation = Quaternion.Euler(attackSwing, 0f, 18f + swing);
            rightArm.localRotation = Quaternion.Euler(-attackSwing, 0f, -18f - swing);
        }

        private void AnimateModel(float movement)
        {
            if (modelAnimator == null || modelAnimator.runtimeAnimatorController == null || !hasSpeedParameter)
            {
                return;
            }

            modelAnimator.SetFloat(SpeedHash, movement);
        }

        private void CacheAnimatorParameters()
        {
            hasSpeedParameter = false;
            hasAttackParameter = false;

            if (modelAnimator == null || modelAnimator.runtimeAnimatorController == null)
            {
                return;
            }

            foreach (var parameter in modelAnimator.parameters)
            {
                if (parameter.nameHash == SpeedHash && parameter.type == AnimatorControllerParameterType.Float)
                {
                    hasSpeedParameter = true;
                }

                if (parameter.nameHash == AttackHash && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    hasAttackParameter = true;
                }
            }
        }

        private static Transform FindDeepChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }

                var nested = FindDeepChild(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }
    }
}
