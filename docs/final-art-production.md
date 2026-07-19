# Produccion artistica final

Este documento fija la diferencia entre el prototipo jugable y el arte que se
puede presentar como produccion comercial. La existencia de un `.fbx`, un rig,
un atlas 2K o un `LODGroup` no convierte automaticamente un modelo en arte
final.

## Estado real actual

Los 8 FBX de personajes y los 30 FBX de mobs que estan en
`Assets/Resources/OriginalArt` son **arte tecnico de prototipo**. Fueron
generados para validar importacion, skinning, LOD, materiales, animaciones y
la integracion con Unity. Sus meshes proceden de construccion procedural y no
cumplen aun una entrega comercial comparable a un MMORPG 3D de referencia.

Por ese motivo el runtime mantiene una presentacion estable con KayKit y
Quaternius mientras se produce el reemplazo real. Reactivar los FBX tecnicos
solo para que aparezcan en pantalla seria una regresion visual, no un avance.

## Lote de produccion

El primer lote visible y revisable sera Zona 1:

- Guerrero masculino y femenino.
- Ninja masculino y femenino.
- Chaman masculino y femenino.
- Umbra masculino y femenino.
- NPC de campamento: Mercader, Almacen y Herrero.
- Familia de mobs de Valle: normal, elite y jefe.
- Armas iniciales, armaduras de demostracion y tres accesorios de lectura.
- Suelo, vegetacion, ruinas, VFX de combate y puntos de interes de la zona.

Despues de aprobar la silueta, escala, materiales y animaciones de este lote,
se replica el lenguaje visual para las otras nueve zonas. No se dara por
terminada una zona porque tenga un color diferente aplicado a un modelo viejo.

## Contrato de un personaje o mob

Cada entrega debe incluir:

1. Fuente `.blend` editable y FBX exportado desde esa fuente.
2. Mesh LOD0/LOD1/LOD2 con nombres semanticos y reduccion geometrica real.
3. Armature limpio, pesos revisados y una lista de huesos estable.
4. UV manual con padding y texel density consistente.
5. Materiales PBR: albedo, normal, roughness y metallic. AO y emissive cuando
   correspondan al diseño.
6. Texturas relativas, autocontenidas y con licencia documentada.
7. Clips `Idle`, `Run`, `Attack`, `Hit` y `Death`; los personajes añaden
   `Skill_01` a `Skill_04` y `Ultimate` cuando corresponda.
8. Revisión visual en cuatro ángulos, escala junto al jugador, lectura a la
   distancia de combate y prueba en Android.
9. Sidecar `.production.json` junto al FBX con procedencia y aprobación.

## Sidecar de aprobacion

Ejemplo mínimo, guardado como `Nombre.production.json` junto al FBX:

```json
{
  "status": "commercial_final",
  "asset_type": "character",
  "artist_or_vendor": "Nombre o proveedor",
  "license": "Licencia y enlace de compra o atribucion",
  "source_blend": "Assets/Art/Production/Nombre.blend",
  "reviewed_by": "Miguel Troncoso",
  "reviewed_at": "2026-07-19",
  "reviewed": true,
  "notes": "Silueta, materiales, rig, animaciones y Android aprobados"
}
```

La plantilla no acredita un asset por sí sola. La auditoria
`Tools/blender/validate_production_fbx.py` comprueba la estructura y exige
este registro para impedir que un prototipo vuelva a presentarse como final.

## Validacion

Desde la raiz del repositorio:

```bash
blender --background --python Tools/blender/validate_production_fbx.py -- \
  --all --report docs/production-art-audit.md \
  --json docs/production-art-audit.json
```

La salida actual debe ser `0/8` personajes y `0/30` mobs aprobados. Eso es
intencional: refleja la deuda real de arte y evita seguir acumulando fases
nominales. La auditoria funcional de contenido 1-105 es independiente y puede
seguir en 17/17.

## Dependencia que sigue faltando

Blender ya esta instalado y el pipeline tecnico esta listo. Lo que no puede
fabricarse con un script de Unity es la parte autoral: escultura high-poly,
retopologia manual, UV artística, pintura PBR coherente y animacion de combate
con criterio de produccion. Para cerrar el lote se necesita una fuente externa
de modelos con licencia comercial compatible o trabajo de un artista 3D. El
repositorio no debe fingir que ese trabajo ya existe.

## Fuente recomendada para iniciar la produccion

La fuente elegida como base de personajes es la **MMORPG Characters - Mega
Collection - Warriors, Archers, Mages** de Ida Faber. La decision y su alcance
estan documentados en `docs/art-source-decision.md`.

La coleccion aporta una familia modular de personajes, rig humanoide, armas y
materiales PBR preparados para Unity. No es un reemplazo automatico: Guerrero,
Ninja, Chaman y Umbra necesitan una pasada de identidad propia, optimizacion
Android, LOD, animaciones y sidecars de licencia. Tampoco incluye por si sola
mobs, NPC, entorno, VFX ni audio.

El paquete no esta instalado en este checkout. Hasta que se descargue desde la
cuenta con licencia, el estado correcto continua siendo `0/8` personajes y
`0/30` mobs comerciales aprobados.

## Orden de trabajo

1. Aprobar concept sheet y siluetas de las cuatro clases.
2. Modelar y revisar el lote de personajes y NPC de Zona 1.
3. Modelar la familia normal/elite/jefe y sus ataques.
4. Crear entorno, materiales, VFX y audio de Zona 1.
5. Integrar en Unity, probar Android y comparar capturas antes/después.
6. Replicar el lenguaje aprobado a las zonas 2-10.
