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

## Acceptance

Antes de avanzar a los mobs finales, las ocho variantes deben verse distintas
en seleccion de personaje y dentro de la Zona 1, conservar sus ranuras de
equipo, animarse en idle/correr/ataque y no producir una caida de rendimiento
visible en Android `Performance`.
