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
3. README.md (historial de fases 1 a 5.25 y como ejecutar).
4. El codigo existente relacionado con tu tarea.

Estado actual (fases 1-5.25 completadas; hoja de ruta A-K completa):
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
- Combate: DamageCalculator puro (rolls inyectados, critico/evasion/defensa,
  varianza ±15%). Botin por LootTableConfig (pesos configurables).
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
- Zonas 1, 2 y 3 (ZoneDefinition + DefaultZones, spawner 100% data-driven
  por zona, una instancia por zona): Bosque de los Susurros (11-20) y
  Colinas Cenicientas (21-30) al norte, cada una con terreno propio,
  enemigos normales, elites y jefe de zona. Los objetivos de matar filtran
  por tier o por id de enemigo. Cadena de 13 misiones. Zona 1: Herrero
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
  si no pasan.
- Guardado local JSON (esquema v9) via ISaveStorage/JsonFileStorage con
  escritura atomica + backup: identidad, nivel, EXP, oro, puntos y
  atributos gastados, inventario, equipo con niveles de mejora, mision
  activa y progreso, mascota activa, montura, almacen y posicion del
  jugador. PlayerPrefs solo para settings.
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
  y alimenta `Speed`/`Attack` si el modelo real trae un Animator compatible.
  Los FBX se copian a mano desde el pack CC0 KayKit Adventurers (ver
  ASSET_LICENSES.md).

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
- Zona 4 en adelante (solo datos con ZoneDefinition).
- Persistencia de estado en el servidor (hoy el hello fija el nivel base).
- Importar/controlar animaciones reales del pack KayKit si se agregan FBX/clips.
- Pasada final de i18n para red/eventos/botones utilitarios.

Empieza proponiendo un plan corto para la etapa que te pida y espera mi ok
antes de escribir codigo masivo.
```
