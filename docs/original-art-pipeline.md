# Pipeline tecnico de arte 3D

Blender 5.2 y Unity 6000.5.3f1 estan preparados para importar, revisar e
integrar personajes y mobs. Este documento describe el pipeline tecnico; el
contrato para aprobar arte comercial esta en
[`final-art-production.md`](final-art-production.md).

## Herramientas instaladas

- Blender 5.2 LTS para macOS Apple Silicon.
- Unity 6000.5.3f1 para importar FBX y generar controllers.
- `assimp` para inspeccion rapida fuera de Unity.

## Assets actuales

Los scripts siguientes generan **prototipos tecnicos**, no modelos comerciales:

```bash
blender --background --python Tools/blender/create_original_art_assets.py
blender --background --python Tools/blender/create_original_mob_assets.py
```

El resultado actual cubre la estructura necesaria para probar:

- 8 variantes de personaje.
- 30 variantes de mob por zona y tier.
- Armature, skinning, UV, LOD0/LOD1/LOD2 y clips basicos.
- Atlas albedo/normal 2K para validar importacion Android.
- Controllers de Unity y fallback visual estable.

La geometria generada usa construccion procedural y debe permanecer fuera de
la presentacion final hasta que un artista o proveedor entregue el lote
aprobado. El hecho de que un FBX tenga cinco animaciones, normales o LOD no
demuestra calidad autoral.

## Contrato de integracion

El arte final debe conservar estos contratos para no romper el runtime:

- Personajes: `Resources/OriginalArt/Characters/{Clase}_{Sexo}.fbx`.
- Mobs: `Resources/OriginalArt/Mobs/{zona}_{tier}.fbx`.
- Controllers en `Resources/OriginalArt/Controllers` y
  `Resources/OriginalArt/Mobs/Controllers`.
- Clips comunes: `Idle`, `Run`, `Attack`, `Hit`, `Death`.
- LOD con nombres o grupos equivalentes a `LOD0`, `LOD1` y `LOD2`.
- Escala y punto de apoyo compatibles con el `CharacterController` existente.

La sustitucion visual se hace manteniendo esos paths, el rig de gameplay, los
colliders y los sockets de armas. No se deben cambiar los stats o el combate
para ocultar un problema de modelado.

## Auditoria automatica

Validar todos los FBX con Blender:

```bash
blender --background --python Tools/blender/validate_production_fbx.py -- \
  --all --report docs/production-art-audit.md \
  --json docs/production-art-audit.json
```

La auditoria comprueba skinning, UV, LOD real, clips, mallas semanticas,
albedo/normal/roughness/metallic, rutas de textura, procedencia, licencia y
revision visual. Exige un sidecar `.production.json` junto a cada FBX.

Resultado de referencia del estado actual:

- Personajes: `0/8` comerciales aprobados.
- Mobs y jefes: `0/30` comerciales aprobados.
- Contenido funcional 1-105: auditoria separada, `17/17` checks.

## Regeneracion de controllers

Unity puede reconstruir los controllers tecnicos con:

```bash
"/Applications/Unity/Hub/Editor/6000.5.3f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit -projectPath "$PWD" \
  -executeMethod MmorpgPrototype.Editor.OriginalArtAnimatorAssetGenerator.Generate
```

No se debe activar el flag experimental de `PlayerAvatarVisual` o
`EnemyVisualController` para mejorar una captura. Esos FBX siguen marcados
como tecnicos hasta superar la auditoria y la revision visual dentro de Unity
y Android.

## Siguiente entrega real

El primer lote de produccion es Zona 1 completo: ocho personajes, tres NPC,
una familia normal/elite/jefe, armas y accesorios, terreno, vegetacion,
ruinas, VFX y audio. La aprobacion se hace con capturas antes/despues y una
APK, no solo con la existencia de archivos en `Assets`.
