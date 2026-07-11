# Checklist Android Y Google Play

## Estado Actual

El prototipo ya tiene una base para Android:

- Escena incluida en Build Settings: `Assets/Scenes/Prototype.unity`.
- Menu de Unity: `MMORPG > Android`.
- Menu de branding: `MMORPG > Branding`.
- Pantalla de carga inicial.
- Safe area para celulares con notch.
- Feed de actividad para loot y objetivos.
- Configuracion runtime a 60 FPS.
- Bundle ID propuesto: `com.migueltroncoso.valledelasreliquias`.
- Version inicial: `0.1.0`.

## Instalar Modulo Android

En Unity Hub:

1. Abre `Installs`.
2. En Unity `6000.5.3f1`, usa el menu de opciones.
3. Selecciona `Add modules`.
4. Instala:
   - Android Build Support.
   - Android SDK & NDK Tools.
   - OpenJDK.

Sin ese modulo, Unity puede correr el juego en Mac, pero no puede generar APK o AAB.

## Build Para Probar En Telefono

En Unity:

1. Abre el proyecto.
2. Usa `MMORPG > Android > Apply Android Settings`.
3. Conecta un telefono Android con USB debugging activado.
4. Usa `MMORPG > Android > Build Debug APK`.
5. Instala el APK en el telefono para una prueba local.

Para probar online en telefono, cambia la URL del servidor dentro del juego desde `ws://localhost:7777` a la IP local del Mac, por ejemplo `ws://192.168.1.14:7777`.

## Arte Placeholder

Para crear una base visual temporal:

```text
MMORPG > Branding > Generate Placeholder Store Art
```

Esto genera:

- `Assets/Art/Generated/valle-reliquias-icon-placeholder.png`
- `Assets/Art/Generated/valle-reliquias-splash-placeholder.png`

Estos archivos sirven para pruebas internas. Antes de publicar deben reemplazarse por arte final propio.

## Build Para Google Play

En Unity:

1. Usa `MMORPG > Android > Apply Android Settings`.
2. Usa `MMORPG > Android > Build Google Play AAB`.
3. Sube el archivo `.aab` a Play Console.

Antes de subir una version real faltan:

- Icono final.
- Screenshots de tienda.
- Descripcion corta y larga.
- Politica de privacidad publicada en una URL.
- Clasificacion de contenido.
- Data Safety.
- Firma de app con Play App Signing.
- Pruebas en varios telefonos.

## Requisitos Google Play A Revisar

Google Play cambia los requisitos por fecha. Antes de subir, revisar siempre:

- Target API: https://support.google.com/googleplay/android-developer/answer/11926878
- Guia tecnica target SDK: https://developer.android.com/google/play/requirements/target-sdk
- Testing para cuentas personales nuevas: https://support.google.com/googleplay/android-developer/answer/14151465
- Preparar app para revision: https://support.google.com/googleplay/android-developer/answer/9859455

Segun la documentacion consultada durante esta fase, desde el 31 de agosto de 2025 las apps nuevas y actualizaciones deben apuntar a Android 15/API 35 o superior. Si publicamos despues de cambios nuevos de politica, hay que actualizar este numero.

Para cuentas personales nuevas, Google puede pedir un test cerrado con al menos 12 testers opt-in durante 14 dias continuos antes de acceder a produccion.
