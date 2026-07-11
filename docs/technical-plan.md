# Plan Tecnico Recomendado

## Motor

Recomendacion: Unity.

Motivos:

- Buen soporte para Android.
- Exportacion a Android App Bundle.
- Mucha documentacion y assets.
- Mejor ecosistema para 3D mobile que muchas alternativas.
- Facilidad para iterar con prototipos visuales.

Godot tambien es viable si buscamos algo mas liviano y open source, pero para un MMORPG 3D mobile con crecimiento a largo plazo, Unity da mas camino comercial.

## Arquitectura Inicial

### Cliente

- Unity 3D.
- C#.
- Input mobile con joystick virtual.
- UI con Canvas o UI Toolkit.
- Sistema de habilidades data-driven con ScriptableObjects.
- Datos de clase, habilidades y enemigos definidos como assets configurables.

### Servidor

Para el MVP online hay dos rutas buenas:

1. Colyseus + Node.js.
   - Rapido para prototipos.
   - Facil de entender.
   - Bueno para salas/mundos pequenos.

2. Nakama.
   - Mas completo para juegos online.
   - Trae usuarios, matchmaking, almacenamiento y funciones de servidor.
   - Mejor si queremos crecer con menos backend propio.

Recomendacion practica:

- Fase offline: no usar servidor.
- Primer online: Colyseus si queremos velocidad.
- Si el juego toma forma seria: evaluar Nakama antes de escalar.

## Servidor Local Actual

La primera capa online usa un servidor WebSocket propio en `Server/`.

- Runtime: Node.js.
- Dependencia: `ws`.
- Puerto por defecto: `7777`.
- Protocolo: JSON simple.
- Sistemas actuales: conexion, snapshot de jugadores, posicion y chat.

Este servidor no reemplaza una solucion final de MMORPG. Es una base para probar sensacion online antes de invertir en cuentas, base de datos, seguridad, persistencia o hosting.

## Contenido Local Actual

La primera capa de retencion vive todavia dentro del cliente Unity para iterar rapido antes de persistir datos online.

- `InventorySystem`: guarda objetos simples y actualiza el HUD.
- `PlayerQuestLog`: controla la mision inicial de enemigos, fragmentos y monolito.
- `EquipmentUpgradeSystem`: aplica mejoras de arma y armadura.
- `ShopNpc`: valida cercania al mercader y ejecuta compras/mejoras.
- `WorldEventSystem`: crea el Monolito Corrupto, recompensas y respawn.

Cuando el prototipo sea divertido, estos datos deberian moverse gradualmente al servidor: inventario, oro, nivel, recompensas de evento y mejoras de equipo.

## Sistemas Del MVP

- CharacterController: movimiento del jugador.
- CombatController: ataque basico, rango, cooldown.
- SkillSystem: habilidades por clase.
- EnemyAI: patrulla, persecucion, ataque.
- ProgressionSystem: experiencia, nivel y stats.
- LootSystem: drops simples.
- InventorySystem: inventario basico.
- WorldEventSystem: monolito corrupto/evento central.

## Datos Iniciales

Las clases deberian tener stats base:

- Vida.
- Energia o mana.
- Ataque fisico.
- Ataque magico.
- Defensa.
- Velocidad.
- Critico.

Las habilidades deberian definir:

- Nombre.
- Clase permitida.
- Dano o efecto.
- Cooldown.
- Rango.
- Coste.
- Animacion.
- VFX.

## Android Y Google Play

Para publicar en Google Play se necesita:

- Cuenta de Google Play Developer.
- Build en formato Android App Bundle.
- Target SDK actualizado segun la politica vigente.
- Politica de privacidad.
- Ficha de tienda: nombre, descripcion, screenshots, icono.
- Clasificacion de contenido.
- Test cerrado si la cuenta lo requiere.

### Estado Android Actual

El proyecto tiene una herramienta de preparacion en `Assets/Editor/AndroidBuildTools.cs`.

- Nombre del producto: Valle de las Reliquias.
- Bundle ID: `com.migueltroncoso.valledelasreliquias`.
- Version inicial: `0.1.0`.
- Min SDK: API 26.
- Target SDK configurado por la herramienta: API 35.
- Scripting backend Android: IL2CPP.
- Arquitectura Android: ARM64.
- Formato Google Play: Android App Bundle `.aab`.
- Safe area runtime: `SafeAreaFitter`.
- Feed de actividad runtime: `PrototypeHud.AddFeed`.
- Arte placeholder: `BrandAssetGenerator`.

En este Mac todavia no aparece instalado el modulo `AndroidPlayer` dentro de Unity 6.5. Para generar builds reales hay que instalar Android Build Support desde Unity Hub.

## Identidad De Personaje Actual

La creacion de personaje MVP vive en el cliente para iterar rapido:

- `CharacterGender`: enum Masculino/Femenino.
- `PlayerCharacterIdentity`: nombre, sexo, etiqueta visible, escala/color temporal y nombre online.
- `PlayerClassController`: aplica clase y reenvia el color/visual a la identidad.
- `PrototypeBootstrap`: crea el panel inicial de seleccion antes de activar el gameplay.

El panel pausa el mundo con `Time.timeScale = 0` hasta que el jugador presiona `CREAR`. En una fase futura, esta identidad debe persistirse en servidor/base de datos.

## Avatar Visual Actual

El avatar visual sigue siendo procedural para evitar depender de assets antes de validar el gameplay.

- `PlayerAvatarVisual`: construye torso, cabeza, brazos y rasgos por clase con primitivas Unity.
- `PlayerCharacterIdentity`: envia clase/sexo al visual y mantiene nombre/etiqueta.
- `NetworkRemotePlayer`: usa el mismo visual para jugadores remotos, por clase. El sexo remoto aun no se sincroniza.

Cuando se importen modelos reales, `PlayerAvatarVisual` deberia transformarse en un selector de prefabs por clase/sexo.

## Riesgos Principales

- Alcance demasiado grande.
- Falta de assets 3D y animaciones.
- Online prematuro antes de que el combate sea divertido.
- Optimizacion en celulares de gama baja.
- Seguridad del servidor si se permite progreso online real.

## Decision Recomendada

Construir primero un prototipo offline en Unity. Cuando el movimiento, camara y combate se sientan bien, recien conectar online.
