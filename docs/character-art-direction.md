# Direccion Artistica De Personajes

## Decision

Valle de las Reliquias usara una fantasia realista estilizada y cinematografica:
formas legibles desde la camara de combate, materiales metalicos controlados,
acentos emisivos pequenos y paletas propias por clase. No se busca
hiperrealismo, porque el objetivo es mantener una lectura clara y estable en
Android.

## Eight Profiles

| Clase | Masculino | Femenino | Silueta | Firma visual |
| --- | --- | --- | --- | --- |
| Guerrero | Vanguard | Vanguard | ancha y pesada | hombreras, cresta y gema de pecho |
| Ninja | Veil | Veil | estrecha y agil | capucha, mascara, faja y dagas |
| Chaman | Spirit | Spirit | vertical y ritual | capucha, corona, orbe y collar |
| Umbra | Void | Void | angular y amenazante | corona bifurcada, nucleo y hombreras |

Las variantes masculina y femenina comparten animaciones y contratos de
equipamiento, pero no deben verse como el mismo avatar recoloreado: cambian la
escala corporal, la proporcion de hombros, la capa y los accesorios de clase.

## Android Budget

- Un esqueleto y un controlador por familia de clase.
- Materiales compartidos y pocos emisivos para reducir cambios de estado.
- Accesorios modulares sin colisionadores en el avatar.
- Silueta reconocible a distancia antes que detalle pequeno.
- El perfil `Performance` debe poder ocultar accesorios cosmeticos secundarios.

## Runtime Implementation

- `CharacterArtProfiles` define las ocho combinaciones y su contrato visual.
- `PlayerAvatarVisual` mantiene el modelo FBX y aplica la firma de clase encima,
  de modo que los assets finales puedan reemplazarse sin cambiar progresion,
  sexo, equipamiento ni animacion.
- `DefaultArmorVisualSets` conserva el contrato de sets y rarezas mientras se
  sustituyen las piezas procedurales por meshes finales.

## Fase 5.66 3+4: Salto 3D Y Zona 1

- Las armas de clase cargan los meshes FBX reales de KayKit: espada de una
  mano, dagas dobles, baston y espada de dos manos.
- El arma de clase solo aparece cuando no hay un arma equipada; al equipar un
  objeto se conserva una unica composicion visual y se usa el mesh 3D de la
  clase para representar su familia.
- La Zona 1 usa `Tribal` para el mob normal, `Orc` para el elite y `Orc_Skull`
  para el jefe, con tintes de reliquia, cicatriz, sigilo y adornos de tier.
- Los modelos Quaternius reciben tratamiento de material por zona, nivel y
  jerarquia, manteniendo los originales CC0 y evitando duplicar materiales.

Esta entrega cierra la primera vertical slice artistica 3D con assets CC0. El
siguiente reemplazo de mayor calidad sera arte propio o comercial de los ocho
personajes y de la familia de mobs, sin cambiar los contratos de gameplay.

## Fase 5.67: Primer Arte Propio 3D

`OriginalArtVisualFactory` genera mallas low-poly originales directamente con
la API de Unity, sin reutilizar geometria externa:

- Guerrero masculino: torso blindado, casco con cresta, hombreras, capa, botas,
  espada, gema de pecho y extremidades que responden al movimiento procedural.
- Mob normal de Zona 1: bestia de reliquia con cuerpo, cabeza, hocico, orejas,
  cuatro patas, cola, cicatriz, espina y ojos emisivos.

La geometria queda limitada a pocos vertices, materiales compartidos y cero
colisionadores visuales para mantener el perfil `Performance` de Android. Esta
referencia se amplio en la Fase 5.68 hasta cubrir las ocho variantes jugables.

## Fase 5.68: Variantes, Texturas Y Rig Modular

`OriginalArtVisualFactory` ahora construye las ocho combinaciones de clase y
sexo con piezas propias: Vanguard para Guerrero, Veil para Ninja, Spirit para
Chaman y Void para Umbra. Cada familia tiene arma inicial, silueta, paleta,
accesorios y proporciones femeninas/masculinas diferenciadas.

- Los meshes generan UVs en runtime y usan microtexturas albedo 8x8 cacheadas
  para placa, tela, cuero, escama, runa, piedra y hueso.
- El rig modular expone `Hips`, `Spine`, `Chest`, `Neck`, brazos, manos, piernas,
  pies, `WeaponGrip` y `SpellHand`; los brazos y piernas siguen siendo
  transforms articulados compatibles con `AvatarMotionAnimator`.
- `OriginalRigDescriptor` conserva clase y sexo para herramientas futuras y
  exportacion de un rig skinned.
- Las texturas son generadas en runtime, no dependen de paquetes externos y
  mantienen el perfil `Performance` de Android.

Esto cierra el bloque de variantes propias y la primera pasada de texturas y
rigging runtime. El siguiente paso de calidad comercial es exportar meshes
skinned, atlas de mayor resolucion, normales, LOD y animaciones FBX finales.

## Acceptance

Las ocho variantes deben verse distintas en seleccion de personaje y dentro de
la Zona 1, conservar sus ranuras de equipo, animarse en idle/correr/ataque y no
producir una caida de rendimiento visible en Android `Performance`.
