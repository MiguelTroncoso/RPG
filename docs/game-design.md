# Diseno Base Del Juego

## Fantasia Central

Un mundo de fantasia oriental oscura donde cuatro ordenes compiten y cooperan contra criaturas corruptas que aparecen alrededor de reliquias antiguas. El jugador crea un heroe, sube de nivel, mejora equipo, aprende habilidades y participa en eventos de mapa con otros jugadores.

## Sensacion Buscada

- Combate rapido, claro y satisfactorio.
- Progresion visible: nivel, equipo, habilidades y apariencia.
- Mapas con zonas de monstruos, jefes y eventos.
- Estetica de fantasia marcial: armaduras, espadas, magia espiritual, energia oscura y clanes.
- Experiencia mobile: sesiones cortas, UI limpia y controles simples.

## Camara Y Control

Para Android, el primer prototipo debe usar:

- Joystick virtual izquierdo para movimiento.
- Boton de ataque a la derecha.
- 2 a 4 botones de habilidad.
- Camara tercera persona con distancia media.
- Target automatico al enemigo mas cercano dentro del rango.

Esto permite acercarnos al estilo MMORPG clasico sin hacer que el control tactil sea pesado.

## Clases

### Guerrero

Rol: tanque/dano cuerpo a cuerpo.

- Alta vida.
- Buen dano cercano.
- Habilidades de area corta.
- Armas: espada, hacha, mandoble.

Habilidades MVP:

- Corte pesado: golpe frontal con dano alto.
- Grito de batalla: aumenta defensa o dano por pocos segundos.

### Ninja

Rol: dano rapido, movilidad y criticos.

- Ataques rapidos.
- Menos vida.
- Puede usar dagas o arco en fases futuras.

Habilidades MVP:

- Estocada veloz: avance corto con dano.
- Sombra fugaz: aumenta velocidad y probabilidad critica por pocos segundos.

### Chaman

Rol: magia, soporte y control.

- Dano magico medio.
- Puede curarse o apoyar aliados.
- Ideal para juego cooperativo.

Habilidades MVP:

- Rayo espiritual: proyectil magico.
- Bendicion: cura pequena o escudo temporal.

### Umbra

Rol: espada y energia oscura.

- Mezcla de cuerpo a cuerpo y magia.
- Consume energia para ataques fuertes.
- Estilo visual mas oscuro.

Habilidades MVP:

- Hoja oscura: golpe con dano magico.
- Marca del vacio: debilita al enemigo por pocos segundos.

## Primer Mapa

Nombre temporal: Valle de las Reliquias.

Elementos:

- Aldea segura pequena.
- Zona de monstruos faciles.
- Zona de monstruos medianos.
- Evento central con una reliquia corrupta.
- Cofres o drops simples.

## Evento Central MVP

En vez de copiar la piedra de otro juego, usaremos una reliquia propia.

Nombre temporal: Monolito Corrupto.

Reglas:

- Aparece cada cierto tiempo en una zona del mapa.
- Tiene mucha vida.
- Invoca monstruos pequenos.
- Al destruirse, entrega experiencia y loot.
- En online, todos los jugadores cercanos reciben recompensa proporcional.

## Progresion Inicial

- Nivel maximo MVP: 10.
- Experiencia por monstruo.
- 3 tipos de equipo: arma, armadura, accesorio.
- Rarezas iniciales: comun, raro, epico.
- Oro basico para comprar pociones o mejorar equipo despues.

