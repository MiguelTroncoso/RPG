using UnityEngine;

namespace MmorpgPrototype
{
    public sealed class ClassDefinition
    {
        public CharacterClassType Type;
        public string DisplayName;
        public string SkillOneName;
        public string SkillTwoName;
        public string WeaponResource;
        public Color BodyColor;
        public Color SkillColor;
        public int MaxHealth;
        public int BaseDamage;
        public float MoveSpeed;
        public float AttackRange;
        public float AttackCooldown;

        public static ClassDefinition Create(CharacterClassType type)
        {
            switch (type)
            {
                case CharacterClassType.Ninja:
                    return new ClassDefinition
                    {
                        Type = type,
                        DisplayName = "Ninja",
                        SkillOneName = "Estocada",
                        SkillTwoName = "Sombra",
                        WeaponResource = "ThirdParty/KayKit/Adventurers/Weapons/dagger",
                        BodyColor = new Color(0.18f, 0.18f, 0.24f),
                        SkillColor = new Color(0.75f, 0.84f, 1f),
                        MaxHealth = 125,
                        BaseDamage = 23,
                        MoveSpeed = 6.35f,
                        AttackRange = 2.05f,
                        AttackCooldown = 0.48f
                    };

                case CharacterClassType.Chaman:
                    return new ClassDefinition
                    {
                        Type = type,
                        DisplayName = "Chaman",
                        SkillOneName = "Rayo",
                        SkillTwoName = "Bendicion",
                        WeaponResource = "ThirdParty/KayKit/Adventurers/Weapons/staff",
                        BodyColor = new Color(0.24f, 0.62f, 0.88f),
                        SkillColor = new Color(0.22f, 0.95f, 0.84f),
                        MaxHealth = 135,
                        BaseDamage = 20,
                        MoveSpeed = 5.05f,
                        AttackRange = 3.25f,
                        AttackCooldown = 0.72f
                    };

                case CharacterClassType.Umbra:
                    return new ClassDefinition
                    {
                        Type = type,
                        DisplayName = "Umbra",
                        SkillOneName = "Hoja Oscura",
                        SkillTwoName = "Marca",
                        WeaponResource = "ThirdParty/KayKit/Adventurers/Weapons/sword_2handed",
                        BodyColor = new Color(0.34f, 0.14f, 0.52f),
                        SkillColor = new Color(0.75f, 0.25f, 1f),
                        MaxHealth = 145,
                        BaseDamage = 28,
                        MoveSpeed = 5.35f,
                        AttackRange = 2.35f,
                        AttackCooldown = 0.76f
                    };

                default:
                    return new ClassDefinition
                    {
                        Type = CharacterClassType.Guerrero,
                        DisplayName = "Guerrero",
                        SkillOneName = "Corte",
                        SkillTwoName = "Grito",
                        WeaponResource = "ThirdParty/KayKit/Adventurers/Weapons/sword_1handed",
                        BodyColor = new Color(0.18f, 0.42f, 0.9f),
                        SkillColor = new Color(1f, 0.72f, 0.18f),
                        MaxHealth = 165,
                        BaseDamage = 27,
                        MoveSpeed = 5.2f,
                        AttackRange = 2.25f,
                        AttackCooldown = 0.65f
                    };
            }
        }
    }
}

