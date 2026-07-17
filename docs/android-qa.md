# QA Android Reproducible

Este documento registra la prueba real de la build Android y separa lo que se
puede validar automaticamente de lo que requiere un telefono fisico.

## Instrumentacion dentro del juego

Abre `TEST` durante una partida. La ventana muestra resolucion, orientacion,
safe area, FPS actual/promedio/minimo, memoria asignada, enemigos y POI activos.
`EXPORTAR QA` guarda un JSON en:

```text
Application.persistentDataPath/qa/android-qa-YYYYMMDD-HHmmss.json
```

Para extraerlo desde el almacenamiento app-specific con ADB:

```bash
adb pull /sdcard/Android/data/com.migueltroncoso.valledelasreliquias/files/qa/android-qa-YYYYMMDD-HHmmss.json .
```

El nombre del paquete puede confirmarse con `aapt2 dump badging` si cambia en
una build futura.

## Matriz minima

Ejecutar cada recorrido en `RENDIMIENTO` y `CALIDAD` cuando el dispositivo lo
permita:

| Recorrido | Validar |
| --- | --- |
| Inicio y acceso | carga inicial, selector, safe area y orientacion horizontal |
| Zona 1 | joystick + ataque + camara simultaneos, regeneracion y minimapa |
| Zonas 2-4 | densidad de mobs, LOD, impactos, muerte y memoria |
| Zonas 5-7 | jefes, habilidades, telemetria y estabilidad de FPS |
| Zonas 8-10 | atlas, materiales, minimapa, jefe mundial y carga sostenida |

En cada zona registrar al menos 60 segundos quieto, 60 segundos corriendo y
una pelea con tres enemigos. Exportar un JSON al terminar cada perfil.

## Criterios de salida

- No hay pantalla gris prolongada ni orientacion vertical durante el combate.
- El centro queda libre para el personaje y el objetivo.
- Joystick, camara, ataque y habilidades aceptan multitactil sin perder input.
- `RENDIMIENTO` prioriza estabilidad y `CALIDAD` mantiene lectura visual sin
  errores de memoria.
- Ningun modelo authored bloquea la carga: si falta un FBX o controller,
  `EnemyVisualController` usa Quaternius o el fallback procedural.

La ejecucion automatica valida compilacion, auditoria, estructura FBX y APK.
Los FPS, memoria, temperatura, consumo y comodidad tactil solo se pueden
cerrar con el dispositivo Android conectado.
