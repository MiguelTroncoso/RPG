using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Cadena inicial de misiones del valle (contenido original). Fuente
    // unica para el generador de assets del editor (MMORPG > Quests >
    // Generate Quests) y el fallback runtime del bootstrap.
    public static class DefaultQuests
    {
        public const string MerchantNpcId = "valley_merchant";
        public const string BlacksmithNpcId = "valley_blacksmith";

        public const string MeetMerchant = "meet_merchant";
        public const string ClearFields = "clear_fields";
        public const string GatherFragments = "gather_fragments";
        public const string DestroyMonolith = "destroy_monolith";
        public const string VisitBlacksmith = "visit_blacksmith";
        public const string ForgeFirstSteel = "forge_first_steel";
        public const string EliteHunt = "elite_hunt";
        public const string ZoneBoss = "zone_boss";

        public static List<QuestDefinition> CreateAll()
        {
            return new List<QuestDefinition>
            {
                Quest(MeetMerchant, 1, "Ecos del valle",
                    "El valle esta inquieto desde que aparecio el monolito. Busca al Mercader del Valle y habla con el.",
                    "Bienvenido, viajero. Las criaturas cada vez llegan mas cerca del campamento... nos vendria bien tu ayuda.",
                    new[]
                    {
                        Objective(QuestObjectiveType.TalkToNpc, MerchantNpcId, 1, "Habla con el Mercader del Valle")
                    },
                    Reward(30, 20, Item(DefaultGameItems.MinorPotion, 1)),
                    ClearFields),

                Quest(ClearFields, 2, "Campos inquietos",
                    "Lobos corruptos y bandidos rondan el campamento. Reduce su numero para ganar tiempo.",
                    "Se nota la diferencia: los campos respiran de nuevo. Pero esa energia extrana sigue ahi...",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, string.Empty, 6, "Derrota criaturas del valle")
                    },
                    Reward(80, 40),
                    GatherFragments),

                Quest(GatherFragments, 3, "Energia residual",
                    "Las criaturas dejan caer fragmentos con energia antigua. Reune algunos para estudiarlos.",
                    "Estos fragmentos vibran igual que el monolito del sur. Ya no hay duda de donde viene todo esto.",
                    new[]
                    {
                        Objective(QuestObjectiveType.CollectItems, DefaultGameItems.AncientFragment, 2, "Consigue Fragmentos antiguos")
                    },
                    Reward(100, 50, Item(DefaultGameItems.ProtectionRune, 1)),
                    DestroyMonolith),

                Quest(DestroyMonolith, 4, "El Monolito Corrupto",
                    "El origen de la corrupcion es el Monolito Corrupto, al sur del campamento. Destruyelo.",
                    "El valle respira... pero algo desperto en el claro. Ve con el herrero.",
                    new[]
                    {
                        Objective(QuestObjectiveType.DefeatWorldEvent, string.Empty, 1, "Destruye el Monolito Corrupto")
                    },
                    Reward(150, 75, Item(DefaultGameItems.ValleyMedal, 1)),
                    VisitBlacksmith),

                Quest(VisitBlacksmith, 5, "El herrero del campamento",
                    "Dicen que el herrero del campamento puede reforzar tu equipo. Presentate con el.",
                    "Con esto ya puedes trabajar la forja. Traeme oro y materiales cuando quieras.",
                    new[]
                    {
                        Objective(QuestObjectiveType.TalkToNpc, BlacksmithNpcId, 1, "Habla con el Herrero")
                    },
                    Reward(60, 30, Item(DefaultGameItems.DullOre, 2), Item(DefaultGameItems.WornRing, 2)),
                    ForgeFirstSteel,
                    BlacksmithNpcId),

                Quest(ForgeFirstSteel, 6, "Acero templado",
                    "Mejora una pieza de tu equipo en la forja (WEAPON o ARMOR cerca del Herrero).",
                    "Buen trabajo. Un equipo bien templado marca la diferencia contra lo que viene.",
                    new[]
                    {
                        Objective(QuestObjectiveType.UpgradeItem, string.Empty, 1, "Mejora una pieza de equipo")
                    },
                    Reward(90, 40, Item(DefaultGameItems.ProtectionRune, 1)),
                    EliteHunt,
                    BlacksmithNpcId),

                Quest(EliteHunt, 7, "Los acechadores",
                    "Al este del campamento rondan acechadores mucho mas fuertes que el resto. Caza dos.",
                    "Asi que el claro ya tiene dueno... El coloso que los lidera sigue alli.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, EnemyTier.Elite.ToString(), 2, "Derrota Acechadores de elite")
                    },
                    Reward(150, 70, Item(DefaultGameItems.MinorPotion, 2)),
                    ZoneBoss),

                Quest(ZoneBoss, 8, "El coloso del claro",
                    "El Coloso de las Reliquias domina el claro al noroeste. Derrotalo y libera la zona.",
                    "El claro esta en paz. La ruta hacia la proxima region se abrira pronto.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, EnemyTier.Boss.ToString(), 1, "Derrota al Coloso de las Reliquias")
                    },
                    Reward(300, 150),
                    string.Empty)
            };
        }

        private static QuestDefinition Quest(string id, int order, string title, string startDialog, string completeDialog,
            QuestObjectiveData[] objectives, RewardBundle reward, string nextQuestId, string giverNpcId = MerchantNpcId)
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.name = id;
            quest.QuestId = id;
            quest.SortOrder = order;
            quest.Title = title;
            quest.StartDialog = startDialog;
            quest.CompleteDialog = completeDialog;
            quest.GiverNpcId = giverNpcId;
            quest.Objectives = objectives;
            quest.Reward = reward;
            quest.NextQuestId = nextQuestId;
            return quest;
        }

        private static QuestObjectiveData Objective(QuestObjectiveType type, string targetId, int count, string label)
        {
            return new QuestObjectiveData { Type = type, TargetId = targetId, RequiredCount = count, Label = label };
        }

        private static RewardBundle Reward(int experience, int gold, params ItemReward[] items)
        {
            return new RewardBundle { Experience = experience, Gold = gold, Items = items };
        }

        private static ItemReward Item(string itemId, int count)
        {
            return new ItemReward { ItemId = itemId, Count = count };
        }
    }
}
