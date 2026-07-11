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

## Fase 6: Lanzamiento Inicial

Objetivo: publicar una version pequena y mantenerla.

- Acceso de produccion en Google Play.
- Analiticas basicas.
- Reporte de errores.
- Primer calendario de eventos.
- Canal de comunidad.

Criterio de exito: jugadores reales pueden instalar, jugar y dar feedback.
