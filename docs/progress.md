# Estado Y Avance

Este porcentaje es una estimacion de producto, no un conteo de scripts. Un
MMORPG completo tambien necesita arte final, contenido, servidores, pruebas,
economia, seguridad, operacion y publicacion.

## Corte Diario: 2026-07-13

- Avance global estimado hacia el MMORPG publicable: **60%**.
- Prototipo jugable offline: **97%**.
- Vertical slice de una zona pulida: **88%**.
- Online persistente y preparado para produccion: **28%**.
- Arte final, contenido completo 1-105 y lanzamiento: **36%**.

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
- APK Android recompilada y validada con Unity 6000.5.3f1 y `aapt2`.
- Cambios sincronizados en GitHub, rama `main`.

## Siguiente Corte

1. Reinstalar APK y registrar desde TEST FPS, memoria, carga y objetos activos
   en las diez zonas del telefono real.
2. Ajustar cantidad de mobs, VFX, decoracion y radio del minimapa con esos
   datos.
3. Fase 5.48: medir en telefono real y ajustar mobs, VFX, decoracion, TTK y
   minimapa con datos reales.
4. Fase 5.48: validar escala, lectura, FPS y memoria de los diez jefes en
   telefono real; ajustar adornos y materiales si algun modelo pierde lectura.
5. Fase 5.45: sustituir progresivamente el set CC0 por meshes finales propios
   cuando la direccion artistica este cerrada.
6. Fase 5.49: mover la validacion definitiva de recompensas al servidor y
   revisar duplicados/concurrencia con telemetria real.

## Registro

| Fecha | Fase | Resultado | Global |
| --- | --- | --- | --- |
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
