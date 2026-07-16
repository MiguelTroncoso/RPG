# Proyecto MMORPG Android

Este proyecto apunta a crear un MMORPG de accion para Android inspirado en clasicos como MU Online y Metin2, pero con identidad, mundo, nombres, arte y sistemas propios.

La meta no es construir todo el MMORPG de una vez. La estrategia recomendada es avanzar por versiones jugables pequenas, donde cada fase agrega una capa real del juego: movimiento, combate, clases, progresion, online, economia y publicacion.

Documentos clave:

- [`GAME_ARCHITECTURE.md`](GAME_ARCHITECTURE.md): arquitectura tecnica objetivo y hoja de ruta de las proximas etapas.
- [`CLAUDE.md`](CLAUDE.md): contrato de desarrollo (reglas, como probar, estado conocido).
- `docs/`: roadmap, plan tecnico, diseno de juego y checklist Android.
- [`docs/progress.md`](docs/progress.md): porcentaje estimado y registro diario.

## Recomendacion Inicial

- Cliente: Unity 3D.
- Plataforma objetivo: Android.
- Primer estilo de camara: tercera persona/isometrica ajustable, similar a MMORPG clasico.
- Combate inicial: accion simple con objetivo cercano, ataque basico y habilidades con cooldown.
- Online inicial: mapa compartido pequeno con chat y monstruos sincronizados.
- Servidor inicial: Colyseus o Nakama, segun el ritmo del prototipo.

## Primer Objetivo

Crear una vertical slice jugable:

- Pantalla de seleccion de personaje.
- 4 clases iniciales.
- Variante masculina y femenina por clase.
- 1 mapa pequeno.
- Movimiento 3D.
- Ataque basico.
- 2 habilidades por clase.
- Monstruos simples.
- Experiencia y subida de nivel.
- Loot basico.
- Un evento central de mapa inspirado en piedras/jefes, con nombre y arte original.

## Fase 1: Prototipo Jugable

La Fase 1 ya tiene una base Unity autocontenida. Al abrir el proyecto y presionar Play, se crea automaticamente una escena de prueba con:

- Jugador temporal con forma de capsula azul.
- Movimiento con WASD/flechas o joystick virtual.
- Camara tercera persona con giro usando click derecho.
- Boton ATK en pantalla para pruebas tactiles.
- Ataque basico con Space, click izquierdo o boton ATK.
- Enemigos rojos con IA simple.
- Barra de vida del jugador.
- Barra de vida del objetivo.
- Texto de dano flotante.

### Como Abrirlo En Unity

1. Espera a que Unity Hub termine de instalar el Editor.
2. En Unity Hub, abre la pestana Projects.
3. Presiona Add project from disk.
4. Selecciona esta carpeta: `/Users/migueltroncoso/Documents/Mmorpg`.
5. Abre el proyecto con Unity 6.5 o compatible.
6. Cuando Unity termine de importar, abre `Assets/Scenes/Prototype.unity` si no se abre sola.
7. Presiona Play.

### Controles

- Moverse: WASD, flechas o joystick virtual.
- Atacar: Space, click izquierdo o boton ATK.
- Girar camara: mantener click derecho y mover el mouse.
- Zoom: rueda del mouse.

## Fase 2: Vertical Slice Inicial

La Fase 2 temprana agrega sistemas de RPG encima del prototipo:

- Selector de clase en pantalla: Guerrero, Ninja, Chaman y Umbra.
- Stats diferentes por clase: vida, dano, velocidad, rango y cooldown.
- Dos habilidades por clase, activadas con Q/E o botones tactiles.
- Nivel, experiencia y oro.
- Loot basico al derrotar enemigos.
- Respawn automatico de enemigos.
- Armas visuales importadas desde KayKit CC0.

### Controles Nuevos

- Habilidad 1: Q o boton de habilidad izquierdo.
- Habilidad 2: E o boton de habilidad derecho.
- Cambiar clase: botones superiores en pantalla.

## Assets Externos

Se importo una seleccion pequena de armas del repositorio publico KayKit Character Pack: Adventurers.

- Fuente: https://github.com/KayKit-Game-Assets/KayKit-Character-Pack-Adventures-1.0
- Licencia: CC0 1.0 Universal.
- Registro local: `ASSET_LICENSES.md`.

## Fase 3: Online Local Inicial

La Fase 3 temprana agrega una primera capa online local:

- Servidor WebSocket en Node.js.
- Boton ONLINE dentro de Unity.
- Sincronizacion de posicion entre clientes conectados.
- Aparicion de otros jugadores como capsulas remotas.
- Nombre/clase visible sobre jugadores remotos.
- Chat local.

### Ejecutar Servidor

En una terminal:

```bash
cd /Users/migueltroncoso/Documents/Mmorpg/Server
npm install
npm start
```

El servidor queda escuchando en:

```text
ws://localhost:7777
```

### Probar En Unity

1. Inicia el servidor.
2. Abre el proyecto en Unity.
3. Presiona Play.
4. Deja la URL como `ws://localhost:7777`.
5. Presiona ONLINE.
6. Abre una segunda instancia/build para ver otro jugador conectado.

Para Android fisico, `localhost` apunta al telefono, no al Mac. En ese caso usa la IP local del Mac, por ejemplo:

```text
ws://192.168.1.14:7777
```

## Fase 4: Contenido Y Retencion Inicial

La Fase 4 agrega la primera vuelta de objetivos para que la sesion tenga algo mas que combate:

- Mision activa en pantalla: eliminar enemigos, reunir fragmentos y destruir el evento central.
- Inventario simple con resumen de objetos.
- Pociones consumibles.
- Mercader del Valle cerca del inicio.
- Compra de pociones con oro.
- Mejora de arma con oro y Mineral opaco.
- Refuerzo de armadura con oro y Anillo gastado.
- Monolito Corrupto al sur del mapa como evento de mundo con recompensa especial.

### Controles Nuevos De Fase 4

- POTION: consume una Pocion menor y cura vida.
- BUY 25: compra una Pocion menor si estas cerca del Mercader.
- WEAPON: mejora el dano del arma si tienes oro y Mineral opaco.
- ARMOR: aumenta la vida maxima si tienes oro y Anillo gastado.

El Monolito Corrupto aparece al sur del mapa y reaparece despues de ser destruido. Al completarlo junto con la mision, recibes EXP, oro y una Medalla del valle.

## Fase 5: Preparacion Android Inicial

La Fase 5 deja el proyecto encaminado para probar en celular:

- Nombre temporal del juego: Valle de las Reliquias.
- Bundle ID propuesto: `com.migueltroncoso.valledelasreliquias`.
- Version inicial: `0.1.0`.
- Pantalla de carga simple al iniciar.
- Objetivo de rendimiento runtime: 60 FPS.
- Herramientas de build en Unity: menu `MMORPG > Android`.
- UI con safe area para telefonos con notch.
- Paneles HUD con mejor contraste.
- Feed de actividad para loot, misiones, compras y mejoras.
- Generador de arte placeholder para icono/splash: menu `MMORPG > Branding`.
- Checklist de Android y Google Play en `docs/android-release-checklist.md`.

### Crear Build Android

Primero instala en Unity Hub el modulo Android para Unity 6.5:

- Android Build Support.
- Android SDK & NDK Tools.
- OpenJDK.

Luego abre el proyecto y usa:

```text
MMORPG > Android > Apply Android Settings
MMORPG > Android > Build Debug APK
MMORPG > Android > Build Google Play AAB
```

El APK sirve para probar en telefono. El AAB es el formato que se sube a Google Play.

Para generar placeholders visuales de marca:

```text
MMORPG > Branding > Generate Placeholder Store Art
```

Eso crea archivos temporales utiles en `Assets/Art/Generated`. No son arte final, solo una base para probar flujo de build y tienda.

## Fase 5.5: Creacion De Personaje MVP

Esta fase agrega el primer flujo de entrada del jugador:

- Panel inicial de creacion de personaje al presionar Play.
- Nombre editable de hasta 14 caracteres.
- Seleccion de clase: Guerrero, Ninja, Chaman y Umbra.
- Seleccion de sexo: Masculino o Femenino.
- Diferencia visual temporal por color, escala y etiqueta sobre el personaje.
- El mundo queda pausado hasta presionar CREAR.
- El nombre elegido se usa tambien en el cliente online local.

Esto sigue siendo un placeholder. Cuando tengamos modelos reales, este sistema se puede conservar y reemplazar solo la parte visual.

## Fase 5.6: Avatar Visual Procedural

Esta fase reemplaza la capsula generica por un avatar temporal armado con primitivas Unity:

- Silueta visual con torso, cabeza, brazos y acentos.
- Rasgos diferentes por clase:
  - Guerrero: hombreras y espada dorsal.
  - Ninja: mascara y dagas.
  - Chaman: orbe y baston espiritual.
  - Umbra: cresta oscura y nucleo de energia.
- Diferencia masculino/femenino sin afectar collider ni movimiento.
- Jugadores remotos tambien usan silueta por clase en el online local.

El objetivo no es arte final, sino tener lectura visual de clase/identidad hasta importar modelos definitivos con licencia segura.

## Fase 5.7: Persistencia Local MVP

El progreso ya no se pierde al cerrar el juego:

- Se guardan nombre, clase, sexo, nivel, experiencia, oro, inventario y niveles de mejora de arma/armadura.
- Guardado en JSON local (`Application.persistentDataPath/saves/player.json`) con escritura atomica y respaldo automatico del ultimo guardado valido.
- Autosave cada 30 segundos, al pausar la app (Android) y al cerrar.
- Si existe un guardado, el panel de creacion muestra un boton CONTINUAR con los datos previos precargados; CREAR empieza un personaje nuevo y sobreescribe el guardado.
- El almacenamiento es intercambiable (`ISaveStorage`) para migrar a servidor mas adelante.
- Limite de nivel configurado en 105.

Limitaciones actuales: el progreso de la mision activa y la posicion del jugador no se guardan todavia; la mision reinicia en cada sesion.

## Fase 5.8: Progresion 1-105 Con Tabla Configurable

La subida de nivel deja de usar la formula fija `nivel x 100`:

- Curva de experiencia configurable en un asset `ExpCurveConfig` (exp base, exponente, puntos de atributo por nivel, nivel maximo).
- Tabla `LevelProgressionTable` con las filas 1..105 generada desde la curva, nunca escrita a mano.
- Menu de editor `MMORPG > Progression > Generate Level Table` para crear/regenerar los assets en `Assets/Resources/Game/`.
- Si los assets no existen, el juego genera la tabla en runtime con los valores por defecto (no se rompe nada).
- La resolucion de EXP y multi-nivel es una funcion pura (`ExperienceResolver`), portable al servidor.
- Puntos de atributo por cada nivel ganado, visibles en el HUD y guardados en la partida (el gasto de puntos llega con la ventana de stats).
- Al llegar al nivel 105 el HUD muestra EXP MAX y la experiencia sobrante se descarta.

## Fase 5.9: Items, Rarezas Y Equipamiento

El inventario deja de ser una lista de textos:

- Items definidos como ScriptableObjects con ID estable, nombre, descripcion, categoria, rareza y precios (`ItemDefinition`, `ConsumableItemDefinition`, `EquipmentItemDefinition`).
- Base de datos central (`ItemDatabase`) con validacion de IDs duplicados.
- Rarezas Common a Mythic con colores centralizados en `RarityTable`; el inventario del HUD colorea cada item por rareza.
- Inventario basado en instancias (`ItemInstance` con GUID) preparado para anti-duplicacion en servidor.
- Equipamiento con slots (arma, casco, pechera, collar y mas) con requisitos de nivel y clase.
- Boton EQUIPAR: equipa lo mejor del inventario y muestra el motivo si algo no se puede equipar.
- 4 piezas iniciales: Espada de recluta, Casco de cuero (nivel 2), Pechera del guardia (nivel 3) y Amuleto del valle (nivel 4). Los enemigos pueden soltarlas como loot raro.
- La pocion cura segun su definicion de datos, no un valor fijo en codigo.
- Menu `MMORPG > Items > Generate Item Database` genera los assets; sin assets, el juego usa el set por defecto en runtime.
- Guardado esquema v3: inventario por IDs + equipo puesto. Los guardados anteriores migran automaticamente.

## Fase 5.10: Mejora +0..+15 Con Riesgo

Los botones WEAPON y ARMOR ahora mejoran la pieza equipada (la instancia real), al estilo de los MMORPG clasicos:

- Tabla de mejora configurable (`UpgradeConfig`): probabilidad de exito, costo en oro, cantidad de materiales y politica de fallo por cada paso +1..+15.
- Por defecto: +1 a +6 sin riesgo (solo se pierden materiales), +7 a +9 puede bajar un nivel, +10 a +15 puede destruir el objeto.
- Runa de proteccion (drop raro): se consume automaticamente en mejoras riesgosas y evita la perdida o destruccion.
- La rareza limita la mejora maxima (Common llega a +9; rarezas superiores a +15).
- La resolucion del intento es una funcion pura (`UpgradeResolver`), portable al servidor.
- El arma mejora dano por nivel y la armadura vida por nivel; los valores por nivel tambien son datos.
- Menu `MMORPG > Items > Generate Upgrade Config` genera el asset; sin asset hay valores por defecto en runtime.
- Guardado esquema v4: el nivel de mejora viaja con cada pieza equipada. Los guardados antiguos (contador global de arma/armadura) migran automaticamente.

## Fase 5.11: Combate Con Critico/Evasion Y Misiones Data-Driven

Combate (Etapa D):

- Calculo de dano centralizado en `DamageCalculator`, funcion pura con rolls inyectados: precision vs evasion, variacion ±15%, golpes criticos por clase y reduccion por defensa.
- Cada clase tiene critico, precision, evasion y defensa propios (el Ninja critea 18%, el Guerrero aguanta mas).
- Popups distintos: critico naranja con "!", fallo "Fallo"/"Esquivado". Los enemigos tambien pueden fallar y critear.
- Tabla de botin como asset (`LootTableConfig`): probabilidad global + entradas con peso, editable sin tocar codigo (menu `MMORPG > Items > Generate Loot Table`).
- Interfaces de combate: `IDamageable`, `IAttackable`, `ILootSource`, `IInteractable`.

Misiones (Etapa F):

- Misiones definidas como ScriptableObjects (`QuestDefinition`): titulo, dialogos, objetivos, recompensas y mision siguiente.
- Objetivos soportados: hablar con NPC, derrotar enemigos, recolectar items y destruir el evento de mundo.
- Cadena inicial de 4 misiones originales: Ecos del valle (hablar con el Mercader) > Campos inquietos (derrotar 6) > Energia residual (2 fragmentos) > El Monolito Corrupto.
- Boton HABLAR cerca del Mercader: muestra el dialogo de la mision activa y avanza los objetivos de conversacion.
- Recompensas entregadas por `RewardService`, punto unico para EXP, oro e items.
- Menu `MMORPG > Quests > Generate Quests` genera los assets; sin assets hay fallback runtime.
- Guardado esquema v5: mision activa, progreso de objetivos y misiones completadas se conservan al cerrar.

## Fase 5.12: Mascotas Y Monturas

Primeros companeros del jugador:

- Mascotas definidas como ScriptableObjects (`PetDefinition`): tipo (apoyo, recolectora, ofensiva, defensiva, experiencia), rareza y bonos pasivos.
- Zorro del valle: mascota inicial de experiencia (+10% EXP, +5% oro mientras esta invocada). Boton MASCOTA para invocar/guardar; te sigue con un visual procedural y etiqueta.
- Monturas definidas como ScriptableObjects (`MountDefinition`): categoria (caballo, lobo, tigre, criatura magica, dragon terrestre y voladora para el futuro), nivel requerido y multiplicador de velocidad.
- Caballo bayo: montura inicial (nivel 5, velocidad x1.6). Boton MONTURA para montar/desmontar; el multiplicador pasa por el recomputo central de stats.
- La mascota activa y la montura seleccionada se guardan (esquema v6).
- Menu `MMORPG > Companions > Generate Companions` genera los assets; sin assets hay fallback runtime.

## Fase 5.13: Generador De Variantes De Items

Herramienta de editor para poblar el equipo de niveles 1 a 105 sin crear items a mano:

- `ItemArchetype` (ScriptableObject): plantilla con slot, stats base en nivel 1, curva de crecimiento (exponente), rareza por conjunto visual de 10 niveles y formato de nombre.
- Ventana `MMORPG > Items > Item Variant Generator`: elige arquetipo, rango de niveles y paso, y genera los assets con IDs deterministas (`valley_sword_lv025`). Regenerar actualiza en lugar de duplicar.
- Las variantes se registran automaticamente en el `ItemDatabase` asset y se valida que no haya IDs duplicados.
- Incluye boton para crear un arquetipo de ejemplo (espada del valle).

## Fase 5.14: Zona 1 Completa

El campamento se convierte en la primera zona real (Etapa J):

- `ZoneDefinition` (ScriptableObject): nombre, rango de niveles, configuracion de elites y jefe de zona. Las zonas siguientes usaran el mismo molde.
- Cartel de zona en el campamento: Valle de las Reliquias, niveles 1-10.
- Herrero del Campamento (NPC nuevo): las mejoras WEAPON/ARMOR ahora se hacen cerca de el; el Mercader queda para pociones.
- Almacen del Campamento (NPC nuevo) + boton ALMACEN: deposita todos tus materiales y retiralos cuando quieras; el contenido se guarda.
- Area de elites al este: Acechadores del claro (mas vida, dano y defensa, mejor recompensa) con reaparicion propia.
- Jefe de zona al noroeste: el Coloso de las Reliquias, con drop garantizado y reaparicion lenta.
- Cadena de misiones extendida de 4 a 8: herrero > mejorar una pieza (nuevo objetivo UpgradeItem) > cazar 2 elites > derrotar al Coloso.
- Los objetivos de matar ahora filtran por tier (cualquiera / Elite / Boss).
- Guardado esquema v7 (incluye el almacen). Menu `MMORPG > World > Generate Zones`.

## Fase 5.15: Identidad Completa En Red E Intenciones

Cierre de la hoja de ruta inicial (Etapa K):

- El sexo del personaje ahora se sincroniza por red: los jugadores remotos se ven con su clase y sexo reales (avatar procedural y etiqueta), no mas masculino por defecto.
- El hello se reenvia automaticamente al cambiar nombre, clase o sexo.
- Capa de intenciones: las acciones criticas (subir de nivel, mejora exitosa) se reportan al servidor como mensajes `action`; el servidor las difunde como actividad y los demas jugadores las ven en el chat ("Valle: X subio a nivel 5"). Es la base para que el servidor valide estas acciones cuando tenga autoridad.
- El servidor sanitiza y valida el sexo recibido (solo Masculino/Femenino).

## Fase 5.16: Ventana De Atributos

Los puntos de atributo por fin se pueden gastar:

- Boton STATS abre la ventana de atributos: Fuerza (+dano), Vitalidad (+vida maxima) y Agilidad (+velocidad y +critico).
- Cuanto aporta cada punto es configurable en un asset `AttributeConfig` (menu `MMORPG > Progression > Generate Attribute Config`).
- Los bonos pasan por el recomputo central de stats junto con equipo, mejoras y montura; el critico extra entra al calculo de dano.
- Los atributos gastados se guardan (esquema v8).

## Fase 5.17: Posicion Persistente

- La posicion y orientacion del jugador se guardan con el resto del estado (esquema v9).
- Al CONTINUAR reapareces exactamente donde cerraste el juego, no en el punto inicial.

## Fase 5.18: Textos Con Claves i18n

Cumple la regla del contrato: los mensajes del sistema ya no son literales en el codigo.

- `Localization.Tr(clave)` resuelve todos los mensajes de progresion, mejora, equipo, inventario, mascotas, monturas, almacen, NPCs, misiones, combate y HUD (~85 claves).
- La tabla espanola vive en un asset `LocalizationTable` (menu `MMORPG > Localization > Generate Localization`); sin asset, fallback runtime con la tabla por defecto.
- Si falta una clave, se muestra la clave misma para que el hueco sea evidente.
- Agregar otro idioma = crear otra tabla; el contenido de datos (nombres de items, dialogos de misiones) sigue en sus ScriptableObjects.
- Pendiente de migrar: textos del panel de creacion de personaje y habilidades (estan en el bootstrap/PlayerSkills).

## Fase 5.19: Zona 2 - Bosque De Los Susurros

Segunda region jugable, construida 100% con los moldes de la Zona 1:

- `ZoneDefinition` ahora tambien describe a los enemigos normales, el terreno y el cartel: el spawner es completamente data-driven y se crea una instancia por zona.
- Bosque de los Susurros (niveles 11-20) al norte del campamento: terreno propio, Espinosos del bosque, dos Sombras del bosque (elites) y el Anciano de Espinas como jefe de zona (2200 de vida, drop garantizado).
- Los enemigos llevan un id propio (`forest_elite`, `valley_boss`...) y los objetivos de matar pueden filtrar por tier o por id: las misiones del bosque no se completan cazando en el valle.
- Cadena de misiones extendida de 8 a 10: Ecos del bosque (3 Sombras) y El corazon del bosque (el Anciano).
- Crear la Zona 3 en adelante = un asset mas de `ZoneDefinition` (menu `MMORPG > World > Generate Zones`).

## Fase 5.20: Autoridad De Servidor Sobre Intenciones

- El servidor valida las intenciones antes de difundirlas: level_up debe subir el nivel registrado (sincronizado en el hello, tope 105) y upgrade debe estar entre +1 y +15, con limite de ritmo de 800 ms por jugador.
- Las intenciones invalidas ya no se difunden: vuelven solo al emisor como `actionRejected` y el cliente las muestra en el chat.
- El nivel viaja con la identidad y se ve en la etiqueta de los jugadores remotos (Nv X).

## Fase 5.21: Zona 3 - Colinas Cenicientas

- Tercera region (niveles 21-30) al norte del bosque, construida SOLO con datos: Cenicientos errantes, Devoradores de ceniza (elites) y el Corazon de Ceniza como jefe.
- Cadena de misiones extendida de 10 a 13.
- Cero codigo nuevo: los moldes de zona absorbieron la region completa.

## Fase 5.22: Pipeline De Modelos 3D Para El Avatar

- Cada clase define su modelo real (`CharacterModelResource`): Guerrero=Knight, Ninja=Rogue, Chaman=Mage, Umbra=Barbarian del pack CC0 KayKit Adventurers.
- `PlayerAvatarVisual` carga el modelo desde Resources si existe; si no, usa el avatar procedural de siempre. Jugadores remotos incluidos, sin tocar mas codigo.
- Los cuatro FBX y sus texturas ya estan importados en `Assets/Resources/ThirdParty/KayKit/Adventurers/Characters/`. Ver `ASSET_LICENSES.md`.
- Si los modelos traen `Animator` y parametros `Speed`/`Attack`, el puente de animacion puede alimentarlos.

## Fase 5.23: Animacion Procedural Del Avatar

- Nuevo `AvatarMotionAnimator`: idle con respiracion, caminar con bob/swing y golpe con impulso corto.
- El fallback procedural ya no queda rigido: brazos y cuerpo reaccionan al movimiento y a ataques/habilidades.
- `PlayerAvatarVisual` evita reconstruir el avatar en cada snapshot de red si clase/sexo no cambiaron.
- Si en el futuro un modelo real incluye un `Animator` con parametros `Speed` y trigger `Attack`, el mismo componente los actualiza sin cambiar gameplay.
- El golpe basico y las habilidades Q/E disparan la animacion de ataque.

## Fase 5.24: StatSheet Con Modificadores Por Origen

- Nuevo `StatSheet`: base por clase + modificadores separados por origen (`Equipment`, `Attributes`, `Mount`, `Buff`).
- Nuevo `PlayerStatSheet`: expone dano, vida, defensa, velocidad, rango, cooldown, critico, precision y evasion ya resueltos.
- `EquipmentUpgradeSystem.ApplyBonuses` sigue siendo el punto unico de recomputo, pero ahora alimenta la hoja en vez de sumar todo a mano.
- Combate y HUD leen el dano total desde `PlayerStatSheet`; los buffs de habilidades ya no sobrescriben el dano base.
- La montura queda modelada como modificador de velocidad dentro de la hoja, lista para balance futuro.

## Fase 5.25: i18n De Creacion Y Habilidades

- `Localization.Initialize` ahora mezcla la tabla por defecto con el asset generado: si el asset esta desactualizado, las claves nuevas siguen resolviendo.
- El panel de creacion usa claves i18n para titulo, subtitulo, placeholder, botones, preview, genero y mensajes de crear/continuar.
- `PlayerSkills` usa claves i18n para recargas, impactos, curacion, marca, buffs y nombres internos de efectos.
- Las etiquetas de genero del jugador local y remoto salen de `Localization`.
- Textos iniciales del HUD/ayuda tambien pasan por claves.

## Fase 5.26: Zona 4 - Cumbres De Cristal

- Cuarta region (niveles 31-40) al norte de las Colinas Cenicientas, construida solo con `ZoneDefinition`.
- Nuevos enemigos: Centinelas de cristal, Custodios prismales (elites) y el Oraculo Fragmentado como jefe de zona.
- Cadena de misiones extendida de 13 a 16: limpiar centinelas, derrotar custodios y apagar al Oraculo.
- Cero sistemas nuevos: el spawner, objetivos filtrados por id, elites y jefe reutilizan los moldes existentes.

## Fase 5.27: Persistencia Basica De Servidor

- El cliente envia `playerKey` en `hello` para identificar de forma estable al personaje online.
- El servidor guarda estado online en `Server/data/players.json` con escritura atomica.
- Se persisten nombre, clase, sexo, nivel, posicion, rotacion y ultima mejora validada.
- Al reconectar, el servidor restaura lo guardado y conserva el mayor nivel entre estado local y estado del servidor.
- `Server/data/players.json` queda fuera de git; inventario/equipo completo/misiones siguen en el guardado local Unity.

## Fase 5.28: Puente Para Animaciones Reales

- `AvatarMotionAnimator` ahora soporta dos estilos de Animator real:
  - Parametros `Speed` (float) y `Attack` (trigger).
  - Estados simples `Idle`, `Run` y `Attack` en la Base Layer, aunque no existan parametros.
- El fallback procedural sigue funcionando si no hay modelo/Animator real.
- El generador `MMORPG > Characters > Generate KayKit Controllers` crea controladores desde los clips incluidos en cada FBX. Los controladores se guardan en Resources y se conectan automaticamente por clase.

## Fase 5.29: Pasada Final i18n Visible

- Estados de red, chat, botones utilitarios, labels de NPCs, carteles de zona, splash y evento del monolito usan claves i18n.
- Los botones principales (`POTION`, `WEAPON`, `MASCOTA`, `ONLINE`, etc.) ya no estan como literales sueltos en el bootstrap.
- Quedan como contenido de datos los nombres/dialogos de zonas, enemigos y misiones.

## Fase 5.30: Zonas 5-10 Hasta Nivel 105

- Se agregaron zonas data-driven para cubrir toda la progresion actual:
  - 41-50 Paso Glacial.
  - 51-60 Ruinas Sumergidas.
  - 61-70 Forja Obsidiana.
  - 71-80 Jardin Astral.
  - 81-90 Santuario del Eclipse.
  - 91-105 Trono del Vacio.
- Cada zona trae enemigos normales, elites, jefe, recompensas y balance base.
- La cadena principal de misiones crece de 16 a 34 y termina con el Rey Sin Alba.

## Fase 5.31: Previews Visuales

- `docs/previews/world-progression.svg`: mapa conceptual de progresion 1-105.
- `docs/previews/avatar-and-ui-preview.svg`: preview de clases, avatar procedural, puente de animaciones e i18n.

## Fase 5.32: Balance Inicial Y Feedback De Combate

- Enemigos normales, elites y jefes ahora tienen ritmo distinto: cooldowns y windup por tier.
- Los ataques enemigos avisan antes de impactar con popup `!` y pulso rojo cerca del jugador.
- Si el jugador sale del rango durante el windup, el golpe se cancela y el enemigo reintenta luego.
- El calculo defensivo contra el jugador usa `PlayerStatSheet` cuando existe, no solo la clase base.
- Jugador, enemigos y monolito tienen flash breve al recibir dano.
- Los criticos usan popup mas grande para leerse mejor en combate.

## Fase 5.33: Telemetria Y Persistencia Servidor Completa

- `CombatTelemetry` mide kills, muertes, dano dado/recibido y tiempo promedio para matar por zona.
- La telemetria se guarda localmente en `Application.persistentDataPath/telemetry/combat-telemetry.json`.
- Si el cliente esta online, envia snapshots de telemetria al servidor con el mensaje `telemetry`.
- El cliente envia `saveState` con el JSON completo de `PlayerSaveData`: identidad, clase, sexo, nivel, EXP, oro, atributos, inventario, equipo, misiones, mascotas, montura, almacen y posicion.
- El servidor guarda el snapshot completo en `Server/data/players.json` por `playerKey` y lo devuelve al reconectar como `savedState`.
- Si el guardado remoto esta mas avanzado, el cliente lo aplica; si el local esta mejor, conserva el local y lo vuelve a empujar al servidor.

## Fase 5.34: Panel De Telemetria Y Balance 41-105

- El boton `DATOS` abre un panel dentro del juego con kills, muertes, dano dado/recibido y ruta del JSON local.
- La telemetria separa el tiempo medio para derrotar enemigos normales, elites y jefes por zona.
- `ZoneDefinition` expone objetivos TTK configurables: normal 3-8 s, elite 8-18 s y jefe 30-75 s.
- Las zonas 5-10 recibieron una primera calibracion de vida, dano, EXP y oro para evitar saltos excesivos entre rangos.
- Los estados `RAPIDO`, `LENTO` y `OK` son orientativos hasta reunir sesiones reales de juego.

## Fase 5.35: Menu Mobile De Personaje

- El boton `MENU` abre pestañas separadas para Inventario, Equipo y Mision.
- Inventario muestra todos los objetos y cantidades; Equipo muestra slots, mejoras y bonos principales.
- Mision muestra objetivos con progreso y recompensa de la mision activa.
- El contenido se actualiza por eventos del HUD cuando cambia el estado del jugador.

## Fase 5.36: Resolucion, Modelos Y Feedback Audiovisual

- `ResponsivePanelScaler` mantiene ventanas modales dentro del safe area en resoluciones pequenas, portrait y landscape.
- Los modelos KayKit CC0 Knight, Rogue, Mage y Barbarian ya estan importados con sus texturas.
- `MMORPG > Characters > Generate KayKit Controllers` genera controladores `Idle`, `Run` y `Attack` desde los clips del FBX.
- Audio procedural ligero para ataque, golpe, critico, dano, habilidad y subida de nivel.
- Particulas de impacto, habilidades y nivel con material compartido y bajo costo.
- La prueba visual en dispositivo Android real sigue siendo necesaria antes de cerrar la UX movil.

## Fase 5.37: Variantes Femeninas, Audio Final Y Diagnostico Movil

- La Ninja femenina usa la variante real `RogueHooded.fbx`; las demas clases conservan su modelo KayKit y reciben silueta/armadura visual diferenciada por clase y sexo.
- `MobileRuntimeDiagnostics` registra resolucion, orientacion, safe area y volumen SFX/musica al probar en Android.
- Los SFX cargan primero clips CC0 de Kenney (`knifeSlice`, `metalPot`, `bookOpen`, `handleCoins`); los tonos procedurales quedan como fallback.
- La musica usa un clip importado de Kenney y mantiene ambiente procedural solo cuando el asset no esta disponible.
- La seleccion de audio esta documentada en `ASSET_LICENSES.md`.

## Fase 5.38: Prueba Movil, Modelos Femeninos Y Equipo Visual

- El boton `TEST` abre una ventana de diagnostico con resolucion, orientacion,
  safe area y controles de musica/SFX para probar Android sin tocar el codigo.
- Guerrero, Chaman y Umbra femeninos usan el FBX CC0 `AnimatedWoman.fbx` de
  Quaternius; Ninja mantiene `RogueHooded.fbx`. Las clases se diferencian por
  color, armadura y arma, con fallback procedural si falta un asset.
- `EquipmentItemDefinition.VisualId` conecta piezas equipables con su
  apariencia. Casco, pechera, amuleto y espada ya muestran componentes
  visuales al equiparse.
- `PlayerEquipment.Changed` y `EquipmentVisualController` actualizan la
  apariencia despues de equipar, desequipar, destruir o restaurar una pieza.
- Los bonos siguen calculandose exclusivamente en
  `EquipmentUpgradeSystem.ApplyBonuses`.

La interfaz `VisualId` queda preparada para que los futuros conjuntos de
armadura no queden acoplados a la logica de stats.

## Fase 5.39: Sets Modulares Y Preparacion Android

- `ArmorVisualSetDefinition` define ocho perfiles clase/sexo con estilo,
  colores, silueta y piezas distintivas para Guerrero, Ninja, Chaman y Umbra.
- `DefaultArmorVisualSets` mantiene un fallback runtime reutilizable, listo
  para reemplazarse por assets ScriptableObject sin cambiar el avatar.
- El flujo `MMORPG > Android > Apply Android Settings` configura identificador,
  ARM64, SDK minimo 26, target SDK 35, landscape y 60 FPS en dispositivo.
- Android Build Support ya esta instalado y la build debug se genero
  correctamente en ARM64.
- El diagnostico movil ahora incluye modelo del dispositivo, DPI y FPS objetivo.

La prueba fisica queda pendiente de conectar un dispositivo Android por ADB.

## Fase 5.40: APK Android Debug

- APK generada: `Builds/Android/valle-reliquias-debug.apk`.
- Paquete: `com.migueltroncoso.valledelasreliquias`.
- Version: `0.1.0`, codigo 1.
- Validada con `minSdk 26`, `targetSdk 35`, ARM64 y permiso de Internet.
- El SDK de Unity incluye `platform-tools/adb`; no hay telefono conectado
  todavia para completar la instalacion y la prueba TEST.

Proxima accion: conectar el telefono con depuracion USB activada y ejecutar
`adb install -r Builds/Android/valle-reliquias-debug.apk` usando el adb del
SDK de Unity si el comando global no existe.

## Fase 5.41: Correcciones De QA Movil

- La APK de prueba ya no usa `BuildOptions.Development`, evitando la consola
  de Unity que cubria el joystick en la parte inferior.
- Se agrego una superficie de colision explicita para el campo de entrenamiento.
- El jugador se recupera en el punto seguro si una posicion guardada lo deja
  bajo el mapa; los guardados antiguos tambien corrigen la altura minima.
- La UI Android usa una referencia mas adecuada para landscape y la pantalla
  de creacion escala dentro del safe area.
- La camara usa un color de fondo controlado para evitar el gris plano durante
  la carga del mundo.

La build corregida queda en `Builds/Android/valle-reliquias-debug.apk` y debe
reinstalarse antes de repetir la prueba tactil.

## Fase 5.42: Entrada Multitactil Y Combate Movil

- En Android el ataque tactil queda reservado al boton `ATK`; el toque del
  joystick ya no se interpreta como clic de mouse ni dispara ataques por error.
- El joystick conserva el `pointerId` del dedo que lo activo y no se limpia
  cuando entra o sale un segundo dedo sobre otro control.
- La APK debe reinstalarse y probarse con movimiento sostenido mas ataque,
  ataque repetido y cambio de objetivo.

La prueba multitactil sigue pendiente en telefono real. Las siguientes fases
son reorganizar la interfaz movil, importar mobs 3D finales para la Zona 1 y
pulir terreno y ambientacion de las primeras zonas.

## Fase 5.43: Mobs Y Lectura De Combate

- Los mobs ya no nacen como capsulas: ahora usan familias visuales
  procedurales de bestia, guardian, espiritu y sombra segun su `EnemyId`.
- Los normales, elites y jefes tienen escala, color, adorno y lectura de tier
  diferenciados sin tocar las reglas de vida, dano, loot o IA.
- Cada enemigo muestra su nombre y una barra de vida sobre la cabeza; la barra
  se actualiza con el mismo componente `Health` usado por el combate.
- La implementacion no agrega dependencias externas y queda lista para cambiar
  cada familia por un prefab o modelo final mas adelante.

La APK actual incluye esta fase y debe reinstalarse para verla en Android.

## Fase 5.44: Interfaz De Juego Mobile

- El HUD principal ahora prioriza vida, objetivo, estado, clase, progresion y
  mision; inventario y equipo dejan de ocupar espacio permanente en pantalla.
- Los botones secundarios se agrupan bajo `MAS` en Android y `MENU` permanece
  visible para abrir inventario, equipo y misiones.
- El menu ahora ofrece accesos directos a `STATS` y `DATOS`, sin obligar a
  abandonar el flujo de combate para consultar progresion y telemetria.
- La escala landscape, safe area, chat y zona de combate quedan separadas para
  reducir paneles superpuestos durante la exploracion.
- El input multitactil conserva el movimiento mientras se pulsa `ATK`: el
  ataque responde al `pointer down` y no fuerza la rotacion si el joystick esta
  entregando una direccion.
- En Android, arrastrar una zona libre del mundo gira la camara en horizontal
  y vertical; la superficie tactil queda detras del joystick, acciones y
  ventanas para que cada control conserve su propio toque.

## Fase 5.45: Primera Capa De Mobs 3D

- Los mobs de las zonas 1-10 ya cargan modelos reales animados de Quaternius
  Ultimate Monsters segun zona y tier: bestias, orcos, demonios, gólems, yetis,
  criaturas abisales, fantasmas y dragones.
- El pack importado queda en
  `Assets/Resources/ThirdParty/Quaternius/UltimateMonsters/` con licencia CC0
  local y un controlador por modelo con estados `Idle`, `Run` y `Attack`.
- Si falta un modelo, `EnemyVisualController` conserva las siluetas
  procedurales, por lo que el spawner y el combate no dependen del arte.
- La Zona 1 usa una criatura procedural propia de tipo bestia de reliquia para
  normal, elite y jefe cuando el asset real no se encuentre, con collar,
  hombreras, placa y cuernos por tier.
- Las diez familias de zona conservan sus adornos de identidad para elite/jefe:
  espinas del bosque, brasa, prismas, aletas abisales, forja, halos astrales y
  mantos del vacio.
- Los diez jefes de zona usan ahora un modelo Quaternius distinto: Orc Skull,
  Yeti, Blue Demon, Dragon, Goleling Evolved, Fish, Dragon Evolved, Ghost Skull,
  Demon y Orc, en orden de progresion 1-105.
- Cada jefe recibe una silueta adicional propia (reliquia, raices, brasa,
  cristales, hielo, abismo, forja, astral, eclipse o vacio) y un tinte de zona
  aplicado con `MaterialPropertyBlock`, sin duplicar los materiales importados.
- La proxima pasada queda reservada para validar escala/lectura de los jefes en
  Android y reemplazar progresivamente el set CC0 por meshes finales propios.

## Fase 5.46: Ambientacion De Todas Las Zonas

- Se genero ambientacion determinista para las zonas 1 a 10, cubriendo nivel
  1 a 105: arboles, cristales, ruinas, jardines astrales y obeliscos segun el
  `ZoneId`.
- El sistema usa decoracion mobile-friendly sin colision para no bloquear el
  movimiento y mantiene el molde data-driven de `ZoneDefinition`.
- La ambientacion final con assets de terreno, caminos, obstaculos y puntos de
  interes queda pendiente; ya se puede recorrer visualmente toda la progresion.

## Fase 5.47: Animacion Y Feedback De Combate

- Los enemigos con modelo usan `AvatarMotionAnimator` para idle, movimiento y
  ataque; los ataques enemigos disparan una animacion al comenzar el windup.
- El windup muestra telegraph visual, los impactos conservan dano flotante y
  VFX, y la muerte reduce visualmente el enemigo antes del respawn.
- La logica de dano, loot, misiones y telemetria no cambia.

La APK de estas cuatro mejoras debe reinstalarse para revisar el salto visual.

## Fase 5.48: Balance Y Rendimiento Android

- Los `EnemySpawner` lejanos permanecen sin enemigos hasta que el jugador
  entra en un radio de 112 unidades; al salir limpian sus instancias.
- Android reduce decoracion por zona, sombras, luces, antialiasing y reflejos
  para mantener una base de 60 FPS sin eliminar ninguna zona del mundo.
- Los materiales de mobs y decoracion se reutilizan por color para reducir
  instanciaciones durante la carga.
- La ventana `TEST` ahora muestra FPS actual/promedio/minimo, memoria asignada,
  tiempo de sesion, enemigos activos y puntos de interes activos.

La validacion de FPS, memoria, TTK y recompensas en telefono real queda como
la siguiente prueba de balance.

## Fase 5.49: Progresion Visual Y Balance Por Zona

- Cada `EnemyId` normal tiene una variante visual propia: reliquia, espinas,
  ceniza, cristal, escarcha, abisal, magma, astral, eclipse o vacio.
- Cada zona tiene camino principal, entrada, landmarks de elite y jefe, y
  marcadores de limite desde nivel 1 hasta 105.
- Los cuatro obstaculos de cada zona tienen colision real y quedan fuera del
  camino principal; `EXPLORAR` muestra el estado del landmark mas cercano.
- `EXPLORAR` entrega una recompensa de EXP/oro una vez por punto y personaje
  para la entrada, los landmarks de elite y el jefe; el campamento seguro
  informa su estado sin recompensa de combate.
- Los reclamos se guardan en `PlayerSaveData` esquema 10 y viajan dentro del
  snapshot `saveState`, por lo que cerrar/reabrir o reconectar no duplica la
  recompensa del mismo personaje.
- Los datos de vida, dano, EXP y oro siguen siendo los de `ZoneDefinition`;
  la recompensa exploratoria usa `RewardService` y no altera el combate.
- `ZoneBalanceResolver` registra TTK estimado, dano base esperado y si cada
  tier cae dentro de su objetivo; falta cerrar el ajuste con telemetria real.

La validacion de reclamos en telefono y la autoridad definitiva del servidor
para recompensas siguen pendientes para una fase online posterior.

## Fase 5.50: Zona Segura De Comercio

- El Valle de las Reliquias tiene un campamento seguro alrededor del mercader,
  herrero y almacen.
- Los spawns normales se filtran para no aparecer dentro del campamento.
- Los enemigos no pueden perseguir ni atacar dentro de la zona segura y el
  jugador tampoco puede iniciar combate alli.
- La zona queda marcada visualmente y `EXPLORAR` informa que es un espacio sin
  combate.
- El Monolito Corrupto queda fuera del campamento para conservar el evento.

## Fase 5.51: Recuperacion Y Minimap Mobile

- El jugador espera cuatro segundos desde el ultimo dano antes de comenzar a
  recuperar salud automaticamente.
- La curacion se entrega de forma fraccionada y progresiva, con un ritmo
  ligeramente mayor mientras el jugador se desplaza.
- El minimapa es un radar 2D liviano centrado en el jugador: muestra mobs
  normales en rojo, elites en violeta, jefes en naranja y puntos de interes.
- El radar reutiliza marcadores y se actualiza por intervalos para conservar
  rendimiento Android; no crea una segunda camara 3D.
- El ritmo de regeneracion, el radio de lectura y la escala visual quedan para
  validacion con el telefono real.

## Fase 5.52: Progresion Real De Loot 1-105

- `ProgressionItemCatalog` crea una progresion determinista para las diez
  bandas: Valle 1-10, Bosque 11-20, Ceniza 21-30, Cristal 31-40, Escarcha
  41-50, Abismo 51-60, Obsidiana 61-70, Astral 71-80, Eclipse 81-90 y Vacio
  91-105.
- Cada banda tiene dos materiales propios: fragmento comun para drops normales
  y nucleo de refinamiento para elites/jefes. Los equipos nuevos apuntan al
  nucleo de su propia zona al mejorar.
- Cada zona tiene doce piezas de equipo (arma, casco, pechera, guantes,
  pantalones, botas, collar, capa, dos anillos, brazalete y cinturon) con nivel
  requerido, rareza, estadisticas y precios escalados; ademas cada jefe entrega
  una reliquia exclusiva de tipo talisman.
- `ZoneLootProgression` separa tablas para normal, elite y jefe. Los normales
  dan materiales comunes, los elites refinamiento y piezas del set, y los
  jefes combinan su reliquia garantizada con un segundo drop adicional.
- Las probabilidades suben por banda de forma progresiva: las zonas iniciales
  conservan escasez y las avanzadas priorizan refinamiento y equipo sin llegar
  a drop garantizado.
- `ZoneBalanceResolver` calcula el TTK usando el arma esperada de la banda y
  registra daño esperado, aporte del equipo y TTK normal/elite/jefe.
- Los equipos antiguos siguen siendo validos para guardados y tutoriales. La
  siguiente calibracion debe usar sesiones reales para ajustar oro, precios,
  probabilidades, clases permitidas y bonus de conjunto.

## Fase 5.53: Acceso Mobile Y Seleccion De Personaje

- El arranque muestra una identidad visual mas cuidada para Valle de las
  Reliquias antes de entrar al mundo.
- Si existe un guardado local, aparece una tarjeta con personaje, clase, sexo,
  nivel, EXP y oro junto a `ENTRAR AL VALLE`.
- `CREAR NUEVO PERSONAJE` abre el flujo de clase, sexo y nombre; `VOLVER`
  regresa a la tarjeta guardada sin perder el guardado.
- La pantalla muestra perfiles de servidor: `S-01 Valle Central` esta activo
  para la prueba local (`ws://localhost:7777`), mientras S-02 y S-03 quedan
  bloqueados hasta implementar instancias reales.
- La APK debe reinstalarse para validar safe area, lectura, teclado y flujo de
  acceso en el telefono Android.

## Fase 5.54: Preparacion Online Para Prueba Compartida

- Los perfiles de servidor viven en `ServerProfile.cs`: cada uno tiene ID,
  nombre, URL y disponibilidad.
- En la pantalla de acceso se puede introducir la direccion real de S-01,
  por ejemplo `ws://192.168.1.20:7777`, para que un telefono se conecte al
  servidor del Mac/PC dentro de la misma red.
- `ENTRAR AL VALLE` conecta automaticamente y el estado queda visible. La
  URL y el perfil se recuerdan como configuracion del dispositivo.
- El servidor se puede iniciar con identidad y limites propios:

```bash
cd Server
npm install
SERVER_ID=S-01 SERVER_NAME="Valle Central" HOST=0.0.0.0 PORT=7777 MAX_PLAYERS=100 npm start
```

- Para amigos fuera de la red local falta desplegar el servidor con dominio,
  WSS/TLS, firewall y persistencia protegida. S-02 y S-03 no se habilitan hasta
  tener instancias independientes.

## Fase 5.55: Cosméticos, Compañeros Y Eventos Diarios

- El avatar tiene slots separados para atuendo y alas. El primer atuendo es
  inicial; las alas y atuendos avanzados se compran con oro o se desbloquean
  mediante recompensas especiales.
- Los cosmeticos aportan daño, vida y critico a traves de `PlayerStatSheet`.
  El boton `TIENDA` compra la siguiente oferta y `ATUENDO`/`ALAS` alternan su
  visual en el personaje.
- Mascotas y monturas mantienen propiedad y rareza. El zorro y el caballo son
  de inicio; el zorro de brasa y el lobo de tormenta quedan reservados para
  contenido dificil y futuras misiones.
- `Caceria de Reliquias` se activa cada dia, cuenta cinco derrotas y entrega
  EXP, oro y una posibilidad de progresion hacia Alas de brasa. Esta primera
  version es local; la recompensa final debe validarse en el servidor.
- APK validada despues de esta fase en:
  `Builds/Android/valle-reliquias-debug.apk`.

## Fase 5.56: Pasada Visual 3D

- Se centralizaron los materiales 3D en `VisualMaterialUtility` para controlar
  color, brillo, metal y emision sin crear un material diferente por objeto.
- El mundo usa iluminacion mas calida y niebla lineal para dar profundidad;
  la camara mantiene HDR desactivado para cuidar Android.
- Las runas, cristales, nucleos, anillos, perlas y monolitos ahora tienen
  acentos emisivos; los mobs, jefes, avatares, NPC, mascotas y monturas reciben
  una respuesta de luz mas coherente.
- Cada zona recibe acentos de suelo y fragmentos con una paleta propia para
  diferenciar mejor las bandas del nivel 1 al 105.
- La validacion visual final en el telefono queda pendiente para ajustar
  contraste, brillo, FPS y memoria por dispositivo.

## Fase 5.58: Habilidades Progresivas

- Cada clase tiene cuatro habilidades propias en `Q`, `E`, `R` y `F`. `Q` y `E`
  estan disponibles desde nivel 1; `R` se desbloquea en nivel 8 y `F` en nivel
  20.
- Las habilidades tienen niveles 1-5. Cada mejora aumenta dano o curacion y
  reduce el enfriamiento de forma progresiva.
- El `Manual de habilidades` es un material raro. La primera mision entrega uno
  para probar la mejora y los enemigos elite/jefe pueden soltar mas.
- Los botones `+` del HUD consumen manuales y guardan el nivel en
  `PlayerSaveData` esquema 12. Las ranuras bloqueadas indican el nivel
  necesario.
- APK validada despues de esta fase en:
  `Builds/Android/valle-reliquias-debug.apk`.

## Fase 5.59: Habilidad Final Y Equipo Visible

- La ranura `G` contiene una habilidad final propia para cada clase. Se
  desbloquea automaticamente en nivel 50 y puede subir de nivel 1 a 10.
- La habilidad final tiene un cooldown de 30 minutos medido con hora UTC y
  persistido en `PlayerSaveData` esquema 13. El cierre de la app no lo reinicia.
- Guerrero, Ninja, Chaman y Umbra tienen efectos finales distintos: area masiva,
  ejecucion veloz, juicio magico o eclipse de sombra.
- Cada banda 1-105 tiene doce piezas visuales de equipo y el avatar muestra
  rareza/tier en armas, armadura y accesorios. Cada clase lleva un emblema 3D.
- Las diez zonas reciben landmarks de identidad: reliquia, bosque antiguo,
  obelisco de ceniza, cristales, portal glacial, ruinas sumergidas, forja,
  jardin astral, altar del eclipse y trono del vacio.

## Fase 5.60: Preflight De Hetzner

- `Server/src/server.js` expone `/health`, limita mensajes WebSocket a 64 KB,
  mantiene heartbeat y persiste al recibir senales de apagado.
- `Server/README.md` incluye la ruta recomendada para Ubuntu + systemd + Nginx
  + WSS, firewall y backups.
- Falta ejecutar el despliegue en la cuenta Hetzner, configurar dominio y
  certificado, y probar dos telefonos desde Internet.

## Fase 5.61: Cierre De Contenido Funcional 1-105

- Se corrigio la cadena de misiones para que el Bosque de los Susurros tenga
  una mision normal antes de sus objetivos elite y jefe.
- El contenido funcional queda cubierto por 10 zonas, 35 misiones, 162 items,
  30 tablas de loot, 120 piezas de set y 10 reliquias de jefe.
- El auditor verifica rangos 1-105, mobs, TTK, recompensas, materiales,
  habilidades, eventos diarios y recursos 3D masculinos/femeninos.
- Ejecutar `MMORPG > QA > Generate Content Completeness Report` para regenerar
  `docs/content-completeness.md`; el corte 5.62 pasa 13/13 checks.
- Esto marca el contenido funcional al 100%, pero el arte comercial final,
  online autoritativo, optimizacion y publicacion siguen pendientes.

## Fase 5.62: Longevidad Y Renacimiento

Las 35 misiones principales se mantienen como una campana clara de nivel
1-105. Para que el juego continue despues de completarla se agregaron:

- Tres contratos diarios rotativos por banda: caceria normal, caceria elite y
  recoleccion de materiales de mejora.
- Progreso visible, recompensas de EXP/oro/items y guardado local en esquema
  14.
- Renacimiento al nivel 105, con boton `RENACER` dentro de `MAS`.
- El Renacimiento reinicia nivel, EXP, atributos y campana, pero conserva
  equipo, inventario, cosmeticos, mascotas, monturas y habilidades.
- Cada Renacimiento suma Renombre y un 2% permanente de EXP. Esto permite
  repetir la campana y seguir progresando sin crear niveles infinitos desde
  el primer lanzamiento.

La fecha diaria usa UTC para que cliente y futuro servidor compartan el mismo
corte. Antes de abrir el juego se debe validar contratos, recompensas y
Renacimientos desde S-01 para evitar manipulacion local.

## Fase 5.63: Eventos Semanales Y Temporadas

- `Conquista del Valle` se reinicia cada lunes UTC y pide 30 derrotas, con al
  menos 3 enemigos elite. Entrega EXP, oro, runas y manuales.
- La temporada actual dura 28 dias y tiene 30 niveles. La XP llega por
  derrotar enemigos, completar misiones, contratos y el evento semanal.
- Los niveles 5/10/15/20/25/30 entregan recompensas una sola vez. El nivel 30
  desbloquea el `Atuendo del eclipse`.
- El progreso semanal y de temporada se guarda en `PlayerSaveData` esquema 15
  y aparece dentro del menu `Mision`.

Esta primera version es local para probar el ritmo. Antes de publicar, S-01
debe convertirse en la autoridad de fechas, progreso, recompensas y cambios de
temporada.

## Fase 5.64: Calendario Diario Y Eventos Competitivos

- El calendario UTC rota siete actividades: elites, forja, reliquias, tesoro,
  guerra de clanes y todos contra todos, además de la Conquista semanal.
- El jefe mundial aparece en el calendario todos los días a una hora UTC
  variable y tiene una ventana de 60 minutos para su recompensa diaria.
- El progreso de clan se guarda por semana y el modo todos contra todos acepta
  resultados de una futura partida autoritativa.
- La APK local muestra estos estados desde el menu `Mision`; antes de publicar,
  S-01 debe validar horarios, membresias, rankings, recompensas y resultados PvP.

## Fase 5.66: Direccion Artistica Y Personajes

La direccion visual queda fijada como fantasia realista estilizada y
cinematografica, optimizada para Android. `CharacterArtProfiles` define las
ocho combinaciones jugables con siluetas Vanguard, Veil, Spirit y Void.
`PlayerAvatarVisual` aplica tratamiento de materiales, capas, accesorios y
firmas de clase sobre los modelos FBX existentes sin romper animaciones,
equipamiento, sexo, cosmeticos o jugadores remotos.

La especificacion completa queda en `docs/character-art-direction.md`. La
siguiente parte reemplazara estas firmas procedurales por meshes finales de la
Zona 1 y luego extendera el pipeline a las diez zonas.

## Fase 5.65: Interfaz Cinematografica Para Android

- `UiThemeConfig` aplica un lenguaje visual comun de paneles oscuros, marcos,
  sombras, acentos por clase y estados de boton sin depender de un paquete
  artistico cerrado.
- El HUD de combate ahora concentra vida, energia, objetivo, minimapa, feed,
  joystick, ataque y habilidades en zonas separadas; el chat puede colapsarse.
- La habilidad final usa un boton circular distintivo y los `+` de mejora pasan
  a la ventana `HABILIDADES`, fuera del combate.
- La conexion se representa como un indicador discreto `S-01`; URL y errores
  tecnicos completos quedan en `Servidor y conexion`.
- `GRAFICOS` permite elegir `CALIDAD` o `RENDIMIENTO`; Android parte en
  `RENDIMIENTO` y ajusta FPS objetivo, sombras, antialiasing y reflejos.
- La orientacion Android queda fija en horizontal, con safe area y referencia
  de UI 1600x900. El joystick, camara y multitactil se mantienen activos.

La APK de esta fase se genero con Unity 6000.5.3f1, se valido como ZIP Android
sin errores y queda lista para reinstalar en el telefono. Falta medir FPS,
memoria y carga en varios formatos de pantalla reales.

## Clases Iniciales

Los nombres pueden cambiar durante el desarrollo. Se recomienda evitar copiar nombres, enemigos, efectos o sistemas exactos de otros juegos.

- Guerrero: cuerpo a cuerpo, resistente, espada o hacha.
- Ninja: rapido, critico, dagas o arco.
- Chaman: soporte, magia espiritual, curacion y dano elemental.
- Umbra: guerrero oscuro/mistico inspirado en arquetipos de magia y espada.

## Filosofia de Desarrollo

1. Primero hacer que se sienta divertido moverse y atacar.
2. Luego agregar progresion.
3. Luego agregar online.
4. Luego ampliar contenido.
5. Luego preparar publicacion.

Un MMORPG crece con comunidad, balance y contenido constante. La mejor forma de lograrlo es lanzar una base pequena pero solida y evolucionarla.
