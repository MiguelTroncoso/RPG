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

Estado: prototipo con fases 1–5.32 completadas (movimiento, combate, 4 clases
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
y persistencia basica de servidor por `playerKey`, puente para Animator real,
i18n visible secundaria, zonas 5-10 hasta nivel 105, previews SVG y primera
pasada de balance/feedback de combate). Detalle en
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

- El avatar visual puede ser procedural (`PlayerAvatarVisual` + `AvatarMotionAnimator`) o modelo FBX KayKit si existe en Resources; no es arte final.
- Textos pendientes de migrar a i18n: algunos mensajes secundarios de red,
  eventos de mundo y botones utilitarios.
- Los modelos 3D del avatar se activan al copiar los FBX de KayKit
  Adventurers (ver `ASSET_LICENSES.md`); mientras tanto hay fallback procedural.
  El puente alimenta `Speed` y `Attack` si el modelo trae un Animator compatible.
- Persistencia del servidor cubre identidad, nivel, posicion, rotacion y ultima
  mejora validada; inventario/equipo completo/misiones siguen en JSON local.
- Siguientes candidatos: importar clips/modelos reales de personaje, expandir
  persistencia del servidor a inventario/equipo/misiones, balancear zonas
  41-105 en Play con telemetria simple, generar assets reales para las zonas
  finales, o pulir UI mobile/inventario.
