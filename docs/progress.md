# Estado Y Avance

Este porcentaje es una estimacion de producto, no un conteo de scripts. Un
MMORPG completo tambien necesita arte final, contenido, servidores, pruebas,
economia, seguridad, operacion y publicacion.

## Corte Diario: 2026-07-18

- Avance global estimado hacia el MMORPG publicable: **98%**.
- Prototipo jugable offline: **100%**.
- Vertical slice de una zona pulida: **99%**.
- Online persistente y preparado para produccion: **48%**.
- Contenido funcional 1-105: **100%**.
- Longevidad offline (contratos y Renacimiento): **75%**.
- Eventos y temporadas offline: **82%**.
- Arte authored integrado y listo para iteracion: **94%**.
- Arte manual comercial, online y lanzamiento: **78%**.

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
- Fase 5.71: los ocho FBX ahora contienen cuerpo y arma en `LOD0`/`LOD1`/`LOD2`
  con decimacion geometrica real; Unity agrupa los renderers authored con
  umbrales de distancia 0.52/0.22/0.08. La geometria base tiene mas segmentos,
  suavizado y bordes biselados, y los clips de idle/carrera/ataque animan el
  cuerpo completo. Assimp, Unity batch y APK Android pasan la validacion.
- Fase 5.72: atlas albedo/normal 1024x1024 con tiles 64x64, normal detail
  generado offline desde alturas de materiales, esferas/conos mas densos,
  biseles de tres segmentos y normales ponderadas para armadura. Las poses
  authored incorporan cuello/cabeza y conservan anticipacion, impacto y
  recuperacion en `Attack`; la APK queda en 141 MB y pasa target SDK 35.
- Fase 5.73: cuatro familias de clase usan atlas propios 2K de albedo/normal;
  Blender ejecuta 16 bakes reales desde fuentes high-poly temporales con
  subdivision y microdesplazamiento sobre los objetivos skinned. Unity importa
  mipmaps, normal maps y ETC2 para Android; se mantienen 8 FBX, LOD0/1/2, tres
  clips por variante y 17/17 checks. La APK final queda en 39 MB, con target
  SDK 35. Esto cierra el pipeline tecnico, no la escultura manual comercial.
- Fases 5.74-5.75: la fuente high-poly usa Catmull-Clark nivel 2, detalle de
  escultura y microdetalle; el objetivo limpia vertices duplicados y normales,
  conserva UVs padded y recibe pintura authored procedural de desgaste, grano,
  tejido, cuero, piel, hueso y runas. Los ocho FBX mantienen cinco clips y la
  jaula exportada no incorpora la geometria high-poly.
- Fase 5.76: Guerrero y Ninja masculino/femenino reciben una pasada authored
  de mayor lectura con placas, inlays, hebillas, guardas, rodilleras, cintas,
  vendas, bolsos, ornamentos y crestas por variante.
- Fase 5.77: las cuatro variantes de Guerrero/Ninja conservan retopologia
  objetivo, UV padded, bake normal y atlas 2K por familia; Unity mantiene
  mipmaps, ETC2 y LOD0/1/2 para Android.
- Fase 5.78: los ocho personajes exportan cinco clips authored (`Idle`, `Run`,
  `Attack`, `Hit`, `Death`); los controllers de personajes y mobs exponen
  `Speed`, `Attack`, `Hit` y `Death`, y el runtime dispara impacto/muerte.
- Fase 5.79: las diez zonas y sus 30 spawns normal/elite/jefe reciben la misma
  capa de presentacion por zona/tier: modelo 3D, paleta, adornos, sombras,
  skinning movil, culling y fallback procedural. Los 16 controllers de mobs
  quedan regenerados con estados de combate y muerte.
- Fase 5.80: `MobileRuntimeDiagnostics` exporta snapshots JSON con dispositivo,
  perfil, FPS, memoria, safe area, enemigos y POI; la ventana TEST incorpora
  `EXPORTAR QA` y la matriz reproducible queda en `docs/android-qa.md`.
- Fase 5.81: se generan 30 FBX authored propios, uno normal/elite/jefe por
  zona, con atlas albedo/normal 2K compartido, skinning, LOD0/1/2 y cinco clips
  de combate. `EnemyVisualController` los prioriza con fallback y Unity genera
  30 controllers dedicados.
- El reporte `docs/content-completeness.md` confirma 17/17 checks: 10 zonas,
  35 misiones, 162 items, 30 tablas de loot, 15 modelos de mobs y modelos de
  personaje masculino/femenino disponibles. La auditoria actual pasa 17/17.
- APK Android recompilada y validada con Unity 6000.5.3f1 y `aapt2`: 42 MB,
  SHA-256 `4da6efe55fe57ff1b789875a2836b95fb277d7e1c04bd6c913f13ccbe347cbf6`.
- Cambios sincronizados en GitHub, rama `main`.

- Fases 5.89-5.90: pase offline audiovisual con anclajes de sombra/aura por
  tier, cielo y atmosfera, splash propio, anillos de combate y audio separado.
  La implementacion queda lista en codigo; falta regenerar la APK por la
  perdida temporal del servicio de licencia de Unity Hub.
- Fase 5.91: Atlas offline con las diez zonas, requisitos de nivel, jefe,
  reliquia y modo de prueba para teletransportar el personaje. El acceso y la
  seleccion de personaje usan fondos cinematograficos propios; falta la APK de
  validacion por el mismo bloqueo temporal de licencia.
- Fase 5.92: primera pasada de reemplazo artistico del entorno de Zona 1. Se
  integra una textura original de suelo con reliquias enterradas, musgo, roca y
  trazas magicas, junto con pinos, monolitos y ruinas ligeras de malla propia.
  Esta pasada mejora lectura y atmosfera, pero no sustituye todavia los FBX
  high-poly comerciales de personajes y mobs.

## Estado Real Del Arte

La interfaz y la direccion visual ya tienen una base cinematografica, pero los
modelos jugables siguen siendo una mezcla de assets CC0 y meshes authored de
transicion. Para llegar a una calidad comparable a MU Online o Metin2 faltan
modelado manual, texturas PBR pintadas, rigging y animaciones profesionales.
La siguiente prioridad artistica es completar la vertical slice de Zona 1 con
un Guerrero, un Ninja, el lobo corrupto y el jefe usando activos 3D finales;
despues se replica el pipeline a las nueve zonas restantes.

## Cierre De Fases 5.83-5.86

- Fase 5.83: HUD con EXP visible, nombre/nivel separados, objetivo
  contextual, cooldown radial, chat cerrado por defecto en Android y estado
  S-01 discreto.
- Fase 5.84: inventario, equipo y misiones convertidos a tarjetas visuales;
  habilidades con nodos conectados y estados de desbloqueo/mejora.
- Fases 5.85-5.86: cobertura verificada para las 8 variantes jugables y los
  30 mobs normal/elite/jefe de las 10 zonas, con atlas 2K, normales, skinning,
  LOD y cinco clips.
- El arte manual high-poly, texturas pintadas, VFX/audio profesional y
  animaciones finales siguen pendientes como salto comercial.
- APK Android recompilada: 77 MB, target SDK 35, SHA-256
  7d925d27c941a611f2fd1b5503d361c042b016e45d00619547b82b83e869d52b.
- Auditoria regenerada: 17/17 checks sin errores.

## Cierre De Fases 5.89-5.90

- Fase 5.89: personajes y mobs authored de las diez zonas reciben anclaje de
  sombra/aura por tier, cielo procedural, atmosfera movil y una presentacion
  mas consistente para la prueba offline.
- Fase 5.90: splash propio de Valle de las Reliquias, Unity splash oculto,
  anillos VFX para impactos/habilidades/ultimate/telegraph y audio separado
  para las acciones de combate.
- La nueva APK debe regenerarse cuando Unity Hub vuelva a tener licencia; la
  ultima APK validada continua siendo la de 77 MB con SHA-256
  `7d925d27c941a611f2fd1b5503d361c042b016e45d00619547b82b83e869d52b`.
- El arte high-poly manual comercial y la QA fisica en Android siguen siendo
  pendientes honestos, mientras la cobertura authored tecnica permanece en las
  ocho variantes y 30 mobs/jefes.

## Fase 5.92: Beta Offline Legible

- HUD compacto y coherente en editor/Android con referencia horizontal unica;
  chat y acciones secundarias ya no ocupan la partida al iniciar.
- Spawners acotados por zona: maximo de 4-5 normales y un elite activo,
  activacion a 46 unidades y jefe contextual al landmark. Esto evita que las
  diez zonas se mezclen visualmente en una sola captura.
- Superficie de suelo con variacion por bioma, marcos ambientales para las diez
  zonas y campamento comercial del Valle. Los nombres de POI salen del mundo y
  permanecen en minimapa/objetivos.
- Particulas reordenadas para que Unity no inicie el sistema antes de asignar
  duracion. APK y medicion en Android: en compilacion.

## Fase 5.93: Acceso Offline Y Modelos Legibles

- La pantalla de acceso offline conserva `S-01` como mundo local, pero ya no
  muestra URL WebSocket ni estado tecnico de red. Entrar al valle no abre una
  conexion automatica; la configuracion queda dentro de `Datos` para QA online.
- Los FBX authored `OriginalArt` pasan a ser la presentacion primaria de
  personajes y mobs de las diez zonas. KayKit/Quaternius queda como fallback
  tecnico para recursos faltantes o contenido legacy.
- El arma legacy deja de duplicarse sobre el avatar. Guerrero usa Knight,
  Ninja Rogue/Rogue Hooded, Chaman Mage y Umbra Barbarian en ambas variantes;
  sexo, paleta y accesorios siguen diferenciados por `CharacterArtProfile`.

## Fase 5.94: Reemplazo Artistico

- El acceso offline elimina por completo el selector de mundos; la eleccion de
  servidor queda reservada para la configuracion online futura.
- Se inicia el reemplazo de arte del prototipo con una direccion original de
  fantasia oscura oriental para personajes, mobs, equipamiento, zonas, UI y
  pantallas de acceso. La implementacion tecnica no sustituye la escultura,
  retopologia, texturas PBR y animaciones que deben producirse en Blender.
- El acceso usa ahora `valle-reliquias-access-roster-v1`, una ilustracion
  cinematografica original con los cuatro arquetipos; su composicion reserva
  el centro para la seleccion de personaje.

## Fase 5.91: Beta Offline Integrada

- El acceso y seleccion de personaje usan una escena propia de reliquiario;
  conservan tarjeta de guardado, crear personaje y selector S-01/S-02/S-03.
- `MAPA` abre el Atlas del Valle: presenta las diez zonas, nivel, mobs, elite,
  jefe y reliquia; en modo campana respeta nivel y en modo prueba permite QA
  directo de los mapas 1-10 sin falsear la progresion normal.
- La guia reproducible queda en `docs/offline-beta-qa.md`.
- La APK de este corte sigue pendiente de compilar: Unity Hub perdio el canal
  de licencia antes de importar scripts y recursos, por lo que no hay una
  validacion falsa de Android.

## Fase 5.95: Acceso de Cuenta para Beta Offline

- La portada deja que la ilustracion cinematografica sea visible: la
  seleccion de personaje se concentra en una tarjeta inferior compacta.
- El flujo ahora es `servidor -> cuenta -> seleccion/creacion de personaje`.
  S-01 funciona para la beta local; S-02 y S-03 quedan visibles pero bloqueados
  hasta que exista backend.
- Las cuentas locales validan usuario y contrasena sin guardar la contrasena
  en texto plano. Cada cuenta usa un archivo de personaje independiente en el
  dispositivo. No reemplaza autenticacion, recuperacion de cuenta ni seguridad
  de servidor para una beta publica.

## Fase 5.96: Refinamiento Visual del Acceso

- El formulario de acceso gana una jerarquia mas clara entre mundo de juego,
  servidor, cuenta y acciones principales.
- Se ajustan escala, espaciado, bordes, sombras y estados de botones para que
  el panel no parezca un bloque tecnico sobre la ilustracion.
- El marco 9-slice comun recibe un borde mas legible para reutilizarlo despues
  en seleccion de personaje, inventario, tienda y ventanas de combate.

## Fase 5.97: Siluetas Authored de Personajes y Zona 1

- Se incorpora un pequeno factory de meshes authored runtime para crear conos,
  placas facetadas, elipsoides y hojas sin colliders ni dependencias externas.
- Guerrero, Ninja, Chaman y Umbra reciben detalles de clase con una silueta mas
  clara: placas, hombreras, capucha, mascara, coronas, nucleos y armas.
- La variante femenina conserva la misma identidad de clase y escala visual,
  preparada para recibir los meshes skinned finales sin cambiar el contrato de
  equipamiento.
- Los mobs de Valle dejan de usar el mapeo visual generico de orcos. Zona 1
  presenta ahora lobos corruptos facetados, elite blindada y un guardian de
  reliquia con placas, ojos emisivos, orejas, cola y coronas diferenciadas.
- La geometria es una pasada authored de transicion optimizada para Android;
  no sustituye aun la escultura high-poly, retopologia, UV, atlas 2K y rigging
  profesional de produccion.
- La siguiente validacion debe comprobar escala, suelo, colision visual,
  lectura a distancia, animacion procedural y rendimiento con grupos de mobs.

## Fase 5.98: FBX Skinned Authored Primarios

- Las ocho variantes de personaje y los 30 mobs/jefes de las diez zonas pasan
  a instanciarse primero desde `Resources/OriginalArt`.
- Personajes y mobs conservan armature skinned, cinco clips (`Idle`, `Run`,
  `Attack`, `Hit`, `Death`), atlas 2K de albedo/normal y LOD0/LOD1/LOD2.
- Los mobs ahora reciben `LODGroup` real al instanciar sus FBX; KayKit,
  Quaternius y el factory procedural quedan como fallback de compatibilidad.
- Este corte activa el pipeline FBX authored dentro del juego, pero sigue
  siendo un arte original estilizado de producción técnica. La escultura
  manual high-poly, retopologia artística y animaciones profesionales siguen
  siendo el siguiente salto comercial.

## Fase 5.99: Gate De Validacion Authored Y Hotfix De Compilacion

- La auditoria de recursos visuales ahora exige los ocho FBX y ocho controllers
  de personajes, además de los 30 FBX y 30 controllers de mobs por zona/tier.
  Ya no basta con que existan únicamente los assets fallback de KayKit o
  Quaternius.
- Se corrigió el warning de API obsoleta de `ZonePointOfInterest` y el error de
  compilación del selector de servidor causado por la inicialización tardía de
  `selectedServerText`. El Asset Pipeline de Unity volvió a completar la
  compilación sin errores nuevos.
- La APK disponible antes de este corte sigue siendo anterior a la activación
  primaria de FBX. La validación visual y de rendimiento en Android queda
  pendiente de generar una APK nueva desde Unity y recorrer las diez zonas.

## Prioridad Recomendada

1. Generar la APK nueva desde `MMORPG > Android > Build Debug APK` y confirmar
   que el menu abre sin errores de compilación.
2. Generar `MMORPG > QA > Generate Content Completeness Report` y exigir los
   17/17 checks, incluyendo 8 personajes y 30 mobs authored.
3. Probar joystick + ataque + camara + habilidades simultaneamente y recorrer
   las diez zonas en Android.
4. Exportar QA en los perfiles Calidad y Rendimiento y superar
   `docs/offline-beta-qa.md`; despues preparar S-01 privado en
   OVHcloud Hillsboro o Hetzner tras comparar latencia real desde Chile, antes
   de convertir recompensas, inventario y compras en autoridad online.
5. Validar la nueva silueta authored de Zona 1 en Unity y Android; despues
   sustituir por lotes el authored estilizado mediante arte manual comercial:
   kit de cada clase, luego normal/elite/jefe de cada banda.
6. Al levantar S-01 online, sustituir `OfflineAccountStore` por cuentas de
   backend con hash de contrasena fuerte, tokens de sesion, rate limiting,
   verificacion de correo y recuperacion de cuenta.

## Siguiente Corte

1. Reinstalar el APK y probar durante varios minutos joystick + ataque + giro de
   camara, especialmente atravesando grupos de mobs.
2. Confirmar que el campamento de la Zona 1 no contiene mobs y que la transicion
   al campo de combate se entiende visualmente.
3. Abrir TEST y pulsar `EXPORTAR QA` en cada perfil y recorrido de zona; seguir
   la matriz de `docs/android-qa.md`.
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
10. Validar en un telefono real las nuevas reacciones `Hit`/`Death`, el peso de
    los cinco clips y la lectura de Guerrero/Ninja a distancia.
11. Prioridad de lanzamiento: levantar S-01 en Hetzner con firewall, TLS/WSS,
    persistencia protegida, backups y dos clientes Android antes de publicar.
12. Sustituir por encargo artistico las siluetas authored y los modelos base de
    mobs/jefes por esculturas high-poly manuales, retopologia, UV, atlas 2K y
    animaciones profesionales cuando el presupuesto de arte este aprobado.
13. Aplicar esa sustitucion por lotes: Zona 1 primero, luego zonas 2-4 y por
    ultimo 5-10; conservar contratos de nombres, LOD, hitboxes y controllers.
14. Ejecutar `MMORPG > QA > Generate Content Completeness Report` después de
    cada cambio de zonas, misiones, loot o equipos.
15. Probar contratos diarios y Renacimiento con varios personajes antes de
    mover sus recompensas a autoridad del servidor.
16. Probar cambio de lunes UTC, cambio de temporada, hitos 5/10/15/20/25/30
    calendario diario y persistencia de recompensas en Android.
17. Validar en S-01 horarios del jefe, membresias de clan, ranking y resultados
    del modo todos contra todos antes de habilitar recompensas online.

## Registro

| Fecha | Fase | Resultado | Global |
| --- | --- | --- | --- |
| 2026-07-18 | 5.98 | FBX skinned authored primarios: 8 personajes, 30 mobs/jefes, controllers de cinco clips y LODGroup de mobs; validacion Unity/Android pendiente | 61% beta offline |
| 2026-07-18 | 5.97 | Siluetas authored de las cuatro clases y lobos/guardian de reliquia en Zona 1; validacion Unity/Android pendiente | 60% beta offline |
| 2026-07-18 | 5.92 | HUD compacto, spawns por zona, suelo de bioma y campamento offline; APK requiere build desde Unity Hub | 60% beta offline |
| 2026-07-18 | 5.91 | Atlas offline 1-10, teletransporte de QA y acceso cinematografico; APK pendiente por licencia Unity | 98% |
| 2026-07-18 | 5.89-5.90 | Pase offline audiovisual, splash propio, grounding por tier y feedback de combate; APK pendiente por licencia Unity | 98% |
| 2026-07-17 | 5.83-5.86 | HUD contextual, menus visuales, nodos de habilidades y cobertura authored 3D en las 10 zonas; APK 77 MB | 98% |
| 2026-07-17 | 5.80-5.81 | QA Android exportable y 30 mobs/jefes authored propios para las 10 zonas, con atlas 2K, LOD y controllers | 98% |
| 2026-07-16 | 5.76-5.79 | Heroes Guerrero/Ninja detallados, cinco clips, reacciones de combate y tratamiento de mobs/jefes 1-105 | 98% |
| 2026-07-16 | 5.74-5.75 | High-poly authored V2, retopologia objetivo, UV padded y pintura 2K por material | 98% |
| 2026-07-16 | 5.73 | Bake normal tecnico desde high-poly temporal, atlas 2K por familia y compresion Android | 97% |
| 2026-07-16 | 5.72 | Atlas 1K, normal detail offline, geometria densificada y poses de combate refinadas | 96% |
| 2026-07-16 | 5.71 | LOD0/LOD1/LOD2 geometrico real, geometria suavizada y animaciones de combate de cuerpo completo | 95% |
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
