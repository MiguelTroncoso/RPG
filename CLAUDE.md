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

Estado: prototipo con fases 1–5.51 entregadas en primera pasada y refinamientos activos de 5.44, 5.45 y 5.49 (movimiento, combate, 4 clases
Guerrero/Ninja/Chamán/Umbra, EXP/oro/loot, online local con chat, misión,
mercader, mejora de equipo, Android-ready a 60 FPS, creación de personaje,
avatar procedural, persistencia local vía `ISaveStorage` + JSON esquema v9 (incluye posición),
progresión 1–105 con `LevelProgressionTable`/`ExpCurveConfig`, items como
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
  regenera salud fuera de combate y existe un minimapa 2D; falta confirmar
  regeneracion, radar y balance en telefono real.
- Persistencia del servidor cubre snapshot completo `PlayerSaveData` por
  `playerKey` via `saveState`/`savedState`; el JSON local sigue siendo respaldo.
- Telemetria de combate local y opcionalmente online: kills, muertes, dano y
  tiempo promedio para matar por zona.
- Siguientes candidatos: reinstalar la APK corregida y registrar TEST; validar
  Stats/Datos, recompensas de POI y safe zone en telefono; convertir los
  perfiles de armadura y mobs restantes en assets editables; jugar sesiones de
  prueba en zonas 41-105 y ajustar con DATOS; pulir comercio y uso tactil de
  items.
