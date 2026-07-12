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

## Fase 6: Lanzamiento Inicial

Objetivo: publicar una version pequena y mantenerla.

- Acceso de produccion en Google Play.
- Analiticas basicas.
- Reporte de errores.
- Primer calendario de eventos.
- Canal de comunidad.

Criterio de exito: jugadores reales pueden instalar, jugar y dar feedback.
