# Direccion 2D Isometrica Gratuita

## Decision

El proyecto adopta una presentacion **2D isometrica HD/pixel**, con camara
ortografica y lectura de MMORPG clasico. Esta decision evita comprar un paquete
3D y permite conservar la progresion, el inventario, las misiones, los eventos,
las clases y el futuro backend que ya existen.

La conversion se hara por cortes jugables. El primer corte de Zona 1 ya tiene
movimiento por toque, joystick, ataque, regeneracion, minimapa, NPC, campamento
seguro y criaturas visibles. El runtime 3D permanece como respaldo tecnico
mientras se valida la direccion 2D.

## Fuentes Gratuitas

### Puny Characters

- Fuente: [Puny Characters en OpenGameArt](https://opengameart.org/content/puny-characters)
- Descarga usada: `Assets/Resources/Art/IsometricFree/Source/PunyCharacters/`
- Licencia: CC0, uso comercial y no comercial permitido por la ficha del
  recurso.
- Uso actual: Warrior-Blue para el avatar inicial, Slime para criaturas de
  Zona 1 y la misma familia como base de futuras clases/NPC.
- Resolucion: sprites de 32x32 por frame, apropiados para un primer corte
  movil. Para una beta comercial se debe validar la escala final y sustituir
  los sprites de prueba por una familia propia coherente si la lectura resulta
  demasiado pixelada.

### Kenney Isometric City

- Fuente: [Kenney Isometric Tiles City](https://kenney.nl/assets/isometric-tiles-city)
- Descarga de referencia: se conserva fuera del repositorio hasta usarla en la
  zona comercial, para no inflar la primera APK con sprites que aun no aparecen
  en pantalla.
- Licencia: CC0, confirmada en el `License.txt` del paquete descargable.
- Uso previsto: campamento, comercio y props de la ciudad cuando se complete
  la zona segura. No se mezcla con el primer corte hasta definir la paleta.

### Kenney Isometric Landscape

- Fuente: [Kenney Isometric Tiles Landscape](https://kenney.nl/assets/isometric-tiles-landscape)
- Uso previsto: biomas, caminos, rocas y vegetacion de las zonas 1-10.
- Licencia: el soporte oficial de [Kenney](https://kenney.nl/support) indica
  que los assets de sus paginas se publican como CC0 y pueden usarse en
  proyectos comerciales.

### Screaming Brain Overworld

- Fuente: [Isometric Tiles - Overworld Pack](https://screamingbrainstudios.itch.io/iso-overworld-pack)
- Uso previsto: alternativa para mapas de campo y transiciones de bioma si la
  familia Kenney no cubre la lectura fantastica que buscamos.
- Licencia: la pagina y el sitio del autor indican CC0 y permiten uso,
  modificacion y redistribucion comercial.

## Reglas De Integracion

1. Una zona debe usar una familia principal de tiles y una paleta definida.
2. Personajes, mobs y NPC deben compartir escala, sombra, outline y velocidad
   de animacion.
3. No se mezclan sprites pixel-art, renders 3D y props low-poly en la misma
   escena sin una capa artistica que los unifique.
4. Cada recurso importado conserva su licencia o un sidecar de procedencia.
5. El cliente no debe depender de una URL externa en tiempo de ejecucion.
6. El minimapa, la interfaz y los iconos se mantienen como sistema propio del
   proyecto y no se copian de juegos existentes.

## Estado Del Primer Corte

- Camara isometrica ortografica: implementada.
- Zona 1 visual y campamento seguro: implementados como vertical slice.
- Jugador 2D CC0 con frames de animacion: implementado.
- Tres criaturas 2D CC0 y dos guardianes de prueba: implementados.
- Joystick, toque para moverse, ataque, feed y minimapa: implementados.
- Integracion con el inventario/progresion/backend existente: pendiente.
- Diez zonas con tiles 2D finales: pendiente.
- Elenco final de cuatro clases, sexo masculino/femenino, NPC, mascotas,
  monturas y jefes: pendiente.

## Fases Siguientes

### 6.07: Vertical Slice 2D de Zona 1

Reemplazar el tablero procedural por tiles isometricos importados, usar una
paleta unificada de campo/campamento y conectar el combate del corte con los
servicios de loot, misiones, inventario y guardado que ya existen.

### 6.08: Elenco 2D jugable

Crear cuatro siluetas de clase y sus variantes masculina/femenina con frames
de idle, caminar, ataque, impacto y muerte. La prioridad es que Guerrero,
Ninja, Chaman y Umbra se reconozcan sin leer texto.

### 6.09: Mobs, NPC y drops de Zona 1

Crear familias normal, elite y jefe con proporciones consistentes, etiquetas
limpias, barras de vida y drops visibles. Sustituir los guardianes de prueba
por criaturas propias o CC0 compatibles con la misma paleta.

### 6.10: Mapa 2D de las diez zonas

Construir por bandas de nivel, primero zonas 1-3, luego 4-7 y por ultimo 8-10.
Cada zona debe tener suelo, landmark, campamento o entrada, familia de mobs,
jefe, minimapa y regla de zona segura.

### 6.11: Accesorios y lectura de progresion

Representar armas, armaduras, alas, mascotas y monturas como capas 2D o
sprites equipables. El inventario mostrara iconos de la misma familia visual.

### 6.12: QA Android y decision de beta

Validar 16:9 y 19.5:9, multitactil, memoria, FPS, carga, escala de sprites,
legibilidad y 17/17 checks. Solo despues se decide si el corte 2D pasa a ser la
ruta unica para el cliente online.

## Nota Comercial

CC0 evita pagar por licencias, pero no elimina el trabajo de produccion. Para
una apariencia comparable a un MMORPG comercial todavia hay que diseñar
variantes, animaciones, iconos, mapas, VFX, audio, QA y consistencia de arte.
Las fuentes gratuitas son la base permitida; no se presentan como arte final
de lanzamiento hasta completar las fases anteriores.
