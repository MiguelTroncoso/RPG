# Estado Y Avance

Este porcentaje es una estimacion de producto, no un conteo de scripts. Un
MMORPG completo tambien necesita arte final, contenido, servidores, pruebas,
economia, seguridad, operacion y publicacion.

## Corte Diario: 2026-07-16

- Avance global estimado hacia el MMORPG publicable: **93%**.
- Prototipo jugable offline: **100%**.
- Vertical slice de una zona pulida: **98%**.
- Online persistente y preparado para produccion: **48%**.
- Contenido funcional 1-105: **100%**.
- Longevidad offline (contratos y Renacimiento): **75%**.
- Eventos y temporadas offline: **82%**.
- Arte final comercial, online y lanzamiento: **76%**.

La cifra global sube lentamente porque los sistemas base estan bastante
avanzados, pero todavia faltan muchas horas de arte, contenido, pruebas y
operacion real. La referencia principal es la vertical slice: una zona con
personajes, mobs y HUD suficientemente pulidos para ensenar el juego.

## Hecho En Este Corte

- Fase 5.42: entrada multitactil estable para mover y atacar en Android.
- Fase 5.43: familias visuales procedurales para mobs, distintivos de elite y
  jefe, nombres sobre los enemigos y barras de vida sobre cada objetivo.
- Fases 5.44-5.47: HUD compacto, primera capa 3D KayKit CC0, ambientacion de
  las zonas 1-10 y animaciones/telegraph/muerte para enemigos.
- Fases 5.45, 5.46 y 5.48: variantes normales por zona, caminos y landmarks
  1-10, spawners por proximidad y perfil de calidad Android.
- Fase 5.49: obstaculos solidos, puntos de interes explorables y reporte de TTK
  por zona; Android limita tambien la cantidad de enemigos activos.
- Fase 5.50: zona segura de comercio en la Zona 1, con bloqueo de spawns,
  persecucion y combate dentro del campamento.
- Refinamiento 5.44: menu con accesos directos a Stats y Datos/telemetria.
- Refinamiento 5.45: criatura propia de reliquia para normal, elite y jefe de
  la Zona 1, conservando fallback para las demas zonas.
- Refinamiento 5.49: POI de entrada, elite y jefe con recompensas de EXP/oro
  una vez por sesion mediante `RewardService`.
- Hotfix mobile: movimiento y ataque simultaneos conservan el joystick y el
  ataque tactil responde al presionar.
- Refinamiento de interfaz mobile: camara tactil con giro horizontal/vertical
  en zona libre y ayuda de controles separada del chat.
- Fase 5.51: regeneracion progresiva tras cuatro segundos sin recibir dano y
  minimapa 2D liviano con jugador, mobs cercanos y puntos de interes.
- Fase 5.48: ventana TEST instrumentada con FPS real, memoria, tiempo de sesion
  y conteo de objetos activos para la prueba Android.
- Fase 5.45: segunda capa de identidad visual por familia y tier para mobs de
  las zonas 1-10, compatible con modelos KayKit y fallback procedural.
- Fase 5.49: reclamos de `EXPLORAR` persistidos por personaje en `PlayerSaveData`
  esquema 10 y enviados dentro del snapshot online.
- Fase 5.45: primer set de 16 mobs 3D Quaternius CC0 importados para las zonas
  1-10, con controladores de idle, movimiento y ataque generados en Unity.
- Fase 5.45: diez jefes con modelos distintos, silueta propia y paleta de zona
  aplicada sin duplicar materiales; la base CC0 sigue siendo reemplazable.
- Fase 5.52: progresion de loot 1-105 con 20 materiales, 120 piezas de set,
  10 reliquias de jefe, tablas por zona/tier y materiales de mejora asociados.
- Los bosses entregan ahora la reliquia garantizada y un segundo drop adicional.
- Fase 5.52: primera pasada de balance por banda; las probabilidades progresan
  y `ZoneBalanceResolver` usa el arma esperada para calcular TTK.
- Fase 5.53: pantalla de acceso mobile renovada con identidad visual, tarjeta
  de personaje guardado, flujo crear/entrar y perfiles de servidor S-01/S-02/S-03.
  S-01 queda activo para la prueba local; S-02 y S-03 estan preparados para
  backend futuro y bloqueados mientras no existan esas instancias.
- Fase 5.54: perfiles de servidor data-driven, URL configurable desde la APK,
  ultimo servidor recordado, conexion automatica al entrar y estado visible.
- Fase 5.54: servidor Node configurable con `SERVER_ID`, `SERVER_NAME`, `HOST`,
  `PORT` y `MAX_PLAYERS`; el welcome comunica los datos del servidor y el host
  por defecto permite prueba dentro de la red local.
- Fase 5.55: slot de atuendo y alas con visual 3D procedural, bonificaciones de
  daño/vida/critico, coleccion persistida y compra de cosmeticos desde una
  tienda de objetos MVP.
- Fase 5.55: mascotas y monturas incorporan propiedad, rareza y bonificaciones
  de combate; se agregan compañeros raros bloqueados para recompensas futuras.
- Fase 5.55: evento automatico diario Caceria de Reliquias con progreso,
  recompensa de EXP/oro y desbloqueo dificil de Alas de brasa; queda local por
  ahora y preparado para autoridad del servidor.
- Fase 5.56: pasada visual 3D transversal con materiales compartidos, acabado
  metalico controlado, runas emisivas, niebla de profundidad, iluminacion
  calida y acentos de terreno para las diez zonas.
- Fase 5.56: el mismo acabado visual se aplica a avatar, armaduras, mobs,
  jefes, NPC, mascotas, monturas, monolito y VFX de combate sin agregar
  paquetes externos.
- Fase 5.57: movimiento y ataque simultaneos estabilizados; el ataque ya no
  roba la orientacion del joystick y los `CharacterController` de los mobs no
  bloquean al jugador ni forman barreras entre ellos.
- Fase 5.57: Zona 1 reforzada como recorrido nivel 1-10, con campamento seguro
  separado del campo de combate, marcador visual del area de lobos y valores
  explicitos de spawn, elite y jefe.
- Fase 5.58: cuatro habilidades por clase con ranuras Q/E/R/F; Q/E comienzan
  disponibles, R se desbloquea en nivel 8 y F en nivel 20.
- Fase 5.58: cada habilidad tiene nivel 1-5, aumenta su dano o curacion y
  reduce progresivamente su enfriamiento; el `Manual de habilidades` se usa
  para mejorarla, se guarda en el esquema 12 y cae con mas frecuencia de
  elites y jefes.
- Fase 5.59: habilidad final `G` por clase, desbloqueada en nivel 50, con
  niveles 1-10, efectos de ejecucion/area propios y cooldown persistente de
  30 minutos basado en hora UTC.
- Fase 5.59: las diez zonas reciben un landmark de identidad propio y cada
  banda agrega capa, anillos, brazalete y cinturon; el avatar muestra piezas
  por ranura, rareza y tier, ademas del emblema de clase.
- Fase 5.60: servidor preparado para preflight de Hetzner con endpoint
  `/health`, limite de mensajes, heartbeat, apagado limpio y guia WSS/Nginx.
- Fase 5.61: auditoria automatica de contenido 1-105 y correccion de la cadena
  de misiones; las diez bandas quedan cubiertas con normal, elite y jefe.
- Fase 5.62: tres contratos diarios rotativos por banda (caceria normal,
  elite y materiales de mejora), con progreso, recompensas y persistencia.
- Fase 5.62: Renacimiento al nivel 105; reinicia nivel, EXP, atributos y
  campana, conserva colecciones/equipo/cosmeticos/habilidades, otorga Renombre
  y un bono permanente de EXP del 2% por Renacimiento.
- Fase 5.63: evento semanal Conquista del Valle, con reset cada lunes UTC,
  30 derrotas, 3 objetivos elite y recompensas de materiales.
- Fase 5.63: temporada de 28 dias con 30 niveles, XP por combate/misiones/
  contratos/evento semanal, hitos de recompensa y Atuendo del eclipse al nivel
  final; el progreso se guarda en el esquema 15.
- Fase 5.64: calendario UTC con siete actividades rotativas y jefe mundial
  diario con horario variable y ventana de recompensa de 60 minutos.
- Fase 5.64: contribucion semanal de clan y contrato de resultados para todos
  contra todos, preparados para autoridad de servidor sin falsear PvP local.
- Fase 5.65: tema visual cinematografico runtime con marcos, sombras, estados
  de boton, tipografia comun y colores centralizados en `UiThemeConfig`.
- Fase 5.65: HUD Android compacto con barra de energia, objetivo contextual,
  minimapa enmarcado, chat colapsable, habilidades circulares y habilidad final
  separada del grupo de mejoras.
- Fase 5.65: estado de conexion discreto para S-01; URL, errores completos y
  diagnostico pasan a la ventana de servidor. Se agregan ventanas de habilidades
  y perfiles `CALIDAD`/`RENDIMIENTO`, con rendimiento como predeterminado Android.
- Fase 5.65: orientacion horizontal fija, autorrotacion deshabilitada y
  referencia UI 1600x900 conservando safe area y multitactil existente.
- Fase 5.66 (partes 1 a 4): direccion artistica documentada, ocho perfiles
  visuales jugables, armas FBX reales por clase y vertical slice 3D de Zona 1
  con Tribal, Orc y Orc_Skull; se conserva el contrato de sexo, equipo y
  animaciones.
- Fase 5.67: primer arte propio 3D generado con Unity para Guerrero masculino
  y el mob normal de Zona 1, con mallas low-poly, materiales compartidos,
  espada, armadura, reliquia y animacion procedural sin geometria externa.
- Fase 5.68: las ocho variantes jugables usan arte propio modular por clase y
  sexo, con armas, siluetas, rasgos de identidad y proporciones diferenciadas.
  Los meshes generan UVs y microtexturas albedo de Android para placas, tela,
  cuero, escama, runa y hueso; el rig runtime expone torso, brazos, manos,
  piernas, pies y agarres sin colisionadores visuales.
- Fase 5.69: atlas compartido albedo/normal 512x512 con tiles 32x32, cuerpo
  propio combinado en `SkinnedMeshRenderer` con pesos por pieza, `LODGroup`
  con culling lejano para personajes y mob propio, y animacion procedural de
  brazos y piernas compatible con el rig modular.
- Fase 5.70: Blender 5.2 instalado y pipeline reproducible para ocho FBX
  skinned authored con atlas albedo/normal, objeto `Starter Weapon`, clips
  propios `Idle`/`Run`/`Attack` y ocho controllers de Unity. El runtime prioriza
  esos FBX, conserva fallback procedural y aplica culling `LODGroup`.
- El reporte `docs/content-completeness.md` confirma 17/17 checks: 10 zonas,
  35 misiones, 162 items, 30 tablas de loot, 15 modelos de mobs y modelos de
  personaje masculino/femenino disponibles. La auditoria actual pasa 17/17.
- APK Android recompilada y validada con Unity 6000.5.3f1 y `aapt2`.
- Cambios sincronizados en GitHub, rama `main`.

## Siguiente Corte

1. Reinstalar el APK y probar durante varios minutos joystick + ataque + giro de
   camara, especialmente atravesando grupos de mobs.
2. Confirmar que el campamento de la Zona 1 no contiene mobs y que la transicion
   al campo de combate se entiende visualmente.
3. Registrar desde TEST FPS, memoria, carga y objetos activos en la Zona 1.
4. Ajustar cantidad de mobs, VFX, decoracion, TTK y radio del minimapa con datos
   del telefono real.
5. Completar la vertical slice nivel 1-10 con modelos propios de elite/jefe,
   recompensas calibradas y una interfaz de inventario/tienda mas clara.
6. Probar desbloqueo y mejora de habilidades en Android, incluyendo nivel 8/20,
   nivel 50, consumo de manuales, persistencia y lectura del boton G.
7. Revisar las diez zonas en una sesion de recorrido, validando landmark, mobs
   3D, TTK, minimapa y lectura de los nuevos accesorios.
8. Levantar S-01 en LAN, probar dos telefonos y despues preparar endpoint publico
   con firewall, TLS/WSS y persistencia protegida.
9. Mover combate, recompensas, inventario, propiedad y compras a autoridad del
   servidor antes de una prueba publica.
10. Sustituir progresivamente los meshes low-poly authored por esculturas
    finales, LOD geometrico real, normales horneadas y animaciones de combate.
11. Ejecutar `MMORPG > QA > Generate Content Completeness Report` después de
    cada cambio de zonas, misiones, loot o equipos.
12. Probar contratos diarios y Renacimiento con varios personajes antes de
    mover sus recompensas a autoridad del servidor.
13. Probar cambio de lunes UTC, cambio de temporada, hitos 5/10/15/20/25/30
    calendario diario y persistencia de recompensas en Android.
14. Validar en S-01 horarios del jefe, membresias de clan, ranking y resultados
    del modo todos contra todos antes de habilitar recompensas online.

## Registro

| Fecha | Fase | Resultado | Global |
| --- | --- | --- | --- |
| 2026-07-16 | 5.70 | Blender, ocho FBX skinned authored, atlas, normals, clips y controllers Unity | 94% |
| 2026-07-16 | 5.69 | Atlas albedo/normal, skinning runtime, LOD y animacion articulada | 93% |
| 2026-07-16 | 5.68 | Ocho variantes propias, UVs, microtexturas y rig modular runtime | 92% |
| 2026-07-16 | 5.67 | Primer arte propio 3D para Guerrero y mob de Zona 1 | 90% |
| 2026-07-16 | 5.66 1-4 | Ocho personajes 3D con armas FBX y mobs 3D de Zona 1 | 89% |
| 2026-07-16 | 5.65 | Interfaz cinematografica Android, conexion discreta y perfiles graficos | 87% |
| 2026-07-15 | 5.64 | Calendario diario, jefe mundial, clan y todos contra todos preparados | 84% |
| 2026-07-15 | 5.63 | Evento semanal, temporada de 28 dias y recompensas por hitos | 83% |
| 2026-07-15 | 5.62 | Contratos diarios rotativos, Renacimiento y Renombre persistente | 82% |
| 2026-07-15 | 5.61 | Auditoria funcional 1-105, mision normal de Bosque y reporte 11/11 | 80% |
| 2026-07-15 | 5.60 | Preflight Hetzner: health check, heartbeat, apagado limpio y guia WSS | 78% |
| 2026-07-15 | 5.59 | Habilidad final G nivel 50-10, cooldown persistente, landmarks y equipo visual | 78% |
| 2026-07-15 | 5.58 | Habilidades Q/E/R/F, desbloqueos por nivel, mejoras con manuales y guardado | 75% |
| 2026-07-13 | 5.57 | Movimiento + ataque estables y recorrido seguro de Zona 1 nivel 1-10 | 75% |
| 2026-07-13 | 5.56 | Pasada visual 3D de materiales, luz, niebla y zonas | 75% |
| 2026-07-13 | 5.55 | Atuendos, alas, stats de compañeros, tienda MVP y evento diario | 73% |
| 2026-07-13 | 5.54 | Conexion automatica, URL configurable y servidor Node parametrizable | 70% |
| 2026-07-13 | 5.53 | Acceso mobile, seleccion de personaje y perfiles de servidor | 65% |
| 2026-07-13 | 5.52 | Primera pasada de balance de loot y TTK por banda | 64% |
| 2026-07-13 | 5.52 | Progresion de loot 1-105 con materiales, sets y reliquias de jefe | 63% |
| 2026-07-13 | 5.45 | Diez jefes con modelo unico, silueta y paleta propia por zona | 60% |
| 2026-07-13 | 5.45 | Primer set de 16 mobs 3D CC0 con animaciones por zona/tier | 58% |
| 2026-07-12 | 5.45+5.49 | Identidad por familia/tier y POI persistentes por personaje | 56% |
| 2026-07-12 | 5.48 | Instrumentacion de rendimiento para prueba Android | 53% |
| 2026-07-12 | 5.51 | Regeneracion progresiva y minimapa 2D de mobs/POI | 52% |
| 2026-07-12 | 5.42+5.44 | Hotfix multitactil, menu refinado y camara tactil | 49% |
| 2026-07-12 | 5.50 | Zona segura de comercio y bloqueo de combate | 44% |
| 2026-07-12 | 5.44+5.45+5.49 | Menu Stats/Datos, criatura de Zona 1 y recompensas de POI | 47% |
| 2026-07-12 | 5.49 | Obstaculos solidos, POI explorables y reporte de balance | 43% |
| 2026-07-12 | 5.45-5.48 | Variantes por zona, rutas, landmarks y optimizacion Android | 40% |
| 2026-07-12 | 5.44-5.47 | HUD, mobs 3D, zonas 1-10 y feedback de combate | 36% |
| 2026-07-12 | 5.43 | Mobs con familias visuales, tiers y barras de vida | 30% |
| 2026-07-12 | 5.42 | Multitactil de joystick + combate | 29% |
| 2026-07-12 | 5.41 | Suelo, recuperacion de caidas y escala Android | 28% |
