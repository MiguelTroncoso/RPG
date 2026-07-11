using UnityEngine;

namespace MmorpgPrototype
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(PlayerCombat))]
    [RequireComponent(typeof(Health))]
    public sealed class PlayerClassController : MonoBehaviour
    {
        public CharacterClassType CurrentClass { get; private set; } = CharacterClassType.Guerrero;
        public ClassDefinition Definition { get; private set; }

        private PlayerController movement;
        private PlayerCombat combat;
        private Health health;
        private PlayerCharacterIdentity identity;
        private Renderer bodyRenderer;
        private GameObject weaponVisual;

        private void Awake()
        {
            movement = GetComponent<PlayerController>();
            combat = GetComponent<PlayerCombat>();
            health = GetComponent<Health>();
            identity = GetComponent<PlayerCharacterIdentity>();
            bodyRenderer = GetComponentInChildren<Renderer>();
        }

        private void Start()
        {
            ApplyClass(CurrentClass);
        }

        public void ApplyClass(CharacterClassType classType)
        {
            CurrentClass = classType;
            Definition = ClassDefinition.Create(classType);

            health.ResetHealth(Definition.MaxHealth);
            movement.MoveSpeed = Definition.MoveSpeed;
            combat.AttackDamage = Definition.BaseDamage;
            combat.AttackRange = Definition.AttackRange;
            combat.AttackCooldown = Definition.AttackCooldown;
            combat.SkillTint = Definition.SkillColor;
            GetComponent<EquipmentUpgradeSystem>()?.ApplyBonuses();

            if (identity != null)
            {
                identity.ApplyClassVisual(Definition);
            }
            else if (bodyRenderer != null)
            {
                bodyRenderer.material.color = Definition.BodyColor;
            }

            EquipWeapon(Definition.WeaponResource);
            combat.Hud?.SetStatus($"Clase activa: {Definition.DisplayName}");
            combat.Hud?.RefreshClass();
        }

        private void EquipWeapon(string resourcePath)
        {
            if (weaponVisual != null)
            {
                Destroy(weaponVisual);
            }

            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                return;
            }

            weaponVisual = Instantiate(prefab, transform);
            weaponVisual.name = $"Weapon - {Definition.DisplayName}";
            weaponVisual.transform.localPosition = new Vector3(0.45f, 0.75f, 0.22f);
            weaponVisual.transform.localRotation = Quaternion.Euler(88f, 15f, -38f);
            weaponVisual.transform.localScale = Vector3.one * 0.85f;
        }
    }
}
