# CLAUDE.md — Proyecto: RPG de fantasía (Unity)

> Contrato del proyecto. Claude Code lo lee al iniciar cada sesión.
> La arquitectura de referencia vive en `GAME_ARCHITECTURE.md` — leerla antes
> de escribir código nuevo.

## Qué es este proyecto

RPG de acción en tercera persona / isométrico para Android, hecho en **Unity
(6000.5.3f1) + C#**, inspirado en las mecánicas generales de MU Online y
Metin2 (progresión 1–105, mejora de items +0..+15 con riesgo, clases, zonas
por rango de nivel) pero con **identidad, nombres, historia y contenido 100 %
originales**. Preparado para multijugador: hay un servidor WebSocket Node.js
en `Server/`.

Estado: prototipo con fases 1–5.68 entregadas en primera pasada y refinamientos activos de 5.44, 5.45, 5.49, 5.58, 5.59, 5.60, 5.61, 5.62, 5.63, 5.64, 5.65, 5.66, 5.67 y 5.68 (movimiento, combate, 4 clases
Guerrero/Ninja/Chamán/Umbra, EXP/oro/loot, online local con chat, misión,
mercader, mejora de equipo, Android-ready a 60 FPS, creación de personaje,
avatar procedural, persistencia local vía `ISaveStorage` + JSON esquema v15 (incluye posición, reclamos de POI, cosmeticos, eventos diarios/semanales, temporada, contratos diarios, Renombre, habilidades y cooldown final),
progresión 1–105 con `LevelProgressionTable`/`ExpCurveConfig`, cinco habilidades
por clase con desbloqueos en nivel 8/20/50, habilidad final G de nivel 1-10,
mejoras con `Manual de habilidades`, contratos diarios y Renacimiento con
persistencia en esquema 15, items como
ScriptableObjects con rarezas en `RarityTable`, inventario por instancias,
equipamiento con slots y requisitos, mejora +0..+15 con riesgo vía
`UpgradeConfig`/`UpgradeResolver`, daño con crítico/evasión vía
`DamageCalculator` puro, botín por `LootTableConfig`, y misiones
data-driven con `QuestDefinition`/`RewardService`, mascotas y monturas
con bonos pasivos, generador de variantes de items por nivel en el
editor, Zonas 1 y 2 data-driven con herrero/almacén/élites/jefes,
identidad completa sincronizada en red con capa de intenciones validadas
por el servidor, textos del sistema con claves i18n en tabla `es`, Zona 3
data-only, pipeline de modelos 3D con fallback procedural, animacion procedural
idle/walk/attack para avatar y fallback, `StatSheet` con modificadores por
origen, creacion de personaje/habilidades migradas a i18n, Zona 4 data-only,
y persistencia de servidor por `playerKey`, puente para Animator real,
slots de atuendo/alas con stats y tienda MVP, propiedad de mascotas/monturas
con stats y evento diario local persistido,
i18n visible secundaria, zonas 5-10 hasta nivel 105, previews SVG y primera
pasada de balance/feedback de combate, telemetria de combate y snapshot completo
de guardado en servidor). Detalle en
`docs/roadmap.md` y `docs/claude-handoff.md`. La escena se genera en runtime
desde `Assets/Scripts/Core/PrototypeBootstrap.cs`; mantener el estilo C#
simple y autocontenido.

## Reglas no negociables

1. **Leer antes de escribir.** Revisar `README.md`, `GAME_ARCHITECTURE.md`,
   `docs/` y el código existente relacionado antes de tocar nada. No duplicar
   ni reescribir sistemas funcionales sin explicar el motivo. No borrar ni
   revertir cambios existentes.
2. **Datos en ScriptableObjects/JSON, nunca en código.** Curvas de EXP,
   stats, probabilidades de mejora, tablas de botín, colores de rareza.
3. **Separación datos / lógica / presentación.** La UI escucha eventos; no
   hace polling ni contiene reglas de juego.
4. **Definición vs instancia**: los ScriptableObjects no mutan en runtime.
5. **Reglas críticas como funciones puras** (daño, mejora, EXP): testeables
   sin escena y portables al servidor.
6. **Guardado**: JSON local vía `ISaveStorage`; PlayerPrefs solo para
   settings. Nada del estado principal del jugador en PlayerPrefs.
7. **Textos visibles al jugador**: claves i18n preparadas para español, nunca
   literales concatenados.
8. **Código en inglés** (clases, métodos, variables). Commits en imperativo,
   en inglés. Una tarea, un commit.
9. **Sin paquetes externos ni assets nuevos sin avisar.** Solo licencias
   CC0/MIT/propias, documentadas en `ASSET_LICENSES.md`.
10. **Una etapa a la vez** (ver hoja de ruta en `GAME_ARCHITECTURE.md` §16).
    No avanzar si la etapa anterior no compila y funciona en Play.
11. Si una tarea es ambigua, **preguntar antes de asumir**.
12. **Al cerrar cada fase, actualizar `docs/claude-handoff.md`** (estado,
    esquema de guardado y próximos objetivos) además del README y este
    contrato.

## Cómo probar

- Abrir `Assets/Scenes/Prototype.unity` y dar Play.
- Server online local: `cd Server && npm install && npm start`, conectar
  desde Unity a `ws://localhost:7777` (IP local del equipo para móvil físico).
- No intentar build Android real si falta `PlaybackEngines/AndroidPlayer`.
- Al terminar cambios de C#, verificar que el proyecto compila en Unity antes
  de dar por cerrada la tarea; reportar lo hecho, archivos cambiados y
  pruebas ejecutadas.

## Estado conocido / limitaciones

- El avatar visual usa FBX KayKit CC0 y controladores Mecanim generados para
  `Idle`/`Run`/`Attack`; la Ninja femenina usa RogueHooded y Guerrero/Chaman/Umbra
  femeninos usan AnimatedWoman de Quaternius. Las cuatro clases reciben una
  capa visual de armadura por clase/sexo y conservan fallback procedural.
- Textos pendientes de migrar a i18n: algunos mensajes secundarios de red,
  eventos de mundo y botones utilitarios.
- Los modelos 3D y clips del pack KayKit Adventurers ya estan importados (ver
  `ASSET_LICENSES.md`). El generador de controllers mantiene el puente
  `Speed`/`Attack`; si falta un asset se usa fallback procedural.
- `MobileRuntimeDiagnostics` registra resolucion/orientacion/safe area y
  volumenes de audio; la ventana TEST permite cambiar musica y SFX durante
  una prueba en dispositivo.
- `EquipmentVisualController` escucha `PlayerEquipment.Changed` y aplica
  `EquipmentItemDefinition.VisualId` al avatar sin duplicar el calculo de stats.
- `DefaultArmorVisualSets` aporta ocho perfiles de armadura clase/sexo con
  estilos Knight, Assassin, Spirit y Void. El runtime fallback puede migrarse
  a assets ScriptableObject sin cambiar el contrato del avatar.
- `AndroidBuildTools` configura ARM64, SDK minimo 26, target SDK 35,
  identificador de paquete y landscape. La APK debug ya fue generada y
  validada; la version de QA no muestra la Development Console sobre los
  controles y falta instalarla en un telefono fisico.
- QA movil: el campo tiene colision explicita, el jugador recupera caidas,
  los guardados corrigen posiciones bajo el mapa, la UI Android usa una
  referencia landscape mas legible, el ataque tactil conserva el joystick
  durante el movimiento, la camara acepta arrastre en zona libre, el jugador
  regenera salud fuera de combate, existe un minimapa 2D y TEST mide FPS real,
  memoria y objetos activos; falta confirmar regeneracion, radar y balance en
  telefono real.
- Persistencia del servidor cubre snapshot completo `PlayerSaveData` por
  `playerKey` via `saveState`/`savedState`; el JSON local sigue siendo respaldo.
- Fase 5.65: `UiThemeConfig` centraliza el tema visual runtime; `PrototypeHud`
  mantiene vida/energia/objetivo/feed por separado, el chat es colapsable y
  la habilidad final usa un boton circular propio. `ConnectionStatusIndicator`
  reduce la red a S-01 + punto de estado, mientras `ServerSettingsWindowController`
  conserva URL y diagnostico en una ventana secundaria.
- Fase 5.65: `UiPerformanceSettings` mantiene `Quality`/`Performance` en
  PlayerPrefs, con `Performance` como valor inicial Android. La orientacion se
  fuerza a LandscapeRight y las autorrotaciones quedan desactivadas.
- Fase 5.66 (partes 1 a 4): `docs/character-art-direction.md` fija fantasia
  realista estilizada para Android y `CharacterArtProfiles` define ocho
  combinaciones visuales. `PlayerAvatarVisual` aplica firmas Vanguard, Veil,
  Spirit y Void, armas FBX reales por clase y una sola composicion cuando hay
  equipo. Zona 1 usa Tribal, Orc y Orc_Skull por tier.
- Fase 5.67: `OriginalArtVisualFactory` genera mallas originales low-poly para
  el Guerrero masculino y el mob normal de Zona 1 directamente con Unity,
  sin geometria externa. El resto del elenco conserva FBX CC0 hasta completar
  UVs, texturas, rigs y variantes propias.
- Fase 5.68: `OriginalArtVisualFactory` genera las ocho variantes propias por
  clase/sexo, con UVs, microtexturas albedo, armas iniciales y rig modular
  runtime con `WeaponGrip`/`SpellHand`. El rig skinned FBX, atlas de mayor
  resolucion, normales, LOD y animaciones finales quedan como pulido comercial.
- Fase 5.69: el cuerpo propio se combina en `SkinnedMeshRenderer` con pesos por
  pieza, atlas albedo/normal 512x512, `LODGroup` y animacion procedural de
  brazos/piernas.
- Fase 5.70: Blender 5.2 esta instalado y
  `Tools/blender/create_original_art_assets.py` exporta ocho FBX authored con
  skinning, atlas albedo/normal, `Starter Weapon` separado y clips
  `Idle`/`Run`/`Attack`. `OriginalArtAnimatorAssetGenerator` crea ocho
  controllers en Resources; el runtime prioriza estos modelos y mantiene
  fallback procedural. El LOD authored actual es `LODGroup` con culling lejano;
  el LOD geometrico real y las animaciones de combate de mayor fidelidad quedan
  para el siguiente bloque comercial. Ver `docs/original-art-pipeline.md`.
- Guardado local en esquema v15 incluye reclamos de puntos de interes,
  cosmeticos/companeros/monturas desbloqueados y progreso del evento diario;
  tambien conserva contratos diarios, eventos semanales, temporada y Renombre;
  `EXPLORAR` no vuelve a
  entregar la misma recompensa al reabrir.
- Acceso mobile: splash renovado, seleccion de personaje guardado, creacion
  de personaje y perfiles de servidor `S-01/S-02/S-03`. Solo `S-01` esta
  habilitado contra el WebSocket local; los otros perfiles esperan backend.
- Prueba online compartida: `ServerProfile` recuerda perfil/URL, la pantalla
  de acceso conecta automaticamente y el estado queda visible. El servidor
  Node acepta `SERVER_ID`, `SERVER_NAME`, `HOST`, `PORT` y `MAX_PLAYERS` y por
  defecto escucha en `0.0.0.0`; falta validar dos telefonos y desplegar WSS.
- Progresion de loot 1-105 con `ProgressionItemCatalog`: dos materiales y doce
  piezas de equipo por zona, diez reliquias de jefe, tablas normal/elite/jefe y
  materiales de mejora asociados al equipo.
- Fase 5.55: `CosmeticService` administra slots de atuendo/alas, tienda MVP y
  stats desde `StatSheet`; `DailyEventSystem` activa la Caceria de Reliquias
  una vez por fecha local y desbloquea Alas de brasa al completarla. Compras,
  propiedad y recompensas siguen pendientes de autoridad server-authoritative.
- Fase 5.56: `VisualMaterialUtility` centraliza materiales con respuesta de luz,
  emision selectiva, niebla lineal y acentos de suelo por zona; la pasada se
  aplica a personajes, mobs, jefes, NPC, compañeros, monturas y VFX. Falta
  ajustar contraste/FPS/memoria con un telefono real.
- Fase 5.58/5.59: `PlayerSkills` administra cinco habilidades por clase,
  desbloqueos R/F/G en niveles 8/20/50, niveles 1-5 y final 1-10, mejoras
  mediante `skill_tome`, cooldown final UTC de 30 minutos y persistencia en
  esquema 15. El HUD muestra niveles, enfriamientos, botones `+` y `RENACER`.
- Fase 5.59: cada banda genera doce piezas de equipo y el avatar las representa
  por ranura, tier y rareza; hay emblemas 3D de clase y landmarks propios para
  las diez zonas.
- Fase 5.60: el servidor tiene `/health`, payload maximo, heartbeat y apagado
  limpio; falta desplegar S-01 en Hetzner con dominio, Nginx/WSS y backups.
- Fase 5.61/5.62/5.63/5.64: `docs/content-completeness.md` pasa 17/17 checks y confirma
  cobertura funcional 1-105: 10 zonas, 35 misiones, 162 items, loot por tier,
  habilidades, contratos repetibles, Renacimiento, eventos diarios/semanales,
  temporada, calendario diario, jefe mundial y contratos base de clan/PvP,
  ademas de recursos 3D base.
- Fase 5.64: `EventCalendarSystem` rota actividades por dia UTC y calcula un
  horario variable de jefe mundial con ventana de 60 minutos. `GuildEventSystem`
  guarda contribucion semanal de clan y `FreeForAllEventSystem` conserva los
  resultados que debe firmar el servidor. El guardado local pasa a esquema 16.
- Telemetria de combate local y opcionalmente online: kills, muertes, dano y
  tiempo promedio para matar por zona.
- Siguientes candidatos: reinstalar la APK corregida y registrar TEST; validar
  Stats/Datos, regeneracion, minimapa, recompensas de POI, safe zone y lectura
  de jefes en telefono; jugar sesiones de prueba en zonas 41-105 y ajustar con
  DATOS; calibrar loot, oro, precios, TTK, restricciones por clase y bonus de
  conjunto con sesiones reales; sustituir progresivamente la base Quaternius
  CC0 por meshes finales propios cuando la direccion artistica este cerrada;
  validar acceso/selector y conexion LAN en Android, desplegar S-01 con WSS,
  crear instancias reales S-02/S-03 y
  pulir comercio y uso tactil de items; probar desbloqueos y mejoras de
  habilidades en Android; despues mover habilidades, consumos y recompensas a
  autoridad server-authoritative.
