# Estado Y Avance

Este porcentaje es una estimacion de producto, no un conteo de scripts. Un
MMORPG completo tambien necesita arte final, contenido, servidores, pruebas,
economia, seguridad, operacion y publicacion.

## Corte Diario: 2026-07-12

- Avance global estimado hacia el MMORPG publicable: **43%**.
- Prototipo jugable offline: **92%**.
- Vertical slice de una zona pulida: **74%**.
- Online persistente y preparado para produccion: **25%**.
- Arte final, contenido completo 1-105 y lanzamiento: **24%**.

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
- APK Android recompilada y validada con Unity 6000.5.3f1 y `aapt2`.
- Cambios sincronizados en GitHub, rama `main`.

## Siguiente Corte

1. Reinstalar APK y probar dos dedos en un telefono real.
2. Fase 5.45: sustituir los arquetipos KayKit por criaturas 3D unicas de cada
   familia, empezando por la Zona 1.
3. Fase 5.48: medir rendimiento, balance y experiencia Android en las diez
   zonas con el telefono real.
4. Fase 5.49: convertir landmarks en puntos de interes con recompensas y
   ajustar obstaculos, TTK, EXP y oro.

## Registro

| Fecha | Fase | Resultado | Global |
| --- | --- | --- | --- |
| 2026-07-12 | 5.49 | Obstaculos solidos, POI explorables y reporte de balance | 43% |
| 2026-07-12 | 5.45-5.48 | Variantes por zona, rutas, landmarks y optimizacion Android | 40% |
| 2026-07-12 | 5.44-5.47 | HUD, mobs 3D, zonas 1-10 y feedback de combate | 36% |
| 2026-07-12 | 5.43 | Mobs con familias visuales, tiers y barras de vida | 30% |
| 2026-07-12 | 5.42 | Multitactil de joystick + combate | 29% |
| 2026-07-12 | 5.41 | Suelo, recuperacion de caidas y escala Android | 28% |
