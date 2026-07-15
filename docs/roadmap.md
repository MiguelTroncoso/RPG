# Roadmap Por Fases

## Fase 0: Preproduccion

Objetivo: definir el juego antes de escribir demasiado codigo.

- Vision del juego.
- Clases iniciales.
- Primer mapa.
- Lista de sistemas del MVP.
- Estilo visual de referencia.
- Decision final de motor.

Criterio de exito: sabemos exactamente que prototipo vamos a construir primero.

## Fase 1: Prototipo Offline

Objetivo: que el juego ya se sienta bien como accion RPG.

- Proyecto Unity 3D Android.
- Movimiento con joystick virtual.
- Camara tercera persona.
- Personaje temporal.
- 1 clase jugable.
- Ataque basico.
- 1 enemigo con IA simple.
- Barra de vida.

Criterio de exito: moverse, atacar y matar un enemigo es entretenido.

## Fase 2: Vertical Slice De Combate

Objetivo: representar el juego real en pequeno.

- 4 clases. [Base implementada]
- Pantalla de seleccion de personaje. [Base implementada]
- Masculino/femenino visualmente diferenciados. [Base implementada]
- 2 habilidades por clase. [Base implementada]
- 3 tipos de enemigos.
- Experiencia y nivel. [Base implementada]
- Loot basico. [Base implementada]
- Primer mapa jugable.
- UI mobile inicial. [Base implementada]

Criterio de exito: se puede jugar 10-15 minutos y progresar.

## Fase 3: Online Basico

Objetivo: convertir el prototipo en un juego compartido.

- Login temporal.
- Servidor autoritativo simple. [Base local implementada]
- Sincronizacion de posicion. [Base implementada]
- Ver otros jugadores. [Base implementada]
- Chat. [Base implementada]
- Monstruos sincronizados.
- Recompensas por participacion.

Criterio de exito: 2-10 jugadores pueden estar en el mismo mapa.

## Fase 4: Contenido Y Retencion

Objetivo: que haya razones para volver.

- Misiones simples. [Base implementada]
- Mejoras de equipo. [Base implementada]
- Tienda NPC. [Base implementada]
- Inventario. [Base implementada]
- Jefe/evento de mapa. [Base implementada]
- Balance de clases.
- Sonido y efectos.

Criterio de exito: una sesion de 30 minutos tiene objetivos claros.

## Fase 5: Preparacion Android

Objetivo: preparar una build testeable.

- Optimizacion mobile. [Base implementada]
- Build Android App Bundle. [Herramienta implementada]
- Icono, nombre y pantalla de carga. [Nombre y pantalla base implementados]
- Safe area y HUD mobile. [Base implementada]
- Feed de recompensas/actividad. [Base implementada]
- Arte placeholder para tienda. [Generador implementado]
- Politicas de privacidad. [Checklist documentado]
- Test cerrado. [Checklist documentado]
- Correcciones de bugs.

Criterio de exito: el juego se puede distribuir a testers por Google Play Console.

## Fase 5.5: Creacion De Personaje

Objetivo: que el jugador entre al mundo con identidad basica.

- Nombre editable. [Base implementada]
- Seleccion de clase antes de jugar. [Base implementada]
- Seleccion masculino/femenino. [Base implementada]
- Etiqueta visible sobre el personaje. [Base implementada]
- Integracion con nombre online local. [Base implementada]
- Modelos 3D reales por clase/sexo.

Criterio de exito: el jugador reconoce su avatar antes de empezar a combatir.

## Fase 5.6: Avatar Visual MVP

Objetivo: dejar de depender de una capsula generica mientras llegan modelos reales.

- Avatar procedural con partes visibles. [Base implementada]
- Silueta distinta por clase. [Base implementada]
- Variante visual masculino/femenino. [Base implementada]
- Jugadores remotos con silueta por clase. [Base implementada]
- Reemplazo futuro por modelos 3D reales.

Criterio de exito: al mirar el juego se reconoce la clase del personaje sin leer el HUD.

## Fase 5.23: Animacion Procedural Del Avatar

Objetivo: que el avatar deje de sentirse estatico mientras no existan animaciones finales.

- Idle con respiracion. [Base implementada]
- Caminar con bob/swing procedural. [Base implementada]
- Ataque basico y habilidades con impulso visual. [Base implementada]
- Puente para modelos con Animator (`Speed`/`Attack`). [Base implementada]

Criterio de exito: moverse y atacar se siente vivo aunque el arte siga siendo placeholder.

## Fase 5.24: StatSheet Con Modificadores Por Origen

Objetivo: centralizar stats para que equipo, atributos, buffs y montura no sumen reglas por fuera.

- `StatSheet` con base por clase y modificadores por origen. [Base implementada]
- `PlayerStatSheet` como lectura resuelta para combate/HUD. [Base implementada]
- Buffs de dano modelados como origen `Buff`, sin sobrescribir base. [Base implementada]
- Montura como modificador de velocidad dentro de la hoja. [Base implementada]

Criterio de exito: cualquier bono nuevo tiene un origen claro y pasa por un recomputo unico.

## Fase 5.25: i18n De Creacion Y Habilidades

Objetivo: eliminar los textos hardcodeados mas visibles que quedaban en creacion de personaje y habilidades.

- Fallback de localizacion mezclado con asset generado. [Implementado]
- Panel de creacion con claves i18n. [Implementado]
- PlayerSkills con claves i18n. [Implementado]
- Etiquetas de genero local/remoto con claves i18n. [Implementado]

Criterio de exito: crear/continuar personaje y usar habilidades no depende de literales en codigo.

## Fase 5.26: Zona 4 - Cumbres De Cristal

Objetivo: sumar una cuarta region jugable sin tocar sistemas.

- `DefaultZones.CreateZone4` con terreno, normales, elites y jefe. [Implementado]
- Enemigos y objetivos filtrados por ids propios. [Implementado]
- Cadena de misiones extendida a 16. [Implementado]

Criterio de exito: la region 31-40 aparece al norte y se progresa por datos.

## Fase 5.27: Persistencia Basica De Servidor

Objetivo: que el servidor no pierda el estado online basico al reconectar.

- `playerKey` estable en `hello`. [Implementado]
- Guardado JSON atomico en `Server/data/players.json`. [Implementado]
- Restauracion de identidad/nivel/posicion/rotacion. [Implementado]
- Ultima mejora validada persistida como dato online. [Implementado]

Criterio de exito: reiniciar el servidor conserva el estado online basico.

## Fase 5.28: Puente Para Animaciones Reales

Objetivo: aceptar clips reales cuando lleguen assets de personaje.

- Parametros `Speed`/`Attack`. [Implementado]
- Estados `Idle`/`Run`/`Attack` sin parametros. [Implementado]
- Fallback procedural intacto. [Implementado]

Criterio de exito: un Animator Controller simple puede manejar el avatar real sin tocar gameplay.

## Fase 5.29: Pasada Final i18n Visible

Objetivo: sacar literales visibles secundarios del codigo de UI/red/eventos.

- Red/chat/errores via claves. [Implementado]
- Botones utilitarios y labels de mundo via claves. [Implementado]
- Evento del monolito via claves. [Implementado]

Criterio de exito: los textos visibles centrales pasan por Localization.Tr.

## Fase 5.30: Zonas 5-10 Hasta Nivel 105

Objetivo: cubrir el rango completo 1-105 con zonas y misiones data-driven.

- Paso Glacial, Ruinas Sumergidas, Forja Obsidiana, Jardin Astral, Santuario del Eclipse y Trono del Vacio. [Implementado]
- Cadena de misiones extendida a 34. [Implementado]
- Enemigos normales, elites y jefes por zona. [Implementado]

Criterio de exito: la progresion de contenido llega hasta el nivel maximo.

## Fase 5.31: Previews Visuales

Objetivo: dejar imagenes ligeras para revisar direccion visual y progresion.

- Mapa de progresion 1-105 en SVG. [Implementado]
- Preview de clases/avatar/UI en SVG. [Implementado]

Criterio de exito: se puede inspeccionar el avance visual sin entrar a Play.

## Fase 5.32: Balance Inicial Y Feedback De Combate

Objetivo: que el combate sea mas legible y menos brusco antes de balance fino.

- Cooldown/windup por tier de enemigo. [Implementado]
- Aviso visual antes de ataques enemigos. [Implementado]
- Cancelacion de golpe si el jugador sale de rango. [Implementado]
- Flash de dano en jugador/enemigos/monolito. [Implementado]
- Defensa del jugador desde `PlayerStatSheet`. [Implementado]

Criterio de exito: el jugador entiende cuando viene un golpe y recibe feedback claro al pegar/recibir dano.

## Fase 5.33: Telemetria Y Persistencia Servidor Completa

Objetivo: empezar a medir balance real y permitir que el servidor conserve el
progreso completo del personaje.

- Telemetria local por zona: kills, muertes, dano dado/recibido y tiempo para matar. [Implementado]
- Export JSON de telemetria en `Application.persistentDataPath/telemetry`. [Implementado]
- Envio opcional de telemetria al servidor cuando el cliente esta online. [Implementado]
- Mensaje `saveState` con snapshot completo `PlayerSaveData`. [Implementado]
- Mensaje `savedState` para recuperar snapshot remoto al reconectar. [Implementado]
- Resolucion simple de conflicto: aplicar remoto si esta mas avanzado; si no, re-subir local. [Implementado]

Criterio de exito: despues de jugar online, `Server/data/players.json` conserva inventario/equipo/misiones/posicion y tambien un resumen de telemetria.

## Fase 5.34: Panel De Telemetria Y Balance 41-105

Objetivo: revisar el ritmo de combate dentro del juego y dejar una primera
calibracion medible para las zonas finales.

- Panel DATOS dentro del HUD con kills, muertes, dano y archivo local. [Implementado]
- Tiempo medio para derrotar separado por normales, elites y jefes. [Implementado]
- Objetivos TTK configurables en `ZoneDefinition` con estados RAPIDO/LENTO/OK. [Implementado]
- Primera calibracion de vida, dano, EXP y oro para zonas 5-10. [Implementado]

Nota de balance: esta calibracion es un punto de partida antes de contar con
sesiones reales. El siguiente ajuste debe usar el panel despues de jugar y
comparar cada zona con los objetivos TTK normal 3-8 s, elite 8-18 s y jefe
30-75 s.

Criterio de exito: abrir DATOS durante Play muestra las diez zonas y permite
identificar rapidamente una zona demasiado rapida, lenta o sin muestras.

## Fase 5.35: Menu Mobile De Personaje

Objetivo: hacer legibles inventario, equipo y misiones en una pantalla tactil.

- Boton MENU con pestañas Inventario, Equipo y Mision. [Implementado]
- Detalle de objetos, cantidades, slots equipados, mejoras y bonos. [Implementado]
- Detalle de objetivos y recompensas de la mision activa. [Implementado]
- Actualizacion por eventos al recoger, consumir, equipar, mejorar o avanzar. [Implementado]

Criterio de exito: durante Play el jugador puede abrir MENU y revisar su estado
sin depender de las lineas compactas del HUD.

## Fase 5.36: Resolucion, Modelos Y Feedback Audiovisual

Objetivo: hacer que la interfaz modal sobreviva a pantallas moviles y que el
combate tenga una primera respuesta audiovisual clara.

- Escalado responsive de ventanas en portrait, landscape y safe area. [Implementado]
- Personajes KayKit CC0 importados para las cuatro clases. [Implementado]
- Cuatro controladores Mecanim generados desde 76 clips por personaje. [Implementado]
- Audio procedural ligero para ataque, golpe, critico, dano, habilidad y nivel. [Implementado]
- Particulas de impacto, habilidad y subida de nivel con material cacheado. [Implementado]

Criterio de exito: probar MENU en varias resoluciones no recorta la ventana y
el combate comunica sus acciones con modelo, animacion, sonido y VFX.

## Fase 5.37: Variantes Femeninas, Audio Final Y Diagnostico Movil

Objetivo: cerrar la primera pasada de identidad visual por sexo/clase y tener
instrumentacion para probar el prototipo en Android.

- RogueHooded para la Ninja femenina y capa de armadura visual por clase/sexo. [Implementado]
- Clips CC0 de Kenney para SFX de combate, habilidades y nivel. [Implementado]
- Clip musical importado con fallback procedural. [Implementado]
- Diagnostico runtime de resolucion, orientacion, safe area y volumen. [Implementado]

Criterio de exito: una sesion en Android deja en el log la configuracion de
pantalla/audio y el juego no depende de tonos generados si los OGG estan presentes.

## Fase 5.38: Prueba Movil, Modelos Femeninos Y Equipo Visual

Objetivo: comprobar el prototipo en un dispositivo y hacer que el equipo sea
visible, equipable y compatible con la progresion existente.

- Ventana TEST con diagnostico de pantalla, safe area y volumenes ajustables. [Implementado]
- FBX CC0 femenino de Quaternius para Guerrero, Chaman y Umbra, con fallback procedural. [Implementado]
- Variante femenina RogueHooded para Ninja. [Implementado]
- `VisualId` en definiciones de equipo y refresco por evento al cambiar piezas. [Implementado]
- Visuales iniciales para espada, casco, pechera y amuleto. [Implementado]

Criterio de exito: equipar una pieza cambia sus bonos y deja una diferencia
visible en el avatar; la ventana TEST permite registrar una prueba movil
repetible antes de ajustar escala o audio con datos reales.

## Fase 5.39: Sets Modulares Y Preparacion Android

Objetivo: separar la identidad visual de cada clase/sexo y dejar una ruta
repetible para instalar el prototipo en Android.

- Ocho perfiles `ArmorVisualSetDefinition` con estilo y silueta. [Implementado]
- Fallback runtime `DefaultArmorVisualSets` para proyectos sin assets generados. [Implementado]
- Diagnostico con dispositivo, DPI y FPS objetivo. [Implementado]
- Configuracion batch Android: bundle, ARM64, SDK 26/35 y landscape. [Implementado]
- APK debug generada en ARM64 despues de instalar Android Build Support. [Implementado]

Criterio de exito: `MMORPG > Android > Build Debug APK` genera una APK
instalable y la ventana TEST permite anotar resolucion, safe area, FPS y audio
del dispositivo.

## Fase 5.40: Instalacion Y Prueba En Dispositivo

Objetivo: validar la experiencia tactil y audiovisual en un telefono Android.

- APK debug `valle-reliquias-debug.apk` generada y validada por `aapt2`. [Implementado]
- Identificador, version, SDK minimo/target y permiso de Internet verificados. [Implementado]
- Conexion ADB y ejecucion de la APK en telefono fisico. [Pendiente de dispositivo]
- Registro de datos TEST: resolucion, safe area, DPI, FPS y volumenes. [Pendiente de dispositivo]

Criterio de exito: instalar, abrir y jugar en un telefono; revisar MENU, TEST,
joystick, botones de combate, audio y orientacion landscape.

## Fase 5.41: Correcciones De QA Movil

Objetivo: eliminar los bloqueos de la primera prueba Android antes de seguir
ampliando contenido.

- APK sin consola Development sobre el joystick. [Implementado]
- Colision explicita del campo de entrenamiento. [Implementado]
- Recuperacion de caidas y saneamiento de posiciones guardadas. [Implementado]
- Escala Android landscape y seleccion de personaje responsive. [Implementado]
- Repetir prueba tactil en el telefono. [Pendiente]

Criterio de exito: el jugador aparece sobre el suelo, permanece estable, el
joystick mueve al personaje y la ventana de seleccion cabe dentro del safe area.

## Fase 5.42: Entrada Multitactil Y Combate Movil

Objetivo: hacer que mover y atacar al mismo tiempo sea estable en Android.

- El ataque de mouse queda limitado a editor/PC; el boton movil es la unica
  fuente tactil de ataque. [Implementado]
- `VirtualJoystick` conserva el dedo activo mediante `pointerId`. [Implementado]
- Repetir prueba en dispositivo con joystick + `ATK`, habilidades y botones
  de menu sin perder el movimiento. [Pendiente]

Criterio de exito: dos dedos pueden mover y atacar sin ataques espontaneos,
perdida del joystick ni bloqueo del personaje.

## Fase 5.43: Mobs Y Lectura De Combate

Objetivo: reemplazar los enemigos placeholder y hacer que cada familia tenga
una lectura visual y de combate reconocible.

- Siluetas procedurales de bestia, guardian, espiritu y sombra por familia de
  zona. [Implementado]
- Distintivos de elite y jefe con color, escala, crest y nucleo visual. [Implementado]
- Nombre y barra de vida sobre cada enemigo, sincronizados con `Health`.
  [Implementado]
- Modelos o prefabs 3D finales y animaciones especificas por familia.
  [Pendiente]
- Variantes por zona sin cambiar la logica data-driven del spawner.
  [Implementado]

Criterio de exito: el jugador distingue amenaza, distancia, tier y estado del
enemigo sin depender del texto de la consola. La prueba en telefono queda
pendiente para confirmar escala y legibilidad real.

## Fase 5.44: Interfaz De Juego Mobile

Objetivo: convertir el HUD de prototipo en una interfaz comoda para sesiones
largas en pantalla tactil.

- Separar lectura de personaje, objetivo, chat, combate e inventario.
  [Implementado]
- Reducir paneles superpuestos y reservar safe area para joystick y acciones.
  [Implementado]
- Agrupar acciones secundarias bajo `MAS` y dejar `MENU` siempre accesible.
  [Implementado]
- Exponer accesos directos a stats y datos desde el menu principal.
  [Implementado]
- Mantener movimiento y ataque simultaneos en Android sin conflicto de
  rotacion ni perdida del joystick. [Implementado: pendiente de prueba fisica]
- Girar la camara con arrastre tactil horizontal/vertical sin bloquear
  joystick, `ATK`, habilidades ni ventanas. [Implementado: pendiente de prueba fisica]
- Revisar seleccion de personaje, menu, stats, equipo, mision y chat en
  landscape Android. [Pendiente de prueba fisica]

Criterio de exito: combate y exploracion permanecen legibles sin tapar el
avatar ni los enemigos cercanos.

## Fase 5.45: Mobs 3D Finales Por Zona

Objetivo: reemplazar progresivamente las siluetas de la Fase 5.43 por
criaturas con identidad artistica, animaciones y materiales propios.

- Primera capa con modelos Quaternius Ultimate Monsters CC0 animados por zona
  y tier. [Implementado: 16 FBX y controladores Idle/Run/Attack]
- Variantes visuales propias por `EnemyId` normal: reliquia, espinas, ceniza,
  cristal, escarcha, abisal, magma, astral, eclipse y vacio. [Implementado]
- Seleccionar o crear el mob normal de la Zona 1 y sus variantes elite/jefe.
  [Implementado: bestia procedural de reliquia con piezas por tier]
- Importar solo assets CC0, MIT o propios y registrarlos en `ASSET_LICENSES.md`.
  [Implementado: Quaternius CC0 con licencia local]
- Conservar `EnemyVisualController` como fallback mientras cada familia se
  migra a un prefab/modelo real.
- Preparar idle, persecucion, ataque, impacto, muerte y variantes de color.
  [Implementado: controlador por modelo, feedback y fallback]
- Añadir una identidad visual de familia para elite/jefe en las zonas 1-10,
  compatible con modelos KayKit y fallback procedural. [Implementado]
- Crear modelos unicos para los jefes y materiales propios por zona.
  [Implementado: diez modelos distintos del set Quaternius, silueta propia y
  paleta por zona con `MaterialPropertyBlock`]
- Validar en Android la escala, lectura y rendimiento de los diez jefes antes
  de sustituir progresivamente el set CC0 por meshes finales propios. [Pendiente]

Criterio de exito: la Zona 1 tiene mobs reconocibles y consistentes sin
romper rendimiento ni reglas data-driven.

## Fase 5.46: Terreno Y Ambientacion De Zonas

Objetivo: hacer que las zonas 1-4 se sientan distintas y guien al jugador.

- Suelo y decoracion procedural para las zonas 1-10, nivel 1-105. [Implementado]
- Camino principal, entrada, landmarks de elite/jefe y marcadores de limite
  para las zonas 1-10. [Implementado]
- Obstaculos, caminos, puntos de interes e iluminacion final por zona.
  [En progreso: base visual lista, colision y arte final pendientes]
- Señales visuales de nivel, elite, jefe y transicion entre zonas.
  [Base implementada]

Criterio de exito: se reconoce la zona sin leer el nombre y el jugador puede
orientarse con el mundo.

## Fase 5.47: Animacion Y Feedback De Combate

Objetivo: que cada ataque tenga anticipacion, respuesta y cierre visual.

- `AvatarMotionAnimator` conectado a mobs con modelos. [Implementado]
- Telegraph visual al comenzar el windup enemigo. [Implementado]
- Impactos, dano flotante y VFX existentes conservados. [Implementado]
- Muerte visual antes de destruir y respawn. [Implementado]
- Animaciones especificas de criatura y audio final por familia. [Pendiente]

Criterio de exito: el jugador entiende cuando un enemigo prepara, ejecuta y
termina un ataque sin depender de la consola.

## Fase 5.48: Balance Y Rendimiento Android

Objetivo: probar las diez zonas y estabilizar el rendimiento antes de sumar
mas sistemas.

- Activar spawners solo cerca del jugador y limpiar zonas lejanas.
  [Implementado]
- Reducir sombras, luces, antialiasing y reflejos en Android. [Implementado]
- Reutilizar materiales de mobs y decoracion. [Implementado]
- Instrumentar la ventana TEST con FPS medido, promedio/minimo, memoria,
  tiempo de sesion, enemigos y POI activos. [Implementado]
- Medir FPS, memoria y tiempos de carga por cada zona 1-10 en telefono real.
  [Pendiente de dispositivo]
- Ajustar cantidad de decoracion, enemigos, VFX y distancia de lectura con
  datos del telefono. [Pendiente]
- Revisar TTK, recompensas y escalado de jefes con la telemetria.
  [Pendiente]
- Prueba fisica de HUD, mobs, multitactil, audio, safe area y ventana TEST.
  [Pendiente de dispositivo]

Criterio de exito: una sesion de nivel 1 a 105 es legible, estable y medible.

## Fase 5.49: Progresion Visual Y Balance Por Zona

Objetivo: conectar la identidad de cada zona con su ruta y sus enemigos sin
duplicar reglas de combate.

- Variantes normales diferenciadas por `EnemyId`. [Implementado]
- Entrada, camino, landmarks y limites visuales para zonas 1-10. [Implementado]
- Obstaculos solidos y cuatro puntos de interes por zona. [Implementado]
- Arte final de terreno y puntos de interes interactivos con recompensas.
  [En progreso: POI interactivos y recompensas persistentes implementados]
- Persistir el reclamo de cada POI por personaje en `PlayerSaveData` esquema 10
  y transportarlo en `saveState`. [Implementado]
- Validar recompensas con autoridad definitiva del servidor y telemetria real.
  [Pendiente]
- Ruta de prueba nivel 1-105 con telemetria, TTK y recompensas. [Pendiente]

Criterio de exito: cada zona se reconoce, se recorre con orientacion clara y
mantiene un desafio acorde a su rango de nivel.

## Fase 5.50: Zonas Seguras De Comercio

Objetivo: separar claramente el combate de los espacios de comercio y
progresion.

- Zona segura data-driven en la Zona 1 alrededor de los NPC. [Implementado]
- Spawns y persecucion bloqueados dentro de la zona segura. [Implementado]
- Combate del jugador bloqueado dentro de la zona segura. [Implementado]
- Marcador visual y punto `EXPLORAR` para el campamento. [Implementado]
- Guardias, teletransporte y safe zones para futuras ciudades. [Pendiente]

Criterio de exito: el jugador puede organizar inventario, comprar y mejorar
equipo sin recibir ataques ni generar enemigos en el campamento.

## Fase 5.51: Recuperacion Y Minimap Mobile

Objetivo: mejorar la lectura del combate y dar una pausa natural entre
encuentros sin llenar la pantalla de efectos.

- Registrar el ultimo dano recibido en `Health`. [Implementado]
- Regeneracion retrasada y progresiva para el jugador. [Implementado]
- Ritmo ligeramente distinto al estar en movimiento o quieto. [Implementado]
- Radar 2D sin segunda camara para jugador, mobs cercanos y POI. [Implementado]
- Ajustar delay, curacion por segundo, radio y legibilidad en telefono real.
  [Pendiente de prueba fisica]

Criterio de exito: el jugador entiende cuando vuelve a estar fuera de combate
y puede localizar amenazas cercanas sin abrir otro menu.

## Fase 5.52: Progresion Real De Loot 1-105

Objetivo: que explorar las diez zonas produzca materiales y equipo con una
progresion reconocible desde el nivel 1 hasta el 105.

- Catalogo determinista de diez bandas con 20 materiales propios, 70 piezas
  de equipo y diez reliquias exclusivas de jefe. [Implementado]
- Requisitos de nivel, rarezas, estadisticas, precios y materiales de mejora
  escalados por zona. [Implementado]
- Tablas diferenciadas para normal, elite y jefe; los bosses entregan reliquia
  garantizada y un segundo drop de su banda. [Implementado]
- Mantener IDs estables para no romper inventario, guardados ni equipo legacy.
  [Implementado]
- Primera pasada de probabilidades por banda y reporte TTK basado en el arma
  esperada de cada zona. [Implementado]
- Calibrar oro, precios, TTK real, restricciones por clase y bonus de conjunto
  con telemetria de telefono real. [Pendiente de prueba fisica]

Criterio de exito: un jugador puede avanzar por las diez bandas usando drops
de la zona actual y mejorar el equipo con materiales de esa misma zona.

## Fase 5.53: Acceso Mobile Y Seleccion De Personaje

Objetivo: presentar una entrada clara antes de la prueba fisica y dejar
preparado el acceso a futuros servidores sin fingir que ya existe un backend
multiserver.

- Splash inicial con identidad de Valle de las Reliquias y version Android.
  [Implementado]
- Pantalla de acceso con tarjeta del personaje local, nivel, EXP, oro, clase y
  sexo. [Implementado]
- Flujo separado para `ENTRAR AL VALLE`, `CREAR NUEVO PERSONAJE` y `VOLVER`.
  [Implementado]
- Perfiles visibles `S-01 Valle Central`, `S-02 Bosque de los Susurros` y
  `S-03 Reino de Pruebas`; solo S-01 esta habilitado contra `ws://localhost:7777`.
  [Implementado]
- Servidores S-02 y S-03 con instancias, dominio, matchmaking, cuentas y
  persistencia separada. [Pendiente de backend]
- Validar lectura, safe area, teclado, botones y flujo de guardado en Android.
  [Pendiente de dispositivo]

Criterio de exito: un jugador puede abrir la APK, reconocer el juego, elegir
su personaje guardado o crear uno y entrar al servidor de prueba sin confusion.

## Fase 5.54: Preparacion Online Para Prueba Compartida

Objetivo: que el mismo APK pueda probarse en telefono, LAN y posteriormente
en un endpoint publico sin recompilar por cada cambio de direccion.

- Catalogo de perfiles `ServerProfile` con ID, nombre, URL y disponibilidad.
  [Implementado]
- S-01 permite editar la direccion WebSocket desde la pantalla de acceso,
  recuerda URL/perfil en `PlayerPrefs` y copia la direccion al panel de red.
  [Implementado]
- `ENTRAR AL VALLE` inicia la conexion automaticamente y muestra `Offline`,
  `Conectando` u `Online` en la pantalla de acceso/HUD. [Implementado]
- El servidor Node acepta `SERVER_ID`, `SERVER_NAME`, `HOST`, `PORT` y
  `MAX_PLAYERS`, rechaza conexiones cuando esta lleno y envia metadatos en
  `welcome`. [Implementado]
- Probar S-01 con dos telefonos en la misma red usando la IP LAN del equipo.
  [Pendiente de dispositivo]
- Desplegar S-01 en un endpoint publico con firewall, TLS/WSS, dominio y
  persistencia protegida. [Pendiente de infraestructura]
- Habilitar S-02 y S-03 solo cuando tengan instancias y datos separados.
  [Pendiente de backend]

Criterio de exito: dos jugadores pueden conectarse desde APK, verse, chatear
y conservar su guardado sin editar codigo ni recompilar el cliente.

## Fase 5.55: Cosméticos, Compañeros Y Eventos Diarios

Objetivo: dar una capa de progresion visible y aspiracional al personaje sin
convertirla todavia en una economia real de pago.

- Slot de atuendo y slot de alas, con visual 3D procedural reemplazable por
  meshes finales. [Implementado]
- Bonificaciones de daño, vida y critico aplicadas desde `StatSheet` con origen
  `Cosmetic`, sin sumar stats desde la UI. [Implementado]
- Mascotas y monturas con propiedad, rareza, nivel requerido y bonos de combate;
  se conservan el zorro y caballo iniciales y se preparan compañeros raros.
  [Implementado]
- Tienda de objetos MVP con compra de la oferta siguiente usando oro del juego;
  la tienda muestra el camino hacia un panel completo de inventario y precios.
  [Implementado]
- Evento automatico diario `Caceria de Reliquias`: cinco derrotas, recompensa de
  EXP/oro y desbloqueo de Alas de brasa, persistido por fecha local. [Implementado]
- Validar precio, rareza, dificultad y lectura visual en Android. [Pendiente de
  dispositivo]
- Mover compras, propiedad, drops y calendario a autoridad server-authoritative
  antes de la prueba publica. [Pendiente online]

Criterio de exito: el jugador puede comprar o desbloquear un cosmetico, verlo
en su avatar, recibir sus stats, cerrar la app y conservarlo; el evento diario
se completa una vez y entrega su recompensa sin duplicarse.

## Fase 5.56: Pasada Visual 3D Transversal

Objetivo: subir la calidad percibida del mundo actual sin introducir assets
externos ni aumentar de forma peligrosa el coste en Android.

- Utilidad compartida de materiales con smoothness, metallic y soporte de
  emision; reduce duplicados y mantiene una identidad visual coherente.
  [Implementado]
- Iluminacion direccional mas calida, ambiente controlado, niebla lineal y
  camara sin HDR para priorizar legibilidad y rendimiento mobile. [Implementado]
- Runas, cristales, nucleos, halos, perlas, estrellas y monolitos con acentos
  emisivos; mobs, avatares, NPC, mascotas y monturas usan respuesta de luz
  consistente. [Implementado]
- Acentos de suelo y fragmentos por zona para que las bandas 1-105 tengan una
  lectura visual propia aun usando el mismo molde de terreno. [Implementado]
- Validar brillo, contraste, FPS, memoria y lectura de jefes en el telefono
  real; ajustar por perfil de calidad si es necesario. [Pendiente de dispositivo]
- Sustituir progresivamente geometria procedural por arte 3D final propio,
  manteniendo estos contratos de visuales. [Pendiente artistico]

Criterio de exito: al comparar la APK anterior con la nueva, el mundo, los
personajes y los enemigos se distinguen mejor por materiales, silueta, luz y
acentos de zona sin perder jugabilidad en Android.

## Fase 5.57: Combate Estable Y Vertical Slice De Zona 1

Objetivo: que el primer recorrido del jugador sea claro y que mover, atacar y
rodear enemigos no produzca bloqueos en Android.

- Ataque basico compatible con movimiento continuo: conserva la orientacion
  del joystick mientras el personaje golpea. [Implementado]
- Colisiones entre jugador y mobs, y entre mobs, ignoradas para evitar barreras
  de `CharacterController` cuando varios enemigos se agrupan. [Implementado]
- Campamento de la Zona 1 separado del campo de combate mediante radio seguro,
  spawns fuera del comercio y marcador visual del area de lobos. [Implementado]
- Valores explicitos de normales, elite y jefe para la progresion nivel 1-10.
  [Implementado]
- Validacion fisica de movimiento + ataque, safe zone, FPS y memoria en Android.
  [Pendiente de dispositivo]
- Ajuste final de TTK, loot, recompensas y lectura de interfaz despues de la
  prueba real. [Pendiente de balance]

Criterio de exito: un jugador puede entrar al campamento sin combate, salir al
campo, combatir mientras se desplaza, completar la cadena inicial y entender
la diferencia entre zona segura, normales, elite y jefe.

## Fase 5.58: Habilidades Progresivas

Objetivo: dar al combate una progresion clara por clase sin llenar al jugador
de botones desde el primer minuto.

- Cada clase tiene cuatro habilidades propias en las ranuras `Q`, `E`, `R` y
  `F`; las dos primeras estan disponibles desde nivel 1. [Implementado]
- `R` se desbloquea en nivel 8 y `F` en nivel 20; las ranuras bloqueadas
  muestran el nivel requerido y no ejecutan efectos. [Implementado]
- Cada habilidad progresa de nivel 1 a 5. El nivel aumenta dano/curacion y
  reduce el enfriamiento mediante una formula comun. [Implementado]
- El `Manual de habilidades` es un material raro: la primera mision entrega
  uno para probar el sistema y elites/jefes pueden soltar mas. [Implementado]
- Los niveles se conservan en `PlayerSaveData` esquema 12 y se exponen en el
  HUD junto a los botones de accion. [Implementado]
- Ajustar costos, dano, curacion, enfriamientos, drop rate y lectura de R/F con
  telemetria del telefono real. [Pendiente de balance]
- Validar que el servidor sea la autoridad de desbloqueos, consumos y efectos
  antes de permitir progresion compartida. [Pendiente online]

Criterio de exito: un jugador puede aprender y mejorar habilidades con una
economia entendible, notar una diferencia de poder sin romper el TTK y
conservar sus niveles al cerrar y reabrir el juego.

## Fase 5.59: Habilidad Final Y Equipo Visible

Objetivo: crear una recompensa de largo plazo y que la progresion 1-105 se
vea realmente en el personaje, no solo en numeros.

- Cada clase tiene una habilidad final propia en la ranura `G`, desbloqueada
  automaticamente en nivel 50. [Implementado]
- La habilidad final progresa de nivel 1 a 10, tiene dano/curacion y area
  superior a las habilidades normales, y usa un cooldown real de 30 minutos.
  [Implementado]
- El cooldown se persiste en UTC dentro de `PlayerSaveData` esquema 13 para
  que no se reinicie al cerrar la app. [Implementado]
- Cada banda 1-105 entrega doce piezas de set por zona: arma, armadura,
  accesorios y piezas de movilidad, mas la reliquia exclusiva del jefe.
  [Implementado]
- El avatar genera una lectura visual de arma, casco, pecho, guantes, piernas,
  botas, capa, anillos, brazalete, cinturon, collar y talisman, usando tier,
  rareza y color de clase. [Implementado]
- Cada zona recibe un landmark 3D procedural reconocible y cada clase un
  emblema visible sobre su equipo. [Implementado]
- Sustituir overlays procedurales por meshes finales y animaciones de fatality
  cuando la direccion artistica este cerrada. [Pendiente artistico]
- Balancear la habilidad final con TTK real, jefes y contenido PvP futuro.
  [Pendiente de balance]

Criterio de exito: llegar a nivel 50 se siente como un hito, la habilidad G
es poderosa pero no spameable y el equipo de cada banda se reconoce al verlo.

## Fase 5.60: Preflight De Hetzner

Objetivo: dejar S-01 listo para una primera prueba publica controlada, sin
confundir esta etapa operacional con autoridad MMO completa.

- El servidor expone `GET /health` con estado, jugadores, limite y uptime.
  [Implementado]
- El WebSocket limita payload a 64 KB, mantiene heartbeat cada 30 segundos y
  persiste sesiones durante `SIGTERM`/`SIGINT`. [Implementado]
- La guia documenta Ubuntu, systemd, Nginx, WSS, firewall, backups y el uso de
  `ws://` en LAN frente a `wss://` en Hetzner. [Implementado]
- Crear instancia, dominio, certificado, servicio systemd, backup probado y
  prueba con dos APK desde redes distintas. [Pendiente de infraestructura]
- Mover combate, drops, inventario, habilidades, compras y anti-cheat al
  servidor antes de abrir el juego a desconocidos. [Pendiente server-authority]

Criterio de exito: S-01 reinicia sin perder datos, responde al health check,
acepta WebSocket seguro y dos jugadores pueden conectarse desde Internet.

## Fase 6: Lanzamiento Inicial

Objetivo: publicar una version pequena y mantenerla.

- Acceso de produccion en Google Play.
- Analiticas basicas.
- Reporte de errores.
- Primer calendario de eventos.
- Canal de comunidad.

Criterio de exito: jugadores reales pueden instalar, jugar y dar feedback.
