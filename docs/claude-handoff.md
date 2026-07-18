# Prompt De Traspaso (Claude Code / Codex / otro agente)

Copiar y pegar el bloque siguiente al iniciar una sesion nueva con cualquier
asistente de codigo:

```text
Estoy trabajando en un RPG de accion para Android en Unity 6000.5.3f1 + C#,
repo https://github.com/MiguelTroncoso/RPG. Inspirado en las mecanicas
generales de MU Online/Metin2 (progresion 1-105, mejora +0..+15 con riesgo,
clases, zonas por rango) pero con identidad, nombres e historia 100%
originales. Hay un servidor WebSocket Node.js en Server/ para online local.

ANTES DE TOCAR NADA lee, en este orden:
1. CLAUDE.md (contrato del proyecto: reglas no negociables, como probar).
2. GAME_ARCHITECTURE.md (arquitectura objetivo y hoja de ruta §16 con etapas
   marcadas; las que tienen check ya estan hechas).
3. README.md (historial de fases 1 a 5.62 y como ejecutar).
4. docs/progress.md (porcentaje estimado y registro diario).
5. docs/content-completeness.md (ultimo resultado de la auditoria 1-105).
6. El codigo existente relacionado con tu tarea.

Estado actual (fases 1-5.75 entregadas en primera pasada; refinamientos 5.44,
5.45, 5.49, 5.58, 5.59, 5.60, 5.61, 5.62, 5.63, 5.64 y 5.65 completados en esta pasada; hoja de ruta A-K completa):
- La escena se genera 100% en runtime desde
  Assets/Scripts/Core/PrototypeBootstrap.cs. No hay prefabs de escena.
- 4 clases (Guerrero/Ninja/Chaman/Umbra) con stats de combate propios
  (critico, precision, evasion, defensa) en ClassDefinition.cs.
- Progresion 1-105: LevelProgressionTable generada desde ExpCurveConfig;
  la subida de nivel se resuelve en ExperienceResolver (funcion pura).
  Los puntos de atributo se gastan en la ventana STATS (PlayerAttributes +
  AttributeConfig: Fuerza/Vitalidad/Agilidad).
- Items: ItemDefinition/ConsumableItemDefinition/EquipmentItemDefinition
  (ScriptableObjects) + ItemDatabase con validacion de IDs + RarityTable
  (colores centralizados). Inventario por instancias (ItemInstance con GUID)
  en InventorySystem. Equipamiento por slots con requisitos en
  PlayerEquipment. Mejora +0..+15 con riesgo (UpgradeConfig +
  UpgradeResolver puro + runa de proteccion + tope por rareza).
- Progresion de loot 1-105: `ProgressionItemCatalog` crea dos materiales y
  doce piezas de equipo por cada una de las diez zonas, mas una reliquia
  exclusiva por jefe. `ZoneLootProgression` separa drops normal/elite/jefe y
  `EquipmentItemDefinition.UpgradeMaterialId` enlaza cada pieza con el nucleo
  de refinamiento de su zona.
- Combate: DamageCalculator puro (rolls inyectados, critico/evasion/defensa,
  varianza +/-15%). Botin por LootTableConfig (pesos configurables). Enemigos
  tienen windup/cooldown por tier, aviso visual antes de golpear, cancelacion
  si el jugador sale de rango, popup critico mas grande y flash de dano.
- Telemetria: CombatTelemetry mide kills, muertes, dano dado/recibido y tiempo
  promedio para matar por zona, separado por normales/elites/jefes; guarda JSON
  local en persistentDataPath y envia snapshots al servidor si el cliente esta
  online. El boton DATOS abre un panel dentro del HUD con estados RAPIDO/LENTO/OK
  contra objetivos TTK configurables por ZoneDefinition.
- UI mobile: el boton MENU abre pestañas de Inventario, Equipo y Mision. El
  contenido detallado se actualiza por eventos del HUD, incluyendo objetos,
  slots, mejoras, bonos, objetivos y recompensas.
- Modelos y feedback: KayKit CC0 Knight/Rogue/Mage/Barbarian ya estan en
  Resources. Cada uno tiene un controller generado desde 76 clips con
  `Idle`/`Run`/`Attack`; el audio procedural y las particulas de combate tienen
  fallback sin assets externos.
- Variantes y Android: RogueHooded se usa para Ninja femenina, todas las
  clases reciben armadura visual por clase/sexo y `MobileRuntimeDiagnostics`
  registra resolucion, orientacion, safe area y volumenes.
- Audio final de prototipo: SFX y musica de Kenney CC0 se cargan desde
  `Resources/Audio/Kenney`; los tonos procedural son fallback.
- Prueba movil: el boton `TEST` abre `MobileTestWindowController` y muestra
  resolucion/orientacion/safe area junto con controles para musica y SFX.
- Modelos femeninos: `AnimatedWoman.fbx` de Quaternius (CC0) se usa para
  Guerrero/Chaman/Umbra femeninos; `RogueHooded.fbx` queda para Ninja.
  `ClassDefinition.ModelResourceFor` permite reemplazar cada recurso por una
  variante dedicada sin modificar la seleccion de personaje.
- Equipo visual: `EquipmentItemDefinition.VisualId` describe la apariencia,
  `PlayerEquipment.Changed` notifica cambios y `EquipmentVisualController`
  reconstruye casco/pechera/amuleto/espada. `ApplyBonuses` sigue siendo el
  unico punto de recomputo de estadisticas.
- Sets modulares: `DefaultArmorVisualSets` crea ocho perfiles por clase/sexo
  (`Knight`, `Assassin`, `Spirit`, `Void`) con colores, silueta y piezas
  distintivas. Es el fallback mientras se convierten a assets editables.
- Android: `AndroidBuildTools` deja ARM64, SDK minimo 26, target SDK 35,
  landscape e identificador configurados. La APK debug se genero y valido
  con `aapt2`; la instalacion fisica queda pendiente de conectar un telefono.
- Acceso mobile: el splash y la pantalla previa al mundo ahora muestran la
  identidad de Valle de las Reliquias, una tarjeta del personaje local y los
  flujos `ENTRAR AL VALLE`, `CREAR NUEVO PERSONAJE` y `VOLVER`. Tambien se
  muestran S-01 Valle Central, S-02 Bosque de los Susurros y S-03 Reino de
  Pruebas; solo S-01 apunta al WebSocket local 7777. S-02/S-03 requieren
  instancias de backend antes de habilitarse.
- Preparacion online: `ServerProfile` centraliza ID/nombre/URL/disponibilidad;
  S-01 permite editar la URL desde la APK, la recuerda y conecta automaticamente
  al entrar. El servidor Node admite `SERVER_ID`, `SERVER_NAME`, `HOST`, `PORT`
  y `MAX_PLAYERS`, envia metadatos en `welcome` y escucha en `0.0.0.0` por defecto.
  La prueba con dos telefonos y el despliegue WSS/publico siguen pendientes.
- QA Android: la APK corregida no usa la consola Development, tiene colision
  explicita del campo, recuperacion de caidas, saneamiento de posiciones
  guardadas y escala landscape mas legible. En la Fase 5.42, el ataque tactil
  queda reservado al boton movil y el joystick conserva el dedo activo para
  evitar conflictos al jugar con dos dedos. Repetir la prueba tactil antes de
  avanzar a contenido nuevo.
- Mobs: `EnemyVisualController` reemplaza las capsulas por familias
  procedurales de bestia, guardian, espiritu y sombra. Elites y jefes tienen
  adornos propios y todos los enemigos muestran nombre y barra de vida sobre
  la cabeza. La primera capa de modelos reales ya usa 16 FBX de Quaternius
  Ultimate Monsters CC0 en zonas 1-10, con controladores `Idle`, `Run` y
  `Attack` generados en Resources; el fallback procedural sigue activo si un
  asset falta. Los diez jefes usan modelos distintos del pack, una silueta
  adicional propia por zona y una paleta aplicada con `MaterialPropertyBlock`;
  el reemplazo por meshes finales propios queda para una etapa artistica
  posterior.
- Cosméticos: `CosmeticService` mantiene slots separados de atuendo y alas,
  propiedad persistida, tienda MVP y bonos de daño/vida/critico conectados a
  `StatSheet`. `PlayerAvatarVisual.ApplyCosmetics` crea una primera silueta 3D
  procedural reemplazable por meshes finales.
- Compañeros y eventos: `PetService`/`MountService` conservan propiedad,
  rareza y stats de combate; `DailyEventSystem` activa una Caceria de
  Reliquias por fecha local, cuenta cinco derrotas y entrega EXP/oro y Alas de
  brasa. El guardado local es esquema v15. Compras y recompensas deben pasar a
  autoridad del servidor antes de una prueba publica.
- Longevidad: `RepeatableContractSystem` genera tres contratos diarios UTC por
  banda actual (normal, elite y material), muestra progreso y entrega
  recompensas persistidas. `PlayerProgression.TryRebirth` permite Renacer al
  nivel 105, conserva colecciones/equipo/cosmeticos/habilidades, reinicia la
  campana y otorga Renombre mas 2% de EXP por Renacimiento. El boton vive en
  `MAS`; fecha, recompensas y Renacimientos deben pasar a autoridad del
  servidor antes de abrirlo al publico.
- Eventos live-ops locales: `WeeklyEventSystem` reinicia Conquista del Valle
  cada lunes UTC (30 derrotas y 3 elites), y `SeasonProgressionSystem` ofrece
  una temporada de 28 dias y 30 niveles con hitos 5/10/15/20/25/30. La XP
  proviene de combate, misiones, contratos y el evento semanal; el nivel 30
  desbloquea `void_outfit`. `EventCalendarSystem` agrega siete actividades
  diarias y un jefe mundial con horario UTC variable y ventana de 60 minutos.
  `GuildEventSystem` y `FreeForAllEventSystem` dejan contribucion y resultados
  preparados para servidor, sin falsear PvP local. El progreso se guarda en
  esquema 16 y se muestra en el menu de Misiones. Fecha, recompensas y cambio
  de temporada deben pasar a autoridad del servidor antes de publicar.
- Visual 3D: `VisualMaterialUtility` centraliza materiales compartidos con
  smoothness/metal/emision; `PrototypeBootstrap` configura luz calida, niebla
  lineal y camara sin HDR; `ZoneEnvironmentBuilder` agrega acentos de suelo
  por paleta de zona. La pasada visual ya cubre avatar, armadura, mobs, jefes,
  NPC, mascotas, monturas y VFX; falta validar contraste y rendimiento en
  telefono real.
- Habilidades: `PlayerSkills` administra cinco habilidades propias por clase
  en Q/E/R/F/G; R, F y G se desbloquean en niveles 8/20/50, la final progresa
  de nivel 1 a 10 con cooldown UTC de 30 minutos, y `PlayerPersistence`
  conserva el arreglo en `PlayerSaveData` esquema 15. El servidor debe validar
  consumos, desbloqueos y efectos antes de la progresion online.
- Equipo y mundo: cada banda tiene doce piezas de equipo visibles por ranura,
  con rareza/tier, emblemas 3D por clase y landmark propio para cada una de las
  diez zonas.
- Fase 5.61/5.62/5.63/5.64/5.65: `docs/content-completeness.md` pasa 17/17 checks y confirma
  contenido funcional 1-105 completo: 10 zonas, 35 misiones, 162 items, 30
  tablas de loot, 120 piezas de set, 10 reliquias, contratos repetibles,
  Renacimiento, eventos semanales, temporada, calendario diario, jefe mundial
  y contratos base de clan/PvP. El guardado local actual es esquema 16.
- Hetzner preflight: `Server/src/server.js` expone `/health`, limita payload,
  hace heartbeat y persiste al recibir senales de apagado. Falta crear la
  instancia, configurar dominio/Nginx/WSS, firewall, backups y probar dos APK.
- Interfaz mobile: el HUD principal es mas compacto, inventario y equipo se
  consultan desde `MENU`, y las acciones secundarias se agrupan bajo `MAS`.
- Fase 5.65: `UiThemeConfig` aplica paneles con marco, sombras, tipografia y
  estados de boton; `PrototypeHud` separa vida/energia/objetivo/feed, usa
  habilidades circulares y permite colapsar el chat. `ConnectionStatusIndicator`
  muestra solo `S-01` y un punto de estado; URL y errores completos pasan a
  `ServerSettingsWindowController`.
- Fase 5.65: `UiPerformanceSettings` expone `Quality` y `Performance`, guarda
  la preferencia de dispositivo y deja `Performance` como valor inicial en
  Android. La orientacion y autorrotacion quedan fijas en horizontal.
- Fase 5.66 (partes 1 a 4): la direccion artistica esta documentada en
  `docs/character-art-direction.md`. `CharacterArtProfiles` define las ocho
  variantes jugables y `PlayerAvatarVisual` aplica sus escalas, materiales,
  siluetas, armas FBX, hombreras, capas, mascaras, coronas y nucleos de clase
  sobre los FBX actuales. Zona 1 usa Tribal, Orc y Orc_Skull por tier con
  tratamiento de reliquia. Falta como mejora posterior el arte propio o
  comercial de mayor fidelidad.
- Fase 5.67: `OriginalArtVisualFactory` genera el primer arte propio low-poly
  en runtime para Guerrero masculino y el mob normal de Zona 1. Son mallas
  originales sin packs externos, con materiales compartidos y animacion
  procedural. Faltan UVs, texturas atlas, rigs FBX y las otras siete variantes.
- Fase 5.68: `OriginalArtVisualFactory` genera las ocho variantes propias por
  clase/sexo, con UVs, microtexturas albedo, armas iniciales y rig modular
  runtime con `WeaponGrip`/`SpellHand`. El rig skinned FBX, atlas de mayor
  resolucion, normales, LOD y animaciones finales quedan como pulido comercial.
- Fase 5.69: el cuerpo propio se combina en `SkinnedMeshRenderer` con pesos por
  pieza, atlas albedo/normal 512x512, `LODGroup` y animacion procedural de
  brazos/piernas.
- Fase 5.70: Blender 5.2 ya esta instalado. El script
  `Tools/blender/create_original_art_assets.py` genera ocho FBX skinned
  authored, atlas albedo/normal, `Starter Weapon` separado y tres clips por
  variante. `Assets/Editor/OriginalArtAnimatorAssetGenerator.cs` crea los ocho
  controllers de Unity; `PlayerAvatarVisual` los prioriza y conserva fallback
  procedural. Validar el pipeline con `docs/original-art-pipeline.md`.
- Fase 5.71: los ocho FBX incluyen cuerpo y arma en `LOD0`/`LOD1`/`LOD2`, con
  decimacion geometrica real y `LODGroup` Unity a 0.52/0.22/0.08. La geometria
  base tiene mas segmentos, suavizado y bordes biselados; `Idle`, `Run` y
  `Attack` animan torso, brazos, manos, cadera, piernas, pies y cabeza. El
  siguiente salto comercial es escultura de mayor fidelidad, bake desde
  high-poly, atlas 2K y animaciones finales producidas en Blender.
- Fase 5.72: los atlas authored son 1024x1024 con tiles 64x64 y normal detail
  offline para los materiales de clase. La geometria base tiene mas densidad,
  biseles y normales ponderadas; `Idle`, `Run` y `Attack` refinan cuello/cabeza
  y mantienen anticipacion, impacto y recuperacion. El siguiente paso es
  escultura high-poly, bake desde esa escultura y animacion profesional.
- Fase 5.73: cuatro familias de clase usan atlas 2K independientes de albedo y
  normal. Blender ejecuta 16 bakes desde fuentes high-poly temporales y los
  proyecta sobre ocho FBX skinned con `LOD0`/`LOD1`/`LOD2`; Unity conserva tres
  clips por variante y comprime los atlas Android con ETC2. El bake es tecnico,
  no una escultura manual comercial; faltan retopologia, texturas pintadas y
  animaciones profesionales.
- Fase 5.74: la fuente high-poly temporal sube a Catmull-Clark nivel 2 con
  detalle de escultura y microdetalle; cada cuerpo llega a aproximadamente
  36k-51k vertices antes del bake y las armas se procesan por separado.
- Fase 5.75: la jaula objetivo limpia vertices duplicados, recalcula normales y
  registra `authored_target_v2`; las UVs usan padding por tile y el albedo 2K
  recibe pintura authored procedural por material. La siguiente mejora es
  escultura/retopologia/textura manual de produccion.
- Fases 5.76-5.77: Guerrero y Ninja masculino/femenino reciben piezas authored
  adicionales y conservan retopologia, UV padded, bake normal, atlas 2K y
  LOD0/1/2. Los meshes Guerrero/Ninja registran `hero_art_pass`.
- Fase 5.78: los ocho FBX contienen cinco clips (`Idle`, `Run`, `Attack`,
  `Hit`, `Death`); controllers y `AvatarMotionAnimator` pueden disparar las
  reacciones de combate.
- Fase 5.79: las diez zonas y sus 30 spawns normal/elite/jefe conservan modelo
  3D, paleta, adornos, sombras y fallback authored; los 16 controllers de mobs
  agregan estados Hit/Death. Falta el reemplazo comercial por escultura manual.
- Fase 5.80: `MobileRuntimeDiagnostics` exporta JSON desde `TEST` con FPS,
  memoria, safe area, perfil, dispositivo, enemigos y POI. La matriz esta en
  `docs/android-qa.md`.
- Fase 5.81: `Tools/blender/create_original_mob_assets.py` genera 30 FBX
  authored propios, normal/elite/jefe para las diez zonas, con atlas 2K,
  skinning, LOD0/1/2 y cinco clips. Unity genera 30 controllers y el runtime
  prioriza estos modelos con fallback Quaternius/procedural.
- Fases 5.83-5.84: `PrototypeHud` ya usa HUD contextual con vida, EXP, nombre y
  nivel separados, objetivo ocultable, minimapa, estado S-01 discreto, chat
  colapsado en Android y overlays radiales para enfriamientos. Inventario,
  equipo, misiones y eventos usan tarjetas visuales; habilidades muestran
  nodos, requisitos y estado de mejora. La APK de validacion es
  `Builds/Android/valle-reliquias-debug.apk` (77 MB, SHA-256
  `7d925d27c941a611f2fd1b5503d361c042b016e45d00619547b82b83e869d52b`).
- Fases 5.85-5.86: la cobertura tecnica authored esta completa para las 8
  variantes de personaje y los 30 mobs normal/elite/jefe de las 10 zonas,
  incluyendo atlas 2K, normales, skinning, LOD y clips base. Sigue pendiente
  la sustitucion comercial manual por esculturas high-poly, retopologia, bake
  de normales y animaciones profesionales, ademas de QA visual en Android.
- Fases 5.89-5.90 offline: `ArtPresentationUtility` ancla personajes y mobs
  al terreno con sombra/aura por tier; `OfflinePresentationDirector` añade
  cielo procedural y atmosfera ligera; `CombatFeedbackVfx` añade anillos para
  impactos, telegraph, habilidades, nivel y ultimate; el audio separa
  telegraph y ultimate. El splash propio usa
  `Resources/Art/Generated/valle-reliquias-splash-v2.png` y el splash generico
  de Unity queda desactivado. La APK nueva queda pendiente de regenerar por
  una perdida temporal del servicio de licencia de Unity Hub.
- Fase 5.91 beta offline: `OfflineZoneTravelController` alimenta `MAS > MAPA`
  con las diez zonas, rangos, mob, elite, jefe y reliquia. Modo campana respeta
  nivel; modo prueba teletransporta a cada zona para QA sin depender del
  backend. El acceso usa `valle-reliquias-access-hall-v1.png` y el Atlas usa
  `valle-reliquias-world-atlas-v1.png`. Recorrido: `docs/offline-beta-qa.md`.
- Online recomendado despues del QA offline: beta S-01 en OVHcloud Hillsboro,
  primero 2 vCore/4 GB y luego 2 vCore/8 GB con PostgreSQL; la guia es
  `docs/online-beta-runbook.md`. Hetzner Hillsboro/Ashburn queda como alternativa
  costo/rendimiento y Contabo como staging, no S-01 publico inicial.
- Mundo: `ZoneEnvironmentBuilder` genera decoracion determinista para las
  zonas 1-10, cubriendo nivel 1-105. Caminos, obstaculos y puntos de interes
  y landmarks base ya estan generados; hay cuatro obstaculos solidos por zona
  y `EXPLORAR` interactua con entrada, elite, jefe y campamento. Los reclamos
  tienen un id estable por zona/tipo y se persisten por personaje; el arte
  final y la validacion autoritativa online quedan pendientes.
- Feedback: `AvatarMotionAnimator` mueve mobs con modelos, el windup tiene
  telegraph y la muerte escala el visual antes de destruirlo.
- Rendimiento: `EnemySpawner` solo mantiene activos los enemigos dentro de
  112 unidades del jugador; Android desactiva sombras, antialiasing y reflejos
  para priorizar 60 FPS y limita la cantidad de normales/elites activos. El
  `ZoneBalanceResolver` registra TTK esperado por tier; el balance real
  necesita prueba fisica.
- Comercio seguro: `ZoneDefinition` marca la zona segura del Valle alrededor
  de los NPC. Los spawns se filtran, `EnemyAI` expulsa enemigos y detiene
  ataques, y `PlayerCombat` bloquea combate dentro del radio.
- Misiones data-driven: QuestDefinition (objetivos TalkToNpc/KillEnemies/
  CollectItems/DefeatWorldEvent), cadena de 4 misiones originales,
  RewardService como punto unico de recompensas, boton HABLAR con el
  Mercader del Valle.
- Mascotas y monturas: PetDefinition/MountDefinition + PetService (Zorro del
  valle: +10% EXP/+5% oro, sigue al jugador) y MountService (Caballo bayo:
  nivel 5, velocidad x1.6). Botones MASCOTA/MONTURA.
- Textos del sistema via Localization.Tr(clave) con tabla es
  (LocalizationTable + DefaultLocalization, fallback mezclado con asset).
  Creacion de personaje y PlayerSkills ya usan claves i18n; quedan textos
  secundarios de red/eventos/botones para una pasada final.
- Zonas 1 a 10 (ZoneDefinition + DefaultZones, spawner 100% data-driven
  por zona, una instancia por zona): Bosque de los Susurros (11-20) y
  Colinas Cenicientas (21-30) y Cumbres de Cristal (31-40) al norte, cada
  una con terreno propio, enemigos normales, elites y jefe de zona. Zonas
  5-10 cubren 41-105 (Paso Glacial, Ruinas Sumergidas, Forja Obsidiana,
  Jardin Astral, Santuario del Eclipse, Trono del Vacio). Los objetivos de
  matar filtran por tier o por id de enemigo. Cadena de 34 misiones. Zona 1: Herrero
  (las mejoras WEAPON/ARMOR se hacen cerca de el), Almacen (boton ALMACEN
  deposita/retira materiales, persistido), area de elites al este y jefe de
  zona (Coloso de las Reliquias) al noroeste con respawn propio. Cadena de
  8 misiones (incluye objetivo UpgradeItem y kills filtrados por
  EnemyTier). HABLAR habla con el NPC mas cercano.
- Red: identidad completa (nombre/clase/sexo/nivel) sincronizada; los
  remotos se ven con su sexo real y nivel en la etiqueta. Capa de
  intenciones CON autoridad inicial: level_up y upgrade viajan como
  mensajes "action" con valor; el servidor (Server/src/server.js) valida
  plausibilidad (nivel creciente <=105, mejora +1..+15) y ritmo (800ms),
  difunde las validas como actividad y devuelve actionRejected al emisor
  si no pasan. Persistencia por playerKey en Server/data/players.json:
  presencia basica, snapshot completo `PlayerSaveData` via `saveState`/
  `savedState` y ultimo resumen de telemetria recibido.
- Guardado local JSON (esquema v15) via ISaveStorage/JsonFileStorage con
  escritura atomica + backup: identidad, nivel, EXP, oro, puntos y
  atributos gastados, inventario, equipo con niveles de mejora, mision
  activa y progreso, mascota activa, montura, almacen y posicion del
  jugador y reclamos persistidos de puntos de interes. PlayerPrefs solo para
  settings.
- Patron de datos establecido: cada config es un ScriptableObject con un
  generador en el menu de editor (MMORPG > ...) y un fallback runtime en el
  bootstrap si el asset no existe. Generadores: Items, Upgrade Config, Loot
  Table, Quests, Companions, Level Table, Item Variant Generator (ventana
  que genera equipo 1-105 desde ItemArchetype).
- EquipmentUpgradeSystem.ApplyBonuses es el UNICO punto de recomputo de
  stats derivados. Alimenta PlayerStatSheet/StatSheet con modificadores por
  origen (equipo + mejoras, atributos, buffs y montura). No sumar bonos por
  fuera.
- Avatar: pipeline de modelos 3D listo. ClassDefinition.CharacterModelResource
  apunta a ThirdParty/KayKit/Adventurers/Characters/{Knight|Rogue|Mage|
  Barbarian}; si el FBX esta en Resources se instancia, si no, fallback
  procedural. `AvatarMotionAnimator` da idle/walk/attack procedural al fallback
  y alimenta `Speed`/`Attack` si el modelo real trae un Animator compatible;
  tambien hace CrossFade a estados `Idle`/`Run`/`Attack` en Base Layer si no
  hay parametros.
  Los FBX se copian a mano desde el pack CC0 KayKit Adventurers (ver
  ASSET_LICENSES.md).
- Previews visuales versionados: docs/previews/world-progression.svg y
  docs/previews/avatar-and-ui-preview.svg.

Reglas de trabajo (resumen; el detalle esta en CLAUDE.md):
- Datos en ScriptableObjects/JSON, nunca hardcodeados en logica.
- Reglas criticas (dano, mejora, EXP) como funciones puras portables al
  servidor, con rolls inyectados.
- Definicion vs instancia: los ScriptableObjects no mutan en runtime.
- UI solo presenta; no contiene reglas de juego.
- Codigo en ingles, commits en imperativo en ingles, una etapa por commit.
- Solo assets CC0/MIT/propios, documentados en ASSET_LICENSES.md.
- Una etapa a la vez; no avanzar si la anterior no compila y funciona en Play.
- Al terminar: verificar compilacion en Unity y reportar archivos cambiados
  y como probar.

Como probar:
- Abrir Assets/Scenes/Prototype.unity y dar Play (todo se autogenera).
- Online local: cd Server && npm install && npm start, conectar desde Unity
  a ws://localhost:7777 (IP local del equipo para movil fisico).
- No intentar build Android si falta PlaybackEngines/AndroidPlayer.

Regla operativa: al cerrar cada fase se actualizan README.md, CLAUDE.md y
este handoff (docs/claude-handoff.md).

Proximos objetivos sugeridos:
- Reinstalar la APK y probar primero el splash, acceso local, tarjeta de
  personaje guardado, crear/volver, URL y conexion automatica S-01; despues
  probar dos telefonos en LAN, regeneracion tras dano, minimapa, joystick +
  `ATK`, joystick + camara, giro vertical/horizontal, habilidades, Stats/Datos,
  `EXPLORAR` y safe zone en el telefono.
- Fase 5.65: abrir `Servidor y conexion`, comprobar URL/diagnostico, alternar
  `HABILIDADES` y mejorar una ranura, cambiar `GRAFICOS` entre perfiles y
  verificar que el centro de combate queda despejado en 16:9 y 19.5:9.
- Fases 5.80-5.81: reinstalar la APK, recorrer las diez zonas en ambos perfiles,
  exportar JSON desde TEST y revisar escala, LOD, animaciones y memoria de los
  30 modelos authored en telefono real.
- Prioridad inmediata 5.83-5.86: reinstalar la APK y validar en Android el HUD
  tactil, las tarjetas, los nodos de habilidades, el minimapa y los perfiles
  Calidad/Rendimiento. Registrar FPS, memoria, carga y safe area en 16:9 y
  19.5:9; despues revisar los 30 modelos en las diez zonas y separar defectos
  de arte authored de problemas de runtime.
- Prioridad offline 5.89-5.90: regenerar la APK con Unity Hub licenciado,
  comprobar el splash propio, recorrer las diez zonas, validar sombras/aura,
  VFX de telegraph/ultimate y revisar el audio en un telefono real antes de
  abrir el trabajo de servidor S-01.
- Fase 5.92: HUD compacto igual en editor/Android, chat y acciones secundarias
  cerrados, spawns por zona limitados, jefe contextual, superficie de bioma y
  campamento seguro. Validar la APK generada y ejecutar `offline-beta-qa.md`
  antes de abrir S-01 real.
- Siguiente bloque online: preparar servidor autoritativo S-01 en Hetzner,
  autenticacion, persistencia, seguridad anti-cheat, despliegue y pruebas de
  reconexion antes de abrir una beta externa.
- Fase 5.48: abrir TEST y registrar FPS, memoria, carga, TTK, enemigos y POI
  activos en las diez zonas.
- Fase 5.48: reinstalar la APK y medir TEST, TTK, memoria, minimapa y safe zone
  en telefono real en las diez zonas.
- Fase 5.48: validar en telefono real la escala, lectura, FPS y memoria de los
  diez jefes; ajustar adornos y materiales donde sea necesario.
- Fase 5.52: calibrar oro, precios, TTK real, restricciones por clase y bonus
  de conjunto con telemetria de las diez bandas. La primera pasada ya escala
  probabilidades por banda y el reporte TTK considera el arma esperada.
- Fase 5.45: sustituir progresivamente el set CC0 por meshes finales propios
  cuando la direccion artistica este cerrada.
- Fase 5.49: mover la validacion definitiva de recompensas al servidor y
  ajustar valores con telemetria real.
- Fase 5.55: probar tienda, atuendo, alas, stats, evento diario y persistencia
  en Android; despues server-authoritative para propiedad, compras, eventos y
  drops raros.
- Fase 5.56: reinstalar la APK y comparar la lectura visual de zonas, jefes,
  runas emisivas y niebla; medir FPS/memoria y ajustar el perfil Android.
- Fase 5.58: probar en Android las ranuras Q/E/R/F, los desbloqueos de nivel
  8/20, el consumo de manuales y la persistencia de mejoras.
- Fase 5.62: probar tres contratos diarios, cierre/reapertura de la app,
  finalizacion de campana, Renacimiento al nivel 105, conservacion de equipo y
  reinicio correcto de objetivos.
- Fase 5.63: probar el cambio semanal UTC, los hitos de temporada, la
  recompensa final, el cambio de temporada y la persistencia en Android.

Empieza proponiendo un plan corto para la etapa que te pida y espera mi ok
antes de escribir codigo masivo.
```
