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

        // Modelo 3D real (KayKit Adventurers, CC0). Si el asset no esta en
        // Resources, el avatar cae al visual procedural.
        public string CharacterModelResource;
        public Color BodyColor;
        public Color SkillColor;
        public int MaxHealth;
        public int BaseDamage;
        public float MoveSpeed;
        public float AttackRange;
        public float AttackCooldown;
        public float CritChance;
        public float CritMultiplier;
        public float Accuracy;
        public float Evasion;
        public int Defense;

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
                        CharacterModelResource = "ThirdParty/KayKit/Adventurers/Characters/Rogue",
                        BodyColor = new Color(0.18f, 0.18f, 0.24f),
                        SkillColor = new Color(0.75f, 0.84f, 1f),
                        MaxHealth = 125,
                        BaseDamage = 23,
                        MoveSpeed = 6.35f,
                        AttackRange = 2.05f,
                        AttackCooldown = 0.48f,
                        CritChance = 0.18f,
                        CritMultiplier = 1.7f,
                        Accuracy = 0.97f,
                        Evasion = 0.1f,
                        Defense = 1
                    };

                case CharacterClassType.Chaman:
                    return new ClassDefinition
                    {
                        Type = type,
                        DisplayName = "Chaman",
                        SkillOneName = "Rayo",
                        SkillTwoName = "Bendicion",
                        WeaponResource = "ThirdParty/KayKit/Adventurers/Weapons/staff",
                        CharacterModelResource = "ThirdParty/KayKit/Adventurers/Characters/Mage",
                        BodyColor = new Color(0.24f, 0.62f, 0.88f),
                        SkillColor = new Color(0.22f, 0.95f, 0.84f),
                        MaxHealth = 135,
                        BaseDamage = 20,
                        MoveSpeed = 5.05f,
                        AttackRange = 3.25f,
                        AttackCooldown = 0.72f,
                        CritChance = 0.1f,
                        CritMultiplier = 1.6f,
                        Accuracy = 0.96f,
                        Evasion = 0.05f,
                        Defense = 2
                    };

                case CharacterClassType.Umbra:
                    return new ClassDefinition
                    {
                        Type = type,
                        DisplayName = "Umbra",
                        SkillOneName = "Hoja Oscura",
                        SkillTwoName = "Marca",
                        WeaponResource = "ThirdParty/KayKit/Adventurers/Weapons/sword_2handed",
                        CharacterModelResource = "ThirdParty/KayKit/Adventurers/Characters/Barbarian",
                        BodyColor = new Color(0.34f, 0.14f, 0.52f),
                        SkillColor = new Color(0.75f, 0.25f, 1f),
                        MaxHealth = 145,
                        BaseDamage = 28,
                        MoveSpeed = 5.35f,
                        AttackRange = 2.35f,
                        AttackCooldown = 0.76f,
                        CritChance = 0.14f,
                        CritMultiplier = 1.65f,
                        Accuracy = 0.95f,
                        Evasion = 0.06f,
                        Defense = 2
                    };

                default:
                    return new ClassDefinition
                    {
                        Type = CharacterClassType.Guerrero,
                        DisplayName = "Guerrero",
                        SkillOneName = "Corte",
                        SkillTwoName = "Grito",
                        WeaponResource = "ThirdParty/KayKit/Adventurers/Weapons/sword_1handed",
                        CharacterModelResource = "ThirdParty/KayKit/Adventurers/Characters/Knight",
                        BodyColor = new Color(0.18f, 0.42f, 0.9f),
                        SkillColor = new Color(1f, 0.72f, 0.18f),
                        MaxHealth = 165,
                        BaseDamage = 27,
                        MoveSpeed = 5.2f,
                        AttackRange = 2.25f,
                        AttackCooldown = 0.65f,
                        CritChance = 0.08f,
                        CritMultiplier = 1.5f,
                        Accuracy = 0.95f,
                        Evasion = 0.04f,
                        Defense = 3
                    };
            }
        }
    }
}

