using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Cadena principal de misiones 1-105. Fuente
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
        public const string ForestCreatures = "forest_creatures";
        public const string ForestEliteHunt = "forest_elite_hunt";
        public const string ForestBossHunt = "forest_boss_hunt";
        public const string AshWanderers = "ash_wanderers";
        public const string AshEliteHunt = "ash_elite_hunt";
        public const string AshBossHunt = "ash_boss_hunt";
        public const string CrystalSentinels = "crystal_sentinels";
        public const string CrystalEliteHunt = "crystal_elite_hunt";
        public const string CrystalBossHunt = "crystal_boss_hunt";
        public const string FrostFront = "frost_front";
        public const string FrostEliteHunt = "frost_elite_hunt";
        public const string FrostBossHunt = "frost_boss_hunt";
        public const string SunkenDead = "sunken_dead";
        public const string SunkenEliteHunt = "sunken_elite_hunt";
        public const string SunkenBossHunt = "sunken_boss_hunt";
        public const string ObsidianForged = "obsidian_forged";
        public const string ObsidianEliteHunt = "obsidian_elite_hunt";
        public const string ObsidianBossHunt = "obsidian_boss_hunt";
        public const string AstralSeeds = "astral_seeds";
        public const string AstralEliteHunt = "astral_elite_hunt";
        public const string AstralBossHunt = "astral_boss_hunt";
        public const string EclipseDevotees = "eclipse_devotees";
        public const string EclipseEliteHunt = "eclipse_elite_hunt";
        public const string EclipseBossHunt = "eclipse_boss_hunt";
        public const string ThroneHeralds = "throne_heralds";
        public const string ThroneEliteHunt = "throne_elite_hunt";
        public const string ThroneBossHunt = "throne_boss_hunt";

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
                    Reward(30, 20, Item(DefaultGameItems.MinorPotion, 1), Item(DefaultGameItems.SkillTome, 1)),
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
                    ForestCreatures),

                Quest(ForestCreatures, 9, "Susurros entre espinos",
                    "El Bosque de los Susurros no es un camino seguro. Abre la entrada derrotando a sus criaturas.",
                    "Los espinos retroceden y el sendero queda abierto. Las sombras del bosque ya te han visto.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.ForestCreatureId, 8, "Derrota Espinosos del bosque")
                    },
                    Reward(520, 240, Item(DefaultGameItems.MinorPotion, 2)),
                    ForestEliteHunt),

                Quest(ForestEliteHunt, 10, "Ecos del bosque",
                    "En el Bosque de los Susurros, al norte, rondan Sombras que susurran nombres olvidados. Caza tres.",
                    "Las Sombras callan... pero el Anciano de Espinas sigue despierto en lo profundo.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.ForestEliteId, 3, "Derrota Sombras del bosque")
                    },
                    Reward(400, 180, Item(DefaultGameItems.MinorPotion, 2)),
                    ForestBossHunt),

                Quest(ForestBossHunt, 11, "El corazon del bosque",
                    "El Anciano de Espinas corrompe el bosque desde su corazon, al noroeste. Derrotalo.",
                    "El bosque respira... pero el viento trae ceniza desde las colinas del norte.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.ForestBossId, 1, "Derrota al Anciano de Espinas")
                    },
                    Reward(800, 400, Item(DefaultGameItems.ProtectionRune, 1)),
                    AshWanderers),

                Quest(AshWanderers, 12, "Cenizas al viento",
                    "Mas alla del bosque, las Colinas Cenicientas arden sin fuego. Reduce a los Cenicientos errantes.",
                    "La ceniza no cae del cielo: alguien la respira y la devuelve. Sigue subiendo.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.AshCreatureId, 8, "Derrota Cenicientos errantes")
                    },
                    Reward(1200, 500, Item(DefaultGameItems.MinorPotion, 3)),
                    AshEliteHunt),

                Quest(AshEliteHunt, 13, "Los devoradores",
                    "Los Devoradores de ceniza custodian el paso alto, al oeste de las colinas. Caza dos.",
                    "Solo queda su origen: un corazon que late ceniza en lo alto de la colina.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.AshEliteId, 2, "Derrota Devoradores de ceniza")
                    },
                    Reward(1600, 700, Item(DefaultGameItems.ProtectionRune, 1)),
                    AshBossHunt),

                Quest(AshBossHunt, 14, "El corazon de ceniza",
                    "El Corazon de Ceniza arde en el este de las colinas. Apagalo para siempre.",
                    "Las colinas se enfrian. En lo alto brillan cumbres que no reflejan el sol.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.AshBossId, 1, "Derrota al Corazon de Ceniza")
                    },
                    Reward(3000, 1500, Item(DefaultGameItems.ValleyMedal, 1)),
                    CrystalSentinels),

                Quest(CrystalSentinels, 15, "Cumbres de cristal",
                    "Mas alla de las cenizas se alzan cristales que caminan como guardianes. Rompe su avanzada.",
                    "Los centinelas caen en fragmentos, pero cada pedazo susurra una misma voz.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.CrystalCreatureId, 9, "Derrota Centinelas de cristal")
                    },
                    Reward(4200, 1800, Item(DefaultGameItems.MinorPotion, 3)),
                    CrystalEliteHunt),

                Quest(CrystalEliteHunt, 16, "Custodios prismales",
                    "Los Custodios prismales protegen el sendero alto de las cumbres. Derrota a dos.",
                    "El sendero esta abierto. El Oraculo Fragmentado espera entre ecos de luz.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.CrystalEliteId, 2, "Derrota Custodios prismales")
                    },
                    Reward(5200, 2300, Item(DefaultGameItems.ProtectionRune, 1)),
                    CrystalBossHunt),

                Quest(CrystalBossHunt, 17, "El Oraculo Fragmentado",
                    "El Oraculo Fragmentado intenta recomponer la corrupcion en las cumbres. Detenlo.",
                    "Las cumbres dejan de cantar. El paso glacial se abre hacia el norte.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.CrystalBossId, 1, "Derrota al Oraculo Fragmentado")
                    },
                    Reward(7000, 3200, Item(DefaultGameItems.ValleyMedal, 1)),
                    FrostFront),

                Quest(FrostFront, 18, "Frontera glacial",
                    "El Paso Glacial corta el avance con bestias de escarcha. Reduce su manada.",
                    "La nieve deja de moverse. Aun quedan vigias marcando el camino.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.FrostCreatureId, 10, "Derrota Lobos de escarcha")
                    },
                    Reward(9000, 4200, Item(DefaultGameItems.MinorPotion, 3)),
                    FrostEliteHunt),

                Quest(FrostEliteHunt, 19, "Vigias del hielo",
                    "Los Vigias del hielo custodian los puentes helados. Derrota a tres.",
                    "Los puentes ya no tienen ojos. La Matriarca Invernal ruge desde la cima.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.FrostEliteId, 3, "Derrota Vigias del hielo")
                    },
                    Reward(11000, 5200, Item(DefaultGameItems.ProtectionRune, 1)),
                    FrostBossHunt),

                Quest(FrostBossHunt, 20, "La Matriarca Invernal",
                    "La Matriarca Invernal quiere cubrir el valle bajo hielo eterno. Derribala.",
                    "El frio retrocede y revela unas ruinas bajo aguas oscuras.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.FrostBossId, 1, "Derrota a la Matriarca Invernal")
                    },
                    Reward(16000, 7200, Item(DefaultGameItems.ValleyMedal, 1)),
                    SunkenDead),

                Quest(SunkenDead, 21, "Ruinas sumergidas",
                    "En las Ruinas Sumergidas caminan ahogados antiguos. Despeja la entrada.",
                    "Los viejos cuerpos se hunden otra vez, pero algo abisal responde.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.SunkenCreatureId, 10, "Derrota Ahogados antiguos")
                    },
                    Reward(19000, 8500, Item(DefaultGameItems.MinorPotion, 4)),
                    SunkenEliteHunt),

                Quest(SunkenEliteHunt, 22, "Sirvientes abisales",
                    "Los Sirvientes abisales recogen voces en las ruinas. Rompe su ritual.",
                    "El ritual se apaga. La Reina de las Mareas Rotas se muestra.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.SunkenEliteId, 3, "Derrota Sirvientes abisales")
                    },
                    Reward(23000, 10000, Item(DefaultGameItems.ProtectionRune, 1)),
                    SunkenBossHunt),

                Quest(SunkenBossHunt, 23, "Mareas rotas",
                    "La Reina de las Mareas Rotas levanta el agua contra la tierra. Detenla.",
                    "El agua cae. Bajo las ruinas despierta una forja de obsidiana.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.SunkenBossId, 1, "Derrota a la Reina de las Mareas Rotas")
                    },
                    Reward(32000, 14000, Item(DefaultGameItems.ValleyMedal, 1)),
                    ObsidianForged),

                Quest(ObsidianForged, 24, "Forjados de obsidiana",
                    "La Forja Obsidiana produce soldados sin descanso. Destruye sus forjados.",
                    "La cadena de produccion se rompe. Ahora su martillo busca dueno.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.ObsidianCreatureId, 11, "Derrota Forjados de obsidiana")
                    },
                    Reward(38000, 16500, Item(DefaultGameItems.MinorPotion, 4)),
                    ObsidianEliteHunt),

                Quest(ObsidianEliteHunt, 25, "Martillos vivientes",
                    "Los Martillos vivientes alimentan la forja con golpes de guerra. Caza tres.",
                    "La forja tiembla. El Senor del Yunque Negro sale a defenderla.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.ObsidianEliteId, 3, "Derrota Martillos vivientes")
                    },
                    Reward(45000, 19000, Item(DefaultGameItems.ProtectionRune, 1)),
                    ObsidianBossHunt),

                Quest(ObsidianBossHunt, 26, "El Yunque Negro",
                    "El Senor del Yunque Negro quiere rehacer la corrupcion como metal. Derrotalo.",
                    "La forja se enfria y una luz imposible revela el Jardin Astral.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.ObsidianBossId, 1, "Derrota al Senor del Yunque Negro")
                    },
                    Reward(62000, 26000, Item(DefaultGameItems.ValleyMedal, 1)),
                    AstralSeeds),

                Quest(AstralSeeds, 27, "Jardin astral",
                    "Semillas estelares caen como meteoros vivos. Limpia el Jardin Astral.",
                    "Las semillas dejan de brotar. Los guardias zodiacales aun sostienen el cielo.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.AstralCreatureId, 11, "Derrota Semillas estelares")
                    },
                    Reward(72000, 30000, Item(DefaultGameItems.MinorPotion, 5)),
                    AstralEliteHunt),

                Quest(AstralEliteHunt, 28, "Guardias zodiacales",
                    "Los Guardias zodiacales protegen el centro del jardin. Derrota a tres.",
                    "Las constelaciones tiemblan. El Arbol de Constelaciones pierde sus raices.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.AstralEliteId, 3, "Derrota Guardias zodiacales")
                    },
                    Reward(85000, 36000, Item(DefaultGameItems.ProtectionRune, 1)),
                    AstralBossHunt),

                Quest(AstralBossHunt, 29, "Raices de estrellas",
                    "El Arbol de Constelaciones guia la corrupcion desde el cielo. Talo sus raices.",
                    "El cielo se abre y aparece un santuario cubierto por eclipse.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.AstralBossId, 1, "Derrota al Arbol de Constelaciones")
                    },
                    Reward(120000, 52000, Item(DefaultGameItems.ValleyMedal, 1)),
                    EclipseDevotees),

                Quest(EclipseDevotees, 30, "Santuario del eclipse",
                    "Devotos del eclipse rezan para apagar el dia. Rompe sus filas.",
                    "Las plegarias callan. Los profetas sin sombra siguen mirando.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.EclipseCreatureId, 12, "Derrota Devotos del eclipse")
                    },
                    Reward(140000, 62000, Item(DefaultGameItems.MinorPotion, 5)),
                    EclipseEliteHunt),

                Quest(EclipseEliteHunt, 31, "Profetas sin sombra",
                    "Los Profetas sin sombra predicen tu derrota. Demuestra que se equivocan.",
                    "La profecia se rompe. El Sol Negro desciende sobre el santuario.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.EclipseEliteId, 3, "Derrota Profetas sin sombra")
                    },
                    Reward(160000, 72000, Item(DefaultGameItems.ProtectionRune, 1)),
                    EclipseBossHunt),

                Quest(EclipseBossHunt, 32, "Sol Negro",
                    "El Sol Negro absorbe la luz de las zonas liberadas. Apagalo.",
                    "La luz vuelve, pero revela el Trono del Vacio al final del camino.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.EclipseBossId, 1, "Derrota al Sol Negro")
                    },
                    Reward(220000, 95000, Item(DefaultGameItems.ValleyMedal, 1)),
                    ThroneHeralds),

                Quest(ThroneHeralds, 33, "Heraldos del vacio",
                    "Los Heraldos del vacio anuncian al ultimo rey. Silencia su marcha.",
                    "El anuncio termina. Los caballeros sin nombre guardan la puerta.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.ThroneCreatureId, 14, "Derrota Heraldos del vacio")
                    },
                    Reward(280000, 120000, Item(DefaultGameItems.MinorPotion, 6)),
                    ThroneEliteHunt),

                Quest(ThroneEliteHunt, 34, "Caballeros sin nombre",
                    "Los Caballeros sin nombre no recuerdan por que luchan. Libera a cuatro.",
                    "La puerta del trono se abre. Solo queda el Rey Sin Alba.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.ThroneEliteId, 4, "Derrota Caballeros sin nombre")
                    },
                    Reward(340000, 150000, Item(DefaultGameItems.ProtectionRune, 2)),
                    ThroneBossHunt),

                Quest(ThroneBossHunt, 35, "El Rey Sin Alba",
                    "El Rey Sin Alba espera en el Trono del Vacio. Derrotalo y cierra esta era.",
                    "El trono cae en silencio. El mundo queda listo para la siguiente gran historia.",
                    new[]
                    {
                        Objective(QuestObjectiveType.KillEnemies, DefaultZones.ThroneBossId, 1, "Derrota al Rey Sin Alba")
                    },
                    Reward(500000, 220000, Item(DefaultGameItems.ValleyMedal, 2), Item(DefaultGameItems.ProtectionRune, 2)),
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
