# Reemplazo Artistico: Valle de las Reliquias

## Direccion fijada

Fantasía oscura oriental original para Android: ruinas antiguas, reliquias,
metal grabado, tela bordada, cuero envejecido, cristal violeta y magia de alto
contraste. Es una referencia de sensacion, no una copia de MU Online, Metin2 u
otro juego existente.

La primera ilustracion de referencia vive en
`Assets/Resources/Art/Generated/valle-reliquias-access-roster-v1.png`. Sirve
para acceso y seleccion de personaje; no es un sustituto de un modelo 3D.

## Personajes jugables

Cada clase requiere una variante masculina y femenina que conserve su lectura
desde camara isometrica movil:

| Clase | Lectura | Arma | Materiales y color |
| --- | --- | --- | --- |
| Guerrero | placa pesada, hombreras, capa corta | espada y escudo | acero azul, oro viejo, brasa |
| Ninja | mascara, capucha, silueta estrecha | dos dagas | cuero oscuro, plata, azul frio |
| Chaman | tunica ritual, talismanes, baston | baston rúnico | marfil, turquesa, oro |
| Umbra | armadura maldita, capa larga, runas | mandoble oscuro | negro, amatista, violeta |

Objetivo tecnico por variante: mesh skinned humanoide, 20k-35k triangulos en
LOD0, LOD1 y LOD2, UV unico, atlas albedo/normal/metallic-roughness 2K,
materiales URP y socket para arma, alas, mascota y montura.

## Mobs y jefes

Cada zona necesita una familia con normal, elite y jefe. No se reutiliza una
misma silueta cambiando solo el color. Cada familia necesita ataque, impacto,
muerte y telegraph propios; los jefes agregan una silueta y VFX reconocibles.

| Zona | Familia visual |
| --- | --- |
| Valle de las Reliquias | lobos corrompidos y guardianes de reliquia |
| Bosque de los Susurros | bestias de corteza y brujas del musgo |
| Cenizas del Juramento | caballeros de ceniza y demonios de escoria |
| Colinas Cenicientas | gólems de piedra y colosos funerarios |
| Cumbres de Cristal | centinelas de cristal y dragones prismáticos |
| Paso Glacial | yetis blindados y espíritus de escarcha |
| Ruinas Sumergidas | sirvientes abisales y leviatanes |
| Forja Obsidiana | herreros infernales y demonios de magma |
| Jardin Astral | espectros celestes y custodios estelares |
| Santuario del Eclipse | inquisidores umbríos y soberano del eclipse |

## Mundo, equipo e interfaz

- Zonas: suelos PBR, caminos, ruinas, vegetacion, niebla, agua y skybox por
  bioma, con presupuesto de LOD e instancing para Android.
- Equipo: sets visibles por tier, armas con silueta propia, alas, mascotas y
  monturas con materiales coherentes; los iconos salen del mismo atlas de arte.
- UI: paneles de obsidiana y metal, marcos 9-slice, iconos originales,
  cooldown radial y tipografia legible. Nada de botones rectangulares planos
  como presentacion final.
- Acceso: ilustracion cinematografica, seleccion de servidor, cuenta local y
  despues selector de personaje. La URL y los errores tecnicos siguen fuera
  de la portada; S-02 y S-03 se habilitaran con el backend online.

## Orden de produccion

1. Aprobar hojas de concepto de las cuatro clases y de Zona 1.
2. Modelar, retopologizar, hacer UV, bake PBR y rig del Guerrero/Ninja.
3. Sustituir mobs y jefe de Zona 1, validar rendimiento Android.
4. Aplicar el pipeline a Chaman/Umbra, equipo, mascotas y monturas.
5. Repetir por las otras nueve zonas, despues menus/HUD/tienda con el atlas UI.

El codigo actual ya permite intercambiar FBX y materiales; el cuello de botella
real es la produccion de activos 3D, no la logica MMORPG.
