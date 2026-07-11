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
