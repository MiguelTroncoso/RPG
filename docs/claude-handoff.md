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
3. README.md (historial de fases 1 a 5.53 y como ejecutar).
4. docs/progress.md (porcentaje estimado y registro diario).
5. El codigo existente relacionado con tu tarea.

Estado actual (fases 1-5.53 entregadas en primera pasada; refinamientos 5.44,
5.45 y 5.49 completados en esta pasada; hoja de ruta A-K completa):
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
  siete piezas de equipo por cada una de las diez zonas, mas una reliquia
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
- Interfaz mobile: el HUD principal es mas compacto, inventario y equipo se
  consultan desde `MENU`, y las acciones secundarias se agrupan bajo `MAS`.
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
- Guardado local JSON (esquema v10) via ISaveStorage/JsonFileStorage con
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
  personaje guardado, crear/volver y selector S-01; despues regeneracion tras dano, minimapa, joystick +
  `ATK`, joystick + camara, giro vertical/horizontal, habilidades, Stats/Datos,
  `EXPLORAR` y safe zone en el telefono.
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

Empieza proponiendo un plan corto para la etapa que te pida y espera mi ok
antes de escribir codigo masivo.
```
