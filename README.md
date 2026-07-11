# Proyecto MMORPG Android

Este proyecto apunta a crear un MMORPG de accion para Android inspirado en clasicos como MU Online y Metin2, pero con identidad, mundo, nombres, arte y sistemas propios.

La meta no es construir todo el MMORPG de una vez. La estrategia recomendada es avanzar por versiones jugables pequenas, donde cada fase agrega una capa real del juego: movimiento, combate, clases, progresion, online, economia y publicacion.

Documentos clave:

- [`GAME_ARCHITECTURE.md`](GAME_ARCHITECTURE.md): arquitectura tecnica objetivo y hoja de ruta de las proximas etapas.
- [`CLAUDE.md`](CLAUDE.md): contrato de desarrollo (reglas, como probar, estado conocido).
- `docs/`: roadmap, plan tecnico, diseno de juego y checklist Android.

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
