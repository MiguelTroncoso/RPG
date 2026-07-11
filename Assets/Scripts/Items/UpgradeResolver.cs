namespace MmorpgPrototype
{
    public enum UpgradeOutcome
    {
        Success,
        FailKept,
        FailDowngraded,
        Destroyed
    }

    // Resolucion pura de un intento de mejora: sin estado, sin Unity, sin
    // efectos. El roll viene de afuera para poder testear y, mas adelante,
    // ejecutar la misma regla en el servidor.
    public static class UpgradeResolver
    {
        public static UpgradeOutcome Resolve(UpgradeConfig.UpgradeStep step, float roll, bool protectionUsed)
        {
            if (roll < step.SuccessChance)
            {
                return UpgradeOutcome.Success;
            }

            if (protectionUsed)
            {
                return UpgradeOutcome.FailKept;
            }

            switch (step.OnFailure)
            {
                case FailurePolicy.Downgrade:
                    return UpgradeOutcome.FailDowngraded;
                case FailurePolicy.Destroy:
                    return UpgradeOutcome.Destroyed;
                default:
                    return UpgradeOutcome.FailKept;
            }
        }
    }
}
