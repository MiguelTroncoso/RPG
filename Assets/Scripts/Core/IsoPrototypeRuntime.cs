using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    // Free-art direction spike: a playable 2D isometric presentation that keeps
    // the prototype scene and the existing 3D runtime available as a fallback.
    public sealed class IsoPrototypeRuntime : MonoBehaviour
    {
        public const string PresentationModeKey = "Valle.PresentationMode2D";

        private const int ReferenceWidth = 1600;
        private const int ReferenceHeight = 900;
        private const float TileWidth = 1.5f;
        private const float TileHeight = 0.75f;

        private readonly List<IsoActor> mobs = new List<IsoActor>();
        private readonly List<IsoActor> worldActors = new List<IsoActor>();

        private Camera worldCamera;
        private IsoActor player;
        private Vector2 moveTarget;
        private bool hasMoveTarget;
        private Vector2 joystickInput;
        private Text statusText;
        private Text objectiveText;
        private Text playerStatsText;
        private Text activityText;
        private int kills;
        private int playerHp = 147;
        private int playerMaxHp = 147;
        private int playerExp;
        private float nextPassiveRegen;

        public static bool Enabled => PlayerPrefs.GetInt(PresentationModeKey, 1) == 1;

        internal void Build()
        {
            ConfigureRuntime();
            EnsureEventSystem();
            CreateCamera();
            CreateWorld();
            CreateHud();
            SetActivity("Valle Central listo. Explora el campamento y derrota a las criaturas de la espesura.");
        }

        private void Update()
        {
            if (player == null || worldCamera == null)
            {
                return;
            }

            var keyboardInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (keyboardInput.sqrMagnitude > 0.01f)
            {
                joystickInput = Vector2.ClampMagnitude(keyboardInput, 1f);
                hasMoveTarget = false;
            }

            if (joystickInput.sqrMagnitude > 0.01f)
            {
                MovePlayer(joystickInput * 2.8f * Time.deltaTime);
            }
            else if (hasMoveTarget)
            {
                var delta = moveTarget - (Vector2)player.Transform.position;
                if (delta.sqrMagnitude < 0.025f)
                {
                    hasMoveTarget = false;
                }
                else
                {
                    MovePlayer(delta.normalized * 2.8f * Time.deltaTime);
                }
            }

            ReadWorldPointer();
            UpdateActorSorting();
            UpdateActorAnimation();
            UpdatePassiveRecovery();
            UpdateMinimapMarker();
        }

        private void ConfigureRuntime()
        {
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.LandscapeRight;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
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

        private void CreateCamera()
        {
            var cameraObject = new GameObject("Iso Camera");
            worldCamera = cameraObject.AddComponent<Camera>();
            worldCamera.tag = "MainCamera";
            worldCamera.orthographic = true;
            worldCamera.orthographicSize = 6.4f;
            worldCamera.clearFlags = CameraClearFlags.SolidColor;
            worldCamera.backgroundColor = new Color(0.035f, 0.06f, 0.075f);
            cameraObject.transform.position = new Vector3(0f, 4.6f, -20f);
        }

        private void CreateWorld()
        {
            var backdrop = CreateBackdropSprite();
            backdrop.transform.position = new Vector3(0f, 4.4f, 1f);
            backdrop.transform.localScale = new Vector3(1.35f, 1.35f, 1f);

            UnityEngine.Random.InitState(1207);
            for (var x = 0; x < 18; x++)
            {
                for (var y = 0; y < 14; y++)
                {
                    var tilePosition = GridToWorld(x, y);
                    var isPath = y >= 5 && y <= 7 && x >= 2 && x <= 15;
                    var isWater = (x < 2 && y > 4) || (x > 15 && y < 5);
                    var color = isWater
                        ? new Color(0.11f, 0.34f, 0.42f)
                        : isPath
                            ? new Color(0.47f, 0.39f, 0.28f)
                            : new Color(0.15f, 0.28f, 0.24f);
                    var accent = isWater
                        ? new Color(0.23f, 0.61f, 0.68f)
                        : isPath
                            ? new Color(0.67f, 0.55f, 0.36f)
                            : new Color(0.24f, 0.42f, 0.31f);

                    var tile = CreateSpriteObject($"Iso Tile {x}-{y}", CreateDiamondSprite(color, accent), tilePosition);
                    tile.GetComponent<SpriteRenderer>().sortingOrder = -1000 - x - y;
                }
            }

            CreateTree("Ancient Tree", GridToWorld(3, 2), new Color(0.16f, 0.38f, 0.25f));
            CreateTree("Ancient Tree", GridToWorld(13, 2), new Color(0.23f, 0.42f, 0.27f));
            CreateTree("Ancient Tree", GridToWorld(4, 10), new Color(0.12f, 0.32f, 0.3f));
            CreateTree("Ancient Tree", GridToWorld(14, 9), new Color(0.27f, 0.33f, 0.2f));
            CreateShrine(GridToWorld(9, 2));
            CreateCamp(GridToWorld(5, 6));

            CreateNpc("Mercader del Valle", GridToWorld(5, 6) + new Vector2(-0.35f, 0.22f), IsoActorKind.Merchant, new Color(0.88f, 0.57f, 0.23f));
            CreateNpc("Almacen de materiales", GridToWorld(5, 6) + new Vector2(0.42f, 0.28f), IsoActorKind.Storage, new Color(0.34f, 0.64f, 0.82f));
            CreateNpc("Herrero de las Reliquias", GridToWorld(6, 6) + new Vector2(0.42f, -0.1f), IsoActorKind.Blacksmith, new Color(0.74f, 0.3f, 0.22f));

            player = CreateActor("Heroe", GridToWorld(8, 7) + new Vector2(0f, 0.25f), IsoActorKind.Player, new Color(0.24f, 0.55f, 0.88f));
            player.MaxHp = playerHp;
            player.Hp = playerHp;
            player.AnimationFrames = CreatePunyFrames("Art/IsometricFree/Source/PunyCharacters/Puny-Characters/Warrior-Blue", 6, 0);
            player.Renderer.sprite = player.AnimationFrames[0];

            CreatePunyMob("Limo corrupto", GridToWorld(11, 3) + new Vector2(0.1f, 0.15f), new Color(0.62f, 0.2f, 0.3f));
            CreatePunyMob("Limo corrupto", GridToWorld(12, 4) + new Vector2(-0.1f, 0.16f), new Color(0.68f, 0.26f, 0.3f));
            CreatePunyMob("Limo corrupto", GridToWorld(14, 5) + new Vector2(0.15f, 0.14f), new Color(0.46f, 0.18f, 0.33f));
            CreateMob("Guardian de musgo", GridToWorld(10, 1) + new Vector2(0f, 0.15f), new Color(0.31f, 0.47f, 0.28f));
            CreateMob("Guardian de musgo", GridToWorld(15, 2) + new Vector2(0f, 0.15f), new Color(0.32f, 0.42f, 0.27f));

            var landmark = CreateWorldLabel("ZONA SEGURA  •  CAMPAMENTO DEL VALLE", GridToWorld(5, 6) + new Vector2(0f, 1.1f), new Color(0.86f, 0.76f, 0.46f));
            landmark.GetComponent<TextMesh>().characterSize = 0.027f;
        }

        private void CreateHud()
        {
            var canvasObject = new GameObject("Iso HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var safeArea = CreateUiObject("Safe Area", canvas.transform);
            ApplySafeArea(safeArea.GetComponent<RectTransform>());

            var topPanel = CreatePanel(safeArea.transform, "Player Panel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(470f, 170f), new Vector2(26f, -24f), new Color(0.035f, 0.07f, 0.09f, 0.93f), new Color(0.24f, 0.7f, 0.72f, 0.9f));
            CreateText(topPanel.transform, "Player Name", "HEROE  •  GUERRERO", 24, TextAnchor.MiddleLeft, new Vector2(420f, 30f), new Vector2(22f, -22f));
            playerStatsText = CreateText(topPanel.transform, "Stats", "NIVEL 1   HP 147/147   ORO 0", 17, TextAnchor.MiddleLeft, new Vector2(420f, 26f), new Vector2(22f, -55f));
            CreateBar(topPanel.transform, "HP", new Vector2(420f, 18f), new Vector2(22f, -88f), new Color(0.16f, 0.82f, 0.47f));
            CreateBar(topPanel.transform, "EXP", new Vector2(420f, 10f), new Vector2(22f, -112f), new Color(0.34f, 0.64f, 0.94f));
            objectiveText = CreateText(topPanel.transform, "Objective", "Mision: Ecos del valle  •  Habla con el Mercader", 15, TextAnchor.MiddleLeft, new Vector2(420f, 26f), new Vector2(22f, -142f), new Color(0.72f, 0.82f, 0.86f));

            var serverPanel = CreatePanel(safeArea.transform, "Server Panel", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(300f, 64f), new Vector2(-26f, -24f), new Color(0.035f, 0.07f, 0.09f, 0.94f), new Color(0.24f, 0.7f, 0.72f, 0.9f));
            CreateText(serverPanel.transform, "Server", "●  S-01   VALLE CENTRAL   OFFLINE", 16, TextAnchor.MiddleCenter, new Vector2(280f, 50f), Vector2.zero, new Color(0.7f, 0.92f, 0.88f));

            var minimap = CreatePanel(safeArea.transform, "Minimap", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(250f, 250f), new Vector2(-26f, -110f), new Color(0.02f, 0.055f, 0.07f, 0.96f), new Color(0.24f, 0.7f, 0.72f, 0.85f));
            CreateText(minimap.transform, "Title", "MAPA  •  ZONA 1", 16, TextAnchor.MiddleCenter, new Vector2(220f, 28f), new Vector2(0f, 100f), new Color(0.77f, 0.9f, 0.9f));
            CreateMinimapDots(minimap.transform);

            var activityPanel = CreatePanel(safeArea.transform, "Activity", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(590f, 72f), new Vector2(0f, 30f), new Color(0.035f, 0.07f, 0.09f, 0.9f), new Color(0.17f, 0.4f, 0.45f, 0.85f));
            activityText = CreateText(activityPanel.transform, "Activity Text", string.Empty, 15, TextAnchor.MiddleCenter, new Vector2(560f, 56f), Vector2.zero, new Color(0.82f, 0.9f, 0.91f));

            var joystickObject = CreateUiObject("Virtual Joystick", safeArea.transform);
            var joystickImage = joystickObject.AddComponent<Image>();
            joystickImage.sprite = CreateCircleSprite(160, new Color(0.07f, 0.13f, 0.16f, 0.92f), new Color(0.34f, 0.72f, 0.7f, 0.9f));
            joystickImage.preserveAspect = true;
            SetRect(joystickObject.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(190f, 190f), new Vector2(54f, 54f));
            var joystick = joystickObject.AddComponent<IsoVirtualJoystick>();
            joystick.Radius = 58f;
            joystick.Knob = CreateUiObject("Knob", joystickObject.transform).GetComponent<RectTransform>();
            joystick.Knob.gameObject.AddComponent<Image>().sprite = CreateCircleSprite(76, new Color(0.67f, 0.78f, 0.9f, 0.95f), Color.clear);
            joystick.Knob.GetComponent<Image>().preserveAspect = true;
            SetRect(joystick.Knob, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(76f, 76f), Vector2.zero);
            joystick.ValueChanged += value => joystickInput = value;

            var actions = CreatePanel(safeArea.transform, "Actions", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(390f, 250f), new Vector2(-26f, 36f), new Color(0.035f, 0.07f, 0.09f, 0.78f), new Color(0.17f, 0.4f, 0.45f, 0.75f));
            var attack = CreateActionButton(actions.transform, "ATK", new Vector2(0.74f, 0.36f), new Vector2(118f, 118f), new Color(0.68f, 0.12f, 0.13f));
            attack.onClick.AddListener(AttackNearest);
            var skillQ = CreateActionButton(actions.transform, "Q", new Vector2(0.27f, 0.68f), new Vector2(72f, 72f), new Color(0.16f, 0.34f, 0.78f));
            skillQ.onClick.AddListener(() => UseSkill("Golpe de acero"));
            var skillE = CreateActionButton(actions.transform, "E", new Vector2(0.52f, 0.82f), new Vector2(78f, 78f), new Color(0.39f, 0.15f, 0.68f));
            skillE.onClick.AddListener(() => UseSkill("Grito de batalla"));
            var skillR = CreateActionButton(actions.transform, "R", new Vector2(0.84f, 0.82f), new Vector2(72f, 72f), new Color(0.12f, 0.46f, 0.55f));
            skillR.onClick.AddListener(() => UseSkill("Cadena espiritual"));
            var skillF = CreateActionButton(actions.transform, "F", new Vector2(0.24f, 0.28f), new Vector2(68f, 68f), new Color(0.63f, 0.18f, 0.23f));
            skillF.onClick.AddListener(() => UseSkill("Juicio celestial"));
            CreateText(actions.transform, "Hint", "ATAQUE  •  HABILIDADES", 13, TextAnchor.MiddleCenter, new Vector2(340f, 24f), new Vector2(0f, -102f), new Color(0.65f, 0.78f, 0.8f));

            var menu = CreateActionButton(safeArea.transform, "Menu", new Vector2(0f, 0.5f), new Vector2(132f, 48f), new Color(0.08f, 0.16f, 0.25f), new Vector2(92f, 0f));
            menu.onClick.AddListener(() => SetActivity("Menu 2D en preparacion: inventario, equipo, misiones y tienda."));
        }

        private void ReadWorldPointer()
        {
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUi(-1))
            {
                SetMoveTarget(Input.mousePosition);
            }

            for (var i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began && !IsPointerOverUi(touch.fingerId))
                {
                    SetMoveTarget(touch.position);
                }
            }
        }

        private void SetMoveTarget(Vector2 screenPosition)
        {
            var world = worldCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -worldCamera.transform.position.z));
            moveTarget = new Vector2(world.x, world.y);
            hasMoveTarget = true;
            joystickInput = Vector2.zero;
        }

        private void MovePlayer(Vector2 delta)
        {
            var next = (Vector2)player.Transform.position + delta;
            next.x = Mathf.Clamp(next.x, -8.8f, 8.8f);
            next.y = Mathf.Clamp(next.y, -0.5f, 10.3f);
            player.Transform.position = new Vector3(next.x, next.y, 0f);
            if (Mathf.Abs(delta.x) > 0.001f)
            {
                player.Renderer.flipX = delta.x < 0f;
            }
        }

        private void AttackNearest()
        {
            IsoActor nearest = null;
            var nearestDistance = 2.4f;
            foreach (var mob in mobs)
            {
                if (mob == null || !mob.Transform.gameObject.activeSelf)
                {
                    continue;
                }

                var distance = Vector2.Distance(player.Transform.position, mob.Transform.position);
                if (distance < nearestDistance)
                {
                    nearest = mob;
                    nearestDistance = distance;
                }
            }

            if (nearest == null)
            {
                SetActivity("No hay enemigos en rango. Acercate a una criatura de la espesura.");
                return;
            }

            nearest.Hp -= 34;
            nearest.Renderer.color = new Color(1f, 0.68f, 0.68f, 1f);
            SetActivity($"Golpe de acero a {nearest.Name} por 34 de dano.");
            if (nearest.Hp <= 0)
            {
                nearest.Transform.gameObject.SetActive(false);
                kills++;
                playerExp += 34;
                SetActivity($"{nearest.Name} derrotado. +34 EXP  •  Muertes {kills}/5");
                objectiveText.text = $"Objetivo: lobos corruptos  •  {kills}/5 derrotados";
            }
        }

        private void UseSkill(string skillName)
        {
            SetActivity($"{skillName} preparado. El sistema de habilidades conserva los niveles y cooldowns del MMORPG.");
        }

        private void UpdatePassiveRecovery()
        {
            if (Time.time < nextPassiveRegen || playerHp >= playerMaxHp)
            {
                return;
            }

            playerHp = Mathf.Min(playerMaxHp, playerHp + 2);
            nextPassiveRegen = Time.time + 1.25f;
            UpdateStatsText();
        }

        private void UpdateStatsText()
        {
            if (playerStatsText != null)
            {
                playerStatsText.text = $"NIVEL 1   HP {playerHp}/{playerMaxHp}   EXP {playerExp}/100   ORO 0";
            }
        }

        private void UpdateActorSorting()
        {
            foreach (var actor in worldActors)
            {
                if (actor == null || actor.Transform == null)
                {
                    continue;
                }

                actor.Renderer.sortingOrder = 100 - Mathf.RoundToInt(actor.Transform.position.y * 10f);
            }
        }

        private void UpdateActorAnimation()
        {
            var frame = Mathf.FloorToInt(Time.time * 6f);
            foreach (var actor in worldActors)
            {
                if (actor == null || actor.Renderer == null || actor.AnimationFrames == null || actor.AnimationFrames.Length == 0)
                {
                    continue;
                }

                actor.Renderer.sprite = actor.AnimationFrames[frame % actor.AnimationFrames.Length];
            }
        }

        private void UpdateMinimapMarker()
        {
            if (player == null || player.MinimapMarker == null)
            {
                return;
            }

            var position = player.Transform.position;
            player.MinimapMarker.rectTransform.anchoredPosition = new Vector2(position.x * 6.3f, position.y * 5.4f - 8f);
        }

        private IsoActor CreateNpc(string name, Vector2 position, IsoActorKind kind, Color color)
        {
            return CreateActor(name, position, kind, color, CreateNpcSprite(kind, color));
        }

        private void CreateMob(string name, Vector2 position, Color color)
        {
            var mob = CreateActor(name, position, IsoActorKind.Mob, color, CreateMobSprite(color));
            mob.MaxHp = 68;
            mob.Hp = 68;
            mobs.Add(mob);
        }

        private void CreatePunyMob(string name, Vector2 position, Color color)
        {
            var mob = CreateActor(name, position, IsoActorKind.Mob, color);
            mob.AnimationFrames = CreatePunyFrames("Art/IsometricFree/Source/PunyCharacters/Puny-Characters/Slime", 6, 0);
            mob.Renderer.sprite = mob.AnimationFrames[0];
            mob.MaxHp = 68;
            mob.Hp = 68;
            mobs.Add(mob);
        }

        private IsoActor CreateActor(string name, Vector2 position, IsoActorKind kind, Color color, Sprite sprite = null)
        {
            var actorObject = new GameObject(name);
            actorObject.transform.position = new Vector3(position.x, position.y, 0f);
            var renderer = actorObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite ?? CreateCharacterSprite(CharacterClassType.Guerrero, color, Color.white);
            renderer.color = Color.white;

            var actor = new IsoActor
            {
                Name = name,
                Kind = kind,
                Transform = actorObject.transform,
                Renderer = renderer,
                Hp = 100,
                MaxHp = 100
            };
            actor.Label = CreateWorldLabel(name, position + new Vector2(0f, 1.15f), kind == IsoActorKind.Mob ? new Color(1f, 0.72f, 0.62f) : new Color(0.82f, 0.94f, 0.94f));
            worldActors.Add(actor);
            return actor;
        }

        private void CreateTree(string name, Vector2 position, Color canopyColor)
        {
            var tree = CreateSpriteObject(name, CreateTreeSprite(canopyColor), position + new Vector2(0f, 0.25f));
            tree.GetComponent<SpriteRenderer>().sortingOrder = -Mathf.RoundToInt(position.y * 10f);
        }

        private void CreateShrine(Vector2 position)
        {
            CreateSpriteObject("Relic Shrine", CreateShrineSprite(), position + new Vector2(0f, 0.28f));
            CreateWorldLabel("SANTUARIO DE LAS RELIQUIAS", position + new Vector2(0f, 1.3f), new Color(0.64f, 0.86f, 1f));
        }

        private void CreateCamp(Vector2 position)
        {
            CreateSpriteObject("Campfire", CreateCampSprite(), position + new Vector2(0f, 0.12f));
        }

        private static GameObject CreateSpriteObject(string name, Sprite sprite, Vector2 position)
        {
            var objectRoot = new GameObject(name);
            objectRoot.transform.position = new Vector3(position.x, position.y, 0f);
            var renderer = objectRoot.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -Mathf.RoundToInt(position.y * 10f);
            return objectRoot;
        }

        private GameObject CreateBackdropSprite()
        {
            var texture = new Texture2D(128, 64, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            for (var y = 0; y < texture.height; y++)
            {
                var t = y / (float)texture.height;
                var color = Color.Lerp(new Color(0.04f, 0.11f, 0.16f), new Color(0.09f, 0.2f, 0.19f), t);
                for (var x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
            return CreateSpriteObject("Iso Backdrop", Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 8f), Vector2.zero);
        }

        private static Sprite CreateDiamondSprite(Color baseColor, Color accentColor)
        {
            var texture = new Texture2D(96, 48, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            ClearTexture(texture, Color.clear);
            for (var y = 0; y < 48; y++)
            {
                var half = y < 24 ? y * 2 : (47 - y) * 2;
                var start = (96 - half) / 2;
                for (var x = start; x < start + half; x++)
                {
                    var edge = y % 7 == 0 || x % 19 == 0;
                    texture.SetPixel(x, y, edge ? accentColor : baseColor);
                }
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 96f, 48f), new Vector2(0.5f, 0.5f), 64f);
        }

        private static Sprite CreateCharacterSprite(CharacterClassType classType, Color primary, Color metal)
        {
            var texture = NewTexture(80, 108);
            DrawEllipse(texture, 40, 13, 24, 8, new Color(0.02f, 0.04f, 0.05f, 0.55f));
            DrawRect(texture, 28, 28, 24, 42, primary);
            DrawRect(texture, 23, 39, 34, 12, classType == CharacterClassType.Ninja ? new Color(0.09f, 0.11f, 0.16f) : metal);
            DrawCircle(texture, 40, 78, 15, classType == CharacterClassType.Ninja ? new Color(0.16f, 0.18f, 0.24f) : new Color(0.68f, 0.5f, 0.38f));
            DrawRect(texture, 25, 83, 30, 8, classType == CharacterClassType.Umbra ? new Color(0.1f, 0.04f, 0.18f) : primary);
            DrawRect(texture, 24, 22, 10, 30, primary * 0.75f);
            DrawRect(texture, 46, 22, 10, 30, primary * 0.75f);
            DrawRect(texture, 26, 8, 10, 22, new Color(0.1f, 0.12f, 0.14f));
            DrawRect(texture, 44, 8, 10, 22, new Color(0.1f, 0.12f, 0.14f));

            if (classType == CharacterClassType.Guerrero)
            {
                DrawRect(texture, 7, 37, 12, 28, metal);
                DrawRect(texture, 58, 48, 6, 40, new Color(0.72f, 0.76f, 0.8f));
                DrawRect(texture, 63, 77, 14, 8, new Color(0.72f, 0.76f, 0.8f));
            }
            else if (classType == CharacterClassType.Ninja)
            {
                DrawRect(texture, 57, 41, 7, 30, new Color(0.7f, 0.75f, 0.82f));
                DrawRect(texture, 64, 66, 12, 5, new Color(0.7f, 0.75f, 0.82f));
                DrawRect(texture, 25, 72, 30, 6, new Color(0.7f, 0.08f, 0.2f));
            }
            else if (classType == CharacterClassType.Chaman)
            {
                DrawRect(texture, 57, 15, 5, 78, new Color(0.55f, 0.32f, 0.14f));
                DrawCircle(texture, 59, 95, 8, new Color(0.28f, 0.76f, 0.88f));
                DrawRect(texture, 25, 66, 30, 5, new Color(0.85f, 0.7f, 0.24f));
            }
            else
            {
                DrawRect(texture, 19, 21, 42, 7, new Color(0.37f, 0.12f, 0.55f));
                DrawRect(texture, 56, 39, 15, 7, new Color(0.6f, 0.18f, 0.8f));
                DrawCircle(texture, 70, 44, 6, new Color(0.75f, 0.3f, 0.92f));
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.12f), 64f);
        }

        private static Sprite CreateMobSprite(Color color)
        {
            var texture = NewTexture(80, 74);
            DrawEllipse(texture, 40, 8, 28, 9, new Color(0.02f, 0.04f, 0.05f, 0.55f));
            DrawEllipse(texture, 40, 35, 29, 22, color);
            DrawCircle(texture, 28, 42, 5, new Color(0.94f, 0.32f, 0.26f));
            DrawCircle(texture, 52, 42, 5, new Color(0.94f, 0.32f, 0.26f));
            DrawRect(texture, 18, 22, 11, 25, color * 0.75f);
            DrawRect(texture, 51, 22, 11, 25, color * 0.75f);
            DrawRect(texture, 24, 53, 10, 8, new Color(0.25f, 0.12f, 0.13f));
            DrawRect(texture, 46, 53, 10, 8, new Color(0.25f, 0.12f, 0.13f));
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.12f), 64f);
        }

        private static Sprite CreateNpcSprite(IsoActorKind kind, Color color)
        {
            var texture = NewTexture(70, 96);
            DrawEllipse(texture, 35, 10, 24, 8, new Color(0.02f, 0.04f, 0.05f, 0.45f));
            DrawRect(texture, 20, 22, 30, 48, color);
            DrawCircle(texture, 35, 77, 13, new Color(0.62f, 0.45f, 0.33f));
            DrawRect(texture, 13, 34, 12, 30, color * 0.78f);
            DrawRect(texture, 45, 34, 12, 30, color * 0.78f);
            if (kind == IsoActorKind.Merchant)
            {
                DrawRect(texture, 9, 61, 52, 9, new Color(0.2f, 0.36f, 0.22f));
            }
            else if (kind == IsoActorKind.Storage)
            {
                DrawRect(texture, 22, 68, 26, 8, new Color(0.88f, 0.72f, 0.25f));
            }
            else
            {
                DrawRect(texture, 57, 15, 5, 70, new Color(0.5f, 0.28f, 0.12f));
                DrawCircle(texture, 59, 91, 9, new Color(1f, 0.42f, 0.22f));
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.12f), 64f);
        }

        private static Sprite CreateTreeSprite(Color canopy)
        {
            var texture = NewTexture(110, 140);
            DrawEllipse(texture, 55, 12, 42, 11, new Color(0.02f, 0.04f, 0.05f, 0.5f));
            DrawRect(texture, 47, 18, 16, 70, new Color(0.27f, 0.15f, 0.08f));
            DrawCircle(texture, 55, 100, 31, canopy);
            DrawCircle(texture, 34, 88, 23, canopy * 0.86f);
            DrawCircle(texture, 76, 91, 24, canopy * 0.92f);
            DrawCircle(texture, 54, 119, 23, canopy * 1.12f);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.12f), 64f);
        }

        private static Sprite CreateShrineSprite()
        {
            var texture = NewTexture(100, 120);
            DrawEllipse(texture, 50, 11, 38, 10, new Color(0.02f, 0.04f, 0.05f, 0.55f));
            DrawRect(texture, 25, 19, 50, 42, new Color(0.27f, 0.32f, 0.42f));
            DrawRect(texture, 34, 61, 32, 36, new Color(0.37f, 0.43f, 0.55f));
            DrawCircle(texture, 50, 98, 14, new Color(0.25f, 0.86f, 0.94f));
            DrawCircle(texture, 50, 98, 7, new Color(0.82f, 0.98f, 1f));
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.12f), 64f);
        }

        private static Sprite CreateCampSprite()
        {
            var texture = NewTexture(100, 90);
            DrawEllipse(texture, 50, 10, 38, 10, new Color(0.02f, 0.04f, 0.05f, 0.55f));
            DrawRect(texture, 20, 25, 60, 8, new Color(0.27f, 0.14f, 0.08f));
            DrawRect(texture, 27, 33, 8, 34, new Color(0.35f, 0.19f, 0.09f));
            DrawRect(texture, 65, 33, 8, 34, new Color(0.35f, 0.19f, 0.09f));
            DrawCircle(texture, 50, 50, 17, new Color(0.98f, 0.34f, 0.12f));
            DrawCircle(texture, 50, 57, 9, new Color(1f, 0.85f, 0.24f));
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.12f), 64f);
        }

        private static TextMesh CreateWorldLabel(string text, Vector2 position, Color color)
        {
            var labelObject = new GameObject("Label - " + text);
            labelObject.transform.position = new Vector3(position.x, position.y, -0.2f);
            var label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 28;
            label.characterSize = 0.022f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontStyle = FontStyle.Bold;
            label.color = color;
            var renderer = labelObject.GetComponent<MeshRenderer>();
            renderer.sortingOrder = 500;
            return label;
        }

        private void CreateMinimapDots(Transform parent)
        {
            var mapRoot = CreateUiObject("Map Content", parent);
            SetRect(mapRoot.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(210f, 190f), new Vector2(0f, -10f));
            var background = mapRoot.AddComponent<Image>();
            background.color = new Color(0.08f, 0.18f, 0.19f, 0.9f);
            CreateMapDot(mapRoot.transform, new Vector2(-50f, 20f), new Color(0.9f, 0.58f, 0.2f), 10f);
            CreateMapDot(mapRoot.transform, new Vector2(46f, 56f), new Color(0.92f, 0.32f, 0.27f), 10f);
            CreateMapDot(mapRoot.transform, new Vector2(64f, 30f), new Color(0.92f, 0.32f, 0.27f), 10f);
            CreateMapDot(mapRoot.transform, new Vector2(-20f, -45f), new Color(0.3f, 0.74f, 0.94f), 10f);
            if (player != null)
            {
                player.MinimapMarker = CreateMapDot(mapRoot.transform, Vector2.zero, new Color(1f, 0.88f, 0.35f), 14f);
            }
        }

        private static Image CreateMapDot(Transform parent, Vector2 position, Color color, float size)
        {
            var dotObject = CreateUiObject("Map Dot", parent);
            var image = dotObject.AddComponent<Image>();
            image.sprite = CreateCircleSprite(32, color, Color.clear);
            image.preserveAspect = true;
            SetRect(dotObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(size, size), position);
            return image;
        }

        private static Button CreateActionButton(Transform parent, string label, Vector2 anchor, Vector2 size, Color color, Vector2 position = default)
        {
            var buttonObject = CreateUiObject("Action " + label, parent);
            var image = buttonObject.AddComponent<Image>();
            image.sprite = CreateCircleSprite(128, color, new Color(1f, 1f, 1f, 0.24f));
            image.preserveAspect = true;
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            SetRect(buttonObject.GetComponent<RectTransform>(), anchor, new Vector2(0.5f, 0.5f), size, position);
            var text = CreateText(buttonObject.transform, "Label", label, label.Length > 2 ? 15 : 24, TextAnchor.MiddleCenter, size, Vector2.zero, Color.white);
            text.fontStyle = FontStyle.Bold;
            return button;
        }

        private static Image CreateBar(Transform parent, string name, Vector2 size, Vector2 position, Color color)
        {
            var barObject = CreateUiObject(name, parent);
            var image = barObject.AddComponent<Image>();
            image.color = color;
            SetRect(barObject.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), size, position);
            return image;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 position, Color fill, Color outlineColor)
        {
            var panel = CreateUiObject(name, parent);
            var image = panel.AddComponent<Image>();
            image.color = fill;
            var outline = panel.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(2f, -2f);
            SetRect(panel.GetComponent<RectTransform>(), anchor, pivot, size, position);
            return panel;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            var objectRoot = new GameObject(name);
            objectRoot.transform.SetParent(parent, false);
            return objectRoot;
        }

        private static Text CreateText(Transform parent, string name, string value, int fontSize, TextAnchor alignment, Vector2 size, Vector2 position, Color color = default)
        {
            var textObject = CreateUiObject(name, parent);
            var text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color == default ? Color.white : color;
            text.raycastTarget = false;
            SetRect(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, position);
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

        private static void ApplySafeArea(RectTransform rect)
        {
            var safeArea = Screen.safeArea;
            rect.anchorMin = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
            rect.anchorMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Vector2 GridToWorld(int x, int y)
        {
            return new Vector2((x - y) * TileWidth * 0.5f, (x + y) * TileHeight * 0.5f - 0.5f);
        }

        private static bool IsPointerOverUi(int pointerId)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            return pointerId >= 0
                ? EventSystem.current.IsPointerOverGameObject(pointerId)
                : EventSystem.current.IsPointerOverGameObject();
        }

        private void SetActivity(string message)
        {
            if (activityText != null)
            {
                activityText.text = message;
            }
        }

        private static Texture2D NewTexture(int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            ClearTexture(texture, Color.clear);
            return texture;
        }

        private static void ClearTexture(Texture2D texture, Color color)
        {
            var pixels = new Color[texture.width * texture.height];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
        }

        private static void DrawRect(Texture2D texture, int x, int y, int width, int height, Color color)
        {
            for (var px = Mathf.Max(0, x); px < Mathf.Min(texture.width, x + width); px++)
            {
                for (var py = Mathf.Max(0, y); py < Mathf.Min(texture.height, y + height); py++)
                {
                    texture.SetPixel(px, py, color);
                }
            }
        }

        private static void DrawCircle(Texture2D texture, int centerX, int centerY, int radius, Color color)
        {
            for (var x = centerX - radius; x <= centerX + radius; x++)
            {
                for (var y = centerY - radius; y <= centerY + radius; y++)
                {
                    if ((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY) <= radius * radius)
                    {
                        if (x >= 0 && y >= 0 && x < texture.width && y < texture.height)
                        {
                            texture.SetPixel(x, y, color);
                        }
                    }
                }
            }
        }

        private static void DrawEllipse(Texture2D texture, int centerX, int centerY, int radiusX, int radiusY, Color color)
        {
            for (var x = centerX - radiusX; x <= centerX + radiusX; x++)
            {
                for (var y = centerY - radiusY; y <= centerY + radiusY; y++)
                {
                    var normalized = Mathf.Pow((x - centerX) / (float)radiusX, 2f) + Mathf.Pow((y - centerY) / (float)radiusY, 2f);
                    if (normalized <= 1f && x >= 0 && y >= 0 && x < texture.width && y < texture.height)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }

        private static Sprite CreateCircleSprite(int size, Color fill, Color ring)
        {
            var texture = NewTexture(size, size);
            var center = size * 0.5f;
            var radius = size * 0.47f;
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (distance <= radius)
                    {
                        texture.SetPixel(x, y, distance >= radius - 5f ? ring : fill);
                    }
                }
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite[] CreatePunyFrames(string resourcePath, int frameCount, int rowFromTop)
        {
            var texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
            {
                return new[] { CreateCharacterSprite(CharacterClassType.Guerrero, new Color(0.25f, 0.55f, 0.92f), new Color(0.78f, 0.84f, 0.92f)) };
            }

            const int frameWidth = 32;
            const int frameHeight = 32;
            var columns = texture.width / frameWidth;
            var rows = texture.height / frameHeight;
            var row = Mathf.Clamp(rows - 1 - rowFromTop, 0, rows - 1);
            var frames = new Sprite[Mathf.Min(frameCount, columns)];
            for (var i = 0; i < frames.Length; i++)
            {
                frames[i] = Sprite.Create(texture, new Rect(i * frameWidth, row * frameHeight, frameWidth, frameHeight), new Vector2(0.5f, 0.08f), 24f);
            }

            return frames;
        }

        private enum IsoActorKind
        {
            Player,
            Mob,
            Merchant,
            Storage,
            Blacksmith
        }

        private sealed class IsoActor
        {
            public string Name;
            public IsoActorKind Kind;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public TextMesh Label;
            public Image MinimapMarker;
            public Sprite[] AnimationFrames;
            public int Hp;
            public int MaxHp;
        }
    }
}
