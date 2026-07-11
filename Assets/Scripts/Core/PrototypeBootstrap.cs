using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    public sealed class PrototypeBootstrap : MonoBehaviour
    {
        private const string RuntimeRootName = "MmorpgPrototypeRuntime";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateRuntime()
        {
            if (GameObject.Find(RuntimeRootName) != null)
            {
                return;
            }

            var root = new GameObject(RuntimeRootName);
            root.AddComponent<PrototypeBootstrap>().Build();
        }

        private void Build()
        {
            Localization.Initialize(Resources.Load<LocalizationTable>("Game/LocalizationTable"));
            ConfigureRuntime();
            EnsureEventSystem();
            CreateLighting();
            CreateGround();

            var player = CreatePlayer();
            CreateCamera(player.transform);
            var zone = LoadZone();
            var shop = CreateShopNpc(player);
            var blacksmith = CreateBlacksmithNpc(player);
            var storage = CreateStorageNpc(player);
            var hud = CreateHudAndControls(player, shop, blacksmith, storage);
            shop.Hud = hud;
            blacksmith.Hud = hud;
            storage.Hud = hud;
            CreateZoneSign(zone);
            CreateEnemySpawner(player, hud, zone);
            CreateWorldEvent(player, hud);
        }

        private static ZoneDefinition LoadZone()
        {
            var zones = Resources.LoadAll<ZoneDefinition>("Game/Zones");
            if (zones != null && zones.Length > 0)
            {
                return zones[0];
            }

            // Fallback runtime si el asset no se genero todavia
            // (MMORPG > World > Generate Zones).
            return DefaultZones.CreateZone1();
        }

        private static void CreateZoneSign(ZoneDefinition zone)
        {
            if (zone == null)
            {
                return;
            }

            var sign = new GameObject("Zone Sign");
            sign.transform.position = new Vector3(0f, 0f, -6.5f);

            var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = "Post";
            post.transform.SetParent(sign.transform, false);
            post.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            post.transform.localScale = new Vector3(0.25f, 2.2f, 0.25f);

            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.35f, 0.26f, 0.16f);
            post.GetComponent<Renderer>().sharedMaterial = material;

            CreateWorldLabel(sign.transform, $"{zone.DisplayName}\nZona {zone.MinLevel}-{zone.MaxLevel}", new Color(1f, 0.9f, 0.6f), 2.6f);
        }

        private static void ConfigureRuntime()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static void CreateLighting()
        {
            if (FindAnyObjectByType<Light>() == null)
            {
                var lightObject = new GameObject("Directional Light");
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.15f;
                light.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            }

            RenderSettings.ambientLight = new Color(0.42f, 0.46f, 0.52f);
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Training Field";
            ground.transform.localScale = new Vector3(7f, 1f, 7f);

            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.22f, 0.34f, 0.24f);
            ground.GetComponent<Renderer>().sharedMaterial = material;

            for (var i = 0; i < 16; i++)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = "Field Stone";
                marker.transform.position = new Vector3(Random.Range(-28f, 28f), 0.18f, Random.Range(-28f, 28f));
                marker.transform.localScale = new Vector3(Random.Range(0.7f, 1.6f), 0.35f, Random.Range(0.7f, 1.6f));

                var stoneMaterial = new Material(Shader.Find("Standard"));
                stoneMaterial.color = new Color(0.28f, 0.28f, 0.3f);
                marker.GetComponent<Renderer>().sharedMaterial = stoneMaterial;
            }
        }

        private static GameObject CreatePlayer()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player - Guerrero Prototype";
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);

            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.18f, 0.42f, 0.9f);
            player.GetComponent<Renderer>().sharedMaterial = material;

            var capsule = player.GetComponent<CapsuleCollider>();
            if (capsule != null)
            {
                Destroy(capsule);
            }

            var controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.38f;
            controller.stepOffset = 0.35f;

            var health = player.AddComponent<Health>();
            health.ResetHealth(160);

            player.AddComponent<PlayerController>();

            var combat = player.AddComponent<PlayerCombat>();
            combat.AttackDamage = 26;
            combat.AttackRange = 2.25f;
            combat.AttackCooldown = 0.65f;

            player.AddComponent<PlayerCharacterIdentity>();
            player.AddComponent<PlayerAvatarVisual>();
            player.AddComponent<PlayerClassController>();
            var progression = player.AddComponent<PlayerProgression>();
            progression.Table = LoadLevelTable();
            player.AddComponent<InventorySystem>();
            player.AddComponent<PlayerQuestLog>();
            player.AddComponent<EquipmentUpgradeSystem>();
            player.AddComponent<PlayerEquipment>();
            player.AddComponent<PetService>();
            player.AddComponent<MountService>();
            player.AddComponent<StorageService>();
            player.AddComponent<PlayerAttributes>();
            player.AddComponent<PlayerSkills>();
            player.AddComponent<PlayerPersistence>();
            player.AddComponent<MmorpgNetworkClient>();

            return player;
        }

        private static ItemDatabase LoadItemDatabase()
        {
            var database = Resources.Load<ItemDatabase>("Game/ItemDatabase");
            if (database != null && database.Items != null && database.Items.Count > 0)
            {
                database.RebuildLookup();
                return database;
            }

            // Fallback runtime si los assets no se generaron todavia
            // (MMORPG > Items > Generate Item Database).
            var rarities = Resources.Load<RarityTable>("Game/RarityTable");
            if (rarities == null)
            {
                rarities = ScriptableObject.CreateInstance<RarityTable>();
                rarities.FillWithDefaults();
            }

            return DefaultGameItems.CreateRuntimeDatabase(rarities);
        }

        private static UpgradeConfig LoadUpgradeConfig()
        {
            var config = Resources.Load<UpgradeConfig>("Game/UpgradeConfig");
            if (config != null && config.HasSteps)
            {
                return config;
            }

            // Fallback runtime si el asset no se genero todavia
            // (MMORPG > Items > Generate Upgrade Config).
            var runtimeConfig = ScriptableObject.CreateInstance<UpgradeConfig>();
            runtimeConfig.FillWithDefaults();
            return runtimeConfig;
        }

        private static LevelProgressionTable LoadLevelTable()
        {
            var table = Resources.Load<LevelProgressionTable>("Game/LevelProgressionTable");
            if (table != null && table.HasRows)
            {
                return table;
            }

            // Fallback runtime si el asset no se genero todavia
            // (MMORPG > Progression > Generate Level Table).
            var config = Resources.Load<ExpCurveConfig>("Game/ExpCurveConfig");
            var effectiveConfig = config != null ? config : ScriptableObject.CreateInstance<ExpCurveConfig>();
            var runtimeTable = ScriptableObject.CreateInstance<LevelProgressionTable>();
            runtimeTable.GenerateFrom(effectiveConfig);
            return runtimeTable;
        }

        private static void CreateCamera(Transform target)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";

            var camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;
            camera.fieldOfView = 55f;

            var listener = cameraObject.AddComponent<AudioListener>();
            listener.enabled = true;

            var follow = cameraObject.AddComponent<OrbitCamera>();
            follow.Target = target;
        }

        private static void CreateEnemySpawner(GameObject player, PrototypeHud hud, ZoneDefinition zone)
        {
            var spawnerObject = new GameObject("Enemy Spawner");
            var spawner = spawnerObject.AddComponent<EnemySpawner>();
            spawner.Target = player.transform;
            spawner.Progression = player.GetComponent<PlayerProgression>();
            spawner.Inventory = player.GetComponent<InventorySystem>();
            spawner.QuestLog = player.GetComponent<PlayerQuestLog>();
            spawner.Hud = hud;
            spawner.Loot = LoadLootTable();
            spawner.Zone = zone;
        }

        private static System.Collections.Generic.List<QuestDefinition> LoadQuestLine()
        {
            var loaded = Resources.LoadAll<QuestDefinition>("Game/Quests");
            if (loaded != null && loaded.Length > 0)
            {
                var quests = new System.Collections.Generic.List<QuestDefinition>(loaded);
                quests.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
                return quests;
            }

            // Fallback runtime si los assets no se generaron todavia
            // (MMORPG > Quests > Generate Quests).
            return DefaultQuests.CreateAll();
        }

        private static System.Collections.Generic.List<PetDefinition> LoadPets()
        {
            var loaded = Resources.LoadAll<PetDefinition>("Game/Pets");
            if (loaded != null && loaded.Length > 0)
            {
                return new System.Collections.Generic.List<PetDefinition>(loaded);
            }

            return DefaultCompanions.CreatePets();
        }

        private static System.Collections.Generic.List<MountDefinition> LoadMounts()
        {
            var loaded = Resources.LoadAll<MountDefinition>("Game/Mounts");
            if (loaded != null && loaded.Length > 0)
            {
                return new System.Collections.Generic.List<MountDefinition>(loaded);
            }

            return DefaultCompanions.CreateMounts();
        }

        private static AttributeConfig LoadAttributeConfig()
        {
            var config = Resources.Load<AttributeConfig>("Game/AttributeConfig");
            if (config != null)
            {
                return config;
            }

            // Fallback runtime si el asset no se genero todavia
            // (MMORPG > Progression > Generate Attribute Config).
            return ScriptableObject.CreateInstance<AttributeConfig>();
        }

        private static LootTableConfig LoadLootTable()
        {
            var loot = Resources.Load<LootTableConfig>("Game/LootTable");
            if (loot != null && loot.HasEntries)
            {
                return loot;
            }

            // Fallback runtime si el asset no se genero todavia
            // (MMORPG > Items > Generate Loot Table).
            var runtimeLoot = ScriptableObject.CreateInstance<LootTableConfig>();
            runtimeLoot.FillWithDefaults();
            return runtimeLoot;
        }

        private static ShopNpc CreateShopNpc(GameObject player)
        {
            var npc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            npc.name = "Mercader del Valle";
            npc.transform.position = new Vector3(-3.2f, 1f, -2.5f);
            npc.transform.localScale = new Vector3(0.7f, 1.05f, 0.7f);

            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.93f, 0.7f, 0.28f);
            npc.GetComponent<Renderer>().sharedMaterial = material;

            var shop = npc.AddComponent<ShopNpc>();
            shop.Player = player.transform;
            shop.Progression = player.GetComponent<PlayerProgression>();
            shop.Inventory = player.GetComponent<InventorySystem>();
            shop.QuestLog = player.GetComponent<PlayerQuestLog>();

            CreateWorldLabel(npc.transform, "Mercader\ncompra pociones", new Color(1f, 0.92f, 0.55f), 1.55f);
            return shop;
        }

        private static BlacksmithNpc CreateBlacksmithNpc(GameObject player)
        {
            var npc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            npc.name = "Herrero del Campamento";
            npc.transform.position = new Vector3(3.6f, 1f, -3.4f);
            npc.transform.localScale = new Vector3(0.75f, 1.05f, 0.75f);

            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.5f, 0.5f, 0.56f);
            npc.GetComponent<Renderer>().sharedMaterial = material;

            var blacksmith = npc.AddComponent<BlacksmithNpc>();
            blacksmith.Player = player.transform;
            blacksmith.Equipment = player.GetComponent<EquipmentUpgradeSystem>();
            blacksmith.QuestLog = player.GetComponent<PlayerQuestLog>();

            CreateWorldLabel(npc.transform, "Herrero\nmejora tu equipo", new Color(0.8f, 0.85f, 0.95f), 1.55f);
            return blacksmith;
        }

        private static StorageNpc CreateStorageNpc(GameObject player)
        {
            var npc = GameObject.CreatePrimitive(PrimitiveType.Cube);
            npc.name = "Almacen del Campamento";
            npc.transform.position = new Vector3(0.4f, 0.7f, -4.8f);
            npc.transform.localScale = new Vector3(1.2f, 1.4f, 1.2f);

            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.55f, 0.42f, 0.22f);
            npc.GetComponent<Renderer>().sharedMaterial = material;

            var storageService = player.GetComponent<StorageService>();
            var storage = npc.AddComponent<StorageNpc>();
            storage.Player = player.transform;
            storage.Storage = storageService;

            CreateWorldLabel(npc.transform, "Almacen\nguarda materiales", new Color(0.95f, 0.85f, 0.6f), 1.35f);
            return storage;
        }

        private static void CreateWorldEvent(GameObject player, PrototypeHud hud)
        {
            var eventObject = new GameObject("World Event System");
            var worldEvent = eventObject.AddComponent<WorldEventSystem>();
            worldEvent.Target = player.transform;
            worldEvent.Progression = player.GetComponent<PlayerProgression>();
            worldEvent.Inventory = player.GetComponent<InventorySystem>();
            worldEvent.QuestLog = player.GetComponent<PlayerQuestLog>();
            worldEvent.Hud = hud;
        }

        private static PrototypeHud CreateHudAndControls(GameObject player, ShopNpc shop, BlacksmithNpc blacksmith, StorageNpc storage)
        {
            var canvasObject = new GameObject("Prototype HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            var safeAreaRoot = CreateUiObject("Mobile Safe Area Root", canvas.transform);
            StretchToParent(safeAreaRoot.GetComponent<RectTransform>());
            safeAreaRoot.AddComponent<SafeAreaFitter>();
            var uiRoot = safeAreaRoot.transform;

            CreatePanel(uiRoot, "Vitals Panel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(940f, 332f), new Vector2(18f, -18f), new Color(0.025f, 0.032f, 0.04f, 0.58f));
            CreatePanel(uiRoot, "Action Buttons Panel", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(438f, 330f), new Vector2(-22f, 22f), new Color(0.025f, 0.032f, 0.04f, 0.42f));
            CreatePanel(uiRoot, "Network Panel", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(566f, 122f), new Vector2(-18f, -86f), new Color(0.025f, 0.032f, 0.04f, 0.50f));
            CreatePanel(uiRoot, "Activity Panel", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(500f, 132f), new Vector2(-24f, -118f), new Color(0.025f, 0.032f, 0.04f, 0.42f));

            var hud = canvasObject.AddComponent<PrototypeHud>();
            hud.PlayerHealthFill = CreateBar(uiRoot, "Player Health", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(430f, 36f), new Vector2(28f, -28f), new Color(0.05f, 0.08f, 0.1f), new Color(0.1f, 0.8f, 0.32f), out var playerText);
            hud.PlayerHealthText = playerText;
            hud.EnemyHealthFill = CreateBar(uiRoot, "Enemy Health", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(430f, 28f), new Vector2(28f, -76f), new Color(0.05f, 0.08f, 0.1f), new Color(0.95f, 0.24f, 0.18f), out var enemyText);
            hud.EnemyHealthText = enemyText;
            hud.StatusText = CreateText(uiRoot, "Status", "Fase 5: prototipo Android preparado para pruebas.", 25, TextAnchor.MiddleLeft);
            SetRect(hud.StatusText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(760f, 36f), new Vector2(28f, -118f));
            hud.ClassText = CreateText(uiRoot, "Class Info", string.Empty, 23, TextAnchor.MiddleLeft);
            SetRect(hud.ClassText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(760f, 34f), new Vector2(28f, -158f));
            hud.ProgressionText = CreateText(uiRoot, "Progression Info", string.Empty, 23, TextAnchor.MiddleLeft);
            SetRect(hud.ProgressionText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(760f, 34f), new Vector2(28f, -194f));
            hud.QuestText = CreateText(uiRoot, "Quest Info", string.Empty, 21, TextAnchor.MiddleLeft);
            SetRect(hud.QuestText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(840f, 32f), new Vector2(28f, -230f));
            hud.InventoryText = CreateText(uiRoot, "Inventory Info", string.Empty, 21, TextAnchor.MiddleLeft);
            SetRect(hud.InventoryText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(900f, 32f), new Vector2(28f, -264f));
            hud.EquipmentText = CreateText(uiRoot, "Equipment Info", string.Empty, 21, TextAnchor.MiddleLeft);
            SetRect(hud.EquipmentText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(900f, 32f), new Vector2(28f, -298f));
            hud.FeedText = CreateText(uiRoot, "Activity Feed", string.Empty, 18, TextAnchor.LowerRight);
            SetRect(hud.FeedText.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(460f, 104f), new Vector2(-44f, -116f));

            var movementJoystick = CreateJoystick(uiRoot);
            player.GetComponent<PlayerController>().MovementJoystick = movementJoystick;

            var attackButton = CreateRoundButton(uiRoot, "Attack Button", "ATK", new Vector2(1f, 0f), new Vector2(-170f, 170f), new Vector2(148f, 148f), new Color(0.88f, 0.28f, 0.16f));
            attackButton.onClick.AddListener(player.GetComponent<PlayerCombat>().TryAttack);

            var skills = player.GetComponent<PlayerSkills>();
            var skillOneButton = CreateRoundButton(uiRoot, "Skill One Button", "Q", new Vector2(1f, 0f), new Vector2(-344f, 172f), new Vector2(130f, 92f), new Color(0.18f, 0.36f, 0.86f), 22);
            skillOneButton.onClick.AddListener(skills.UseSkillOne);
            hud.SkillOneText = skillOneButton.GetComponentInChildren<Text>();

            var skillTwoButton = CreateRoundButton(uiRoot, "Skill Two Button", "E", new Vector2(1f, 0f), new Vector2(-302f, 286f), new Vector2(130f, 92f), new Color(0.47f, 0.2f, 0.78f), 22);
            skillTwoButton.onClick.AddListener(skills.UseSkillTwo);
            hud.SkillTwoText = skillTwoButton.GetComponentInChildren<Text>();

            CreateClassButtons(uiRoot, player);

            var combat = player.GetComponent<PlayerCombat>();
            combat.Hud = hud;
            var progression = player.GetComponent<PlayerProgression>();
            progression.Hud = hud;
            var inventory = player.GetComponent<InventorySystem>();
            var questLog = player.GetComponent<PlayerQuestLog>();
            var equipment = player.GetComponent<EquipmentUpgradeSystem>();
            var database = LoadItemDatabase();
            inventory.Database = database;
            inventory.Hud = hud;
            inventory.QuestLog = questLog;
            questLog.Hud = hud;
            questLog.Progression = progression;
            questLog.Inventory = inventory;
            equipment.Hud = hud;
            equipment.Progression = progression;
            equipment.Inventory = inventory;
            var upgradeConfig = LoadUpgradeConfig();
            equipment.Config = upgradeConfig;
            var gear = player.GetComponent<PlayerEquipment>();
            gear.Database = database;
            gear.Inventory = inventory;
            gear.Progression = progression;
            gear.ClassController = player.GetComponent<PlayerClassController>();
            gear.UpgradeSystem = equipment;
            gear.Upgrades = upgradeConfig;
            gear.Hud = hud;
            skills.Hud = hud;
            var persistence = player.GetComponent<PlayerPersistence>();
            persistence.Identity = player.GetComponent<PlayerCharacterIdentity>();
            persistence.ClassController = player.GetComponent<PlayerClassController>();
            persistence.Progression = progression;
            persistence.Inventory = inventory;
            persistence.Equipment = equipment;
            persistence.Gear = gear;
            persistence.QuestLog = questLog;
            var petService = player.GetComponent<PetService>();
            petService.Hud = hud;
            petService.Progression = progression;
            petService.Initialize(LoadPets());
            var mountService = player.GetComponent<MountService>();
            mountService.Hud = hud;
            mountService.Progression = progression;
            mountService.UpgradeSystem = equipment;
            mountService.Initialize(LoadMounts());
            persistence.Pets = petService;
            persistence.Mounts = mountService;
            var storageService = player.GetComponent<StorageService>();
            storageService.Inventory = inventory;
            storageService.Hud = hud;
            persistence.Storage = storageService;
            var attributes = player.GetComponent<PlayerAttributes>();
            attributes.Config = LoadAttributeConfig();
            attributes.Progression = progression;
            attributes.UpgradeSystem = equipment;
            attributes.Hud = hud;
            persistence.Attributes = attributes;
            hud.Bind(player.GetComponent<Health>(), player.GetComponent<PlayerClassController>(), player.GetComponent<PlayerCharacterIdentity>(), progression, skills, inventory, questLog, equipment, combat);
            questLog.Initialize(LoadQuestLine());
            inventory.AddItem(DefaultGameItems.MinorPotion, 2);
            inventory.AddItem(DefaultGameItems.RecruitSword);
            CreateShopButtons(uiRoot, player, shop, blacksmith, storage);
            CreateNetworkPanel(uiRoot, player);

            var help = CreateText(uiRoot, "Controls Help", "WASD/joystick moverse  |  ATK/Space ataque  |  Q/E habilidades  |  Click derecho camara", 20, TextAnchor.MiddleCenter);
            SetRect(help.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1040f, 34f), new Vector2(0f, 28f));
            CreateStartupSplash(uiRoot);
            CreateCharacterSelectionPanel(uiRoot, player, hud);
            return hud;
        }

        private static void CreateShopButtons(Transform parent, GameObject player, ShopNpc shop, BlacksmithNpc blacksmith, StorageNpc storage)
        {
            var equipment = player.GetComponent<EquipmentUpgradeSystem>();

            var potionButton = CreateRoundButton(parent, "Use Potion Button", "POTION", new Vector2(0f, 1f), new Vector2(92f, -348f), new Vector2(128f, 42f), new Color(0.15f, 0.52f, 0.36f), 17);
            potionButton.onClick.AddListener(equipment.TryUsePotion);

            var buyButton = CreateRoundButton(parent, "Buy Potion Button", "BUY 25", new Vector2(0f, 1f), new Vector2(232f, -348f), new Vector2(128f, 42f), new Color(0.47f, 0.34f, 0.11f), 17);
            buyButton.onClick.AddListener(shop.BuyPotion);

            var weaponButton = CreateRoundButton(parent, "Upgrade Weapon Button", "WEAPON", new Vector2(0f, 1f), new Vector2(372f, -348f), new Vector2(128f, 42f), new Color(0.58f, 0.2f, 0.15f), 17);
            weaponButton.onClick.AddListener(blacksmith.UpgradeWeapon);

            var armorButton = CreateRoundButton(parent, "Upgrade Armor Button", "ARMOR", new Vector2(0f, 1f), new Vector2(512f, -348f), new Vector2(128f, 42f), new Color(0.22f, 0.28f, 0.48f), 17);
            armorButton.onClick.AddListener(blacksmith.UpgradeArmor);

            var gear = player.GetComponent<PlayerEquipment>();
            var equipButton = CreateRoundButton(parent, "Auto Equip Button", "EQUIPAR", new Vector2(0f, 1f), new Vector2(652f, -348f), new Vector2(128f, 42f), new Color(0.3f, 0.42f, 0.24f), 17);
            equipButton.onClick.AddListener(gear.EquipBestFromInventory);

            var talkButton = CreateRoundButton(parent, "Talk Button", "HABLAR", new Vector2(0f, 1f), new Vector2(792f, -348f), new Vector2(128f, 42f), new Color(0.5f, 0.38f, 0.16f), 17);
            talkButton.onClick.AddListener(() => TalkToNearest(player.transform, shop, blacksmith));

            var petService = player.GetComponent<PetService>();
            var petButton = CreateRoundButton(parent, "Pet Button", "MASCOTA", new Vector2(0f, 1f), new Vector2(92f, -396f), new Vector2(128f, 42f), new Color(0.62f, 0.4f, 0.14f), 17);
            petButton.onClick.AddListener(petService.ToggleDefault);

            var mountService = player.GetComponent<MountService>();
            var mountButton = CreateRoundButton(parent, "Mount Button", "MONTURA", new Vector2(0f, 1f), new Vector2(232f, -396f), new Vector2(128f, 42f), new Color(0.32f, 0.36f, 0.2f), 17);
            mountButton.onClick.AddListener(mountService.ToggleMount);

            var storageButton = CreateRoundButton(parent, "Storage Button", "ALMACEN", new Vector2(0f, 1f), new Vector2(372f, -396f), new Vector2(128f, 42f), new Color(0.42f, 0.34f, 0.18f), 17);
            storageButton.onClick.AddListener(storage.ToggleStorage);

            var statsWindow = CreateStatsWindow(parent, player);
            var statsButton = CreateRoundButton(parent, "Stats Button", "STATS", new Vector2(0f, 1f), new Vector2(512f, -396f), new Vector2(128f, 42f), new Color(0.26f, 0.3f, 0.5f), 17);
            statsButton.onClick.AddListener(statsWindow.Toggle);
        }

        private static StatsWindowController CreateStatsWindow(Transform parent, GameObject player)
        {
            var window = CreateUiObject("Stats Window", parent);
            var windowRect = window.GetComponent<RectTransform>();
            SetRect(windowRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 320f), Vector2.zero);

            var background = window.AddComponent<Image>();
            background.color = new Color(0.04f, 0.055f, 0.07f, 0.94f);
            background.raycastTarget = true;

            var title = CreateText(window.transform, "Title", "Atributos", 30, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(520f, 44f), new Vector2(0f, -14f));

            var controller = window.AddComponent<StatsWindowController>();
            controller.Panel = window;
            controller.Attributes = player.GetComponent<PlayerAttributes>();
            controller.Progression = player.GetComponent<PlayerProgression>();

            controller.PointsText = CreateText(window.transform, "Points", string.Empty, 22, TextAnchor.MiddleLeft);
            SetRect(controller.PointsText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(480f, 34f), new Vector2(26f, -62f));

            controller.StrengthText = CreateText(window.transform, "Strength", string.Empty, 20, TextAnchor.MiddleLeft);
            SetRect(controller.StrengthText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(420f, 32f), new Vector2(26f, -110f));
            var strengthButton = CreateRoundButton(window.transform, "Add Strength", "+", new Vector2(1f, 1f), new Vector2(-52f, -110f), new Vector2(52f, 36f), new Color(0.58f, 0.24f, 0.16f), 24);
            strengthButton.onClick.AddListener(controller.SpendStrength);

            controller.VitalityText = CreateText(window.transform, "Vitality", string.Empty, 20, TextAnchor.MiddleLeft);
            SetRect(controller.VitalityText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(420f, 32f), new Vector2(26f, -158f));
            var vitalityButton = CreateRoundButton(window.transform, "Add Vitality", "+", new Vector2(1f, 1f), new Vector2(-52f, -158f), new Vector2(52f, 36f), new Color(0.16f, 0.5f, 0.3f), 24);
            vitalityButton.onClick.AddListener(controller.SpendVitality);

            controller.AgilityText = CreateText(window.transform, "Agility", string.Empty, 20, TextAnchor.MiddleLeft);
            SetRect(controller.AgilityText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(420f, 32f), new Vector2(26f, -206f));
            var agilityButton = CreateRoundButton(window.transform, "Add Agility", "+", new Vector2(1f, 1f), new Vector2(-52f, -206f), new Vector2(52f, 36f), new Color(0.2f, 0.36f, 0.62f), 24);
            agilityButton.onClick.AddListener(controller.SpendAgility);

            var closeButton = CreateRoundButton(window.transform, "Close Stats", "CERRAR", new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(160f, 44f), new Color(0.32f, 0.32f, 0.36f), 19);
            closeButton.onClick.AddListener(controller.Toggle);

            window.SetActive(false);
            return controller;
        }

        private static void TalkToNearest(Transform player, ShopNpc shop, BlacksmithNpc blacksmith)
        {
            var shopDistance = Vector3.Distance(player.position, shop.transform.position);
            var blacksmithDistance = Vector3.Distance(player.position, blacksmith.transform.position);

            if (blacksmithDistance < shopDistance)
            {
                blacksmith.Talk();
            }
            else
            {
                shop.Talk();
            }
        }

        private static void CreateNetworkPanel(Transform parent, GameObject player)
        {
            var network = player.GetComponent<MmorpgNetworkClient>();
            network.PlayerTransform = player.transform;
            network.ClassController = player.GetComponent<PlayerClassController>();
            network.Identity = player.GetComponent<PlayerCharacterIdentity>();
            player.GetComponent<PlayerProgression>().Network = network;

            network.NetworkStatusText = CreateText(parent, "Network Status", "Offline", 20, TextAnchor.MiddleRight);
            SetRect(network.NetworkStatusText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(520f, 28f), new Vector2(-24f, -96f));

            network.UrlInput = CreateInputField(parent, "Server Url Input", "ws://localhost:7777", "server url", new Vector2(1f, 1f), new Vector2(-244f, -138f), new Vector2(360f, 42f));

            var connectButton = CreateRoundButton(parent, "Online Button", "ONLINE", new Vector2(1f, 1f), new Vector2(-70f, -138f), new Vector2(118f, 42f), new Color(0.08f, 0.48f, 0.34f), 18);
            connectButton.onClick.AddListener(network.Connect);

            var chatBackground = CreateUiObject("Chat Background", parent);
            var chatImage = chatBackground.AddComponent<Image>();
            chatImage.color = new Color(0.04f, 0.05f, 0.06f, 0.58f);
            chatImage.raycastTarget = false;
            SetRect(chatBackground.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(620f, 164f), new Vector2(0f, 88f));

            network.ChatLogText = CreateText(parent, "Chat Log", "Chat online local.", 18, TextAnchor.LowerLeft);
            SetRect(network.ChatLogText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(590f, 94f), new Vector2(-5f, 126f));

            network.ChatInput = CreateInputField(parent, "Chat Input", string.Empty, "mensaje...", new Vector2(0.5f, 0f), new Vector2(-66f, 52f), new Vector2(460f, 42f));

            var sendButton = CreateRoundButton(parent, "Send Chat Button", "SEND", new Vector2(0.5f, 0f), new Vector2(232f, 52f), new Vector2(112f, 42f), new Color(0.18f, 0.32f, 0.62f), 18);
            sendButton.onClick.AddListener(network.SendChatFromInput);
        }

        private static void CreateClassButtons(Transform parent, GameObject player)
        {
            var classController = player.GetComponent<PlayerClassController>();
            var classes = new[]
            {
                CharacterClassType.Guerrero,
                CharacterClassType.Ninja,
                CharacterClassType.Chaman,
                CharacterClassType.Umbra
            };

            for (var i = 0; i < classes.Length; i++)
            {
                var classType = classes[i];
                var definition = ClassDefinition.Create(classType);
                var button = CreateRoundButton(parent, $"{definition.DisplayName} Button", definition.DisplayName, new Vector2(0.5f, 1f), new Vector2(-270f + i * 180f, -38f), new Vector2(160f, 48f), definition.BodyColor, 21);
                button.onClick.AddListener(() => classController.ApplyClass(classType));
            }
        }

        private static VirtualJoystick CreateJoystick(Transform parent)
        {
            var background = CreateUiObject("Move Joystick", parent);
            var backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0.08f, 0.1f, 0.12f, 0.52f);
            backgroundImage.raycastTarget = true;
            SetRect(background.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0.5f, 0.5f), new Vector2(220f, 220f), new Vector2(170f, 170f));

            var knob = CreateUiObject("Joystick Knob", background.transform);
            var knobImage = knob.AddComponent<Image>();
            knobImage.color = new Color(0.74f, 0.82f, 0.92f, 0.86f);
            SetRect(knob.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(92f, 92f), Vector2.zero);

            var joystick = background.AddComponent<VirtualJoystick>();
            joystick.Knob = knob.GetComponent<RectTransform>();
            joystick.Radius = 82f;
            return joystick;
        }

        private static Button CreateRoundButton(Transform parent, string name, string label, Vector2 anchor, Vector2 position, Vector2 size, Color color, int fontSize = 32)
        {
            var buttonObject = CreateUiObject(name, parent);
            var image = buttonObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            SetRect(buttonObject.GetComponent<RectTransform>(), anchor, new Vector2(0.5f, 0.5f), size, position);

            var button = buttonObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = color * 1.14f;
            colors.pressedColor = color * 0.78f;
            button.colors = colors;

            var text = CreateText(buttonObject.transform, "Label", label, fontSize, TextAnchor.MiddleCenter);
            SetRect(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, Vector2.zero);
            text.fontStyle = FontStyle.Bold;

            return button;
        }

        private static InputField CreateInputField(Transform parent, string name, string value, string placeholder, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var inputObject = CreateUiObject(name, parent);
            var image = inputObject.AddComponent<Image>();
            image.color = new Color(0.03f, 0.04f, 0.05f, 0.78f);
            image.raycastTarget = true;
            SetRect(inputObject.GetComponent<RectTransform>(), anchor, new Vector2(0.5f, 0.5f), size, position);

            var text = CreateText(inputObject.transform, "Text", string.Empty, 18, TextAnchor.MiddleLeft);
            text.color = Color.white;
            text.supportRichText = false;
            SetRect(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size - new Vector2(24f, 0f), Vector2.zero);

            var placeholderText = CreateText(inputObject.transform, "Placeholder", placeholder, 18, TextAnchor.MiddleLeft);
            placeholderText.color = new Color(0.72f, 0.78f, 0.84f, 0.65f);
            SetRect(placeholderText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size - new Vector2(24f, 0f), Vector2.zero);

            var input = inputObject.AddComponent<InputField>();
            input.textComponent = text;
            input.placeholder = placeholderText;
            input.text = value;
            input.characterLimit = 160;
            return input;
        }

        private static void CreateWorldLabel(Transform parent, string text, Color color, float height)
        {
            var labelObject = new GameObject("World Label");
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = new Vector3(0f, height, 0f);

            var label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.fontSize = 38;
            label.characterSize = 0.045f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = color;
        }

        private static void CreateStartupSplash(Transform parent)
        {
            var splashObject = CreateUiObject("Startup Splash", parent);
            var rect = splashObject.GetComponent<RectTransform>();
            StretchToParent(rect);

            var background = splashObject.AddComponent<Image>();
            background.color = new Color(0.03f, 0.035f, 0.045f, 0.92f);
            background.raycastTarget = false;

            var group = splashObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;

            var title = CreateText(splashObject.transform, "Title", "Valle de las Reliquias", 54, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(880f, 84f), new Vector2(0f, 34f));

            var subtitle = CreateText(splashObject.transform, "Subtitle", "Prototipo Android 0.1", 24, TextAnchor.MiddleCenter);
            subtitle.color = new Color(0.78f, 0.88f, 1f);
            SetRect(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(640f, 42f), new Vector2(0f, -34f));

            var splash = splashObject.AddComponent<StartupSplash>();
            splash.Group = group;
        }

        private static void CreateCharacterSelectionPanel(Transform parent, GameObject player, PrototypeHud hud)
        {
            var identity = player.GetComponent<PlayerCharacterIdentity>();
            var classController = player.GetComponent<PlayerClassController>();
            var network = player.GetComponent<MmorpgNetworkClient>();
            var persistence = player.GetComponent<PlayerPersistence>();
            var savedData = persistence != null ? persistence.LoadOrNull() : null;

            var selectedClass = CharacterClassType.Guerrero;
            var selectedGender = CharacterGender.Masculino;

            if (savedData != null)
            {
                if (System.Enum.TryParse(savedData.ClassName, out CharacterClassType savedClass))
                {
                    selectedClass = savedClass;
                }

                if (System.Enum.TryParse(savedData.GenderName, out CharacterGender savedGender))
                {
                    selectedGender = savedGender;
                }
            }

            var overlay = CreateUiObject("Character Selection Overlay", parent);
            StretchToParent(overlay.GetComponent<RectTransform>());

            var overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0.015f, 0.018f, 0.024f, 0.88f);
            overlayImage.raycastTarget = true;

            CreatePanel(overlay.transform, "Selection Frame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1060f, 660f), Vector2.zero, new Color(0.04f, 0.055f, 0.07f, 0.94f));

            var title = CreateText(overlay.transform, "Selection Title", "Crear personaje", 48, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(820f, 62f), new Vector2(0f, 252f));

            var subtitle = CreateText(overlay.transform, "Selection Subtitle", "Elige clase, sexo y nombre para entrar al Valle de las Reliquias.", 22, TextAnchor.MiddleCenter);
            subtitle.color = new Color(0.78f, 0.86f, 0.94f);
            SetRect(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(880f, 38f), new Vector2(0f, 204f));

            var preview = CreateText(overlay.transform, "Selection Preview", string.Empty, 26, TextAnchor.MiddleCenter);
            preview.fontStyle = FontStyle.Bold;
            preview.color = new Color(1f, 0.9f, 0.55f);
            SetRect(preview.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(820f, 48f), new Vector2(0f, 132f));

            var nameInput = CreateInputField(overlay.transform, "Character Name Input", savedData != null ? savedData.CharacterName : "Heroe", "nombre del personaje", new Vector2(0.5f, 0.5f), new Vector2(0f, 74f), new Vector2(520f, 52f));
            nameInput.characterLimit = 14;

            var classLabel = CreateText(overlay.transform, "Class Label", "Clase", 22, TextAnchor.MiddleCenter);
            classLabel.fontStyle = FontStyle.Bold;
            SetRect(classLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(280f, 32f), new Vector2(-292f, 10f));

            var genderLabel = CreateText(overlay.transform, "Gender Label", "Sexo", 22, TextAnchor.MiddleCenter);
            genderLabel.fontStyle = FontStyle.Bold;
            SetRect(genderLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(280f, 32f), new Vector2(292f, 10f));

            var classes = new[]
            {
                CharacterClassType.Guerrero,
                CharacterClassType.Ninja,
                CharacterClassType.Chaman,
                CharacterClassType.Umbra
            };

            for (var i = 0; i < classes.Length; i++)
            {
                var classType = classes[i];
                var definition = ClassDefinition.Create(classType);
                var button = CreateRoundButton(overlay.transform, $"Create {definition.DisplayName} Button", definition.DisplayName, new Vector2(0.5f, 0.5f), new Vector2(-292f, -44f - i * 58f), new Vector2(250f, 44f), definition.BodyColor, 19);
                button.onClick.AddListener(() =>
                {
                    selectedClass = classType;
                    classController.ApplyClass(selectedClass);
                    RefreshPreview();
                });
            }

            var maleButton = CreateRoundButton(overlay.transform, "Select Male Button", "Masculino", new Vector2(0.5f, 0.5f), new Vector2(292f, -56f), new Vector2(260f, 52f), new Color(0.18f, 0.34f, 0.72f), 20);
            maleButton.onClick.AddListener(() =>
            {
                selectedGender = CharacterGender.Masculino;
                identity.ApplySelection(nameInput.text, selectedGender);
                classController.ApplyClass(selectedClass);
                RefreshPreview();
            });

            var femaleButton = CreateRoundButton(overlay.transform, "Select Female Button", "Femenino", new Vector2(0.5f, 0.5f), new Vector2(292f, -124f), new Vector2(260f, 52f), new Color(0.62f, 0.24f, 0.58f), 20);
            femaleButton.onClick.AddListener(() =>
            {
                selectedGender = CharacterGender.Femenino;
                identity.ApplySelection(nameInput.text, selectedGender);
                classController.ApplyClass(selectedClass);
                RefreshPreview();
            });

            nameInput.onEndEdit.AddListener(_ =>
            {
                identity.ApplySelection(nameInput.text, selectedGender);
                classController.ApplyClass(selectedClass);
                RefreshPreview();
            });

            var confirmPosition = savedData != null ? new Vector2(150f, -254f) : new Vector2(0f, -254f);
            var confirm = CreateRoundButton(overlay.transform, "Confirm Character Button", "CREAR", new Vector2(0.5f, 0.5f), confirmPosition, new Vector2(260f, 58f), new Color(0.11f, 0.54f, 0.36f), 24);
            confirm.onClick.AddListener(() =>
            {
                identity.ApplySelection(nameInput.text, selectedGender);
                classController.ApplyClass(selectedClass);

                if (network != null)
                {
                    network.PlayerName = identity.CharacterName;
                }

                hud.RefreshClass();
                hud.SetStatus($"Personaje creado: {identity.DisplayLabel}", 3.5f);
                hud.AddFeed($"Entraste como {identity.DisplayLabel}");
                overlay.SetActive(false);
                Time.timeScale = 1f;
                persistence?.MarkCharacterActive();
            });

            if (savedData != null)
            {
                var continueButton = CreateRoundButton(overlay.transform, "Continue Character Button", "CONTINUAR", new Vector2(0.5f, 0.5f), new Vector2(-150f, -254f), new Vector2(260f, 58f), new Color(0.16f, 0.36f, 0.66f), 22);
                continueButton.onClick.AddListener(() =>
                {
                    persistence.ApplyLoadedData(savedData);
                    hud.RefreshClass();
                    hud.SetStatus($"Bienvenido de nuevo: {identity.DisplayLabel}", 3.5f);
                    hud.AddFeed($"Partida cargada: nivel {savedData.Level}");
                    overlay.SetActive(false);
                    Time.timeScale = 1f;
                });
            }

            identity.ApplySelection(nameInput.text, selectedGender);
            classController.ApplyClass(selectedClass);
            RefreshPreview();
            Time.timeScale = 0f;

            void RefreshPreview()
            {
                var definition = ClassDefinition.Create(selectedClass);
                var name = string.IsNullOrWhiteSpace(nameInput.text) ? "Heroe" : nameInput.text.Trim();
                var gender = selectedGender == CharacterGender.Femenino ? "Femenino" : "Masculino";
                preview.text = $"{name}  |  {definition.DisplayName}  |  {gender}";
                preview.color = Color.Lerp(definition.BodyColor, Color.white, 0.28f);
                hud.RefreshClass();
            }
        }

        private static Image CreateBar(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 position, Color backgroundColor, Color fillColor, out Text label)
        {
            var frame = CreateUiObject(name, parent);
            var frameImage = frame.AddComponent<Image>();
            frameImage.color = backgroundColor;
            SetRect(frame.GetComponent<RectTransform>(), anchor, pivot, size, position);

            var fillObject = CreateUiObject("Fill", frame.transform);
            var fill = fillObject.AddComponent<Image>();
            fill.color = fillColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            SetRect(fill.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), size, Vector2.zero);

            label = CreateText(frame.transform, "Label", string.Empty, 19, TextAnchor.MiddleCenter);
            SetRect(label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, Vector2.zero);
            return fill;
        }

        private static Image CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 position, Color color)
        {
            var panel = CreateUiObject(name, parent);
            var image = panel.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            SetRect(panel.GetComponent<RectTransform>(), anchor, pivot, size, position);
            return image;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static Text CreateText(Transform parent, string name, string value, int fontSize, TextAnchor alignment)
        {
            var textObject = CreateUiObject(name, parent);
            var text = textObject.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
            }
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 position)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
