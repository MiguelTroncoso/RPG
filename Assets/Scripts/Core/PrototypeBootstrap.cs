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
            if (IsoPrototypeRuntime.Enabled)
            {
                gameObject.AddComponent<IsoPrototypeRuntime>().Build();
                return;
            }

            Localization.Initialize(Resources.Load<LocalizationTable>("Game/LocalizationTable"));
            ConfigureRuntime();
            EnsureEventSystem();
            CreateLighting();
            CreateGround();

            var player = CreatePlayer();
            CreateCamera(player.transform);
            CreateOfflinePresentation(player.transform);
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

            var material = OfflineWorldSurfaceArt.MaterialFor(zone);
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
            UiPerformanceSettings.LoadAndApply();
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.LandscapeRight;
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

        private void CreateOfflinePresentation(Transform player)
        {
            var presentation = gameObject.AddComponent<OfflinePresentationDirector>();
            presentation.Initialize(player, Camera.main);
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Training Field";
            ground.transform.localScale = new Vector3(7f, 1f, 7f);

            // Zone 1 reuses this base plane, so it must receive the same
            // authored surface material as the rest of the world.
            var material = OfflineWorldSurfaceArt.MaterialFor(null);
            ground.GetComponent<Renderer>().sharedMaterial = material;

            var collision = new GameObject("Training Field Collision");
            collision.transform.position = new Vector3(0f, -0.1f, 0f);
            var collisionBox = collision.AddComponent<BoxCollider>();
            collisionBox.size = new Vector3(70f, 0.2f, 70f);

            for (var i = 0; i < 16; i++)
            {
                var stoneMaterial = VisualMaterialUtility.Create(new Color(0.28f, 0.28f, 0.3f), false, 0.08f, 0.28f);
                var stonePosition = new Vector3(Random.Range(-28f, 28f), 0.18f, Random.Range(-28f, 28f));
                var stoneSize = new Vector3(Random.Range(0.7f, 1.6f), Random.Range(0.24f, 0.42f), Random.Range(0.7f, 1.6f));
                var marker = RuntimeArtMeshFactory.CreateEllipsoid(null, "Field Stone", stonePosition, stoneSize, new Color(0.28f, 0.28f, 0.3f), 7, 3, false, 0.08f, 0.28f);
                marker.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), Random.Range(-8f, 8f));
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
            player.AddComponent<PlayerEnergySystem>();
            player.AddComponent<RepeatableContractSystem>();
            player.AddComponent<WeeklyEventSystem>();
            player.AddComponent<SeasonProgressionSystem>();
            player.AddComponent<EventCalendarSystem>();
            player.AddComponent<GuildEventSystem>();
            player.AddComponent<FreeForAllEventSystem>();
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
            var npc = new GameObject("Mercader del Valle");
            npc.name = "Mercader del Valle";
            npc.transform.position = new Vector3(-3.2f, 0f, -2.5f);
            NpcVisualFactory.BuildMerchant(npc.transform);

            var shop = npc.AddComponent<ShopNpc>();
            shop.Player = player.transform;
            shop.Progression = player.GetComponent<PlayerProgression>();
            shop.Inventory = player.GetComponent<InventorySystem>();
            shop.QuestLog = player.GetComponent<PlayerQuestLog>();

            CreateWorldLabel(npc.transform, Localization.Tr("world.shop_label"), new Color(1f, 0.92f, 0.55f), 2.45f);
            return shop;
        }

        private static BlacksmithNpc CreateBlacksmithNpc(GameObject player)
        {
            var npc = new GameObject("Herrero del Campamento");
            npc.name = "Herrero del Campamento";
            npc.transform.position = new Vector3(3.6f, 0f, -3.4f);
            NpcVisualFactory.BuildBlacksmith(npc.transform);

            var blacksmith = npc.AddComponent<BlacksmithNpc>();
            blacksmith.Player = player.transform;
            blacksmith.Equipment = player.GetComponent<EquipmentUpgradeSystem>();
            blacksmith.QuestLog = player.GetComponent<PlayerQuestLog>();

            CreateWorldLabel(npc.transform, Localization.Tr("world.smith_label"), new Color(0.8f, 0.85f, 0.95f), 2.45f);
            return blacksmith;
        }

        private static StorageNpc CreateStorageNpc(GameObject player)
        {
            var npc = new GameObject("Almacen del Campamento");
            npc.name = "Almacen del Campamento";
            npc.transform.position = new Vector3(0.4f, 0f, -4.8f);
            NpcVisualFactory.BuildStorageKeeper(npc.transform);

            var storageService = player.GetComponent<StorageService>();
            var storage = npc.AddComponent<StorageNpc>();
            storage.Player = player.transform;
            storage.Storage = storageService;

            CreateWorldLabel(npc.transform, Localization.Tr("world.storage_label"), new Color(0.95f, 0.85f, 0.6f), 2.45f);
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
            worldEvent.Calendar = player.GetComponent<EventCalendarSystem>();
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
            // Use the same landscape reference in editor and on Android so
            // desktop captures accurately represent the APK layout.
            scaler.referenceResolution = new Vector2(1600f, 900f);
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

            CreatePanel(uiRoot, "Vitals Panel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(466f, 158f), new Vector2(18f, -18f), new Color(0.018f, 0.028f, 0.04f, 0.82f));
            CreatePanel(uiRoot, "Action Buttons Panel", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(430f, 286f), new Vector2(-18f, 18f), new Color(0.018f, 0.028f, 0.04f, 0.26f));
            var targetPanel = CreatePanel(uiRoot, "Target Panel", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(430f, 66f), new Vector2(0f, -18f), new Color(0.018f, 0.028f, 0.04f, 0.76f));
            CreatePanel(uiRoot, "Activity Panel", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(300f, 64f), new Vector2(-18f, -156f), new Color(0.018f, 0.028f, 0.04f, 0.54f));

            var hud = canvasObject.AddComponent<PrototypeHud>();
            hud.TargetPanel = targetPanel.gameObject;
            hud.PlayerHealthFill = CreateBar(uiRoot, "Player Health", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(286f, 26f), new Vector2(24f, -24f), new Color(0.04f, 0.07f, 0.09f), UiThemeConfig.Runtime.Health, out var playerText);
            hud.PlayerHealthText = playerText;
            hud.PlayerEnergyFill = CreateBar(uiRoot, "Player Energy", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(286f, 14f), new Vector2(24f, -56f), new Color(0.04f, 0.07f, 0.09f), UiThemeConfig.Runtime.Energy, out var energyText);
            hud.PlayerEnergyText = energyText;
            hud.PlayerExperienceFill = CreateBar(uiRoot, "Player Experience", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(286f, 8f), new Vector2(24f, -76f), new Color(0.04f, 0.07f, 0.09f), UiThemeConfig.Runtime.AccentGold, out var experienceText);
            experienceText.gameObject.SetActive(false);
            hud.PlayerNameText = CreateText(uiRoot, "Player Name", string.Empty, 19, TextAnchor.MiddleLeft);
            SetRect(hud.PlayerNameText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(132f, 26f), new Vector2(326f, -24f));
            hud.PlayerNameText.fontStyle = FontStyle.Bold;
            hud.PlayerLevelText = CreateText(uiRoot, "Player Level", string.Empty, 16, TextAnchor.MiddleLeft);
            SetRect(hud.PlayerLevelText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(132f, 24f), new Vector2(326f, -52f));
            hud.EnemyHealthFill = CreateBar(uiRoot, "Enemy Health", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(370f, 18f), new Vector2(0f, -43f), new Color(0.04f, 0.07f, 0.09f), UiThemeConfig.Runtime.Danger, out var enemyText);
            hud.EnemyHealthText = enemyText;
            hud.EnemyNameText = CreateText(uiRoot, "Enemy Name", Localization.Tr("hud.no_target"), 18, TextAnchor.MiddleCenter);
            SetRect(hud.EnemyNameText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(370f, 22f), new Vector2(0f, -22f));
            hud.TargetElements = new[]
            {
                targetPanel.gameObject,
                hud.EnemyHealthFill.transform.parent.gameObject,
                hud.EnemyNameText.gameObject
            };
            hud.StatusText = CreateText(uiRoot, "Status", Localization.Tr("hud.initial_status"), 18, TextAnchor.MiddleLeft);
            SetRect(hud.StatusText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(430f, 22f), new Vector2(20f, -98f));
            hud.ClassText = CreateText(uiRoot, "Class Info", string.Empty, 18, TextAnchor.MiddleLeft);
            SetRect(hud.ClassText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(142f, 22f), new Vector2(326f, -78f));
            hud.ProgressionText = CreateText(uiRoot, "Progression Info", string.Empty, 17, TextAnchor.MiddleLeft);
            SetRect(hud.ProgressionText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(286f, 22f), new Vector2(24f, -120f));
            hud.QuestText = CreateText(uiRoot, "Quest Info", string.Empty, 16, TextAnchor.MiddleLeft);
            SetRect(hud.QuestText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(430f, 22f), new Vector2(20f, -142f));
            hud.InventoryText = CreateText(uiRoot, "Inventory Info", string.Empty, 21, TextAnchor.MiddleLeft);
            SetRect(hud.InventoryText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(560f, 24f), new Vector2(28f, -178f));
            hud.InventoryText.gameObject.SetActive(false);
            hud.EquipmentText = CreateText(uiRoot, "Equipment Info", string.Empty, 21, TextAnchor.MiddleLeft);
            SetRect(hud.EquipmentText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(560f, 24f), new Vector2(28f, -202f));
            hud.EquipmentText.gameObject.SetActive(false);
            hud.FeedText = CreateText(uiRoot, "Activity Feed", string.Empty, 18, TextAnchor.LowerRight);
            SetRect(hud.FeedText.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(270f, 52f), new Vector2(-28f, -156f));

            var movementJoystick = CreateJoystick(uiRoot);
            player.GetComponent<PlayerController>().MovementJoystick = movementJoystick;

            var attackButton = CreateRoundButton(uiRoot, "Attack Button", "ATK", new Vector2(1f, 0f), new Vector2(-112f, 110f), new Vector2(116f, 116f), new Color(0.78f, 0.18f, 0.14f));
            var attackInput = attackButton.gameObject.AddComponent<MobileActionButton>();
            attackInput.Pressed.AddListener(player.GetComponent<PlayerCombat>().TryAttack);

            var skills = player.GetComponent<PlayerSkills>();
            var skillOneButton = CreateRoundButton(uiRoot, "Skill One Button", "Q", new Vector2(1f, 0f), new Vector2(-258f, 88f), new Vector2(68f, 68f), new Color(0.16f, 0.34f, 0.78f), 16);
            skillOneButton.onClick.AddListener(skills.UseSkillOne);
            hud.SkillOneText = skillOneButton.GetComponentInChildren<Text>();
            var skillOneCooldown = AddCooldownOverlay(skillOneButton.transform, new Color(0.02f, 0.03f, 0.05f, 0.72f));

            var skillTwoButton = CreateRoundButton(uiRoot, "Skill Two Button", "E", new Vector2(1f, 0f), new Vector2(-184f, 174f), new Vector2(68f, 68f), new Color(0.46f, 0.2f, 0.76f), 16);
            skillTwoButton.onClick.AddListener(skills.UseSkillTwo);
            hud.SkillTwoText = skillTwoButton.GetComponentInChildren<Text>();
            var skillTwoCooldown = AddCooldownOverlay(skillTwoButton.transform, new Color(0.02f, 0.03f, 0.05f, 0.72f));

            var skillThreeButton = CreateRoundButton(uiRoot, "Skill Three Button", "R", new Vector2(1f, 0f), new Vector2(-98f, 190f), new Vector2(68f, 68f), new Color(0.12f, 0.52f, 0.58f), 16);
            skillThreeButton.onClick.AddListener(skills.UseSkillThree);
            hud.SkillThreeText = skillThreeButton.GetComponentInChildren<Text>();
            var skillThreeCooldown = AddCooldownOverlay(skillThreeButton.transform, new Color(0.02f, 0.03f, 0.05f, 0.72f));

            var skillFourButton = CreateRoundButton(uiRoot, "Skill Four Button", "F", new Vector2(1f, 0f), new Vector2(-36f, 126f), new Vector2(68f, 68f), new Color(0.66f, 0.34f, 0.16f), 16);
            skillFourButton.onClick.AddListener(skills.UseSkillFour);
            hud.SkillFourText = skillFourButton.GetComponentInChildren<Text>();
            var skillFourCooldown = AddCooldownOverlay(skillFourButton.transform, new Color(0.02f, 0.03f, 0.05f, 0.72f));

            var ultimateButton = CreateRoundButton(uiRoot, "Ultimate Skill Button", "G", new Vector2(1f, 0f), new Vector2(-304f, 186f), new Vector2(78f, 78f), new Color(0.78f, 0.14f, 0.16f), 15);
            ultimateButton.onClick.AddListener(skills.UseUltimate);
            hud.UltimateSkillText = ultimateButton.GetComponentInChildren<Text>();
            var ultimateCooldown = AddCooldownOverlay(ultimateButton.transform, new Color(0.03f, 0.01f, 0.05f, 0.78f));
            hud.SkillCooldowns = new[] { skillOneCooldown, skillTwoCooldown, skillThreeCooldown, skillFourCooldown, ultimateCooldown };
            skills.Energy = player.GetComponent<PlayerEnergySystem>();

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
            var eventCalendar = player.GetComponent<EventCalendarSystem>();
            eventCalendar.Progression = progression;
            eventCalendar.Inventory = inventory;
            eventCalendar.Persistence = persistence;
            eventCalendar.Hud = hud;
            var guildEvent = player.GetComponent<GuildEventSystem>();
            guildEvent.Persistence = persistence;
            guildEvent.Hud = hud;
            var freeForAllEvent = player.GetComponent<FreeForAllEventSystem>();
            freeForAllEvent.Persistence = persistence;
            freeForAllEvent.Hud = hud;
            eventCalendar.GuildEvents = guildEvent;
            eventCalendar.Initialize();
            guildEvent.Initialize();
            freeForAllEvent.Initialize();
            weeklyEvent.Season = season;
            weeklyEvent.Initialize();
            contracts.Season = season;
            contracts.Initialize(zones);
            questLog.Contracts = contracts;
            questLog.WeeklyEvent = weeklyEvent;
            questLog.Season = season;
            questLog.EventCalendar = eventCalendar;
            questLog.GuildEvent = guildEvent;
            questLog.FreeForAllEvent = freeForAllEvent;
            persistence.Contracts = contracts;
            persistence.WeeklyEvent = weeklyEvent;
            persistence.Season = season;
            persistence.EventCalendar = eventCalendar;
            persistence.GuildEvent = guildEvent;
            persistence.FreeForAllEvent = freeForAllEvent;
            hud.Bind(player.GetComponent<Health>(), player.GetComponent<PlayerClassController>(), player.GetComponent<PlayerCharacterIdentity>(), progression, skills, inventory, questLog, equipment, combat);
            questLog.Initialize(LoadQuestLine());
            inventory.AddItem(DefaultGameItems.MinorPotion, 2);
            inventory.AddItem(DefaultGameItems.RecruitSword);
            CreateShopButtons(uiRoot, player, shop, blacksmith, storage, cosmetics, zones);
            CreateNetworkPanel(uiRoot, player);

            var helpKey = Application.isMobilePlatform ? "hud.touch_help" : "hud.controls_help";
            var help = CreateText(uiRoot, "Controls Help", Localization.Tr(helpKey), 17, TextAnchor.MiddleCenter);
            SetRect(help.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(900f, 28f), new Vector2(0f, -14f));
            CreatePolishedCharacterAccessPanel(uiRoot, player, hud);
            CreateStartupSplash(uiRoot);
            return hud;
        }

        private static void CreateShopButtons(Transform parent, GameObject player, ShopNpc shop, BlacksmithNpc blacksmith, StorageNpc storage, CosmeticService cosmetics, System.Collections.Generic.List<ZoneDefinition> zones)
        {
            var equipment = player.GetComponent<EquipmentUpgradeSystem>();
            var skills = player.GetComponent<PlayerSkills>();

            var secondaryActions = CreateUiObject("Secondary Actions", parent);
            StretchToParent(secondaryActions.GetComponent<RectTransform>());
            var actionParent = secondaryActions.transform;

            var statsWindow = CreateStatsWindow(parent, player);
            var telemetryWindow = CreateTelemetryWindow(parent, player);
            var menuWindow = CreatePlayerMenuWindow(parent, player, statsWindow, telemetryWindow);
            var skillsWindow = CreateSkillsWindow(parent, player, skills);
            var graphicsWindow = CreateGraphicsWindow(parent);
            var zoneAtlasWindow = CreateZoneAtlasWindow(parent, player, zones);
            var menuButton = CreateRoundButton(parent, "Mobile Menu Button", Localization.Tr("ui.menu"), new Vector2(0f, 0f), new Vector2(322f, 348f), new Vector2(128f, 42f), new Color(0.34f, 0.3f, 0.48f), 17);
            menuButton.onClick.AddListener(menuWindow.Toggle);

            var moreButton = CreateRoundButton(parent, "More Actions Button", Localization.Tr("ui.more"), new Vector2(0f, 0f), new Vector2(462f, 348f), new Vector2(128f, 42f), new Color(0.2f, 0.38f, 0.42f), 17);
            moreButton.onClick.AddListener(() => secondaryActions.SetActive(!secondaryActions.activeSelf));

            // Secondary actions are a drawer in both editor and Android. This
            // keeps the test view faithful to the mobile beta HUD.
            secondaryActions.SetActive(false);

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

            var skillsButton = CreateRoundButton(actionParent, "Skills Button", Localization.Tr("ui.skills"), new Vector2(0f, 1f), new Vector2(792f, -444f), new Vector2(128f, 42f), new Color(0.4f, 0.22f, 0.62f), 15);
            skillsButton.onClick.AddListener(skillsWindow.Toggle);

            var graphicsButton = CreateRoundButton(actionParent, "Graphics Button", Localization.Tr("ui.graphics"), new Vector2(0f, 1f), new Vector2(932f, -444f), new Vector2(128f, 42f), new Color(0.18f, 0.42f, 0.52f), 15);
            graphicsButton.onClick.AddListener(graphicsWindow.Toggle);

            var worldMapButton = CreateRoundButton(actionParent, "World Atlas Button", "MAPA", new Vector2(0f, 1f), new Vector2(1072f, -444f), new Vector2(128f, 42f), new Color(0.2f, 0.48f, 0.52f), 16);
            worldMapButton.onClick.AddListener(zoneAtlasWindow.Toggle);

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

        private static SkillsWindowController CreateSkillsWindow(Transform parent, GameObject player, PlayerSkills skills)
        {
            var window = CreateUiObject("Skills Window", parent);
            SetRect(window.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(820f, 520f), Vector2.zero);
            window.AddComponent<ResponsivePanelScaler>().ReferenceSize = new Vector2(820f, 520f);
            var background = window.AddComponent<Image>();
            UiThemeConfig.Runtime.StylePanel(background, true);
            background.color = new Color(0.018f, 0.028f, 0.04f, 0.98f);
            background.raycastTarget = true;

            var controller = window.AddComponent<SkillsWindowController>();
            controller.Panel = window;
            controller.Skills = skills;
            controller.TitleText = CreateText(window.transform, "Title", Localization.Tr("ui.skills"), 30, TextAnchor.MiddleCenter);
            controller.TitleText.fontStyle = FontStyle.Bold;
            SetRect(controller.TitleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(760f, 44f), new Vector2(0f, -28f));
            controller.BodyText = CreateText(window.transform, "Body", string.Empty, 18, TextAnchor.UpperLeft);
            controller.BodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            controller.BodyText.verticalOverflow = VerticalWrapMode.Overflow;
            SetRect(controller.BodyText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(740f, 220f), new Vector2(0f, -84f));
            controller.BodyText.gameObject.SetActive(false);

            var colors = new[]
            {
                new Color(0.16f, 0.34f, 0.78f),
                new Color(0.46f, 0.2f, 0.76f),
                new Color(0.12f, 0.52f, 0.58f),
                new Color(0.66f, 0.34f, 0.16f),
                new Color(0.78f, 0.14f, 0.16f)
            };
            for (var i = 0; i < PlayerSkills.SkillSlotCount; i++)
            {
                var slot = i;
                var node = CreateUiObject($"Skill Node {i + 1}", window.transform);
                var nodeImage = node.AddComponent<Image>();
                UiThemeConfig.Runtime.StyleCard(nodeImage, colors[i]);
                SetRect(node.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(136f, 112f), new Vector2(-272f + i * 136f, -138f));
                var nodeText = CreateText(node.transform, "Node Text", string.Empty, 13, TextAnchor.MiddleCenter);
                SetRect(nodeText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(124f, 98f), Vector2.zero);
                nodeText.fontStyle = FontStyle.Bold;
                controller.RegisterNode(nodeText, nodeImage);

                if (i < PlayerSkills.SkillSlotCount - 1)
                {
                    var connector = CreateUiObject($"Skill Connector {i + 1}", window.transform);
                    var connectorImage = connector.AddComponent<Image>();
                    connectorImage.color = new Color(0.45f, 0.62f, 0.68f, 0.62f);
                    connectorImage.raycastTarget = false;
                    SetRect(connector.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(28f, 3f), new Vector2(-204f + i * 136f, -138f));
                }

                var button = CreateRoundButton(window.transform, $"Upgrade Skill Window {i + 1}", $"+ {KeyForSkillSlot(i)}", new Vector2(0.5f, 0.5f), new Vector2(-272f + i * 136f, -218f), new Vector2(112f, 34f), colors[i], 13);
                button.onClick.AddListener(() => controller.Upgrade(slot));
            }

            var closeButton = CreateRoundButton(window.transform, "Close Skills", Localization.Tr("ui.close"), new Vector2(0.5f, 0f), new Vector2(0f, 42f), new Vector2(170f, 44f), new Color(0.22f, 0.28f, 0.34f), 17);
            closeButton.onClick.AddListener(controller.Toggle);
            window.SetActive(false);
            return controller;
        }

        private static string KeyForSkillSlot(int slot)
        {
            switch (slot)
            {
                case 0: return "Q";
                case 1: return "E";
                case 2: return "R";
                case 3: return "F";
                default: return "G";
            }
        }

        private static OfflineZoneTravelController CreateZoneAtlasWindow(Transform parent, GameObject player, System.Collections.Generic.List<ZoneDefinition> zones)
        {
            var window = CreateUiObject("Offline World Atlas", parent);
            SetRect(window.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1400f, 780f), Vector2.zero);
            window.AddComponent<ResponsivePanelScaler>().ReferenceSize = new Vector2(1400f, 780f);

            var background = window.AddComponent<Image>();
            var atlasTexture = Resources.Load<Texture2D>("Art/Generated/valle-reliquias-world-atlas-v1");
            if (atlasTexture != null)
            {
                background.sprite = Sprite.Create(atlasTexture, new Rect(0f, 0f, atlasTexture.width, atlasTexture.height), new Vector2(0.5f, 0.5f));
                background.type = Image.Type.Simple;
                background.preserveAspect = false;
                background.color = Color.white;
            }
            else
            {
                background.color = new Color(0.025f, 0.04f, 0.052f, 0.98f);
            }
            background.raycastTarget = true;

            var shade = CreatePanel(window.transform, "Atlas Cinematic Shade", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.006f, 0.012f, 0.018f, 0.48f));
            StretchToParent(shade.rectTransform);
            shade.raycastTarget = false;

            var titlePanel = CreatePanel(window.transform, "Atlas Title Panel", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(1050f, 62f), new Vector2(0f, -18f), new Color(0.01f, 0.022f, 0.03f, 0.88f));
            titlePanel.raycastTarget = false;
            var controller = window.AddComponent<OfflineZoneTravelController>();
            controller.Panel = window;
            controller.Player = player.transform;
            controller.Progression = player.GetComponent<PlayerProgression>();
            controller.Combat = player.GetComponent<PlayerCombat>();
            controller.Hud = parent.GetComponentInParent<PrototypeHud>();
            controller.TitleText = CreateText(window.transform, "Atlas Title", "Atlas del Valle", 28, TextAnchor.MiddleCenter);
            controller.TitleText.fontStyle = FontStyle.Bold;
            SetRect(controller.TitleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(1010f, 44f), new Vector2(0f, -28f));

            var detailPanel = CreatePanel(window.transform, "Atlas Detail Panel", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1120f, 132f), new Vector2(0f, 118f), new Color(0.01f, 0.024f, 0.032f, 0.9f));
            detailPanel.raycastTarget = false;
            controller.DetailsText = CreateText(window.transform, "Atlas Details", string.Empty, 17, TextAnchor.UpperLeft);
            controller.DetailsText.color = new Color(0.82f, 0.9f, 0.94f);
            SetRect(controller.DetailsText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1060f, 94f), new Vector2(0f, 126f));

            controller.ModeText = CreateText(window.transform, "Atlas Mode", string.Empty, 15, TextAnchor.MiddleCenter);
            controller.ModeText.color = new Color(0.52f, 0.9f, 0.78f);
            SetRect(controller.ModeText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(900f, 28f), new Vector2(0f, 48f));

            var reviewButton = CreateRoundButton(window.transform, "Atlas Review Mode", "MODO PRUEBA", new Vector2(0f, 0f), new Vector2(128f, 34f), new Vector2(220f, 48f), new Color(0.26f, 0.4f, 0.56f), 15);
            reviewButton.onClick.AddListener(controller.ToggleReviewMode);
            var travelButton = CreateRoundButton(window.transform, "Atlas Travel", "VIAJAR", new Vector2(1f, 0f), new Vector2(-128f, 34f), new Vector2(220f, 48f), new Color(0.18f, 0.56f, 0.36f), 18);
            controller.TravelButton = travelButton;
            controller.TravelLabel = travelButton.GetComponentInChildren<Text>();
            travelButton.onClick.AddListener(controller.TravelToSelected);
            var closeButton = CreateRoundButton(window.transform, "Close World Atlas", Localization.Tr("ui.close"), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(170f, 46f), new Color(0.28f, 0.3f, 0.36f), 16);
            closeButton.onClick.AddListener(controller.Toggle);

            var zoneCount = zones != null ? zones.Count : 0;
            controller.Zones = new ZoneDefinition[zoneCount];
            if (zones != null)
            {
                zones.CopyTo(controller.Zones);
            }

            controller.ZoneButtons = new Button[zoneCount];
            controller.ZoneImages = new Image[zoneCount];
            controller.ZoneLabels = new Text[zoneCount];
            for (var i = 0; i < zoneCount; i++)
            {
                var index = i;
                var column = i % 5;
                var row = i / 5;
                var position = new Vector2(-480f + column * 240f, 176f - row * 92f);
                var zone = zones[i];
                var zoneButton = CreateRoundButton(window.transform, $"Atlas Zone {i + 1}", string.Empty, new Vector2(0.5f, 0.5f), position, new Vector2(220f, 72f), zone != null ? zone.GroundColor : new Color(0.16f, 0.2f, 0.24f), 14);
                controller.ZoneButtons[i] = zoneButton;
                controller.ZoneImages[i] = zoneButton.GetComponent<Image>();
                controller.ZoneLabels[i] = zoneButton.GetComponentInChildren<Text>();
                zoneButton.onClick.AddListener(() => controller.SelectZone(index));
            }

            window.SetActive(false);
            return controller;
        }

        private static GraphicsWindowController CreateGraphicsWindow(Transform parent)
        {
            var window = CreateUiObject("Graphics Window", parent);
            SetRect(window.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(720f, 360f), Vector2.zero);
            window.AddComponent<ResponsivePanelScaler>().ReferenceSize = new Vector2(720f, 360f);
            var background = window.AddComponent<Image>();
            UiThemeConfig.Runtime.StylePanel(background, true);
            background.color = new Color(0.018f, 0.028f, 0.04f, 0.98f);
            background.raycastTarget = true;

            var controller = window.AddComponent<GraphicsWindowController>();
            controller.Panel = window;
            controller.ProfileText = CreateText(window.transform, "Profile", string.Empty, 20, TextAnchor.MiddleCenter);
            SetRect(controller.ProfileText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(620f, 40f), new Vector2(0f, -82f));
            controller.TitleText = CreateText(window.transform, "Title", Localization.Tr("ui.graphics"), 30, TextAnchor.MiddleCenter);
            controller.TitleText.fontStyle = FontStyle.Bold;
            SetRect(controller.TitleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(650f, 44f), new Vector2(0f, -28f));

            var qualityButton = CreateRoundButton(window.transform, "Quality Profile", Localization.Tr("ui.quality"), new Vector2(0.5f, 0.5f), new Vector2(-130f, 20f), new Vector2(210f, 54f), new Color(0.64f, 0.42f, 0.16f), 18);
            qualityButton.onClick.AddListener(controller.UseQuality);
            var performanceButton = CreateRoundButton(window.transform, "Performance Profile", Localization.Tr("ui.performance"), new Vector2(0.5f, 0.5f), new Vector2(130f, 20f), new Vector2(210f, 54f), new Color(0.16f, 0.46f, 0.5f), 18);
            performanceButton.onClick.AddListener(controller.UsePerformance);
            var closeButton = CreateRoundButton(window.transform, "Close Graphics", Localization.Tr("ui.close"), new Vector2(0.5f, 0f), new Vector2(0f, 42f), new Vector2(170f, 44f), new Color(0.22f, 0.28f, 0.34f), 17);
            closeButton.onClick.AddListener(controller.Toggle);
            window.SetActive(false);
            return controller;
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
            UiThemeConfig.Runtime.StylePanel(background, true);
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

            var exportButton = CreateRoundButton(window.transform, "Export Android QA", Localization.Tr("ui.mobile_export"), new Vector2(0.5f, 0.5f), new Vector2(0f, -88f), new Vector2(240f, 46f), new Color(0.58f, 0.36f, 0.14f), 17);
            exportButton.onClick.AddListener(controller.ExportReport);

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
            UiThemeConfig.Runtime.StylePanel(background, true);
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
            UiThemeConfig.Runtime.StylePanel(background, true);
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
            UiThemeConfig.Runtime.StylePanel(background, true);
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
            controller.ContentText.gameObject.SetActive(false);

            var visualContentObject = CreateUiObject("Menu Visual Content", window.transform);
            SetRect(visualContentObject.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(790f, 330f), new Vector2(0f, -168f));
            var visualContent = visualContentObject.AddComponent<PlayerMenuVisualContent>();
            visualContent.Root = visualContentObject.GetComponent<RectTransform>();
            visualContent.Menu = controller;
            visualContent.Inventory = controller.Inventory;
            visualContent.Equipment = controller.Equipment;
            visualContent.QuestLog = controller.QuestLog;
            visualContent.Database = player.GetComponent<InventorySystem>().Database;
            controller.VisualContent = visualContent;

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

            var serverButton = CreateRoundButton(parent, "Server Status Button", Localization.Tr("net.server_name"), new Vector2(1f, 1f), new Vector2(-122f, -46f), new Vector2(290f, 42f), new Color(0.035f, 0.12f, 0.14f, 0.94f), 14);
            var serverText = serverButton.GetComponentInChildren<Text>();
            var serverIndicatorObject = serverButton.gameObject.AddComponent<ConnectionStatusIndicator>();
            var serverDotObject = CreateUiObject("Server Status Dot", serverButton.transform);
            var serverDot = serverDotObject.AddComponent<Image>();
            serverDot.sprite = UiThemeConfig.Runtime.CircleSprite;
            serverDot.preserveAspect = true;
            serverDot.raycastTarget = false;
            SetRect(serverDotObject.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(14f, 14f), new Vector2(16f, 0f));
            serverIndicatorObject.Client = network;
            serverIndicatorObject.Dot = serverDot;
            serverIndicatorObject.Label = serverText;
            serverIndicatorObject.ServerLabel = "S-01";
            serverIndicatorObject.RefreshNow();

            var serverWindow = CreateServerSettingsWindow(parent, network);
            serverButton.onClick.AddListener(serverWindow.Toggle);

            var chatRoot = CreateUiObject("Compact Chat Panel", parent);
            SetRect(chatRoot.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(700f, 190f), new Vector2(0f, 116f));
            var chatBackground = CreateUiObject("Chat Background", chatRoot.transform);
            var chatImage = chatBackground.AddComponent<Image>();
            UiThemeConfig.Runtime.StylePanel(chatImage);
            chatImage.color = new Color(0.025f, 0.05f, 0.06f, 0.86f);
            chatImage.raycastTarget = false;
            SetRect(chatBackground.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(660f, 164f), new Vector2(0f, 0f));

            network.ChatLogText = CreateText(chatRoot.transform, "Chat Log", Localization.Tr("ui.chat_title"), 17, TextAnchor.LowerLeft);
            SetRect(network.ChatLogText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(620f, 88f), new Vector2(0f, 30f));

            network.ChatInput = CreateInputField(chatRoot.transform, "Chat Input", string.Empty, Localization.Tr("net.chat_placeholder"), new Vector2(0.5f, 0.5f), new Vector2(-54f, -52f), new Vector2(460f, 38f));

            var sendButton = CreateRoundButton(chatRoot.transform, "Send Chat Button", Localization.Tr("ui.send"), new Vector2(0.5f, 0.5f), new Vector2(252f, -52f), new Vector2(106f, 38f), new Color(0.18f, 0.32f, 0.62f), 16);
            sendButton.onClick.AddListener(network.SendChatFromInput);

            var chatToggle = CreateRoundButton(parent, "Chat Toggle Button", Localization.Tr("ui.chat_collapse"), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(118f, 34f), new Color(0.12f, 0.34f, 0.38f), 14);
            var chatOpen = false;
            chatRoot.SetActive(chatOpen);
            chatToggle.onClick.AddListener(() =>
            {
                chatOpen = !chatOpen;
                chatRoot.SetActive(chatOpen);
                var toggleLabel = chatToggle.GetComponentInChildren<Text>();
                if (toggleLabel != null)
                {
                    toggleLabel.text = Localization.Tr(chatOpen ? "ui.chat_collapse" : "ui.chat_expand");
                }
            });
        }

        private static ServerSettingsWindowController CreateServerSettingsWindow(Transform parent, MmorpgNetworkClient network)
        {
            var window = CreateUiObject("Server Settings Window", parent);
            SetRect(window.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(760f, 430f), Vector2.zero);
            window.AddComponent<ResponsivePanelScaler>().ReferenceSize = new Vector2(760f, 430f);
            var background = window.AddComponent<Image>();
            UiThemeConfig.Runtime.StylePanel(background, true);
            background.color = new Color(0.018f, 0.028f, 0.04f, 0.98f);
            background.raycastTarget = true;

            var controller = window.AddComponent<ServerSettingsWindowController>();
            controller.Panel = window;
            controller.Client = network;
            controller.TitleText = CreateText(window.transform, "Title", Localization.Tr("net.server_settings"), 28, TextAnchor.MiddleCenter);
            controller.TitleText.fontStyle = FontStyle.Bold;
            SetRect(controller.TitleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(700f, 44f), new Vector2(0f, -30f));

            var serverName = CreateText(window.transform, "Server Name", Localization.Tr("net.server_name"), 18, TextAnchor.MiddleCenter);
            serverName.color = UiThemeConfig.Runtime.AccentGold;
            SetRect(serverName.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(680f, 30f), new Vector2(0f, -70f));

            controller.UrlInput = CreateInputField(window.transform, "Server Url Input", network.ServerUrl, Localization.Tr("net.url_placeholder"), new Vector2(0.5f, 0.5f), new Vector2(0f, 66f), new Vector2(560f, 48f));
            controller.StatusText = CreateText(window.transform, "Network Status", network.CurrentStatus, 17, TextAnchor.MiddleCenter);
            SetRect(controller.StatusText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(680f, 46f), new Vector2(0f, 12f));

            var hint = CreateText(window.transform, "Hint", Localization.Tr("net.connection_hint"), 14, TextAnchor.MiddleCenter);
            hint.color = UiThemeConfig.Runtime.TextMuted;
            SetRect(hint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(680f, 30f), new Vector2(0f, -28f));

            var connectButton = CreateRoundButton(window.transform, "Connect Server Button", Localization.Tr("ui.online"), new Vector2(0.5f, 0f), new Vector2(-120f, 48f), new Vector2(190f, 48f), new Color(0.08f, 0.48f, 0.34f), 17);
            connectButton.onClick.AddListener(controller.Connect);
            var closeButton = CreateRoundButton(window.transform, "Close Server Settings", Localization.Tr("ui.close"), new Vector2(0.5f, 0f), new Vector2(120f, 48f), new Vector2(190f, 48f), new Color(0.22f, 0.28f, 0.34f), 17);
            closeButton.onClick.AddListener(controller.Toggle);
            window.SetActive(false);
            return controller;
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
            UiThemeConfig.Runtime.StylePanel(backgroundImage);
            backgroundImage.color = new Color(0.08f, 0.1f, 0.12f, 0.72f);
            backgroundImage.raycastTarget = true;
            SetRect(background.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0.5f, 0.5f), new Vector2(188f, 188f), new Vector2(142f, 142f));

            var knob = CreateUiObject("Joystick Knob", background.transform);
            var knobImage = knob.AddComponent<Image>();
            knobImage.sprite = UiThemeConfig.Runtime.CircleSprite;
            knobImage.preserveAspect = true;
            knobImage.color = new Color(0.74f, 0.82f, 0.92f, 0.86f);
            SetRect(knob.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(76f, 76f), Vector2.zero);

            var joystick = background.AddComponent<VirtualJoystick>();
            joystick.Knob = knob.GetComponent<RectTransform>();
            joystick.Radius = 68f;
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
            UiThemeConfig.Runtime.StylePanel(panelImage, true);
            panelImage.color = new Color(0.025f, 0.055f, 0.07f, 0.94f);
            panelImage.raycastTarget = false;
            SetRect(panel.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(184f, 184f), new Vector2(-18f, -112f));

            var title = CreateText(panel.transform, "Mini Map Title", Localization.Tr("ui.minimap"), 16, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(166f, 24f), new Vector2(0f, -12f));

            var map = CreateUiObject("Mini Map Area", panel.transform);
            var mapImage = map.AddComponent<Image>();
            UiThemeConfig.Runtime.StylePanel(mapImage);
            mapImage.color = new Color(0.06f, 0.12f, 0.14f, 0.94f);
            mapImage.raycastTarget = false;
            SetRect(map.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(150f, 138f), new Vector2(0f, -16f));

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

        private static UiCooldownOverlay AddCooldownOverlay(Transform parent, Color color)
        {
            var overlayObject = CreateUiObject("Cooldown Overlay", parent);
            var overlayImage = overlayObject.AddComponent<Image>();
            overlayImage.sprite = UiThemeConfig.Runtime.CircleSprite;
            overlayImage.type = Image.Type.Filled;
            overlayImage.fillMethod = Image.FillMethod.Radial360;
            overlayImage.fillOrigin = 2;
            overlayImage.fillClockwise = false;
            overlayImage.color = color;
            overlayImage.raycastTarget = false;
            SetRect(overlayObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            overlayObject.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            overlayObject.GetComponent<RectTransform>().anchorMax = Vector2.one;
            overlayObject.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            overlayObject.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            var countdown = CreateText(overlayObject.transform, "Cooldown Timer", string.Empty, 15, TextAnchor.MiddleCenter);
            countdown.color = Color.white;
            countdown.fontStyle = FontStyle.Bold;
            SetRect(countdown.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(70f, 30f), Vector2.zero);

            var cooldown = overlayObject.AddComponent<UiCooldownOverlay>();
            cooldown.Mask = overlayImage;
            cooldown.Countdown = countdown;
            overlayObject.SetActive(false);
            return cooldown;
        }

        private static Button CreateRoundButton(Transform parent, string name, string label, Vector2 anchor, Vector2 position, Vector2 size, Color color, int fontSize = 32)
        {
            var buttonObject = CreateUiObject(name, parent);
            var image = buttonObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            SetRect(buttonObject.GetComponent<RectTransform>(), anchor, new Vector2(0.5f, 0.5f), size, position);

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            UiThemeConfig.Runtime.StyleButton(button, image, color);
            if (name.Contains("Skill") || name.Contains("Attack") || name.Contains("Ultimate"))
            {
                image.sprite = UiThemeConfig.Runtime.CircleSprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
            }

            var text = CreateText(buttonObject.transform, "Label", label, fontSize, TextAnchor.MiddleCenter);
            SetRect(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, Vector2.zero);
            text.fontStyle = FontStyle.Bold;

            return button;
        }

        private static InputField CreateInputField(Transform parent, string name, string value, string placeholder, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var inputObject = CreateUiObject(name, parent);
            var image = inputObject.AddComponent<Image>();
            UiThemeConfig.Runtime.StylePanel(image);
            image.color = new Color(0.03f, 0.04f, 0.05f, 0.52f);
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
            label.text = text.Replace("\n", "  ·  ");
            label.fontSize = 26;
            label.characterSize = 0.031f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontStyle = FontStyle.Bold;
            label.color = color;
        }

        private static void CreateStartupSplash(Transform parent)
        {
            var splashObject = CreateUiObject("Startup Splash", parent);
            var rect = splashObject.GetComponent<RectTransform>();
            StretchToParent(rect);

            var background = splashObject.AddComponent<Image>();
            var splashTexture = Resources.Load<Texture2D>("Art/Generated/valle-reliquias-splash-v2");
            if (splashTexture != null)
            {
                background.sprite = Sprite.Create(
                    splashTexture,
                    new Rect(0f, 0f, splashTexture.width, splashTexture.height),
                    new Vector2(0.5f, 0.5f));
                background.type = Image.Type.Simple;
                background.preserveAspect = false;
                background.color = Color.white;
            }
            else
            {
                background.color = new Color(0.03f, 0.035f, 0.045f, 0.92f);
            }
            background.raycastTarget = false;

            var shade = CreatePanel(splashObject.transform, "Cinematic Shade", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.015f, 0.025f, 0.035f, 0.34f));
            StretchToParent(shade.rectTransform);
            shade.raycastTarget = false;

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
            var serverProfiles = DefaultServerProfiles.Create();
            var selectedServerIndex = Mathf.Clamp(PlayerPrefs.GetInt("mmorpg.server.profile", 0), 0, serverProfiles.Length - 1);
            if (!serverProfiles[selectedServerIndex].Enabled)
            {
                selectedServerIndex = 0;
            }

            var selectedServerUrl = PlayerPrefs.GetString("mmorpg.server.url", serverProfiles[selectedServerIndex].Url);

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
            var accessTexture = Resources.Load<Texture2D>("Art/Generated/valle-reliquias-access-roster-v1")
                ?? Resources.Load<Texture2D>("Art/Generated/valle-reliquias-access-hall-v1");
            if (accessTexture != null)
            {
                overlayImage.sprite = Sprite.Create(accessTexture, new Rect(0f, 0f, accessTexture.width, accessTexture.height), new Vector2(0.5f, 0.5f));
                overlayImage.type = Image.Type.Simple;
                overlayImage.preserveAspect = false;
                overlayImage.color = Color.white;
            }
            else
            {
                overlayImage.color = new Color(0.008f, 0.012f, 0.018f, 0.95f);
            }
            overlayImage.raycastTarget = true;

            // The roster artwork is the hero of this screen. UI only occupies the lower
            // centre so the four class silhouettes and the moonlit valley stay visible.
            var accessShade = CreatePanel(overlay.transform, "Access Cinematic Shade", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.004f, 0.008f, 0.014f, 0.24f));
            StretchToParent(accessShade.rectTransform);
            accessShade.raycastTarget = false;

            var characterSelectionLayer = CreateUiObject("Character Selection Layer", overlay.transform);
            StretchToParent(characterSelectionLayer.GetComponent<RectTransform>());

            var accent = CreatePanel(characterSelectionLayer.transform, "Access Accent", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(650f, 4f), new Vector2(0f, -42f), new Color(0.25f, 0.78f, 0.78f, 0.95f));
            accent.raycastTarget = false;

            var brand = CreateText(characterSelectionLayer.transform, "Access Brand", Localization.Tr("ui.game_title").ToUpperInvariant(), 38, TextAnchor.MiddleCenter);
            brand.fontStyle = FontStyle.Bold;
            brand.color = new Color(0.9f, 0.94f, 1f);
            SetRect(brand.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(760f, 48f), new Vector2(0f, -66f));

            var accessLabel = CreateText(characterSelectionLayer.transform, "Access Label", "BETA OFFLINE  •  AVENTURA LOCAL", 15, TextAnchor.MiddleCenter);
            accessLabel.color = new Color(0.46f, 0.79f, 0.8f);
            SetRect(accessLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(760f, 26f), new Vector2(0f, -102f));

            var actionDeck = CreatePanel(characterSelectionLayer.transform, "Access Action Deck", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(850f, 310f), new Vector2(0f, 26f), new Color(0.008f, 0.015f, 0.024f, 0.76f));
            actionDeck.raycastTarget = false;
            var deckAccent = CreatePanel(characterSelectionLayer.transform, "Access Deck Accent", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(810f, 3f), new Vector2(0f, 326f), new Color(0.26f, 0.78f, 0.78f, 0.9f));
            deckAccent.raycastTarget = false;

            var savedView = CreateUiObject("Saved Character View", characterSelectionLayer.transform);
            StretchToParent(savedView.GetComponent<RectTransform>());

            var savedTitle = CreateText(savedView.transform, "Saved Title", "TU AVENTURA", 22, TextAnchor.MiddleCenter);
            savedTitle.fontStyle = FontStyle.Bold;
            SetRect(savedTitle.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(350f, 36f), new Vector2(-210f, 260f));

            var savedSubtitle = CreateText(savedView.transform, "Saved Subtitle", "Personaje local disponible", 15, TextAnchor.MiddleCenter);
            savedSubtitle.color = new Color(0.72f, 0.81f, 0.88f);
            SetRect(savedSubtitle.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(350f, 28f), new Vector2(-210f, 232f));

            var savedCard = CreatePanel(savedView.transform, "Saved Character Card", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(350f, 114f), new Vector2(-210f, 102f), new Color(0.024f, 0.045f, 0.062f, 0.86f));
            savedCard.raycastTarget = false;
            var savedAccent = CreatePanel(savedView.transform, "Saved Character Accent", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(350f, 4f), new Vector2(-210f, 157f), new Color(0.22f, 0.72f, 0.72f, 1f));
            savedAccent.raycastTarget = false;

            var savedBadge = CreateText(savedView.transform, "Saved Badge", "PERSONAJE", 13, TextAnchor.MiddleLeft);
            savedBadge.fontStyle = FontStyle.Bold;
            savedBadge.color = new Color(0.46f, 0.9f, 0.65f);
            SetRect(savedBadge.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(290f, 24f), new Vector2(-182f, 136f));

            var savedCharacterText = CreateText(savedView.transform, "Saved Character Text", string.Empty, 22, TextAnchor.MiddleLeft);
            savedCharacterText.fontStyle = FontStyle.Bold;
            SetRect(savedCharacterText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(290f, 54f), new Vector2(-182f, 97f));

            var savedHint = CreateText(savedView.transform, "Saved Hint", "Progreso guardado en este dispositivo", 14, TextAnchor.MiddleCenter);
            savedHint.color = new Color(0.6f, 0.69f, 0.75f);
            SetRect(savedHint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(350f, 26f), new Vector2(-210f, 77f));

            var readyTitle = CreateText(savedView.transform, "Ready Title", "LISTO PARA ENTRAR", 22, TextAnchor.MiddleCenter);
            readyTitle.fontStyle = FontStyle.Bold;
            SetRect(readyTitle.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(350f, 36f), new Vector2(220f, 260f));

            var readySubtitle = CreateText(savedView.transform, "Ready Subtitle", "Retoma tu expedición en el Valle", 15, TextAnchor.MiddleCenter);
            readySubtitle.color = new Color(0.72f, 0.81f, 0.88f);
            SetRect(readySubtitle.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(350f, 28f), new Vector2(220f, 232f));

            var savedDetails = CreateText(savedView.transform, "Saved Details", string.Empty, 18, TextAnchor.MiddleCenter);
            savedDetails.color = new Color(1f, 0.88f, 0.55f);
            SetRect(savedDetails.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(350f, 40f), new Vector2(220f, 180f));

            var createView = CreateUiObject("Create Character View", characterSelectionLayer.transform);
            StretchToParent(createView.GetComponent<RectTransform>());

            var createDeck = CreatePanel(createView.transform, "Create Character Deck", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900f, 540f), new Vector2(0f, -28f), new Color(0.008f, 0.016f, 0.026f, 0.76f));
            createDeck.raycastTarget = false;

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

            var continueButton = CreateRoundButton(savedView.transform, "Continue Character Button", Localization.Tr("character.enter_button"), new Vector2(0.5f, 0f), new Vector2(220f, 106f), new Vector2(310f, 54f), new Color(0.16f, 0.42f, 0.72f), 20);
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

            var newCharacterButton = CreateRoundButton(savedView.transform, "New Character Button", Localization.Tr("character.new_button"), new Vector2(0.5f, 0f), new Vector2(220f, 52f), new Vector2(310f, 40f), new Color(0.25f, 0.3f, 0.38f), 16);
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

            if (network != null)
            {
                network.ServerUrl = selectedServerUrl;
                if (network.UrlInput != null)
                {
                    network.UrlInput.text = selectedServerUrl;
                }
            }

            var loginView = CreateUiObject("Offline Account Login View", overlay.transform);
            StretchToParent(loginView.GetComponent<RectTransform>());

            var loginCard = CreatePanel(loginView.transform, "Offline Account Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(590f, 520f), new Vector2(0f, -26f), new Color(0.006f, 0.014f, 0.024f, 0.5f));
            loginCard.raycastTarget = false;
            var loginAccent = CreatePanel(loginView.transform, "Offline Account Accent", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(540f, 4f), new Vector2(0f, 215f), new Color(0.25f, 0.78f, 0.78f, 0.94f));
            loginAccent.raycastTarget = false;

            var loginTitle = CreateText(loginView.transform, "Login Title", "ACCESO AL VALLE", 32, TextAnchor.MiddleCenter);
            loginTitle.fontStyle = FontStyle.Bold;
            SetRect(loginTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(520f, 46f), new Vector2(0f, 174f));

            var loginSubtitle = CreateText(loginView.transform, "Login Subtitle", "Selecciona un servidor e inicia sesion", 16, TextAnchor.MiddleCenter);
            loginSubtitle.color = new Color(0.7f, 0.8f, 0.88f);
            SetRect(loginSubtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(520f, 30f), new Vector2(0f, 140f));

            var serverLabel = CreateText(loginView.transform, "Server Label", "MUNDO DE JUEGO", 13, TextAnchor.MiddleCenter);
            serverLabel.fontStyle = FontStyle.Bold;
            serverLabel.color = new Color(0.48f, 0.9f, 0.84f);
            SetRect(serverLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500f, 24f), new Vector2(0f, 101f));

            var selectedServerText = CreateText(loginView.transform, "Selected Server", string.Empty, 14, TextAnchor.MiddleCenter);
            selectedServerText.color = new Color(0.76f, 0.84f, 0.91f);
            SetRect(selectedServerText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500f, 24f), new Vector2(0f, 18f));

            var serverButtons = new Button[serverProfiles.Length];
            for (var i = 0; i < serverProfiles.Length; i++)
            {
                var profileIndex = i;
                var profile = serverProfiles[i];
                var buttonColor = profile.Enabled ? new Color(0.05f, 0.28f, 0.25f) : new Color(0.13f, 0.16f, 0.2f);
                var serverButton = CreateRoundButton(loginView.transform, $"Login {profile.Id} Button", profile.Label, new Vector2(0.5f, 0.5f), new Vector2(-178f + i * 178f, 57f), new Vector2(166f, 46f), buttonColor, 13);
                serverButton.interactable = profile.Enabled;
                serverButton.onClick.AddListener(() =>
                {
                    selectedServerIndex = profileIndex;
                    selectedServerUrl = serverProfiles[selectedServerIndex].Url;
                    RefreshSelectedServer();
                });
                serverButtons[i] = serverButton;
            }

            var serverRule = CreatePanel(loginView.transform, "Server Rule", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(470f, 2f), new Vector2(0f, -4f), new Color(0.28f, 0.62f, 0.66f, 0.52f));
            serverRule.raycastTarget = false;

            var accountLabel = CreateText(loginView.transform, "Account Label", "USUARIO", 13, TextAnchor.MiddleLeft);
            accountLabel.fontStyle = FontStyle.Bold;
            accountLabel.color = new Color(0.7f, 0.8f, 0.88f);
            SetRect(accountLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400f, 24f), new Vector2(0f, -28f));

            var accountInput = CreateInputField(loginView.transform, "Account Input", OfflineAccountStore.CurrentAccount, "nombre de cuenta", new Vector2(0.5f, 0.5f), new Vector2(0f, -61f), new Vector2(400f, 42f));
            accountInput.characterLimit = 16;

            var passwordLabel = CreateText(loginView.transform, "Password Label", "CONTRASENA", 13, TextAnchor.MiddleLeft);
            passwordLabel.fontStyle = FontStyle.Bold;
            passwordLabel.color = new Color(0.7f, 0.8f, 0.88f);
            SetRect(passwordLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400f, 24f), new Vector2(0f, -106f));

            var passwordInput = CreateInputField(loginView.transform, "Password Input", string.Empty, "contrasena", new Vector2(0.5f, 0.5f), new Vector2(0f, -139f), new Vector2(400f, 42f));
            passwordInput.contentType = InputField.ContentType.Password;
            passwordInput.characterLimit = 64;

            var loginFeedback = CreateText(loginView.transform, "Login Feedback", "Cuenta local: tus personajes se guardan por usuario.", 14, TextAnchor.MiddleCenter);
            loginFeedback.color = new Color(0.62f, 0.72f, 0.8f);
            SetRect(loginFeedback.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500f, 32f), new Vector2(0f, -188f));

            var loginButton = CreateRoundButton(loginView.transform, "Login Button", "ENTRAR", new Vector2(0.5f, 0.5f), new Vector2(-110f, -244f), new Vector2(210f, 48f), new Color(0.12f, 0.34f, 0.72f), 18);
            loginButton.onClick.AddListener(() => TryEnterAccount(false));

            var registerButton = CreateRoundButton(loginView.transform, "Register Button", "REGISTRAR", new Vector2(0.5f, 0.5f), new Vector2(110f, -244f), new Vector2(210f, 48f), new Color(0.12f, 0.48f, 0.34f), 17);
            registerButton.onClick.AddListener(() => TryEnterAccount(true));

            void RefreshSelectedServer()
            {
                for (var i = 0; i < serverButtons.Length; i++)
                {
                    var image = serverButtons[i].GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = i == selectedServerIndex
                            ? new Color(0.12f, 0.55f, 0.45f)
                            : serverProfiles[i].Enabled ? new Color(0.05f, 0.28f, 0.25f) : new Color(0.13f, 0.16f, 0.2f);
                    }
                }

                selectedServerText.text = $"{serverProfiles[selectedServerIndex].Id}  •  {serverProfiles[selectedServerIndex].DisplayName}  •  BETA OFFLINE";
            }

            void TryEnterAccount(bool createAccount)
            {
                string message;
                bool success;
                if (createAccount)
                {
                    success = OfflineAccountStore.TryRegister(accountInput.text, passwordInput.text, out message);
                }
                else
                {
                    success = OfflineAccountStore.TrySignIn(accountInput.text, passwordInput.text, out message);
                }

                loginFeedback.text = message;
                loginFeedback.color = success ? new Color(0.42f, 0.94f, 0.63f) : new Color(1f, 0.45f, 0.4f);
                if (!success)
                {
                    return;
                }

                selectedServerUrl = serverProfiles[selectedServerIndex].Url;
                if (network != null)
                {
                    network.PlayerName = OfflineAccountStore.CurrentAccount;
                    network.ServerUrl = selectedServerUrl;
                    if (network.UrlInput != null)
                    {
                        network.UrlInput.text = selectedServerUrl;
                    }
                }

                SyncCharacterSelection();
                loginView.SetActive(false);
                characterSelectionLayer.SetActive(true);
            }

            RefreshSelectedServer();
            SyncCharacterSelection();
            characterSelectionLayer.SetActive(false);
            Time.timeScale = 0f;

            void SyncCharacterSelection()
            {
                savedData = persistence != null ? persistence.LoadOrNull() : null;
                if (savedData != null)
                {
                    if (System.Enum.TryParse(savedData.ClassName, out CharacterClassType restoredClass))
                    {
                        selectedClass = restoredClass;
                    }

                    if (System.Enum.TryParse(savedData.GenderName, out CharacterGender restoredGender))
                    {
                        selectedGender = restoredGender;
                    }

                    nameInput.text = savedData.CharacterName;
                    var savedClassDefinition = ClassDefinition.Create(selectedClass);
                    savedCharacterText.text = $"{savedData.CharacterName}\n{ClassDisplayName(selectedClass)}  •  {GenderDisplayName(selectedGender)}";
                    savedDetails.text = Localization.Tr("character.level_details", savedData.Level, savedData.Experience, savedData.Gold);
                    savedAccent.color = savedClassDefinition.BodyColor;
                    savedBadge.color = Color.Lerp(savedClassDefinition.BodyColor, Color.white, 0.35f);
                    savedView.SetActive(true);
                    createView.SetActive(false);
                }
                else
                {
                    selectedClass = CharacterClassType.Guerrero;
                    selectedGender = CharacterGender.Masculino;
                    nameInput.text = Localization.Tr("identity.default_name");
                    savedView.SetActive(false);
                    createView.SetActive(true);
                }

                backButton.gameObject.SetActive(savedData != null);
                identity.ApplySelection(nameInput.text, selectedGender);
                classController.ApplyClass(selectedClass);
                RefreshPreview();
            }

            void ConnectToSelectedServer()
            {
                if (network == null)
                {
                    return;
                }

                var address = string.IsNullOrWhiteSpace(selectedServerUrl)
                    ? serverProfiles[selectedServerIndex].Url
                    : selectedServerUrl;
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

                // The offline beta never opens a socket automatically. Network setup remains
                // available from Datos once an online backend is ready for a controlled test.
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
            UiThemeConfig.Runtime.StylePanel(frameImage);
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
            UiThemeConfig.Runtime.StylePanel(image, color.a > 0.8f);
            image.color = color;
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
            text.font = UiThemeConfig.Runtime.Font;
            if (text.font == null)
            {
                text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
            }
            text.fontSize = fontSize;
            text.color = UiThemeConfig.Runtime.TextPrimary;
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
