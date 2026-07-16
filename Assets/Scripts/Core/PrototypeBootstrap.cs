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
            var zones = LoadZones();
            ZoneBalanceResolver.LogReports(zones);
            var commerceZone = zones.Find(zone => zone != null && zone.HasSafeZone);
            var playerCombat = player.GetComponent<PlayerCombat>();
            if (commerceZone != null && playerCombat != null)
            {
                playerCombat.SafeZoneCenter = commerceZone.SafeZoneCenter;
                playerCombat.SafeZoneRadius = commerceZone.SafeZoneRadius;
            }
            var shop = CreateShopNpc(player);
            var blacksmith = CreateBlacksmithNpc(player);
            var storage = CreateStorageNpc(player);
            var hud = CreateHudAndControls(player, shop, blacksmith, storage, zones);
            var telemetry = player.GetComponent<CombatTelemetry>();
            telemetry.Hud = hud;
            telemetry.Network = player.GetComponent<MmorpgNetworkClient>();
            telemetry.Initialize(player.GetComponent<Health>(), zones);
            shop.Hud = hud;
            blacksmith.Hud = hud;
            storage.Hud = hud;
            var dailyEvents = CreateDailyEvents(player, hud);

            foreach (var zone in zones)
            {
                CreateZoneGround(zone);
                CreateZoneSign(zone);
                ZoneEnvironmentBuilder.Build(zone);
                CreateEnemySpawner(player, hud, zone, dailyEvents);
            }

            CreateWorldEvent(player, hud);
        }

        private static System.Collections.Generic.List<ZoneDefinition> LoadZones()
        {
            var loaded = Resources.LoadAll<ZoneDefinition>("Game/Zones");
            if (loaded != null && loaded.Length > 0)
            {
                var zones = new System.Collections.Generic.List<ZoneDefinition>(loaded);
                zones.Sort((a, b) => a.MinLevel.CompareTo(b.MinLevel));
                return zones;
            }

            // Fallback runtime si los assets no se generaron todavia
            // (MMORPG > World > Generate Zones).
            return DefaultZones.CreateAll();
        }

        private static void CreateZoneGround(ZoneDefinition zone)
        {
            if (zone == null || !zone.HasOwnGround)
            {
                return;
            }

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = $"Ground - {zone.DisplayName}";
            ground.transform.position = zone.GroundCenter;
            ground.transform.localScale = new Vector3(7f, 1f, 7f);

            var material = VisualMaterialUtility.Create(zone.GroundColor, false, 0.02f, 0.16f);
            ground.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateZoneSign(ZoneDefinition zone)
        {
            if (zone == null)
            {
                return;
            }

            var sign = new GameObject($"Zone Sign - {zone.DisplayName}");
            sign.transform.position = zone.SignPosition;

            var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = "Post";
            post.transform.SetParent(sign.transform, false);
            post.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            post.transform.localScale = new Vector3(0.25f, 2.2f, 0.25f);

            var material = VisualMaterialUtility.Create(new Color(0.35f, 0.26f, 0.16f), false, 0.04f, 0.2f);
            post.GetComponent<Renderer>().sharedMaterial = material;

            CreateWorldLabel(sign.transform, Localization.Tr("world.zone_sign", zone.DisplayName, zone.MinLevel, zone.MaxLevel), new Color(1f, 0.9f, 0.6f), 2.6f);
        }

        private static void ConfigureRuntime()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            if (Application.platform == RuntimePlatform.Android)
            {
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.shadowDistance = 24f;
                QualitySettings.pixelLightCount = 1;
                QualitySettings.antiAliasing = 0;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                QualitySettings.realtimeReflectionProbes = false;
            }
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
                light.intensity = 1.2f;
                light.color = new Color(1f, 0.92f, 0.8f);
                light.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            }

            RenderSettings.ambientLight = new Color(0.34f, 0.4f, 0.5f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.09f, 0.12f, 0.16f);
            RenderSettings.fogStartDistance = 52f;
            RenderSettings.fogEndDistance = 170f;
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Training Field";
            ground.transform.localScale = new Vector3(7f, 1f, 7f);

            var material = VisualMaterialUtility.Create(new Color(0.22f, 0.34f, 0.24f), false, 0.02f, 0.12f);
            ground.GetComponent<Renderer>().sharedMaterial = material;

            var collision = new GameObject("Training Field Collision");
            collision.transform.position = new Vector3(0f, -0.1f, 0f);
            var collisionBox = collision.AddComponent<BoxCollider>();
            collisionBox.size = new Vector3(70f, 0.2f, 70f);

            for (var i = 0; i < 16; i++)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = "Field Stone";
                marker.transform.position = new Vector3(Random.Range(-28f, 28f), 0.18f, Random.Range(-28f, 28f));
                marker.transform.localScale = new Vector3(Random.Range(0.7f, 1.6f), 0.35f, Random.Range(0.7f, 1.6f));

                var stoneMaterial = VisualMaterialUtility.Create(new Color(0.28f, 0.28f, 0.3f), false, 0.08f, 0.28f);
                marker.GetComponent<Renderer>().sharedMaterial = stoneMaterial;
            }
        }

        private static GameObject CreatePlayer()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player - Guerrero Prototype";
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);

            var material = VisualMaterialUtility.Create(new Color(0.18f, 0.42f, 0.9f), false, 0.12f, 0.32f);
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
            player.AddComponent<PlayerRegeneration>();
            var playerFlash = player.AddComponent<HitFlashOnDamage>();
            playerFlash.FlashColor = new Color(1f, 0.42f, 0.32f);

            player.AddComponent<PlayerController>();

            var combat = player.AddComponent<PlayerCombat>();
            combat.AttackDamage = 26;
            combat.AttackRange = 2.25f;
            combat.AttackCooldown = 0.65f;

            player.AddComponent<PlayerCharacterIdentity>();
            player.AddComponent<PlayerAvatarVisual>();
            player.AddComponent<EquipmentVisualController>();
            player.AddComponent<PlayerStatSheet>();
            player.AddComponent<PlayerClassController>();
            var progression = player.AddComponent<PlayerProgression>();
            progression.Table = LoadLevelTable();
            player.AddComponent<InventorySystem>();
            player.AddComponent<PlayerQuestLog>();
            player.AddComponent<RepeatableContractSystem>();
            player.AddComponent<WeeklyEventSystem>();
            player.AddComponent<SeasonProgressionSystem>();
            player.AddComponent<EquipmentUpgradeSystem>();
            player.AddComponent<PlayerEquipment>();
            player.AddComponent<PetService>();
            player.AddComponent<MountService>();
            player.AddComponent<CosmeticService>();
            player.AddComponent<StorageService>();
            player.AddComponent<PlayerAttributes>();
            player.AddComponent<PlayerSkills>();
            player.AddComponent<PlayerPersistence>();
            player.AddComponent<MmorpgNetworkClient>();
            player.AddComponent<CombatTelemetry>();
            player.AddComponent<CombatFeedbackAudio>();

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
            camera.allowHDR = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.09f, 0.12f, 0.16f);

            var listener = cameraObject.AddComponent<AudioListener>();
            listener.enabled = true;

            var follow = cameraObject.AddComponent<OrbitCamera>();
            follow.Target = target;
        }

        private static void CreateEnemySpawner(GameObject player, PrototypeHud hud, ZoneDefinition zone, DailyEventSystem dailyEvents)
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
            spawner.Telemetry = player.GetComponent<CombatTelemetry>();
            spawner.DailyEvents = dailyEvents;
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

        private static System.Collections.Generic.List<CosmeticDefinition> LoadCosmetics()
        {
            var loaded = Resources.LoadAll<CosmeticDefinition>("Game/Cosmetics");
            if (loaded != null && loaded.Length > 0)
            {
                return new System.Collections.Generic.List<CosmeticDefinition>(loaded);
            }

            return DefaultCosmetics.CreateAll();
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

            var material = VisualMaterialUtility.Create(new Color(0.93f, 0.7f, 0.28f), false, 0.16f, 0.34f);
            npc.GetComponent<Renderer>().sharedMaterial = material;

            var shop = npc.AddComponent<ShopNpc>();
            shop.Player = player.transform;
            shop.Progression = player.GetComponent<PlayerProgression>();
            shop.Inventory = player.GetComponent<InventorySystem>();
            shop.QuestLog = player.GetComponent<PlayerQuestLog>();

            CreateWorldLabel(npc.transform, Localization.Tr("world.shop_label"), new Color(1f, 0.92f, 0.55f), 1.55f);
            return shop;
        }

        private static BlacksmithNpc CreateBlacksmithNpc(GameObject player)
        {
            var npc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            npc.name = "Herrero del Campamento";
            npc.transform.position = new Vector3(3.6f, 1f, -3.4f);
            npc.transform.localScale = new Vector3(0.75f, 1.05f, 0.75f);

            var material = VisualMaterialUtility.Create(new Color(0.5f, 0.5f, 0.56f), false, 0.2f, 0.38f);
            npc.GetComponent<Renderer>().sharedMaterial = material;

            var blacksmith = npc.AddComponent<BlacksmithNpc>();
            blacksmith.Player = player.transform;
            blacksmith.Equipment = player.GetComponent<EquipmentUpgradeSystem>();
            blacksmith.QuestLog = player.GetComponent<PlayerQuestLog>();

            CreateWorldLabel(npc.transform, Localization.Tr("world.smith_label"), new Color(0.8f, 0.85f, 0.95f), 1.55f);
            return blacksmith;
        }

        private static StorageNpc CreateStorageNpc(GameObject player)
        {
            var npc = GameObject.CreatePrimitive(PrimitiveType.Cube);
            npc.name = "Almacen del Campamento";
            npc.transform.position = new Vector3(0.4f, 0.7f, -4.8f);
            npc.transform.localScale = new Vector3(1.2f, 1.4f, 1.2f);

            var material = VisualMaterialUtility.Create(new Color(0.55f, 0.42f, 0.22f), false, 0.08f, 0.24f);
            npc.GetComponent<Renderer>().sharedMaterial = material;

            var storageService = player.GetComponent<StorageService>();
            var storage = npc.AddComponent<StorageNpc>();
            storage.Player = player.transform;
            storage.Storage = storageService;

            CreateWorldLabel(npc.transform, Localization.Tr("world.storage_label"), new Color(0.95f, 0.85f, 0.6f), 1.35f);
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
            worldEvent.Telemetry = player.GetComponent<CombatTelemetry>();
        }

        private static DailyEventSystem CreateDailyEvents(GameObject player, PrototypeHud hud)
        {
            var eventObject = new GameObject("Daily Event System");
            var dailyEvents = eventObject.AddComponent<DailyEventSystem>();
            dailyEvents.Progression = player.GetComponent<PlayerProgression>();
            dailyEvents.Cosmetics = player.GetComponent<CosmeticService>();
            dailyEvents.Persistence = player.GetComponent<PlayerPersistence>();
            dailyEvents.Hud = hud;
            dailyEvents.Initialize();
            dailyEvents.Persistence.DailyEvents = dailyEvents;
            return dailyEvents;
        }

        private static PrototypeHud CreateHudAndControls(GameObject player, ShopNpc shop, BlacksmithNpc blacksmith, StorageNpc storage, System.Collections.Generic.List<ZoneDefinition> zones)
        {
            var canvasObject = new GameObject("Prototype HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = Application.platform == RuntimePlatform.Android
                ? new Vector2(1600f, 900f)
                : new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            var safeAreaRoot = CreateUiObject("Mobile Safe Area Root", canvas.transform);
            StretchToParent(safeAreaRoot.GetComponent<RectTransform>());
            safeAreaRoot.AddComponent<SafeAreaFitter>();
            var uiRoot = safeAreaRoot.transform;
            var mobileDiagnostics = canvasObject.AddComponent<MobileRuntimeDiagnostics>();
            mobileDiagnostics.SafeAreaRoot = safeAreaRoot.GetComponent<RectTransform>();
            mobileDiagnostics.Audio = player.GetComponent<CombatFeedbackAudio>();

            var camera = Camera.main;
            CreateCameraLookSurface(uiRoot, camera != null ? camera.GetComponent<OrbitCamera>() : null);
            CreateMiniMap(uiRoot, player.transform);

            CreatePanel(uiRoot, "Vitals Panel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(820f, 250f), new Vector2(18f, -18f), new Color(0.025f, 0.032f, 0.04f, 0.52f));
            CreatePanel(uiRoot, "Action Buttons Panel", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(640f, 390f), new Vector2(-22f, 22f), new Color(0.025f, 0.032f, 0.04f, 0.42f));
            CreatePanel(uiRoot, "Network Panel", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(500f, 112f), new Vector2(-18f, -82f), new Color(0.025f, 0.032f, 0.04f, 0.46f));
            CreatePanel(uiRoot, "Activity Panel", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(460f, 116f), new Vector2(-24f, -118f), new Color(0.025f, 0.032f, 0.04f, 0.36f));

            var hud = canvasObject.AddComponent<PrototypeHud>();
            hud.PlayerHealthFill = CreateBar(uiRoot, "Player Health", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(360f, 34f), new Vector2(28f, -28f), new Color(0.05f, 0.08f, 0.1f), new Color(0.1f, 0.8f, 0.32f), out var playerText);
            hud.PlayerHealthText = playerText;
            hud.EnemyHealthFill = CreateBar(uiRoot, "Enemy Health", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(360f, 28f), new Vector2(28f, -70f), new Color(0.05f, 0.08f, 0.1f), new Color(0.95f, 0.24f, 0.18f), out var enemyText);
            hud.EnemyHealthText = enemyText;
            hud.StatusText = CreateText(uiRoot, "Status", Localization.Tr("hud.initial_status"), 25, TextAnchor.MiddleLeft);
            SetRect(hud.StatusText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(700f, 32f), new Vector2(28f, -108f));
            hud.ClassText = CreateText(uiRoot, "Class Info", string.Empty, 23, TextAnchor.MiddleLeft);
            SetRect(hud.ClassText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(700f, 32f), new Vector2(28f, -144f));
            hud.ProgressionText = CreateText(uiRoot, "Progression Info", string.Empty, 23, TextAnchor.MiddleLeft);
            SetRect(hud.ProgressionText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(700f, 32f), new Vector2(28f, -180f));
            hud.QuestText = CreateText(uiRoot, "Quest Info", string.Empty, 21, TextAnchor.MiddleLeft);
            SetRect(hud.QuestText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(760f, 32f), new Vector2(28f, -216f));
            hud.InventoryText = CreateText(uiRoot, "Inventory Info", string.Empty, 21, TextAnchor.MiddleLeft);
            SetRect(hud.InventoryText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(900f, 32f), new Vector2(28f, -264f));
            hud.InventoryText.gameObject.SetActive(false);
            hud.EquipmentText = CreateText(uiRoot, "Equipment Info", string.Empty, 21, TextAnchor.MiddleLeft);
            SetRect(hud.EquipmentText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(900f, 32f), new Vector2(28f, -298f));
            hud.EquipmentText.gameObject.SetActive(false);
            hud.FeedText = CreateText(uiRoot, "Activity Feed", string.Empty, 18, TextAnchor.LowerRight);
            SetRect(hud.FeedText.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(460f, 104f), new Vector2(-44f, -116f));

            var movementJoystick = CreateJoystick(uiRoot);
            player.GetComponent<PlayerController>().MovementJoystick = movementJoystick;

            var attackButton = CreateRoundButton(uiRoot, "Attack Button", Localization.Tr("ui.attack"), new Vector2(1f, 0f), new Vector2(-170f, 170f), new Vector2(148f, 148f), new Color(0.88f, 0.28f, 0.16f));
            var attackInput = attackButton.gameObject.AddComponent<MobileActionButton>();
            attackInput.Pressed.AddListener(player.GetComponent<PlayerCombat>().TryAttack);

            var skills = player.GetComponent<PlayerSkills>();
            var skillOneButton = CreateRoundButton(uiRoot, "Skill One Button", "Q", new Vector2(1f, 0f), new Vector2(-500f, 172f), new Vector2(130f, 92f), new Color(0.18f, 0.36f, 0.86f), 17);
            skillOneButton.onClick.AddListener(skills.UseSkillOne);
            hud.SkillOneText = skillOneButton.GetComponentInChildren<Text>();
            CreateSkillUpgradeButton(uiRoot, skills, 0, new Vector2(-500f, 232f), new Color(0.18f, 0.36f, 0.86f));

            var skillTwoButton = CreateRoundButton(uiRoot, "Skill Two Button", "E", new Vector2(1f, 0f), new Vector2(-500f, 286f), new Vector2(130f, 92f), new Color(0.47f, 0.2f, 0.78f), 17);
            skillTwoButton.onClick.AddListener(skills.UseSkillTwo);
            hud.SkillTwoText = skillTwoButton.GetComponentInChildren<Text>();
            CreateSkillUpgradeButton(uiRoot, skills, 1, new Vector2(-500f, 346f), new Color(0.47f, 0.2f, 0.78f));

            var skillThreeButton = CreateRoundButton(uiRoot, "Skill Three Button", "R", new Vector2(1f, 0f), new Vector2(-330f, 286f), new Vector2(130f, 92f), new Color(0.18f, 0.52f, 0.52f), 17);
            skillThreeButton.onClick.AddListener(skills.UseSkillThree);
            hud.SkillThreeText = skillThreeButton.GetComponentInChildren<Text>();
            CreateSkillUpgradeButton(uiRoot, skills, 2, new Vector2(-330f, 346f), new Color(0.18f, 0.52f, 0.52f));

            var skillFourButton = CreateRoundButton(uiRoot, "Skill Four Button", "F", new Vector2(1f, 0f), new Vector2(-330f, 172f), new Vector2(130f, 92f), new Color(0.62f, 0.34f, 0.16f), 17);
            skillFourButton.onClick.AddListener(skills.UseSkillFour);
            hud.SkillFourText = skillFourButton.GetComponentInChildren<Text>();
            CreateSkillUpgradeButton(uiRoot, skills, 3, new Vector2(-330f, 232f), new Color(0.62f, 0.34f, 0.16f));

            var ultimateButton = CreateRoundButton(uiRoot, "Ultimate Skill Button", "G", new Vector2(1f, 0f), new Vector2(-640f, 228f), new Vector2(160f, 104f), new Color(0.78f, 0.16f, 0.14f), 15);
            ultimateButton.onClick.AddListener(skills.UseUltimate);
            hud.UltimateSkillText = ultimateButton.GetComponentInChildren<Text>();
            CreateSkillUpgradeButton(uiRoot, skills, 4, new Vector2(-640f, 228f), new Color(0.78f, 0.16f, 0.14f));

            CreateClassButtons(uiRoot, player);

            var combat = player.GetComponent<PlayerCombat>();
            combat.Hud = hud;
            var regeneration = player.GetComponent<PlayerRegeneration>();
            regeneration.Hud = hud;
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
            var equipmentVisuals = player.GetComponent<EquipmentVisualController>();
            equipmentVisuals.Avatar = player.GetComponent<PlayerAvatarVisual>();
            equipmentVisuals.Equipment = gear;
            equipmentVisuals.Initialize();
            skills.Hud = hud;
            skills.Progression = progression;
            skills.Inventory = inventory;
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
            var cosmetics = player.GetComponent<CosmeticService>();
            cosmetics.Hud = hud;
            cosmetics.Progression = progression;
            cosmetics.UpgradeSystem = equipment;
            cosmetics.Avatar = player.GetComponent<PlayerAvatarVisual>();
            cosmetics.Initialize(LoadCosmetics());
            persistence.Cosmetics = cosmetics;
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
            persistence.Skills = skills;
            var contracts = player.GetComponent<RepeatableContractSystem>();
            contracts.Progression = progression;
            contracts.Inventory = inventory;
            contracts.Hud = hud;
            contracts.Persistence = persistence;
            var weeklyEvent = player.GetComponent<WeeklyEventSystem>();
            weeklyEvent.Progression = progression;
            weeklyEvent.Inventory = inventory;
            weeklyEvent.Persistence = persistence;
            weeklyEvent.Hud = hud;
            var season = player.GetComponent<SeasonProgressionSystem>();
            season.Progression = progression;
            season.Inventory = inventory;
            season.Cosmetics = cosmetics;
            season.Persistence = persistence;
            season.Hud = hud;
            season.Initialize();
            weeklyEvent.Season = season;
            weeklyEvent.Initialize();
            contracts.Season = season;
            contracts.Initialize(zones);
            questLog.Contracts = contracts;
            questLog.WeeklyEvent = weeklyEvent;
            questLog.Season = season;
            persistence.Contracts = contracts;
            persistence.WeeklyEvent = weeklyEvent;
            persistence.Season = season;
            hud.Bind(player.GetComponent<Health>(), player.GetComponent<PlayerClassController>(), player.GetComponent<PlayerCharacterIdentity>(), progression, skills, inventory, questLog, equipment, combat);
            questLog.Initialize(LoadQuestLine());
            inventory.AddItem(DefaultGameItems.MinorPotion, 2);
            inventory.AddItem(DefaultGameItems.RecruitSword);
            CreateShopButtons(uiRoot, player, shop, blacksmith, storage, cosmetics);
            CreateNetworkPanel(uiRoot, player);

            var helpKey = Application.isMobilePlatform ? "hud.touch_help" : "hud.controls_help";
            var help = CreateText(uiRoot, "Controls Help", Localization.Tr(helpKey), 17, TextAnchor.MiddleCenter);
            SetRect(help.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(900f, 28f), new Vector2(0f, -14f));
            CreatePolishedCharacterAccessPanel(uiRoot, player, hud);
            CreateStartupSplash(uiRoot);
            return hud;
        }

        private static void CreateShopButtons(Transform parent, GameObject player, ShopNpc shop, BlacksmithNpc blacksmith, StorageNpc storage, CosmeticService cosmetics)
        {
            var equipment = player.GetComponent<EquipmentUpgradeSystem>();

            var secondaryActions = CreateUiObject("Secondary Actions", parent);
            StretchToParent(secondaryActions.GetComponent<RectTransform>());
            var actionParent = secondaryActions.transform;

            var statsWindow = CreateStatsWindow(parent, player);
            var telemetryWindow = CreateTelemetryWindow(parent, player);
            var menuWindow = CreatePlayerMenuWindow(parent, player, statsWindow, telemetryWindow);
            var menuButton = CreateRoundButton(parent, "Mobile Menu Button", Localization.Tr("ui.menu"), new Vector2(0f, 0f), new Vector2(322f, 348f), new Vector2(128f, 42f), new Color(0.34f, 0.3f, 0.48f), 17);
            menuButton.onClick.AddListener(menuWindow.Toggle);

            var moreButton = CreateRoundButton(parent, "More Actions Button", Localization.Tr("ui.more"), new Vector2(0f, 0f), new Vector2(462f, 348f), new Vector2(128f, 42f), new Color(0.2f, 0.38f, 0.42f), 17);
            moreButton.onClick.AddListener(() => secondaryActions.SetActive(!secondaryActions.activeSelf));

            if (Application.platform == RuntimePlatform.Android)
            {
                secondaryActions.SetActive(false);
            }

            var potionButton = CreateRoundButton(actionParent, "Use Potion Button", Localization.Tr("ui.potion"), new Vector2(0f, 1f), new Vector2(92f, -348f), new Vector2(128f, 42f), new Color(0.15f, 0.52f, 0.36f), 17);
            potionButton.onClick.AddListener(equipment.TryUsePotion);

            var buyButton = CreateRoundButton(actionParent, "Buy Potion Button", Localization.Tr("ui.buy_potion"), new Vector2(0f, 1f), new Vector2(232f, -348f), new Vector2(128f, 42f), new Color(0.47f, 0.34f, 0.11f), 17);
            buyButton.onClick.AddListener(shop.BuyPotion);

            var weaponButton = CreateRoundButton(actionParent, "Upgrade Weapon Button", Localization.Tr("ui.weapon"), new Vector2(0f, 1f), new Vector2(372f, -348f), new Vector2(128f, 42f), new Color(0.58f, 0.2f, 0.15f), 17);
            weaponButton.onClick.AddListener(blacksmith.UpgradeWeapon);

            var armorButton = CreateRoundButton(actionParent, "Upgrade Armor Button", Localization.Tr("ui.armor"), new Vector2(0f, 1f), new Vector2(512f, -348f), new Vector2(128f, 42f), new Color(0.22f, 0.28f, 0.48f), 17);
            armorButton.onClick.AddListener(blacksmith.UpgradeArmor);

            var gear = player.GetComponent<PlayerEquipment>();
            var equipButton = CreateRoundButton(actionParent, "Auto Equip Button", Localization.Tr("ui.equip"), new Vector2(0f, 1f), new Vector2(652f, -348f), new Vector2(128f, 42f), new Color(0.3f, 0.42f, 0.24f), 17);
            equipButton.onClick.AddListener(gear.EquipBestFromInventory);

            var talkButton = CreateRoundButton(actionParent, "Talk Button", Localization.Tr("ui.talk"), new Vector2(0f, 1f), new Vector2(792f, -348f), new Vector2(128f, 42f), new Color(0.5f, 0.38f, 0.16f), 17);
            talkButton.onClick.AddListener(() => TalkToNearest(player.transform, shop, blacksmith));

            var petService = player.GetComponent<PetService>();
            var petButton = CreateRoundButton(actionParent, "Pet Button", Localization.Tr("ui.pet"), new Vector2(0f, 1f), new Vector2(92f, -396f), new Vector2(128f, 42f), new Color(0.62f, 0.4f, 0.14f), 17);
            petButton.onClick.AddListener(petService.ToggleDefault);

            var mountService = player.GetComponent<MountService>();
            var mountButton = CreateRoundButton(actionParent, "Mount Button", Localization.Tr("ui.mount"), new Vector2(0f, 1f), new Vector2(232f, -396f), new Vector2(128f, 42f), new Color(0.32f, 0.36f, 0.2f), 17);
            mountButton.onClick.AddListener(mountService.ToggleMount);

            var storageButton = CreateRoundButton(actionParent, "Storage Button", Localization.Tr("ui.storage"), new Vector2(0f, 1f), new Vector2(372f, -396f), new Vector2(128f, 42f), new Color(0.42f, 0.34f, 0.18f), 17);
            storageButton.onClick.AddListener(storage.ToggleStorage);

            var statsButton = CreateRoundButton(actionParent, "Stats Button", Localization.Tr("ui.stats"), new Vector2(0f, 1f), new Vector2(512f, -396f), new Vector2(128f, 42f), new Color(0.26f, 0.3f, 0.5f), 17);
            statsButton.onClick.AddListener(statsWindow.Toggle);

            var telemetryButton = CreateRoundButton(actionParent, "Telemetry Button", Localization.Tr("ui.telemetry"), new Vector2(0f, 1f), new Vector2(652f, -396f), new Vector2(128f, 42f), new Color(0.18f, 0.42f, 0.46f), 17);
            telemetryButton.onClick.AddListener(telemetryWindow.Toggle);

            var mobileWindow = CreateMobileTestWindow(parent, player);
            var mobileButton = CreateRoundButton(actionParent, "Mobile Test Button", Localization.Tr("ui.mobile_test"), new Vector2(0f, 1f), new Vector2(792f, -396f), new Vector2(128f, 42f), new Color(0.16f, 0.5f, 0.46f), 17);
            mobileButton.onClick.AddListener(mobileWindow.Toggle);

            var exploreButton = CreateRoundButton(actionParent, "Explore POI Button", Localization.Tr("ui.explore"), new Vector2(0f, 1f), new Vector2(932f, -396f), new Vector2(128f, 42f), new Color(0.62f, 0.42f, 0.14f), 17);
            exploreButton.onClick.AddListener(() => ZonePointOfInterest.InteractNearest(player.transform));

            var itemShopButton = CreateRoundButton(actionParent, "Item Shop Button", Localization.Tr("ui.item_shop"), new Vector2(0f, 1f), new Vector2(932f, -348f), new Vector2(128f, 42f), new Color(0.3f, 0.42f, 0.62f), 16);
            itemShopButton.onClick.AddListener(cosmetics.TryBuyFeatured);

            var outfitButton = CreateRoundButton(actionParent, "Outfit Button", Localization.Tr("ui.outfit"), new Vector2(0f, 1f), new Vector2(1072f, -348f), new Vector2(128f, 42f), new Color(0.22f, 0.5f, 0.4f), 16);
            outfitButton.onClick.AddListener(cosmetics.ToggleOutfit);

            var wingsButton = CreateRoundButton(actionParent, "Wings Button", Localization.Tr("ui.wings"), new Vector2(0f, 1f), new Vector2(1212f, -348f), new Vector2(128f, 42f), new Color(0.58f, 0.24f, 0.54f), 16);
            wingsButton.onClick.AddListener(cosmetics.ToggleWings);

            var rebirthButton = CreateRoundButton(actionParent, "Rebirth Button", Localization.Tr("ui.rebirth"), new Vector2(0f, 1f), new Vector2(1072f, -396f), new Vector2(128f, 42f), new Color(0.7f, 0.24f, 0.16f), 16);
            rebirthButton.onClick.AddListener(() => TryRebirth(player));
        }

        private static void TryRebirth(GameObject player)
        {
            var progression = player.GetComponent<PlayerProgression>();
            if (progression == null || !progression.TryRebirth())
            {
                return;
            }

            player.GetComponent<PlayerQuestLog>()?.ResetForRebirth();
            player.GetComponent<PlayerPersistence>()?.SaveNow();
        }

        private static void CreateSkillUpgradeButton(Transform parent, PlayerSkills skills, int slot, Vector2 position, Color color)
        {
            var button = CreateRoundButton(parent, $"Upgrade Skill {slot + 1}", "+", new Vector2(1f, 0f), position + new Vector2(44f, 24f), new Vector2(38f, 30f), color, 20);
            button.onClick.AddListener(() => skills.UpgradeSkill(slot));
        }

        private static MobileTestWindowController CreateMobileTestWindow(Transform parent, GameObject player)
        {
            var window = CreateUiObject("Mobile Test Window", parent);
            var windowRect = window.GetComponent<RectTransform>();
            SetRect(windowRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(700f, 390f), Vector2.zero);
            window.AddComponent<ResponsivePanelScaler>().ReferenceSize = new Vector2(700f, 390f);

            var background = window.AddComponent<Image>();
            background.color = new Color(0.035f, 0.055f, 0.07f, 0.96f);
            background.raycastTarget = true;

            var title = CreateText(window.transform, "Title", Localization.Tr("ui.mobile_test_title"), 30, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(650f, 44f), new Vector2(0f, -18f));

            var controller = window.AddComponent<MobileTestWindowController>();
            controller.Panel = window;
            controller.Diagnostics = parent.GetComponentInParent<MobileRuntimeDiagnostics>();
            controller.Audio = player.GetComponent<CombatFeedbackAudio>();
            controller.BodyText = CreateText(window.transform, "Body", string.Empty, 19, TextAnchor.UpperLeft);
            controller.BodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            controller.BodyText.verticalOverflow = VerticalWrapMode.Overflow;
            controller.BodyText.supportRichText = false;
            SetRect(controller.BodyText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(620f, 112f), new Vector2(0f, -86f));

            var musicButton = CreateRoundButton(window.transform, "Toggle Test Music", Localization.Tr("ui.mobile_music"), new Vector2(0.5f, 0.5f), new Vector2(-212f, 34f), new Vector2(180f, 46f), new Color(0.22f, 0.38f, 0.58f), 17);
            musicButton.onClick.AddListener(controller.ToggleMusic);
            var sfxDown = CreateRoundButton(window.transform, "Sfx Down", Localization.Tr("ui.mobile_sfx_down"), new Vector2(0.5f, 0.5f), new Vector2(-38f, 34f), new Vector2(150f, 46f), new Color(0.45f, 0.28f, 0.2f), 17);
            sfxDown.onClick.AddListener(() => controller.AdjustSfx(-0.05f));
            var sfxUp = CreateRoundButton(window.transform, "Sfx Up", Localization.Tr("ui.mobile_sfx_up"), new Vector2(0.5f, 0.5f), new Vector2(122f, 34f), new Vector2(150f, 46f), new Color(0.2f, 0.5f, 0.34f), 17);
            sfxUp.onClick.AddListener(() => controller.AdjustSfx(0.05f));

            var musicDown = CreateRoundButton(window.transform, "Music Down", Localization.Tr("ui.mobile_music_down"), new Vector2(0.5f, 0.5f), new Vector2(-112f, -30f), new Vector2(150f, 46f), new Color(0.45f, 0.28f, 0.2f), 17);
            musicDown.onClick.AddListener(() => controller.AdjustMusic(-0.02f));
            var musicUp = CreateRoundButton(window.transform, "Music Up", Localization.Tr("ui.mobile_music_up"), new Vector2(0.5f, 0.5f), new Vector2(48f, -30f), new Vector2(150f, 46f), new Color(0.2f, 0.5f, 0.34f), 17);
            musicUp.onClick.AddListener(() => controller.AdjustMusic(0.02f));

            var closeButton = CreateRoundButton(window.transform, "Close Mobile Test", Localization.Tr("ui.close"), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(170f, 42f), new Color(0.32f, 0.32f, 0.36f), 17);
            closeButton.onClick.AddListener(controller.Toggle);

            window.SetActive(false);
            return controller;
        }

        private static StatsWindowController CreateStatsWindow(Transform parent, GameObject player)
        {
            var window = CreateUiObject("Stats Window", parent);
            var windowRect = window.GetComponent<RectTransform>();
            SetRect(windowRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 320f), Vector2.zero);
            window.AddComponent<ResponsivePanelScaler>().ReferenceSize = new Vector2(560f, 320f);

            var background = window.AddComponent<Image>();
            background.color = new Color(0.04f, 0.055f, 0.07f, 0.94f);
            background.raycastTarget = true;

            var title = CreateText(window.transform, "Title", Localization.Tr("ui.stats_title"), 30, TextAnchor.MiddleCenter);
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

            var closeButton = CreateRoundButton(window.transform, "Close Stats", Localization.Tr("ui.close"), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(160f, 44f), new Color(0.32f, 0.32f, 0.36f), 19);
            closeButton.onClick.AddListener(controller.Toggle);

            window.SetActive(false);
            return controller;
        }

        private static TelemetryWindowController CreateTelemetryWindow(Transform parent, GameObject player)
        {
            var window = CreateUiObject("Telemetry Window", parent);
            var windowRect = window.GetComponent<RectTransform>();
            SetRect(windowRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(860f, 560f), Vector2.zero);
            window.AddComponent<ResponsivePanelScaler>().ReferenceSize = new Vector2(860f, 560f);

            var background = window.AddComponent<Image>();
            background.color = new Color(0.035f, 0.055f, 0.07f, 0.96f);
            background.raycastTarget = true;

            var title = CreateText(window.transform, "Title", Localization.Tr("ui.telemetry_title"), 30, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(820f, 44f), new Vector2(0f, -16f));

            var controller = window.AddComponent<TelemetryWindowController>();
            controller.Panel = window;
            controller.Telemetry = player.GetComponent<CombatTelemetry>();
            controller.BodyText = CreateText(window.transform, "Body", string.Empty, 17, TextAnchor.UpperLeft);
            controller.BodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            controller.BodyText.verticalOverflow = VerticalWrapMode.Overflow;
            controller.BodyText.supportRichText = false;
            SetRect(controller.BodyText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(790f, 410f), new Vector2(0f, -70f));

            var refreshButton = CreateRoundButton(window.transform, "Refresh Telemetry", Localization.Tr("ui.telemetry_refresh"), new Vector2(0.5f, 0f), new Vector2(-245f, 42f), new Vector2(190f, 42f), new Color(0.18f, 0.42f, 0.46f), 17);
            refreshButton.onClick.AddListener(controller.Refresh);

            var saveButton = CreateRoundButton(window.transform, "Save Telemetry", Localization.Tr("ui.telemetry_save"), new Vector2(0.5f, 0f), new Vector2(0f, 42f), new Vector2(190f, 42f), new Color(0.28f, 0.38f, 0.2f), 17);
            saveButton.onClick.AddListener(controller.SaveNow);

            var closeButton = CreateRoundButton(window.transform, "Close Telemetry", Localization.Tr("ui.close"), new Vector2(0.5f, 0f), new Vector2(245f, 42f), new Vector2(160f, 42f), new Color(0.32f, 0.32f, 0.36f), 17);
            closeButton.onClick.AddListener(controller.Toggle);

            window.SetActive(false);
            return controller;
        }

        private static PlayerMenuWindowController CreatePlayerMenuWindow(Transform parent, GameObject player, StatsWindowController statsWindow, TelemetryWindowController telemetryWindow)
        {
            var window = CreateUiObject("Player Menu Window", parent);
            var windowRect = window.GetComponent<RectTransform>();
            SetRect(windowRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(860f, 560f), Vector2.zero);
            window.AddComponent<ResponsivePanelScaler>().ReferenceSize = new Vector2(860f, 560f);

            var background = window.AddComponent<Image>();
            background.color = new Color(0.035f, 0.055f, 0.07f, 0.96f);
            background.raycastTarget = true;

            var controller = window.AddComponent<PlayerMenuWindowController>();
            controller.Panel = window;
            controller.Hud = parent.GetComponentInParent<PrototypeHud>();
            controller.Inventory = player.GetComponent<InventorySystem>();
            controller.Equipment = player.GetComponent<PlayerEquipment>();
            controller.QuestLog = player.GetComponent<PlayerQuestLog>();
            controller.StatsWindow = statsWindow;
            controller.TelemetryWindow = telemetryWindow;

            controller.TitleText = CreateText(window.transform, "Title", Localization.Tr("ui.menu_inventory"), 30, TextAnchor.MiddleCenter);
            controller.TitleText.fontStyle = FontStyle.Bold;
            SetRect(controller.TitleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(820f, 44f), new Vector2(0f, -16f));

            var inventoryButton = CreateRoundButton(window.transform, "Menu Inventory Tab", Localization.Tr("ui.menu_inventory"), new Vector2(0.5f, 1f), new Vector2(-270f, -70f), new Vector2(240f, 42f), new Color(0.18f, 0.42f, 0.46f), 17);
            inventoryButton.onClick.AddListener(controller.ShowInventory);
            var equipmentButton = CreateRoundButton(window.transform, "Menu Equipment Tab", Localization.Tr("ui.menu_equipment"), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(240f, 42f), new Color(0.26f, 0.3f, 0.5f), 17);
            equipmentButton.onClick.AddListener(controller.ShowEquipment);
            var questButton = CreateRoundButton(window.transform, "Menu Quest Tab", Localization.Tr("ui.menu_quest"), new Vector2(0.5f, 1f), new Vector2(270f, -70f), new Vector2(240f, 42f), new Color(0.5f, 0.38f, 0.16f), 17);
            questButton.onClick.AddListener(controller.ShowQuest);

            var statsButton = CreateRoundButton(window.transform, "Menu Stats Tab", Localization.Tr("ui.stats"), new Vector2(0.5f, 1f), new Vector2(-140f, -122f), new Vector2(240f, 42f), new Color(0.26f, 0.3f, 0.5f), 17);
            statsButton.onClick.AddListener(controller.ShowStats);
            var telemetryButton = CreateRoundButton(window.transform, "Menu Telemetry Tab", Localization.Tr("ui.telemetry"), new Vector2(0.5f, 1f), new Vector2(140f, -122f), new Vector2(240f, 42f), new Color(0.18f, 0.42f, 0.46f), 17);
            telemetryButton.onClick.AddListener(controller.ShowTelemetry);

            controller.ContentText = CreateText(window.transform, "Content", string.Empty, 18, TextAnchor.UpperLeft);
            controller.ContentText.horizontalOverflow = HorizontalWrapMode.Wrap;
            controller.ContentText.verticalOverflow = VerticalWrapMode.Overflow;
            controller.ContentText.supportRichText = false;
            SetRect(controller.ContentText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(790f, 330f), new Vector2(0f, -168f));

            var closeButton = CreateRoundButton(window.transform, "Close Player Menu", Localization.Tr("ui.close"), new Vector2(0.5f, 0f), new Vector2(0f, 42f), new Vector2(170f, 42f), new Color(0.32f, 0.32f, 0.36f), 17);
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
            network.Progression = player.GetComponent<PlayerProgression>();
            network.Persistence = player.GetComponent<PlayerPersistence>();
            player.GetComponent<PlayerProgression>().Network = network;

            network.NetworkStatusText = CreateText(parent, "Network Status", Localization.Tr("net.status_offline"), 20, TextAnchor.MiddleRight);
            SetRect(network.NetworkStatusText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(520f, 28f), new Vector2(-24f, -96f));

            network.UrlInput = CreateInputField(parent, "Server Url Input", "ws://localhost:7777", Localization.Tr("net.url_placeholder"), new Vector2(1f, 1f), new Vector2(-244f, -138f), new Vector2(360f, 42f));

            var connectButton = CreateRoundButton(parent, "Online Button", Localization.Tr("ui.online"), new Vector2(1f, 1f), new Vector2(-70f, -138f), new Vector2(118f, 42f), new Color(0.08f, 0.48f, 0.34f), 18);
            connectButton.onClick.AddListener(network.Connect);

            var chatBackground = CreateUiObject("Chat Background", parent);
            var chatImage = chatBackground.AddComponent<Image>();
            chatImage.color = new Color(0.04f, 0.05f, 0.06f, 0.58f);
            chatImage.raycastTarget = false;
            SetRect(chatBackground.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(620f, 164f), new Vector2(0f, 88f));

            network.ChatLogText = CreateText(parent, "Chat Log", Localization.Tr("ui.chat_title"), 18, TextAnchor.LowerLeft);
            SetRect(network.ChatLogText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(590f, 94f), new Vector2(-5f, 126f));

            network.ChatInput = CreateInputField(parent, "Chat Input", string.Empty, Localization.Tr("net.chat_placeholder"), new Vector2(0.5f, 0f), new Vector2(-66f, 52f), new Vector2(460f, 42f));

            var sendButton = CreateRoundButton(parent, "Send Chat Button", Localization.Tr("ui.send"), new Vector2(0.5f, 0f), new Vector2(232f, 52f), new Vector2(112f, 42f), new Color(0.18f, 0.32f, 0.62f), 18);
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
                var button = CreateRoundButton(parent, $"{definition.DisplayName} Button", ClassDisplayName(classType), new Vector2(0.5f, 1f), new Vector2(-270f + i * 180f, -38f), new Vector2(160f, 48f), definition.BodyColor, 21);
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

        private static MobileCameraLookSurface CreateCameraLookSurface(Transform parent, OrbitCamera camera)
        {
            var surface = CreateUiObject("Mobile Camera Look Surface", parent);
            var image = surface.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = true;
            StretchToParent(surface.GetComponent<RectTransform>());

            var look = surface.AddComponent<MobileCameraLookSurface>();
            look.Camera = camera;
            return look;
        }

        private static MiniMapController CreateMiniMap(Transform parent, Transform target)
        {
            var panel = CreateUiObject("Mini Map", parent);
            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.025f, 0.055f, 0.07f, 0.84f);
            panelImage.raycastTarget = false;
            SetRect(panel.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(220f, 220f), new Vector2(-142f, -274f));

            var title = CreateText(panel.transform, "Mini Map Title", Localization.Tr("ui.minimap"), 16, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(200f, 26f), new Vector2(0f, -12f));

            var map = CreateUiObject("Mini Map Area", panel.transform);
            var mapImage = map.AddComponent<Image>();
            mapImage.color = new Color(0.06f, 0.12f, 0.14f, 0.9f);
            mapImage.raycastTarget = false;
            SetRect(map.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(184f, 174f), new Vector2(0f, -18f));

            var playerMarker = CreateUiObject("Mini Map Player", map.transform);
            var playerMarkerImage = playerMarker.AddComponent<Image>();
            playerMarkerImage.color = new Color(0.3f, 0.9f, 1f);
            playerMarkerImage.raycastTarget = false;
            SetRect(playerMarker.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(14f, 14f), Vector2.zero);

            var controller = panel.AddComponent<MiniMapController>();
            controller.Target = target;
            controller.MapRect = map.GetComponent<RectTransform>();
            controller.PlayerMarker = playerMarker.GetComponent<RectTransform>();
            return controller;
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

            var topAccent = CreatePanel(splashObject.transform, "Top Accent", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(720f, 6f), new Vector2(0f, 112f), new Color(0.2f, 0.72f, 0.72f, 0.9f));
            topAccent.raycastTarget = false;

            var overline = CreateText(splashObject.transform, "Overline", "M M O R P G  •  A N D R O I D", 17, TextAnchor.MiddleCenter);
            overline.color = new Color(0.45f, 0.82f, 0.82f);
            SetRect(overline.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(720f, 30f), new Vector2(0f, 144f));

            var title = CreateText(splashObject.transform, "Title", Localization.Tr("ui.game_title"), 54, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(880f, 84f), new Vector2(0f, 34f));

            var subtitle = CreateText(splashObject.transform, "Subtitle", Localization.Tr("ui.version_subtitle"), 24, TextAnchor.MiddleCenter);
            subtitle.color = new Color(0.78f, 0.88f, 1f);
            SetRect(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(640f, 42f), new Vector2(0f, -34f));

            var footer = CreateText(splashObject.transform, "Footer", "CARGANDO EL VALLE", 16, TextAnchor.MiddleCenter);
            footer.color = new Color(0.58f, 0.66f, 0.72f);
            SetRect(footer.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(460f, 28f), new Vector2(0f, -112f));

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

            var selectionFrame = CreatePanel(overlay.transform, "Selection Frame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1060f, 660f), Vector2.zero, new Color(0.04f, 0.055f, 0.07f, 0.94f));
            selectionFrame.gameObject.AddComponent<ResponsivePanelScaler>().ReferenceSize = new Vector2(1060f, 660f);

            var title = CreateText(overlay.transform, "Selection Title", Localization.Tr("character.title"), 48, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(820f, 62f), new Vector2(0f, 252f));

            var subtitle = CreateText(overlay.transform, "Selection Subtitle", Localization.Tr("character.subtitle"), 22, TextAnchor.MiddleCenter);
            subtitle.color = new Color(0.78f, 0.86f, 0.94f);
            SetRect(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(880f, 38f), new Vector2(0f, 204f));

            var preview = CreateText(overlay.transform, "Selection Preview", string.Empty, 26, TextAnchor.MiddleCenter);
            preview.fontStyle = FontStyle.Bold;
            preview.color = new Color(1f, 0.9f, 0.55f);
            SetRect(preview.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(820f, 48f), new Vector2(0f, 132f));

            var nameInput = CreateInputField(overlay.transform, "Character Name Input", savedData != null ? savedData.CharacterName : Localization.Tr("identity.default_name"), Localization.Tr("character.name_placeholder"), new Vector2(0.5f, 0.5f), new Vector2(0f, 74f), new Vector2(520f, 52f));
            nameInput.characterLimit = 14;

            var classLabel = CreateText(overlay.transform, "Class Label", Localization.Tr("character.class_label"), 22, TextAnchor.MiddleCenter);
            classLabel.fontStyle = FontStyle.Bold;
            SetRect(classLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(280f, 32f), new Vector2(-292f, 10f));

            var genderLabel = CreateText(overlay.transform, "Gender Label", Localization.Tr("character.gender_label"), 22, TextAnchor.MiddleCenter);
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
                var button = CreateRoundButton(overlay.transform, $"Create {definition.DisplayName} Button", ClassDisplayName(classType), new Vector2(0.5f, 0.5f), new Vector2(-292f, -44f - i * 58f), new Vector2(250f, 44f), definition.BodyColor, 19);
                button.onClick.AddListener(() =>
                {
                    selectedClass = classType;
                    classController.ApplyClass(selectedClass);
                    RefreshPreview();
                });
            }

            var maleButton = CreateRoundButton(overlay.transform, "Select Male Button", Localization.Tr("identity.gender_male"), new Vector2(0.5f, 0.5f), new Vector2(292f, -56f), new Vector2(260f, 52f), new Color(0.18f, 0.34f, 0.72f), 20);
            maleButton.onClick.AddListener(() =>
            {
                selectedGender = CharacterGender.Masculino;
                identity.ApplySelection(nameInput.text, selectedGender);
                classController.ApplyClass(selectedClass);
                RefreshPreview();
            });

            var femaleButton = CreateRoundButton(overlay.transform, "Select Female Button", Localization.Tr("identity.gender_female"), new Vector2(0.5f, 0.5f), new Vector2(292f, -124f), new Vector2(260f, 52f), new Color(0.62f, 0.24f, 0.58f), 20);
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
            var confirm = CreateRoundButton(overlay.transform, "Confirm Character Button", Localization.Tr("character.create_button"), new Vector2(0.5f, 0.5f), confirmPosition, new Vector2(260f, 58f), new Color(0.11f, 0.54f, 0.36f), 24);
            confirm.onClick.AddListener(() =>
            {
                identity.ApplySelection(nameInput.text, selectedGender);
                classController.ApplyClass(selectedClass);

                if (network != null)
                {
                    network.PlayerName = identity.CharacterName;
                }

                hud.RefreshClass();
                hud.SetStatus(Localization.Tr("character.created", identity.DisplayLabel), 3.5f);
                hud.AddFeed(Localization.Tr("character.created_feed", identity.DisplayLabel));
                overlay.SetActive(false);
                Time.timeScale = 1f;
                persistence?.MarkCharacterActive();
            });

            if (savedData != null)
            {
                var continueButton = CreateRoundButton(overlay.transform, "Continue Character Button", Localization.Tr("character.continue_button"), new Vector2(0.5f, 0.5f), new Vector2(-150f, -254f), new Vector2(260f, 58f), new Color(0.16f, 0.36f, 0.66f), 22);
                continueButton.onClick.AddListener(() =>
                {
                    persistence.ApplyLoadedData(savedData);
                    hud.RefreshClass();
                    hud.SetStatus(Localization.Tr("character.welcome_back", identity.DisplayLabel), 3.5f);
                    hud.AddFeed(Localization.Tr("character.loaded_feed", savedData.Level));
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
                var name = string.IsNullOrWhiteSpace(nameInput.text) ? Localization.Tr("identity.default_name") : nameInput.text.Trim();
                var gender = selectedGender == CharacterGender.Femenino ? Localization.Tr("identity.gender_female") : Localization.Tr("identity.gender_male");
                preview.text = Localization.Tr("character.preview", name, ClassDisplayName(selectedClass), gender);
                preview.color = Color.Lerp(definition.BodyColor, Color.white, 0.28f);
                hud.RefreshClass();
            }
        }

        private static void CreatePolishedCharacterAccessPanel(Transform parent, GameObject player, PrototypeHud hud)
        {
            var identity = player.GetComponent<PlayerCharacterIdentity>();
            var classController = player.GetComponent<PlayerClassController>();
            var network = player.GetComponent<MmorpgNetworkClient>();
            var persistence = player.GetComponent<PlayerPersistence>();
            var savedData = persistence != null ? persistence.LoadOrNull() : null;
            InputField accessUrlInput = null;
            var serverProfiles = DefaultServerProfiles.Create();
            var selectedServerIndex = Mathf.Clamp(PlayerPrefs.GetInt("mmorpg.server.profile", 0), 0, serverProfiles.Length - 1);
            if (!serverProfiles[selectedServerIndex].Enabled)
            {
                selectedServerIndex = 0;
            }

            var selectedClass = CharacterClassType.Guerrero;
            var selectedGender = CharacterGender.Masculino;
            if (savedData != null)
            {
                System.Enum.TryParse(savedData.ClassName, out selectedClass);
                System.Enum.TryParse(savedData.GenderName, out selectedGender);
            }

            var overlay = CreateUiObject("Polished Character Access Overlay", parent);
            StretchToParent(overlay.GetComponent<RectTransform>());
            var overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0.008f, 0.012f, 0.018f, 0.95f);
            overlayImage.raycastTarget = true;

            var accent = CreatePanel(overlay.transform, "Access Accent", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1260f, 6f), new Vector2(0f, 334f), new Color(0.2f, 0.72f, 0.72f, 0.95f));
            accent.raycastTarget = false;

            var brand = CreateText(overlay.transform, "Access Brand", Localization.Tr("ui.game_title").ToUpperInvariant(), 26, TextAnchor.MiddleCenter);
            brand.fontStyle = FontStyle.Bold;
            brand.color = new Color(0.9f, 0.94f, 1f);
            SetRect(brand.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(760f, 38f), new Vector2(0f, 298f));

            var accessLabel = CreateText(overlay.transform, "Access Label", "ACCESO LOCAL  •  PROTOTIPO ANDROID", 16, TextAnchor.MiddleCenter);
            accessLabel.color = new Color(0.46f, 0.79f, 0.8f);
            SetRect(accessLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(760f, 26f), new Vector2(0f, 270f));

            var frame = CreatePanel(overlay.transform, "Access Frame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1260f, 660f), new Vector2(0f, -14f), new Color(0.035f, 0.05f, 0.065f, 0.98f));
            frame.gameObject.AddComponent<ResponsivePanelScaler>().ReferenceSize = new Vector2(1260f, 660f);

            var leftPanel = CreatePanel(overlay.transform, "Access Left Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(530f, 560f), new Vector2(-340f, -14f), new Color(0.055f, 0.075f, 0.095f, 0.98f));
            leftPanel.raycastTarget = false;
            var rightPanel = CreatePanel(overlay.transform, "Access Right Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(650f, 560f), new Vector2(260f, -14f), new Color(0.045f, 0.06f, 0.078f, 0.98f));
            rightPanel.raycastTarget = false;

            var savedView = CreateUiObject("Saved Character View", overlay.transform);
            StretchToParent(savedView.GetComponent<RectTransform>());

            var savedTitle = CreateText(savedView.transform, "Saved Title", Localization.Tr("character.select_title"), 32, TextAnchor.MiddleCenter);
            savedTitle.fontStyle = FontStyle.Bold;
            SetRect(savedTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(480f, 48f), new Vector2(-340f, 226f));

            var savedSubtitle = CreateText(savedView.transform, "Saved Subtitle", Localization.Tr("character.select_subtitle"), 18, TextAnchor.MiddleCenter);
            savedSubtitle.color = new Color(0.72f, 0.81f, 0.88f);
            SetRect(savedSubtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(480f, 34f), new Vector2(-340f, 190f));

            var savedCard = CreatePanel(savedView.transform, "Saved Character Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(470f, 250f), new Vector2(-340f, 35f), new Color(0.025f, 0.037f, 0.048f, 1f));
            savedCard.raycastTarget = false;
            var savedAccent = CreatePanel(savedView.transform, "Saved Character Accent", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(470f, 6f), new Vector2(-340f, 156f), new Color(0.22f, 0.72f, 0.72f, 1f));
            savedAccent.raycastTarget = false;

            var savedBadge = CreateText(savedView.transform, "Saved Badge", Localization.Tr("character.saved_label"), 16, TextAnchor.MiddleCenter);
            savedBadge.fontStyle = FontStyle.Bold;
            savedBadge.color = new Color(0.46f, 0.9f, 0.65f);
            SetRect(savedBadge.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(180f, 30f), new Vector2(-472f, 118f));

            var savedCharacterText = CreateText(savedView.transform, "Saved Character Text", string.Empty, 23, TextAnchor.MiddleLeft);
            savedCharacterText.fontStyle = FontStyle.Bold;
            SetRect(savedCharacterText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(350f, 104f), new Vector2(-300f, 50f));

            var savedHint = CreateText(savedView.transform, "Saved Hint", Localization.Tr("character.local_save_note"), 16, TextAnchor.MiddleCenter);
            savedHint.color = new Color(0.6f, 0.69f, 0.75f);
            SetRect(savedHint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(450f, 42f), new Vector2(-340f, -90f));

            var readyTitle = CreateText(savedView.transform, "Ready Title", Localization.Tr("character.ready_title"), 32, TextAnchor.MiddleCenter);
            readyTitle.fontStyle = FontStyle.Bold;
            SetRect(readyTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(580f, 48f), new Vector2(260f, 226f));

            var readySubtitle = CreateText(savedView.transform, "Ready Subtitle", Localization.Tr("character.ready_subtitle"), 19, TextAnchor.MiddleCenter);
            readySubtitle.color = new Color(0.72f, 0.81f, 0.88f);
            SetRect(readySubtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 72f), new Vector2(260f, 164f));

            var savedDetails = CreateText(savedView.transform, "Saved Details", string.Empty, 22, TextAnchor.MiddleCenter);
            savedDetails.color = new Color(1f, 0.88f, 0.55f);
            SetRect(savedDetails.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 82f), new Vector2(260f, 78f));

            var createView = CreateUiObject("Create Character View", overlay.transform);
            StretchToParent(createView.GetComponent<RectTransform>());

            var createTitle = CreateText(createView.transform, "Create Title", Localization.Tr("character.new_title"), 32, TextAnchor.MiddleCenter);
            createTitle.fontStyle = FontStyle.Bold;
            SetRect(createTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(580f, 48f), new Vector2(260f, 226f));

            var createSubtitle = CreateText(createView.transform, "Create Subtitle", Localization.Tr("character.new_subtitle"), 18, TextAnchor.MiddleCenter);
            createSubtitle.color = new Color(0.72f, 0.81f, 0.88f);
            SetRect(createSubtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 52f), new Vector2(260f, 183f));

            var createLeftText = CreateText(createView.transform, "Create Left Text", "TU AVENTURERO\n\nElige una clase y un sexo para definir tu estilo de combate.\n\nTu progreso se guarda localmente durante esta prueba.", 20, TextAnchor.UpperLeft);
            createLeftText.color = new Color(0.72f, 0.81f, 0.88f);
            SetRect(createLeftText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(420f, 240f), new Vector2(-340f, 54f));

            var nameLabel = CreateText(createView.transform, "Name Label", Localization.Tr("character.name_label"), 18, TextAnchor.MiddleLeft);
            nameLabel.color = new Color(0.72f, 0.81f, 0.88f);
            SetRect(nameLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(520f, 28f), new Vector2(260f, 139f));

            var nameInput = CreateInputField(createView.transform, "Character Name Input", savedData != null ? savedData.CharacterName : Localization.Tr("identity.default_name"), Localization.Tr("character.name_placeholder"), new Vector2(0.5f, 0.5f), new Vector2(260f, 98f), new Vector2(520f, 52f));
            nameInput.characterLimit = 14;

            var classLabel = CreateText(createView.transform, "Class Label", Localization.Tr("character.class_label"), 19, TextAnchor.MiddleCenter);
            classLabel.fontStyle = FontStyle.Bold;
            SetRect(classLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(240f, 30f), new Vector2(100f, 49f));

            var genderLabel = CreateText(createView.transform, "Gender Label", Localization.Tr("character.gender_label"), 19, TextAnchor.MiddleCenter);
            genderLabel.fontStyle = FontStyle.Bold;
            SetRect(genderLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(240f, 30f), new Vector2(425f, 49f));

            var preview = CreateText(createView.transform, "Selection Preview", string.Empty, 23, TextAnchor.MiddleCenter);
            preview.fontStyle = FontStyle.Bold;
            preview.color = new Color(1f, 0.9f, 0.55f);
            SetRect(preview.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(580f, 42f), new Vector2(260f, -202f));

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
                var classButton = CreateRoundButton(createView.transform, $"Access {definition.DisplayName} Button", ClassDisplayName(classType), new Vector2(0.5f, 0.5f), new Vector2(100f, -2f - i * 48f), new Vector2(230f, 38f), definition.BodyColor, 17);
                classButton.onClick.AddListener(() =>
                {
                    selectedClass = classType;
                    classController.ApplyClass(selectedClass);
                    RefreshPreview();
                });
            }

            var maleButton = CreateRoundButton(createView.transform, "Access Male Button", Localization.Tr("identity.gender_male"), new Vector2(0.5f, 0.5f), new Vector2(425f, 0f), new Vector2(230f, 44f), new Color(0.18f, 0.34f, 0.72f), 18);
            maleButton.onClick.AddListener(() =>
            {
                selectedGender = CharacterGender.Masculino;
                identity.ApplySelection(nameInput.text, selectedGender);
                classController.ApplyClass(selectedClass);
                RefreshPreview();
            });

            var femaleButton = CreateRoundButton(createView.transform, "Access Female Button", Localization.Tr("identity.gender_female"), new Vector2(0.5f, 0.5f), new Vector2(425f, -58f), new Vector2(230f, 44f), new Color(0.62f, 0.24f, 0.58f), 18);
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

            var confirm = CreateRoundButton(createView.transform, "Confirm Character Button", Localization.Tr("character.create_button"), new Vector2(0.5f, 0.5f), new Vector2(320f, -264f), new Vector2(260f, 54f), new Color(0.11f, 0.54f, 0.36f), 21);
            confirm.onClick.AddListener(() =>
            {
                identity.ApplySelection(nameInput.text, selectedGender);
                classController.ApplyClass(selectedClass);
                if (network != null)
                {
                    network.PlayerName = identity.CharacterName;
                }

                hud.RefreshClass();
                hud.SetStatus(Localization.Tr("character.created", identity.DisplayLabel), 3.5f);
                hud.AddFeed(Localization.Tr("character.created_feed", identity.DisplayLabel));
                overlay.SetActive(false);
                Time.timeScale = 1f;
                persistence?.MarkCharacterActive();
                ConnectToSelectedServer();
            });

            var continueButton = CreateRoundButton(savedView.transform, "Continue Character Button", Localization.Tr("character.enter_button"), new Vector2(0.5f, 0.5f), new Vector2(260f, -170f), new Vector2(360f, 60f), new Color(0.16f, 0.42f, 0.72f), 22);
            continueButton.onClick.AddListener(() =>
            {
                if (savedData == null)
                {
                    return;
                }

                persistence.ApplyLoadedData(savedData);
                hud.RefreshClass();
                hud.SetStatus(Localization.Tr("character.welcome_back", identity.DisplayLabel), 3.5f);
                hud.AddFeed(Localization.Tr("character.loaded_feed", savedData.Level));
                overlay.SetActive(false);
                Time.timeScale = 1f;
                ConnectToSelectedServer();
            });

            var newCharacterButton = CreateRoundButton(savedView.transform, "New Character Button", Localization.Tr("character.new_button"), new Vector2(0.5f, 0.5f), new Vector2(260f, -244f), new Vector2(360f, 48f), new Color(0.25f, 0.3f, 0.38f), 18);
            newCharacterButton.onClick.AddListener(() =>
            {
                selectedClass = CharacterClassType.Guerrero;
                selectedGender = CharacterGender.Masculino;
                nameInput.text = Localization.Tr("identity.default_name");
                identity.ApplySelection(nameInput.text, selectedGender);
                classController.ApplyClass(selectedClass);
                savedView.SetActive(false);
                createView.SetActive(true);
                RefreshPreview();
            });

            var backButton = CreateRoundButton(createView.transform, "Back To Saved Character Button", Localization.Tr("character.back_button"), new Vector2(0.5f, 0.5f), new Vector2(85f, -264f), new Vector2(190f, 54f), new Color(0.25f, 0.3f, 0.38f), 18);
            backButton.onClick.AddListener(() =>
            {
                if (savedData == null)
                {
                    return;
                }

                if (System.Enum.TryParse(savedData.ClassName, out CharacterClassType restoredClass))
                {
                    selectedClass = restoredClass;
                }

                if (System.Enum.TryParse(savedData.GenderName, out CharacterGender restoredGender))
                {
                    selectedGender = restoredGender;
                }

                nameInput.text = savedData.CharacterName;
                identity.ApplySelection(nameInput.text, selectedGender);
                classController.ApplyClass(selectedClass);
                createView.SetActive(false);
                savedView.SetActive(true);
                RefreshPreview();
            });
            backButton.gameObject.SetActive(savedData != null);

            var serverPanel = CreatePanel(overlay.transform, "Server Selector Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(530f, 92f), new Vector2(-340f, -192f), new Color(0.025f, 0.037f, 0.048f, 1f));
            serverPanel.raycastTarget = false;
            var serverTitle = CreateText(overlay.transform, "Server Title", Localization.Tr("server.title"), 16, TextAnchor.MiddleLeft);
            serverTitle.fontStyle = FontStyle.Bold;
            SetRect(serverTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(120f, 24f), new Vector2(-565f, -165f));
            var serverStatus = CreateText(overlay.transform, "Server Status", Localization.Tr("server.active_profile"), 14, TextAnchor.MiddleLeft);
            serverStatus.color = new Color(0.48f, 0.86f, 0.64f);
            SetRect(serverStatus.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(430f, 24f), new Vector2(-390f, -219f));

            var savedServerUrl = PlayerPrefs.GetString("mmorpg.server.url", serverProfiles[selectedServerIndex].Url);
            if (network != null)
            {
                network.ServerUrl = savedServerUrl;
                if (network.UrlInput != null)
                {
                    network.UrlInput.text = savedServerUrl;
                }
            }

            var serverButtons = new Button[serverProfiles.Length];
            var serverImages = new Image[serverProfiles.Length];
            for (var i = 0; i < serverProfiles.Length; i++)
            {
                var index = i;
                var profile = serverProfiles[i];
                var selected = i == selectedServerIndex;
                serverButtons[i] = CreateRoundButton(overlay.transform, $"Server {profile.Id} Button", profile.Label, new Vector2(0.5f, 0.5f), new Vector2(-505f + i * 165f, -188f), new Vector2(152f, 48f), selected ? new Color(0.12f, 0.48f, 0.38f) : new Color(0.16f, 0.19f, 0.23f), 13);
                serverImages[i] = serverButtons[i].GetComponent<Image>();
                serverButtons[i].interactable = profile.Enabled;
                serverButtons[i].onClick.AddListener(() =>
                {
                    if (!profile.Enabled)
                    {
                        return;
                    }

                    selectedServerIndex = index;
                    accessUrlInput.text = profile.Url;
                    serverStatus.text = $"{profile.Id} {profile.DisplayName}";
                    PlayerPrefs.SetInt("mmorpg.server.profile", selectedServerIndex);
                    PlayerPrefs.SetString("mmorpg.server.url", profile.Url);
                    PlayerPrefs.Save();

                    for (var buttonIndex = 0; buttonIndex < serverImages.Length; buttonIndex++)
                    {
                        serverImages[buttonIndex].color = buttonIndex == index
                            ? new Color(0.12f, 0.48f, 0.38f)
                            : new Color(0.16f, 0.19f, 0.23f);
                    }
                });
            }

            var futureServers = CreateText(overlay.transform, "Future Servers", Localization.Tr("server.coming_soon"), 13, TextAnchor.MiddleLeft);
            futureServers.color = new Color(0.55f, 0.62f, 0.69f);
            SetRect(futureServers.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(520f, 24f), new Vector2(-340f, -244f));

            var serverAddressLabel = CreateText(overlay.transform, "Server Address Label", Localization.Tr("server.address_label"), 13, TextAnchor.MiddleLeft);
            serverAddressLabel.color = new Color(0.55f, 0.62f, 0.69f);
            SetRect(serverAddressLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(140f, 24f), new Vector2(-565f, -273f));
            accessUrlInput = CreateInputField(overlay.transform, "Access Server Url Input", savedServerUrl, Localization.Tr("net.url_placeholder"), new Vector2(0.5f, 0.5f), new Vector2(-280f, -286f), new Vector2(500f, 38f));
            accessUrlInput.characterLimit = 160;

            var accessConnectionLabel = CreateText(overlay.transform, "Access Connection Label", Localization.Tr("server.connection_status"), 14, TextAnchor.MiddleLeft);
            accessConnectionLabel.color = new Color(0.55f, 0.62f, 0.69f);
            SetRect(accessConnectionLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(130f, 24f), new Vector2(100f, -273f));
            var accessConnectionStatus = CreateText(overlay.transform, "Access Connection Status", network != null ? network.CurrentStatus : Localization.Tr("net.status_offline"), 14, TextAnchor.MiddleLeft);
            SetRect(accessConnectionStatus.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(380f, 34f), new Vector2(320f, -286f));
            var statusView = overlay.AddComponent<ServerConnectionStatusView>();
            statusView.Client = network;
            statusView.Label = accessConnectionStatus;

            if (savedData != null)
            {
                var savedClassDefinition = ClassDefinition.Create(selectedClass);
                savedCharacterText.text = $"{savedData.CharacterName}\n{ClassDisplayName(selectedClass)}  •  {GenderDisplayName(selectedGender)}";
                savedDetails.text = Localization.Tr("character.level_details", savedData.Level, savedData.Experience, savedData.Gold);
                savedAccent.color = savedClassDefinition.BodyColor;
                savedBadge.color = Color.Lerp(savedClassDefinition.BodyColor, Color.white, 0.35f);
            }
            else
            {
                savedView.SetActive(false);
            }

            createView.SetActive(savedData == null);
            identity.ApplySelection(nameInput.text, selectedGender);
            classController.ApplyClass(selectedClass);
            RefreshPreview();
            Time.timeScale = 0f;

            void ConnectToSelectedServer()
            {
                if (network == null)
                {
                    return;
                }

                var address = accessUrlInput != null ? accessUrlInput.text.Trim() : serverProfiles[selectedServerIndex].Url;
                if (string.IsNullOrWhiteSpace(address))
                {
                    return;
                }

                network.ServerUrl = address;
                if (network.UrlInput != null)
                {
                    network.UrlInput.text = address;
                }

                PlayerPrefs.SetInt("mmorpg.server.profile", selectedServerIndex);
                PlayerPrefs.SetString("mmorpg.server.url", address);
                PlayerPrefs.Save();
                network.Connect();
            }

            void RefreshPreview()
            {
                var definition = ClassDefinition.Create(selectedClass);
                var name = string.IsNullOrWhiteSpace(nameInput.text) ? Localization.Tr("identity.default_name") : nameInput.text.Trim();
                var gender = GenderDisplayName(selectedGender);
                preview.text = Localization.Tr("character.preview", name, ClassDisplayName(selectedClass), gender);
                preview.color = Color.Lerp(definition.BodyColor, Color.white, 0.28f);
                hud.RefreshClass();
            }
        }

        private static string GenderDisplayName(CharacterGender gender)
        {
            return gender == CharacterGender.Femenino
                ? Localization.Tr("identity.gender_female")
                : Localization.Tr("identity.gender_male");
        }

        private static string ClassDisplayName(CharacterClassType type)
        {
            return Localization.Tr($"class.{type.ToString().ToLowerInvariant()}");
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
