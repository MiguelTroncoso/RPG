using System;
using UnityEngine;

namespace MmorpgPrototype
{
    public enum FailurePolicy
    {
        KeepLevel,
        Downgrade,
        Destroy
    }

    // Tabla de mejora +1..+MaxUpgradeLevel. Todas las probabilidades, costos
    // y politicas de fallo son datos; la logica solo las lee.
    [CreateAssetMenu(menuName = "MMORPG/Items/Upgrade Config", fileName = "UpgradeConfig")]
    public sealed class UpgradeConfig : ScriptableObject
    {
        [Serializable]
        public struct UpgradeStep
        {
            public int TargetLevel;
            [Range(0f, 1f)] public float SuccessChance;
            public FailurePolicy OnFailure;
            public int GoldCost;
            public int MaterialCost;
        }

        [Min(1)] public int MaxUpgradeLevel = 15;
        [Min(0)] public int WeaponDamagePerLevel = 4;
        [Min(0)] public int ArmorHealthPerLevel = 15;
        public UpgradeStep[] Steps;

        public bool HasSteps => Steps != null && Steps.Length > 0;

        public UpgradeStep GetStep(int targetLevel)
        {
            if (HasSteps)
            {
                foreach (var step in Steps)
                {
                    if (step.TargetLevel == targetLevel)
                    {
                        return step;
                    }
                }
            }

            return DefaultStep(targetLevel);
        }

        public void FillWithDefaults()
        {
            Steps = new UpgradeStep[MaxUpgradeLevel];
            for (var i = 0; i < MaxUpgradeLevel; i++)
            {
                Steps[i] = DefaultStep(i + 1);
            }
        }

        // +1..+6 no hay riesgo real (solo se mantiene), +7..+9 puede bajar,
        // +10..+15 puede destruir el objeto (la runa de proteccion lo evita).
        public static UpgradeStep DefaultStep(int targetLevel)
        {
            var safeTarget = Mathf.Max(1, targetLevel);
            var chance = Mathf.Clamp01(1.05f - 0.06f * safeTarget);
            var policy = safeTarget <= 6
                ? FailurePolicy.KeepLevel
                : safeTarget <= 9
                    ? FailurePolicy.Downgrade
                    : FailurePolicy.Destroy;

            return new UpgradeStep
            {
                TargetLevel = safeTarget,
                SuccessChance = chance,
                OnFailure = policy,
                GoldCost = 40 + (safeTarget - 1) * 25,
                MaterialCost = 1 + safeTarget / 6
            };
        }
    }
}
