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
        public const string ForestEliteHunt = "forest_elite_hunt";
        public const string ForestBossHunt = "forest_boss_hunt";
        public const string AshWanderers = "ash_wanderers";
        public const string AshEliteHunt = "ash_elite_hunt";
        public const string AshBossHunt = "ash_boss_hunt";
        public const string CrystalSentinels = "crystal_sentinels";
        public const string CrystalEliteHunt = "crystal_elite_hunt";
        public const string CrystalBossHunt = "crystal_boss_hunt";

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
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.ValleyEliteId, 2, "Derrota Acechadores de elite")
                    },
                    Reward(150, 70, Item(DefaultGameItems.MinorPotion, 2)),
                    ZoneBoss),

                Quest(ZoneBoss, 8, "El coloso del claro",
                    "El Coloso de las Reliquias domina el claro al noroeste. Derrotalo y libera la zona.",
                    "El claro esta en paz. Al norte, el Bosque de los Susurros despierta...",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.ValleyBossId, 1, "Derrota al Coloso de las Reliquias")
                    },
                    Reward(300, 150),
                    ForestEliteHunt),

                Quest(ForestEliteHunt, 9, "Ecos del bosque",
                    "En el Bosque de los Susurros, al norte, rondan Sombras que susurran nombres olvidados. Caza tres.",
                    "Las Sombras callan... pero el Anciano de Espinas sigue despierto en lo profundo.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.ForestEliteId, 3, "Derrota Sombras del bosque")
                    },
                    Reward(400, 180, Item(DefaultGameItems.MinorPotion, 2)),
                    ForestBossHunt),

                Quest(ForestBossHunt, 10, "El corazon del bosque",
                    "El Anciano de Espinas corrompe el bosque desde su corazon, al noroeste. Derrotalo.",
                    "El bosque respira... pero el viento trae ceniza desde las colinas del norte.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.ForestBossId, 1, "Derrota al Anciano de Espinas")
                    },
                    Reward(800, 400, Item(DefaultGameItems.ProtectionRune, 1)),
                    AshWanderers),

                Quest(AshWanderers, 11, "Cenizas al viento",
                    "Mas alla del bosque, las Colinas Cenicientas arden sin fuego. Reduce a los Cenicientos errantes.",
                    "La ceniza no cae del cielo: alguien la respira y la devuelve. Sigue subiendo.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.AshCreatureId, 8, "Derrota Cenicientos errantes")
                    },
                    Reward(1200, 500, Item(DefaultGameItems.MinorPotion, 3)),
                    AshEliteHunt),

                Quest(AshEliteHunt, 12, "Los devoradores",
                    "Los Devoradores de ceniza custodian el paso alto, al oeste de las colinas. Caza dos.",
                    "Solo queda su origen: un corazon que late ceniza en lo alto de la colina.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.AshEliteId, 2, "Derrota Devoradores de ceniza")
                    },
                    Reward(1600, 700, Item(DefaultGameItems.ProtectionRune, 1)),
                    AshBossHunt),

                Quest(AshBossHunt, 13, "El corazon de ceniza",
                    "El Corazon de Ceniza arde en el este de las colinas. Apagalo para siempre.",
                    "Las colinas se enfrian. En lo alto brillan cumbres que no reflejan el sol.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.AshBossId, 1, "Derrota al Corazon de Ceniza")
                    },
                    Reward(3000, 1500, Item(DefaultGameItems.ValleyMedal, 1)),
                    CrystalSentinels),

                Quest(CrystalSentinels, 14, "Cumbres de cristal",
                    "Mas alla de las cenizas se alzan cristales que caminan como guardianes. Rompe su avanzada.",
                    "Los centinelas caen en fragmentos, pero cada pedazo susurra una misma voz.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.CrystalCreatureId, 9, "Derrota Centinelas de cristal")
                    },
                    Reward(4200, 1800, Item(DefaultGameItems.MinorPotion, 3)),
                    CrystalEliteHunt),

                Quest(CrystalEliteHunt, 15, "Custodios prismales",
                    "Los Custodios prismales protegen el sendero alto de las cumbres. Derrota a dos.",
                    "El sendero esta abierto. El Oraculo Fragmentado espera entre ecos de luz.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.CrystalEliteId, 2, "Derrota Custodios prismales")
                    },
                    Reward(5200, 2300, Item(DefaultGameItems.ProtectionRune, 1)),
                    CrystalBossHunt),

                Quest(CrystalBossHunt, 16, "El Oraculo Fragmentado",
                    "El Oraculo Fragmentado intenta recomponer la corrupcion en las cumbres. Detenlo.",
                    "Las cumbres dejan de cantar. El camino hacia regiones mas peligrosas queda abierto.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.CrystalBossId, 1, "Derrota al Oraculo Fragmentado")
                    },
                    Reward(7000, 3200, Item(DefaultGameItems.ValleyMedal, 1)),
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
