namespace MmorpgPrototype
{
    public interface IDamageable
    {
        bool IsDead { get; }
        void TakeDamage(int amount);
    }

    public interface IAttackable
    {
        bool CanBeAttacked { get; }
    }

    public interface ILootSource
    {
        string RollLoot();
    }

    public interface IInteractable
    {
        void Interact();
    }
}
